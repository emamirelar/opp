# UNOPSPartnerTreeManager - Unit Test Cases

**Manager**: `UNOPSPartnerTreeManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSPartnerTreeManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `UNOPSPartnerTreeManager` with focus on:
- Hierarchical tree management
- Parent-child relationships
- Tree traversal
- Hierarchy validation
- Circular reference prevention

**Total Test Cases**: 15+

---

## 1. Tree Structure Tests

### TC-PT-001: Build Partner Tree
**Test**: `BuildTree_Should_CreateHierarchy_When_PartnersHaveParentChild`

### TC-PT-002: Get Root Partners
**Test**: `GetRoots_Should_ReturnTopLevel_When_PartnersHaveNoParent`

### TC-PT-003: Get Children
**Test**: `GetChildren_Should_ReturnDirectChildren_When_PartnerHasChildren`

### TC-PT-004: Get All Descendants
**Test**: `GetDescendants_Should_ReturnAllLevels_When_PartnerHasNestedChildren`

### TC-PT-005: Get Parent Path
**Test**: `GetParentPath_Should_ReturnAncestors_When_PartnerHasParents`

---

## 2. Relationship Management Tests

### TC-PT-006: Add Child Partner
**Test**: `AddChild_Should_CreateRelationship_When_ValidParentAndChild`

### TC-PT-007: Remove Child Partner
**Test**: `RemoveChild_Should_BreakRelationship_When_RelationshipExists`

### TC-PT-008: Move Partner
**Test**: `MovePartner_Should_ChangeParent_When_NewParentValid`

### TC-PT-009: Prevent Circular Reference
**Test**: `MovePartner_Should_ThrowException_When_WouldCreateCircularRef`

### TC-PT-010: Update Parent
**Test**: `UpdateParent_Should_ModifyRelationship_When_ValidParentProvided`

---

## 3. Tree Traversal Tests

### TC-PT-011: Depth-First Traversal
**Test**: `TraverseDepthFirst_Should_VisitAllNodes_When_TreeExists`

### TC-PT-012: Breadth-First Traversal
**Test**: `TraverseBreadthFirst_Should_VisitLevelByLevel_When_TreeExists`

### TC-PT-013: Get Tree Depth
**Test**: `GetDepth_Should_ReturnMaxLevel_When_TreeHasMultipleLevels`

### TC-PT-014: Find Partner in Tree
**Test**: `FindInTree_Should_LocatePartner_When_PartnerExistsInHierarchy`

### TC-PT-015: Get Siblings
**Test**: `GetSiblings_Should_ReturnPartnersWithSameParent_When_SiblingsExist`

---

## Coverage Goals
**Overall**: 15+ tests, 70%+ coverage

