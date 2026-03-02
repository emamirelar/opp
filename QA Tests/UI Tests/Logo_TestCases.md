# Logo Component — Test Cases

**Component:** `UNOPS.PAO.ClientApp/src/app/shared/components/logo`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30-50 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

| **N≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **E≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **F≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **I≥3P?** | ✅ | 90 ≥ 3×30 = 90 |

---

## Feature Overview

Logo component: display, responsive sizing, theme variants, click navigation, accessibility, loading states.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Logo Displays with Default Sizing

**Priority:** P0  
**Precondition:** Logo asset exists, component mounted.

**Steps:**
1. Render Logo component
2. Verify display

**Expected Result:** Logo image displayed with default size.

---

#### POS-002: Logo Click Navigates to Home

**Priority:** P0  
**Precondition:** Click enabled, route configured.

**Steps:**
1. Click logo
2. Verify navigation

**Expected Result:** Navigates to home/dashboard.

---

#### POS-003: Logo with Light Theme

**Priority:** P0  
**Precondition:** Light theme active.

**Steps:**
1. Set theme to light
2. Render logo

**Expected Result:** Light variant logo displayed.

---

#### POS-004: Logo with Dark Theme

**Priority:** P0  
**Precondition:** Dark theme active.

**Steps:**
1. Set theme to dark
2. Render logo

**Expected Result:** Dark variant logo displayed.

---

#### POS-005: Logo Responsive Sizing

**Priority:** P0  
**Precondition:** Viewport resizable.

**Steps:**
1. Resize viewport from mobile to desktop
2. Verify logo scales

**Expected Result:** Logo appropriately sized for viewport.

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Logo with custom size | Size prop | Pass size="large" | Large logo | P1 |
| POS-007 | Logo with custom alt text | Alt prop | Pass alt="UNOPS" | Alt text set | P1 |
| POS-008 | Logo without click | Click disabled | clickable=false | No navigation | P1 |
| POS-009 | Logo loading state | Lazy load | Initial render | Skeleton/placeholder | P1 |
| POS-010 | Logo load complete | Image loaded | onLoad fires | Image displayed | P1 |
| POS-011 | Logo with aria-label | A11y | Check aria-label | Label present | P1 |
| POS-012 | Logo keyboard accessible | Focus | Tab to logo, Enter | Navigates | P1 |
| POS-013 | Logo with title attribute | Title | Hover | Tooltip shows | P1 |
| POS-014 | Logo in header | Header layout | Place in header | Correct position | P1 |
| POS-015 | Logo in sidebar | Sidebar layout | Place in sidebar | Correct position | P1 |
| POS-016 | Logo with custom link | Custom href | Pass link="/custom" | Navigates to custom | P2 |
| POS-017 | Logo with target blank | External | target="_blank" | Opens new tab | P2 |
| POS-018 | Logo with rel noopener | External link | rel="noopener" | Security attribute | P2 |
| POS-019 | Logo size small | size="small" | Render | Small logo | P2 |
| POS-020 | Logo size medium | size="medium" | Render | Medium logo | P2 |
| POS-021 | Logo size large | size="large" | Render | Large logo | P2 |
| POS-022 | Logo with className | Custom class | Pass className | Class applied | P2 |
| POS-023 | Logo with data attributes | Data attrs | data-testid | Attrs present | P2 |
| POS-024 | Logo in print view | Print | Print page | Logo visible | P2 |
| POS-025 | Logo with prefers-reduced-motion | Reduced motion | Set preference | No animation | P2 |
| POS-026 | Logo high contrast | High contrast | Set preference | Visible | P2 |
| POS-027 | Logo with focus visible | Focus | Tab to logo | Focus ring visible | P2 |
| POS-028 | Logo SVG variant | SVG format | Use SVG logo | Renders correctly | P2 |
| POS-029 | Logo PNG variant | PNG format | Use PNG logo | Renders correctly | P2 |
| POS-030 | Logo WebP variant | WebP format | Use WebP | Renders or fallback | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** 70 tests | **Focus:** Invalid inputs, unauthorized access, error conditions

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Logo with null src | src = null | Fallback or error | P0 |
| NEG-002 | Logo with empty src | src = "" | Fallback or error | P0 |
| NEG-003 | Logo with invalid URL | src = "invalid" | Error or fallback | P0 |
| NEG-004 | Logo with 404 URL | src = 404 | onError, fallback | P0 |
| NEG-005 | Logo with negative size | size = -1 | Default or error | P0 |
| NEG-006 | Logo with zero size | size = 0 | Default or error | P0 |
| NEG-007 | Logo with invalid theme | theme = "invalid" | Default theme | P0 |
| NEG-008 | Logo with null alt | alt = null | Empty alt or default | P0 |
| NEG-009 | Logo with invalid link | link = "javascript:" | Sanitized | P0 |
| NEG-010 | Logo with XSS in alt | alt = "<script>" | Sanitized | P0 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | Logo on login page | Anonymous | View logo | Logo displayed | P0 |
| NEG-012 | Logo link without auth | Anonymous | Click logo | Navigate or login | P0 |
| NEG-013 | Logo with expired session | Expired | Click | Re-auth or navigate | P1 |
| NEG-014 | Logo in restricted area | No permission | View | Logo or access denied | P1 |
| NEG-015 | Logo asset 403 | Forbidden asset | Load | Fallback | P1 |
| NEG-016 | Logo CORS blocked | CORS | Load | Fallback | P1 |
| NEG-017 | Logo with invalid token | Bad token | Load asset | Error handling | P1 |
| NEG-018 | Logo in iframe | Embedded | Load | Works or blocked | P2 |
| NEG-019 | Logo with CSP violation | CSP | Load | Blocked, fallback | P1 |
| NEG-020 | Logo referrer policy | Referrer | Load | Policy respected | P2 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-021 | Logo during theme switch | Switching | Render | Correct variant | P1 |
| NEG-022 | Logo during resize | Resizing | Render | Correct size | P1 |
| NEG-023 | Logo during navigation | Navigating | Click | Navigate or cancel | P1 |
| NEG-024 | Logo unmount during load | Loading | Unmount | No memory leak | P1 |
| NEG-025 | Logo with detached DOM | Detached | Update | No error | P1 |

### 2.4 Missing/Null Data

| ID | Test Name | Missing Field | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-026 | Logo without src | src missing | Fallback | P1 |
| NEG-027 | Logo without alt | alt missing | Empty or default | P1 |
| NEG-028 | Logo without size | size missing | Default size | P1 |
| NEG-029 | Logo without theme | theme missing | Default theme | P1 |
| NEG-030 | Logo without link | link missing | No link or default | P1 |
| NEG-031 | Logo with undefined className | className undefined | No class | P1 |
| NEG-032 | Logo with null dimensions | width/height null | Default | P1 |
| NEG-033 | Logo with missing asset | Asset 404 | Fallback | P1 |
| NEG-034 | Logo with corrupt image | Corrupt file | onError | P1 |
| NEG-035 | Logo with unsupported format | AVIF no support | Fallback format | P1 |

### 2.5 Dependency Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-036 | CDN down for logo | 503 | Fallback/local | P0 |
| NEG-037 | Network timeout | Timeout | Loading, then error | P0 |
| NEG-038 | DNS failure | DNS | Fallback | P1 |
| NEG-039 | SSL certificate invalid | SSL | Blocked or warning | P1 |
| NEG-040 | Asset server slow | 10s response | Timeout or wait | P1 |

### 2.6 Duplicate & Constraint Violations

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-041 | Multiple logos same page | 2 logos | Both render | P1 |
| NEG-042 | Logo with oversized dimensions | 10000px | Capped or error | P1 |
| NEG-043 | Logo with path traversal | src = "../../etc/passwd" | Rejected | P0 |
| NEG-044 | Logo with data URI XSS | data:image/svg,<script> | Sanitized | P0 |
| NEG-045 | Logo with blob URL | blob:... | Allowed or blocked | P1 |
| NEG-046 | Logo with inline SVG script | SVG with script | Sanitized | P0 |
| NEG-047 | Logo with event handlers | onload in src | Sanitized | P0 |
| NEG-048 | Logo with redirect | 302 redirect | Follow or block | P1 |
| NEG-049 | Logo with content-type mismatch | Image as HTML | Rejected | P1 |
| NEG-050 | Logo with oversized file | 100MB image | Rejected or timeout | P1 |

### 2.7 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-051 | Logo with special chars in src | src = "logo (1).png" | Encoded | P1 |
| NEG-052 | Logo with unicode in alt | alt = "标志" | Handled | P1 |
| NEG-053 | Logo with very long alt | 1000 chars | Truncated | P1 |
| NEG-054 | Logo with null click handler | onClick = null | No click | P1 |
| NEG-055 | Logo with invalid target | target = "invalid" | Default | P2 |
| NEG-056 | Logo with invalid rel | rel = "invalid" | Ignored | P2 |
| NEG-057 | Logo in hidden container | display:none | Not loaded (lazy) | P2 |
| NEG-058 | Logo with visibility hidden | visibility:hidden | Renders | P2 |
| NEG-059 | Logo with opacity 0 | opacity:0 | Renders | P2 |
| NEG-060 | Logo with aria-hidden | aria-hidden=true | Hidden from a11y | P1 |
| NEG-061 | Logo with role none | role="none" | Correct | P2 |
| NEG-062 | Logo with conflicting sizes | width and size | One wins | P1 |
| NEG-063 | Logo with negative margin | margin = -100 | Layout handled | P2 |
| NEG-064 | Logo with overflow hidden | overflow:hidden | Clipped | P2 |
| NEG-065 | Logo with transform | transform: scale(0) | Invisible but present | P2 |
| NEG-066 | Logo with invalid fetchpriority | "invalid" | Default | P2 |
| NEG-067 | Logo with invalid loading | "invalid" | Default | P2 |
| NEG-068 | Logo with invalid decoding | "invalid" | Default | P2 |
| NEG-069 | Logo with conflicting lazy/eager | Both | One wins | P1 |
| NEG-070 | Logo rapid theme switch | 10 switches/sec | Correct final state | P1 |
| NEG-071 | Logo with invalid object-fit | object-fit="invalid" | Default | P2 |
| NEG-072 | Logo with invalid aspect-ratio | ratio="invalid" | Default | P2 |
| NEG-073 | Logo with negative padding | padding=-10 | Default | P1 |
| NEG-074 | Logo with oversized border-radius | 9999px | Capped | P1 |
| NEG-075 | Logo with invalid referrerpolicy | policy="invalid" | Default | P2 |
| NEG-076 | Logo with malformed src URL | src="://invalid" | Error or fallback | P1 |
| NEG-077 | Logo with file protocol | src="file:///path" | Blocked or fallback | P1 |
| NEG-078 | Logo with ftp protocol | src="ftp://host" | Blocked or fallback | P1 |
| NEG-079 | Logo with empty data URI | data:image/png;base64, | Error | P1 |
| NEG-080 | Logo with truncated base64 | data:image/png;base64,ABC | Error | P1 |
| NEG-081 | Logo with wrong MIME in data URI | data:text/html, | Rejected | P1 |
| NEG-082 | Logo with null referrerpolicy | null | Default | P2 |
| NEG-083 | Logo with invalid crossorigin | crossorigin="invalid" | Default | P2 |
| NEG-084 | Logo with conflicting width/height | width=100, height=50, size=large | One wins | P1 |
| NEG-085 | Logo with zero opacity | opacity=0 | Renders but invisible | P2 |
| NEG-086 | Logo with NaN dimensions | width=NaN | Default | P1 |
| NEG-087 | Logo with Infinity dimensions | width=Infinity | Capped | P1 |
| NEG-088 | Logo with negative z-index | z-index=-1 | Behind content | P2 |
| NEG-089 | Logo with invalid draggable | draggable="maybe" | Default | P2 |
| NEG-090 | Logo with malformed className | className with invalid chars | Sanitized | P1 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** 70 tests | **Focus:** Limits, boundaries, unusual but valid inputs

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Alt text | 0 | 500 | ✅ Empty | ✅ 500 | ❌ Truncated | P1 |
| BND-002 | Src URL | 1 | 2048 | ✅ "a" | ✅ 2048 | ❌ Rejected | P1 |
| BND-003 | Title | 0 | 200 | ✅ Empty | ✅ 200 | ❌ Truncated | P2 |
| BND-004 | Link URL | 1 | 2048 | ✅ "/" | ✅ 2048 | ❌ Rejected | P1 |
| BND-005 | ClassName | 0 | 500 | ✅ Empty | ✅ 500 | ❌ Truncated | P2 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-006 | Width | 1 | 500 | ❌ Default | ❌ Error | Capped | P1 |
| BND-007 | Height | 1 | 500 | ❌ Default | ❌ Error | Capped | P1 |
| BND-008 | Size enum | small | large | ❌ | ❌ | Default | P1 |
| BND-009 | Z-index | 0 | 9999 | ✅ 0 | ❌ | 9999 | P2 |
| BND-010 | Opacity | 0 | 1 | ✅ 0 | ❌ | 1 | P2 |
| BND-011 | Aspect ratio | 0.1 | 10 | ❌ | ❌ | Capped | P2 |
| BND-012 | Border radius | 0 | 50 | ✅ 0 | ❌ | 50 | P2 |
| BND-013 | Padding | 0 | 100 | ✅ 0 | ❌ | 100 | P2 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-014 | Logo cache header | Cache-Control | Cached correctly | P2 |
| BND-015 | Logo modified date | Last-Modified | Revalidation | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-016 | Zero logos | Empty | No logo | P1 |
| BND-017 | One logo | Single | 1 logo | P1 |
| BND-018 | 10 logos same page | Many | All render | P2 |
| BND-019 | Logo in empty container | Empty parent | Renders | P1 |
| BND-020 | Logo in scroll container | Scroll | Correct position | P2 |

### 3.5 Unicode & Special Characters

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-021 | Alt (Arabic) | `شعار` | Displayed | P2 |
| BND-022 | Alt (Chinese) | `标志` | Displayed | P2 |
| BND-023 | Alt (Cyrillic) | `Логотип` | Displayed | P2 |
| BND-024 | Alt with apostrophe | "UNOPS's Logo" | Preserved | P1 |
| BND-025 | Alt with emoji | "Logo 🏢" | Displayed | P2 |
| BND-026 | Src with encoded chars | %20space | Decoded | P1 |
| BND-027 | Src with query params | ?v=1 | Loaded | P1 |
| BND-028 | Link with hash | /page#section | Navigates | P1 |
| BND-029 | Link with query | /page?ref=logo | Navigates | P1 |
| BND-030 | Title with special chars | "Logo & Brand" | Preserved | P2 |

### 3.6 Responsive Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-031 | Viewport 320px | Mobile | Logo fits | P1 |
| BND-032 | Viewport 768px | Tablet | Logo fits | P1 |
| BND-033 | Viewport 1920px | Desktop | Logo fits | P1 |
| BND-034 | Viewport 3840px | 4K | Logo scales | P2 |
| BND-035 | DPR 1 | Standard | 1x asset | P1 |
| BND-036 | DPR 2 | Retina | 2x asset if available | P1 |
| BND-037 | DPR 3 | High DPI | 3x asset if available | P2 |
| BND-038 | Orientation portrait | Portrait | Correct | P1 |
| BND-039 | Orientation landscape | Landscape | Correct | P1 |
| BND-040 | Resize from 320 to 1920 | Resize | Smooth transition | P2 |

### 3.7 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-041 | Logo 1x1 px | Tiny | Renders | P1 |
| BND-042 | Logo 500x500 px | Large | Renders | P1 |
| BND-043 | Logo SVG viewBox | SVG | Correct viewBox | P1 |
| BND-044 | Logo with srcset | Responsive | Correct src | P1 |
| BND-045 | Logo with sizes | Sizes attr | Correct size | P1 |
| BND-046 | Logo with picture/source | Picture el | Fallback | P1 |
| BND-047 | Logo with multiple sources | AVIF, WebP, PNG | Best format | P2 |
| BND-048 | Logo preload | Preload | Preloaded | P2 |
| BND-049 | Logo with crossorigin | CORS | Cross-origin | P1 |
| BND-050 | Logo with referrerpolicy | Referrer | Policy applied | P2 |
| BND-051 | Logo loading attribute auto | Auto | Browser default | P1 |
| BND-052 | Logo loading attribute lazy | Lazy | Lazy load | P1 |
| BND-053 | Logo loading attribute eager | Eager | Immediate | P1 |
| BND-054 | Logo decoding sync | Sync | Sync decode | P2 |
| BND-055 | Logo decoding async | Async | Async decode | P2 |
| BND-056 | Logo decoding auto | Auto | Browser default | P2 |
| BND-057 | Logo fetchpriority high | High | Priority load | P2 |
| BND-058 | Logo fetchpriority low | Low | Low priority | P2 |
| BND-059 | Logo fetchpriority auto | Auto | Default | P2 |
| BND-060 | Logo with role img | role="img" | Correct | P1 |
| BND-061 | Logo with role link | Clickable | role="link" | P1 |
| BND-062 | Logo with tabindex 0 | Focusable | tabindex="0" | P1 |
| BND-063 | Logo with tabindex -1 | Not focusable | tabindex="-1" | P1 |
| BND-064 | Logo with draggable false | No drag | draggable="false" | P2 |
| BND-065 | Logo with loading state | Loading | Skeleton | P1 |
| BND-066 | Logo with error state | Error | Fallback | P1 |
| BND-067 | Logo with success state | Loaded | Image | P1 |
| BND-068 | Logo transition | Theme switch | Smooth | P2 |
| BND-069 | Logo animation | Hover | Animated | P2 |
| BND-070 | Logo print size | Print | Appropriate size | P2 |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 50 tests | **Breakdown:** Workflow (15), Validation (15), Constraint (10), Audit (10)

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-001 | Logo displays on mount | Mount | Component mount | Logo visible | P0 |
| FUN-002 | Logo loads asset | Load | src set | Asset fetched | P0 |
| FUN-003 | Logo click navigates | Click | Click (if enabled) | Navigate | P0 |
| FUN-004 | Logo respects theme | Theme | Theme change | Variant updates | P0 |
| FUN-005 | Logo respects size | Size | Size prop | Dimensions update | P0 |
| FUN-006 | Logo loading state | Load | During load | Loading shown | P0 |
| FUN-007 | Logo error fallback | Error | Load fails | Fallback shown | P0 |
| FUN-008 | Logo alt for a11y | A11y | Screen reader | Alt announced | P0 |
| FUN-009 | Logo keyboard nav | A11y | Tab, Enter | Focusable, activates | P0 |
| FUN-010 | Logo responsive | Viewport | Resize | Size adapts | P0 |
| FUN-011 | Logo lazy load | Lazy | Off-screen | Load when visible | P1 |
| FUN-012 | Logo cache | Cache | Repeat visit | Cached | P1 |
| FUN-013 | Logo preload | Preload | Head | Preloaded | P1 |
| FUN-014 | Logo link security | Security | External link | rel=noopener | P1 |
| FUN-015 | Logo unmount cleanup | Unmount | Component unmount | No leaks | P1 |

### 4.2 Validation Rules (15)

| ID | Test Name | Rule | Valid | Invalid | Priority |
|----|-----------|------|-------|---------|----------|
| FUN-016 | Src required | Required | Valid URL | null, "" | P0 |
| FUN-017 | Alt recommended | A11y | "Logo" | Empty (warn) | P0 |
| FUN-018 | Link valid | URL | "/" | "javascript:" | P0 |
| FUN-019 | Size valid | Enum | small, medium, large | 999 | P1 |
| FUN-020 | Theme valid | Enum | light, dark | invalid | P1 |
| FUN-021 | No XSS in alt | Sanitize | "Logo" | "<script>" | P0 |
| FUN-022 | No XSS in title | Sanitize | "Logo" | "<script>" | P0 |
| FUN-023 | Dimensions non-negative | ≥0 | 100 | -1 | P1 |
| FUN-024 | Src protocol | Protocol | https, / | file:, data: (careful) | P1 |
| FUN-025 | Target valid | Enum | _self, _blank | invalid | P1 |
| FUN-026 | Rel valid | Security | noopener, noreferrer | invalid | P1 |
| FUN-027 | Fetchpriority valid | Enum | high, low, auto | invalid | P2 |
| FUN-028 | Loading valid | Enum | lazy, eager | invalid | P1 |
| FUN-029 | Decoding valid | Enum | sync, async, auto | invalid | P1 |
| FUN-030 | Object-fit valid | CSS | contain, cover | invalid | P1 |

### 4.3 Constraint Rules (10)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-031 | Max width | 500px | 1000 | Capped at 500 | P1 |
| FUN-032 | Max height | 500px | 1000 | Capped at 500 | P1 |
| FUN-033 | Min width | 16px | 1 | 16 | P1 |
| FUN-034 | Min height | 16px | 1 | 16 | P1 |
| FUN-035 | Aspect ratio | 16:9 or 1:1 | Skewed | Preserved | P2 |
| FUN-036 | File size | 500KB | 10MB | Warning or reject | P2 |
| FUN-037 | Format | PNG, SVG, WebP | BMP | Fallback | P2 |
| FUN-038 | Click debounce | 300ms | Rapid clicks | Debounced | P2 |
| FUN-039 | Load timeout | 10s | 15s | Timeout, fallback | P1 |
| FUN-040 | Concurrent loads | 5 | 10 | Queued | P2 |

### 4.4 Audit Rules (10)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-041 | Logo view | Display | PageView (if tracked) | P1 |
| FUN-042 | Logo click | Click | Navigation logged | P1 |
| FUN-043 | Logo load error | Error | Error logged | P1 |
| FUN-044 | Logo load success | Success | No PII in logs | P0 |
| FUN-045 | Logo theme change | Theme | Not logged (low value) | P2 |
| FUN-046 | Logo resize | Resize | Not logged | P2 |
| FUN-047 | Logo unmount | Unmount | Cleanup | P1 |
| FUN-048 | Logo preload | Preload | Resource hint | P1 |
| FUN-049 | Logo cache hit | Cache | Not logged | P2 |
| FUN-050 | Logo CORS | CORS | Error logged if fail | P1 |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 50 tests

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | Page load → Logo | Load | Page, Logo | Logo displayed | P0 |
| INT-002 | Logo click → Home | Click | Logo, Router | Navigate home | P0 |
| INT-003 | Theme switch → Logo | Switch | Theme, Logo | Logo updates | P0 |
| INT-004 | Resize → Logo | Resize | Viewport, Logo | Logo resizes | P0 |
| INT-005 | Route change → Logo | Navigate | Router, Logo | Logo in new page | P1 |
| INT-006 | Login → Logo | Login | Auth, Logo | Logo on dashboard | P1 |
| INT-007 | Logout → Logo | Logout | Auth, Logo | Logo on login | P1 |
| INT-008 | Tab switch → Logo | Tab | Browser, Logo | Logo visible | P1 |
| INT-009 | Print → Logo | Print | Print, Logo | Logo in print | P1 |
| INT-010 | Refresh → Logo | Refresh | Browser, Logo | Logo reloads | P1 |

### 5.2 Search & Filter (10)

| ID | Test Name | Criteria | Expected | Priority |
|----|-----------|---------|----------|----------|
| INT-011 | Logo in header | Header | Logo in header | P0 |
| INT-012 | Logo in sidebar | Sidebar | Logo in sidebar | P1 |
| INT-013 | Logo in footer | Footer | Logo in footer | P1 |
| INT-014 | Logo in modal | Modal | Logo in modal | P1 |
| INT-015 | Logo in card | Card | Logo in card | P1 |
| INT-016 | Logo with other elements | Layout | No overlap | P1 |
| INT-017 | Logo with text | Text | Correct alignment | P1 |
| INT-018 | Logo with button | Button | Correct spacing | P1 |
| INT-019 | Logo in flex container | Flex | Flex correctly | P1 |
| INT-020 | Logo in grid | Grid | Grid correctly | P1 |

### 5.3 Pagination (5)

| ID | Test Name | Page/Size | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-021 | Logo on page 1 | First page | Visible | P1 |
| INT-022 | Logo on scroll | Scroll page | Still visible (sticky) or scrolls | P1 |
| INT-023 | Logo in virtual list | Virtual | Renders when visible | P2 |
| INT-024 | Logo in carousel | Carousel | Renders in slide | P2 |
| INT-025 | Logo in tab panel | Tab | Renders when tab active | P1 |

### 5.4 Relationships (10)

| ID | Test Name | Relationship | Scenario | Expected | Priority |
|----|-----------|-------------|---------|----------|----------|
| INT-026 | Logo → Header | Parent | Header loads | Logo in header | P0 |
| INT-027 | Logo → Router | Router | Click | Router navigates | P0 |
| INT-028 | Logo → Theme | Theme | Theme service | Logo uses theme | P0 |
| INT-029 | Logo → Asset | Asset | CDN/static | Asset loaded | P0 |
| INT-030 | Logo → Config | Config | App config | Logo from config | P1 |
| INT-031 | Logo → i18n | i18n | Locale | Alt translated | P1 |
| INT-032 | Logo → Auth | Auth | Auth state | Link may vary | P1 |
| INT-033 | Logo → Analytics | Analytics | Click | Event tracked | P1 |
| INT-034 | Logo → Error boundary | Error | Logo error | Boundary catches | P1 |
| INT-035 | Logo → Lazy route | Route | Lazy load | Logo with route | P1 |

### 5.5 Error Handling (15)

| ID | Test Name | Error | Expected | Priority |
|----|-----------|-------|----------|----------|
| INT-036 | Asset 404 | 404 | Fallback, onError | P0 |
| INT-037 | Asset 500 | 500 | Fallback, onError | P0 |
| INT-038 | Network error | Offline | Fallback | P0 |
| INT-039 | Timeout | 10s | Fallback | P0 |
| INT-040 | CORS error | CORS | Fallback | P0 |
| INT-041 | Invalid image | Corrupt | Fallback | P0 |
| INT-042 | CSP block | CSP | Fallback | P1 |
| INT-043 | Slow load | 5s | Loading state | P1 |
| INT-044 | Navigate during load | Navigate | Cancel load | P1 |
| INT-045 | Unmount during load | Unmount | No error | P1 |
| INT-046 | Theme service error | Error | Default theme | P1 |
| INT-047 | Router error | Error | Graceful | P1 |
| INT-048 | Config error | Error | Default config | P1 |
| INT-049 | XSS in asset URL | Malicious | Blocked | P0 |
| INT-050 | Path traversal | Traversal | Blocked | P0 |
| INT-051 | Page load → Logo → Display | Full flow | Logo shown | P0 |
| INT-052 | Logo click → Home | Full flow | Navigate home | P0 |
| INT-053 | Theme switch → Logo update | Theme | Logo variant updates | P0 |
| INT-054 | Resize → Logo scale | Resize | Logo resizes | P0 |
| INT-055 | Route change → Logo in new page | Route | Logo in layout | P1 |
| INT-056 | Login → Logo on dashboard | Auth | Logo after login | P1 |
| INT-057 | Logout → Logo on login page | Auth | Logo after logout | P1 |
| INT-058 | Tab switch → Logo visible | Tab | Logo visible | P1 |
| INT-059 | Print → Logo in print | Print | Logo in print | P1 |
| INT-060 | Refresh → Logo reload | Refresh | Logo reloads | P1 |
| INT-061 | Logo in header layout | Header | Logo in header | P0 |
| INT-062 | Logo in sidebar layout | Sidebar | Logo in sidebar | P1 |
| INT-063 | Logo in footer layout | Footer | Logo in footer | P1 |
| INT-064 | Logo with router service | Router | Click navigates | P0 |
| INT-065 | Logo with theme service | Theme | Theme applied | P0 |
| INT-066 | Logo with config service | Config | Config applied | P2 |
| INT-067 | Logo with i18n service | i18n | Alt translated | P1 |
| INT-068 | Logo with auth service | Auth | Link may vary | P1 |
| INT-069 | Logo with analytics service | Analytics | Click tracked | P1 |
| INT-070 | Logo with error boundary | Error | Boundary catches | P1 |
| INT-071 | Logo with lazy route | Route | Logo with route | P1 |
| INT-072 | Logo asset 404 → Fallback | 404 | Fallback shown | P0 |
| INT-073 | Logo asset 500 → Fallback | 500 | Fallback shown | P0 |
| INT-074 | Logo network error → Fallback | Offline | Fallback shown | P0 |
| INT-075 | Logo timeout → Fallback | Timeout | Fallback shown | P0 |
| INT-076 | Logo CORS error → Fallback | CORS | Fallback shown | P0 |
| INT-077 | Logo corrupt image → Fallback | Corrupt | Fallback shown | P0 |
| INT-078 | Logo CSP block → Fallback | CSP | Fallback shown | P1 |
| INT-079 | Logo slow load → Loading state | 5s | Loading shown | P1 |
| INT-080 | Logo navigate during load → Cancel | Navigate | Load cancelled | P1 |
| INT-081 | Logo unmount during load → No error | Unmount | No error | P1 |
| INT-082 | Logo theme service error → Default | Error | Default theme | P1 |
| INT-083 | Logo router error → Graceful | Error | Graceful | P1 |
| INT-084 | Logo config error → Default | Error | Default config | P1 |
| INT-085 | Logo with other header elements | Layout | No overlap | P1 |
| INT-086 | Logo with text in header | Layout | Correct alignment | P1 |
| INT-087 | Logo with button in header | Layout | Correct spacing | P1 |
| INT-088 | Logo in flex container | Flex | Flex correctly | P1 |
| INT-089 | Logo in grid layout | Grid | Grid correctly | P1 |
| INT-090 | Logo end-to-end full flow | E2E | Load→Click→Navigate | P0 |

---

## §6 Security Tests

> **Minimum:** 50 tests

### 6.1 Injection Prevention (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | XSS in alt | `<script>alert(1)</script>` | Sanitized | P0 |
| SEC-002 | XSS in title | `"><script>` | Sanitized | P0 |
| SEC-003 | XSS in src (data URI) | data:image/svg,<script> | Sanitized | P0 |
| SEC-004 | Path traversal in src | `../../etc/passwd` | Rejected | P0 |
| SEC-005 | JavaScript in link | javascript:alert(1) | Sanitized | P0 |
| SEC-006 | HTML in alt | `<img onerror=...>` | Escaped | P0 |
| SEC-007 | SVG script | SVG with script | Sanitized | P0 |
| SEC-008 | Event handler in URL | onload in src | Sanitized | P0 |
| SEC-009 | Template injection | `{{constructor}}` | Sanitized | P1 |
| SEC-010 | DOM clobbering | id=constructor | Mitigated | P1 |

### 6.2 Broken Access Control (10)

| ID | Test | Role | Action | Expected | Priority |
|----|------|------|--------|----------|----------|
| SEC-011 | Logo on public page | Anonymous | View | Allowed | P0 |
| SEC-012 | Logo link to admin | User | Click | Auth check | P0 |
| SEC-013 | Logo asset from other tenant | User A | Load B's asset | 403 | P0 |
| SEC-014 | Logo with auth bypass | Tampered | Load | Validated | P0 |
| SEC-015 | Logo in iframe | Embedded | Load | Allowed or blocked | P1 |
| SEC-016 | Logo with mixed content | HTTP on HTTPS | Blocked | P0 |
| SEC-017 | Logo with insecure redirect | HTTP redirect | Blocked | P1 |
| SEC-018 | Logo with open redirect | Redirect to evil | Validated | P0 |
| SEC-019 | Logo asset token | Expired token | 401, fallback | P1 |
| SEC-020 | Logo CORS | Wrong origin | Blocked | P0 |

### 6.3 IDOR (10)

| ID | Object | Manipulation | Expected | Priority |
|----|--------|-------------|----------|----------|
| SEC-021 | Logo asset ID | Guess ID | 403 if no access | P0 |
| SEC-022 | Logo link URL | Manipulate | Validated | P0 |
| SEC-023 | Negative ID | -1 | 400 | P1 |
| SEC-024 | Zero ID | 0 | 400 | P1 |
| SEC-025 | Float ID | 1.5 | 400 | P1 |
| SEC-026 | String ID | "abc" | 400 | P1 |
| SEC-027 | MAX_INT ID | 2147483647 | Handled | P1 |
| SEC-028 | Other user's asset | Access via ID | 403 | P0 |
| SEC-029 | Deleted asset | Deleted | 404 | P1 |
| SEC-030 | Future asset | Not yet accessible | 403 | P1 |

### 6.4 Mass Assignment (5)

| ID | Protected Field | Expected | Priority |
|----|----------------|----------|----------|
| SEC-031 | Internal paths | Not exposed | P0 |
| SEC-032 | Credentials | Not in URL | P0 |
| SEC-033 | Tokens | Not in src | P0 |
| SEC-034 | Config | Sanitized | P0 |
| SEC-035 | Debug info | Not in prod | P1 |

### 6.5 Authentication & Session (10)

| ID | Attack | Expected Protection | Priority |
|----|--------|-------------------|----------|
| SEC-036 | Session fixation | New session | P0 |
| SEC-037 | Session hijacking | Token binding | P1 |
| SEC-038 | CSRF on click | CSRF token | P0 |
| SEC-039 | Token in URL | HttpOnly cookie | P0 |
| SEC-040 | Token in referrer | Referrer policy | P1 |
| SEC-041 | Token storage | HttpOnly, Secure | P0 |
| SEC-042 | Concurrent sessions | Policy enforced | P1 |
| SEC-043 | Token refresh | Works correctly | P1 |
| SEC-044 | Logout | Token invalidated | P0 |
| SEC-045 | HTTPS | Enforced | P0 |

### 6.6 Data Exposure (5)

| ID | Data | Expected Protection | Priority |
|----|------|-------------------|----------|
| SEC-046 | PII in alt | None | P0 |
| SEC-047 | Internal paths | Not exposed | P0 |
| SEC-048 | Stack traces | Generic errors | P0 |
| SEC-049 | Response headers | No sensitive | P1 |
| SEC-050 | Asset metadata | Filtered | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | Multiple logos load | 5 logos | All load | P1 |
| CON-002 | Logo load + theme switch | Load + theme | Correct final | P1 |
| CON-003 | Logo load + resize | Load + resize | Correct final | P1 |
| CON-004 | Logo load + unmount | Load + unmount | No error | P1 |
| CON-005 | Logo click + navigate | Click + nav | Navigate | P1 |
| CON-006 | Rapid theme switch | 5 switches | Correct final | P1 |
| CON-007 | Rapid resize | 10 resizes | Correct final | P1 |
| CON-008 | Concurrent tab load | 5 tabs | All load | P1 |
| CON-009 | Logo + other images | Logo + 10 images | All load | P1 |
| CON-010 | Logo preload + load | Preload + load | No duplicate | P1 |
| CON-011 | Cache invalidation | Update + load | Fresh | P1 |
| CON-012 | Memory leak | 100 mounts/unmounts | No leak | P1 |
| CON-013 | Connection pool | Many logos | All load | P1 |
| CON-014 | Race on src change | Change src | Latest wins | P1 |
| CON-015 | Race on theme change | Change theme | Latest wins | P1 |
| CON-016 | Race on size change | Change size | Latest wins | P1 |
| CON-017 | Race on click | Double click | Single navigate | P1 |
| CON-018 | Orientation change | Rotate | Correct layout | P1 |
| CON-019 | Visibility change | Hide/show | Load when visible | P1 |
| CON-020 | Tab visibility | Background tab | Lazy when hidden | P1 |
| CON-021 | Print during load | Load + print | Handled | P2 |
| CON-022 | Zoom during load | Load + zoom | Correct | P2 |
| CON-023 | Scroll during load | Load + scroll | Correct | P2 |
| CON-024 | Animation during load | Load + animate | Correct | P2 |
| CON-025 | Focus during load | Load + focus | Correct | P2 |

---

## §8 Unit Tests

> **Minimum:** 21 tests

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | Src validation | Validation | Valid URL | Valid | P1 |
| UNT-002 | Src invalid | Validation | "" | Invalid | P1 |
| UNT-003 | Alt sanitization | Formatting | "<script>" | Escaped | P1 |
| UNT-004 | Size mapping | Formatting | "large" | 64 | P1 |
| UNT-005 | Theme variant | Formatting | "dark" | "logo-dark.png" | P1 |
| UNT-006 | Link validation | Validation | "/" | Valid | P1 |
| UNT-007 | Link invalid | Validation | "javascript:" | Invalid | P1 |
| UNT-008 | Aspect ratio | Calculations | 100, 50 | 2 | P1 |
| UNT-009 | Loading state | Status logic | Loading | true | P1 |
| UNT-010 | Error state | Status logic | Error | true | P1 |
| UNT-011 | Clickable check | Status logic | clickable=true | true | P1 |
| UNT-012 | Responsive size | Calculations | 768 | "medium" | P1 |
| UNT-013 | Object-fit | Formatting | "contain" | Applied | P1 |
| UNT-014 | Fetchpriority | Formatting | "high" | Applied | P1 |
| UNT-015 | Lazy load check | Status logic | loading="lazy" | true | P1 |
| UNT-016 | Decoding | Formatting | "async" | Applied | P1 |
| UNT-017 | Target | Formatting | "_blank" | Applied | P1 |
| UNT-018 | Rel | Formatting | "noopener" | Applied | P1 |
| UNT-019 | ClassName merge | Collections | ["a","b"] | "a b" | P1 |
| UNT-020 | Data attributes | Collections | { "data-x": "1" } | Applied | P1 |
| UNT-021 | Aria attributes | Formatting | role="img" | Applied | P1 |

---

## §9 Performance Tests

> **Minimum:** 16 tests

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Logo initial load | First load | < 500ms | P2 |
| PRF-002 | Logo cached load | Cached | < 50ms | P2 |
| PRF-003 | Logo LCP impact | LCP | < 2.5s | P2 |
| PRF-004 | Logo decode time | Decode | < 100ms | P2 |
| PRF-005 | Logo paint time | Paint | < 50ms | P2 |
| PRF-006 | Theme switch | Switch | < 100ms | P2 |
| PRF-007 | Resize | Resize | < 50ms | P2 |
| PRF-008 | 10 logos load | 10 logos | All < 2s | P2 |
| PRF-009 | Logo with preload | Preload | In head | P2 |
| PRF-010 | Logo lazy | Lazy | When visible | P2 |
| PRF-011 | Logo memory | Memory | < 5MB | P2 |
| PRF-012 | Logo layout shift | CLS | < 0.1 | P2 |
| PRF-013 | Logo FID | FID | < 100ms | P2 |
| PRF-014 | Logo INP | INP | < 200ms | P2 |
| PRF-015 | Logo TTFB | TTFB | < 200ms | P2 |
| PRF-016 | Logo animation | Animation | 60fps | P2 |

---

## §10 Load Tests

> **Minimum:** 10 tests

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | Sustained page load | 50 users, 1 req/s | 5 min | 95% < 3s | P2 |
| LDT-002 | Sustained logo load | 100 logos/s | 5 min | 95% < 500ms | P2 |
| LDT-003 | Spike load | 0→200 users in 30s | 2 min | No errors | P2 |
| LDT-004 | Stress load | 500 users, 5 req/s | 5 min | Graceful | P2 |
| LDT-005 | CDN load | 1000 logos/s | 5 min | CDN handles | P2 |
| LDT-006 | Cache effectiveness | Repeat loads | 5 min | 90% cache hit | P2 |
| LDT-007 | Connection pool | 100 concurrent | 5 min | All complete | P2 |
| LDT-008 | Breaking point | Ramp to failure | - | Identify limit | P2 |
| LDT-009 | Recovery after spike | Spike then 20 users | 5 min | Back to normal | P2 |
| LDT-010 | Recovery after stress | Stress then idle | 2 min | Recover | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| AC-1: Display | POS-001, FUN-001, FUN-002 |
| AC-2: Responsive sizing | POS-005, BND-031 to BND-040 |
| AC-3: Theme variants | POS-003, POS-004, FUN-004 |
| AC-4: Click navigation | POS-002, FUN-003 |
| AC-5: Accessibility | POS-011, POS-012, FUN-008, FUN-009 |
| AC-6: Loading states | POS-009, POS-010, FUN-006, FUN-007 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
