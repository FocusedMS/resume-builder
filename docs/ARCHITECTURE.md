# Architecture

The AI‑Powered Resume Builder follows a clear separation of concerns
between the frontend and backend, and within the backend itself. This
ensures the system is modular, maintainable and testable.

## High‑level overview

```
┌────────────┐     HTTP     ┌─────────────────────┐     ORM      ┌─────────────┐
|  Frontend  | ───────────> | ASP.NET Core API    | ───────────> | SQL Server  |
└────────────┘              | Controllers & Routes|             └─────────────┘
                            └─────────────────────┘
```

1. **Frontend (React + Vite + TypeScript)**: Provides a responsive, single
   page application. It communicates with the backend via fetch/axios calls,
   storing the JWT in `localStorage` and adding it to the `Authorization`
   header for secured requests.
2. **ASP.NET Core API**: Exposes RESTful endpoints for authentication,
   resume CRUD operations, AI suggestion generation and PDF export. It uses
   controllers to orchestrate requests, services to encapsulate business
   logic and repositories to abstract data access.
3. **Database**: A SQL Server instance accessed through Entity Framework
   Core. The `AppDbContext` defines DbSets for Identity tables and the
   `Resume` entity. Migrations manage schema evolution.

## Backend layers

```
Client
  ↓ HTTP
Controllers (AuthController, ResumesController, AdminController)
  ↓ delegate
Services (JwtService, AiSuggestionService, PdfService)
  ↓ access
Repositories (ResumeRepository)
  ↓ use
DbContext (AppDbContext) → SQL Server
```

* **Controllers** handle routing, model binding, authentication and
  authorization. They are thin; any business logic is delegated to services.
* **Services** implement application logic. For example, `JwtService`
  encapsulates token creation, `AiSuggestionService` runs heuristics on
  resume content and `PdfService` generates PDFs with iText.
* **Repositories** abstract the details of EF Core. The API does not
  directly query or update the `AppDbContext`, instead going through
  repository interfaces that can be swapped out for alternative storage.
* **DbContext** orchestrates EF Core to create and query the database.

## Error handling

Serilog is configured to log structured events to the console. ASP.NET
Core’s built‑in exception handling middleware returns RFC7807
ProblemDetails payloads on unhandled exceptions without leaking stack
traces to clients. Validation errors return `400 Bad Request` with detailed
field information.

## Authentication & authorization

Upon successful login, the backend issues a JWT signed with a secret key.
Clients must include this token in the `Authorization: Bearer` header for
protected endpoints. ASP.NET Core Identity stores user credentials and roles
in the database. Custom policies restrict access to actions based on role
(`Admin`, `RegisteredUser`) and resource ownership.

## AI suggestions

The AI suggestion feature runs locally in the backend without hitting
external services. It applies simple heuristics to the length and content of
resume sections and returns a structured list of suggestions with
priorities and sample text. The results are persisted as JSON to the
`AiSuggestionsJson` column on the resume for future reference.