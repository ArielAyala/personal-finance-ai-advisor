# Personal Finance AI Advisor

# Architecture Specification Document

## 1. Purpose

Este documento define la arquitectura técnica de la aplicación **Personal Finance AI Advisor**.

El objetivo es establecer una arquitectura escalable que permita:

* Gestionar información financiera personal estructurada.
* Importar datos desde archivos externos (Excel/PDF).
* Mantener histórico financiero.
* Registrar patrimonio personal mediante cierres periódicos.
* Gestionar obligaciones financieras.
* Integrar modelos de inteligencia artificial mediante un AI Agent.
* Implementar RAG para conocimiento financiero adicional.
* Permitir evolución futura hacia una plataforma inteligente de análisis financiero.

---

# 2. Architecture Principles

La arquitectura seguirá los siguientes principios:

## 2.1 Data First

La información financiera será almacenada primero en sistemas determinísticos.

La IA no será la fuente de verdad.

Ejemplo:

Correcto:

```
PostgreSQL
    |
    |
AI Agent
    |
    |
Respuesta
```

Incorrecto:

```
LLM
 |
 |
Memoria del modelo
 |
 |
Datos financieros
```

---

## 2.2 Separation of Responsibilities

Cada componente tendrá una responsabilidad clara.

```
Frontend
    |
    | UI
    |
Backend API
    |
    | Business Logic
    |
Database
    |
    | Source of Truth
    |
AI Layer
    |
    | Reasoning
    |
LLM
```

---

## 2.3 AI as an Application Layer

La inteligencia artificial será una capa adicional sobre el sistema.

No reemplazará:

* SQL queries
* cálculos financieros
* reglas de negocio
* validaciones

La IA será responsable de:

* interpretar preguntas
* decidir qué herramientas usar
* generar explicaciones
* crear recomendaciones

## 2.4 Multi-currency Support

El sistema debe soportar múltiples monedas desde el diseño inicial.

Monedas principales:

* PYG (Paraguayan Guaraní)
* USD (United States Dollar)

Cada registro financiero almacenará su moneda original. Los reportes consolidados utilizarán una moneda base (PYG por defecto) aplicando tipos de cambio históricos registrados por el usuario.

El tipo de cambio será configurable manualmente por fecha, permitiendo calcular equivalencias como:

"Mi cuenta en USD equivale hoy a X guaraníes."

---

# 3. High Level Architecture

```text
                         USER
                          |
                          |
                          v

                 +----------------+
                 |    Frontend    |
                 | Next.js + TS   |
                 +----------------+
                          |
                          |
                     HTTPS / API
                          |
                          v

                 +----------------+
                 | Backend API    |
                 | ASP.NET Core   |
                 +----------------+

                          |
        -------------------------------------
        |                 |                 |
        v                 v                 v

 +--------------+  +--------------+  +--------------+
 | PostgreSQL   |  | File Storage |  | AI Layer     |
 |              |  |              |  |              |
 | Financial DB |  | Excel/PDF    |  | Agent        |
 +--------------+  +--------------+  +--------------+

                                            |
                                            |
                               -------------------------
                               |           |           |
                               v           v           v

                            SQL Tool    RAG Tool   Analytics Tool

```

---

# 4. Technology Stack

## Frontend

Technology:

* Next.js
* TypeScript
* Tailwind CSS
* React Query
* Chart libraries

Responsibilities:

* Dashboard
* Data visualization
* File upload
* Chat interface
* Financial reports

---

## Backend

Technology:

* ASP.NET Core 8/9
* Entity Framework Core
* FluentValidation
* JWT Authentication

Responsibilities:

* Business rules
* API endpoints
* Authentication
* Data processing
* AI orchestration
* Security

---

## Database

Technology:

* PostgreSQL
* pgvector extension

Responsibilities:

Relational data:

* users
* accounts
* transactions
* bills
* snapshots
* goals

Vector data:

* embeddings
* financial documents
* personal knowledge

---

## Infrastructure

Deployment:

```
VPS

Docker
 |
 +-- Nginx
 |
 +-- Frontend Container
 |
 +-- Backend Container
 |
 +-- PostgreSQL Container
 |
 +-- Storage Volume

```

---

# 5. Backend Architecture

Se utilizará una arquitectura modular tipo Clean Architecture.

```
Financial.Api

        |
        |

Financial.Application

        |
        |

Financial.Domain

        |
        |

Financial.Infrastructure

```

---

# 6. Backend Layers

## API Layer

Responsabilidad:

Exponer endpoints HTTP.

Ejemplo:

```
GET /api/transactions

POST /api/imports

GET /api/net-worth

POST /api/ai/chat
```

---

## Application Layer

Contiene casos de uso.

Ejemplo:

```
ImportTransactionsCommand

CreateFinancialSnapshotCommand

AnalyzeMonthlyExpensesQuery

AskFinancialAdvisorQuery
```

### Entity-DTO Mapping Strategy

Para el mapeo entre entidades del dominio y DTOs se utiliza **Mapperly** (source generator):

**Entity → DTO (lectura):**
- Mapperly genera código en tiempo de compilación
- Mappers estáticos en `Financial.Application/Mappers/`
- Sin overhead de runtime (cero reflexión)
- Validación de errores en compilación

**DTO → Entity (escritura):**
- Mapeo **manual** en servicios
- Preserva validación de negocio
- Control explícito de ownership checks
- Lógica de defaults y transformaciones visibles

Ejemplo de uso:
```csharp
// Lectura: Entity → DTO (Mapperly)
return accounts.Select(AccountMapper.ToDto);

// Escritura: DTO → Entity (Manual)
var account = new Account
{
    UserId = userId,
    Name = dto.Name,
    Type = Enum.Parse<AccountType>(dto.Type),
    // ... reglas de negocio explícitas
};
```

---

## Domain Layer

Contiene reglas del negocio.

Ejemplo:

```
Transaction

Account

Bill

FinancialSnapshot

Goal
```

---

## Infrastructure Layer

Implementaciones externas:

* PostgreSQL
* File storage
* AI providers
* Email notifications

---

# 7. Database Architecture

## Core Tables

```
User

 |
 +---- Account
 |
 +---- Transaction
 |
 +---- Category
 |
 +---- Bill
 |
 +---- FinancialSnapshot
 |
 +---- Goal
 |
 +---- Document
 |
 +---- ExchangeRate

```

La tabla `ExchangeRate` almacena tipos de cambio históricos entre monedas (ej. USD/PYG) para permitir la conversión de montos y el cálculo consolidado del patrimonio.

---

# 8. Financial Data Flow

## Import Transactions

```
Excel/PDF

    |

Upload API

    |

Import Service

    |

Parser

    |

Normalizer

    |

Validation

    |

PostgreSQL

```

---

# 9. Financial Snapshot Flow

Usuario crea cierre:

```
Create Draft Snapshot

        |

Add Accounts

        |

Register Exchange Rates

        |

Convert Amounts to Base Currency

        |

Calculate Total Net Worth

        |

Review

        |

Close Snapshot

        |

Historical Data

```

---

Los snapshots permiten:

```
July 2026

Net Worth:
Gs 25.000.000


August 2026

Net Worth:
Gs 28.500.000


Growth:

+14%

```

---

# 10. Bills Architecture

Flujo:

```
Create Bill

    |

Recurring Scheduler

    |

Generate Pending Payment

    |

Track Payment

    |

Update Financial Analysis

```

Ejemplo:

```
Internet

Monthly

250.000

Due:
10th

```

---

# 11. AI Architecture

## AI Agent Layer

El agente será responsable de coordinar herramientas.

```
                 User Question

                       |

                       v

                 AI Agent

                       |

        --------------------------------

        |              |              |

        v              v              v

     SQL Tool      RAG Tool    Analytics Tool

        |              |              |

        v              v              v

   PostgreSQL      pgvector     Calculations

```

---

# 12. AI Agent Responsibilities

## Intent Understanding

Ejemplo:

Pregunta:

```
¿Estoy gastando demasiado?
```

Interpretación:

```
Need:
- expenses
- income
- historical trend
- budget

```

---

## Tool Selection

Ejemplo:

Pregunta:

```
¿Cuánto gasté en comida?
```

Uso:

```
SQL Tool
```

---

Pregunta:

```
¿Qué estrategia recomienda la regla 50/30/20?
```

Uso:

```
RAG Tool
```

---

Pregunta:

```
¿Puedo comprar un vehículo?
```

Uso:

```
SQL Tool
+
Analytics Tool
+
RAG Tool

```

---

# 13. AI Tools

## SQL Financial Tool

Permite:

```
get_transactions()

get_income()

get_expenses()

get_net_worth()

get_bills()

```

---

## Analytics Tool

Responsable de:

```
calculate_savings_rate()

calculate_growth()

calculate_budget_health()

forecast_cash_flow()

convert_currency()

```

---

## RAG Tool

Responsable de:

```
search_financial_documents()

retrieve_user_notes()

retrieve_financial_rules()

```

---

# 14. RAG Architecture

```
Documents

    |

Text Extraction

    |

Chunking

    |

Embeddings

    |

pgvector

    |

Semantic Search

    |

LLM Context

```

---

Fuentes posibles:

```
Financial books

Personal notes

Investment rules

Budget strategies

User goals

```

---

# 15. Security Architecture

## Authentication

Implementar:

* JWT
* Refresh Tokens
* Password hashing

---

## Data Protection

Información sensible:

* números de cuenta
* documentos
* movimientos financieros

Debe aplicar:

* encryption
* access control
* backups
* HTTPS

---

# 16. Observability

Registrar:

* API logs
* AI requests
* Token usage
* Errors
* Import failures

Ejemplo:

```
AI Request

Question:
"Can I buy a car?"

Tools used:

SQL Tool
Analytics Tool

Tokens:
2500

Duration:
4 seconds

```

---

# 17. Development Evolution

## Stage 1

Monolith:

```
Next.js

+

ASP.NET Core

+

PostgreSQL

```

---

## Stage 2

Add AI Layer:

```
Backend

+

LLM API

+

Tool Calling

```

---

## Stage 3

Add RAG:

```
PostgreSQL

+

pgvector

```

---

## Stage 4

Advanced Agent:

```
Multi-step reasoning

Multiple tools

Financial planning

Forecasting

```

---

# 18. Final Architecture Goal

La arquitectura final será:

```
                         USER

                          |

                     Next.js UI

                          |

                    ASP.NET API

                          |

                  AI Orchestrator

                          |

        ----------------------------------

        |                |               |

   PostgreSQL        pgvector       AI Provider

        |

        |

 Financial Data

 - Transactions
 - Accounts
 - Bills
 - Snapshots
 - Goals
 - ExchangeRates


```

El sistema será un **AI-powered Financial Operating System personal**, donde la IA no almacena la información financiera, sino que utiliza herramientas para consultar, analizar y razonar sobre datos reales del usuario.
