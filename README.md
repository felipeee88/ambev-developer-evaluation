# Ambev.DeveloperEvaluation — Sales API

Sales REST API built on the `Ambev.DeveloperEvaluation` template, implementing a CRUD for Sales with per-item discount tiers, cancellation flows and domain events, following DDD + CQRS.

## Tech stack

- **.NET 8** · **ASP.NET Core** WebApi
- **MediatR** (CQRS) · **AutoMapper** · **FluentValidation**
- **EF Core 8** · **PostgreSQL** (Npgsql provider)
- **xUnit** · **NSubstitute** · **Bogus** · **FluentAssertions** · **Testcontainers**
- **Docker Compose** for local Postgres

## Prerequisites

- **.NET 8 SDK**
- **Docker Desktop** — must be running for Postgres (dev loop) and for the `dotnet test` suites (Integration and Functional spin up Postgres containers via Testcontainers).
- **dotnet-ef** tool:

  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Running the API

From the root of this repo:

```bash
# 1. Start Postgres (only the DB service from docker-compose)
docker-compose up -d ambev.developerevaluation.database

# 2. Apply migrations
dotnet ef database update \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi

# 3. Run the API (http profile)
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi --launch-profile http
```

Endpoints once the API is up:

- Health: http://localhost:5119/health
- Swagger UI: http://localhost:5119/swagger

The `https` profile also works (`https://localhost:7181`) — you'll need a trusted dev certificate (`dotnet dev-certs https --trust`).

### Postgres connection

`docker-compose.yml` publishes Postgres on host port **55432** (port 5432 was taken on the dev machine when this was written). Credentials:

| Key | Value |
|---|---|
| Host | `localhost` |
| Port | `55432` |
| Database | `developer_evaluation` |
| User | `developer` |
| Password | `ev@luAt10n` |

`src/Ambev.DeveloperEvaluation.WebApi/appsettings.json` already points to this connection:

```json
"DefaultConnection": "Host=localhost;Port=55432;Database=developer_evaluation;Username=developer;Password=ev@luAt10n"
```

If port 55432 is taken on your machine, change both the host-side port in [docker-compose.yml](docker-compose.yml) and the `Port=` in [appsettings.json](src/Ambev.DeveloperEvaluation.WebApi/appsettings.json).

## Authentication

Every `/api/Sales` endpoint requires a **JWT Bearer** token. Bootstrap flow:

### 1. Create an admin user

```bash
curl -X POST http://localhost:5119/api/Users \
  -H "Content-Type: application/json" \
  -d '{
    "username":"admin",
    "password":"Admin@123",
    "phone":"+5511999999999",
    "email":"admin@ambev.test",
    "status":1,
    "role":3
  }'
```

Gotchas from the template:

- `phone` must be in E.164 (digits with optional `+`, no formatting).
- `status` and `role` are numeric enums: `1` = Active, `3` = Admin.
- Password policy: ≥ 8 chars, with upper, lower, digit and special char.

### 2. Authenticate

```bash
curl -X POST http://localhost:5119/api/Auth \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@ambev.test","password":"Admin@123"}'
```

Copy the `data.data.token` from the response into the `Authorize` button in Swagger UI (`Bearer {token}`), or pass it via `Authorization: Bearer ...` in curl.

## API examples

### Create a sale

```bash
curl -X POST http://localhost:5119/api/Sales \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "saleNumber": "S-0001",
    "saleDate": "2026-04-19T10:00:00Z",
    "customerId": "11111111-1111-1111-1111-111111111111",
    "customerName": "Acme Ltda",
    "branchId":   "22222222-2222-2222-2222-222222222222",
    "branchName": "SP Centro",
    "items": [
      { "productId":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "productName":"Skol 600ml",  "quantity":5,  "unitPrice":10.00 },
      { "productId":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "productName":"Brahma 600ml","quantity":12, "unitPrice":9.50  }
    ]
  }'
```

Response (abbreviated) — note discounts applied per tier:

```json
{
  "data": {
    "saleNumber": "S-0001",
    "totalAmount": 136.20,
    "items": [
      { "quantity": 5,  "unitPrice": 10.00, "discount": 5.00,  "totalAmount": 45.00 },
      { "quantity": 12, "unitPrice":  9.50, "discount": 22.80, "totalAmount": 91.20 }
    ]
  },
  "success": true
}
```

### List with pagination / filters

```bash
curl "http://localhost:5119/api/Sales?_page=1&_size=10&_order=saleDate%20desc&saleNumber=S-" \
  -H "Authorization: Bearer $TOKEN"
```

Supported query parameters: `_page`, `_size`, `_order`, `customerId`, `branchId`, `saleNumber` (partial, case-insensitive), `cancelled`, `_minSaleDate`, `_maxSaleDate`, `_minTotalAmount`, `_maxTotalAmount`.

### Other operations

| Method + Path | Status codes |
|---|---|
| `GET /api/Sales/{id}` | 200 / 404 |
| `PUT /api/Sales/{id}` | 200 / 400 / 404 / 422 |
| `DELETE /api/Sales/{id}` | 204 / 404 |
| `PATCH /api/Sales/{id}/cancel` | 200 / 404 |
| `PATCH /api/Sales/{id}/items/{itemId}/cancel` | 200 / 404 / 422 |

## Business rules — Discount tiers

Discount is applied **per item** (same product), derived in the domain — clients never send the discount value.

| Quantity | Discount rate | Example (unitPrice = 10.00) |
|---|---|---|
| 1–3  | 0%  | Q=3  → TotalAmount = 30.00 |
| 4–9  | 10% | Q=5  → TotalAmount = 45.00 |
| 10–20 | 20% | Q=12 → TotalAmount = 96.00 |
| 21+  | *rejected* | throws `DomainException("Cannot sell more than 20 identical items.")` |

`TotalAmount = (Quantity × UnitPrice) − (Quantity × UnitPrice × rate)`.
Sale-level `TotalAmount` is the sum of non-cancelled items' `TotalAmount`.

## Error handling

The global `ExceptionHandlingMiddleware` maps known exception types to a consistent JSON payload `{ type, error, detail }`:

| Exception | HTTP | `type` |
|---|---|---|
| `FluentValidation.ValidationException` | 400 | `ValidationError` |
| `Application.Exceptions.NotFoundException` | 404 | `ResourceNotFound` |
| `Domain.Exceptions.DomainException` | 422 | `DomainError` |
| Any other `Exception` | 500 | `InternalError` |

Stack traces of unhandled exceptions are logged server-side and never returned in the response.

## Testing

```bash
dotnet test
```

Projects:

- **Ambev.DeveloperEvaluation.Unit** — aggregates, handlers and validators. No infra.
- **Ambev.DeveloperEvaluation.Integration** — `SaleRepository` against an ephemeral Postgres container (Testcontainers.PostgreSql).
- **Ambev.DeveloperEvaluation.Functional** — `WebApplicationFactory<Program>` + Postgres container + `TestAuthHandler` that bypasses JWT so `[Authorize]` endpoints can be hit without real tokens.

All three pass green locally; Integration/Functional require Docker Desktop running.

## Architecture

### Project layering

```
   WebApi  ─────────▶  IoC  ─────────▶  Application  ─────────▶  Domain
     │                                       │                        ▲
     │                                       └──▶  ORM  ──────────────┘
     └────────────────▶  ORM (via IoC module)
```

- **Domain** has no framework dependencies beyond MediatR (for `INotification` events).
- **Application** depends only on Domain + AutoMapper + FluentValidation + MediatR.
- **ORM** depends on Domain and wires EF Core + PostgreSQL.
- **IoC** composes all modules and is consumed by **WebApi**.
- **Common** hosts cross-cutting utilities (JWT, `ValidationBehavior`, health checks).

### Request flow (POST /api/Sales)

```
HTTP request
    │
    ▼
SalesController  ── Mediator.Send(CreateSaleCommand) ──▶ ValidationBehavior (FluentValidation)
                                                            │
                                                            ▼
                                                      CreateSaleHandler
                                                            │
                                                            ▼
                                                   Sale aggregate (domain rules)
                                                            │
                                                            ▼
                                                     SaleRepository (EF Core)
                                                            │
                                                            ▼
                                                         PostgreSQL
                                                            │
                          ┌─────────────── SaveChanges OK ──┘
                          ▼
                 drain DomainEvents ── Mediator.Publish ──▶ SalesLoggingHandlers (ILogger)
                          │
                          ▼
                    SaleResponse (AutoMapper) ──▶ HTTP 201
```

Events are only published **after** `SaveChangesAsync` succeeds. Logging uses structured placeholders (`{EventName}`, `{SaleId}`, etc.) and goes through Serilog.

## Database migrations

Migrations live in [src/Ambev.DeveloperEvaluation.ORM/Migrations/](src/Ambev.DeveloperEvaluation.ORM/Migrations/). The WebApi is the startup project.

Add a migration:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

Apply pending migrations:

```bash
dotnet ef database update \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

## Project structure

```
ambev-developer-evaluation/
├── docker-compose.yml
├── Ambev.DeveloperEvaluation.sln
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain/       (Entities, VOs, Events, Repositories)
│   ├── Ambev.DeveloperEvaluation.Application/  (Commands, Queries, Handlers, Validators)
│   ├── Ambev.DeveloperEvaluation.ORM/          (DbContext, Mappings, Repositories, Migrations)
│   ├── Ambev.DeveloperEvaluation.WebApi/       (Controllers, DTOs, Middleware)
│   ├── Ambev.DeveloperEvaluation.IoC/          (ModuleInitializers)
│   └── Ambev.DeveloperEvaluation.Common/       (JWT, Logging, Validation behavior)
└── tests/
    ├── Ambev.DeveloperEvaluation.Unit/
    ├── Ambev.DeveloperEvaluation.Integration/
    └── Ambev.DeveloperEvaluation.Functional/
```

## Decisions / notes

- **Concurrency token** uses Postgres' native `xmin` system column (shadow property, no entity field). A literal `byte[] RowVersion` would NOT self-update in Postgres — we picked the idiomatic Npgsql approach.
- **External Identity** — `Sale` holds denormalized `CustomerInfo`, `BranchInfo`, and each `SaleItem` holds a `ProductInfo`. No FK to those bounded contexts.
- **UpdateSale** changes only the sale header (SaleNumber / SaleDate / Customer / Branch). Items are managed through dedicated commands (`CancelSaleItem`) to keep event flow unambiguous.
- **Delete vs Cancel** — `DELETE` is a hard delete (administrative, no event). `PATCH /cancel` is the soft delete that raises `SaleCancelledEvent` and cascades into every non-cancelled item.
- **Quantity > 20 returns 400** (not 422) because the `CreateSaleValidator` catches it before the Domain invariant fires. Defense in depth: if someone constructs a `SaleItem` directly bypassing the validator, the aggregate still throws `DomainException`.
- **Local Postgres on 55432** — avoids conflict with a native Postgres install on 5432 on the dev machine. Changeable in both `docker-compose.yml` and `appsettings.json`.
