# HelpDeskHQ

**An internal IT & Facilities helpdesk system with an automated SLA engine.**

Employees raise tickets. Support agents work them. The system does not just store tickets — it actively tracks Service Level Agreement (SLA) deadlines for every ticket and **automatically escalates** any ticket that breaches its response or resolution target, the same way real tools like Jira Service Management or Zendesk do under the hood.

This is a portfolio project built to demonstrate full-stack engineering and QA automation skills for Software Engineer / QA Engineer roles.

---

## Why this project exists

Most CRUD portfolio projects store data and let a human decide what to do with it. HelpDeskHQ's core feature is a **rules engine + recurring background job** that acts on data over time without anyone touching it:

- SLA targets are **data, not hardcoded logic** — admins configure response/resolution time targets per category and priority.
- A recurring background job (Hangfire) scans every open ticket on a schedule, calculates elapsed time against its SLA target, and **auto-escalates** any ticket that breaches its deadline — reassigning it to a Team Lead and logging the event.
- Ticket status follows an enforced state machine (e.g. you cannot jump straight from `New` to `Closed`) — every transition is validated server-side.

This combination — configurable business rules, time-based background processing, and a strict state machine — is intentionally different from typical tutorial projects, and is the same category of problem found in real enterprise ITSM tools.

---

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core Web API (.NET) |
| Database | PostgreSQL |
| ORM | Entity Framework Core |
| Background jobs | Hangfire (SLA escalation engine, auto-close job) |
| Real-time | SignalR (live ticket queue & SLA status updates) |
| Auth | JWT Bearer tokens, PBKDF2 password hashing |
| Frontend | React + TypeScript + Vite, Tailwind CSS |
| QA Automation | NUnit, RestSharp, Dapper, Npgsql |

---

## Core features

### Ticketing
- Role-based access: **Employee**, **Support Agent**, **Team Lead**, **Admin**
- Tickets categorized by type (Hardware, Software, Network, Facilities, Access Request) and priority (Critical, High, Medium, Low)
- Automatic routing to the correct support team based on category
- Full comment thread per ticket
- Complete, immutable status-change audit history

### SLA Engine (core feature)
- Admin-configurable SLA policy table — response and resolution time targets per category × priority combination
- Every ticket is stamped with calculated due dates at creation time
- Recurring background job continuously evaluates all open tickets:
  - Flags tickets approaching breach (**At Risk**)
  - Auto-escalates tickets that breach their deadline (**Breached** → reassigned to Team Lead)
- Separate background job auto-closes resolved tickets with no employee response after a configurable timeout

### Dashboard & Reporting
- Ticket counts by status, priority, and team
- SLA compliance rate (rolling 30-day window)
- Currently at-risk and breached tickets, sorted by urgency
- Team-level performance view for Team Leads

### Real-time updates
- Live ticket queue updates via SignalR (new tickets, status changes, escalations, new comments) without manual refresh

---

## Project structure

```
helpdeskhq/
├── backend/                 # ASP.NET Core Web API, Hangfire jobs, EF Core
│   ├── HelpDeskHQ.API/
│   ├── HelpDeskHQ.Core/
│   └── HelpDeskHQ.Infrastructure/
├── frontend/                # React + TypeScript + Vite
│   └── src/
├── qa-automation/           # NUnit test suite
│   └── HelpDeskHQ.Tests/
├── README.md
└── QA-TESTING.md
```

---

## Running locally

### Prerequisites
- .NET SDK
- Node.js
- PostgreSQL (with pgAdmin)

### Backend

```bash
cd backend
dotnet run --project HelpDeskHQ.API
```

Update the connection string in `backend/HelpDeskHQ.API/appsettings.json` to match your local PostgreSQL setup before running.

Swagger UI: `https://localhost:7XXX/swagger`
Hangfire dashboard: `https://localhost:7XXX/hangfire`

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend: `http://localhost:5173`

### QA test suite

Backend must be running first.

```bash
cd qa-automation/HelpDeskHQ.Tests
dotnet test --logger "console;verbosity=detailed"
```

See [QA-TESTING.md](./QA-TESTING.md) for full test coverage details.

---

## QA automation highlights

The most technically interesting tests in this project validate the **SLA escalation logic** directly:

- A ticket is seeded with a backdated `CreatedAt` timestamp via direct SQL (Dapper)
- The escalation job's logic is invoked directly (not via the Hangfire scheduler, for fast and deterministic tests)
- Assertions confirm the ticket's SLA breach status and escalation state updated correctly

This validates time-dependent background business logic, which is a meaningfully different testing skill than standard request/response API testing.

---

## Status

🚧 Actively under development as a portfolio project.

---

## Author

**Dilshan Kumarasingha**
Full-Stack Engineer & QA Automation Specialist — Colombo, Sri Lanka
[GitHub](https://github.com/Dilshan-Kumarasingha)
