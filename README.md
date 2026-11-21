# ToolShare

A community tool library. Members borrow shared tools from a catalogue and return them; an administrator looks after the catalogue and sees who has what. It has a REST API and an accessible, responsive web site.

Built to practise the same principles as a larger full-stack system: role-based access, a relational model with EF Core, a tested API, a separate front end that only talks to the API, containers, CI, and accessibility from the start.

## What it does
- **Members:** create an account, search tools, borrow for 1 to 30 days, return tools, see their loans and which are overdue.
- **Administrator** (the first account created): add, edit and delete tools, see every loan, filter to overdue loans.
- Passwords are hashed (BCrypt), sessions are server-side, forms are protected against CSRF, and role checks happen in the API (401 not signed in, 403 wrong role).

## Accessibility and inclusion
Targets WCAG 2.2 AA. See the in-app statement at `/Home/Accessibility`.
- Semantic landmarks, one `h1` per page, a skip link, `aria-current` on navigation, labelled forms with `autocomplete` tokens, tables with captions and `scope`, status announced with `role="status"` and `role="alert"`.
- Status never relies on colour alone (Available, On loan, Overdue are written out).
- Light and dark themes, `prefers-contrast` and `prefers-reduced-motion` respected, visible focus rings, 44 px minimum touch targets, text scales with browser zoom.
- Mobile-first responsive grid and scrollable, keyboard-focusable table regions; no horizontal page scroll.
- Plain-language copy, a single "Full name" field, and error messages that say how to fix the problem.
- Works without JavaScript: the site ships none.

Please still run an audit before relying on this (see "Checking accessibility").

## Projects
```
src/ToolShare.Api/        REST API (ASP.NET Core, EF Core, SQLite, Swagger)
src/ToolShare.Web/        MVC web site that calls the API (no database access)
tests/ToolShare.Api.Tests API integration tests (xUnit, in-memory database)
```

## Run it
Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

1. Open `ToolShare.sln` in Visual Studio. Right-click the solution, **Configure Startup Projects**, choose **Multiple startup projects**, and set `ToolShare.Api` and `ToolShare.Web` to **Start**. Press F5.
   Or in two terminals:
   ```
   dotnet run --project src/ToolShare.Api --launch-profile http
   dotnet run --project src/ToolShare.Web --launch-profile http
   ```
2. Open http://localhost:5100 and create an account (the first becomes the admin). Sample tools are added in development. Swagger is at http://localhost:5000/swagger.
3. To try the member view, create a second account in a private window.

Docker (whole system): `docker compose up --build`, then open http://localhost:8080.

## Tests
`dotnet test` (or Visual Studio Test Explorer). 13 integration tests cover registration, login, roles, search, borrowing, returning, and deletion rules.

## Checking accessibility
- Keyboard: tab through every page; you should always see where focus is and reach every control.
- Run Lighthouse (Chrome DevTools) or the axe DevTools extension on each page.
- Try a screen reader (NVDA on Windows, VoiceOver on macOS and iOS) and 200 percent zoom.
- Try forced colours / high contrast mode and the dark theme.

## API
| Method | Route | Who | Purpose |
|---|---|---|---|
| POST | /api/auth/register, /login, /logout | Anyone / signed in | Account and session |
| GET | /api/tools?search=&available= | Signed in | List and search |
| POST/PUT/DELETE | /api/tools[/{id}] | Admin | Manage catalogue |
| POST | /api/tools/{id}/borrow | Signed in | Borrow for N days |
| POST | /api/loans/{id}/return | Borrower or Admin | Return |
| GET | /api/loans/mine, /api/loans?overdue= | Signed in / Admin | View loans |

## Design notes and roadmap
- Sessions keep the example simple; JWT could replace them in `SessionAuthAttribute` and `AuthController`.
- `EnsureCreated()` keeps setup to zero steps; use EF migrations for production.
- [ ] Tool photos with alt text, [ ] reservations, [ ] email reminders for overdue loans, [ ] localisation, [ ] automated axe checks in CI.

Licence: MIT
