# Frontend views

Status of the frontend views. Update it when starting or finishing a view.

Legend: ⬜ Pending · 🚧 In progress · ✅ Done

| View | Route | Backend | Service/hook | Status | Notes |
|---|---|---|---|---|---|
| Login | `/login` | `/api/auth/login` | `services/auth.ts` | ✅ Done | |
| Register | `/register` | `/api/auth/register` | `services/auth.ts` | ✅ Done | |
| Accounts | `/accounts` | `/api/accounts` | `services/accounts.ts`, `hooks/use-accounts.ts` | ✅ Done | Verify full CRUD |
| Dashboard | `/dashboard` | `/api/analytics/monthly-summary`, `/api/ai/quick-insights` | — | ⬜ Pending | `app/page.tsx` is still the Next.js template |
| Transactions | `/transactions` | `/api/transactions` | `services/transactions.ts`, `hooks/use-transactions.ts` | ✅ Done | Filters, pagination, create/edit/delete |
| Categories | `/categories` | `/api/categories` | `services/categories.ts`, `hooks/use-categories.ts` | ⬜ Pending | Service ready |
| Bills | `/bills` | `/api/bills/*` | — | ⬜ Pending | Includes payments and installments |
| Goals | `/goals` | `/api/goals/*` | — | ⬜ Pending | Includes progress tracking |
| Import | `/import` | `/api/import/*` | — | ⬜ Pending | Preview → validate → execute |
| Snapshots | `/snapshots` | `/api/snapshots/*` | — | ⬜ Pending | Draft → Closed |
| Analytics | `/analytics` | `/api/analytics/*` | — | ⬜ Pending | 7 endpoints; may be split into sub-views |
| Exchange rates | `/exchange-rates` | `/api/exchange-rates/*` | — | ⬜ Pending | |
| AI chat | `/chat` | `/api/ai/chat`, `/api/ai/query` | — | ⬜ Pending | |
| Documents | `/documents` | `/api/documents/*` | — | ⬜ Pending | RAG: upload, processing, search |
