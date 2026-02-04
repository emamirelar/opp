# Partner Ecosystem Test Cases

## Overview
Test cases for the Partner Ecosystem feature in the UNOPS Opportunity+ system, including partner hierarchy visualization and navigation.

**JIRA Story:** PNO-150  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 12

---

## Test Cases

### POS_001 - Validate Partner Ecosystem View Existence
**Priority:** High  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem

**Objective:** Validate that the Partner Ecosystem view is accessible from the Partners module.

**Preconditions:**
- User is Partner User or Partner Global Admin

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to the Partners module | Partners list view loads |
| 3 | Locate and click on "Partner Ecosystem" view option | Partner Ecosystem visualization loads |
| 4 | Verify ecosystem visualization is displayed | Hierarchical/tree view of partners is visible |

---

### POS_002 - Validate Partner Hierarchy Display
**Priority:** High  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem, Hierarchy

**Objective:** Validate that the partner hierarchy correctly shows parent-child relationships.

**Preconditions:**
- Partner with child partners exists in system

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Ecosystem view | Ecosystem view loads |
| 2 | Locate a parent partner in the hierarchy | Parent partner node is visible |
| 3 | Expand the parent partner node | Child partners are displayed beneath parent |
| 4 | Verify parent-child relationship visualization | Lines/connectors clearly show relationships |

**Test Data:**
- Parent Partner: World Bank
- Child Partners: World Bank Group, IBRD, IFC

---

### POS_003 - Navigate to Partner Record from Ecosystem
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem, Navigation

**Objective:** Validate that a user can navigate to a partner record directly from the ecosystem view.

**Preconditions:**
- Partner Ecosystem view is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate a partner in the ecosystem view | Partner node is visible |
| 2 | Click on the partner name or node | Partner detail page opens |
| 3 | Verify correct partner record is displayed | Partner name and details match selected node |

---

### POS_004 - Search Partners in Ecosystem View
**Priority:** High  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem, Search

**Objective:** Validate that the search functionality works within the Partner Ecosystem view.

**Preconditions:**
- Partner Ecosystem view is open
- Multiple partners exist in system

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate the search field in ecosystem view | Search field is visible |
| 2 | Enter a partial partner name | Search results appear or tree filters/highlights |
| 3 | Click on a search result | Partner is highlighted or focused in the view |
| 4 | Verify search results are accurate | Matching partners are correctly identified |

**Test Data:**
- Search Term: "World"
- Expected Results: World Bank, World Food Programme, World Health Organization

---

### POS_005 - Filter Ecosystem by Partner Type
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem, Filter

**Objective:** Validate that the ecosystem view can be filtered by partner type.

**Preconditions:**
- Partners of different types exist (Funding, Client, Implementation)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Ecosystem view | Ecosystem view loads |
| 2 | Locate and use the Partner Type filter | Filter dropdown or toggle is available |
| 3 | Select "Funding Partners" filter | View updates to show only Funding Partners |
| 4 | Verify filter is correctly applied | Only Funding Partners are displayed |

---

### POS_006 - Expand and Collapse Partner Nodes
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem, Navigation

**Objective:** Validate that partner nodes can be expanded and collapsed in the hierarchy view.

**Preconditions:**
- Parent partner with children exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Ecosystem view | Ecosystem view loads |
| 2 | Locate a parent partner with children | Expand/collapse icon is visible |
| 3 | Click to expand the parent node | Child partners are displayed |
| 4 | Click to collapse the parent node | Child partners are hidden |
| 5 | Verify expand/collapse state persists correctly | State changes are reflected in UI |

---

### POS_007 - View Partner Ecosystem Summary Statistics
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem, Statistics

**Objective:** Validate that summary statistics are displayed in the ecosystem view.

**Preconditions:**
- Partner Ecosystem view is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Ecosystem view | Ecosystem view loads |
| 2 | Locate the summary statistics section | Statistics panel or widget is visible |
| 3 | Verify displayed statistics | Total partners, partners by type, active partners counts are shown |

---

### POS_008 - Zoom and Pan in Ecosystem Visualization
**Priority:** Low  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem, UI

**Objective:** Validate zoom and pan functionality in the ecosystem visualization.

**Preconditions:**
- Large number of partners exist in hierarchy

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Ecosystem view | Ecosystem view loads |
| 2 | Use zoom controls to zoom in | View zooms in showing larger nodes |
| 3 | Use zoom controls to zoom out | View zooms out showing more of the hierarchy |
| 4 | Click and drag to pan the view | View pans in the dragged direction |

---

### NEG_009 - Validate Empty Ecosystem View
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem, Negative

**Objective:** Validate appropriate message is shown when no partners exist.

**Preconditions:**
- No partners exist in the system (or filter applied returns no results)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Ecosystem view | Ecosystem view loads |
| 2 | Verify empty state message | Message displayed: "No partners found" or similar |
| 3 | Verify option to create new partner | "Add Partner" button or link is available |

---

### POS_010 - View Partner Relationships in Ecosystem
**Priority:** High  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem, Relationships

**Objective:** Validate that partner relationships are visualized correctly.

**Preconditions:**
- Partners with defined relationships exist

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Ecosystem view | Ecosystem view loads |
| 2 | Locate partners with known relationships | Partners are displayed in view |
| 3 | Verify relationship lines/connectors | Lines connect related partners |
| 4 | Hover or click on relationship line | Relationship type is displayed (Parent/Child, Partnership, etc.) |

---

### POS_011 - Export Ecosystem View
**Priority:** Low  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem, Export

**Objective:** Validate that the ecosystem view can be exported.

**Preconditions:**
- Partner Ecosystem view is open with data

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Ecosystem view | Ecosystem view loads |
| 2 | Locate and click export option | Export options are displayed |
| 3 | Select export format (PDF/PNG/CSV) | Export is initiated |
| 4 | Verify exported file is correct | File contains ecosystem visualization or data |

---

### POS_012 - Partner Ecosystem Responsive Layout
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Partner, Ecosystem, Responsive

**Objective:** Validate that the ecosystem view is responsive on different screen sizes.

**Preconditions:**
- Access to different device types or responsive testing tools

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Ecosystem view on desktop | Full ecosystem view loads |
| 2 | Resize browser to tablet dimensions | View adapts to smaller screen |
| 3 | Resize browser to mobile dimensions | View adapts with appropriate mobile layout |
| 4 | Verify all functionality remains accessible | Key features work on all screen sizes |

---

## Summary

| Priority | Count |
|----------|-------|
| High | 4 |
| Normal | 6 |
| Low | 2 |
| **TOTAL** | **12** |
