# Architecture Decisions

**Status: Approved by the project owner on 2026-11-07.**

| ID | Topic | Approved decision | Rationale | Owner approval |
|---|---|---|---|---|
| D-01 | System shape | One ASP.NET Core API, one Angular SPA, one SQL Server database | Fits the three-day exercise and avoids distributed-system overhead | Approved |
| D-02 | Backend boundaries | Domain, Application, Infrastructure, and API projects; dependencies point inward | Enforces practical Clean Architecture without excessive patterns | Approved |
| D-03 | User identity | ASP.NET Core Identity with a Guid user key, implemented in Infrastructure | Uses established password handling while keeping Domain independent | Approved |
| D-04 | Ownership | Derive owner from JWT subject; scope every query/mutation; return 404 for another user's task | Prevents IDOR and existence disclosure consistently | Approved |
| D-05 | Task fields | Title required (max 120); description optional (max 2,000); due date optional UTC instant | Provides practical validation where the brief leaves details open | Approved |
| D-06 | Status behavior | Exact four string values; unrestricted movement between them | Matches the Kanban requirement without adding workflow rules | Approved |
| D-07 | Auth | JWT access token, 60-minute expiry, no refresh flow in initial scope | Simple, testable authentication for a short exercise | Approved |
| D-08 | SPA token storage | `sessionStorage` plus one bearer interceptor; clear on logout | Survives in-tab navigation/refresh but not browser-session closure; still exposed to XSS | Approved |
| D-09 | API errors | ASP.NET Core Problem Details; duplicate registration returns 409 | Consistent client handling and no implementation leakage | Approved |
| D-10 | Testing | Unit tests plus API integration tests against configured SQL Server | Verifies actual relational behavior and ownership boundary | Approved |
| D-11 | Current scaffold | Preserve Angular 22 and .NET 10 baselines unless the owner approves changes | These versions are already present in the repository | Approved |
| D-12 | Password policy | Minimum 8 characters, with uppercase, lowercase, digit, and non-alphanumeric character | Explicit, testable registration behavior using Identity password validation | Approved |
| D-13 | Registration name | Omit full name from registration; collect email and password only | Keeps the form and request aligned with the approved registration contract | Approved 2026-11-07 |
| D-14 | Task status UI | Add a status selector to the edit-task dialog with the four exact API values and Figma display labels | Provides the required status-update capability; intentional addition to the Figma edit frame | Approved 2026-11-07 |
| D-15 | Account label | Use authenticated email in the dashboard account area and derive avatar initials from it; do not persist a profile name | No display-name property or registration field is in scope | Approved 2026-11-07 |

## Decision process

The Architect records decisions and rationale here. The owner approved this set before implementation. If new evidence requires a change, the responsible agent raises the conflict, records the agreed amendment, then updates dependent contracts and code. Do not silently change a business rule or API contract.
