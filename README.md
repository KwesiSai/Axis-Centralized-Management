# Centralized User Management System

A full-stack application for managing user accounts, application access, and login validation.

**Stack:** .NET 8 Web API · PostgreSQL · React 18 (Vite)

---

## Project Structure

```
centralized-user-management/
├── backend/
│   └── UserManagementAPI/
│       ├── Controllers/
│       │   ├── AuthController.cs         # POST /api/auth/login
│       │   ├── UsersController.cs        # Full user CRUD + app assignments
│       │   └── ApplicationsController.cs # GET /api/applications
│       ├── Data/
│       │   └── AppDbContext.cs           # EF Core context + seed data
│       ├── DTOs/
│       │   └── Dtos.cs                   # Request/response records
│       ├── Migrations/
│       │   └── InitialCreate.cs
│       ├── Models/
│       │   ├── Application.cs
│       │   ├── User.cs
│       │   └── UserApplication.cs
│       ├── appsettings.json
│       └── Program.cs
└── frontend/
    └── admin-ui/
        └── src/
            ├── context/AuthContext.jsx   # JWT auth state
            ├── pages/
            │   ├── LoginPage.jsx
            │   ├── UsersPage.jsx         # List, create, filter, toggle
            │   └── UserDetailPage.jsx    # Assign/revoke app access
            ├── services/api.js           # Axios service layer
            └── App.jsx                   # Protected routing
```

---

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 8.x |
| Node.js | 18+ |
| PostgreSQL | 14+ |

---

## Setup

### 1 — Database

Create the database in PostgreSQL:

```sql
CREATE DATABASE user_management;
```

### 2 — Backend

```bash
cd backend/UserManagementAPI
```

**Edit `appsettings.json`** — update the connection string and JWT key:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=user_management;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Key": "REPLACE_WITH_A_STRONG_SECRET_AT_LEAST_32_CHARS",
    "Issuer": "UserManagementAPI",
    "Audience": "UserManagementClient"
  }
}
```

Install the EF Core CLI tool (once):

```bash
dotnet tool install --global dotnet-ef
```

Install packages, run migrations, and start:

```bash
dotnet restore
dotnet ef migrations add InitialCreate    # skip if Migrations/ folder already exists
dotnet ef database update                 # creates tables + seeds HR and Accounting apps
dotnet run
```

API runs at: **http://localhost:5000**
Swagger UI: **http://localhost:5000/swagger**

> Migrations also run automatically on startup via `db.Database.Migrate()` in `Program.cs`.

### 3 — Frontend

```bash
cd frontend/admin-ui
npm install
npm run dev
```

App runs at: **http://localhost:5173**

---

## API Reference

### Auth

| Method | Endpoint | Body | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/login` | `{ username, password, application }` | Returns JWT token |

### Users

| Method | Endpoint | Body | Description |
|--------|----------|------|-------------|
| GET | `/api/users` | — | List all users |
| GET | `/api/users/{id}` | — | Get user by ID |
| POST | `/api/users` | `{ username, password }` | Create user |
| PATCH | `/api/users/{id}/deactivate` | — | Deactivate user |
| PATCH | `/api/users/{id}/activate` | — | Reactivate user |
| DELETE | `/api/users/{id}` | — | Delete user |

### Application Assignment

| Method | Endpoint | Body | Description |
|--------|----------|------|-------------|
| POST | `/api/users/{id}/applications` | `{ applicationId }` | Assign app to user |
| DELETE | `/api/users/{id}/applications/{appId}` | — | Revoke app from user |
| GET | `/api/applications` | — | List all applications |

---

## Test Cases

Run these to verify core behaviour.

### Setup — Create users via Swagger or API

```bash
# Create user assigned to HR
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","password":"pass123"}'

# Assign alice to HR (applicationId 1)
curl -X POST http://localhost:5000/api/users/1/applications \
  -H "Content-Type: application/json" \
  -d '{"applicationId":1}'

# Create user assigned to Accounting
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{"username":"bob","password":"pass123"}'

# Assign bob to Accounting (applicationId 2)
curl -X POST http://localhost:5000/api/users/2/applications \
  -H "Content-Type: application/json" \
  -d '{"applicationId":2}'
```

### Test Case 1 — HR user logs into HR ✅

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","password":"pass123","application":"HR"}'
# Expected: 200 OK with JWT token
```

### Test Case 2 — Accounting user logs into Accounting ✅

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"bob","password":"pass123","application":"Accounting"}'
# Expected: 200 OK with JWT token
```

### Test Case 3 — HR user tries Accounting (negative) ❌

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","password":"pass123","application":"Accounting"}'
# Expected: 401 Unauthorized — "Access denied to 'Accounting'."
```

### Test Case 4 — Deactivated user cannot log in ❌

```bash
curl -X PATCH http://localhost:5000/api/users/1/deactivate
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","password":"pass123","application":"HR"}'
# Expected: 401 Unauthorized — "Invalid credentials or account is inactive."
```

---

## Security Notes

- Passwords are hashed using **BCrypt** (cost factor 11) — never stored in plaintext
- Login error messages are intentionally generic to prevent username enumeration
- JWT tokens expire after **8 hours**
- CORS is restricted to `localhost:5173` and `localhost:3000` in development
- Replace the JWT key in `appsettings.json` with a cryptographically strong secret before any deployment
