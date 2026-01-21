# InteractionManager - Business Logic Test Cases

## Manager Overview
**Manager**: `InteractionManager` / `UNOPSInteractionManager`  
**Location**: `UNOPS.PAO.Business/Managers/InteractionManager.cs`, `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSInteractionManager.cs`  
**Purpose**: Manages interactions (meetings, calls, emails, visits) between contacts and the organization.

## Key Business Rules (From PRD)

1. **Interaction Types**: InPersonMeeting, VirtualMeeting, Call, Email, Chat, SiteVisit
2. **Contact Association**: Interactions linked to one or more contacts via InteractionContact
3. **Date Tracking**: FromDate/ToDate for meetings, Date for general timestamp
4. **Document Attachment**: Interactions can have documents attached
5. **Gmail Integration**: Interactions can be created from Gmail emails
6. **Status Lifecycle**: Interactions have EntityStatus (Active, Closed, etc.)
7. **Soft Delete**: Interactions are soft-deleted to preserve history

---

## P0 - Critical Business Logic Tests

### TC-IM-BL-P0-001: Create Interaction - Valid Meeting
**Priority**: P0 - Critical  
**Description**: Verify meeting interaction creation  
**Business Rule**: Meetings require Subject, Type, and Date  
**Preconditions**: Contact exists

**Test Steps**:
1. Create InteractionRequest with Type = InPersonMeeting
2. Set Subject, Date, FromDate, ToDate
3. Associate with contact
4. Call `CreateInteractionAsync(model)`
5. Verify interaction created

**Expected Result**: Meeting interaction created  
**Business Impact**: Core relationship tracking

---

### TC-IM-BL-P0-002: Create Interaction - Type Validation
**Priority**: P0 - Critical  
**Description**: Verify interaction type is valid  
**Business Rule**: Type must be valid InteractionType enum value  
**Preconditions**: None

**Test Steps**:
1. Create with Type = InPersonMeeting - should succeed
2. Create with Type = Email - should succeed
3. Create with invalid type - should fail validation

**Expected Result**: Valid types accepted, invalid rejected  
**Business Impact**: Data classification accuracy

---

### TC-IM-BL-P0-003: Create Interaction - Contact Association
**Priority**: P0 - Critical  
**Description**: Verify interaction-contact relationship  
**Business Rule**: Interactions must be linked to at least one contact  
**Preconditions**: Multiple contacts exist

**Test Steps**:
1. Create interaction with single contact
2. Verify InteractionContact record created
3. Create interaction with multiple contacts
4. Verify all relationships created

**Expected Result**: Contact associations created correctly  
**Business Impact**: Relationship tracking accuracy

---

### TC-IM-BL-P0-004: Get Interaction - Include Related Data
**Priority**: P0 - Critical  
**Description**: Verify interaction retrieval includes related entities  
**Business Rule**: Eager load contacts, documents, partner info  
**Preconditions**: Interaction with contacts and documents

**Test Steps**:
1. Call `GetInteractionAsync(interactionId)`
2. Verify contacts loaded
3. Verify documents loaded
4. Verify partner info included

**Expected Result**: Complete interaction data returned  
**Business Impact**: Complete history display

---

### TC-IM-BL-P0-005: Update Interaction - Change Type
**Priority**: P0 - Critical  
**Description**: Verify interaction type can be changed  
**Business Rule**: Type change should be allowed  
**Preconditions**: Interaction of type InPersonMeeting

**Test Steps**:
1. Update interaction type to VirtualMeeting
2. Call `UpdateInteractionAsync`
3. Verify type changed
4. Verify other fields preserved

**Expected Result**: Type successfully changed  
**Business Impact**: Data correction capability

---

### TC-IM-BL-P0-006: Delete Interaction - Soft Delete
**Priority**: P0 - Critical  
**Description**: Verify interaction is soft-deleted  
**Business Rule**: Interactions must be soft-deleted for audit  
**Preconditions**: Active interaction exists

**Test Steps**:
1. Call `DeleteInteractionAsync(interactionId)`
2. Verify IsDeleted = true
3. Verify interaction not in active lists
4. Verify record still in database

**Expected Result**: Soft delete applied  
**Business Impact**: Audit trail preservation

---

### TC-IM-BL-P0-007: Interaction Date Range - Meeting Duration
**Priority**: P0 - Critical  
**Description**: Verify FromDate/ToDate handling for meetings  
**Business Rule**: Meeting duration captured via date range  
**Preconditions**: None

**Test Steps**:
1. Create meeting with FromDate < ToDate
2. Verify both dates stored
3. Create with FromDate > ToDate - should fail or swap
4. Verify duration calculation correct

**Expected Result**: Valid date ranges accepted  
**Business Impact**: Calendar integration accuracy

---

### TC-IM-BL-P0-008: Gmail Integration - Create from Email
**Priority**: P0 - Critical  
**Description**: Verify interaction creation from Gmail  
**Business Rule**: Gmail Add-on can create Email interactions  
**Preconditions**: Contact exists with matching email

**Test Steps**:
1. Call Gmail integration method with email data
2. Verify Email type interaction created
3. Verify subject from email
4. Verify contact associated

**Expected Result**: Interaction created from email  
**Business Impact**: Gmail Add-on core functionality

---

### TC-IM-BL-P0-009: Permission Check - User Access
**Priority**: P0 - Critical  
**Description**: Verify interaction access permissions  
**Business Rule**: Users can only access interactions they have permission for  
**Preconditions**: Interactions for different org units

**Test Steps**:
1. User in OrgUnit A queries interactions
2. Verify only OrgUnit A interactions visible
3. Admin user sees all

**Expected Result**: Permission-based filtering  
**Business Impact**: Data security

---

### TC-IM-BL-P0-010: Interaction by Contact - Filtered List
**Priority**: P0 - Critical  
**Description**: Verify interactions filtered by contact  
**Business Rule**: Get only interactions for specific contact  
**Preconditions**: Contact with 10 interactions

**Test Steps**:
1. Call GetInteractionsByContact
2. Verify exactly 10 interactions returned
3. Verify all linked to that contact

**Expected Result**: Correct contact filtering  
**Business Impact**: Contact history accuracy

---

## P1 - High Priority Business Logic Tests

### TC-IM-BL-P1-001: Interaction Search - Date Range
**Priority**: P1 - High  
**Description**: Verify date range filtering  
**Business Rule**: Filter interactions by date range  
**Preconditions**: Interactions spanning multiple months

**Test Steps**:
1. Query with date range Jan 1 - Jan 31
2. Verify only January interactions returned
3. Verify boundary dates handled correctly

**Expected Result**: Date range filtering works  
**Business Impact**: Reporting accuracy

---

### TC-IM-BL-P1-002: Interaction Search - By Type
**Priority**: P1 - High  
**Description**: Verify type-based filtering  
**Business Rule**: Filter interactions by type  
**Preconditions**: Mix of interaction types

**Test Steps**:
1. Query for Type = Email
2. Verify only email interactions returned
3. Query for Type = InPersonMeeting
4. Verify only meetings returned

**Expected Result**: Type filtering works  
**Business Impact**: Activity analysis

---

### TC-IM-BL-P1-003: Interaction Pagination
**Priority**: P1 - High  
**Description**: Verify pagination for large result sets  
**Business Rule**: PaginationRequest applies correctly  
**Preconditions**: 100 interactions

**Test Steps**:
1. Query Page 1, Size 20
2. Verify 20 results
3. Verify TotalCount = 100

**Expected Result**: Pagination works correctly  
**Business Impact**: Performance

---

### TC-IM-BL-P1-004: Interaction Update - Add Contacts
**Priority**: P1 - High  
**Description**: Verify adding contacts to existing interaction  
**Business Rule**: Can add more contacts to interaction  
**Preconditions**: Interaction with 1 contact

**Test Steps**:
1. Add 2 more contacts to interaction
2. Call Update
3. Verify now has 3 contacts

**Expected Result**: Contacts added successfully  
**Business Impact**: Multi-person meeting tracking

---

### TC-IM-BL-P1-005: Interaction Update - Remove Contacts
**Priority**: P1 - High  
**Description**: Verify removing contacts from interaction  
**Business Rule**: Can remove contacts from interaction  
**Preconditions**: Interaction with 3 contacts

**Test Steps**:
1. Update with only 1 contact
2. Verify now has 1 contact
3. Verify removed relationships deleted

**Expected Result**: Contacts removed correctly  
**Business Impact**: Data correction

---

### TC-IM-BL-P1-006: Interaction Description - Long Text
**Priority**: P1 - High  
**Description**: Verify long description handling  
**Business Rule**: Description can be lengthy notes  
**Preconditions**: None

**Test Steps**:
1. Create with 5000 character description
2. Verify stored completely
3. Retrieve and verify no truncation

**Expected Result**: Long descriptions supported  
**Business Impact**: Meeting notes capability

---

### TC-IM-BL-P1-007: Interaction Audit Fields
**Priority**: P1 - High  
**Description**: Verify audit trail  
**Business Rule**: CreatedBy/Date and ModifiedBy/Date tracked  
**Preconditions**: Interaction exists

**Test Steps**:
1. Create interaction as User A
2. Verify CreatedBy = User A
3. Update as User B
4. Verify LastModifiedBy = User B

**Expected Result**: Audit fields maintained  
**Business Impact**: Accountability

---

### TC-IM-BL-P1-008: Recent Interactions - Partner View
**Priority**: P1 - High  
**Description**: Verify getting recent interactions for partner  
**Business Rule**: Show recent activity for partner contacts  
**Preconditions**: Partner with contacts having interactions

**Test Steps**:
1. Get recent interactions for partner
2. Verify ordered by date descending
3. Verify includes all partner's contacts' interactions

**Expected Result**: Recent activity shown  
**Business Impact**: Relationship overview

---

### TC-IM-BL-P1-009: Find or Create - Gmail Deduplication
**Priority**: P1 - High  
**Description**: Verify Gmail doesn't create duplicates  
**Business Rule**: Same email should not create duplicate interactions  
**Preconditions**: Email interaction already exists

**Test Steps**:
1. Process same email again via Gmail
2. Verify existing interaction found
3. Verify no duplicate created

**Expected Result**: Duplicate prevention  
**Business Impact**: Data quality

---

### TC-IM-BL-P1-010: Interaction Status - Lifecycle
**Priority**: P1 - High  
**Description**: Verify status transitions  
**Business Rule**: EntityStatus follows valid transitions  
**Preconditions**: Active interaction

**Test Steps**:
1. Verify starts as Active
2. Close interaction
3. Verify status = Closed
4. Verify closed interactions excluded from default queries

**Expected Result**: Status lifecycle works  
**Business Impact**: Status management

---

## P2 - Medium Priority Business Logic Tests

### TC-IM-BL-P2-001: Interaction with Documents
**Priority**: P2 - Medium  
**Description**: Verify document attachment  
**Test Steps**:
1. Create interaction
2. Attach document
3. Verify document linked

**Expected Result**: Documents attachable

---

### TC-IM-BL-P2-002: Virtual Meeting - URL Storage
**Priority**: P2 - Medium  
**Description**: Verify meeting URL stored  
**Test Steps**:
1. Create VirtualMeeting with meeting URL
2. Verify URL stored correctly

**Expected Result**: Meeting URL preserved

---

### TC-IM-BL-P2-003: Interaction Location
**Priority**: P2 - Medium  
**Description**: Verify location field  
**Test Steps**:
1. Create with Location field
2. Verify stored and searchable

**Expected Result**: Location tracked

---

### TC-IM-BL-P2-004: Site Visit Type
**Priority**: P2 - Medium  
**Description**: Verify SiteVisit interaction type  
**Test Steps**:
1. Create SiteVisit type interaction
2. Verify type-specific handling

**Expected Result**: Site visits tracked

---

### TC-IM-BL-P2-005: Chat Interaction
**Priority**: P2 - Medium  
**Description**: Verify Chat type interaction  
**Test Steps**:
1. Create Chat type interaction
2. Verify appropriate for chat logs

**Expected Result**: Chat interactions supported

---

## P3 - Low Priority Edge Cases

### TC-IM-BL-P3-001: Interaction Without Contacts
**Priority**: P3 - Low  
**Description**: Verify handling of orphaned interaction  
**Test Steps**:
1. Create interaction without contacts
2. Verify created or rejected appropriately

**Expected Result**: Handled gracefully

---

### TC-IM-BL-P3-002: Zero Duration Meeting
**Priority**: P3 - Low  
**Description**: Verify FromDate = ToDate handling  
**Test Steps**:
1. Create with FromDate = ToDate
2. Verify handled (allowed or rejected)

**Expected Result**: Edge case handled

---

### TC-IM-BL-P3-003: Future Dated Interaction
**Priority**: P3 - Low  
**Description**: Verify future dates allowed  
**Test Steps**:
1. Create interaction with future date
2. Verify allowed (scheduled meeting)

**Expected Result**: Future dates accepted

---

## Integration with Unit Tests

Implement in: `tests/UNOPS.PAO.Business.Tests/Managers/InteractionManagerBusinessLogicTests.cs`

---

## Related Documentation

- [Interaction Entity](../../UNOPS.PAO.Domain/Entities/Interaction.cs)
- [InteractionType Enum](../../UNOPS.PAO.Domain/Enums/InteractionType.cs)
- [Existing Test Cases](../Business/InteractionManager/InteractionManager_TestCases.md)

