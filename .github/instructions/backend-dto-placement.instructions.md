---
description: "Use when adding or changing ASP.NET Core controllers, API request/response DTOs, or application models in the TaskFlow backend."
applyTo: "backend/src/**/*.cs"
---
# Backend Layer and Feature Placement

- Organize each project by its Clean Architecture responsibility first, then by feature/use case. Keep namespaces aligned with folders.
- In `TaskFlow.Api`, group controllers and HTTP contracts by feature: `Auth/` and `Tasks/Contracts/`. Keep OpenAPI and authentication configuration in their own API-level folders. Do not define request or response DTOs inside controllers.
- In `TaskFlow.Application`, group ports, use-case services, and results under the use case that owns them, such as `Auth/Login/`, `Auth/Register/`, `Auth/Tokens/`, and `Tasks/CreateTask/`. Do not place HTTP request/response DTOs here.
- In `TaskFlow.Domain`, keep entities and business concepts grouped by domain, such as `Tasks/` and `Users/`; do not add API, EF Core, or Identity dependencies.
- In `TaskFlow.Infrastructure`, group adapters by technical responsibility and feature: Identity/JWT adapters under `Auth/`, and persistence adapters under `Persistence/<Feature>/`; keep EF Core migrations under `Persistence/Migrations/`.
- Put each public type in its own file named after the type. Avoid generic `Models/` and `Services/` buckets when a feature/use-case folder expresses ownership more clearly.
- Apply this structure when adding or moving backend types; do not create duplicate DTOs or abstractions solely to mirror folders.
