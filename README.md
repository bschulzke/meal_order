# meal_order

A monorepo web app with a .NET 10 / ASP.NET Core API, Vue 3 + TypeScript frontend, and Microsoft SQL Server database — all orchestrated with Docker Compose.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (includes Docker Compose)
- That's it for running the app. For local development outside Docker:
  - [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
  - [Node.js 20+](https://nodejs.org/)

## Getting started

### 1. Clone and configure environment

```bash
git clone <repo-url>
cd meal_order
cp .env.example .env
```

Open `.env` and set a strong SQL Server password (8+ chars, mixed case, digit, symbol):

```
DB_SA_PASSWORD=YourStrong@Passw0rd
```

### 2. Configure local API credentials (User Secrets)

The connection string is kept out of source control using [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets). Run this once after cloning, replacing the password with the value you set in `.env`:

```bash
cd backend/MealOrder.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=MealOrder;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
cd ../..
```

ASP.NET Core loads these automatically when running locally in Development mode. The Docker `api` service uses its own environment variable and is unaffected.

### 3. Start all services

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| Frontend (Vue SPA) | http://localhost |
| API (Swagger UI) | http://localhost/api/swagger |
| SQL Server | `localhost,1433` |

## Project structure

```
meal_order/
├── docker-compose.yml          # Orchestrates db, api, and frontend services
├── .env.example                # Template — copy to .env and fill in secrets
├── backend/
│   ├── Dockerfile
│   ├── MealOrder.sln
│   └── MealOrder.Api/
│       ├── Controllers/        # Add API controllers here
│       ├── Data/
│       │   └── AppDbContext.cs # Add DbSet<T> properties here for new models
│       ├── Program.cs
│       └── appsettings.json
└── frontend/
    ├── Dockerfile
    ├── nginx.conf              # Reverse-proxies /api/ to the API container
    └── src/
        ├── main.ts
        └── App.vue
```

---

## Development workflow

### Backend

The API project is at `backend/MealOrder.Api/`. It uses Entity Framework Core (code-first) with SQL Server.

**Adding a model and migration:**

```bash
cd backend/MealOrder.Api

# 1. Add a new model class and DbSet in Data/AppDbContext.cs
# 2. Create the migration
dotnet ef migrations add <MigrationName>

# 3. Apply it to your local database (or let the app apply it on next startup)
dotnet ef database update
```

**Running the API locally (without Docker):**

Start just the database in Docker, then run the API natively for hot-reload with `dotnet watch`:

```bash
# Terminal 1 — database only
docker compose up db

# Terminal 2 — API with hot-reload
cd backend/MealOrder.Api
dotnet watch run
```

### Frontend

```bash
# Terminal 3 — frontend with HMR
cd frontend
npm install   # first time only
npm run dev
```

The Vite dev server runs on http://localhost:5173 and proxies `/api/` requests to `http://localhost:5000`. Changes to `.vue` and `.ts` files reflect in the browser instantly without a full page reload.

---

## Connecting to SQL Server

Use any SQL client (SSMS, Azure Data Studio, VS Code SQL Server extension):

| Setting | Value |
|---|---|
| Server | `localhost,1433` |
| Authentication | SQL Login |
| Username | `sa` |
| Password | value from your `.env` |
| Trust server certificate | Yes |
