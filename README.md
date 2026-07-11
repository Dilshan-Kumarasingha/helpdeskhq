# HelpDeskHQ

An internal IT & Facilities helpdesk system with SLA tracking and automatic escalation.

Employees raise tickets, support agents work them, and a background job keeps an eye on SLA deadlines in the background — flagging tickets that are about to breach their target and auto-escalating the ones that actually do, reassigning them to a team lead. It's the same basic idea behind tools like Jira Service Management or Zendesk, just built from scratch to actually understand how that kind of system works under the hood.

## Why I built it this way

I didn't want another CRUD app that just stores data and calls it done. The interesting part of a real helpdesk system isn't the ticket form — it's what happens to a ticket *over time* after it's created. So the core of this project is:

- SLA response/resolution targets are configurable per category and priority (stored as data, not hardcoded if/else chains)
- A Hangfire background job runs on a schedule, checks every open ticket against its SLA clock, and escalates anything that's breached — it also correctly excludes time a ticket spent "On Hold" so a paused ticket doesn't get unfairly flagged
- Ticket status changes go through a proper state machine — you can't jump from New straight to Closed, every transition is validated server-side

## Stack

- **Backend:** ASP.NET Core Web API (.NET), Clean Architecture (API / Core / Infrastructure)
- **Database:** PostgreSQL + EF Core
- **Background jobs:** Hangfire (SLA escalation job, auto-close job)
- **Real-time:** SignalR, JWT-authenticated, scoped so users only get updates for tickets they can actually see
- **Auth:** JWT bearer tokens, PBKDF2 password hashing, role-based authorization
- **Frontend:** React + TypeScript + Vite, Tailwind
- **QA:** NUnit, RestSharp, Dapper, Npgsql

## Features

**Ticketing**
- Four roles: Employee, Support Agent, Team Lead, Admin
- Categories (Hardware, Software, Network, Facilities, Access Request) route to the right team automatically
- Comment threads, full status-change audit trail
- Assign/resolve/status-change actions are locked down server-side to staff roles only — an Employee can't call those endpoints directly even if they find them in Swagger

**SLA engine**
- Admin-configurable policy table: response/resolution targets per category × priority
- Due dates get calculated and stamped on the ticket at creation
- Background job evaluates elapsed time (minus hold time) against the target, flags At Risk at 80%, Breached at 100%, and auto-escalates to the team's lead
- A second job auto-closes resolved tickets that sit untouched past a timeout
- Both jobs skip and log a bad ticket instead of failing the whole batch

**Dashboard**
- Ticket counts by status/priority/team
- Rolling 30-day SLA compliance rate
- At-risk / breached tickets sorted by urgency

**Real-time**
- SignalR pushes ticket updates, new comments, and escalations live
- Hub requires auth, and a client can only join the update group for a ticket they're actually allowed to view

## A note on the backend structure

I went through this and cleaned up a bunch of stuff after getting it working the first time — replaced a pile of generic `InvalidOperationException` throws with proper typed exceptions (`NotFoundException`, `ConflictException`, `ValidationException`) mapped to real HTTP status codes in a middleware, pulled repeated "does this exist / is this a duplicate" checks out of the admin services into a shared helper, and locked down a couple of things that had no business being open — the Hangfire dashboard had zero auth on it originally, and ticket assignment didn't actually check whether the person you were assigning to was staff. Also moved the JWT key and DB connection string out of `appsettings.json` and into user-secrets, since they were sitting in plaintext before.

None of this changed what the app does — it was about making the code something I can actually walk someone through in an interview without getting caught out.

## Running it locally

**You'll need:** .NET SDK, Node.js, PostgreSQL (pgAdmin's handy for poking at the DB)

**Backend** — set up secrets first, don't skip this:

```bash
cd backend/HelpDeskHQ.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=helpdeskhq;Username=postgres;Password=YOUR_PASSWORD"
dotnet user-secrets set "Jwt:Key" "YOUR_OWN_RANDOM_64_CHAR_SECRET"
```

Then:

```bash
dotnet run --project HelpDeskHQ.API
```

- Swagger: `https://localhost:7XXX/swagger`
- Hangfire dashboard: `https://localhost:7XXX/hangfire` — you'll need to be logged in as an Admin to actually see anything, it's not open

**Frontend:**

```bash
cd frontend
npm install
npm run dev
```

Runs on `http://localhost:5173`.

**Tests** (backend needs to be running):

```bash
cd qa-automation/HelpDeskHQ.Tests
dotnet test --logger "console;verbosity=detailed"
```

More detail on what's covered in [QA-TESTING.md](./QA-TESTING.md).

## The test I'm most happy with

The SLA escalation tests are the ones I'd actually talk through in an interview — they seed a ticket with a backdated `CreatedAt` straight into Postgres via Dapper (bypassing the API so I control the exact timestamp), then call the escalation job's logic directly instead of waiting on Hangfire's scheduler, and assert the breach status and escalation record came out right. Testing time-based background logic is a different problem than testing a normal request/response endpoint, and I wanted at least one test in here that proves I can do that.

## Status

Still actively working on this — next up is probably attachments upload, which the data model already supports but nothing in the API exposes yet.

## Author

**Dilshan Kumarasingha**
Full-Stack Engineer & QA Automation Specialist — Colombo, Sri Lanka
[GitHub](https://github.com/Dilshan-Kumarasingha)
