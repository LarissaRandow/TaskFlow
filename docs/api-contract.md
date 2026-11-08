# Proposed API Contract

**Status: Approved by the project owner on 2026-11-07.** This contract is the single source of truth for frontend/backend integration. Do not implement divergent behavior silently; raise a contract conflict for resolution.

## Conventions

- Base path: `/api`.
- JSON uses camelCase property names and ISO-8601 date/time strings.
- IDs are UUIDs.
- Task endpoints require `Authorization: Bearer <access-token>`.
- The JWT `sub` claim is the user's UUID. The API assigns `userId` from this authenticated principal. Write requests must not contain `userId`.
- Status is the exact case-sensitive string `Todo`, `InProgress`, `Completed`, or `Discarded`.
- Errors use `application/problem+json` (RFC 9457 / ASP.NET Core Problem Details). Validation errors may include an `errors` object keyed by field.
- Proposed validation: title is required, trimmed, and at most 120 characters; description is optional and at most 2,000 characters; due date is optional. Registration uses the proposed password policy in [business-rules.md](./business-rules.md).

## Authentication

### `POST /api/auth/register` — public

Request:

```json
{ "email": "person@example.com", "password": "example-password" }
```

Success: `201 Created`, body:

```json
{ "id": "uuid", "email": "person@example.com" }
```

Invalid input: `400 Bad Request`. Duplicate email: **proposed** `409 Conflict`. Never return a password or hash.

### `POST /api/auth/login` — public

Request:

```json
{ "email": "person@example.com", "password": "example-password" }
```

Success: `200 OK`, body:

```json
{
  "accessToken": "signed-jwt",
  "tokenType": "Bearer",
  "expiresAt": "2026-01-01T12:00:00Z",
  "user": { "id": "uuid", "email": "person@example.com" }
}
```

Invalid credentials: `401 Unauthorized` with a generic message. **Proposed:** access token lifetime is 60 minutes; no refresh-token endpoint.

## Tasks — authenticated

### `GET /api/tasks`

Returns `200 OK` with an array of the current user's tasks. No pagination or filtering is introduced in this exercise. Each task:

```json
{
  "id": "uuid",
  "title": "Prepare demo",
  "description": "Review the interview walkthrough",
  "status": "Todo",
  "dueDate": "2026-01-01T12:00:00Z",
  "userId": "uuid"
}
```

### `POST /api/tasks`

Request:

```json
{
  "title": "Prepare demo",
  "description": "Review the interview walkthrough",
  "dueDate": "2026-01-01T12:00:00Z"
}
```

The server always creates the task with status `Todo`. The request does not accept a `status` field.

Success: `201 Created` with the task representation, including server-generated `id` and `userId`, and status `Todo`. Invalid input: `400 Bad Request`.

### `GET /api/tasks/{id}`

Success: `200 OK` with the task representation. Missing or non-owned ID: `404 Not Found`.

### `PUT /api/tasks/{id}`

Request has the same writable fields as task creation, including `status`; `userId` and `id` are not writable. The frontend edit form must let the user choose one of the four allowed status strings. Success: `200 OK` with the updated task representation. Invalid input or an unknown status: `400 Bad Request`. Missing or non-owned ID: `404 Not Found`.

### `DELETE /api/tasks/{id}`

Success: `204 No Content`. Missing or non-owned ID: `404 Not Found`.

## Common responses

- `400 Bad Request`: invalid JSON, validation failure, or invalid status on task update.
- `401 Unauthorized`: missing/invalid/expired bearer token (or invalid login credentials).
- `404 Not Found`: task does not exist for this user; does not reveal cross-user existence.
- `409 Conflict`: **proposed** duplicate email during registration.
- `500 Internal Server Error`: unexpected failure, with no sensitive implementation details.

The API must not return success-shaped fallback data when persistence or authentication fails.

## Approved contract assumptions

The owner approved the proposed error/status codes, field optionality/limits, password policy, and token lifetime in [business-rules.md](./business-rules.md) and [decisions.md](./decisions.md). Generate or update OpenAPI from the implemented API and keep it consistent with this document.
