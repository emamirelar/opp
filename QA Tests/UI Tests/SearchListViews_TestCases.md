# Search and List Views Test Cases

## Overview
Test cases for search functionality and list view features across the UNOPS Opportunity+ system.

**JIRA Stories:** PNO-146, PNO-230, PNO-235, PNO-311  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 35

---

## 1. General Search Features (PNO-146)

### POS_001 - Validate Search Box Visibility
**Priority:** High  
**Labels:** Search, UI, List_View

**Objective:** Validate that the search box is visible on list views.

**Preconditions:**
- User is logged in

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners list view | List view loads |
| 2 | Locate search field | Search input is visible at top of list |
| 3 | Navigate to Opportunities list view | List view loads |
| 4 | Verify search field exists | Search input is visible |

---

### POS_002 - Basic Text Search
**Priority:** High  
**Labels:** Search, List_View

**Objective:** Validate basic text search functionality.

**Preconditions:**
- Records exist in the list

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners list view | List shows all partners |
| 2 | Enter search term in search box | Results filter as user types |
| 3 | Press Enter or click search | Search is executed |
| 4 | Verify results match search term | Only matching records displayed |

**Test Data:**
- Search Term: "World Bank"

---

### POS_003 - Search with Partial Match
**Priority:** Normal  
**Labels:** Search, List_View

**Objective:** Validate that partial text matches return results.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view | List loads |
| 2 | Enter partial search term | Search is executed |
| 3 | Verify results | Records containing partial match are displayed |

**Test Data:**
- Search Term: "Wor" (should find "World Bank", "World Health Organization")

---

### POS_004 - Search Case Insensitivity
**Priority:** Normal  
**Labels:** Search, List_View

**Objective:** Validate that search is case insensitive.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view | List loads |
| 2 | Search using lowercase term | Results displayed |
| 3 | Search using uppercase term | Same results displayed |
| 4 | Search using mixed case | Same results displayed |

**Test Data:**
- Terms: "world bank", "WORLD BANK", "World Bank"

---

### POS_005 - Clear Search Results
**Priority:** Normal  
**Labels:** Search, List_View

**Objective:** Validate that search can be cleared to show all records.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Perform a search | Filtered results displayed |
| 2 | Click clear/X button | Search field cleared |
| 3 | Verify all records shown | Full list is restored |

---

### NEG_006 - Search with No Results
**Priority:** Normal  
**Labels:** Search, List_View, Negative

**Objective:** Validate appropriate message when no results match.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view | List loads |
| 2 | Enter search term that matches nothing | Search executed |
| 3 | Verify empty state message | "No results found" message displayed |

**Test Data:**
- Search Term: "XYZNONEXISTENT123"

---

### POS_007 - Search Across Multiple Columns
**Priority:** High  
**Labels:** Search, List_View

**Objective:** Validate that search queries multiple fields.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners list | List loads |
| 2 | Search for a value that appears in Name field | Partner found |
| 3 | Search for a value that appears in Type field | Partners found |
| 4 | Search for a value that appears in Status field | Partners found |

---

### POS_008 - Search Persistence During Navigation
**Priority:** Normal  
**Labels:** Search, List_View, Navigation

**Objective:** Validate search term persists when navigating away and back.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view and perform search | Filtered results displayed |
| 2 | Click on a record to view details | Detail page loads |
| 3 | Navigate back to list view | List view loads |
| 4 | Verify search term and results | Search term retained, filtered results shown |

---

## 2. List View Filtering

### POS_009 - Filter by Status
**Priority:** High  
**Labels:** Filter, List_View

**Objective:** Validate filtering by status column.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Opportunities list | List loads |
| 2 | Click status filter dropdown | Filter options appear |
| 3 | Select "Active" | List filters to Active opportunities only |
| 4 | Verify all visible records have Active status | All displayed records are Active |

---

### POS_010 - Multiple Filter Combination
**Priority:** Normal  
**Labels:** Filter, List_View

**Objective:** Validate applying multiple filters simultaneously.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view | List loads |
| 2 | Apply first filter (e.g., Status = Active) | Results filter |
| 3 | Apply second filter (e.g., Type = Funding) | Results narrow further |
| 4 | Verify both filters applied | Only records matching both criteria shown |

---

### POS_011 - Clear All Filters
**Priority:** Normal  
**Labels:** Filter, List_View

**Objective:** Validate clearing all applied filters at once.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Apply multiple filters | Filtered results shown |
| 2 | Click "Clear All Filters" button | All filters removed |
| 3 | Verify full list restored | All records displayed |

---

### POS_012 - Filter by Date Range
**Priority:** Normal  
**Labels:** Filter, List_View, Date

**Objective:** Validate filtering by date range.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list with date column | List loads |
| 2 | Click date filter | Date range selector appears |
| 3 | Select start and end date | Filter applied |
| 4 | Verify records within date range | Only matching records shown |

**Test Data:**
- Start Date: 2026-01-01
- End Date: 2026-01-31

---

## 3. List View Columns

### POS_013 - Validate Default Column Display
**Priority:** High  
**Labels:** List_View, Columns

**Objective:** Validate that default columns are displayed correctly.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners list view | List loads |
| 2 | Verify default columns | Name, Type, Status, Created Date columns visible |
| 3 | Navigate to Opportunities list view | List loads |
| 4 | Verify default columns | Name, Partner, Status, Stage columns visible |

---

### POS_014 - Column Sorting Ascending
**Priority:** High  
**Labels:** List_View, Columns, Sorting

**Objective:** Validate ascending sort on columns.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view | List loads |
| 2 | Click on Name column header | List sorts alphabetically A-Z |
| 3 | Verify sort order | First record starts with A, last with Z |

---

### POS_015 - Column Sorting Descending
**Priority:** High  
**Labels:** List_View, Columns, Sorting

**Objective:** Validate descending sort on columns.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view with ascending sort | List sorted A-Z |
| 2 | Click same column header again | Sort reverses to Z-A |
| 3 | Verify sort order | First record starts with Z, last with A |

---

### POS_016 - Column Reordering
**Priority:** Normal  
**Labels:** List_View, Columns

**Objective:** Validate that columns can be reordered via drag and drop.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view | List loads with default column order |
| 2 | Drag a column header to new position | Column moves |
| 3 | Release column | Column placed in new position |
| 4 | Verify new column order | Columns display in new arrangement |

---

### POS_017 - Column Visibility Toggle
**Priority:** Normal  
**Labels:** List_View, Columns

**Objective:** Validate hiding and showing columns.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view | List loads |
| 2 | Open column visibility settings | Column list with checkboxes appears |
| 3 | Uncheck a visible column | Column is hidden |
| 4 | Check a hidden column | Column becomes visible |

---

### POS_018 - Column Width Adjustment
**Priority:** Low  
**Labels:** List_View, Columns

**Objective:** Validate adjusting column widths.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view | List loads |
| 2 | Hover over column border | Resize cursor appears |
| 3 | Drag to resize column | Column width changes |
| 4 | Verify content adjusts | Text truncates or wraps appropriately |

---

## 4. Interactions List View Columns (PNO-230)

### POS_019 - Validate Interactions List Columns
**Priority:** High  
**Labels:** Interactions, List_View, Columns

**Objective:** Validate that Interactions list displays all required columns.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Interactions list view | List loads |
| 2 | Verify column headers | Required columns visible: Subject, Type, Date, Partner, Status |
| 3 | Verify data displays in each column | Data populates correctly |

---

### POS_020 - Interactions Date Column Format
**Priority:** Normal  
**Labels:** Interactions, List_View, Date

**Objective:** Validate date format in Interactions list.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Interactions list | List loads |
| 2 | Locate Date column | Dates are visible |
| 3 | Verify date format | Dates display in consistent format (e.g., DD/MM/YYYY) |

---

### POS_021 - Interactions Type Column Values
**Priority:** Normal  
**Labels:** Interactions, List_View

**Objective:** Validate Interaction Type column displays correct values.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Interactions list | List loads |
| 2 | Review Type column | Types display as: Meeting, Email, Call, Visit, Other |

---

## 5. Contact List Columns (PNO-235)

### POS_022 - Validate Contact List Columns
**Priority:** High  
**Labels:** Contacts, List_View, Columns

**Objective:** Validate that Contact list displays all required columns.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Contacts list view | List loads |
| 2 | Verify column headers | Required columns: Name, Email, Phone, Partner, Title |
| 3 | Verify data in each column | Data populates correctly |

---

### POS_023 - Contact Email Column Clickable
**Priority:** Normal  
**Labels:** Contacts, List_View, Email

**Objective:** Validate that email addresses in Contact list are clickable.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Contacts list | List loads |
| 2 | Locate email column | Email addresses visible |
| 3 | Click on an email address | Email client opens with address pre-filled |

---

### POS_024 - Contact Phone Column Format
**Priority:** Normal  
**Labels:** Contacts, List_View, Phone

**Objective:** Validate phone number format in Contact list.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Contacts list | List loads |
| 2 | Locate Phone column | Phone numbers visible |
| 3 | Verify format consistency | Numbers display in consistent format |

---

## 6. Partner Navigation (PNO-311)

### POS_025 - Navigate by Partner Category
**Priority:** High  
**Labels:** Partners, Navigation, Category

**Objective:** Validate navigation by Partner Category.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners module | Partners page loads |
| 2 | Locate Category navigation/filter | Category options visible |
| 3 | Click on a Category (e.g., "Government") | Partners filter to selected category |
| 4 | Verify filtered results | Only Government partners shown |

---

### POS_026 - Navigate by Partner Group
**Priority:** High  
**Labels:** Partners, Navigation, Group

**Objective:** Validate navigation by Partner Group.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners module | Partners page loads |
| 2 | Locate Group navigation | Group options visible |
| 3 | Select a Partner Group | Partners filter to selected group |
| 4 | Verify filtered results | Only partners in selected group shown |

---

### POS_027 - Category and Group Combination
**Priority:** Normal  
**Labels:** Partners, Navigation, Filter

**Objective:** Validate filtering by both Category and Group.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners module | Partners page loads |
| 2 | Select a Category | Partners filter |
| 3 | Additionally select a Group | Partners narrow further |
| 4 | Verify results match both criteria | Only matching partners displayed |

---

### POS_028 - Breadcrumb Navigation
**Priority:** Normal  
**Labels:** Partners, Navigation, Breadcrumb

**Objective:** Validate breadcrumb navigation in Partner hierarchy.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate deep into Partner hierarchy | Deep level loads |
| 2 | View breadcrumb trail | Breadcrumbs show navigation path |
| 3 | Click on parent level breadcrumb | Navigate back to that level |
| 4 | Verify navigation works | Previous level loads correctly |

---

## 7. Pagination

### POS_029 - Validate Pagination Controls
**Priority:** High  
**Labels:** List_View, Pagination

**Objective:** Validate pagination controls are present and functional.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list with many records | List loads with pagination |
| 2 | Verify pagination controls | Page numbers, next/prev buttons visible |
| 3 | Click next page | Next page of results loads |
| 4 | Click previous page | Previous page loads |

---

### POS_030 - Page Size Selection
**Priority:** Normal  
**Labels:** List_View, Pagination

**Objective:** Validate changing the number of records per page.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view | List loads with default page size |
| 2 | Locate page size selector | Dropdown with options (10, 25, 50, 100) |
| 3 | Select larger page size | More records display per page |
| 4 | Verify record count matches selection | Correct number of records shown |

---

### POS_031 - Navigate to Specific Page
**Priority:** Normal  
**Labels:** List_View, Pagination

**Objective:** Validate jumping to a specific page number.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list with multiple pages | List loads |
| 2 | Click on page number "5" | Page 5 loads |
| 3 | Verify page indicator | Current page shows as 5 |
| 4 | Verify correct records | Records from page 5 displayed |

---

### POS_032 - First and Last Page Navigation
**Priority:** Normal  
**Labels:** List_View, Pagination

**Objective:** Validate first and last page buttons.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to middle page of list | Page loads |
| 2 | Click "Last Page" button | Last page loads |
| 3 | Click "First Page" button | First page loads |

---

## 8. Export Functionality

### POS_033 - Export List to CSV
**Priority:** Normal  
**Labels:** List_View, Export

**Objective:** Validate exporting list data to CSV.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to list view | List loads |
| 2 | Click Export button | Export options appear |
| 3 | Select CSV format | Download initiates |
| 4 | Open downloaded file | CSV contains list data |

---

### POS_034 - Export Respects Filters
**Priority:** Normal  
**Labels:** List_View, Export, Filter

**Objective:** Validate export includes only filtered data.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Apply filter to list | Filtered results shown |
| 2 | Export to CSV | Download initiates |
| 3 | Open exported file | Only filtered records included |

---

### POS_035 - Export All vs Current Page
**Priority:** Normal  
**Labels:** List_View, Export, Pagination

**Objective:** Validate option to export all records or current page only.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to paginated list | Multiple pages exist |
| 2 | Click Export and select "Current Page" | Only current page records exported |
| 3 | Click Export and select "All Records" | All records exported |

---

## Summary

| Section | Total Tests |
|---------|-------------|
| General Search Features | 8 |
| List View Filtering | 4 |
| List View Columns | 6 |
| Interactions Columns | 3 |
| Contact List Columns | 3 |
| Partner Navigation | 4 |
| Pagination | 4 |
| Export Functionality | 3 |
| **TOTAL** | **35** |
