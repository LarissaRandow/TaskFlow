# TaskFlow Frontend

The frontend is a single-page application built with Angular 22, TypeScript, and SCSS. It provides registration and sign-in screens and an authenticated Kanban board for managing personal tasks.

For the full-stack Docker setup, application URLs, demo account, and product overview, see the [root README](../README.md). This guide focuses on frontend development and implementation details.

## Prerequisites

- Node.js and npm
- The TaskFlow API running locally for end-to-end frontend/API behavior

## Install and run

From the `frontend` directory, install dependencies and start the development server:

```powershell
npm install
npm start
```

Open http://localhost:4200. The development server uses [proxy.conf.json](proxy.conf.json) to forward `/api` requests to the local API, so the browser can use the same relative API paths as the application.

To run the complete application, including SQL Server and API, use the Docker Compose instructions in the [root README](../README.md).

## Main features

- **Authentication** — registration and login forms with client-side validation and API error feedback.
- **Session handling** — the access token is stored in `sessionStorage`, attached to task API requests by an HTTP interceptor, and removed on sign-out.
- **Route protection** — the dashboard route uses an authentication guard for navigation. API authorization remains the security boundary.
- **Task board** — displays the user's tasks in the four supported status columns: `Todo`, `InProgress`, `Completed`, and `Discarded`.
- **Task management** — create, edit, and delete dialogs integrate with the documented API contract and display loading, validation, success, and error states.
- **Responsive UI** — desktop layouts follow the approved Figma reference, with responsive adaptations for smaller screens.

The frontend uses the approved [API contract](../.github/docs/api-contract.md) as the source of truth for routes, request/response fields, and task status values. It does not assign task ownership; the API derives `userId` from the authenticated user.

## Project structure

```text
src/app/
  auth/       Registration, login, route guard, and HTTP interceptor
  tasks/      Dashboard and task API service
  shared/     Reusable presentation components
```

The application uses standalone Angular components, reactive forms, signals, the Angular router, and lightweight injectable services. It does not use a separate state-management library.

## Build and tests

Run a production build:

```powershell
npm run build
```

Run frontend unit/component tests with Vitest:

```powershell
npm test
```

The current tests cover the application shell, login and registration flows, auth interceptor behavior, and dashboard/task interactions. The repository does not currently configure an end-to-end browser test runner.

## Design reference

The approved Figma frames and documented resolutions are described in [ui-design-reference.md](../.github/docs/ui-design-reference.md). The reference contains desktop designs; responsive behavior on smaller screens is an implementation adaptation rather than a directly specified Figma layout.
