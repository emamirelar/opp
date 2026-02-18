# ContactManager - Business Logic Test Cases

## Manager Overview
**Manager**: `ContactManager` / `UNOPSContactManager`  
**Location**: `UNOPS.PAO.Business/Managers/ContactManager.cs`, `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSContactManager.cs`  
**Purpose**: Manages contacts including CRUD, partner associations, document management, Gmail integration, and interaction tracking.

## Key Business Rules (From PRD)

1. **Contact-Partner Association**: Contacts belong to partner organizations
2. **Email Uniqueness**: Same email can exist for multiple partners (person as contact for multiple organizations)
3. **Interaction Tracking**: Contacts have interaction history (meetings, calls, emails)
4. **Document Management**: Contacts can have associated documents
5. **Gmail Integration**: Contacts can be created/updated via Gmail Add-on
6. **Profile Picture**: Contacts can have profile pictures uploaded
7. **Soft Delete**: Contacts are soft-deleted to preserve historical data

---

## P0 - Critical Business Logic Tests

### TC-CM-BL-P0-001: Create Contact - Partner Association
**Priority**: P0 - Critical  
**Description**: Verify contact is properly associated with partner  
**Business Rule**: Contact PartnerId must reference valid, active partner  
**Preconditions**: Active partner exists

**Test Steps**:
1. Create ContactRequest with valid PartnerId
2. Call `CreateContactAsync(model)`
3. Verify contact created with PartnerId set
4. Query contact and verify partner relationship loaded

**Expected Result**: Contact associated with partner  
**Business Impact**: Core relationship between contacts and organizations

---

### TC-CM-BL-P0-002: Create Contact - Required Fields Validation
**Priority**: P0 - Critical  
**Description**: Verify required fields enforced on creation  
**Business Rule**: Email and LastName are required  
**Preconditions**: None

**Test Steps**:
1. Attempt create without Email - should fail
2. Attempt create without LastName - should fail
3. Create with Email and LastName - should succeed

**Expected Result**: Required field validation enforced  
**Business Impact**: Data quality and integrity

---

### TC-CM-BL-P0-003: Create Contact - Email Format Validation
**Priority**: P0 - Critical  
**Description**: Verify email format validation  
**Business Rule**: Email must be valid format  
**Preconditions**: None

**Test Steps**:
1. Create with valid email "john@example.com" - should succeed
2. Create with invalid email "notanemail" - should fail
3. Create with email "john+tag@example.com" - should succeed

**Expected Result**: Valid email format required  
**Business Impact**: Communication capability

---

### TC-CM-BL-P0-004: Get Contact - Include Documents
**Priority**: P0 - Critical  
**Description**: Verify GetContactAsync includes documents  
**Business Rule**: Contact retrieval should eager load documents  
**Preconditions**: Contact with 5 documents

**Test Steps**:
1. Call `GetContactAsync(contactId)`
2. Verify Documents collection populated
3. Verify all 5 documents loaded

**Expected Result**: Documents included in response  
**Business Impact**: Complete contact information display

---

### TC-CM-BL-P0-005: Get Contact with Interactions - Complete History
**Priority**: P0 - Critical  
**Description**: Verify interaction history loaded  
**Business Rule**: GetContactWithInteractionsAsync includes all interactions  
**Preconditions**: Contact with 20 interactions

**Test Steps**:
1. Call `GetContactWithInteractionsAsync(contactId)`
2. Verify Interactions collection populated
3. Verify all 20 interactions loaded
4. Verify interaction details (date, type, partner)

**Expected Result**: Complete interaction history  
**Business Impact**: Relationship tracking and history

---

### TC-CM-BL-P0-006: Update Contact - Change Partner
**Priority**: P0 - Critical  
**Description**: Verify contact can be moved to different partner  
**Business Rule**: PartnerId can be changed to reassign contact  
**Preconditions**: Contact with Partner A

**Test Steps**:
1. Update contact with new PartnerId (Partner B)
2. Call `UpdateContactAsync(userId, model)`
3. Verify PartnerId changed
4. Verify contact appears in Partner B's contact list
5. Verify contact removed from Partner A's list

**Expected Result**: Partner association updated  
**Business Impact**: Organizational changes support

---

### TC-CM-BL-P0-007: Update Contact - Non-Existent
**Priority**: P0 - Critical  
**Description**: Verify graceful handling of non-existent contact update  
**Business Rule**: Update of non-existent contact returns null  
**Preconditions**: None

**Test Steps**:
1. Call `UpdateContactAsync(userId, model)` with invalid ID
2. Verify returns null (not exception)

**Expected Result**: Null returned for non-existent contact  
**Business Impact**: Error handling robustness

---

### TC-CM-BL-P0-008: Delete Contact - Soft Delete Behavior
**Priority**: P0 - Critical  
**Description**: Verify contact is soft-deleted  
**Business Rule**: Contacts are soft-deleted (IsDeleted=true), not removed  
**Preconditions**: Active contact exists

**Test Steps**:
1. Call `DeleteContactAsync(userId, contactId)`
2. Verify IsDeleted = true in database
3. Verify contact not in active lists
4. Verify contact still in database for audit

**Expected Result**: Contact soft-deleted  
**Business Impact**: Historical data preservation

---

### TC-CM-BL-P0-009: Delete Contact - With Interactions Preserved
**Priority**: P0 - Critical  
**Description**: Verify interactions preserved when contact deleted  
**Business Rule**: Interaction history must survive contact deletion  
**Preconditions**: Contact with 10 interactions

**Test Steps**:
1. Delete contact
2. Verify interactions still exist in database
3. Verify interaction history accessible for reporting

**Expected Result**: Interactions preserved after contact deletion  
**Business Impact**: Historical data integrity

---

### TC-CM-BL-P0-010: Get Partner Contacts - Filtered List
**Priority**: P0 - Critical  
**Description**: Verify GetPartnerContacts returns only partner's contacts  
**Business Rule**: Only contacts with matching PartnerId returned  
**Preconditions**: Partner A with 10 contacts, Partner B with 5 contacts

**Test Steps**:
1. Call `GetPartnerContacts(partnerAId)`
2. Verify exactly 10 contacts returned
3. Verify all have PartnerId = Partner A
4. Verify Partner B contacts not included

**Expected Result**: Correct partner filtering  
**Business Impact**: Data isolation between partners

---

### TC-CM-BL-P0-011: Permission Check - ClaimsPrincipal Access
**Priority**: P0 - Critical  
**Description**: Verify contact access respects user claims  
**Business Rule**: Users can only access contacts they have permission for  
**Preconditions**: Contact exists, users with different permissions

**Test Steps**:
1. User with contact view permission - should succeed
2. User without permission - should fail or return filtered results
3. Admin user - should have full access

**Expected Result**: Permissions correctly enforced  
**Business Impact**: Data security

---

### TC-CM-BL-P0-012: Gmail Integration - Contact Lookup by Email
**Priority**: P0 - Critical  
**Description**: Verify Gmail Add-on can find contacts by email  
**Business Rule**: GetContactsForGmailAddon matches emails to contacts  
**Preconditions**: Contacts with various email addresses

**Test Steps**:
1. Call `GetContactsForGmailAddon(request, user)` with known emails
2. Verify matching contacts returned
3. Verify non-matching emails return empty
4. Verify multiple matches handled

**Expected Result**: Contacts found by email  
**Business Impact**: Gmail Add-on functionality

---

## P1 - High Priority Business Logic Tests

### TC-CM-BL-P1-001: Contact Pagination - Standard Query
**Priority**: P1 - High  
**Description**: Verify pagination works correctly  
**Business Rule**: PaginationRequest parameters applied correctly  
**Preconditions**: 100 contacts exist

**Test Steps**:
1. Request PageIndex=1, PageSize=20
2. Verify 20 records returned
3. Verify TotalCount=100
4. Request PageIndex=5
5. Verify different 20 records returned

**Expected Result**: Correct pagination  
**Business Impact**: Performance and usability

---

### TC-CM-BL-P1-002: Contact Specification - Email Domain Filter
**Priority**: P1 - High  
**Description**: Verify specification pattern filtering  
**Business Rule**: Specifications filter correctly  
**Preconditions**: Contacts with various email domains

**Test Steps**:
1. Create specification for email domain "example.com"
2. Call `GetContactsWithSpecificationAsync`
3. Verify only @example.com contacts returned

**Expected Result**: Domain-based filtering works  
**Business Impact**: Advanced search functionality

---

### TC-CM-BL-P1-003: Posted Contacts - Active Only
**Priority**: P1 - High  
**Description**: Verify GetPostedContacts returns only active  
**Business Rule**: Deleted contacts excluded from posted list  
**Preconditions**: Mix of active and deleted contacts

**Test Steps**:
1. Call `GetPostedContacts()`
2. Verify no deleted contacts in result
3. Verify all active contacts included

**Expected Result**: Only active contacts returned  
**Business Impact**: Clean public data

---

### TC-CM-BL-P1-004: Contact by Email - Exact Match
**Priority**: P1 - High  
**Description**: Verify email lookup returns exact match  
**Business Rule**: GetContactByEmailAsync returns single match  
**Preconditions**: Contact with email "john@example.com"

**Test Steps**:
1. Call `GetContactByEmailAsync(user, "john@example.com")`
2. Verify correct contact returned
3. Call with "john@example" - should not match
4. Call with "JOHN@EXAMPLE.COM" - verify case handling

**Expected Result**: Exact email matching  
**Business Impact**: Accurate lookups

---

### TC-CM-BL-P1-005: Contact Update - Audit Fields
**Priority**: P1 - High  
**Description**: Verify audit fields updated on change  
**Business Rule**: LastModifiedBy/Date updated on any change  
**Preconditions**: Contact exists

**Test Steps**:
1. Update contact as User A
2. Verify LastModifiedBy = User A
3. Verify LastModifiedDate = now
4. Verify CreatedBy unchanged

**Expected Result**: Audit trail maintained  
**Business Impact**: Change tracking

---

### TC-CM-BL-P1-006: Profile Picture Upload - File Handling
**Priority**: P1 - High  
**Description**: Verify profile picture upload process  
**Business Rule**: Valid image files accepted and stored  
**Preconditions**: Contact exists

**Test Steps**:
1. Upload JPG file
2. Verify file saved (or handled appropriately - stub returns null)
3. Verify contact profile updated

**Expected Result**: File upload handled  
**Business Impact**: Visual contact identification

---

### TC-CM-BL-P1-007: Unmatched Emails - Partner Suggestions
**Priority**: P1 - High  
**Description**: Verify partner suggestions for unknown emails  
**Business Rule**: System suggests partners for unmatched email domains  
**Preconditions**: Partners with various email domains

**Test Steps**:
1. Call `GetUnmatchedEmailsWithPartnerSuggestionsAsync`
2. Verify suggestions based on domain matching
3. Verify @unops.org handled specially

**Expected Result**: Partner suggestions returned  
**Business Impact**: Data enrichment during import

---

### TC-CM-BL-P1-008: Contact Search Fields - Metadata
**Priority**: P1 - High  
**Description**: Verify search field metadata returned  
**Business Rule**: GetContactSearchFields returns searchable fields  
**Preconditions**: None

**Test Steps**:
1. Call `GetContactSearchFields()`
2. Verify includes FirstName, LastName, Email
3. Verify field types specified

**Expected Result**: Search metadata available  
**Business Impact**: Dynamic search UI building

---

### TC-CM-BL-P1-009: Posted Contact - With Eligible Entities
**Priority**: P1 - High  
**Description**: Verify eligible entities loaded  
**Business Rule**: GetPostedContact includes EligibleEntities  
**Preconditions**: Contact with eligible entities defined

**Test Steps**:
1. Call `GetPostedContact(contactId)`
2. Verify EligibleEntities collection loaded
3. Verify entities are correct

**Expected Result**: Eligible entities included  
**Business Impact**: External user visibility rules

---

### TC-CM-BL-P1-010: Duplicate Email - Different Partners
**Priority**: P1 - High  
**Description**: Verify same email allowed for different partners  
**Business Rule**: Same person can be contact for multiple partners  
**Preconditions**: Contact "john@example.com" for Partner A

**Test Steps**:
1. Create contact with "john@example.com" for Partner B
2. Verify created successfully
3. Verify both contacts exist with same email

**Expected Result**: Duplicate email allowed across partners  
**Business Impact**: Real-world contact modeling

---

## P2 - Medium Priority Business Logic Tests

### TC-CM-BL-P2-001: Contact with All Fields
**Priority**: P2 - Medium  
**Description**: Verify all optional fields saved  
**Test Steps**:
1. Create contact with all fields populated
2. Retrieve and verify all fields preserved

**Expected Result**: Complete data persistence

---

### TC-CM-BL-P2-002: Contact Name - Special Characters
**Priority**: P2 - Medium  
**Description**: Verify special characters in names  
**Test Steps**:
1. Create contact "François O'Brien-Smythe"
2. Verify stored correctly
3. Verify searchable

**Expected Result**: Special characters preserved

---

### TC-CM-BL-P2-003: Contact Without Partner
**Priority**: P2 - Medium  
**Description**: Verify contact can exist without partner  
**Test Steps**:
1. Create contact with PartnerId = null
2. Verify created successfully
3. Verify appears in orphaned contacts list

**Expected Result**: Orphaned contact allowed

---

### TC-CM-BL-P2-004: Contact Salutation
**Priority**: P2 - Medium  
**Description**: Verify salutation field handling  
**Test Steps**:
1. Create with Salutation = "Dr."
2. Verify stored correctly
3. Update salutation
4. Verify updated

**Expected Result**: Salutation managed correctly

---

### TC-CM-BL-P2-005: Contact Mobile vs Phone
**Priority**: P2 - Medium  
**Description**: Verify both phone fields work  
**Test Steps**:
1. Create with Mobile and Phone
2. Verify both stored
3. Search by phone number

**Expected Result**: Multiple phone fields supported

---

## P3 - Low Priority Edge Cases

### TC-CM-BL-P3-001: Very Long Name
**Priority**: P3 - Low  
**Description**: Verify maximum length name handling  
**Test Steps**:
1. Create contact with 255-character name
2. Verify stored or truncated appropriately

**Expected Result**: Long names handled

---

### TC-CM-BL-P3-002: Unicode in Email
**Priority**: P3 - Low  
**Description**: Verify internationalized email handling  
**Test Steps**:
1. Create with unicode email domain
2. Verify handling (accept or reject with clear error)

**Expected Result**: Unicode handled appropriately

---

### TC-CM-BL-P3-003: Empty Partner Contact List
**Priority**: P3 - Low  
**Description**: Verify partner with no contacts  
**Test Steps**:
1. Query contacts for partner with none
2. Verify empty list returned
3. Verify no errors

**Expected Result**: Empty list returned gracefully

---

## Integration with Unit Tests

These test cases should be implemented in:
`tests/UNOPS.PAO.Business.Tests/Managers/ContactManagerBusinessLogicTests.cs`

---

## Related Documentation

- [Contact Entity](../../UNOPS.PAO.Domain/Entities/Contact.cs)
- [Contact Models](../../UNOPS.PAO.Models/Contacts/)
- [Existing Test Cases](../Business/ContactManager/ContactManager_TestCases.md)

