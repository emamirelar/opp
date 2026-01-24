# Test Database Configuration Guide
**Purpose**: Configure local PostgreSQL database for integration testing  
**Audience**: Developers, QA Engineers  
**Estimated Time**: 30-45 minutes

---

## 🎯 **OVERVIEW**

This guide helps you set up a local PostgreSQL database for running integration tests. Once configured, 40+ integration tests that currently fail due to database connectivity will pass.

---

## 📋 **PREREQUISITES**

- PostgreSQL 13+ installed locally
- .NET 9 SDK installed
- Access to the UNOPS Opportunity+ repository
- PowerShell or Bash terminal

---

## 🛠️ **STEP 1: CREATE TEST DATABASE**

### Option A: Using psql Command Line

```bash
# Connect to PostgreSQL as superuser
psql -U postgres

# Run the following SQL commands:
CREATE DATABASE unops_pao_test;
CREATE USER pao_test_user WITH ENCRYPTED PASSWORD 'Test_Pass_123!';
GRANT ALL PRIVILEGES ON DATABASE unops_pao_test TO pao_test_user;

# Connect to the new database
\c unops_pao_test;

# Set up schema
CREATE SCHEMA IF NOT EXISTS public;
GRANT ALL ON SCHEMA public TO pao_test_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO pao_test_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO pao_test_user;

# Exit psql
\q
```

### Option B: Using pgAdmin GUI

1. Open pgAdmin
2. Right-click "Databases" → "Create" → "Database"
   - Database name: `unops_pao_test`
   - Owner: postgres
   - Click "Save"
3. Right-click "Login/Group Roles" → "Create" → "Login/Group Role"
   - General tab: Name: `pao_test_user`
   - Definition tab: Password: `Test_Pass_123!`
   - Privileges tab: Check "Can login?"
   - Click "Save"
4. Right-click `unops_pao_test` database → "Properties"
   - Security tab → Add "pao_test_user" with ALL privileges

---

## 🔧 **STEP 2: CREATE TEST CONFIGURATION FILE**

Create `appsettings.Testing.json` in test projects:

### File: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/appsettings.Testing.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=unops_pao_test;Username=pao_test_user;Password=Test_Pass_123!;Include Error Detail=true",
    "DbSchema": "public"
  },
  "IsUNOPSOverride": true,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AISettings": {
    "DisableExternalCalls": true,
    "ModelName": "gemini-pro",
    "ProjectId": "test-project",
    "Location": "us-central1"
  },
  "GoogleCloud": {
    "ProjectId": "test-project",
    "PubSubTopic": "test-topic",
    "UseMockServices": true
  },
  "ExchangeRate": {
    "ApiKey": "test-key",
    "BaseUrl": "https://test-api.example.com"
  }
}
```

### File: `QA Tests/C# Tests/UNOPS.PAO.FastTests/appsettings.Testing.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=unops_pao_test;Username=pao_test_user;Password=Test_Pass_123!;Include Error Detail=true",
    "DbSchema": "public"
  },
  "IsUNOPSOverride": true,
  "AISettings": {
    "DisableExternalCalls": true
  },
  "GoogleCloud": {
    "UseMockServices": true
  }
}
```

### File: `QA Tests/Integration Tests/appsettings.Testing.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=unops_pao_test;Username=pao_test_user;Password=Test_Pass_123!;Include Error Detail=true",
    "DbSchema": "public"
  },
  "IsUNOPSOverride": true,
  "AISettings": {
    "DisableExternalCalls": true
  },
  "GoogleCloud": {
    "UseMockServices": true
  }
}
```

---

## 🗄️ **STEP 3: RUN DATABASE MIGRATIONS**

### PowerShell (Windows)

```powershell
# Navigate to workspace root
cd C:\Users\Leonardc\git\opportunityplus

# Set environment variable to use Testing configuration
$env:ASPNETCORE_ENVIRONMENT = "Testing"

# Apply migrations to test database
dotnet ef database update `
  --project "UNOPS.PAO.UNOPSDataAccess\UNOPS.PAO.UNOPSDataAccess.csproj" `
  --startup-project "UNOPS.PAO.Server\UNOPS.PAO.Server.csproj" `
  --context UNOPSAppDbContext

Write-Host "✅ Test database migrations applied successfully!" -ForegroundColor Green
```

### Bash (Linux/Mac)

```bash
# Navigate to workspace root
cd ~/git/opportunityplus

# Set environment variable to use Testing configuration
export ASPNETCORE_ENVIRONMENT=Testing

# Apply migrations to test database
dotnet ef database update \
  --project UNOPS.PAO.UNOPSDataAccess/UNOPS.PAO.UNOPSDataAccess.csproj \
  --startup-project UNOPS.PAO.Server/UNOPS.PAO.Server.csproj \
  --context UNOPSAppDbContext

echo "✅ Test database migrations applied successfully!"
```

---

## ✅ **STEP 4: VERIFY DATABASE SETUP**

### Check Database Contents

```sql
-- Connect to test database
psql -U pao_test_user -d unops_pao_test

-- List all tables
\dt public.*

-- You should see tables like:
-- Partners, Contacts, Interactions, Opportunities, PAOUsers, etc.

-- Check a sample table
SELECT COUNT(*) FROM public."Partners";

-- Exit
\q
```

### Run a Quick Test

```powershell
# Run Fast Tests (should complete quickly)
cd "QA Tests/C# Tests/UNOPS.PAO.FastTests"
dotnet test --logger "console;verbosity=detailed"

# Expected: All 78 tests should PASS
```

---

## 🧪 **STEP 5: RUN INTEGRATION TESTS**

```powershell
# Set environment
$env:ASPNETCORE_ENVIRONMENT = "Testing"

# Run Business Tests
cd "QA Tests/C# Tests/UNOPS.PAO.Business.Tests"
dotnet test --filter "Category!=RequiresEnvironment" `
  --logger "console;verbosity=normal"

# Run Integration Tests
cd "QA Tests/Integration Tests"
dotnet test --filter "Category!=RequiresEnvironment" `
  --logger "console;verbosity=normal"
```

---

## 🎭 **OPTIONAL: TEST DATA SEEDING**

For more realistic test scenarios, seed sample data:

### Create Seed Script: `seed-test-data.sql`

```sql
-- Connect to test database
\c unops_pao_test;

-- Seed Currencies
INSERT INTO public."Currencies" ("Id", "Code", "Name", "IsDeleted", "CreatedBy", "CreatedDate") 
VALUES 
  (1, 'USD', 'US Dollar', false, 1, NOW()),
  (2, 'EUR', 'Euro', false, 1, NOW()),
  (3, 'GBP', 'British Pound', false, 1, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Seed Countries
INSERT INTO public."Countries" ("Id", "Name", "Iso2Code", "Iso3Code") 
VALUES 
  (1, 'United States', 'US', 'USA'),
  (2, 'United Kingdom', 'GB', 'GBR'),
  (3, 'Denmark', 'DK', 'DNK'),
  (4, 'Bangladesh', 'BD', 'BGD')
ON CONFLICT ("Id") DO NOTHING;

-- Seed Organization Hierarchies
INSERT INTO public."OrganizationHierarchies" ("Id", "Name", "Code", "Description", "IsDeleted", "CreatedBy", "CreatedDate") 
VALUES 
  (1, 'HQ Copenhagen', 'HQ-CPH', 'Headquarters in Copenhagen', false, 1, NOW()),
  (2, 'Asia Pacific', 'APAC', 'Asia Pacific Regional Office', false, 1, NOW()),
  (3, 'Latin America', 'LATAM', 'Latin America Regional Office', false, 1, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Seed Test Users
INSERT INTO public."PAOUsers" ("Id", "Email", "FirstName", "LastName") 
VALUES 
  (1, 'testuser1@unops.org', 'Test', 'User 1'),
  (2, 'testuser2@unops.org', 'Test', 'User 2'),
  (3, 'testadmin@unops.org', 'Admin', 'User')
ON CONFLICT ("Id") DO NOTHING;

-- Seed Proposed Initiative Types
INSERT INTO public."ProposedInitiativeTypes" ("Id", "Name", "IsDeleted", "CreatedBy", "CreatedDate") 
VALUES 
  (1, 'Project', false, 1, NOW()),
  (2, 'Programme', false, 1, NOW()),
  (3, 'Advisory', false, 1, NOW())
ON CONFLICT ("Id") DO NOTHING;

COMMIT;
```

Run seed script:

```bash
psql -U pao_test_user -d unops_pao_test -f seed-test-data.sql
```

---

## 🏷️ **CATEGORIZING ENVIRONMENT-DEPENDENT TESTS**

Add traits to tests that require specific environment setup:

```csharp
[Fact]
[Trait("Category", "RequiresEnvironment")]
[Trait("Environment", "IntegrationDatabase")]
public async Task TestRequiringDatabaseConnection()
{
    // Test implementation
}
```

Run tests excluding environment-dependent ones:

```bash
dotnet test --filter "Category!=RequiresEnvironment"
```

---

## 🚀 **CI/CD INTEGRATION**

### GitHub Actions Example

```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:13
        env:
          POSTGRES_USER: pao_test_user
          POSTGRES_PASSWORD: Test_Pass_123!
          POSTGRES_DB: unops_pao_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Run Migrations
        env:
          ASPNETCORE_ENVIRONMENT: Testing
        run: |
          dotnet ef database update \
            --project UNOPS.PAO.UNOPSDataAccess/UNOPS.PAO.UNOPSDataAccess.csproj \
            --startup-project UNOPS.PAO.Server/UNOPS.PAO.Server.csproj
      
      - name: Run Integration Tests
        env:
          ASPNETCORE_ENVIRONMENT: Testing
        run: |
          dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests" \
            --logger "trx;LogFileName=test-results.trx"
```

---

## 🔧 **TROUBLESHOOTING**

### Issue: "Database does not exist"

```bash
# Verify database was created
psql -U postgres -c "\l" | grep unops_pao_test

# If not found, recreate:
psql -U postgres -c "CREATE DATABASE unops_pao_test;"
```

### Issue: "Password authentication failed"

```bash
# Reset password
psql -U postgres -c "ALTER USER pao_test_user WITH PASSWORD 'Test_Pass_123!';"
```

### Issue: "Permission denied for schema public"

```sql
-- Reconnect as superuser
psql -U postgres -d unops_pao_test

-- Grant all privileges
GRANT ALL ON SCHEMA public TO pao_test_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO pao_test_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO pao_test_user;
```

### Issue: "Migration failed"

```bash
# Check current migration status
dotnet ef migrations list --project UNOPS.PAO.UNOPSDataAccess

# If needed, drop and recreate database
dropdb -U postgres unops_pao_test
createdb -U postgres -O pao_test_user unops_pao_test

# Rerun migrations
dotnet ef database update --project UNOPS.PAO.UNOPSDataAccess
```

---

## ✅ **VERIFICATION CHECKLIST**

- [ ] PostgreSQL installed and running
- [ ] Test database `unops_pao_test` created
- [ ] Test user `pao_test_user` created with correct password
- [ ] User has ALL privileges on test database
- [ ] `appsettings.Testing.json` files created in all test projects
- [ ] Connection string points to localhost test database
- [ ] Migrations applied successfully
- [ ] Database contains expected tables (Partners, Contacts, etc.)
- [ ] Fast Tests (78 tests) all pass
- [ ] Integration Tests run without database connection errors

---

## 📊 **EXPECTED RESULTS**

After completing this setup:

| Test Suite | Before | After | Improvement |
|------------|--------|-------|-------------|
| Integration Tests | 91.9% | **95%+** | +3.1% |
| Business Tests | 91.8% | **95%+** | +3.2% |
| Tests with DB errors | 40+ failing | **0 failing** | 100% fixed |

---

## 📚 **ADDITIONAL RESOURCES**

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Entity Framework Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [UNOPS PAO Architecture Documentation](../../docs/architecture.md)

---

**Last Updated**: January 23, 2026  
**Maintainer**: UNOPS Opportunity+ Development Team
