# Team Notes API

A shared team note-taking REST API built with ASP.NET Core, PostgreSQL, Docker, JWT authentication, and role-based authorization.

## Technology Stack

- ASP.NET Core Web API
- C#
- Entity Framework Core
- PostgreSQL
- Docker Compose
- JWT bearer authentication
- Swagger/OpenAPI
- xUnit integration tests

## Project Structure

```text
database/                  PostgreSQL schema and seed scripts
src/TeamNotes.Api/         ASP.NET Core API
tests/TeamNotes.Api.Tests/ Automated API tests
docker-compose.yml         PostgreSQL container configuration
```

## Prerequisites

- .NET 10 SDK
- Docker Desktop
- Git

## Configuration

Copy the development configuration template:

```powershell
Copy-Item `
  src\TeamNotes.Api\appsettings.Development.example.json `
  src\TeamNotes.Api\appsettings.Development.json
```

Update `appsettings.Development.json` with the local PostgreSQL password and a development JWT key. This file is intentionally ignored by Git.

## Start PostgreSQL

From the project root:

```powershell
docker compose up -d postgres
```

The local database uses database `teamnotes`, username `teamnotes`, password `teamnotes`, and port `5432`.

The schema is automatically initialized when the PostgreSQL volume is created.

To inspect the database tables:

```powershell
docker exec -it teamnotes-postgres `
  psql -U teamnotes -d teamnotes -c "\dt"
```

To completely reset the local database:

```powershell
docker compose down -v
docker compose up -d postgres
```

This deletes the local PostgreSQL volume and all local data.

## Run the API

From the project root:

```powershell
dotnet run --project src\TeamNotes.Api
```

The API runs at `http://localhost:5082`.

Health endpoints:

```text
GET /health
GET /health/database
```

## Swagger UI

Open `http://localhost:5082/swagger` for interactive API documentation.

To test protected endpoints:

1. Use `POST /api/users` to create a user.
2. Use `POST /api/auth/login`.
3. Copy the returned `accessToken`.
4. Click Swagger's **Authorize** button.
5. Paste only the JWT token beginning with `eyJ`.
6. Do not include the `Bearer` prefix.
7. Click **Authorize** and then **Close**.

Swagger automatically sends the token as `Authorization: Bearer <token>`.

## Main API Endpoints

### Users and Authentication

```text
POST /api/users
POST /api/auth/login
```

### Teams

```text
GET  /api/teams
POST /api/teams
POST /api/teams/{teamId}/members
```

### Notes

```text
GET    /api/teams/{teamId}/notes
POST   /api/teams/{teamId}/notes
GET    /api/notes/{noteId}
PUT    /api/notes/{noteId}
DELETE /api/notes/{noteId}
```

## Team Roles

Roles are ordered from lowest to highest permission:

```text
Member < Editor < TeamLead < Owner
```

| Role | Permissions |
|---|---|
| Member | View team notes |
| Editor | View, create, and update notes |
| TeamLead | Editor permissions plus delete notes |
| Owner | Full team control, including adding members |

A TeamLead is also a member of the team.

## Run Tests

Stop the running API before running tests so the test build can replace the API executable:

```text
Ctrl+C
```

Run all tests:

```powershell
dotnet test tests\TeamNotes.Api.Tests\TeamNotes.Api.Tests.csproj
```

The tests cover JWT claim extraction, unauthorized requests, invalid and valid login, authenticated team creation, authenticated note creation and retrieval, and Editor permissions.

Restart the API after testing:

```powershell
dotnet run --project src\TeamNotes.Api
```

## Design Decisions

### JWT Authentication

JWT authentication was selected because it is stateless, widely supported, and appropriate for REST APIs. The authenticated user's ID is stored in token claims and used to enforce team membership and permissions.

### PostgreSQL with Docker

PostgreSQL runs in Docker Compose so development databases are consistent across environments. Database initialization is handled through versioned SQL scripts.

### Soft Deletion for Notes

Notes are not physically deleted. Instead, they are marked with `is_deleted = true`. This preserves historical data while keeping deleted notes out of normal API results.

### Role-Based Authorization

Permissions are represented using team membership roles. This keeps authorization close to the domain model and allows different teams to assign different roles to the same user.

## Future Improvements

- Add refresh tokens and token revocation.
- Add pagination metadata and filtering for notes.
- Add an endpoint to list team members.
- Add role update and member removal endpoints.
- Add database migrations instead of `EnsureCreated`.
- Add centralized error codes and structured logging.
- Add CI pipeline execution for tests.
- Add a web-based frontend for non-technical users.
- Add production secret management through environment variables or a secure secret store.

## Local Development Notes

The committed configuration template is safe to share. Do not commit:

```text
src/TeamNotes.Api/appsettings.Development.json
```

Use environment-specific configuration and secure secret management for production deployments.
