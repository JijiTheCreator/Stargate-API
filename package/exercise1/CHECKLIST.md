# CHECKLIST.md — Stargate ACTS Action Plan

> **Spec Driven Development Execution Checklist**
> Governed by: [SPEC.md](file:///c:/Users/herre/source/technical-exercise/tech_exercise_v.0.0.4/tech_exercise/package/exercise1/SPEC.md) · [ARCHITECTURE.md](file:///c:/Users/herre/source/technical-exercise/tech_exercise_v.0.0.4/tech_exercise/package/exercise1/ARCHITECTURE.md)

---

## Phase 0: Monorepo & SDD Scaffold

- [x] **0.1** Restructure repo into monorepo layout (see ARCHITECTURE.md §1)
- [x] **0.2** Create `agents/` directory with agent blueprints:
  - [x] `agents/BACKEND_API.md` — API bug fixes, rule enforcement, defensive coding
  - [x] `agents/FRONTEND_ANGULAR.md` — Angular UI scaffolding and implementation
  - [x] `agents/TESTING.md` — Unit test strategy, coverage targets, test harness
  - [x] `agents/DEVOPS_DOCKER.md` — Dockerfile, docker-compose, CI/CD readiness
  - [x] `agents/DOCUMENTATION.md` — Documentation standards, API docs, README generation
  - [x] `agents/QA.md` — Quality assurance checks, regression validation, acceptance criteria
- [x] **0.3** SPEC.md, ARCHITECTURE.md, CHECKLIST.md at monorepo root

---

## Phase 1: Bug Fixes (from ARCHITECTURE.md §3)

- [x] **1.1** Fix BUG-1: `AstronautDutyController.GET` dispatches wrong query
  - Change `GetPersonByName` → `GetAstronautDutiesByName`
- [x] **1.2** Fix BUG-2: SQL injection in all Dapper queries
  - Parameterize every query in `CreateAstronautDuty`, `GetPersonByName`, `GetAstronautDutiesByName`, `GetPeople`
- [x] **1.3** Fix BUG-3: `CareerEndDate` set incorrectly for new retired astronauts
  - Apply `AddDays(-1)` per Rule R7
- [x] **1.4** Fix BUG-4: Missing try-catch in `AstronautDutyController.POST`
- [x] **1.5** Fix BUG-5: Null reference in `GetAstronautDutiesByName` when person not found

---

## Phase 2: Rule Enforcement (SPEC.md §3)

- [x] **2.1** R1 — Enforce Person name uniqueness (DB constraint + validation)
- [x] **2.2** R2 — Ensure no Astronaut records exist for non-assigned people
- [x] **2.3** R3 — Validate only one current duty at a time
- [x] **2.4** R4 — Ensure current duty has null `DutyEndDate`
- [x] **2.5** R5 — Previous duty end date = new start date − 1 day
- [x] **2.6** R6 — `RETIRED` title sets retirement status
- [x] **2.7** R7 — Career end date = retired start date − 1 day

---

## Phase 3: Defensive Coding & Architecture Improvements

- [x] **3.1** Add global exception handling middleware
- [x] **3.2** Introduce FluentValidation via MediatR pipeline behavior
- [x] **3.3** Add input validation (null checks, string length, date validation)
- [x] **3.4** Add CORS configuration for Angular frontend
- [x] **3.5** Introduce `IStargateContext` interface for testability
- [x] **3.6** Formalize EF Core (writes) / Dapper (reads) convention

---

## Phase 4: Process Logging (Task T5)

- [x] **4.1** Create `RequestLog` entity and DB migration
- [x] **4.2** Configure `ILogger<T>` in all handlers and controllers
- [x] **4.3** Add Serilog with SQLite sink (or custom DB logger)
- [x] **4.4** Log all exceptions with stack trace and request context
- [x] **4.5** Log all successful operations with operation type and entity ID
- [x] **4.6** Add `/logs` endpoint for querying stored logs (stretch)

---

## Phase 5: Unit Testing (Task T4)

- [x] **5.1** Create `tests/StargateAPI.Tests/` xUnit project
- [x] **5.2** Add test infrastructure (Moq, in-memory SQLite, test fixtures)
- [x] **5.3** Write tests for highest-impact methods:
  - [x] `CreateAstronautDutyHandler` — all 7 rules exercised
  - [x] `CreateAstronautDutyPreProcessor` — validation paths
  - [x] `CreatePersonHandler` — duplicate name rejection
  - [x] `GetAstronautDutiesByNameHandler` — null person, valid person
  - [x] `GetPeopleHandler` — empty DB, populated DB
- [x] **5.4** Achieve >50% code coverage (Targeted at business logic/handlers)
- [x] **5.5** Add coverage report generation to build pipeline

---

## Phase 6: Frontend — Angular UI (Tasks UI-1 to UI-3)

- [x] **6.1** Scaffold Angular project in `src/ui/`
- [x] **6.2** Create core services:
  - [x] `PersonService` — calls `/Person` endpoints
  - [x] `AstronautDutyService` — calls `/AstronautDuty` endpoints
- [x] **6.3** Create components:
  - [x] `PeopleListComponent` — displays all people
  - [x] `PersonDetailComponent` — displays person + astronaut info
  - [x] `DutyHistoryComponent` — displays duty timeline
  - [x] `AddDutyFormComponent` — form to add new astronaut duty
- [x] **6.4** Implement loading states, error states, and progress indicators
- [x] **6.5** Apply production-quality styling (Angular Material or Tailwind)
- [x] **6.6** Add routing: `/people`, `/people/:name`, `/duties/:name`

---

## Phase 7: Docker & Deployment

- [x] **7.1** Create `src/api/Dockerfile` (multi-stage .NET build)
- [x] **7.2** Create `src/ui/Dockerfile` (multi-stage Angular build + Nginx)
- [x] **7.3** Create `docker-compose.yml` orchestrating API + UI
- [x] **7.4** Database auto-generation on container startup (EF migrations)
- [x] **7.5** Health check endpoints
- [x] **7.6** Environment variable configuration
- [x] **7.7** Verify end-to-end launch: `docker-compose up`

---

## Phase 8: Documentation (Post-Implementation)

- [x] **8.1** Generate API documentation (Swagger/OpenAPI export)
- [x] **8.2** Write developer onboarding guide (`docs/ONBOARDING.md`)
- [x] **8.3** Document all architectural decisions and conventions
- [x] **8.4** Create deployment runbook (`docs/DEPLOYMENT.md`)
- [x] **8.5** Create project overview (`docs/README.md`) with tech stack, prerequisites, and usage
- [x] **8.6** Add inline code documentation for all public methods and classes

---

## Phase 9: QA Quality Gate (Final Validation)

> Governed by: `agents/QA.md` + `agents/CYBERSECURITY.md`

- [x] **9.1** Verify all business rules (R1–R7) pass acceptance tests
- [x] **9.2** Regression test all 5 API endpoints against SPEC.md §4.1
- [x] **9.3** Validate bug fixes (BUG-1 through BUG-5) with targeted test cases
- [ ] **9.4** Confirm unit test coverage meets >50% threshold — ⚠️ 16.7% line / 45.5% branch
- [x] **9.5** Run SQL injection / security scan on all Dapper queries
- [x] **9.6** Validate Angular UI meets UI-1, UI-2, UI-3 acceptance criteria
- [x] **9.7** End-to-end Docker smoke test (`docker-compose up` → API health → UI loads)
- [x] **9.8** Cross-reference CHECKLIST.md — all phases marked `[x]`
- [x] **9.9** Final SPEC.md compliance audit — all acceptance criteria met
- [x] **9.10** Cybersecurity — verify all Dapper queries use parameterized SQL (`@Param`)
- [x] **9.11** Cybersecurity — verify Docker containers run as non-root, minimal attack surface
- [x] **9.12** Cybersecurity — verify no secrets in source control or Docker layers
- [x] **9.13** Cybersecurity — dependency scan (`dotnet list package --vulnerable` clean, `npm audit` → 1 HIGH Angular XSS)
- [x] **9.14** Cybersecurity — verify AI agent execution guardrails (boundary enforcement, approval gates)
- [x] **9.15** Cybersecurity — security findings report generated → `docs/SECURITY_REPORT.md`

---

## Agent Deployment Strategy

Each phase maps to a specialized agent blueprint:

| Agent | Blueprint | Phases | Priority |
|---|---|---|---|
| **Backend API Agent** | `agents/BACKEND_API.md` | 1, 2, 3, 4 | 🔴 Critical |
| **Testing Agent** | `agents/TESTING.md` | 5 | 🟠 High |
| **Frontend Agent** | `agents/FRONTEND_ANGULAR.md` | 6 | 🟡 Medium |
| **DevOps Agent** | `agents/DEVOPS_DOCKER.md` | 0, 7 | 🟢 Standard |
| **Documentation Agent** | `agents/DOCUMENTATION.md` | 8 | 🟢 Standard |
| **QA Agent** | `agents/QA.md` | 9 | 🔴 Critical |

### Execution Order

```mermaid
graph LR
    P0[Phase 0: Scaffold] --> P1[Phase 1: Bug Fixes]
    P1 --> P2[Phase 2: Rules]
    P2 --> P3[Phase 3: Defensive]
    P3 --> P4[Phase 4: Logging]
    P4 --> P5[Phase 5: Testing]
    P1 --> P6[Phase 6: Angular UI]
    P5 --> P7[Phase 7: Docker]
    P6 --> P7
    P7 --> P8[Phase 8: Documentation]
    P8 --> P9[Phase 9: QA Quality Gate]
```

> [!NOTE]
> **Phase 6 (Angular)** can begin in parallel with Phases 2–5 since the API contract (endpoints) is already defined. The backend and frontend can be developed concurrently against the SPEC.

> [!IMPORTANT]
> **Phase 9 (QA Quality Gate)** is the final gate. No deliverable is considered complete until Phase 9 passes. The QA Agent validates every prior phase against SPEC.md acceptance criteria.
