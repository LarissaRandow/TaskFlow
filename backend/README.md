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
`ConnectionStrings:TaskFlow`. In Development, the API generates a temporary
signing key if `Jwt:SigningKey` is not set; tokens are invalidated on restart.
Production must set `Jwt:SigningKey` to a secret of at least 32 UTF-8 bytes
through User Secrets during local production-like testing or the
`Jwt__SigningKey` environment variable in deployment. Never commit signing keys.

Run backend tests from the repository root:

```powershell
dotnet test .\backend\tests\TaskFlow.Api.Tests\TaskFlow.Api.Tests.csproj
```
