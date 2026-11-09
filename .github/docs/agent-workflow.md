# Agent Collaboration and Development Workflow

**Status: Approved by the project owner on 2026-11-07.** This completes the planning/setup task; application implementation was not part of this task.

## Source of truth

1. The Architect owns and maintains requirements interpretation, business rules, architecture, API contracts, and decision records.
2. The approved documents in `.github/docs/` are authoritative. In particular, the API contract governs frontend/backend integration.
3. If code and a contract disagree, the implementation agent reports the conflict and pauses the affected change. The Architect proposes a documented resolution; the owner approves material changes.
4. Agents inspect the existing repository and relevant documents before editing. Existing implementation is not assumed to match the proposal.

## Agent roles and file ownership

Detailed personas live in `.github/agents/`.

| Agent | Primary ownership | Must not do |
|---|---|---|
| Solution Architect / Domain Expert | `.github/docs/requirements.md`, `.github/docs/user-story.md`, `.github/docs/business-rules.md`, `.github/docs/architecture.md`, `.github/docs/api-contract.md`, `.github/docs/decisions.md`, `.github/docs/agent-workflow.md`, and project-wide instructions | Implement application features or make unapproved product decisions |
| Backend Developer | `backend/` application code and backend tests; implementation-specific endpoint/test notes coordinated with the Architect | Change agreed business rules/contracts or edit frontend-owned code without coordination |
| Frontend Developer | `frontend/` application code and frontend tests | Invent API behavior or edit backend-owned code/contracts without coordination |
| QA Engineer / Code Reviewer | Review findings and test/review reports; read-only inspection of implementation | Rewrite implementation or edit other agents' owned files without explicit coordination |

Shared configuration, root README, migrations that affect contracts, and any cross-stack changes require explicit coordination before editing. Ownership is a default collision-prevention rule, not a substitute for review.

## Required task handoff

Every implementation task or PR must state:

- Goal and scope, with links to the relevant approved document/contract in `.github/docs/`.
- Expected behavior and acceptance criteria.
- Dependencies and files/components expected to change.
- Tests/checks to run.
- Modified files, implementation decisions, commands/tests executed, and results.
- Deviations, unresolved questions, and contract conflicts.

Keep changes small and focused. No agent may add out-of-scope features, overengineer abstractions, or modify another agent's owned area without explicit coordination.

## Communication and shared context

- Use the issue/PR or task description as the durable work item; include links to the relevant `.github/docs/` sections.
- Record decisions in `.github/docs/decisions.md`, not only in chat.
- The Architect sends a concise contract/change note to both implementers when an approved decision changes.
- Backend publishes route, DTO, status, error, and auth changes through an OpenAPI update and a contract-aligned implementation note.
- Frontend reports integration assumptions and blocks on undocumented behavior rather than fabricating it.
- QA reports findings with severity, evidence (file/line or request/test), impact, reproduction/acceptance case, and suggested next step. QA prioritizes finding problems, not rewriting code.

## Stage workflow

### Stage 1 — Planning

Architect reviews the repository, refines requirements, agrees business rules/entities/relationships, establishes architecture, and finalizes API/DTO contracts and decisions. Owner approves documents. No application features are implemented in this stage.

### Stage 2 — Backend development

Backend establishes the solution/project boundaries, persistence and migrations, identity/JWT, use cases, CRUD API, and tests according to approved documents. Update OpenAPI and report endpoints, modified files, and test results.

### Stage 3 — Frontend development

Frontend inspects the approved Figma frames and reports design/contract mismatches, then builds auth screens, board, task forms/delete confirmation, API integration, and loading/error/validation/success states from the approved API contract. It can start its shell and isolated presentation work in parallel with backend foundation after approval; integration depends on the stable contract and reachable API. See [ui-design-reference.md](./ui-design-reference.md).

### Stage 4 — Integration and review

Integrate API and SPA, then QA reviews both sides and validates auth, user isolation, CRUD, four statuses, validation, and edge cases. Fixes are owned by the responsible implementation agent; QA verifies the fixes.

### Stage 5 — Final delivery

Update the root README with setup/run/test instructions, verify safe development-only seed data and demo credentials, check full application behavior, and prepare a concise technical presentation of architecture, trade-offs, testing, and known limitations.

## Dependencies and parallel work

```text
Owner approval of requirements/architecture/API contract
                 |
      +----------+-----------+
      |                      |
Backend foundation     Frontend shell/design
      |                      |
Auth + task API        Screens + components
      +----------+-----------+
                 |
         End-to-end integration
                 |
          QA review and fixes
                 |
         Final delivery checks
```

- Planning is a prerequisite for implementation.
- After approval, independent backend and frontend scaffolding can proceed in parallel.
- Backend auth and task work may proceed in parallel after shared persistence, identity, and API conventions are established, with one backend owner coordinating shared files.
- Frontend shell/components and QA test-matrix preparation can proceed alongside backend work.
- Frontend API integration depends on implemented/stable routes and an available API; cross-stack end-to-end testing depends on both.
- QA review can begin as backend/frontend changes land, but final validation follows integration.

## Scope and quality gates

- No implementation before explicit owner approval of this proposal.
- No secret keys, production credentials, or personal data in source control.
- Require tests for behavior changed and validate security-sensitive ownership and authentication paths.
- Do not claim completion based on a build alone; verify acceptance criteria.
- Avoid microservices, message brokers, charts, complex state management, and unrequested features.
