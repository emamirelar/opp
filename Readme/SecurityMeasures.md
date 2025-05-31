# Security Measures for Generic Row Filter Service

## Overview
The Generic Row Filter Service implements comprehensive security measures to prevent SQL injection, code injection, and other security threats when processing dynamic LINQ expressions from the database.

## Security Layers

### 1. Input Validation
- **Expression Length Limit**: Maximum 1000 characters to prevent DoS attacks
- **Nesting Depth Limit**: Maximum 10 levels of parentheses nesting to prevent stack overflow
- **Structural Validation**: Checks for balanced parentheses and quotes
- **Forbidden Characters**: Blocks dangerous characters like `;`, `--`, `/*`, `*/`, null bytes, etc.

### 2. Parameter Processing Security
- **Type Validation**: Only allows specific safe types (string, int, long, decimal, double, float, bool)
- **String Escaping**: Properly escapes quotes and special characters in string values
- **Parameter Replacement**: Secure replacement of user context variables like `@userOrgUnit`, `@currentUserId`

### 3. Expression Content Filtering

#### Dangerous Pattern Blocking
- **.NET System Types**: Blocks access to `System.*`, `Process.*`, `File.*`, etc.
- **Reflection**: Prevents `Type.*`, `Method.*`, `Assembly.*`, etc.
- **SQL Injection**: Blocks `DROP`, `DELETE`, `INSERT`, `UPDATE`, `UNION`, etc.
- **XSS Prevention**: Blocks `<script>`, `javascript:`, event handlers, etc.
- **File System Access**: Prevents `../`, `C:\`, `/etc/`, etc.
- **Network Access**: Blocks `http://`, `ftp://`, etc.
- **Code Execution**: Prevents `eval`, `compile`, `execute`, etc.

#### Whitelisted Patterns
- **Entity Properties**: `Id`, `Code`, `Name`, `Email`, `CreatedAt`, etc.
- **Navigation Properties**: `Partner`, `Contact`, `Interaction`, `OrgUnit`, etc.
- **LINQ Methods**: `Any()`, `All()`, `Count()`, `Where()`, `Contains()`, etc.
- **Comparison Operators**: `==`, `!=`, `>=`, `<=`, `&&`, `||`, etc.
- **Safe String Methods**: `ToLower()`, `ToUpper()`, `Trim()`, `StartsWith()`, etc.

### 4. Secure Execution Context
- **Parsing Configuration**: Uses secure `ParsingConfig` settings:
  - `AllowNewToEvaluateAnyType = false`: Prevents instantiation of arbitrary types
  - `DisableMemberAccessToIndexAccessorForDynamicTypes = true`: Blocks dynamic indexing
  - `ResolveTypesBySimpleName = false`: Prevents type resolution by simple names
  - `EvaluateGroupByAtDatabase = true`: Forces database evaluation

### 5. Runtime Validation
- **Syntax Validation**: Pre-validates LINQ expression syntax before execution
- **Dry Run Testing**: Tests expressions against dummy queryables to catch errors
- **Exception Handling**: Graceful error handling with comprehensive logging
- **Fail-Safe Behavior**: Returns empty results on security violations

## Usage Examples

### Safe Expressions
```json
{
  "CanUpdate": "PartnerOffice != null && PartnerOffice.Code == @userOrgUnit",
  "CanDelete": "OrgUnit != null && OrgUnit.Code == @userOrgUnit",
  "CanRead": "InteractionUsers.Any(iu => iu.UserId == @currentUserId)"
}
```

### Blocked Expressions
```json
{
  "CanUpdate": "System.IO.File.Delete('C:\\temp\\file.txt')",
  "CanDelete": "'; DROP TABLE Partners; --",
  "CanRead": "javascript:alert('xss')"
}
```

## Security Monitoring

### Logging
- All security violations are logged with detailed information
- Expression validation failures are tracked
- User context and filtering attempts are audited

### Metrics to Monitor
- Number of security violations per user/session
- Frequency of blocked expressions
- Performance impact of security validations
- Failed expression parsing attempts

## Best Practices

### For Database Administrators
1. Regularly review and audit RowFilter JSON in EntityPermissions table
2. Use principle of least privilege when defining filters
3. Test all expressions in a development environment first
4. Monitor security logs for attempted violations

### For Developers
1. Always use parameterized expressions with `@userOrgUnit`, `@currentUserId`, etc.
2. Keep expressions simple and focused on business logic
3. Test edge cases and error conditions
4. Document any new allowed patterns in the whitelist

### For Security Teams
1. Regularly review the security patterns and update as needed
2. Monitor for new attack vectors and update validation accordingly
3. Conduct penetration testing on the filtering system
4. Review security logs for patterns indicating attempted attacks

## Future Enhancements

1. **Expression Caching**: Cache validated expressions to improve performance
2. **Rate Limiting**: Implement rate limiting for expression evaluation
3. **Audit Trail**: Enhanced audit trail for all filtering operations
4. **Machine Learning**: Use ML to detect suspicious expression patterns
5. **Sandboxing**: Additional isolation for expression execution 