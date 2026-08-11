# Personal Finance AI Advisor

## Software Specification Document (Spec-Driven Development)

## 1. Vision

Construir una aplicación personal de gestión financiera que permita importar, almacenar y analizar información financiera personal, integrando un AI Financial Advisor capaz de responder preguntas, analizar patrones, detectar oportunidades de mejora y ayudar en la toma de decisiones financieras.

La aplicación será un sistema híbrido:

* Base de datos relacional para información financiera estructurada.
* AI Agent para razonamiento y generación de recomendaciones.
* RAG para conocimiento financiero y documentos no estructurados.
* Herramientas internas (SQL, Analytics, Calculators) utilizadas por el agente.

El objetivo no es solamente almacenar datos financieros, sino crear un asistente que tenga contexto histórico sobre:

* ingresos
* gastos
* cuentas
* patrimonio
* inversiones
* deudas
* objetivos financieros
* hábitos de consumo

---

## Multi-currency Support

El sistema soportará múltiples monedas desde el inicio.

Monedas principales:

* PYG - Paraguayan Guaraní
* USD - United States Dollar

Cada cuenta, transacción, obligación y cierre de patrimonio debe registrar su moneda original.

El sistema permitirá registrar tipos de cambio históricos para convertir montos a una moneda base de reporte (por defecto PYG).

Ejemplo:

```
Date:
2026-08-10

USD to PYG:
8.150

```

Esto permite responder:

"¿Cuánto equivale hoy mi cuenta en dólares en guaraníes?"

---

# 2. High Level Architecture

```
Frontend
(Next.js + TypeScript)
        |
        |
Backend API
(ASP.NET Core)
        |
        |
------------------------------------------------
|                    |                         |
PostgreSQL        AI Layer              File Storage
                  |
                  |
        ---------------------
        |          |        |
       SQL       RAG     Tools
       Tool      Tool    Calculator

```

---

# 3. Main Modules

## 3.1 Transaction Management

Permite almacenar movimientos financieros provenientes de:

* Excel exportado de aplicaciones financieras.
* Importaciones manuales.

Ejemplo:

```
Date:
2026-07-01

Amount:
-62000

Currency:
PYG

Merchant:
Github Copilot

Category:
Professional Service

Account:
Mastercard Credit Card Ueno

Notes:
NULL
```

---

## 3.2 Account Management

Representa las cuentas donde existe dinero o deuda.

Tipos:

* Banco local
* Cuenta USD
* Caja física (cash)
* Tarjeta de crédito
* Broker / Investments
* Otros activos

Ejemplo:

```
Account

Name:
Ueno Bank

Type:
Bank Account

Currency:
PYG

Number:
xxxx2039432
```

La moneda es obligatoria para cada cuenta. El sistema debe permitir cuentas en PYG y USD.

Información sensible debe almacenarse protegida.

---

# 4. Nuevo módulo: Financial Closing / Net Worth Snapshot

## Objetivo

Permitir registrar periódicamente el estado total del patrimonio personal.

El usuario actualmente realiza este proceso manualmente mediante Excel:

Ejemplo:

```
Fecha cierre:
2026-07-31

Tipo de cambio USD/PYG:
8.150


Cuenta bancaria Ueno Bank
Número:
xxxx2039432

Monto:
Gs. 1.000.000


Cuenta USD Banco Familiar

Monto:
500 USD

Monto convertido a PYG:
Gs. 4.075.000


Investment Account

Monto:
Gs. 5.000.000
```

Luego calcula:

```
Patrimonio total actual
-
Patrimonio total cierre anterior

=
Variación absoluta


Variación absoluta / patrimonio anterior

=
Porcentaje crecimiento
```

---

# Financial Closing Entities

## FinancialSnapshot

Representa un cierre mensual.

```
FinancialSnapshot

Id

UserId

Date

BaseCurrency

TotalAmount

ExchangeRateDate

Status

Notes

CreatedAt
```

Ejemplo:

```
July 2026 Closing

Date:
2026-07-31

Total:
Gs. 25.000.000

Status:
Closed
```

---

## SnapshotAccount

Detalle de cada cuenta dentro del cierre.

```
SnapshotAccount

Id

SnapshotId

AccountId

AccountName

AccountNumberMasked

OriginalAmount

OriginalCurrency

ExchangeRate

ConvertedAmount

BaseCurrency
```

Ejemplo:

```
Snapshot:
July 2026

Account:
Ueno Bank

Original Amount:
1.000.000 PYG

Converted Amount:
1.000.000 PYG
```

---

# Snapshot States

Un cierre puede tener estados:

## Draft

Durante el mes.

El usuario puede modificarlo.

Ejemplo:

```
Agosto 2026 - Draft
```

Permite:

* actualizar cuentas
* corregir montos
* agregar inversiones

## Closed

Representa un cierre definitivo.

Ejemplo:

```
Agosto 2026 - Closed
```

Luego será usado para:

* histórico patrimonial
* evolución
* análisis AI

---

# Exchange Rate Entity

Permite registrar el tipo de cambio entre monedas en una fecha determinada.

```
ExchangeRate

Id

SourceCurrency

TargetCurrency

Rate

Date

Source

Notes

CreatedAt
```

Ejemplo:

```
Date:
2026-08-10

From:
USD

To:
PYG

Rate:
8150

Source:
Manual input
```

El tipo de cambio se usa para convertir cuentas en USD a PYG al calcular el patrimonio total.

---

# AI Usage

Este módulo permite preguntas como:

## Ejemplos

"¿Cuánto aumentó mi patrimonio este año?"

Proceso:

```
AI Agent

    |
    |
SQL Tool

    |
    |
FinancialSnapshot

    |
    |
Calculation

    |
    |
Response
```

Respuesta:

```
Tu patrimonio aumentó 18.5%
desde enero 2026.

El crecimiento principal vino de:
- inversiones +10%
- ahorro mensual +8.5%
```

---

# 5. Nuevo módulo: Bills / Financial Obligations

## Objetivo

Registrar obligaciones recurrentes y deudas futuras.

Ejemplos:

* alquiler
* préstamos
* suscripciones
* seguros
* cuotas de tarjeta
* financiación de productos

---

# Bill Entity

```
Bill

Id

UserId

Title

Amount

Currency

PaidAmount

DueDate

Periodicity

Installments

PaidInstallments

Status

CategoryId

AccountId

Notes
```

---

# Fields Definition

## Title

Nombre del compromiso.

Ejemplo:

```
Internet Fibra Optica
```

---

## Amount

Monto esperado.

Ejemplo:

```
250000 PYG
```

---

## PaidAmount

Opcional.

Permite registrar pagos parciales.

Ejemplo:

```
Expected:
500000

Paid:
250000
```

---

## DueDate

Fecha límite.

Ejemplo:

```
2026-08-10
```

---

## Periodicity

Valores:

```
Monthly
Weekly
Yearly
OneTime
Custom
```

Ejemplo:

```
Monthly
```

---

## Installments

Cantidad total de cuotas.

Opcional.

Ejemplo:

```
24
```

---

## PaidInstallments

Cuotas pagadas.

Opcional.

Ejemplo:

```
10
```

Representa:

```
10 / 24 cuotas pagadas
```

---

# Ejemplo

```
Bill

Title:
Notebook Lenovo

Amount:
3000000

Periodicity:
Monthly

Installments:
12

PaidInstallments:
5

Status:
Active
```

---

# AI Usage

Permite preguntas:

## "¿Cuáles son mis compromisos financieros del próximo mes?"

El agente consulta:

```
Bills
+
Transactions
+
Income
```

Respuesta:

```
Tus obligaciones próximas son:

Internet:
250.000

Cuota notebook:
250.000

Seguro:
150.000


Total:
650.000
```

---

## "¿Puedo comprar un nuevo monitor?"

El agente analiza:

```
Income

-

Fixed Bills

-

Average Expenses

-

Financial Goals

=

Available Capacity
```

---

# 6. AI Financial Advisor Architecture

El agente tendrá acceso a herramientas.

## SQL Tool

Para datos estructurados.

Ejemplo:

```
get_transactions()

get_financial_snapshots()

get_active_bills()

get_monthly_expenses()
```

---

## Analytics Tool

Para cálculos:

```
calculate_net_worth_growth()

calculate_savings_rate()

calculate_expense_trends()

convert_currency()
```

---

## RAG Tool

Para información no estructurada:

Fuentes:

* libros financieros
* notas personales
* estrategias
* objetivos escritos
* documentos

Tecnología:

```
PostgreSQL
+
pgvector
```

---

# 7. Database Model Overview

```
User

 |
 |
 +---- Accounts
 |
 +---- Transactions
 |
 +---- Categories
 |
 +---- FinancialSnapshots
 |          |
 |          +---- SnapshotAccounts
 |
 +---- Bills
 |
 +---- Goals
 |
 +---- Documents
 |
 +---- Embeddings
```

---

# 8. Development Phases

# Phase 1 - Core Financial System

Objetivo:

Crear la base financiera.

Implementar:

* Authentication
* Users
* Accounts
* Categories
* Transactions
* Dashboard básico

No AI todavía.

---

# Phase 2 - Import System

Objetivo:

Importar información real.

Implementar:

* Excel upload
* CSV parser
* PDF parser
* Import validation
* Duplicate detection

Flujo:

```
File

↓

Parser

↓

Preview

↓

Validation

↓

Database
```

---

# Phase 3 - Financial Closing Module

Implementar:

* Monthly snapshots
* Draft closing
* Closed snapshots
* Historical net worth
* Growth calculations

---

# Phase 4 - Bills Module

Implementar:

* Recurring bills
* Installments
* Payment tracking
* Due dates
* Monthly obligations

---

# Phase 5 - Financial Analytics

Crear servicios:

* Monthly summary
* Expense trends
* Savings rate
* Net worth evolution
* Debt analysis

---

# Phase 6 - AI Assistant v1

Agregar:

* LLM integration
* Chat interface
* SQL tools
* Structured responses

Ejemplos:

```
¿Cuánto gasté este mes?

¿Dónde gasto más?

¿Cuánto puedo ahorrar?
```

---

# Phase 7 - RAG Integration

Agregar:

* Document ingestion
* Embeddings
* Vector search
* Financial knowledge base

---

# Phase 8 - AI Agent Advanced

Implementar:

* Tool calling
* Multi-step reasoning
* Financial planning
* Recommendations
* Goal tracking

---

# 9. MVP Definition

El primer producto funcional debe permitir:

Usuario:

1. Importa Excel de gastos.
2. Visualiza gastos.
3. Registra cierre mensual.
4. Registra bills.
5. Pregunta al AI Advisor:

Ejemplos:

```
¿Cómo están mis finanzas este mes?

¿Mi patrimonio está creciendo?

¿Qué gastos puedo reducir?

¿Puedo permitirme una nueva compra?

¿Cuánto debo pagar este mes?
```

---

# 10. Final Goal

Crear un AI Financial Advisor personal que tenga:

* memoria histórica financiera
* conocimiento de ingresos y gastos
* evolución patrimonial
* obligaciones futuras
* objetivos personales
* capacidad de análisis y recomendaciones

No almacenar todo dentro del modelo de IA.

El AI Agent debe consultar las fuentes correctas mediante herramientas y generar respuestas basadas en datos reales.

```
```
