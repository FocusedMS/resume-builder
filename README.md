# AI‑Powered Resume Builder

This repository contains a full‑stack implementation of an AI‑powered
resume builder. The backend is built with ASP.NET Core 8, Entity
Framework Core and Identity with JWT authentication. The frontend uses
React, Vite and TypeScript to deliver a modern, responsive UI.

## Project structure

```
app/
  backend/ResumeApi/    # ASP.NET Core Web API
  frontend/             # React + Vite SPA
  database/Migrations/  # EF Core migrations (see README)
  docs/                 # API docs, roles, screens and architecture
  README.md             # Top‑level guide (this file)
```

## Getting started

To run the project locally you will need the .NET SDK 8, Node.js and
SQL Server (or SQL Server Express/localdb). The following steps assume
you are working from the `app` directory.

### Backend

1. Navigate to the backend directory:

   ```bash
   cd backend/ResumeApi
   ```

2. Restore dependencies and build the project:

   ```bash
   dotnet restore
   ```

3. Update `appsettings.json` with your own connection string and JWT
   secret. The default uses LocalDB.

4. Apply the initial migration and update the database:

   ```bash
   dotnet tool install --global dotnet-ef # if not already installed
   dotnet ef migrations add InitialCreate --output-dir ../../database/Migrations --project ResumeApi.csproj --startup-project ResumeApi.csproj
   dotnet ef database update --project ResumeApi.csproj --startup-project ResumeApi.csproj
   ```

5. Start the API:

   ```bash
   dotnet run
   ```

   The API will be available at `http://localhost:5000` and `https://localhost:5001`.
   Swagger UI is hosted under `/swagger`.

### Frontend

1. Navigate to the frontend directory:

   ```bash
   cd frontend
   ```

2. Install dependencies:

   ```bash
   npm install
   ```

3. Start the development server:

   ```bash
   npm run dev
   ```

   The app will run at `http://localhost:5173` (default Vite port). Ensure
   that the `VITE_API_URL` in `.env.local` points to your running backend.

### Environment variables

The frontend reads its configuration from `.env.local`. An example file is
included:

```
VITE_API_URL=http://localhost:5000
```

Update the URL if your backend is running on a different host or port.

## Documentation

Detailed API documentation and other project information can be found in
the `docs` folder:

* [API.md](docs/API.md) – description of available endpoints, sample
  requests and responses
* [ROLES.md](docs/ROLES.md) – overview of the different user roles and their
  permissions
* [SCREENS.md](docs/SCREENS.md) – summaries of the primary UI screens
* [ARCHITECTURE.md](docs/ARCHITECTURE.md) – high‑level architecture and
  design notes

## Acknowledgements

This project was generated as part of a capstone exercise. It omits
deployment and automated testing to focus on the core functionality.