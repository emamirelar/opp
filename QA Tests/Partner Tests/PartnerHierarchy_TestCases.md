# Partner Hierarchy and Navigation Test Cases

## Overview
Test cases for Partner Tree hierarchy navigation in the UNOPS Opportunity+ system.

**JIRA Story:** PNO-130  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 14

---

## Test Cases

### POS_001 - Validate Partner Tree View Existence
**Priority:** High  
**Labels:** Partners, Hierarchy, Navigation

**Objective:** Validate that the Partner Tree view is accessible.

**Preconditions:**
- User is logged in with Partner User or higher role

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners module | Partners page loads |
| 2 | Locate "Partner Tree" or "Hierarchy" view option | Tree view option is visible |
| 3 | Click to access Partner Tree view | Hierarchical tree structure displays |

---

### POS_002 - Display Root Level Partners
**Priority:** High  
**Labels:** Partners, Hierarchy

**Objective:** Validate that root-level (parent) partners display at top of hierarchy.

**Preconditions:**
- Partners with no parent exist in system

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Tree view | Tree loads |
| 2 | Identify root nodes | Parent partners displayed at root level |
| 3 | Verify root partners have expand icons | Expandable indicators visible for parents with children |

---

### POS_003 - Expand Partner Node to Show Children
**Priority:** High  
**Labels:** Partners, Hierarchy, Navigation

**Objective:** Validate expanding a parent partner to reveal child partners.

**Preconditions:**
- Parent partner with child partners exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Tree view | Tree loads |
| 2 | Locate a parent partner node | Parent node visible |
| 3 | Click expand icon (+) | Children partners appear beneath parent |
| 4 | Verify indentation | Child nodes indented to show hierarchy |

**Test Data:**
- Parent: World Bank Group
- Children: IBRD, IFC, IDA

---

### POS_004 - Collapse Partner Node
**Priority:** Normal  
**Labels:** Partners, Hierarchy, Navigation

**Objective:** Validate collapsing an expanded partner node.

**Preconditions:**
- Partner node is expanded showing children

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate expanded parent node | Children visible |
| 2 | Click collapse icon (-) | Children hidden |
| 3 | Verify only parent visible | Child nodes no longer displayed |

---

### POS_005 - Navigate to Partner Detail from Tree
**Priority:** High  
**Labels:** Partners, Hierarchy, Navigation

**Objective:** Validate navigating to partner detail page from tree node.

**Preconditions:**
- Partner Tree is displayed

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate a partner in tree | Partner node visible |
| 2 | Click on partner name | Partner detail page opens |
| 3 | Verify correct partner displayed | Partner details match selected node |

---

### POS_006 - Multi-Level Hierarchy Display
**Priority:** Normal  
**Labels:** Partners, Hierarchy

**Objective:** Validate display of multi-level hierarchy (grandparent, parent, child).

**Preconditions:**
- Three or more levels of partner hierarchy exist

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Tree view | Tree loads |
| 2 | Expand grandparent node | Parent nodes visible |
| 3 | Expand parent node | Child nodes visible |
| 4 | Verify three levels of indentation | Clear visual hierarchy |

**Test Data:**
- Level 1: UN System
- Level 2: World Bank Group
- Level 3: IBRD

---

### POS_007 - Search Within Partner Tree
**Priority:** Normal  
**Labels:** Partners, Hierarchy, Search

**Objective:** Validate searching for partners within tree view.

**Preconditions:**
- Partner Tree is displayed
- Multiple partners exist

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate search field in tree view | Search input visible |
| 2 | Enter partial partner name | Search executes |
| 3 | Verify matching results highlighted or filtered | Matching partners shown |
| 4 | Clear search | Full tree restored |

**Test Data:**
- Search Term: "Bank"

---

### POS_008 - Partner Tree Shows Partner Count
**Priority:** Normal  
**Labels:** Partners, Hierarchy

**Objective:** Validate that child count is displayed for parent nodes.

**Preconditions:**
- Parent partners with children exist

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Tree view | Tree loads |
| 2 | Locate a collapsed parent node | Parent visible |
| 3 | Verify child count indicator | Number of children shown (e.g., "(3)") |

---

### NEG_009 - Handle Partners Without Children
**Priority:** Normal  
**Labels:** Partners, Hierarchy, Negative

**Objective:** Validate display of partners with no children (leaf nodes).

**Preconditions:**
- Partners with no child relationships exist

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Tree view | Tree loads |
| 2 | Locate a partner with no children | Partner visible |
| 3 | Verify no expand icon | No expand/collapse control for leaf nodes |

---

### POS_010 - Expand All Nodes
**Priority:** Low  
**Labels:** Partners, Hierarchy, Navigation

**Objective:** Validate "Expand All" functionality if available.

**Preconditions:**
- Partner Tree with multiple levels

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Tree view | Tree loads collapsed |
| 2 | Click "Expand All" button | All levels expanded |
| 3 | Verify all children visible | Complete hierarchy displayed |

---

### POS_011 - Collapse All Nodes
**Priority:** Low  
**Labels:** Partners, Hierarchy, Navigation

**Objective:** Validate "Collapse All" functionality.

**Preconditions:**
- Partner Tree is fully or partially expanded

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Expand multiple nodes | Children visible |
| 2 | Click "Collapse All" button | All nodes collapse |
| 3 | Verify only root nodes visible | All children hidden |

---

### POS_012 - Partner Tree Preserves Expand State
**Priority:** Normal  
**Labels:** Partners, Hierarchy, State

**Objective:** Validate that expand/collapse state persists during session.

**Preconditions:**
- Partner Tree is displayed

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Expand some nodes, leave others collapsed | Mixed state |
| 2 | Navigate away to another page | Other page loads |
| 3 | Return to Partner Tree view | Tree loads |
| 4 | Verify expand state preserved | Previously expanded nodes still expanded |

---

### POS_013 - Partner Tree Context Menu
**Priority:** Normal  
**Labels:** Partners, Hierarchy, Context

**Objective:** Validate right-click context menu on tree nodes (if available).

**Preconditions:**
- Partner Tree is displayed

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Right-click on a partner node | Context menu appears |
| 2 | Verify menu options | Options: View Details, Add Child Partner, Edit |
| 3 | Click "View Details" | Partner detail page opens |

---

### NEG_014 - Handle Empty Partner Hierarchy
**Priority:** Normal  
**Labels:** Partners, Hierarchy, Negative

**Objective:** Validate empty state when no partners exist or match filter.

**Preconditions:**
- Filter applied that matches no partners OR empty system

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Tree with filter | Tree view loads |
| 2 | Apply filter with no matches | Filter applied |
| 3 | Verify empty state message | "No partners found" or similar displayed |
| 4 | Verify option to clear filter or add partner | Action options available |

---

## Summary

| Priority | Count |
|----------|-------|
| High | 4 |
| Normal | 8 |
| Low | 2 |
| **TOTAL** | **14** |
