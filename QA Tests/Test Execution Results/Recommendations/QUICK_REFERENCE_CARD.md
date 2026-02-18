# Defect Prevention - Quick Reference Card

**UNOPS Opportunity+ System**  
**Keep this handy during development!**

---

## 🚨 The Four Defects We're Preventing

| Defect | What Happened | Prevention |
|--------|--------------|------------|
| **PNO-686** | Partner code generated as 10,000 instead of 1962 | Unit tests for edge cases |
| **PNO-680** | Export to Google Sheets failed in production | Integration tests + config validation |
| **PNO-677** | Advanced search didn't work for certain fields | Field configuration validation |
| **PNO-676** | Edited duplicate contacts still marked as duplicates | E2E tests for workflows |

**Bottom Line**: All four were preventable with proper testing!

---

## ✅ Pre-Commit Checklist

**Before submitting ANY pull request**:

- [ ] **Unit tests written** (75%+ coverage for new code)
- [ ] **Edge cases tested** (null, empty, boundary values)
- [ ] **Integration tests added** (if API endpoints changed)
- [ ] **E2E tests updated** (if user workflow affected)
- [ ] **All tests passing** locally
- [ ] **No linting errors**
- [ ] **Configuration externalized** (no hardcoded values)
- [ ] **Error handling implemented**
- [ ] **Logging added** for troubleshooting
- [ ] **Documentation updated**

**If ANY checkbox is unchecked → DO NOT SUBMIT PR**

---

## 🧪 Test Coverage Requirements

| Layer | Minimum | What to Test |
|-------|---------|-------------|
| **Business Logic** | 80% | All manager methods, edge cases, validations |
| **API Controllers** | 70% | All endpoints, error responses |
| **Frontend Components** | 75% | User interactions, state changes |
| **Critical Workflows** | 100% | E2E tests, no exceptions |

**Overall Application**: **75% minimum**

---

## 🎯 Testing Pyramid (What to Write)

```
┌──────────────────────────────┐
│    E2E Tests (~5%)           │  ← 20-30 tests for critical workflows
│    Critical user journeys    │
├──────────────────────────────┤
│  Integration Tests (~25%)    │  ← 100-150 tests for API endpoints
│  API + Database testing      │
├──────────────────────────────┤
│   Unit Tests (~70%)          │  ← 300-500 tests for business logic
│   Fast, isolated testing     │
└──────────────────────────────┘
```

**Bottom Heavy = Healthy Test Suite**

---

## 🔴 Common Mistakes to Avoid

| ❌ DON'T | ✅ DO INSTEAD |
|----------|---------------|
| Skip tests because "it's simple" | Write tests for everything |
| Hardcode configuration values | Use environment variables |
| Assume it works if it passes locally | Test in CI/CD environment |
| Only test happy path | Test edge cases and errors |
| Write tests after code | Write tests alongside code (TDD) |
| Ignore linting errors | Fix all linting errors |
| Skip integration tests | Test external dependencies |
| Merge PRs with failing tests | Fix ALL tests before merge |

---

## 🏗️ Test Templates (Copy & Use)

### Unit Test Template

```csharp
[Fact]
public async Task MethodName_Should_ExpectedBehavior_When_Condition()
{
    // Arrange: Setup test data
    var testData = new TestObject { /* ... */ };
    
    // Act: Execute method
    var result = await _sut.MethodAsync(testData);
    
    // Assert: Verify behavior
    result.Should().NotBeNull();
    result.Property.Should().Be(expectedValue);
}
```

### Integration Test Template

```csharp
[Fact]
public async Task ApiEndpoint_Should_Return_StatusCode_When_Condition()
{
    // Arrange: Setup database
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // ... seed data
    
    // Act: Call API
    var response = await _client.PostAsJsonAsync("/api/endpoint", request);
    
    // Assert: Verify response and DB
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### E2E Test Template

```typescript
test('should complete workflow successfully', async ({ page }) => {
  // Navigate
  await page.goto('/feature');
  
  // Interact
  await page.click('button:has-text("Action")');
  
  // Verify
  await expect(page.locator('.result')).toBeVisible();
});
```

---

## 🚫 Testing Anti-Patterns

**Watch out for these!**

1. **No Tests** - "I tested it manually" ❌
   - Manual testing doesn't prevent regressions

2. **Testing Implementation Details** - Testing method calls instead of behavior ❌
   - Test what users experience, not how code works internally

3. **Shared Test State** - Tests depend on each other ❌
   - Each test must be independent

4. **Too Many Mocks** - Everything is mocked ❌
   - Use real dependencies where practical (especially in integration tests)

5. **Slow Tests** - Tests take minutes to run ❌
   - Unit tests should run in milliseconds

6. **Flaky Tests** - Tests pass/fail randomly ❌
   - Fix immediately, never ignore

---

## ⚡ Quick Commands

### Run Tests

```bash
# .NET - All tests
dotnet test

# .NET - Specific project
dotnet test UNOPS.PAO.Business.Tests

# .NET - With coverage
dotnet test /p:CollectCoverage=true

# Angular - All tests
npm test

# Angular - With coverage
npm run test:coverage

# E2E tests
npx playwright test
```

### Check Coverage

```bash
# Generate coverage report
dotnet test /p:CollectCoverage=true
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"

# Open report (Windows)
start coveragereport/index.html
```

### Run Linting

```bash
# .NET
dotnet format

# Angular
npm run lint
npm run lint:fix
```

---

## 🎓 Test Naming Convention

**Pattern**: `MethodName_Should_ExpectedBehavior_When_Condition`

**Examples**:
- ✅ `GetContact_Should_Return_NotFound_When_Contact_Does_Not_Exist`
- ✅ `ApprovePartner_Should_Generate_Sequential_ErpDimValue`
- ✅ `CreateContact_Should_Throw_When_Email_Is_Duplicate`
- ❌ `Test1` (too vague)
- ❌ `TestGetContact` (doesn't describe behavior)

---

## 🔍 What to Test (Examples from Our Defects)

### Business Logic (PNO-686)

```csharp
[Fact]
public async Task GetNextErpDimValue_Should_Skip_Reserved_Range()
{
    // Test that 8000-9999 is excluded
}

[Theory]
[InlineData(7999, 8000)]  // Before reserved
[InlineData(9999, 10000)] // After reserved
public async Task GetNextErpDimValue_Should_Handle_Boundaries(int existing, int expected)
{
    // Test boundary conditions
}
```

### Configuration (PNO-680)

```csharp
[Fact]
public void Startup_Should_Fail_When_Google_ClientId_Missing()
{
    // Test configuration validation
}

[Fact]
public async Task Export_Should_Return_BadRequest_When_Service_Unavailable()
{
    // Test external service error handling
}
```

### Field Configuration (PNO-677)

```csharp
[Theory]
[InlineData("pooledFund", true)]
[InlineData("keyGlobalPartner", false)]
public async Task AdvancedSearch_Should_Filter_By_Boolean_Fields(string field, bool value)
{
    // Test boolean field search
}
```

### UI Workflows (PNO-676)

```typescript
test('should update duplicate status after edit', async ({ page }) => {
  // Test that editing clears duplicate flag
});
```

---

## 📊 Success Metrics (Track Weekly)

| Metric | Target | How to Check |
|--------|--------|--------------|
| **Code Coverage** | 75%+ | Coverage reports in CI/CD |
| **Build Success** | 95%+ | CI/CD dashboard |
| **Test Execution Time** | < 10 min | CI/CD logs |
| **Failing Tests** | 0 | CI/CD results |
| **Linting Errors** | 0 | Lint reports |

---

## 🆘 When You Need Help

### Test is Too Slow
- Move to integration tests if testing multiple layers
- Use mocks for external dependencies
- Check for N+1 queries or inefficient operations

### Test is Flaky
- Look for timing issues (use proper waits in E2E)
- Check for shared state between tests
- Verify test isolation

### Coverage is Low
- Identify untested methods: `dotnet test /p:CollectCoverage=true`
- Focus on critical business logic first
- Add tests for edge cases

### Don't Know What to Test
- Test happy path first
- Add tests for error conditions
- Test boundary values (null, empty, min, max)
- Test edge cases specific to business rules

---

## 💡 Pro Tips

1. **Write Tests First** (TDD)
   - Clarifies requirements
   - Ensures testable design
   - Catches bugs earlier

2. **Test Edge Cases**
   - Null values
   - Empty strings/arrays
   - Boundary values (0, -1, max)
   - Reserved ranges (like 8000-9999)

3. **Keep Tests Simple**
   - One assertion per test (usually)
   - Clear arrange-act-assert structure
   - Descriptive test names

4. **Use Test Data Factories**
   - Create reusable test data generators
   - Reduces test setup code
   - Ensures consistency

5. **Mock External Dependencies**
   - Database calls (unit tests)
   - API calls
   - File system
   - Time (use TimeProvider)

---

## 🎯 This Week's Focus

**Phase 1 Priorities**:

1. **Day 1-2**: Add unit tests for partner approval
2. **Day 3**: Implement configuration validation
3. **Day 4-5**: Fix duplicate detection + advanced search
4. **Day 6**: Set up coverage reporting

**Remember**: Small, consistent progress beats perfection!

---

## 📞 Resources

- **Full Analysis**: `DEFECT_ANALYSIS_AND_PREVENTION_RECOMMENDATIONS.md`
- **Action Plan**: `IMPLEMENTATION_ACTION_PLAN.md`
- **Executive Summary**: `EXECUTIVE_SUMMARY.md`
- **Backend Testing Guide**: `docs/Development/BACKEND_TESTING_GUIDE.md`

---

## 🎬 Daily Reminder

**Before you start coding today**:
- [ ] Which tests will I write?
- [ ] What edge cases exist?
- [ ] Are there configuration dependencies?
- [ ] What could go wrong?

**Before you finish coding today**:
- [ ] Did I write the tests?
- [ ] Do all tests pass?
- [ ] Is coverage adequate?
- [ ] Did I run linting?

**"Code without tests is broken by design."** - Jacob Kaplan-Moss

---

**Print this card and keep it visible while coding!** 🚀

