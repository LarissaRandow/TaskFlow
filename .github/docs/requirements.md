# TaskFlow Requirements

**Status: Approved by the project owner on 2026-11-07.**

## Product

TaskFlow is a web-based task manager. Registered users authenticate and manage their own tasks in an Angular Kanban board. The backend is an ASP.NET Core Web API using C#, Entity Framework Core, SQL Server, and JWT bearer authentication.

## Required capabilities

### Authentication

- Register an account and log in with email and password.
- Registration collects email and password only; do not request a full name.
- Issue and validate JWT access tokens.
- Expose registration and login without authentication.
- Protect task APIs and associate every task operation with the authenticated user.

### Tasks

An authenticated user can create, list, retrieve by ID, update, and delete their own tasks. A task contains an ID, title, description, status, due date, and user ID. The client must not choose or change the owning user ID.

The board has exactly these four statuses: `Todo`, `InProgress`, `Completed`, and `Discarded`. Do not add statuses or unrelated product features.

### User experience

- Provide login and registration screens.
- Provide a responsive Kanban board with the four required columns.
- Treat the approved TaskFlow Figma file as the primary visual reference for frontend layout, components, typography, colors, spacing, and interactions; report any mismatch with functional requirements before making assumptions. See [ui-design-reference.md](./ui-design-reference.md).
- Provide task create/edit forms and delete confirmation.
- The edit form must let users change task status among the four approved statuses.
- Show loading, validation, success, and error states.
- Integrate only against the documented API contract.

## Quality and delivery requirements

- Use Clean Architecture boundaries and keep the solution straightforward to explain.
- Include automated tests for business rules, API behavior, and authorization.
- Document API behavior and important architectural decisions.
- Prepare setup instructions, a safe demo-data approach, and interview presentation notes for final delivery.

## Out of scope

Microservices, message brokers, charts, complex client state management, task sharing, roles, notifications, recurring tasks, refresh-token flows, and features not listed above are out of scope unless the product owner approves a change.
