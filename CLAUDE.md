# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Purpose

This repository demonstrates 5 common Clean Architecture implementation mistakes that hurt .NET performance, with practical fixes and benchmarks. Each mistake has "Bad" and "Good" implementations to showcase the performance differences.

## Essential Commands

### Building & Testing
```bash
# Build entire solution
dotnet build

# Build in release mode (required for benchmarks)
dotnet build --configuration Release

# Run fast unit tests (~2ms each)
dotnet test tests/Unit

# Run slow integration tests (for comparison, ~847ms each)
dotnet test tests/BadExamples

# Run specific test category
dotnet test tests/Unit --filter "ArchitectureTests"

# Run single test
dotnet test tests/Unit --filter "MethodName"
```

### Performance Benchmarking
```bash
# Run full benchmarks (takes 10+ minutes)
dotnet run --project benchmarks --configuration Release

# Architecture validation
powershell -ExecutionPolicy Bypass -File tools/architecture-audit.ps1
```

## Architecture Overview

### Project Structure Philosophy
The repository uses a **comparison architecture** where each mistake is demonstrated with:
- `Bad/` - Implementation showcasing the problem
- `Good/` - Implementation demonstrating the fix

### The 5 Mistakes Demonstrated

1. **Mistake1-FolderIllusion** (src/): Interface placement - interfaces should be in domain layer where consumed, not infrastructure
2. **Mistake2-TestingTrap** (tests/): Unit vs integration testing - true unit tests vs database-dependent tests
3. **Mistake3-TooManyLayers** (src/): Excessive mapping - 4-layer mapping vs direct projection
4. **Mistake4-CargoCult** (src/): Over-engineering - 47 interfaces vs pragmatic design
5. **Mistake5-InterfaceOverload** (src/): Interface abuse - unnecessary abstractions vs right-sized interfaces

### Key Implementation Details

**Domain Model Design** (`src/Mistake1-FolderIllusion/Good/`):
- Rich domain models with behavior (not anemic)
- Value objects with validation (UserId, EmailAddress)
- Domain events for side effects
- Repository interfaces in domain layer (consumed), not infrastructure

**Benchmarking Setup** (`benchmarks/`):
- Uses BenchmarkDotNet with .NET 9 runtime
- Configured for quick iteration (3 iterations, 1 warmup)
- Memory diagnostics enabled
- Both implementations use in-memory databases for consistent testing

**Testing Strategy**:
- **Unit tests** (`tests/Unit/`): Fast, no dependencies, ~2ms execution
- **Bad examples** (`tests/BadExamples/`): Slow, database-dependent, ~847ms execution
- Architecture tests using NetArchTest.Rules to enforce clean architecture principles

### Technology Stack (.NET 9)
- **Framework**: .NET 9 with C# 13, nullable reference types enabled
- **Testing**: xUnit 2.9.3, FluentAssertions 8.6.0, NetArchTest.Rules 1.3.2
- **Benchmarking**: BenchmarkDotNet 0.15.2
- **ORM**: Entity Framework Core 9.0.9 (in-memory for demonstrations)
- **Build**: TreatWarningsAsErrors enabled, .NET analyzers active

### Important Configuration Notes

**Project Settings** (`Directory.Build.props`):
- All projects target .NET 9.0 with C# 13
- Nullable reference types enabled globally
- XML documentation generation disabled for "Bad" example projects (intentionally)
- Code analysis with latest Microsoft analyzers

**Performance Expectations**:
- Benchmarks use in-memory databases, so performance characteristics differ from real SQL
- "Bad" examples might perform better in memory due to optimized single-record lookups
- Real-world benefits appear with actual database I/O and complex queries

### Working with Examples

**When modifying "Bad" examples**:
- These intentionally demonstrate anti-patterns
- Don't "fix" the problems - they're educational
- XML documentation is disabled to avoid noise

**When modifying "Good" examples**:
- Follow clean architecture principles
- Maintain performance optimizations
- Keep domain models rich with behavior

**When adding new examples**:
- Follow the Bad/Good folder structure
- Include corresponding tests and benchmarks
- Update the main README.md with performance claims
- Add architecture tests to validate clean architecture compliance

### Benchmark Results Interpretation

The benchmarks currently show reverse performance (Good being slower) because:
- In-memory databases favor simple lookups over projections
- Real SQL databases would show the expected performance benefits
- Network I/O and data transfer optimizations don't apply in-memory

This is documented behavior and demonstrates why benchmark environments must match production scenarios.
# important-instruction-reminders
Do what has been asked; nothing more, nothing less.
NEVER create files unless they're absolutely necessary for achieving your goal.
ALWAYS prefer editing an existing file to creating a new one.
NEVER proactively create documentation files (*.md) or README files. Only create documentation files if explicitly requested by the User.