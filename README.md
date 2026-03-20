# Storefront

A small full-stack e-commerce-style application demonstrating a modern Angular SPA and ASP.NET Core API. Users can browse products, create orders, and simulate a payment flow. The system uses JWT authentication and an in-memory persistence layer for simplicity.

## Architecture overview

The solution follows a clean, layered architecture:

- **Domain** — core business entities and rules (for example `Order`, payment state transitions).
- **Application** — use cases and orchestration (`OrderService`, `ProductService`).
- **Infrastructure** — persistence implementations (in-memory repositories for this demo).
- **API** — HTTP layer, authentication, and dependency injection (composition root).
- **Frontend (Angular SPA)** — consumes the API and manages UI state.

**Dependency direction:**

- `Storefront.Api` → `Storefront.Application` → `Storefront.Domain`
- `Storefront.Infrastructure` → `Storefront.Application` and `Storefront.Domain` (implements application abstractions and uses domain types)

The API acts as the composition root, binding application abstractions (interfaces) to concrete infrastructure implementations via dependency injection.

## Example request flow (create order)

1. The Angular client sends `POST /api/orders`.
2. `OrdersController` receives the request.
3. The controller delegates to `IOrderService`.
4. `OrderService` validates input, retrieves product data, calculates totals, and creates a domain `Order`.
5. `IOrderRepository` persists the order.
6. A DTO is returned to the client.

This keeps transport (API), business logic (Application and Domain), and infrastructure concerns separate.

## Tech stack

| Area | Technology |
|------|--------------|
| Backend API | .NET 8, ASP.NET Core, controllers, Swagger |
| Architecture | Clean architecture style (Domain / Application / Infrastructure) |
| Auth | JWT Bearer (symmetric key, development only) |
| Persistence | In-memory repositories (demo only) |
| Frontend | Angular (standalone components, signals) |
| Frontend tests | Jasmine, Karma |
| Backend tests | xUnit, Moq, `WebApplicationFactory` |

## Repository structure

```
src/
  Storefront.Domain/
  Storefront.Application/
  Storefront.Infrastructure/
  Storefront.Api/
client/
  storefront.client/
tests/
  Storefront.Application.Tests/
  Storefront.Api.IntegrationTests/
Storefront.slnx          # All .NET projects
DEVELOPMENT.md           # Local troubleshooting (e.g. mixed DLLs, locked API)
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS recommended)

## Authentication and authorization

- JWT-based authentication at `POST /api/auth/login`.
- Tokens carry identity and role claims.
- The Angular app stores the token and attaches it with an HTTP interceptor (`Authorization: Bearer …`).
- The API enforces authorization with `[Authorize(Roles = "Admin")]` on product mutations.

**Important:** client-side role checks are for UX only. Real authorization is enforced on the API.

**Demo users:**

| Username | Password | Role |
|----------|----------|------|
| `admin` | `admin123` | Admin |
| `user` | `user123` | User |

## Running the application

**API**

```bash
cd src/Storefront.Api
dotnet run --launch-profile http
```

- Default URL: **http://localhost:5041**
- Swagger (development): **http://localhost:5041/swagger**

An `https` launch profile also exists; if you use it, update `client/storefront.client/proxy.conf.json` to match the API port.

**Angular client**

```bash
cd client/storefront.client
npm install
npm start
```

- App: **http://localhost:4200**
- Requests to `/api` are proxied to the API via `proxy.conf.json`.

**CORS:** the API allows **http://localhost:4200** for browser calls when not using the proxy.

## Key features

- Browse products (`GET /api/products`).
- Admin-only product creation and deletion.
- Create orders with line items (`POST /api/orders`).
- Retrieve orders by ID (`GET /api/orders/{id}`).
- Mock payment success/failure (`POST /api/orders/{id}/payments/mock`).
- JWT login and role-based authorization for admin product APIs.
- Home screen and navigation between flows.

## Error handling

The API uses RFC 7807 **ProblemDetails** (`application/problem+json`): `status`, `title`, and `detail`. The Angular client resolves user-visible messages with `getApiErrorMessage()` in `client/storefront.client/src/app/core/utils/api-error.ts`, preferring `detail` and falling back to a legacy `message` field when present.

## Idempotency and edge cases

- `POST /api/orders` is **not** idempotent; retries can create duplicate orders. Production systems often use **idempotency keys** on writes.
- Mock payments enforce state transitions (for example you cannot pay again once the order is no longer `PendingPayment`); the API returns **400** with a clear `detail`.
- Network failures: prefer deliberate retry policies; idempotent **GET**s are safer to retry than **POST**s unless idempotency is designed in.

## Testing strategy

**Application tests (unit)** — exercise services in isolation with mocks (fast, deterministic).

**API tests (integration)** — use `WebApplicationFactory` to run the full HTTP pipeline against the in-memory stack.

**Commands**

Application (unit):

```bash
dotnet test tests/Storefront.Application.Tests/Storefront.Application.Tests.csproj
```

API (integration) — stop any running `Storefront.Api` first if the build fails on a locked `Storefront.Api.exe`:

```bash
dotnet test tests/Storefront.Api.IntegrationTests/Storefront.Api.IntegrationTests.csproj
```

Frontend ([Jasmine](https://jasmine.github.io/), [Karma](https://karma-runner.github.io/)):

```bash
cd client/storefront.client
npm install
npm test        # watch mode
npm run test:ci # single run, headless Chrome
```

## Key design principles

- **Separation of concerns** — clear boundaries between layers.
- **Dependency inversion** — application code depends on abstractions (`IOrderRepository`, etc.).
- **Testability** — business logic can be tested without HTTP or real databases.
- **Replaceability** — the persistence implementation can be swapped (for example EF Core and SQL) without rewriting use cases.
- **Stateless API** — authentication via JWT; no server session store in this sample.

## Limitations (intentional for demo)

- In-memory persistence (no durability across restarts; not safe for multiple API instances sharing state).
- Hard-coded demo users and embedded signing key.
- Orders are not tied to the authenticated user.
- No rate limiting or advanced hardening.

## Security note

The JWT signing key and in-memory stores are for **local development only**. Do not deploy this configuration as-is.

## What I would do differently in production

- Externalize secrets (for example environment variables or a key vault).
- Configure JWT with proper issuer, audience, and key rotation.
- Use an external identity provider instead of hard-coded users.
- Replace in-memory persistence with a real database (for example EF Core and SQL).
- Design for horizontal scalability (no shared in-memory state).
- Associate orders with authenticated users and enforce authorization.
- Introduce idempotency for write operations.
- Add structured logging, monitoring, and observability.
- Implement rate limiting for API protection.

### Frontend improvements

- Use environment-specific API base URLs (instead of relying only on the dev proxy).
- Improve token storage (for example httpOnly cookies where appropriate).
- Add route guards aligned with backend authorization.
- Apply security headers (for example CSP).

### Infrastructure and integrations

- Integrate real payment providers (APIs and webhooks, PCI-aware design).
- Terminate TLS properly.
- Restrict CORS to known origins or use a single gateway for SPA and API.