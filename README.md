# TaskFlow

TaskFlow is a web-based task manager with an Angular frontend, ASP.NET Core
API, and SQL Server database.

## Run the application with Docker

Requirements: Docker Desktop running with Linux containers enabled.

From the repository root, run:

```powershell
docker compose up --build
```

On first startup, Compose builds the frontend and API, starts SQL Server,
waits for the database to become healthy, and the API applies its EF Core
migrations automatically. Open:

- Frontend: <http://localhost:4200>
- API Swagger: <http://localhost:5004/swagger>
- SQL Server for SSMS: `localhost,1433`, SQL Server authentication, login
  `sa`, password `TaskFlow_dev_2026!`

These are **local development credentials only**. Do not expose this setup to
the internet or reuse its password elsewhere. The SQL Server data is stored
in a Docker volume and survives container restarts and `docker compose down`.

## Run tests locally

The backend tests use a test registration service and do not require Docker or
a SQL Server connection. From the repository root:

```powershell
dotnet test .\backend\tests\TaskFlow.Api.Tests\TaskFlow.Api.Tests.csproj
```

## Run the API locally outside Docker

To run the .NET API directly from the host, configure its SQL Server connection
with .NET User Secrets (do not put passwords in tracked configuration files).
Start the SQL Server service with Docker, then save the development connection
string:

```powershell
docker compose up -d sqlserver
$connectionString = "Server=127.0.0.1,1433;Database=TaskFlow;User Id=sa;Password=TaskFlow_dev_2026!;Encrypt=True;TrustServerCertificate=True"
dotnet user-secrets set "ConnectionStrings:TaskFlow" $connectionString --project .\backend\src\TaskFlow.Api\TaskFlow.Api.csproj
Remove-Variable connectionString
```

Set Development so EF Core loads User Secrets, then apply migrations and run
the API from the repository root:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project .\backend\src\TaskFlow.Infrastructure\TaskFlow.Infrastructure.csproj --startup-project .\backend\src\TaskFlow.Api\TaskFlow.Api.csproj
dotnet run --project .\backend\src\TaskFlow.Api\TaskFlow.Api.csproj --launch-profile http
```

If needed, install the EF Core CLI once with
`dotnet tool install --global dotnet-ef --version 10.0.14`.

Open Swagger at <http://localhost:5004/swagger>. The frontend can be run
separately from another terminal with `npm start` in `frontend/`.
