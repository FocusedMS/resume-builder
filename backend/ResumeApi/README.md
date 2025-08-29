# ResumeApi

This directory contains the ASP.NET Core 8 Web API for the AI‑Powered Resume
Builder application. The API provides endpoints for user authentication,
resume management, AI suggestions and PDF generation. It uses Entity
Framework Core for data access, ASP.NET Core Identity for user management,
JWT for authentication, Serilog for logging and Swagger for API
documentation.

## Prerequisites

* .NET 8 SDK
* SQL Server (or SQL Server Express/localdb)
* EF Core CLI tools (optional for generating migrations)

## Getting started

1. Restore packages:

   ```bash
   dotnet restore
   ```

2. Update your connection string and JWT settings in `appsettings.json` as
   appropriate for your environment. The default connection string uses
   LocalDB on Windows.

3. Apply database migrations:

   ```bash
   # If the EF Core CLI is not installed:
   dotnet tool install --global dotnet-ef

   # Generate the initial migration (only needed the first time)
   dotnet ef migrations add InitialCreate --output-dir ../database/Migrations --project ResumeApi.csproj --startup-project ResumeApi.csproj

   # Apply migrations to your database
   dotnet ef database update --project ResumeApi.csproj --startup-project ResumeApi.csproj
   ```

4. Run the API:

   ```bash
   dotnet run
   ```

   The API will listen on `https://localhost:5001` and `http://localhost:5000`
   by default. Swagger UI is available at `/swagger`.

## Seeding roles

On first run, the API seeds three roles: `Admin`, `RegisteredUser` and
`Guest`. New accounts registered via the `POST /api/auth/register` endpoint
are automatically assigned the `RegisteredUser` role.

## Endpoints overview

For a full description of each endpoint, including request/response
examples and status codes, please see the API documentation in
`../../docs/API.md` or visit the built‑in Swagger UI.