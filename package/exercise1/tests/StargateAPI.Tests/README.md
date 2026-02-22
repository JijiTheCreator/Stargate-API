# Stargate API — Unit Tests

> **Status**: Pending — implement during Phase 5 (CHECKLIST.md)

This directory will contain the xUnit test project for the Stargate API.

## Scaffold Command

```bash
dotnet new xunit -n StargateAPI.Tests
dotnet add reference ../../src/api/StargateAPI.csproj
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

## Governed by

- [TESTING.md](../../agents/TESTING.md) — Agent blueprint
- [CHECKLIST.md](../../CHECKLIST.md) — Phase 5
