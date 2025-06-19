# 🧪 RBAC Testing Guide for Contact Entity

This guide walks you through testing the Role-Based Access Control (RBAC) system for the Contact entity.

## 🚀 Quick Start

### 1. **Start Your Application**
```bash
cd UNOPS.PAO.Server
dotnet run
```

### 2. **Run the Test Script**
```powershell
# From the root directory
.\test-rbac.ps1
```

### 3. **Check Swagger UI**
Navigate to: `https://localhost:7123/swagger` and look for the `RBACTest` controller endpoints.

---

## 🔍 Manual Testing Steps

### **Step 1: Check User Permissions**
```http
GET https://localhost:7123/api/RBACTest/permissions
```

**Expected Response:**
```json
{
  "entity": "Contact",
  "permissions": {
    "canRead": true,
    "canCreate": false,
    "canUpdate": false,
    "canDelete": false
  },
  "userClaims": [
    {
      "type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
      "value": "test@example.com"
    },
    {
      "type": "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", 
      "value": "PARTNER_USER"
    }
  ]
}
```

### **Step 2: Test Contact List Access**
```http
GET https://localhost:7123/api/RBACTest/contacts?pageIndex=1&pageSize=10
```

**What to Expect:**
- ✅ **Success (200)**: RBAC allows access, returns filtered contacts
- 🚫 **Forbidden (403)**: RBAC blocks access due to insufficient permissions
- 🔐 **Unauthorized (401)**: Authentication issue

### **Step 3: Test Specific Contact Access**
```http
GET https://localhost:7123/api/RBACTest/contacts/1
```

**What to Expect:**
- ✅ **Success (200)**: User can access this specific contact
- 📭 **Not Found (404)**: Contact doesn't exist OR user can't access it (row-level security)
- 🚫 **Forbidden (403)**: User doesn't have read permission for Contact entity

---

## 🎯 What Each Test Validates

### **Permissions Test** (`/api/RBACTest/permissions`)
- ✅ **BusinessSecurityService** is working
- ✅ **EntityPermissions** table lookup is functioning
- ✅ **User roles** are being read correctly
- ✅ **Permission aggregation** across multiple roles

### **Contacts List Test** (`/api/RBACTest/contacts`)
- ✅ **RBAC Interceptor** is intercepting method calls
- ✅ **Row-level filtering** is being applied to queries
- ✅ **Column-level filtering** is being applied to results
- ✅ **Permission checks** are enforced before method execution

### **Single Contact Test** (`/api/RBACTest/contacts/{id}`)
- ✅ **Entity-specific access** checks are working
- ✅ **EntityIdParameterName** attribute property is functioning
- ✅ **Row-level security** for individual records

---

## 🔧 Troubleshooting

### **🚫 Getting 403 Forbidden?**
1. **Check EntityPermissions table:**
   ```sql
   SELECT * FROM EntityPermissions WHERE Entity = 'Contact';
   ```

2. **Verify user roles:**
   ```sql
   SELECT u.Email, r.Name as RoleName 
   FROM AspNetUsers u
   JOIN AspNetUserRoles ur ON u.Id = ur.UserId
   JOIN AspNetRoles r ON ur.RoleId = r.Id
   WHERE u.Email = 'your-test-email@example.com';
   ```

3. **Check role permissions:**
   ```sql
   SELECT * FROM EntityPermissions 
   WHERE Entity = 'Contact' AND Role IN ('PARTNER_USER', 'UNOPS_GEN_USER', 'PARTNER_GLOB_ADMIN');
   ```

### **🔐 Getting 401 Unauthorized?**
1. **Check authentication setup** in development mode
2. **Verify IAP headers** are being set correctly
3. **Check application logs** for authentication issues

### **❌ Getting 500 Internal Server Error?**
1. **Check application logs** for detailed error messages
2. **Verify Castle.Core** package is installed
3. **Ensure RBAC services** are registered correctly in Startup.cs

---

## 📊 Expected Log Output

When RBAC is working correctly, you should see logs like:

```
[INFO] Applying RBAC security for method GetContactsAsync, Entity: Contact, Action: read
[INFO] User has read permission for Contact entity
[INFO] Applying row filtering to query results
[INFO] Applying column filtering to 15 entities
[INFO] RBAC test successful - returned 15 contacts
```

---

## 🎭 Testing Different Scenarios

### **Scenario 1: Admin User**
- **Role**: `PARTNER_GLOB_ADMIN`
- **Expected**: Full access to all contacts, no filtering applied

### **Scenario 2: Regular Partner User**
- **Role**: `PARTNER_USER`  
- **Expected**: Only sees contacts from their organization, some columns filtered

### **Scenario 3: UNOPS User**
- **Role**: `UNOPS_GEN_USER`
- **Expected**: Sees all contacts, specific columns may be filtered

### **Scenario 4: No Role User**
- **Role**: None
- **Expected**: 403 Forbidden on all operations

---

## 🔄 Testing Row-Level Filtering

To test row-level filtering, you need different users with different `OrgUnit` values:

1. **Create test users** with different org units
2. **Insert test contacts** with different `PartnerId` values
3. **Configure row filters** in EntityPermissions table
4. **Test each user** sees only their allowed contacts

Example row filter configuration:
```json
{
  "CanRead": "Partner.PartnerOffice.Code = @userOrgUnit"
}
```

---

## 🎨 Testing Column-Level Filtering

To test column-level filtering:

1. **Configure PropertyFilter** in EntityPermissions table:
   ```json
   {
     "CanRead": ["CreatedBy", "LastModifiedBy", "Department"]
   }
   ```

2. **Make API calls** and verify restricted fields are not returned

3. **Check different roles** have different column restrictions

---

## 📝 Next Steps After Testing

1. **✅ RBAC Working**: Move on to implementing RBAC for other entities (Partner, Interaction)
2. **🔧 Issues Found**: Check the troubleshooting section and application logs
3. **🎯 Performance Testing**: Test with larger datasets to ensure performance is acceptable
4. **🛡️ Security Audit**: Verify all sensitive operations are properly protected

---

## 🎉 Success Criteria

Your RBAC system is working correctly if:

- ✅ **Permissions endpoint** returns correct user permissions
- ✅ **Unauthorized users** get 403 Forbidden responses  
- ✅ **Authorized users** get filtered data appropriate to their role
- ✅ **Row-level filtering** shows only allowed records
- ✅ **Column-level filtering** hides restricted fields
- ✅ **Application logs** show RBAC interceptor activity
- ✅ **No manual security checks** needed in manager methods

**🎊 Congratulations! Your RBAC system is bulletproof!** 