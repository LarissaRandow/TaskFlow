# User Stories and Acceptance Criteria

**Status: Approved by the project owner on 2026-11-07.** Stories are intentionally limited to the requested interview exercise.

## Authentication

### Register

As a visitor, I want to create an account with my email and password so that I can manage private tasks.

Acceptance criteria:

- The registration form requests email and password only; it does not request a full name.
- A valid registration creates one account.
- Invalid or duplicate account data returns a documented client error and a useful field-level or general message.
- Registration does not disclose or grant access to another user's data.
- The API does not return a password or password hash.

### Log in

As a registered user, I want to log in and receive an access token so that I can use protected task operations.

Acceptance criteria:

- Valid credentials return a signed JWT and its expiry information.
- Invalid credentials fail without revealing whether the email or password was incorrect.
- The UI displays authentication errors and does not enter the authenticated state on failure.
- The frontend sends the access token only to protected API requests and clears its client-side token on logout.

## Personal task management

### View board

As an authenticated user, I want to see my tasks grouped into the four Kanban columns so that I can understand my work at a glance.

Acceptance criteria:

- The board displays exactly `Todo`, `InProgress`, `Completed`, and `Discarded`.
- The list contains only tasks owned by the authenticated user.
- Empty columns, loading, and API error states are handled.
- The API does not allow a caller to select another user as the task owner.

### Create a task

As an authenticated user, I want to create a task with a title, description, status, and optional due date so that it appears on my board.

Acceptance criteria:

- A successful creation assigns the current user's ID on the server.
- Invalid input returns a documented validation response.
- A newly created task is visible in the correct board column.

### View, edit, and delete a task

As an authenticated user, I want to retrieve, update, and delete one of my tasks so that I can keep my board accurate.

Acceptance criteria:

- An owner can retrieve and update their task, including changing it to any of the four valid statuses.
- The edit form lets the owner select one of the four allowed statuses and saves it through the documented update API.
- A non-owner cannot read, update, or delete the task. The proposed API responds as though the task does not exist (404) to avoid disclosing its existence.
- A successful deletion removes the task and reports success without a response body.
- The UI confirms deletion and reflects successful updates; failures remain visible and do not look like success.

## Delivery checks

- A clean checkout can be configured and run using the eventual README.
- Automated tests cover the acceptance criteria, especially ownership boundaries.
- Demo data and credentials, if provided, are development-only and documented safely.
