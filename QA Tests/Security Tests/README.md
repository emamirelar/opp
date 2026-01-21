# Security Tests

**Status**: 🟡 **PLANNED - Awaiting Requirements**  
**Created**: January 15, 2026  
**Purpose**: Validate security controls and identify vulnerabilities

---

## 🔒 **Overview**

Security tests are designed to verify that the UNOPS Opportunity+ system properly implements security controls, protects against common vulnerabilities, and maintains data confidentiality, integrity, and availability.

---

## 🛡️ **Test Categories**

### **1. Injection Security Tests** (`Injection/`)
**Scope**: SQL injection, LINQ injection, XSS, and code injection

**Planned Tests:**
- `SQLInjectionTests.cs` - SQL injection prevention
- `DynamicLINQSecurityTests.cs` - Dynamic LINQ expression security (CRITICAL)
- `XSSPreventionTests.cs` - Cross-site scripting prevention
- `CommandInjectionTests.cs` - OS command injection prevention

**Attack Vectors to Test:**
```sql
-- SQL Injection attempts
' OR '1'='1
'; DROP TABLE Partners; --
' UNION SELECT * FROM Users--

-- Dynamic LINQ injection
System.IO.File.Delete('C:\\temp\\file.txt')
Process.Start('cmd.exe')
Assembly.Load('malicious.dll')

-- XSS attempts
<script>alert('xss')</script>
javascript:alert(document.cookie)
<img src=x onerror=alert('xss')>
```

**Key Security Controls:**
- Input validation and sanitization
- Parameterized queries
- Expression whitelist/blacklist
- Output encoding

---

### **2. Authentication Security Tests** (`Authentication/`)
**Scope**: IAP authentication, session management, credential handling

**Planned Tests:**
- `IAPAuthenticationSecurityTests.cs` - IAP header validation
- `SessionManagementSecurityTests.cs` - Session hijacking prevention
- `AuthenticationBypassTests.cs` - Authentication bypass attempts
- `BruteForceProtectionTests.cs` - Brute force attack prevention
- `TokenSecurityTests.cs` - JWT token security

**Attack Scenarios:**
```
❓ Can attacker bypass IAP by manipulating headers?
❓ Can attacker hijack another user's session?
❓ Are sessions properly invalidated on logout?
❓ Are passwords/tokens protected from exposure?
❓ Is there rate limiting on authentication endpoints?
```

**Key Security Controls:**
- IAP header verification
- Session timeout
- Secure cookie flags (HttpOnly, Secure, SameSite)
- Rate limiting
- Account lockout

---

### **3. Authorization Security Tests** (`Authorization/`)
**Scope**: RBAC, row-level security, privilege escalation

**Planned Tests:**
- `RBACSecurityTests.cs` - Role-based access control
- `RowLevelSecurityTests.cs` - Row-level filtering (CRITICAL)
- `PrivilegeEscalationTests.cs` - Vertical/horizontal privilege escalation
- `PermissionBypassTests.cs` - Permission bypass attempts

**Attack Scenarios:**
```
❓ Can user A access user B's data?
❓ Can regular user access admin functions?
❓ Can user modify data they don't own?
❓ Can user escalate their own permissions?
❓ Are OrgUnit filters properly enforced?
```

**Key Security Controls:**
- Permission checks on all operations
- Row-level security filters
- OrgUnit hierarchy validation
- Audit logging of access attempts

---

### **4. File Upload Security Tests** (`FileUpload/`)
**Scope**: Malicious file upload, path traversal, file validation

**Planned Tests:**
- `MaliciousFileUploadTests.cs` - Malicious file detection
- `FileValidationSecurityTests.cs` - File type/size validation
- `PathTraversalTests.cs` - Directory traversal prevention
- `FileContentSecurityTests.cs` - Content scanning

**Attack Vectors:**
```
❓ Can attacker upload .exe, .bat, .php files?
❓ Can attacker use path traversal (../../etc/passwd)?
❓ Can attacker upload files exceeding size limits?
❓ Can attacker upload files with embedded scripts?
❓ Are uploaded files scanned for malware?
```

**Key Security Controls:**
- File extension whitelist
- File size limits
- Content-Type validation
- Virus scanning
- Secure file storage (Google Cloud Storage)

---

## 🔧 **Test Infrastructure Requirements**

### **Prerequisites:**
- [ ] Penetration testing approval from security team
- [ ] Isolated test environment (NOT production!)
- [ ] Security testing tools integration
- [ ] Vulnerability disclosure process

### **Test Environment:**
- **Database**: Isolated test database
- **Authentication**: IAP simulation environment
- **Monitoring**: Security event logging enabled
- **Isolation**: No access to production data

### **Security Tools** (To Be Decided):
```
❓ OWASP ZAP - Dynamic application security testing
❓ SonarQube - Static code analysis
❓ Dependency Check - Vulnerable dependency scanning
❓ Burp Suite - Manual penetration testing
```

---

## 📋 **Questions to Answer Before Test Creation**

### **1. Security Testing Scope:**
- [ ] Can we perform actual attack simulations?
- [ ] Should tests be passive (detection only) or active (attempted exploitation)?
- [ ] Are there any off-limits attack vectors?
- [ ] Do we need external penetration testing certification?

### **2. Known Vulnerabilities:**
- [ ] Are there any known security issues to focus on?
- [ ] Have there been previous security audits?
- [ ] Are there any high-risk areas identified?

### **3. Compliance Requirements:**
- [ ] What security standards must be met (ISO 27001, SOC 2, GDPR)?
- [ ] Are there specific UN/UNOPS security requirements?
- [ ] What is the vulnerability disclosure process?

### **4. Security Controls:**
- [ ] Is there a WAF (Web Application Firewall)?
- [ ] Is there DDoS protection?
- [ ] Is there intrusion detection/prevention?
- [ ] Are there rate limits on API endpoints?

---

## 🎨 **Test Template Structure**

```csharp
[Trait("Category", "Security")]
[Trait("Type", "SQLInjection")]
public class SQLInjectionTests
{
    [Theory]
    [InlineData("' OR '1'='1")]
    [InlineData("'; DROP TABLE Partners; --")]
    [InlineData("' UNION SELECT * FROM Users--")]
    public async Task Search_SQLInjectionAttempt_IsBlocked(string maliciousInput)
    {
        // Arrange
        var searchRequest = new PartnerSearchRequest
        {
            SearchText = maliciousInput
        };
        
        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            await PartnerService.SearchAsync(searchRequest);
        });
        
        // Assert
        Assert.NotNull(exception); // Should throw or return empty
        // Verify no SQL injection occurred
        // Verify security event was logged
    }
}
```

---

## 🚨 **Critical Security Areas**

### **1. Dynamic LINQ Expression Evaluation** ⚠️ **HIGH RISK**
**Location**: `GenericRowFilterService.cs`  
**Risk**: Arbitrary code execution via malicious expressions

**Test Focus:**
- Expression validation and sanitization
- Whitelist/blacklist enforcement
- Type safety checks
- Reflection prevention

**Example Malicious Expressions:**
```csharp
"System.IO.File.Delete('C:\\important.txt')"
"Process.Start('cmd.exe', '/c del /f /q *.*')"
"Assembly.Load('malicious.dll').GetType('Exploit').GetMethod('Run').Invoke(null, null)"
```

---

### **2. IAP Authentication Bypass** ⚠️ **HIGH RISK**
**Location**: `IAPVerificationMiddleware.cs`  
**Risk**: Unauthorized access by header manipulation

**Test Focus:**
- IAP header validation
- Development mode bypass restrictions
- Header injection attempts
- Token verification

**Attack Scenarios:**
```
- Missing X-Goog-IAP-JWT-Assertion header
- Invalid JWT signature
- Expired JWT token
- Manipulated user claims
- Development mode exploitation
```

---

### **3. Row-Level Security Bypass** ⚠️ **MEDIUM RISK**
**Location**: OrgUnit filtering, RBAC implementation  
**Risk**: Horizontal privilege escalation (accessing other users' data)

**Test Focus:**
- OrgUnit filter enforcement
- Row-level security rules
- Specification filter application
- Query result validation

---

## 📊 **Security Metrics to Capture**

### **Vulnerability Metrics:**
- Number of vulnerabilities by severity (Critical, High, Medium, Low)
- Vulnerability by OWASP Top 10 category
- Time to detect vs. time to fix

### **Test Coverage:**
- % of attack vectors tested
- % of endpoints with security tests
- % of user inputs validated

### **Compliance Metrics:**
- Security controls implemented vs. required
- Audit log coverage
- Encryption coverage

---

## 🚀 **Test Execution Strategy**

### **Phase 1: Threat Modeling**
1. Identify assets and data flows
2. List potential threats (STRIDE model)
3. Prioritize by risk
4. Create attack scenarios

### **Phase 2: Automated Testing**
1. Implement security test suite
2. Run in CI/CD pipeline
3. Block deployments on critical failures

### **Phase 3: Manual Testing**
1. Penetration testing by security experts
2. Code review for security issues
3. Vulnerability scanning

### **Phase 4: Remediation**
1. Fix identified vulnerabilities
2. Re-test to verify fixes
3. Update security controls
4. Document lessons learned

---

## 📈 **Expected Deliverables**

Once requirements are defined, this folder will contain:
- ✅ Comprehensive security test suite
- ✅ OWASP Top 10 coverage
- ✅ Penetration test scenarios
- ✅ Security regression tests
- ✅ Vulnerability report templates
- ✅ Security test automation in CI/CD

---

## 🎯 **OWASP Top 10 Coverage Plan**

| OWASP Category | Test Coverage | Status |
|----------------|---------------|--------|
| **A01:2021 Broken Access Control** | RBAC, Row-level security | 🟡 Planned |
| **A02:2021 Cryptographic Failures** | Password storage, data encryption | 🟡 Planned |
| **A03:2021 Injection** | SQL, LINQ, XSS, Command injection | 🟡 Planned |
| **A04:2021 Insecure Design** | Threat modeling, secure defaults | 🟡 Planned |
| **A05:2021 Security Misconfiguration** | Headers, CORS, error handling | 🟡 Planned |
| **A06:2021 Vulnerable Components** | Dependency scanning | 🟡 Planned |
| **A07:2021 Authentication Failures** | IAP, session management | 🟡 Planned |
| **A08:2021 Data Integrity Failures** | Input validation, serialization | 🟡 Planned |
| **A09:2021 Security Logging Failures** | Audit logging, monitoring | 🟡 Planned |
| **A10:2021 SSRF** | API calls, external requests | 🟡 Planned |

---

## 📞 **Contact & Next Steps**

**Status**: Awaiting answers to questions above  
**Timeline**: TBD based on requirement gathering  
**Owner**: Security Team + QA Team

**To proceed:**
1. Answer questions in this document
2. Get security testing approval
3. Provision isolated test environment
4. Review and approve security test plan
5. Implement tests
6. Schedule penetration testing

---

**⚠️ IMPORTANT**: All security testing must be performed in isolated, non-production environments with proper authorization. Unauthorized security testing may violate laws and policies.

---

*Security test structure created: January 15, 2026*  
*Awaiting: Security testing requirements and approval*
