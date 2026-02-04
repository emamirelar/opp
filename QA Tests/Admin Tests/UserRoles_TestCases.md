# User Roles Management Test Cases

## Overview
Test cases for the Administration - Manage User Roles feature in the UNOPS Opportunity+ system.

**JIRA Story:** PNO-233  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 15

---

## Test Cases

### POS_001 - Access User Roles Management Page
**Priority:** High  
**Labels:** Administration, User_Roles, Access

**Objective:** Validate that administrators can access the User Roles management page.

**Preconditions:**
- User has Administrator role

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as Administrator | Dashboard loads |
| 2 | Navigate to Administration menu | Admin options displayed |
| 3 | Click on "Manage User Roles" | User Roles management page loads |
| 4 | Verify page content | List of roles and users displayed |

---

### POS_002 - View List of Available Roles
**Priority:** High  
**Labels:** Administration, User_Roles

**Objective:** Validate that all system roles are displayed.

**Preconditions:**
- User is on User Roles management page

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View the Roles section | List of roles visible |
| 2 | Verify expected roles are present | Roles include: Administrator, Partner User, Partner Global Admin, General User |
| 3 | Verify role count matches system configuration | All configured roles displayed |

---

### POS_003 - Assign Role to User
**Priority:** High  
**Labels:** Administration, User_Roles, Assignment

**Objective:** Validate assigning a role to a user.

**Preconditions:**
- User exists without the target role
- Administrator access

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Search for user by name | User found in list |
| 2 | Click on user to view details | User role details displayed |
| 3 | Click "Add Role" | Role selection dialog opens |
| 4 | Select "Partner User" role | Role selected |
| 5 | Save changes | Role assigned successfully |
| 6 | Verify user now has the role | Role appears in user's role list |

**Test Data:**
- User: John Smith
- Role to Assign: Partner User

---

### POS_004 - Remove Role from User
**Priority:** High  
**Labels:** Administration, User_Roles, Removal

**Objective:** Validate removing a role from a user.

**Preconditions:**
- User has at least two roles assigned

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Search for user with multiple roles | User found |
| 2 | Click on user to view details | Roles displayed |
| 3 | Click "Remove" next to a role | Confirmation dialog appears |
| 4 | Confirm removal | Role removed |
| 5 | Verify role no longer appears | User's role list updated |

---

### NEG_005 - Prevent Removing Last Admin Role
**Priority:** High  
**Labels:** Administration, User_Roles, Negative

**Objective:** Validate system prevents removing the last Administrator from the system.

**Preconditions:**
- Only one user has Administrator role

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate the only Administrator user | User displayed |
| 2 | Attempt to remove Administrator role | System prevents removal |
| 3 | Verify error message | "Cannot remove the last Administrator" or similar |

---

### POS_006 - Search Users in Role Management
**Priority:** Normal  
**Labels:** Administration, User_Roles, Search

**Objective:** Validate searching for users in role management.

**Preconditions:**
- Multiple users exist in system

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to User Roles management | Page loads |
| 2 | Enter search term in user search | Results filter |
| 3 | Verify matching users displayed | Only matching users shown |
| 4 | Clear search | All users displayed again |

**Test Data:**
- Search Term: "John"

---

### POS_007 - Filter Users by Role
**Priority:** Normal  
**Labels:** Administration, User_Roles, Filter

**Objective:** Validate filtering users by their assigned role.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to User Roles management | Page loads |
| 2 | Select role filter (e.g., "Partner User") | Filter applied |
| 3 | Verify displayed users | Only users with Partner User role shown |
| 4 | Select "All Roles" | Filter cleared, all users shown |

---

### POS_008 - Assign Multiple Roles to Single User
**Priority:** Normal  
**Labels:** Administration, User_Roles, Multi-Role

**Objective:** Validate assigning multiple roles to one user.

**Preconditions:**
- User has one role

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate user with single role | User displayed |
| 2 | Add second role | Role selection dialog opens |
| 3 | Select additional role and save | Second role added |
| 4 | Verify user has both roles | Both roles listed |

---

### NEG_009 - Prevent Duplicate Role Assignment
**Priority:** Normal  
**Labels:** Administration, User_Roles, Negative

**Objective:** Validate system prevents assigning same role twice.

**Preconditions:**
- User already has "Partner User" role

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Add Role dialog for user | Dialog opens |
| 2 | Attempt to select "Partner User" again | Role is grayed out or not selectable |
| 3 | Verify warning message | "User already has this role" or role not available |

---

### POS_010 - View Role Permissions
**Priority:** Normal  
**Labels:** Administration, User_Roles, Permissions

**Objective:** Validate viewing permissions associated with a role.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to User Roles management | Page loads |
| 2 | Click on a role to view details | Role details panel opens |
| 3 | Verify permissions list | List of permissions for the role displayed |
| 4 | Verify permissions are read-only | Cannot modify permissions from this view |

---

### NEG_011 - Non-Admin Cannot Access Role Management
**Priority:** High  
**Labels:** Administration, User_Roles, Security

**Objective:** Validate non-administrators cannot access role management.

**Preconditions:**
- Logged in as non-Administrator (e.g., Partner User)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Attempt to navigate to User Roles management | Access denied |
| 2 | Try direct URL access | Redirected or 403 error |
| 3 | Verify Administration menu is hidden | Menu not visible for non-admins |

---

### POS_012 - Audit Log for Role Changes
**Priority:** Normal  
**Labels:** Administration, User_Roles, Audit

**Objective:** Validate that role changes are logged in audit trail.

**Preconditions:**
- Audit logging is enabled

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Assign a new role to a user | Role assigned |
| 2 | Navigate to audit log | Audit log page loads |
| 3 | Search for role change entry | Entry found |
| 4 | Verify audit entry details | Shows: user affected, role added, who made change, timestamp |

---

### POS_013 - Bulk Role Assignment
**Priority:** Low  
**Labels:** Administration, User_Roles, Bulk

**Objective:** Validate assigning a role to multiple users at once (if feature exists).

**Preconditions:**
- Multiple users exist
- Bulk operations are supported

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select multiple users via checkbox | Users selected |
| 2 | Click "Assign Role" for bulk action | Role selection dialog opens |
| 3 | Select role and confirm | Role assigned to all selected users |
| 4 | Verify each user has the new role | All selected users updated |

---

### NEG_014 - Invalid Role Assignment Handling
**Priority:** Normal  
**Labels:** Administration, User_Roles, Error

**Objective:** Validate error handling for invalid role operations.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Attempt to assign role that doesn't exist (via API manipulation) | Request fails |
| 2 | Verify error message | Appropriate error displayed |
| 3 | Verify user roles unchanged | No invalid role assigned |

---

### POS_015 - Role Change Takes Effect Immediately
**Priority:** High  
**Labels:** Administration, User_Roles, Real-time

**Objective:** Validate that role changes take effect without requiring re-login.

**Preconditions:**
- User is logged in with browser session open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Admin assigns new role to logged-in user | Role assigned |
| 2 | User refreshes their page | Page reloads |
| 3 | Verify new role permissions apply | User can access new features granted by role |

---

## Summary

| Priority | Count |
|----------|-------|
| High | 6 |
| Normal | 7 |
| Low | 2 |
| **TOTAL** | **15** |
