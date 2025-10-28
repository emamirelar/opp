# Backend Quick Start Guide

**Project**: UNOPS Opportunity Plus Backend  
**Purpose**: Quick reference for backend architecture and testing

---

## 📁 Quick Links

- **[Full Backend Analysis](./BACKEND_CODEBASE_ANALYSIS.md)** - Complete codebase analysis with detailed recommendations
- **[Backend Testing Guide](./BACKEND_TESTING_GUIDE.md)** - Comprehensive testing strategies and examples
- **[Frontend Analysis](./UNOPS.PAO.ClientApp/ANGULAR_CODEBASE_ANALYSIS.md)** - Angular frontend analysis
- **[Frontend Testing](./UNOPS.PAO.ClientApp/ANGULAR_TESTING_FRAMEWORKS_GUIDE.md)** - Angular testing guide

---

## 🚨 Critical Issues Summary

| Issue | Severity | Impact | Quick Fix Time |
|-------|----------|--------|----------------|
| **Missing Unit Tests** | 🔴 CRITICAL | High | 60 min setup |
| **Duplicate Architecture Layers** | 🔴 High | Medium | 1-2 weeks |
| **262 Migration Files** | 🔴 High | Medium | 4-8 hours |
| **Temporary Files in Root** | 🟡 Medium | Low | 15 minutes |
| **Models Project Disorganized** | 🟡 Medium | Medium | 2-4 hours |
| **Controllers Unorganized** | 🟢 Low | Low | 2-4 hours |

---

## ⚡ Quick Wins (Do These First!)

### 1. Clean Up Root Directory (15 min)

```bash
# Create organized folders
mkdir -p tools/Deprecated
mkdir -p database/Scripts/Temp

# Move temporary files
mv test_*.py tools/Deprecated/
mv update_liaison_office_ids.py tools/Deprecated/
mv generate_pubsub_embedding_messages.sql database/Scripts/Temp/
rm package-lock.json
```

### 2. Create Unit Test Projects (60 min)

```bash
# Create test projects
dotnet new xunit -n UNOPS.PAO.Business.Tests -o tests/Unit/UNOPS.PAO.Business.Tests
dotnet new xunit -n UNOPS.PAO.Domain.Tests -o tests/Unit/UNOPS.PAO.Domain.Tests
dotnet new xunit -n UNOPS.PAO.Presentation.Tests -o tests/Unit/UNOPS.PAO.Presentation.Tests

# Add to solution
dotnet sln add tests/Unit/UNOPS.PAO.Business.Tests
dotnet sln add tests/Unit/UNOPS.PAO.Domain.Tests
dotnet sln add tests/Unit/UNOPS.PAO.Presentation.Tests

# Install packages
cd tests/Unit/UNOPS.PAO.Business.Tests
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package AutoFixture
dotnet add package AutoFixture.AutoMoq
dotnet add package coverlet.collector

# Add project references
dotnet add reference ../../../UNOPS.PAO.Business/UNOPS.PAO.Business.csproj
dotnet add reference ../../../UNOPS.PAO.Domain/UNOPS.PAO.Domain.csproj
```

### 3. Write First Tests (30 min)

See examples in [Backend Testing Guide](./BACKEND_TESTING_GUIDE.md#real-world-examples)

### 4. Set Up Coverage Reporting (15 min)

```bash
# Install report generator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"

# View report
start coveragereport/index.html  # Windows
open coveragereport/index.html   # macOS
```

---

## 📊 Current vs Target State

### Current State
```
✅ Clean Architecture layers present
❌ Duplicate PAO/UNOPS layers confusing
❌ 262 migration files (bloat)
❌ 12+ temporary files in root
❌ No unit test projects
❌ ~20% test coverage (integration only)
❌ 107 model files in flat structure
❌ 35 controllers in flat structure
```

### Target State
```
✅ Clean Architecture with clear boundaries
✅ Consolidated layers (no duplicates)
✅ < 10 migration files (squashed)
✅ 0 temporary files in root
✅ 4 unit test projects + 1 integration
✅ 75%+ test coverage (unit + integration)
✅ Models organized in 15 folders by feature
✅ Controllers organized in 10 folders by feature
```

---

## 🏗️ Recommended Architecture

```
src/
├── Core/
│   ├── UNOPS.PAO.Domain           # Entities, Specifications, Enums
│   └── UNOPS.PAO.Domain.Tests     # ⬅️ Unit tests
│
├── Application/
│   ├── UNOPS.PAO.Business         # Managers, Services, Logic
│   └── UNOPS.PAO.Business.Tests   # ⬅️ Unit tests
│
├── Infrastructure/
│   ├── UNOPS.PAO.DataAccess       # EF Core, DbContext
│   ├── UNOPS.PAO.Identity         # Auth/Identity
│   ├── UNOPS.PAO.GoogleServices   # Google APIs
│   └── UNOPS.PAO.MailSender       # Email
│
├── Presentation/
│   ├── UNOPS.PAO.Presentation     # Controllers
│   └── UNOPS.PAO.Presentation.Tests # ⬅️ Unit tests
│
├── Models/
│   └── UNOPS.PAO.Models           # DTOs, organized by feature
│
└── WebHost/
    └── UNOPS.PAO.Server           # Entry point

tests/
├── Unit/                          # ⬅️ Fast tests (70%)
│   ├── UNOPS.PAO.Domain.Tests
│   ├── UNOPS.PAO.Business.Tests
│   └── UNOPS.PAO.Presentation.Tests
│
└── Integration/                   # ⬅️ Slow tests (30%)
    └── UNOPS.PAO.IntegrationTests
```

---

## 🧪 Testing Quick Reference

### Test Distribution

```
Testing Pyramid:
       ╱╲        E2E/Integration (5-10%)
      ╱  ╲       API + Database integration
     ╱────╲      
    ╱      ╲     Integration Tests (20-25%)
   ╱────────╲    WebApplicationFactory
  ╱          ╲   
 ╱────────────╲  Unit Tests (70%)
╱______________╲ xUnit + Moq + FluentAssertions
```

### When to Use Which

| Test Type | Purpose | Speed | When to Run |
|-----------|---------|-------|-------------|
| **Unit** | Test individual components | ⚡ Milliseconds | Every change |
| **Integration** | Test API + DB together | 🐢 Seconds | Before commit |
| **E2E** | Test complete workflows | 🐌 Minutes | Before deploy |

### Common Commands

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific project
dotnet test UNOPS.PAO.Business.Tests

# Run in watch mode
dotnet watch test

# Run only unit tests (exclude integration)
dotnet test --filter "FullyQualifiedName!~IntegrationTests"
```

### Coverage Requirements

| Layer | Minimum | Target |
|-------|---------|--------|
| Domain | 80% | 90%+ |
| Business | 80% | 85%+ |
| Controllers | 70% | 80%+ |
| **Overall** | **75%** | **80%+** |

---

## 📁 File Organization Guidelines

### Models Project Organization

**Current**: 107 files in flat structure ❌

**Target**: Organized by feature ✅

```
UNOPS.PAO.Models/
├── Partners/            # Partner-related models
├── Contacts/            # Contact-related models
├── Interactions/        # Interaction-related models
├── Documents/           # Document-related models
├── OrganizationUnits/   # Org unit models
├── AI/                  # AI/Gemini models
├── Admin/               # Admin models
├── User/                # User models
├── Search/              # Search models
├── Shared/              # Common models
└── Workflow/            # Workflow models
```

### Controllers Organization

**Current**: 35 controllers in flat structure ❌

**Target**: Organized by feature ✅

```
UNOPS.PAO.Presentation/Controllers/
├── Partners/            # Partner controllers
├── Contacts/            # Contact controllers
├── Interactions/        # Interaction controllers
├── Documents/           # Document controllers
├── OrganizationUnits/   # Org unit controllers
├── Admin/               # Admin controllers
├── AI/                  # AI controllers
├── User/                # User controllers
└── Shared/              # Base/common controllers
```

---

## 🔧 Common Tasks

### Create a New Feature

```bash
# 1. Add entity to Domain
# UNOPS.PAO.Domain/Entities/{FeatureName}.cs

# 2. Add models
# UNOPS.PAO.Models/{FeatureName}/{FeatureName}Model.cs

# 3. Add manager
# UNOPS.PAO.Business/Managers/{FeatureName}Manager.cs

# 4. Add controller
# UNOPS.PAO.Presentation/Controllers/{FeatureName}Controller.cs

# 5. Add unit tests
# tests/Unit/UNOPS.PAO.Business.Tests/Managers/{FeatureName}ManagerTests.cs
# tests/Unit/UNOPS.PAO.Presentation.Tests/Controllers/{FeatureName}ControllerTests.cs

# 6. Add integration tests
# UNOPS.PAO.IntegrationTests/Controllers/{FeatureName}ControllerTests.cs
```

### Add a Migration

```bash
# Add migration
dotnet ef migrations add {MigrationName} \
    --project UNOPS.PAO.DataAccess \
    --startup-project UNOPS.PAO.Server

# Update database
dotnet ef database update \
    --project UNOPS.PAO.DataAccess \
    --startup-project UNOPS.PAO.Server

# Revert migration
dotnet ef migrations remove \
    --project UNOPS.PAO.DataAccess \
    --startup-project UNOPS.PAO.Server
```

### Run the Application

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
cd UNOPS.PAO.Server
dotnet run

# Or use watch mode (auto-rebuild)
dotnet watch run
```

---

## 🎯 Team Guidelines

### Code Review Checklist

- [ ] **Tests included**: Unit tests for business logic
- [ ] **Coverage maintained**: No decrease in coverage percentage
- [ ] **Architecture followed**: Code in correct project/folder
- [ ] **Naming consistent**: Follows conventions
- [ ] **No warnings**: Build is clean
- [ ] **Migration included**: If database schema changed
- [ ] **Documentation updated**: If architecture changed

### Testing Requirements

**Before committing**:
```bash
# 1. Run unit tests (should be < 10 seconds)
dotnet test --filter "FullyQualifiedName!~IntegrationTests"

# 2. Check coverage (should be ≥ 75%)
dotnet test /p:CollectCoverage=true

# 3. Run integration tests (before push)
dotnet test UNOPS.PAO.IntegrationTests

# 4. Fix any failing tests
```

**New code requirements**:
- ✅ Every manager method needs unit tests
- ✅ Every controller action needs unit tests
- ✅ New API endpoints need integration tests
- ✅ Aim for 80%+ coverage on new code
- ✅ All tests must pass before PR

---

## 📚 Additional Resources

### Documentation
- **[Backend Codebase Analysis](./BACKEND_CODEBASE_ANALYSIS.md)** - Full analysis with migration plan
- **[Backend Testing Guide](./BACKEND_TESTING_GUIDE.md)** - Complete testing guide with examples
- **[Frontend Analysis](./UNOPS.PAO.ClientApp/ANGULAR_CODEBASE_ANALYSIS.md)** - Angular codebase analysis
- **[Frontend Testing](./UNOPS.PAO.ClientApp/ANGULAR_TESTING_FRAMEWORKS_GUIDE.md)** - Angular testing guide

### External Resources
- [Clean Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/)
- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

## 🆘 Troubleshooting

### Tests Not Running

```bash
# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build

# Verify test projects
dotnet test --list-tests
```

### Coverage Not Generating

```bash
# Ensure coverlet is installed
dotnet add package coverlet.collector
dotnet add package coverlet.msbuild

# Run with explicit parameters
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./coverage/
```

### Integration Tests Failing

```bash
# Check database connection
# Verify appsettings.Testing.json exists

# Clear test database
# Integration tests should use in-memory database or separate test DB

# Check for port conflicts
# Ensure no other instances running
```

---

## 📞 Support

For architecture questions:
1. Check this guide
2. Review full documentation (links above)
3. Discuss with team lead
4. Document decisions in ADRs

For testing questions:
1. Check testing guide examples
2. Review existing tests
3. Ask in team chat
4. Pair program with experienced team member

---

**Document Version**: 1.0  
**Last Updated**: January 15, 2025  
**Status**: Quick Reference Guide

