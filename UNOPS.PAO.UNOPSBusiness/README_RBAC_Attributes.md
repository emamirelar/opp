# RBAC Attributes - Declarative Security for UNOPS PAO

## Overview

The RBAC (Role-Based Access Control) attributes system provides a declarative way to apply security constraints to your manager methods without writing repetitive security code. Instead of manually calling security services in every method, you simply add attributes like `[RBACRead]`, `[RBACUpdate]`, etc.

## ✅ **Benefits**

- **🧹 Clean Code**: No more manual security checks in every method
- **🔒 Consistent Security**: Automatic enforcement ensures no forgotten checks
- **📖 Declarative**: Security requirements are clear from method signatures
- **🚀 Easier Maintenance**: Security logic centralized in interceptor
- **🔧 Flexible**: Supports both row and column filtering automatically

## ��️ **Setup**

### 1. Install Castle.Core Package

```xml
<PackageReference Include="Castle.Core" Version="5.1.1" />
```

### 2. Register RBAC Infrastructure in Startup.cs

```csharp
public void ConfigureContainer(ServiceRegistry services)
{
    // Add RBAC Infrastructure (interceptors, proxy generator, etc.)
    services.AddRBACInfrastructure();
    
    // Register your managers with RBAC
    services.AddRBACServices(builder =>
    {
        builder.AddService<IContactManager, ContactManager>();
        builder.AddService<IPartnerManager, PartnerManager>();
        builder.AddService<IInteractionManager, InteractionManager>();
    });
    
    // Or register individual services
    services.AddRBACService<IContactManager, ContactManager>();
}
```

## 📋 **The RBAC Attribute**

Now there's just **one unified attribute** with configurable properties! 🎉

### **Basic Usage Examples**

#### **Read Operations**
```csharp
[RBAC("read", Entity = "Contact")]
public async Task<ContactModel?> GetContactByIdAsync(ClaimsPrincipal user, int contactId)
{
    // No manual security checks needed!
    var contact = await GetContactFromDatabase(contactId);
    return MapToModel(contact);
}
```

#### **Create Operations**
```csharp
[RBAC("create", Entity = "Contact")]
public async Task<ContactModel> CreateContactAsync(ClaimsPrincipal user, ContactCreateModel model)
{
    // Interceptor ensures user can create contacts and set all specified fields
    var contact = new Contact { FirstName = model.FirstName, /* ... */ };
    return await SaveContact(contact);
}
```

#### **Update Operations**
```csharp
[RBAC("update", Entity = "Contact", EntityIdParameterName = "contactId")]
public async Task<ContactModel?> UpdateContactAsync(ClaimsPrincipal user, int contactId, ContactUpdateModel model)
{
    var contact = await GetContact(contactId);
    contact.FirstName = model.FirstName;
    return await SaveContact(contact);
}
```

#### **Delete Operations**
```csharp
[RBAC("delete", Entity = "Contact", EntityIdParameterName = "contactId")]
public async Task<bool> DeleteContactAsync(ClaimsPrincipal user, int contactId)
{
    return await DeleteFromDatabase(contactId);
}
```

#### **Custom Actions**
```csharp
[RBAC("approve", Entity = "Contact", EntityIdParameterName = "contactId")]
public async Task<bool> ApproveContactAsync(ClaimsPrincipal user, int contactId)
{
    var contact = await GetContact(contactId);
    contact.IsApproved = true;
    return await SaveContact(contact);
}
```

### **Advanced Configuration**

#### **Skip Column Filtering (Keep Row Filtering)**
```csharp
[RBAC("read", Entity = "Contact", ApplyColumnFiltering = false)]
public async Task<List<Contact>> GetRawContactsAsync(ClaimsPrincipal user)
{
    // Returns contacts with row filtering but no column filtering
    return await _context.Contacts.ToListAsync();
}
```

#### **Skip Row Filtering (Keep Column Filtering)**
```csharp
[RBAC("read", Entity = "Contact", ApplyRowFiltering = false)]
public async Task<List<ContactModel>> GetAllContactsForAdminAsync(ClaimsPrincipal user)
{
    // Returns all contacts but with column filtering applied
    return await _context.Contacts.Select(MapToModel).ToListAsync();
}
```

#### **Auto-Detect Entity Name**
```csharp
// In UNOPSContactManager class - Entity auto-detected as "Contact"
[RBAC("read")]
public async Task<List<ContactModel>> GetContactsAsync(ClaimsPrincipal user)
{
    return await _context.Contacts.Select(MapToModel).ToListAsync();
}
```

#### **Custom Entity Parameter**
```csharp
[RBAC("update", Entity = "Contact", EntityParameterName = "contactEntity")]
public async Task<ContactModel> UpdateContactAsync(ClaimsPrincipal user, Contact contactEntity)
{
    // Interceptor uses the contactEntity parameter for access checks
    contactEntity.LastModified = DateTime.UtcNow;
    return await SaveContact(contactEntity);
}
```

### **Skip RBAC for Internal Operations**
```csharp
[SkipRBAC("Internal system operation")]
public async Task<List<Contact>> GetAllContactsForReportAsync()
{
    // Bypasses all RBAC checks - use carefully!
    return await _context.Contacts.ToListAsync();
}
```

### **🎛️ All Available Properties**

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `Action` | string | Security action (read/create/update/delete/custom) | **Required** |
| `Entity` | string? | Entity name for permissions | Auto-detected from manager |
| `ApplyRowFiltering` | bool | Apply row-level filtering | `true` (false for create) |
| `ApplyColumnFiltering` | bool | Apply column-level filtering | `true` |
| `RequireEntityAccess` | bool | Check specific entity access | `false` (true for update/delete) |
| `EntityParameterName` | string? | Parameter containing entity object | `null` |
| `EntityIdParameterName` | string? | Parameter containing entity ID | `null` |
| `Reason` | string? | Documentation/debugging reason | `null` |

## 🎯 **Before vs After Example**

### ❌ **Before: Manual Security Checks**

```csharp
public async Task<PaginatedResponse<ContactModel>> GetContactsAsync(ClaimsPrincipal user, PaginatedRequest request)
{
    // 😫 Manual permission check
    var permissions = await _securityService.GetEntityPermissionsAsync(user, "Contact");
    if (!permissions.CanRead)
        throw new UnauthorizedAccessException("User cannot read contacts");
    
    // 😫 Manual row filtering
    var query = _context.Contacts.AsQueryable();
    var filteredQuery = await _securityService.ApplyRowFiltersAsync(query, user, "read");
    
    // 😫 Execute query with pagination
    var contacts = await filteredQuery
        .Skip(request.Skip)
        .Take(request.PageSize)
        .ToListAsync();
    
    // 😫 Manual column filtering
    var filteredContacts = await _securityService.FilterEntityColumnsForList(contacts, user, "read");
    
    // 😫 Map to models
    var contactModels = filteredContacts.Select(MapToModel).ToList();
    
    return new PaginatedResponse<ContactModel>
    {
        Data = contactModels,
        TotalCount = await filteredQuery.CountAsync(),
        PageSize = request.PageSize,
        CurrentPage = request.CurrentPage
    };
}
```

### ✅ **After: RBAC Attribute**

```csharp
[RBAC("read", Entity = "Contact")]
public async Task<PaginatedResponse<ContactModel>> GetContactsAsync(ClaimsPrincipal user, PaginatedRequest request)
{
    // 🎉 All security handled automatically by the interceptor!
    var contacts = await _context.Contacts
        .Skip(request.Skip)
        .Take(request.PageSize)
        .ToListAsync();
    
    return new PaginatedResponse<ContactModel>
    {
        Data = contacts.Select(MapToModel).ToList(),
        TotalCount = await _context.Contacts.CountAsync(),
        PageSize = request.PageSize,
        CurrentPage = request.CurrentPage
    };
}
```

**The interceptor automatically:**
1. ✅ Checks user permissions
2. ✅ Applies row filtering to `_context.Contacts`
3. ✅ Applies column filtering to returned `ContactModel` objects

## 🔧 **Configuration**

Works with your existing EntityPermissions table! The interceptor uses the same `BusinessSecurityService` and column filtering you already have.

### Row Filtering (RowFilter field)
```json
{
  "CanRead": "",
  "CanUpdate": "OrgUnit != @userOrgUnit",
  "CanDelete": ""
}
```

### Column Filtering (PropertyFilter field - DENYLIST approach)
```json
{
  "CanRead": ["CreatedBy", "LastModifiedBy"],
  "CanCreate": ["CreatedBy", "CreatedDate"],
  "CanUpdate": ["Id", "CreatedBy", "CreatedDate"],
  "CanDelete": []
}
```

## 🚀 **Migration Strategy**

1. **Install Castle.Core** and add RBAC infrastructure to Startup.cs
2. **Register your existing managers** with `AddRBACService`
3. **Add attributes to methods** one by one
4. **Remove manual security checks** from those methods
5. **Test thoroughly** with different user roles

## 🔄 **How It Works**

1. **Castle DynamicProxy** intercepts all interface method calls
2. **RBACInterceptor** checks for RBAC attributes on the method
3. **Pre-execution**: Validates permissions and entity access
4. **Execution**: Your business logic runs normally
5. **Post-execution**: Applies row/column filtering to return values

## 🐛 **Troubleshooting**

### "No ClaimsPrincipal found in method parameters"
- Ensure your method has a `ClaimsPrincipal user` parameter

### "User does not have X permission for Y"
- Check EntityPermissions table for the user's roles
- Verify boolean permission flags (CanRead, CanCreate, etc.)

### "Interceptor not working"
- Ensure service is registered with `AddRBACService<Interface, Implementation>()`
- Verify you're injecting the interface, not the implementation

## 📊 **Performance Impact**

- **Minimal overhead**: Only processes methods with RBAC attributes
- **Same security logic**: Uses existing BusinessSecurityService
- **Cached permissions**: Permission lookups can be cached
- **Lazy filtering**: Only applies filtering to actual return values

## 🎊 **The Big Win: One Attribute to Rule Them All**

### **❌ Before: Multiple Attributes**
```csharp
[RBACRead("Contact")]           // Read operations
[RBACCreate("Contact")]         // Create operations  
[RBACUpdate("Contact", entityIdParameterName: "id")]  // Update operations
[RBACDelete("Contact", entityIdParameterName: "id")]  // Delete operations
[RBACCustom("approve", "Contact")]  // Custom operations
```

### **✅ After: One Unified Attribute**
```csharp
[RBAC("read", Entity = "Contact")]                    // Read operations
[RBAC("create", Entity = "Contact")]                  // Create operations
[RBAC("update", Entity = "Contact", EntityIdParameterName = "id")]  // Update operations
[RBAC("delete", Entity = "Contact", EntityIdParameterName = "id")]  // Delete operations
[RBAC("approve", Entity = "Contact", EntityIdParameterName = "id")] // Custom operations
```

**🎯 Benefits:**
- ✅ **Simpler**: One attribute instead of 5+ different ones
- ✅ **Flexible**: All properties configurable on one attribute
- ✅ **Consistent**: Same syntax for all operations
- ✅ **Extensible**: Easy to add new actions without new attributes
- ✅ **Discoverable**: IntelliSense shows all options in one place

---

## 🎭 **Hybrid Approach: Security + Frontend Permissions**

Sometimes you need both security enforcement AND permission information for the frontend. Here's the clean way to handle that:

### **✅ New Simplified Approach**

```csharp
[RBAC("read", Entity = "Contact")]
public async Task<PaginatedResponse<ContactModel>> GetContactsAsync(ClaimsPrincipal user, PaginatedRequest request)
{
    // RBAC interceptor automatically:
    // ✅ Checks user has read permission
    // ✅ Applies row filtering to queries
    // ✅ Applies column filtering to results
    
    var contacts = await _repository.GetContactsAsync();
    
    // Single method call gets all permissions for frontend UI
    foreach (var contact in contacts)
    {
        contact.Permissions = await GetEntityPermissionsAsync(contact, user);
    }
    
    return new PaginatedResponse<ContactModel> { Data = contacts };
}
```

### **🔧 What `GetEntityPermissionsAsync()` Does**

This helper method (available in all managers via `BaseUNOPSManager`):
- ✅ **Gets all permissions in one call** (read, create, update, delete)
- ✅ **Runs checks in parallel** for better performance
- ✅ **Handles admin users** automatically (full permissions)
- ✅ **Provides fallback** if security service is unavailable

### **❌ Old Way (Multiple Calls)**
```csharp
// 😫 Multiple async calls - slow and verbose
contact.Permissions = new EntityPermissionsModel
{
    CanRead = await _securityService.CanUserAccessEntityAsync(contact, user, "read"),
    CanUpdate = await _securityService.CanUserAccessEntityAsync(contact, user, "update"),
    CanDelete = await _securityService.CanUserAccessEntityAsync(contact, user, "delete"),
    CanCreate = await _securityService.CanUserAccessEntityAsync(contact, user, "create")
};
```

### **✅ New Way (Single Call)**
```csharp
// 🎉 One call gets everything - fast and clean
contact.Permissions = await GetEntityPermissionsAsync(contact, user);
```

**🎯 This gives you:**
- ✅ **Bulletproof Security**: RBAC interceptor enforces all security rules
- ✅ **Frontend UX**: Permission flags control what users see in the UI
- ✅ **Clean Code**: Single method call for all permissions
- ✅ **Better Performance**: Parallel permission checks

**🎉 Result: Clean, secure, maintainable code with zero repetition!**

## 📚 **Further Reading**

- [Role-Based Access Control Implementation Guide](../Readme/Role-Based-Access-Control-Implementation.md)
- [Column Filter Configuration](../Scripts/sample-column-filters.sql)
- [Castle DynamicProxy Documentation](https://github.com/castleproject/Core) 