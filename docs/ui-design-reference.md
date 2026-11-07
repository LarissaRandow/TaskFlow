# TaskFlow UI Design Reference

**Status: Approved visual source of truth.** The project's primary UI/UX reference is the Figma file [Taks Flow](https://www.figma.com/design/JBzurmXKm1pWnj31wGUtU3/Taks-Flow?node-id=0-1). The Frontend Developer must inspect the relevant frame in Figma through the connected Figma MCP before frontend implementation and compare rendered screens against it.

## Inspected Figma frames

File key: `JBzurmXKm1pWnj31wGUtU3`; page `0:1` (`Page 1`).

| Frame | Node ID | Size | What it shows |
|---|---:|---:|---|
| Login | `5:3133` | 1440 × 960 | Brand header, centered sign-in card, email/password fields, password visibility control, create-account link, footer |
| Register | `5:3162` | 1440 × 960 | Brand header, centered registration card, email/password/confirm-password fields, password visibility controls, sign-in link, footer |
| Dashboard · Task actions | `5:3201` | 1440 × 960 | Personal task board, four columns, task cards, card action menu |
| Dashboard · Create task | `5:3354` | 1440 × 960 | Board with create-task modal, title/description/due-date inputs, cancel/save actions |
| Dashboard · Edit task | `5:3532` | 1440 × 960 | Board with edit modal, prefilled title/description/due date, current-status context, cancel/save actions |
| Dashboard · Delete task | `5:3710` | 1440 × 960 | Board with delete-confirmation modal, task summary, cancel/destructive delete actions |
| TaskFlow · Reusable components | `5:4254` | 1700 × 2740 | Component sheet for brand, inputs, buttons, task cards, action menu, and form controls |

These are desktop frames only. The file provides no inspected mobile or tablet layout. The developer must implement a responsive adaptation that preserves the desktop visual language, but must report where responsive behavior necessarily departs from the supplied frames; do not claim those layouts came from Figma.

## Observed visual language

- The dashboard uses a light neutral page (`#f7f8fa`), white header and cards, pale-gray columns (`#f0f2f5`), fine gray borders (`#e4e7ec`), dark navy text (`#202536`), muted slate text (`#667085` / `#98a2b3`), and indigo primary actions (`#5155d9`).
- The destructive action uses red (`#d04444`). Status accents distinguish the columns, including blue for In Progress, green for Completed, and red for Discarded.
- Typography is Inter with medium/semibold weights. The interface favors compact labels, restrained borders, rounded controls/cards, and generous surrounding whitespace.
- The dashboard header contains the TaskFlow mark, “My tasks” navigation, and a user avatar/menu. The board heading includes a short subtitle and a “Create Task” action.
- The board shows four side-by-side columns with status/count headers and task cards containing a title, description, due-date row, and overflow action.
- Create, edit, and delete use centered white dialogs over a dimmed dashboard backdrop. Keep each dialog's own contents/actions and use the exact Figma asset slots; adapt their layout on narrow screens.
- Authentication pages use a centered white card on the neutral canvas, a brand header, small tagline, concise title/subtitle, full-width fields/actions, and understated footer.

Figma MCP inspection is authoritative for exact geometry, typography, assets, and component variants when a frame is being implemented. The notes above are a quick orientation, not a substitute for inspecting the target frame and its component/variable context. Do not use the generated React-like snippets as the application implementation; adapt the visual reference to Angular, TypeScript, and SCSS.

## Reusable components to seek or create

- TaskFlow brand mark and header/navigation.
- Primary, secondary, and destructive buttons.
- Labeled text, password, and date fields.
- Authentication card and form field layout.
- Kanban column/status header and task card.
- Task overflow menu with Edit/Delete actions.
- Shared task create/edit dialog shell and delete confirmation dialog.
- Status/count badges and icon affordances.

Reuse available Figma assets exactly where they appear. Before implementation, inspect matching assets and project dependencies/components; download required static assets through the Figma MCP and use local files rather than temporary Figma URLs.

## Requirement/contract mismatches to resolve, not assume

1. **Registration full name:** although the Figma registration frame includes a full-name field, the owner decided to omit it. The registration UI and request follow the approved contract (`email` and `password` only); do not collect or persist a client-only name.
2. **Task status editing:** the owner requested an editable status field in the edit-task dialog. Add a status selector with exactly `Todo`, `InProgress`, `Completed`, and `Discarded`, displaying the human-readable labels below. This is an intentional addition beyond the current Figma edit frame, which only shows the existing status as context. Do not add drag-and-drop unless separately approved.
3. **Status labels:** the API/domain values are `Todo`, `InProgress`, `Completed`, and `Discarded`; the visual labels are “To Do”, “In Progress”, “Completed”, and “Discarded”. Keep wire values exact and map to the human-readable Figma labels in presentation.
4. **Responsive layouts:** only 1440 × 960 screens were inspected. Responsive rearrangements are necessary but are not specified by Figma; report the adaptation and test it at narrow widths.
5. **Account display name:** the dashboard shows a user's name and initials, while the login response supplies ID and email and registration does not collect a name. Do not fabricate a persistent profile-name source; use the authenticated email for the visible account label and derive any avatar initials from it.

## Frontend agent checklist

- Inspect the specific Figma frame and relevant component/variable definitions before implementation; verify page/frame IDs rather than relying only on this summary.
- Capture the design screenshot as the visual target and compare the implemented screen at the same 1440 × 960 size.
- Use Figma's colors, typography, spacing, hierarchy, component states, and static assets; identify reusable components before recreating them.
- Implement responsive behavior while distinguishing design observations from necessary adaptations.
- Follow the owner resolutions above. Report any remaining requirement/API mismatch before making assumptions. Do not pretend to inspect unavailable screens or fabricate Figma guidance.
