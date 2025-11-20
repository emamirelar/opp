# Audit Field Seeder Fix

## Problem

When running seeder scripts to update audit fields (`CreatedBy`, `LastModifiedBy`, etc.), the values were being overwritten by the current user running the seeding operation, even though the code explicitly set them to `-1` (system user) or other specific values.

## Root Cause

The issue was in `UNOPS.PAO.DataAccess/Context/AuditableDbContext.cs`:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    ApplyAuditInformation();
    return await base.SaveChangesAsync(cancellationToken);
}

private void ApplyAuditInformation()
{
    var entries = ChangeTracker.Entries();

    foreach (var entry in entries)
    {
        // ... 
        
        if (entry is { Entity: IModifiableEntity<TId, TUserId> modifiable, State: EntityState.Modified})
        {
            modifiable.SetUpdateAuditData(_currentUserId);  // ← This overwrites LastModifiedBy!
        }
        
        // ...
    }
}
```

The `ApplyAuditInformation()` method automatically sets audit fields for ALL modified entities, regardless of whether they were explicitly set in the code. This is by design for normal application operations, but it interferes with seeding operations that need to set historical audit data.

## Solution

Use `ExecuteUpdateAsync()` instead of `SaveChangesAsync()` in seeder scripts. This method:

1. **Bypasses the change tracker** - Updates are executed directly as SQL
2. **Bypasses the audit interceptor** - `ApplyAuditInformation()` is never called
3. **More performant** - No entity loading or tracking overhead
4. **Atomic** - Each update is executed as a single SQL statement

### Before (Incorrect)

```csharp
// Load entity into change tracker
var partner = await context.Partners.FirstOrDefaultAsync(p => p.Id == partnerId);

// Modify audit fields
partner.LastModifiedBy = -1;  // Will be overwritten!
partner.LastModifiedDate = DateTime.UtcNow;

// SaveChanges triggers audit interceptor
await context.SaveChangesAsync();  // ← Overwrites LastModifiedBy with current user
```

### After (Correct)

```csharp
// Direct SQL update - bypasses change tracker and audit interceptor
await context.Partners
    .Where(p => p.Id == partnerId)
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(p => p.LastModifiedBy, -1)
        .SetProperty(p => p.LastModifiedDate, DateTime.UtcNow));
```

## Files Updated

### 1. Partner_Audit_Data_Fixes_v3.cs

**Original approach:**
- Loaded all partners into change tracker
- Modified `CreatedBy` and `LastModifiedBy` in memory
- Called `SaveChangesAsync()` (which triggered audit interceptor)

**Fixed approach:**
```csharp
// Update CreatedBy for partners where it matches larsj user ID
int createdByUpdates = await context.Partners
    .Where(p => p.CreatedBy == larsjUserId)
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(p => p.CreatedBy, -1));

// Update LastModifiedBy for partners where it matches larsj user ID
int lastModifiedByUpdates = await context.Partners
    .Where(p => p.LastModifiedBy == larsjUserId)
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(p => p.LastModifiedBy, -1));
```

### 2. Contact_Audit_Data_Fixes_v3.cs

**Original approach:**
- Loaded each contact into change tracker
- Modified multiple audit fields (`CreatedBy`, `CreatedDate`, `LastModifiedBy`, `LastModifiedDate`)
- Called `SaveChangesAsync()` once for all changes

**Fixed approach:**
```csharp
// Use ExecuteUpdateAsync to bypass audit interceptor
var updateQuery = context.Contacts.Where(c => c.ContactNumber == updateData.ContactId);

if (createdByUserId.HasValue && createdDate.HasValue && 
    shouldUpdateLastModified && lastModifiedByUserId.HasValue && lastModifiedDate.HasValue)
{
    // Update all four fields in one SQL statement
    await updateQuery.ExecuteUpdateAsync(setters => setters
        .SetProperty(c => c.CreatedBy, createdByUserId.Value)
        .SetProperty(c => c.CreatedDate, createdDate.Value)
        .SetProperty(c => c.LastModifiedBy, lastModifiedByUserId.Value)
        .SetProperty(c => c.LastModifiedDate, lastModifiedDate.Value));
}
```

## Benefits

1. **Correctness**: Audit fields are set to intended values without being overwritten
2. **Performance**: No entity loading or change tracking overhead
3. **Consistency**: All audit seeders now use the same pattern
4. **Maintainability**: Clear separation between application code (uses SaveChanges) and seeding code (uses ExecuteUpdateAsync)

## When to Use Each Approach

### Use `SaveChangesAsync()`:
- Normal application CRUD operations
- When you want automatic audit tracking
- When modifying complex entity graphs with navigation properties

### Use `ExecuteUpdateAsync()`:
- Data seeding operations
- Bulk updates
- When you need to set audit fields explicitly
- When performance is critical (no tracking overhead)
- When you need to bypass interceptors

## PostgreSQL DateTime UTC Requirement

**Important**: PostgreSQL's `timestamp with time zone` type only accepts DateTime values with `Kind=Utc`. When parsing DateTime strings for database updates, you must ensure they are converted to UTC:

```csharp
if (DateTime.TryParse(dateString, out DateTime parsedDate))
{
    // Ensure DateTime is in UTC for PostgreSQL
    var utcDate = parsedDate.Kind == DateTimeKind.Utc 
        ? parsedDate 
        : DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
}
```

**Without this conversion**, you'll get the error:
```
System.ArgumentException: 'Cannot write DateTime with Kind=Local to PostgreSQL type 
'timestamp with time zone', only UTC is supported. (Parameter 'value')'
```

## EF Core Version Requirement

`ExecuteUpdateAsync()` was introduced in **Entity Framework Core 7.0**. Ensure your project is using EF Core 7.0 or later.

## Testing

After applying this fix:

1. Run the seeder scripts
2. Verify in the database that audit fields have the expected values:
   ```sql
   SELECT "Id", "Name", "CreatedBy", "LastModifiedBy" 
   FROM "Partners" 
   WHERE "LastModifiedBy" = -1;
   ```
3. Confirm that `LastModifiedBy` is `-1` (system user) and not the ID of the user who ran the seeder

## References

- [EF Core 7.0 ExecuteUpdate Documentation](https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete)
- `UNOPS.PAO.DataAccess/Context/AuditableDbContext.cs` - Audit interceptor implementation

