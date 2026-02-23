# Stargate ACTS — Astronaut Career Tracking System

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Angular 17](https://img.shields.io/badge/Angular-17-DD0031?logo=angular)
![SQLite](https://img.shields.io/badge/SQLite-3-003B57?logo=sqlite)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)

> **Version**: `v1.0.0` · Governed by [SPEC.md](../SPEC.md) · [ARCHITECTURE.md](../ARCHITECTURE.md) · [CHECKLIST.md](../CHECKLIST.md)

---

## Overview

ACTS maintains a record of all People who have served as Astronauts. Each Astronaut's career is tracked by Rank, Duty Title, and Start/End Dates. The system enforces 7 business rules governing duty assignments, retirement, and career lifecycle.

The application consists of a **.NET 8 REST API** backed by **SQLite**, an **Angular 17** frontend, and full **Docker Compose** orchestration for single-command deployment.

---

## Tech Stack

| Layer | Technology | Version |
|---|---|---|
| **API** | ASP.NET Core | 8.0 |
| **ORM (Writes)** | Entity Framework Core | 8.0.4 |
| **ORM (Reads)** | Dapper | 2.1.35 |
| **Database** | SQLite | 3.x |
| **Mediator / CQRS** | MediatR | 12.2.0 |
| **Validation** | FluentValidation | 11.9.0 |
| **Logging** | Serilog | 8.0.1 |
| **API Docs** | Swashbuckle (Swagger) | 6.5.0 |
| **Frontend** | Angular + Angular Material | 17.x |
| **Containerization** | Docker + Docker Compose | — |

---

## Prerequisites

| Software | Version | Install |
|---|---|---|
| .NET SDK | 8.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Node.js | 20+ | [nodejs.org](https://nodejs.org/) |
| Angular CLI | 17+ | `npm install -g @angular/cli` |
| Docker Desktop | Latest | [docker.com](https://www.docker.com/products/docker-desktop/) |
| `dotnet-ef` | Latest | `dotnet tool install --global dotnet-ef` |

---

## Quick Start (Docker)

```bash
cd package/exercise1
docker-compose up --build
```

| Service | URL |
|---|---|
| **API** | [http://localhost:5204](http://localhost:5204) |
| **Health Check** | [http://localhost:5204/health](http://localhost:5204/health) |
| **Swagger UI** | [http://localhost:5204/swagger](http://localhost:5204/swagger) |
| **Angular UI** | [http://localhost:4200](http://localhost:4200) |

To stop and clean up:

```bash
docker-compose down -v
```

---

## Development Setup

### API (manual)

```bash
cd src/api
dotnet restore
dotnet ef database update          # Generate SQLite database
dotnet run                         # Starts on http://localhost:5204
```

### UI (manual)

```bash
cd src/ui
npm install
ng serve                           # Starts on http://localhost:4200
```

> [!NOTE]
> The Angular dev server proxies `/api/*` requests to `http://localhost:5204` via `proxy.conf.json`.

### Tests

```bash
cd tests/StargateAPI.Tests
dotnet test --verbosity minimal
```

---

## API Reference

All endpoints are documented via Swagger at `/swagger` when the API is running.

| # | Endpoint | Method | Description |
|---|---|---|---|
| API-1 | `/Person` | `GET` | Retrieve all people |
| API-2 | `/Person/{name}` | `GET` | Retrieve a person by name (includes astronaut detail) |
| API-3 | `/Person` | `POST` | Create a new person |
| API-4 | `/AstronautDuty/{name}` | `GET` | Retrieve astronaut duty history by name |
| API-5 | `/AstronautDuty` | `POST` | Add a new astronaut duty assignment |
| — | `/Logs` | `GET` | Query stored process logs |
| — | `/health` | `GET` | Health check (returns `{ "status": "healthy" }`) |

---

## Project Structure

```
exercise1/
├── README.md                          # Original exercise requirements
├── SPEC.md                            # Business specification (SDD)
├── ARCHITECTURE.md                    # Architecture & design patterns
├── CHECKLIST.md                       # Implementation checklist
├── docker-compose.yml                 # Service orchestration
├── .env                               # Environment overrides (gitignored)
├── agents/                            # SDD Agent blueprints
│   ├── BACKEND_API.md
│   ├── FRONTEND_ANGULAR.md
│   ├── TESTING.md
│   ├── DEVOPS_DOCKER.md
│   ├── DOCUMENTATION.md
│   └── QA.md
├── src/
│   ├── api/                           # .NET 8 Web API
│   │   ├── Controllers/               # REST endpoints
│   │   ├── Business/                  # CQRS: Commands, Queries, Data
│   │   ├── Middleware/                # Global exception handler
│   │   └── Dockerfile                 # Multi-stage .NET build
│   └── ui/                            # Angular 17 Application
│       ├── src/app/                   # Components, services, models
│       ├── nginx.conf                 # SPA routing + API proxy
│       └── Dockerfile                 # Multi-stage Angular + Nginx
├── tests/
│   └── StargateAPI.Tests/             # xUnit test project
└── docs/
    ├── README.md                      # This document
    ├── ONBOARDING.md                  # Developer quickstart
    └── DEPLOYMENT.md                  # Deployment runbook
```

---

## Contributing

1. **Read** [SPEC.md](../SPEC.md) and [ARCHITECTURE.md](../ARCHITECTURE.md) to understand the domain and design patterns
2. **Pick a task** from [CHECKLIST.md](../CHECKLIST.md)
3. **Follow the agent blueprint** for the relevant phase (see `agents/`)
4. **Run tests** before committing: `dotnet test`
5. **Verify Docker build**: `docker-compose build`

### Conventions

- **EF Core** for write operations, **Dapper** for read operations
- **MediatR** CQRS pattern: all business logic lives in Commands/Queries, not Controllers
- **FluentValidation** via MediatR pipeline for input validation
- **XML doc comments** (`///`) on all public C# classes and methods
- **JSDoc** (`/** */`) on all exported TypeScript services and interfaces

---

## License

MIT — See project root for details.
