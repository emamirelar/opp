# Role Matrix Test Cases

## Overview
Test cases for the Opportunity+ Role Matrix, validating permission-based access control across all user roles.

**JIRA Story:** PNO-562  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 20

---

## Roles Under Test

| Role ID | Role Name | Description |
|---------|-----------|-------------|
| 1 | Administrator | Full system access |
| 2 | Partner Global Admin | Global partner management access |
| 3 | Partner User | Standard partner operations |
| 4 | General User (GENUSER) | Read-only or limited access |
| 5 | Org Unit Admin | Organizational unit specific access |

---

## Test Cases

### Partner Module Permissions

#### POS_001 - Administrator Can Create Partners
**Priority:** High  
**Labels:** Authorization, Role_Matrix, Partners

**Objective:** Validate Administrator role has Create Partner permission.

**Preconditions:**
- Logged in as Administrator

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners module | Partners list loads |
| 2 | Locate "Create Partner" button | Button is visible and enabled |
| 3 | Click to create a new partner | Create form opens |
| 4 | Complete and save partner | Partner created successfully |

---

#### POS_002 - Partner User Can Create Partners
**Priority:** High  
**Labels:** Authorization, Role_Matrix, Partners

**Objective:** Validate Partner User role has Create Partner permission.

**Preconditions:**
- Logged in as Partner User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners module | Partners list loads |
| 2 | Locate "Create Partner" button | Button is visible and enabled |
| 3 | Click to create a new partner | Create form opens |
| 4 | Complete and save partner | Partner created successfully |

---

#### NEG_003 - General User Cannot Create Partners
**Priority:** High  
**Labels:** Authorization, Role_Matrix, Partners, Negative

**Objective:** Validate General User role does NOT have Create Partner permission.

**Preconditions:**
- Logged in as General User (GENUSER)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners module | Partners list loads |
| 2 | Look for "Create Partner" button | Button is hidden or disabled |
| 3 | Attempt direct API call to create partner | Request denied (403) |

---

### Opportunity Module Permissions

#### POS_004 - Partner User Can Create Opportunities
**Priority:** High  
**Labels:** Authorization, Role_Matrix, Opportunities

**Objective:** Validate Partner User can create opportunities.

**Preconditions:**
- Logged in as Partner User
- Partner record exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Opportunities module | Opportunities list loads |
| 2 | Click "Create Opportunity" | Create form opens |
| 3 | Complete required fields and save | Opportunity created |

---

#### NEG_005 - General User Cannot Create Opportunities
**Priority:** High  
**Labels:** Authorization, Role_Matrix, Opportunities, Negative

**Objective:** Validate General User cannot create opportunities.

**Preconditions:**
- Logged in as General User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Opportunities module | Opportunities list loads |
| 2 | Look for "Create Opportunity" button | Button is hidden or disabled |
| 3 | Attempt direct API call | Request denied (403) |

---

#### POS_006 - Opportunity Manager Can Edit Their Opportunities
**Priority:** High  
**Labels:** Authorization, Role_Matrix, Opportunities

**Objective:** Validate Opportunity Manager can edit opportunities they manage.

**Preconditions:**
- Logged in as Partner User who is Opportunity Manager

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to an Opportunity they manage | Opportunity detail loads |
| 2 | Click Edit | Edit mode activates |
| 3 | Make changes and save | Changes saved successfully |

---

#### NEG_007 - Partner User Cannot Edit Others' Opportunities
**Priority:** Normal  
**Labels:** Authorization, Role_Matrix, Opportunities, Negative

**Objective:** Validate Partner User cannot edit opportunities they don't manage.

**Preconditions:**
- Logged in as Partner User
- Viewing opportunity managed by someone else

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Opportunity managed by other user | Opportunity detail loads |
| 2 | Look for Edit button | Edit button hidden or disabled |
| 3 | Attempt direct API edit | Request denied (403) |

---

### Contact Module Permissions

#### POS_008 - Partner User Can Create Contacts
**Priority:** Normal  
**Labels:** Authorization, Role_Matrix, Contacts

**Objective:** Validate Partner User can create contacts.

**Preconditions:**
- Logged in as Partner User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Contacts | Contact list loads |
| 2 | Click "Create Contact" | Create form opens |
| 3 | Complete and save | Contact created |

---

#### NEG_009 - General User Cannot Create Contacts
**Priority:** Normal  
**Labels:** Authorization, Role_Matrix, Contacts, Negative

**Objective:** Validate General User cannot create contacts.

**Preconditions:**
- Logged in as General User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Contacts | Contact list loads |
| 2 | Look for "Create Contact" button | Button hidden or disabled |

---

### Interaction Module Permissions

#### POS_010 - Partner User Can Log Interactions
**Priority:** Normal  
**Labels:** Authorization, Role_Matrix, Interactions

**Objective:** Validate Partner User can create interactions.

**Preconditions:**
- Logged in as Partner User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Interactions | Interaction list loads |
| 2 | Click "Log New Interaction" | Create form opens |
| 3 | Complete and save | Interaction logged |

---

#### NEG_011 - General User Cannot Log Interactions
**Priority:** Normal  
**Labels:** Authorization, Role_Matrix, Interactions, Negative

**Objective:** Validate General User cannot create interactions.

**Preconditions:**
- Logged in as General User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Interactions | Interaction list loads |
| 2 | Look for create button | Button hidden or disabled |

---

### Administration Permissions

#### POS_012 - Administrator Can Access All Admin Features
**Priority:** High  
**Labels:** Authorization, Role_Matrix, Administration

**Objective:** Validate Administrator has full admin access.

**Preconditions:**
- Logged in as Administrator

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Administration menu | All admin options visible |
| 2 | Access User Management | Page loads |
| 3 | Access AI Prompts | Page loads |
| 4 | Access Entity Manager | Page loads |

---

#### NEG_013 - Partner User Cannot Access Admin Features
**Priority:** High  
**Labels:** Authorization, Role_Matrix, Administration, Negative

**Objective:** Validate Partner User cannot access admin features.

**Preconditions:**
- Logged in as Partner User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Look for Administration menu | Menu hidden |
| 2 | Attempt direct URL to User Management | Access denied |
| 3 | Attempt direct URL to AI Prompts | Access denied |

---

### View Permissions

#### POS_014 - General User Can View Partners (Read-Only)
**Priority:** Normal  
**Labels:** Authorization, Role_Matrix, View

**Objective:** Validate General User has read access to partners.

**Preconditions:**
- Logged in as General User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners list | List loads successfully |
| 2 | Click on a partner | Partner details viewable |
| 3 | Verify no edit controls | Edit button hidden |

---

#### POS_015 - General User Can View Opportunities (Read-Only)
**Priority:** Normal  
**Labels:** Authorization, Role_Matrix, View

**Objective:** Validate General User can view opportunities.

**Preconditions:**
- Logged in as General User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Opportunities list | List loads |
| 2 | Click on an opportunity | Details viewable |
| 3 | Verify no edit controls | Edit button hidden |

---

### Delete Permissions

#### NEG_016 - Partner User Cannot Delete Partners
**Priority:** Normal  
**Labels:** Authorization, Role_Matrix, Delete, Negative

**Objective:** Validate Partner User cannot delete partners.

**Preconditions:**
- Logged in as Partner User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to a Partner | Partner details load |
| 2 | Look for Delete option | Delete button hidden |
| 3 | Attempt API delete | Request denied (403) |

---

#### POS_017 - Administrator Can Delete Partners
**Priority:** Normal  
**Labels:** Authorization, Role_Matrix, Delete

**Objective:** Validate Administrator can delete partners.

**Preconditions:**
- Logged in as Administrator
- Test partner exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to test Partner | Partner details load |
| 2 | Click Delete | Confirmation dialog appears |
| 3 | Confirm deletion | Partner deleted (soft delete) |

---

### Org Unit Admin Permissions

#### POS_018 - Org Unit Admin Can Manage Own Unit Partners
**Priority:** Normal  
**Labels:** Authorization, Role_Matrix, Org_Unit

**Objective:** Validate Org Unit Admin can manage partners within their unit.

**Preconditions:**
- Logged in as Org Unit Admin
- Partners exist in their org unit

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners in their org unit | Partners visible |
| 2 | Edit a partner in their unit | Edit allowed |
| 3 | Save changes | Changes saved |

---

#### NEG_019 - Org Unit Admin Cannot Manage Other Unit Partners
**Priority:** Normal  
**Labels:** Authorization, Role_Matrix, Org_Unit, Negative

**Objective:** Validate Org Unit Admin cannot manage partners in other units.

**Preconditions:**
- Logged in as Org Unit Admin
- Partner exists in different org unit

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View partner from different org unit | May be visible (read-only) |
| 2 | Attempt to edit | Edit button hidden or disabled |
| 3 | Attempt API edit | Request denied (403) |

---

### Cross-Module Permission Test

#### POS_020 - Partner Global Admin Full Partner Access
**Priority:** High  
**Labels:** Authorization, Role_Matrix, Global_Admin

**Objective:** Validate Partner Global Admin has comprehensive partner access.

**Preconditions:**
- Logged in as Partner Global Admin

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create a new partner | Success |
| 2 | Edit any partner | Success |
| 3 | Create opportunity from partner | Success |
| 4 | Log interaction for any partner | Success |
| 5 | Access partner ecosystem view | Success |

---

## Role Permission Matrix Summary

| Feature | Admin | Partner Global Admin | Partner User | Org Unit Admin | General User |
|---------|-------|---------------------|--------------|----------------|--------------|
| View Partners | ✓ | ✓ | ✓ | ✓ (own unit) | ✓ |
| Create Partners | ✓ | ✓ | ✓ | ✓ (own unit) | ✗ |
| Edit Partners | ✓ | ✓ | ✓ | ✓ (own unit) | ✗ |
| Delete Partners | ✓ | ✓ | ✗ | ✗ | ✗ |
| View Opportunities | ✓ | ✓ | ✓ | ✓ | ✓ |
| Create Opportunities | ✓ | ✓ | ✓ | ✓ | ✗ |
| Edit Opportunities | ✓ | ✓ | ✓ (own) | ✓ (own) | ✗ |
| View Contacts | ✓ | ✓ | ✓ | ✓ | ✓ |
| Create Contacts | ✓ | ✓ | ✓ | ✓ | ✗ |
| Log Interactions | ✓ | ✓ | ✓ | ✓ | ✗ |
| Access Admin | ✓ | ✗ | ✗ | ✗ | ✗ |
| User Management | ✓ | ✗ | ✗ | ✗ | ✗ |

---

## Summary

| Priority | Count |
|----------|-------|
| High | 8 |
| Normal | 12 |
| **TOTAL** | **20** |
