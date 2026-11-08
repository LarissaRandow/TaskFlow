# Backend

The API is in `src/TaskFlow.Api`, with Application, Domain, and Infrastructure
projects under `src/`. From the repository root, the recommended way to start
the complete application and initialize the database is:

```powershell
docker compose up --build
```

See the root [README](../README.md) for URLs and local Docker credentials.
Compose waits for SQL Server and the API applies EF Core migrations on startup.
The API can also be run separately with .NET User Secrets configured for
`ConnectionStrings:TaskFlow`.

Run backend tests from the repository root:

```powershell
dotnet test .\backend\tests\TaskFlow.Api.Tests\TaskFlow.Api.Tests.csproj
```
