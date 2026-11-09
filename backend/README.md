# TaskFlow Backend

The backend is a .NET 10 ASP.NET Core Web API. It exposes the authentication and task-management endpoints, enforces task ownership, and persists application data in SQL Server through Entity Framework Core.

For the complete application setup, local URLs, and demo account credentials, see the [root README](../README.md). This document focuses on backend structure, architectural boundaries, and tests.

## Architecture

The solution is inspired by Clean Architecture: responsibilities are separated into projects, and dependencies point inward toward the Domain and Application layers. It is intentionally kept small for this application rather than introducing additional frameworks or abstractions.

```text
HTTP request
    |
    v
TaskFlow.Api  ->  TaskFlow.Application  ->  TaskFlow.Domain
                       ^                         ^
                       |                         |
                 TaskFlow.Infrastructure --------+
                       |
                       v
                   SQL Server
```

### Projects

- **TaskFlow.Domain** — core task concepts, status values, and invariants. It does not depend on ASP.NET Core, Entity Framework Core, or SQL Server.
- **TaskFlow.Application** — feature-oriented use cases for registration, login, and task operations. It defines application-facing interfaces for dependencies such as task persistence and token issuance.
- **TaskFlow.Infrastructure** — adapters implementing persistence and identity/token services. It contains the EF Core `DbContext`, SQL Server configuration, Identity integration, and database migrations.
- **TaskFlow.Api** — HTTP boundary and composition root. Controllers map requests to application use cases and return HTTP responses; the API also configures authentication, authorization, and OpenAPI.
- **TaskFlow.Api.Tests** — API-level tests that exercise endpoints and HTTP behavior using ASP.NET Core's test host.
- **TaskFlow.Application.Tests** — unit tests for task application services using a recording repository fake.
- **TaskFlow.Infrastructure.Tests** — SQL Server integration tests for EF Core migrations and task persistence.

Controllers are kept focused on HTTP concerns. Business validation and task rules are handled by Application and Domain code, while Infrastructure implements external technical details.

## Important backend rules

- Registration and login are public; task endpoints require a valid bearer token.
- The authenticated user's ID is taken from the validated JWT subject claim. Clients cannot assign task ownership.
- Task reads and writes are scoped to the authenticated user. A task owned by a different user is treated as not found.
- New tasks start in `Todo`. Updates accept only `Todo`, `InProgress`, `Completed`, or `Discarded`.
- Password hashing and account management are handled by ASP.NET Core Identity. JWT signing configuration must be supplied securely outside source control.

The approved routes and request/response shapes are documented in the [API contract](../.github/docs/api-contract.md).

## Database and demo seeding

EF Core migrations are maintained in `TaskFlow.Infrastructure`. In the Compose development setup, the API applies migrations on startup. The demo seeder runs only in Development when startup migrations and `DemoData:Enabled` are enabled; it creates the configured demo account and sample tasks if they do not already exist.

For direct local API execution, configure `ConnectionStrings:TaskFlow` and enable `Database:ApplyMigrationsOnStartup` through User Secrets or environment variables. Demo credentials and full-stack startup instructions are in the [root README](../README.md). Never use the demo account or development database credentials in a shared or production environment.

## Tests

Run all backend tests from the repository root:

```powershell
dotnet test .\backend\backend.slnx
```

The Infrastructure tests create uniquely named temporary databases and apply the EF Core migrations. Configure `TASKFLOW_TEST_SQLSERVER` with a SQL Server connection string that has permission to create and drop databases to run them; otherwise, those tests are reported as skipped. For example, with SQL Server LocalDB on Windows:

```powershell
$env:TASKFLOW_TEST_SQLSERVER = 'Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True'
dotnet test .\backend\tests\TaskFlow.Infrastructure.Tests\TaskFlow.Infrastructure.Tests.csproj
Remove-Item Env:\TASKFLOW_TEST_SQLSERVER
```

The API tests cover:

- Registration validation, duplicate-email behavior, and exclusion of password material from responses
- Login validation, generic invalid-credential responses, and issued JWT authentication
- Task creation defaults and field validation
- Authentication requirements and task-list behavior
- Task updates, allowed statuses, validation, and cross-user ownership isolation
- Task deletion, including attempts to delete another user's task
- Selected OpenAPI endpoint and bearer-authentication metadata

The Application tests cover task creation and update validation, exact allowed statuses and field limits, authenticated-user ownership propagation, missing-task and failed-update results, and repository call/cancellation-token forwarding for all task services. They use a recording repository fake and do not require Docker or SQL Server.

The Infrastructure tests exercise migrations, task persistence, user-scoped reads/deletes, and updates against SQL Server. They use a separate temporary database per test and require an explicitly configured test connection string.

The API tests use the ASP.NET Core test host and stubbed application services; they also do not require Docker or SQL Server. The three suites cover the HTTP/authentication boundary, application-service behavior, and EF Core persistence respectively.
