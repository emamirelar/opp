# DEF-001: Route Permission Guard Analysis

**Defect:** Route Permission Guard blocks access to detail pages  
**Impact:** 29 Playwright tests blocked  
**Date:** February 2, 2026

---

## Problem Summary

The `routePermissionGuard` in Angular routing is blocking access to entity detail pages even when users have valid permissions. Users are redirected to `/access-denied` (403 error page) instead of seeing the entity details.

**Affected Routes:**
- `/#/partnerships/partners/{id}`
- `/#/partnerships/contacts/{id}`
- `/#/partnerships/interactions/{id}`
- `/#/partnerships/opportunities/{id}`

---

## Root Cause Analysis

### Hypothesis 1: Permission Structure Mismatch

The guard may be checking for permissions in a format that doesn't match what the API returns.

**API Returns:**
```json
{
  "canView": true,
  "canEdit": true,
  "canDelete": false
}
```

**Guard May Expect:**
```json
{
  "permissions": ["view", "edit"],
  "role": "user"
}
```

### Hypothesis 2: Claims Not Available

The guard may be looking for specific claims in `/user/claims` that aren't being provided in the test environment.

### Hypothesis 3: Permission Endpoint Not Called

The guard may be failing before even calling the permission check endpoint.

---

## Investigation Steps

### Step 1: Review Guard Implementation

**File Location:** `UNOPS.PAO.ClientApp/src/app/core/guards/`

Check:
- How does the guard determine permission?
- What endpoint does it call?
- What does it expect in the response?

```typescript
// Example guard structure to review
@Injectable()
export class RoutePermissionGuard implements CanActivate {
  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
    // What logic is here?
    // What permissions are checked?
    // What causes the redirect to access-denied?
  }
}
```

### Step 2: Check Permission Service

**File Location:** `UNOPS.PAO.ClientApp/src/app/core/services/permission.service.ts`

Verify:
- How are permissions fetched?
- How are they cached?
- What format are they stored in?

### Step 3: Compare API Response Format

**Endpoint:** `/api/permissions/check/partnerships/*`

Test with real request:
```bash
curl -X GET "https://localhost/api/permissions/check/partnerships/partner/123" \
  -H "Authorization: Bearer {token}"
```

Compare response format with what guard expects.

### Step 4: Check User Claims

**Endpoint:** `/user/claims`

Verify claims returned include necessary data:
```json
{
  "sub": "user-id",
  "roles": ["user"],
  "permissions": ["CanViewPartners", "CanEditPartners"]
}
```

---

## Proposed Fixes

### Fix A: Update Guard to Match API Response

If the API returns `canView: true` but guard checks for `permissions.includes('view')`:

```typescript
// Before (broken)
canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
  return this.permissionService.getPermissions(entityId).pipe(
    map(perms => perms.permissions.includes('view')) // Wrong format
  );
}

// After (fixed)
canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
  return this.permissionService.getPermissions(entityId).pipe(
    map(perms => perms.canView === true) // Correct format
  );
}
```

### Fix B: Ensure Permission Endpoint is Called

Add logging to verify the endpoint is being called:

```typescript
canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
  console.log('RoutePermissionGuard: Checking permissions for', route.params);
  
  return this.permissionService.getPermissions(entityId).pipe(
    tap(perms => console.log('Permissions received:', perms)),
    map(perms => perms.canView === true)
  );
}
```

### Fix C: Handle Missing Permissions Gracefully

Instead of redirecting immediately, check if permissions are still loading:

```typescript
canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
  return this.permissionService.getPermissions(entityId).pipe(
    // Wait for permissions to load
    filter(perms => perms !== null),
    first(),
    map(perms => perms.canView === true),
    catchError(() => {
      // Log error but don't redirect immediately
      console.error('Failed to get permissions');
      return of(false);
    })
  );
}
```

---

## Testing the Fix

### Manual Test

1. Log in to the application
2. Navigate to partner list
3. Click on a partner to view details
4. **Expected:** Partner details page loads
5. **Actual (bug):** Redirect to access-denied

### Playwright Test

```typescript
test('should navigate to partner details', async ({ page }) => {
  await authenticateWithRealBackend(page, '/#/partnerships/partners');
  
  // Click first partner in list
  await page.locator('[data-testid="partner-row"]').first().click();
  
  // Should NOT redirect to access-denied
  await expect(page).not.toHaveURL(/access-denied/);
  
  // Should show partner details
  await expect(page.locator('[data-testid="partner-details"]')).toBeVisible();
});
```

---

## Impact of Fix

| Metric | Before | After (Expected) |
|--------|--------|------------------|
| Playwright Tests Passing | 76/105 (72.4%) | 105/105 (100%) |
| Detail Page Access | Blocked | Working |
| User Experience | Broken | Fixed |

---

## Files to Review/Modify

1. `UNOPS.PAO.ClientApp/src/app/core/guards/route-permission.guard.ts`
2. `UNOPS.PAO.ClientApp/src/app/core/services/permission.service.ts`
3. `UNOPS.PAO.ClientApp/src/app/app.routes.ts` (route configuration)
4. Backend: `UNOPS.PAO.Presentation/Controllers/PermissionController.cs`

---

## Estimated Effort

| Task | Effort |
|------|--------|
| Investigation | 1-2 hours |
| Fix implementation | 1-2 hours |
| Testing | 1 hour |
| **Total** | **3-5 hours** |

---

## Priority

**High** - This single bug blocks 29 tests (27.6% of Phase 1A suite) and affects user experience for all detail page navigation.

---

*This analysis should guide the development team in investigating and resolving DEF-001.*
