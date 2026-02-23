# Developer Onboarding Guide

> **Goal**: Clone → running application in under 30 minutes.

---

## 1. Clone the Repository

```bash
git clone <repository-url>
cd tech_exercise/package/exercise1
```

---

## 2. Install Prerequisites

| Tool | Command | Verify |
|---|---|---|
| **.NET 8 SDK** | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) | `dotnet --version` → `8.x.x` |
| **Node.js 20+** | [Download](https://nodejs.org/) | `node --version` → `v20.x.x` |
| **Angular CLI** | `npm install -g @angular/cli` | `ng version` → `17.x.x` |
| **Docker Desktop** | [Download](https://www.docker.com/products/docker-desktop/) | `docker --version` |
| **EF Core Tools** | `dotnet tool install --global dotnet-ef` | `dotnet ef --version` |

> [!IMPORTANT]
> The `dotnet-ef` global tool is required for generating and managing database migrations. It is **not** bundled with the project.

---

## 3. Generate the Database

```bash
cd src/api
dotnet ef database update
```

**Expected output**:

```
Build started...
Build succeeded.
Applying migration '20240122154939_InitialCreate'.
Applying migration '20260222205644_AddPersonNameUniqueIndex'.
Applying migration '20260222212028_AddRequestLog'.
Done.
```

This creates `starbase.db` in `src/api/`.

---

## 4. Run the API

```bash
cd src/api
dotnet run
```

**Expected output**:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5204
```

**Verify**: Open [http://localhost:5204/swagger](http://localhost:5204/swagger) — Swagger UI loads with all endpoints.

---

## 5. Run the UI

In a **separate terminal**:

```bash
cd src/ui
npm install
ng serve
```

**Expected output**:

```
** Angular Live Development Server is listening on localhost:4200 **
```

**Verify**: Open [http://localhost:4200](http://localhost:4200) — Angular app loads with the People list.

> [!NOTE]
> The Angular dev server proxies `/api/*` requests to `http://localhost:5204` via `proxy.conf.json`. Both the API and UI must be running simultaneously.

---

## 6. Run Tests

```bash
cd tests/StargateAPI.Tests
dotnet test --verbosity minimal
```

**Expected output**:

```
Passed!  - Failed: 0, Passed: XX, Skipped: 0, Total: XX
```

For a coverage report:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 7. Docker Quick Start

If you just want to run everything with one command:

```bash
cd package/exercise1
docker-compose up --build
```

| Service | URL |
|---|---|
| API | [http://localhost:5204](http://localhost:5204) |
| Swagger | [http://localhost:5204/swagger](http://localhost:5204/swagger) |
| UI | [http://localhost:4200](http://localhost:4200) |
| Health | [http://localhost:5204/health](http://localhost:5204/health) |

To tear down:

```bash
docker-compose down -v
```

---

## 8. Key Architectural Concepts

Read [ARCHITECTURE.md](../ARCHITECTURE.md) for the full picture. Key takeaways:

| Concept | Summary |
|---|---|
| **CQRS** | Commands (writes) and Queries (reads) are separate classes dispatched via MediatR |
| **EF Core for writes, Dapper for reads** | Explicit dual-ORM convention |
| **FluentValidation pipeline** | Input validation runs automatically in the MediatR pipeline before handlers |
| **Global exception middleware** | Centralized error handling — controllers don't need try-catch |
| **Serilog** | Structured logging to console; request logs stored in the `RequestLog` DB table |
| **7 Business Rules** | See [SPEC.md §3](../SPEC.md) — enforced in `CreateAstronautDutyHandler` and `CreateAstronautDutyPreProcessor` |

---

## Common Issues

| Problem | Solution |
|---|---|
| `dotnet ef` not found | Run `dotnet tool install --global dotnet-ef` |
| Port 5204 in use | Stop other .NET apps or change port in `launchSettings.json` |
| Angular proxy errors | Ensure the API is running before starting `ng serve` |
| Docker build fails | Ensure Docker Desktop is running and has sufficient resources |
| SQLite "database locked" | Stop all other processes accessing `starbase.db` |
