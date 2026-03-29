# Auth Assessment - Fund Transfer API

A full-stack application built with **ASP.NET Core 9** and **React + TypeScript**, featuring JWT authentication with refresh token rotation, role-based authorization, and fund transfer capabilities.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 9, C# 13 |
| Database | SQL Server (Docker) |
| ORM | Entity Framework Core 9 |
| Auth | JWT (HS256) + BCrypt + Refresh Token Rotation |
| Frontend | React 19, TypeScript, Material-UI, Vite |
| Testing | xUnit, Moq, FluentAssertions, WebApplicationFactory |

## Architecture

Clean Architecture with strict layer boundaries:

```
Domain          → Entities, Enums (no dependencies)
Application     → DTOs, Interfaces, Use Cases (depends on Domain)
Infrastructure  → EF Core, Repositories, Services (depends on Application)
API             → Controllers, Middleware (depends on Application)
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)

## Quick Start with Docker

```bash
docker compose up --build
```

This starts:
- **API** at `http://localhost:5154`
- **React frontend** at `http://localhost:5173`
- **SQL Server** at `localhost:1433`

## Manual Setup

### 1. Database (SQL Server via Docker)

```bash
docker compose up db -d
```

Or use any SQL Server instance and update the connection string.

### 2. Backend API

```bash
cd src/AuthAssessment.API
dotnet run
```

The API starts at `http://localhost:5154`. Swagger UI is available at `/swagger`.

Database migrations run automatically on startup.

### 3. Frontend

```bash
cd src/client-app
npm install
npm run dev
```

The frontend starts at `http://localhost:5173`.

## Environment Variables

| Variable | Description | Default |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | See `appsettings.json` |
| `Jwt__SecretKey` | JWT signing key (min 32 chars) | Must be set in production |
| `Jwt__Issuer` | JWT issuer | `AuthAssessment` |
| `Jwt__Audience` | JWT audience | `AuthAssessment` |
| `Jwt__AccessTokenExpiryMinutes` | Access token lifetime | `15` |
| `Jwt__RefreshTokenExpiryDays` | Refresh token lifetime | `7` |

For local development, use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```bash
cd src/AuthAssessment.API
dotnet user-secrets set "Jwt:SecretKey" "YourSecureProductionKeyAtLeast32CharsLong!"
```

## API Endpoints

### Auth
| Method | Route | Description | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/login` | Login, returns JWT | No |
| POST | `/api/auth/refresh` | Refresh access token | No |
| POST | `/api/auth/logout` | Revoke all refresh tokens | Yes |

### Users
| Method | Route | Description | Auth |
|---|---|---|---|
| GET | `/api/users/me` | Get current user profile | Yes |
| GET | `/api/users` | List all users | Admin |

### Transactions
| Method | Route | Description | Auth |
|---|---|---|---|
| POST | `/api/transactions/transfer` | Transfer funds | User |
| GET | `/api/transactions` | My transaction history | User |
| GET | `/api/transactions/all` | All transactions | Admin |

## Default Account

| Email | Password | Role |
|---|---|---|
| `admin@authassessment.com` | `Admin@123` | Admin |

New users register with a starting balance of **$10,000**.

## Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/AuthAssessment.Tests.Unit

# Integration tests only
dotnet test tests/AuthAssessment.Tests.Integration
```

**Test coverage**: 22 unit tests + 8 integration tests covering auth flows, fund transfers, transaction history, and user management.

## Key Design Decisions

- **Refresh token rotation**: Old tokens are revoked on refresh, preventing token reuse attacks
- **BCrypt password hashing**: Industry-standard adaptive hashing with salt
- **Global exception middleware**: Centralized error handling maps exceptions to HTTP status codes
- **Consolidated use cases**: `AuthUseCase`, `TransactionUseCase`, `UserUseCase` — each containing all related operations
- **15-minute access token expiry**: Auto-logout after inactivity; frontend handles transparent refresh
