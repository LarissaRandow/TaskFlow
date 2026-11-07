# Business Rules

**Status: Approved by the project owner on 2026-11-07.** Rules marked **Proposed** fill gaps in the original brief; the owner approved these assumptions for this exercise.

## Identity and ownership

1. A task belongs to exactly one user.
2. The authenticated principal is the only source of the task owner ID. Ignore or reject client-supplied ownership fields; never trust them.
3. Every task query and mutation is scoped by both task ID (when applicable) and authenticated user ID.
4. A task owned by a different user is indistinguishable from a missing task at the API boundary (**proposed:** return 404).
5. Registration and login are public. All task endpoints require a valid bearer token.
6. **Proposed password policy:** require at least 8 characters, including an uppercase letter, a lowercase letter, a digit, and a non-alphanumeric character. Store only a password hash using ASP.NET Core Identity; never log credentials.

## Task fields

- `Id`: server-generated UUID.
- `Title`: required, trimmed, non-empty (**proposed maximum: 120 characters**).
- `Description`: **proposed optional**, with a maximum of 2,000 characters.
- `Status`: required; exactly `Todo`, `InProgress`, `Completed`, or `Discarded`.
- `DueDate`: **proposed optional** ISO-8601 date/time; persist and return an unambiguous UTC instant.
- `UserId`: server-assigned UUID from the authenticated identity; never editable through task DTOs.

The title/description limits, optionality of description and due date, and password policy fill details not explicit in the original brief. These assumptions are approved for this exercise.

## Status

- The allowed set is closed and contains exactly four values: `Todo`, `InProgress`, `Completed`, `Discarded`.
- A task may be moved directly between any two allowed statuses; no transition workflow was requested (**proposed**).
- Status values are represented by their exact names in JSON; unknown values are rejected, not silently mapped.
- The task edit experience exposes a status selector for exactly the allowed status values.

## Validation and failures

- Malformed requests and invalid field values return a client error with actionable validation details.
- Missing/invalid authentication returns 401. Authenticated access to another user's task returns the same 404 as a missing task.
- Infrastructure failures must not be reported as successful operations.
- Authentication errors must not reveal whether an account exists or which credential was incorrect.

## Approved assumptions

- Maximum field lengths and optional fields are as stated above.
- The password policy, 404 behavior for non-owned task IDs, and unrestricted transitions between valid statuses are as stated above.
- Registration collects email and password only. The frontend shows the authenticated email as the account label and may derive avatar initials from it; it does not persist a profile name.
- The task edit dialog includes a status selector even though that control is not present in the current Figma frame.
- The JWT lifetime and related decisions are recorded in [decisions.md](./decisions.md).
