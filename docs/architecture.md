# Proposed Architecture

**Status: Approved by the project owner on 2026-11-07.**

## Technology baseline

- Backend: C#, ASP.NET Core Web API, Entity Framework Core, SQL Server, JWT bearer authentication, Swagger/OpenAPI, xUnit, and Moq.
- Frontend: Angular, TypeScript, and SCSS.
- The repository currently targets .NET 10 and has an Angular 22 scaffold. Keep those versions unless compatibility or the owner calls for a change.

## System shape

Use one deployable ASP.NET Core API and one Angular single-page application. SQL Server is the only application database. This is a small modular monolith, not a microservice system.

```text
Angular SPA  -- HTTPS + JSON + Bearer JWT -->  ASP.NET Core API
                                                |
                                                v
                                         Application use cases
                                                |
                                      Domain rules / entities
                                                |
                                      Infrastructure adapters
                                                |
                                           SQL Server
```

The API owns validation, authentication, authorization, persistence, and task ownership. The SPA owns presentation, client-side form feedback, and rendering the API's data; it is not an authorization boundary.

## Backend boundaries

Proposed layout under `backend/`:

```text
src/
  TaskFlow.Api/
    Auth/                   Auth controller and HTTP contracts
    Tasks/                  Task controller and HTTP contracts
    Authentication/         JWT bearer configuration
    OpenApi/                Swagger/OpenAPI configuration
  TaskFlow.Application/
    Auth/Login/             Login use case and result
    Auth/Register/          Registration use case and result
    Auth/Tokens/             Access-token port and result
    Tasks/CreateTask/       Create-task use case, result, and persistence port
  TaskFlow.Domain/
    Tasks/                  Task entity, status, and invariants
    Users/                  Domain user representation
  TaskFlow.Infrastructure/
    Auth/                   Identity, JWT implementations, and JWT options
    Persistence/
      Tasks/                Task persistence adapter
      Migrations/           EF Core migrations and model snapshot
tests/
  TaskFlow.Domain.Tests/
  TaskFlow.Application.Tests/
  TaskFlow.Api.IntegrationTests/
TaskFlow.slnx               retain the repository's existing solution format
```

Responsibilities and dependency direction:

- **Domain** contains task rules and has no ASP.NET Core, EF Core, or SQL Server dependency.
- **Application** coordinates use cases and defines persistence/authentication ports; organize these by feature/use case. It depends on Domain, not Infrastructure. HTTP request/response contracts belong to API.
- **Infrastructure** implements those ports with EF Core/SQL Server and ASP.NET Core Identity. It depends inward on Application/Domain.
- **API** is the composition root and HTTP adapter. It maps requests to application use cases and maps application results to HTTP responses.
- **Tests** are separated by behavior; API integration tests exercise the real HTTP/authentication boundary.

Keep controllers/endpoints thin. Avoid a generic repository framework, CQRS/MediatR, event buses, and extra layers unless evidence demonstrates a need.

### Identity and task relationship

Proposed: use ASP.NET Core Identity for password hashing and account management with an `ApplicationUser : IdentityUser<Guid>` in Infrastructure. Keep task ownership as a `Guid UserId` in the Domain task model; configure its relationship to the Identity user in Infrastructure without making Domain reference ASP.NET Identity. Account registration/login orchestration and JWT generation stay behind Application-facing interfaces.

## Frontend boundaries

Keep Angular feature code under `frontend/src/app/`, grouped by a small number of features:

```text
core/       API/auth services, token interceptor, route guard
auth/       login and registration screens
tasks/      board, task card, task form, delete confirmation
shared/     only reusable presentation elements that emerge from use
```

Prefer Angular's built-in standalone components, forms, router, and lightweight services. Do not add a state-management library for this scope. Centralize API URLs and DTO types; the documented OpenAPI contract is authoritative. Keep the four status labels in one shared frontend definition derived from the contract.

Use the approved [Figma UI reference](./ui-design-reference.md) as the primary visual source. Match inspected desktop frames and components closely, adapt responsively without claiming unsupplied designs, and resolve the documented registration-name/status-interaction gaps before assuming behavior.

**Proposed token handling:** hold the access token in `sessionStorage` for this interview application, send it through a single HTTP interceptor, and clear it on logout. This is a trade-off, not protection against XSS; do not put secrets in frontend configuration. Revisit if deployment/security requirements differ.

## API and security boundaries

- Use HTTPS outside local development and bearer JWT authentication for protected endpoints.
- Configure JWT signing key, issuer, audience, and expiry from configuration/environment; never commit signing secrets.
- Restrict CORS to the configured frontend origin(s), not a wildcard with credentials.
- Derive the task owner from a validated subject/user-ID claim.
- Scope every task read/write in the query itself, not only after loading a record.
- Return consistent Problem Details errors and do not expose stack traces, password material, signing keys, or another user's task details.

See [api-contract.md](./api-contract.md) and [business-rules.md](./business-rules.md).

## Data and testing

Use EF Core migrations for SQL Server schema changes. The Infrastructure project owns DbContext and migrations. Keep persistence models/configuration out of Domain.

- Domain unit tests cover task invariants and allowed statuses.
- Application unit tests use Moq for ports and verify ownership-aware use-case behavior.
- API integration tests verify routes, validation, JWT authentication, persistence, and cross-user isolation.
- Prefer a configured SQL Server test instance for persistence integration. Do not treat EF InMemory tests as proof of SQL Server behavior.
- Frontend tests cover form validation, board/status rendering, API-service behavior, and error states using the existing Angular test setup.

## Deployment shape

For the exercise, run the API, Angular development server, and one SQL Server instance locally. Keep production hosting/deployment, observability platforms, and container orchestration out of scope unless requested.
