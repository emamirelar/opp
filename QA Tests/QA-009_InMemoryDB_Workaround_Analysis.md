# QA-009: InMemory Database Workaround Analysis

**Issue:** Z.EntityFramework.Extensions fails with InMemory database  
**Impact:** ~38-46 Opportunity tests failing  
**Date:** February 2, 2026

---

## Problem Summary

The `Z.EntityFramework.Extensions` library (used for `BulkUpdate`, `SingleUpdateAsync`) requires a relational database provider. The InMemory provider used in tests doesn't support `GetRelationalModel()`.

**Error:**
```
System.InvalidOperationException: The model must be finalized and its runtime 
dependencies must be initialized before 'GetRelationalModel' can be used.
```

---

## Affected Operations

| Method | Location | Tests Affected |
|--------|----------|----------------|
| `BulkUpdate` | BaseRepository.cs | ~20 |
| `SingleUpdateAsync` | BaseRepository.cs | ~15 |
| `BulkInsertAsync` | Various managers | ~10 |

---

## Workaround Options

### Option A: SQLite Test Database (RECOMMENDED)

**Effort:** 4-6 hours  
**Pros:** Relational provider, supports most EF operations, fast  
**Cons:** Some PostgreSQL-specific features may not work

**Implementation:**

```csharp
// In TestDbContextFactory.cs
public static AppDbContext CreateWithSQLite()
{
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;
    
    var context = new AppDbContext(options);
    context.Database.EnsureCreated();
    
    return context;
}
```

**Changes Required:**
1. Add `Microsoft.EntityFrameworkCore.Sqlite` package to test project
2. Update `TestDbContextFactory` to use SQLite
3. Handle SQLite-specific quirks (e.g., no `DateTimeOffset`)
4. Run tests to identify remaining issues

### Option B: Mock Repository Layer

**Effort:** 8-12 hours  
**Pros:** Fast tests, no database needed  
**Cons:** Doesn't test actual database operations

**Implementation:**

```csharp
// Create mock repository
public class MockBaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly List<T> _entities = new();
    
    public Task<T> UpdateAsync(T entity)
    {
        var existing = _entities.FirstOrDefault(e => GetId(e) == GetId(entity));
        if (existing != null)
        {
            _entities.Remove(existing);
        }
        _entities.Add(entity);
        return Task.FromResult(entity);
    }
    
    // ... other methods
}
```

### Option C: Conditional Logic in Repository

**Effort:** 2-4 hours  
**Pros:** Quick fix  
**Cons:** Production code contains test logic (NOT RECOMMENDED)

```csharp
// In BaseRepository.cs - NOT RECOMMENDED
public async Task<T> UpdateAsync(T entity)
{
    if (_context.Database.IsInMemory())
    {
        // Use regular EF update
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }
    else
    {
        // Use Z.EntityFramework.Extensions
        await _context.BulkUpdateAsync(new[] { entity });
    }
    return entity;
}
```

### Option D: PostgreSQL Test Container

**Effort:** 6-10 hours  
**Pros:** Tests real PostgreSQL behavior  
**Cons:** Slower tests, requires Docker

**Implementation:**

```csharp
// Using Testcontainers library
public class PostgresTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .Build();
    
    public string ConnectionString => _postgres.GetConnectionString();
    
    public async Task InitializeAsync() => await _postgres.StartAsync();
    public async Task DisposeAsync() => await _postgres.DisposeAsync();
}
```

---

## Recommendation

**Implement Option A (SQLite)** as the primary fix:

1. Fast and reliable
2. Supports relational operations
3. No Docker dependency
4. Widely used pattern

**Fallback to Option B (Mock)** for tests that still fail with SQLite.

---

## Implementation Steps

### Step 1: Add SQLite Package

```xml
<!-- UNOPS.PAO.Business.Tests.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
```

### Step 2: Update Test Factory

```csharp
public static class TestDbContextFactory
{
    private static SqliteConnection _connection;
    
    public static AppDbContext Create(DbContextOptions<AppDbContext> options = null)
    {
        if (options != null)
        {
            return new AppDbContext(options);
        }
        
        // Use SQLite by default
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        
        var sqliteOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        
        var context = new AppDbContext(sqliteOptions);
        context.Database.EnsureCreated();
        
        return context;
    }
}
```

### Step 3: Handle SQLite Quirks

```csharp
// SQLite doesn't support DateTimeOffset natively
// Add value converter in OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    if (Database.IsSqlite())
    {
        // Convert DateTimeOffset to string for SQLite
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(DateTimeOffset) 
                         || p.PropertyType == typeof(DateTimeOffset?));
            
            foreach (var property in properties)
            {
                modelBuilder.Entity(entityType.Name)
                    .Property(property.Name)
                    .HasConversion<string>();
            }
        }
    }
    
    base.OnModelCreating(modelBuilder);
}
```

### Step 4: Run Tests and Fix Remaining Issues

```bash
cd "QA Tests/C# Tests"
dotnet test UNOPS.PAO.Business.Tests --filter "FullyQualifiedName~Opportunity" --logger "console;verbosity=detailed"
```

---

## Expected Results After Fix

| Metric | Before | After (Expected) |
|--------|--------|------------------|
| Opportunity Tests Passing | ~92% | ~98% |
| Z.EntityFramework Errors | 38-46 | 0 |
| Test Duration | ~2 min | ~2.5 min (slightly slower) |

---

## Files to Modify

1. `UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj` - Add SQLite package
2. `UNOPS.PAO.Business.Tests/TestBase/TestDbContextFactory.cs` - Use SQLite
3. `UNOPS.PAO.DataAccess/Context/AppDbContext.cs` - Add SQLite quirk handling (optional)

---

## Timeline

| Task | Effort | Owner |
|------|--------|-------|
| Add SQLite package | 30 min | Dev/QA |
| Update TestDbContextFactory | 2 hours | Dev/QA |
| Handle SQLite quirks | 2 hours | Dev/QA |
| Test and fix issues | 2 hours | QA |
| **Total** | **6-7 hours** | - |

---

*This analysis provides the path forward for resolving QA-009. Implementation should be coordinated with the development team.*
