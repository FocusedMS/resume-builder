This directory is intended to contain Entity Framework Core database migration files.

Since the execution environment used to generate this code does not include
the .NET SDK or the `dotnet ef` CLI, the initial migration could not be
generated automatically. To create the initial migration in your local
development environment, install the EF Core tools and run:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project ../backend/ResumeApi/ResumeApi.csproj --startup-project ../backend/ResumeApi/ResumeApi.csproj --output-dir ../database/Migrations
```

This will scaffold the `InitialCreate` migration class and a snapshot of the
model. Commit the generated files under `database/Migrations` to version
control.