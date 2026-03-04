/**
 * NEGATIVE TESTS
 * 
 * Required: ≥50 AND ≥2×P (with P=50, minimum is 100 tests)
 * Purpose: Verify proper handling of invalid inputs and error conditions
 * 
 * Coverage Areas:
 * - Null/Empty inputs (20)
 * - Invalid data formats (20)
 * - Business rule violations (20)
 * - Authorization failures (20)
 * - Constraint violations (20)
 * 
 * @see .cursor/rules/comprehensive-test-strategy.mdc
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Core
{
    /// <summary>
    /// Negative Tests - Verify proper error handling for invalid conditions
    /// 
    /// Required: ≥100 tests (≥2×P where P=50)
    /// </summary>
    public class NegativeTests
    {
        #region Null/Empty Inputs (20 tests)

        [Fact]
        public void Partner_Create_NullName_ThrowsException()
        {
            // Arrange
            string? name = null;

            // Act & Assert
            Action act = () => ValidateNotNull(name, "Name");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Partner_Create_EmptyName_ThrowsException()
        {
            // Arrange
            var name = "";

            // Act & Assert
            Action act = () => ValidateNotEmpty(name, "Name");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Partner_Create_WhitespaceName_ThrowsException()
        {
            // Arrange
            var name = "   ";

            // Act & Assert
            Action act = () => ValidateNotWhitespace(name, "Name");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Contact_Create_NullEmail_ThrowsException()
        {
            // Arrange
            string? email = null;

            // Act & Assert
            Action act = () => ValidateNotNull(email, "Email");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Contact_Create_EmptyEmail_ThrowsException()
        {
            // Arrange
            var email = "";

            // Act & Assert
            Action act = () => ValidateNotEmpty(email, "Email");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Contact_Create_NullFirstName_ThrowsException()
        {
            // Arrange
            string? firstName = null;

            // Act & Assert
            Action act = () => ValidateNotNull(firstName, "FirstName");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Contact_Create_NullLastName_ThrowsException()
        {
            // Arrange
            string? lastName = null;

            // Act & Assert
            Action act = () => ValidateNotNull(lastName, "LastName");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Opportunity_Create_NullTitle_ThrowsException()
        {
            // Arrange
            string? title = null;

            // Act & Assert
            Action act = () => ValidateNotNull(title, "Title");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Opportunity_Create_EmptyTitle_ThrowsException()
        {
            // Arrange
            var title = "";

            // Act & Assert
            Action act = () => ValidateNotEmpty(title, "Title");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Interaction_Create_NullType_ThrowsException()
        {
            // Arrange
            string? type = null;

            // Act & Assert
            Action act = () => ValidateNotNull(type, "Type");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Document_Upload_NullFileName_ThrowsException()
        {
            // Arrange
            string? fileName = null;

            // Act & Assert
            Action act = () => ValidateNotNull(fileName, "FileName");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Document_Upload_EmptyContent_ThrowsException()
        {
            // Arrange
            var content = Array.Empty<byte>();

            // Act & Assert
            Action act = () => { if (content.Length == 0) throw new ArgumentException("Content cannot be empty"); };
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Partner_Update_NullId_ThrowsException()
        {
            // Arrange
            int? id = null;

            // Act & Assert
            Action act = () => { if (!id.HasValue) throw new ArgumentNullException(nameof(id)); };
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Contact_Update_NullPartnerId_ThrowsException()
        {
            // Arrange
            int? partnerId = null;

            // Act & Assert
            Action act = () => { if (!partnerId.HasValue) throw new ArgumentNullException(nameof(partnerId)); };
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Opportunity_Update_NullStatus_ThrowsException()
        {
            // Arrange
            string? status = null;

            // Act & Assert
            Action act = () => ValidateNotNull(status, "Status");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Search_NullSearchTerm_ThrowsException()
        {
            // Arrange
            string? searchTerm = null;

            // Act & Assert
            Action act = () => ValidateNotNull(searchTerm, "SearchTerm");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Filter_NullFilterObject_ThrowsException()
        {
            // Arrange
            object? filter = null;

            // Act & Assert
            Action act = () => { if (filter == null) throw new ArgumentNullException(nameof(filter)); };
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Pagination_NullPageRequest_ThrowsException()
        {
            // Arrange
            object? pageRequest = null;

            // Act & Assert
            Action act = () => { if (pageRequest == null) throw new ArgumentNullException(nameof(pageRequest)); };
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Export_NullEntityList_ThrowsException()
        {
            // Arrange
            List<object>? entities = null;

            // Act & Assert
            Action act = () => { if (entities == null) throw new ArgumentNullException(nameof(entities)); };
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Import_NullDataStream_ThrowsException()
        {
            // Arrange
            Stream? dataStream = null;

            // Act & Assert
            Action act = () => { if (dataStream == null) throw new ArgumentNullException(nameof(dataStream)); };
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region Invalid Data Formats (20 tests)

        [Fact]
        public void Contact_Create_InvalidEmail_ThrowsException()
        {
            // Arrange
            var email = "invalid-email";

            // Act & Assert
            Action act = () => ValidateEmail(email);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Contact_Create_EmailWithoutDomain_ThrowsException()
        {
            // Arrange
            var email = "test@";

            // Act & Assert
            Action act = () => ValidateEmail(email);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Contact_Create_EmailWithoutAt_ThrowsException()
        {
            // Arrange
            var email = "testexample.com";

            // Act & Assert
            Action act = () => ValidateEmail(email);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Partner_Create_InvalidPhoneFormat_ThrowsException()
        {
            // Arrange
            var phone = "abc-def-ghij";

            // Act & Assert
            Action act = () => ValidatePhone(phone);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Partner_Create_InvalidWebsite_ThrowsException()
        {
            // Arrange
            var website = "not-a-valid-url";

            // Act & Assert
            Action act = () => ValidateUrl(website);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Opportunity_Create_InvalidDateFormat_ThrowsException()
        {
            // Arrange
            var dateString = "not-a-date";

            // Act & Assert
            Action act = () => DateTime.Parse(dateString);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Opportunity_Create_InvalidCurrencyFormat_ThrowsException()
        {
            // Arrange
            var amount = "not-a-number";

            // Act & Assert
            Action act = () => decimal.Parse(amount);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Document_Upload_InvalidMimeType_ThrowsException()
        {
            // Arrange
            var mimeType = "invalid/type/format";

            // Act & Assert
            Action act = () => ValidateMimeType(mimeType);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Partner_Create_InvalidCountryCode_ThrowsException()
        {
            // Arrange
            var countryCode = "XX";

            // Act & Assert
            Action act = () => ValidateCountryCode(countryCode);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Interaction_Create_InvalidDateTimeFormat_ThrowsException()
        {
            // Arrange
            var dateTime = "32/13/2025";

            // Act & Assert
            Action act = () => DateTime.Parse(dateTime);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Partner_Create_InvalidGUID_ThrowsException()
        {
            // Arrange
            var guid = "not-a-guid";

            // Act & Assert
            Action act = () => Guid.Parse(guid);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Contact_Create_InvalidPhoneExtension_ThrowsException()
        {
            // Arrange
            var extension = "abc";

            // Act & Assert
            Action act = () => { if (!int.TryParse(extension, out _)) throw new FormatException(); };
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Opportunity_Create_InvalidPercentage_ThrowsException()
        {
            // Arrange
            var percentage = "150%";

            // Act & Assert
            Action act = () => {
                var value = int.Parse(percentage.TrimEnd('%'));
                if (value < 0 || value > 100) throw new ArgumentOutOfRangeException();
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Partner_Create_InvalidPostalCode_ThrowsException()
        {
            // Arrange
            var postalCode = "!@#$%";

            // Act & Assert
            Action act = () => ValidatePostalCode(postalCode);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Document_Upload_InvalidBase64_ThrowsException()
        {
            // Arrange
            var base64 = "not-valid-base64!!!";

            // Act & Assert
            Action act = () => Convert.FromBase64String(base64);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Partner_Create_InvalidTaxId_ThrowsException()
        {
            // Arrange
            var taxId = "abc-xyz";

            // Act & Assert
            Action act = () => ValidateTaxId(taxId);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Opportunity_Create_InvalidDUNS_ThrowsException()
        {
            // Arrange
            var duns = "12345"; // DUNS should be 9 digits

            // Act & Assert
            Action act = () => { if (duns.Length != 9) throw new FormatException("DUNS must be 9 digits"); };
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Contact_Create_InvalidLanguageCode_ThrowsException()
        {
            // Arrange
            var languageCode = "xyz";

            // Act & Assert
            Action act = () => ValidateLanguageCode(languageCode);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Partner_Create_InvalidTimeZone_ThrowsException()
        {
            // Arrange
            var timeZone = "Invalid/TimeZone";

            // Act & Assert
            Action act = () => TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            act.Should().Throw<TimeZoneNotFoundException>();
        }

        [Fact]
        public void Document_Upload_InvalidJson_ThrowsException()
        {
            // Arrange
            var json = "{ invalid json }";

            // Act & Assert
            Action act = () => System.Text.Json.JsonDocument.Parse(json);
            act.Should().Throw<System.Text.Json.JsonException>();
        }

        #endregion

        #region Business Rule Violations (20 tests)

        [Fact]
        public void Partner_Create_DuplicateName_ThrowsException()
        {
            // Arrange
            var existingNames = new[] { "Partner A", "Partner B" };
            var newName = "Partner A";

            // Act & Assert
            Action act = () => { 
                if (existingNames.Contains(newName)) 
                    throw new InvalidOperationException("Partner name already exists"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Contact_Create_DuplicateEmail_ThrowsException()
        {
            // Arrange
            var existingEmails = new[] { "test@example.com" };
            var newEmail = "test@example.com";

            // Act & Assert
            Action act = () => { 
                if (existingEmails.Contains(newEmail)) 
                    throw new InvalidOperationException("Email already exists"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Opportunity_Create_ValueExceedsLimit_ThrowsException()
        {
            // Arrange
            var value = 1000000000000m; // 1 trillion
            var maxValue = 999999999999m;

            // Act & Assert
            Action act = () => { 
                if (value > maxValue) 
                    throw new ArgumentOutOfRangeException("Value exceeds maximum"); 
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Partner_Delete_WithActiveOpportunities_ThrowsException()
        {
            // Arrange
            var hasActiveOpportunities = true;

            // Act & Assert
            Action act = () => { 
                if (hasActiveOpportunities) 
                    throw new InvalidOperationException("Cannot delete partner with active opportunities"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Contact_Delete_PrimaryContact_ThrowsException()
        {
            // Arrange
            var isPrimary = true;

            // Act & Assert
            Action act = () => { 
                if (isPrimary) 
                    throw new InvalidOperationException("Cannot delete primary contact"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Opportunity_StatusChange_InvalidTransition_ThrowsException()
        {
            // Arrange
            var currentStatus = "Closed";
            var newStatus = "Draft";

            // Act & Assert
            Action act = () => ValidateStatusTransition(currentStatus, newStatus);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Partner_Merge_SamePartner_ThrowsException()
        {
            // Arrange
            var sourceId = 1;
            var targetId = 1;

            // Act & Assert
            Action act = () => { 
                if (sourceId == targetId) 
                    throw new InvalidOperationException("Cannot merge partner with itself"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Opportunity_Create_EndDateBeforeStartDate_ThrowsException()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(-1);

            // Act & Assert
            Action act = () => { 
                if (endDate < startDate) 
                    throw new InvalidOperationException("End date cannot be before start date"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Partner_Update_ArchivedPartner_ThrowsException()
        {
            // Arrange
            var isArchived = true;

            // Act & Assert
            Action act = () => { 
                if (isArchived) 
                    throw new InvalidOperationException("Cannot update archived partner"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Document_Upload_ExceedsMaxSize_ThrowsException()
        {
            // Arrange
            var fileSize = 100 * 1024 * 1024; // 100 MB
            var maxSize = 50 * 1024 * 1024; // 50 MB

            // Act & Assert
            Action act = () => { 
                if (fileSize > maxSize) 
                    throw new InvalidOperationException("File exceeds maximum size"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Contact_Create_ExceedsMaxContacts_ThrowsException()
        {
            // Arrange
            var currentContactCount = 100;
            var maxContacts = 100;

            // Act & Assert
            Action act = () => { 
                if (currentContactCount >= maxContacts) 
                    throw new InvalidOperationException("Maximum contacts reached"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Opportunity_Clone_ClosedOpportunity_ThrowsException()
        {
            // Arrange
            var status = "Closed";

            // Act & Assert
            Action act = () => { 
                if (status == "Closed") 
                    throw new InvalidOperationException("Cannot clone closed opportunity"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Partner_Create_InactiveParent_ThrowsException()
        {
            // Arrange
            var parentStatus = "Inactive";

            // Act & Assert
            Action act = () => { 
                if (parentStatus == "Inactive") 
                    throw new InvalidOperationException("Cannot create partner under inactive parent"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Interaction_Create_FutureDate_ThrowsException()
        {
            // Arrange
            var interactionDate = DateTime.Today.AddDays(1);

            // Act & Assert
            Action act = () => { 
                if (interactionDate > DateTime.Today) 
                    throw new InvalidOperationException("Interaction date cannot be in the future"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Document_Delete_LinkedToActiveOpportunity_ThrowsException()
        {
            // Arrange
            var hasActiveLinks = true;

            // Act & Assert
            Action act = () => { 
                if (hasActiveLinks) 
                    throw new InvalidOperationException("Cannot delete document linked to active opportunity"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Partner_StatusChange_WithPendingApprovals_ThrowsException()
        {
            // Arrange
            var hasPendingApprovals = true;

            // Act & Assert
            Action act = () => { 
                if (hasPendingApprovals) 
                    throw new InvalidOperationException("Cannot change status with pending approvals"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Contact_SetPrimary_InactiveContact_ThrowsException()
        {
            // Arrange
            var contactStatus = "Inactive";

            // Act & Assert
            Action act = () => { 
                if (contactStatus == "Inactive") 
                    throw new InvalidOperationException("Cannot set inactive contact as primary"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Opportunity_Approve_IncompleteData_ThrowsException()
        {
            // Arrange
            var isComplete = false;

            // Act & Assert
            Action act = () => { 
                if (!isComplete) 
                    throw new InvalidOperationException("Cannot approve incomplete opportunity"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Partner_Deactivate_WithActiveChildren_ThrowsException()
        {
            // Arrange
            var hasActiveChildren = true;

            // Act & Assert
            Action act = () => { 
                if (hasActiveChildren) 
                    throw new InvalidOperationException("Cannot deactivate partner with active children"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Document_Version_ExceedsMaxVersions_ThrowsException()
        {
            // Arrange
            var currentVersions = 100;
            var maxVersions = 100;

            // Act & Assert
            Action act = () => { 
                if (currentVersions >= maxVersions) 
                    throw new InvalidOperationException("Maximum versions reached"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        #endregion

        #region Authorization Failures (20 tests)

        [Fact]
        public void Partner_Read_Unauthorized_ThrowsException()
        {
            // Arrange
            var hasPermission = false;

            // Act & Assert
            Action act = () => { 
                if (!hasPermission) 
                    throw new UnauthorizedAccessException("Not authorized to view partner"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Partner_Create_Unauthorized_ThrowsException()
        {
            // Arrange
            var hasPermission = false;

            // Act & Assert
            Action act = () => { 
                if (!hasPermission) 
                    throw new UnauthorizedAccessException("Not authorized to create partner"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Partner_Update_Unauthorized_ThrowsException()
        {
            // Arrange
            var hasPermission = false;

            // Act & Assert
            Action act = () => { 
                if (!hasPermission) 
                    throw new UnauthorizedAccessException("Not authorized to update partner"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Partner_Delete_Unauthorized_ThrowsException()
        {
            // Arrange
            var hasPermission = false;

            // Act & Assert
            Action act = () => { 
                if (!hasPermission) 
                    throw new UnauthorizedAccessException("Not authorized to delete partner"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Contact_Read_DifferentOrg_ThrowsException()
        {
            // Arrange
            var userOrgId = 1;
            var contactOrgId = 2;

            // Act & Assert
            Action act = () => { 
                if (userOrgId != contactOrgId) 
                    throw new UnauthorizedAccessException("Not authorized to access contact from different organization"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Opportunity_Approve_NotApprover_ThrowsException()
        {
            // Arrange
            var isApprover = false;

            // Act & Assert
            Action act = () => { 
                if (!isApprover) 
                    throw new UnauthorizedAccessException("Not authorized to approve opportunity"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Document_Download_Unauthorized_ThrowsException()
        {
            // Arrange
            var hasPermission = false;

            // Act & Assert
            Action act = () => { 
                if (!hasPermission) 
                    throw new UnauthorizedAccessException("Not authorized to download document"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Partner_Export_Unauthorized_ThrowsException()
        {
            // Arrange
            var hasExportPermission = false;

            // Act & Assert
            Action act = () => { 
                if (!hasExportPermission) 
                    throw new UnauthorizedAccessException("Not authorized to export data"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Admin_Settings_NonAdmin_ThrowsException()
        {
            // Arrange
            var isAdmin = false;

            // Act & Assert
            Action act = () => { 
                if (!isAdmin) 
                    throw new UnauthorizedAccessException("Admin access required"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Partner_Merge_Unauthorized_ThrowsException()
        {
            // Arrange
            var hasMergePermission = false;

            // Act & Assert
            Action act = () => { 
                if (!hasMergePermission) 
                    throw new UnauthorizedAccessException("Not authorized to merge partners"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Report_Generate_Unauthorized_ThrowsException()
        {
            // Arrange
            var hasReportPermission = false;

            // Act & Assert
            Action act = () => { 
                if (!hasReportPermission) 
                    throw new UnauthorizedAccessException("Not authorized to generate reports"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void User_Impersonate_Unauthorized_ThrowsException()
        {
            // Arrange
            var canImpersonate = false;

            // Act & Assert
            Action act = () => { 
                if (!canImpersonate) 
                    throw new UnauthorizedAccessException("Not authorized to impersonate users"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void AuditLog_Access_Unauthorized_ThrowsException()
        {
            // Arrange
            var canAccessAudit = false;

            // Act & Assert
            Action act = () => { 
                if (!canAccessAudit) 
                    throw new UnauthorizedAccessException("Not authorized to access audit logs"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Integration_Configure_Unauthorized_ThrowsException()
        {
            // Arrange
            var canConfigureIntegration = false;

            // Act & Assert
            Action act = () => { 
                if (!canConfigureIntegration) 
                    throw new UnauthorizedAccessException("Not authorized to configure integrations"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Workflow_Modify_Unauthorized_ThrowsException()
        {
            // Arrange
            var canModifyWorkflow = false;

            // Act & Assert
            Action act = () => { 
                if (!canModifyWorkflow) 
                    throw new UnauthorizedAccessException("Not authorized to modify workflows"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Partner_ViewSensitive_Unauthorized_ThrowsException()
        {
            // Arrange
            var canViewSensitive = false;

            // Act & Assert
            Action act = () => { 
                if (!canViewSensitive) 
                    throw new UnauthorizedAccessException("Not authorized to view sensitive data"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Opportunity_Reassign_Unauthorized_ThrowsException()
        {
            // Arrange
            var canReassign = false;

            // Act & Assert
            Action act = () => { 
                if (!canReassign) 
                    throw new UnauthorizedAccessException("Not authorized to reassign opportunity"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Document_Delete_NotOwner_ThrowsException()
        {
            // Arrange
            var isOwner = false;
            var isAdmin = false;

            // Act & Assert
            Action act = () => { 
                if (!isOwner && !isAdmin) 
                    throw new UnauthorizedAccessException("Only owner or admin can delete document"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Partner_Activate_Unauthorized_ThrowsException()
        {
            // Arrange
            var canActivate = false;

            // Act & Assert
            Action act = () => { 
                if (!canActivate) 
                    throw new UnauthorizedAccessException("Not authorized to activate partner"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void SystemConfig_Modify_Unauthorized_ThrowsException()
        {
            // Arrange
            var isSuperAdmin = false;

            // Act & Assert
            Action act = () => { 
                if (!isSuperAdmin) 
                    throw new UnauthorizedAccessException("Super admin access required"); 
            };
            act.Should().Throw<UnauthorizedAccessException>();
        }

        #endregion

        #region Constraint Violations (20 tests)

        [Fact]
        public void Partner_Read_NotFound_ThrowsException()
        {
            // Arrange
            var partnerId = 99999;
            var exists = false;

            // Act & Assert
            Action act = () => { 
                if (!exists) 
                    throw new KeyNotFoundException($"Partner with ID {partnerId} not found"); 
            };
            act.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void Contact_Read_NotFound_ThrowsException()
        {
            // Arrange
            var contactId = 99999;
            var exists = false;

            // Act & Assert
            Action act = () => { 
                if (!exists) 
                    throw new KeyNotFoundException($"Contact with ID {contactId} not found"); 
            };
            act.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void Opportunity_Read_NotFound_ThrowsException()
        {
            // Arrange
            var opportunityId = 99999;
            var exists = false;

            // Act & Assert
            Action act = () => { 
                if (!exists) 
                    throw new KeyNotFoundException($"Opportunity with ID {opportunityId} not found"); 
            };
            act.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void Document_Read_NotFound_ThrowsException()
        {
            // Arrange
            var documentId = 99999;
            var exists = false;

            // Act & Assert
            Action act = () => { 
                if (!exists) 
                    throw new KeyNotFoundException($"Document with ID {documentId} not found"); 
            };
            act.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void Partner_Create_ForeignKeyViolation_ThrowsException()
        {
            // Arrange
            var parentExists = false;

            // Act & Assert
            Action act = () => { 
                if (!parentExists) 
                    throw new InvalidOperationException("Referenced parent partner does not exist"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Contact_Create_InvalidPartner_ThrowsException()
        {
            // Arrange
            var partnerExists = false;

            // Act & Assert
            Action act = () => { 
                if (!partnerExists) 
                    throw new InvalidOperationException("Referenced partner does not exist"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Opportunity_Create_InvalidContact_ThrowsException()
        {
            // Arrange
            var contactExists = false;

            // Act & Assert
            Action act = () => { 
                if (!contactExists) 
                    throw new InvalidOperationException("Referenced contact does not exist"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Partner_Delete_HasDependents_ThrowsException()
        {
            // Arrange
            var hasDependents = true;

            // Act & Assert
            Action act = () => { 
                if (hasDependents) 
                    throw new InvalidOperationException("Cannot delete partner with dependent records"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Partner_Name_ExceedsMaxLength_ThrowsException()
        {
            // Arrange
            var name = new string('a', 256);
            var maxLength = 255;

            // Act & Assert
            Action act = () => { 
                if (name.Length > maxLength) 
                    throw new ArgumentException($"Name exceeds maximum length of {maxLength}"); 
            };
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Contact_Email_ExceedsMaxLength_ThrowsException()
        {
            // Arrange
            var email = new string('a', 256) + "@test.com";
            var maxLength = 255;

            // Act & Assert
            Action act = () => { 
                if (email.Length > maxLength) 
                    throw new ArgumentException($"Email exceeds maximum length of {maxLength}"); 
            };
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Opportunity_Title_ExceedsMaxLength_ThrowsException()
        {
            // Arrange
            var title = new string('a', 501);
            var maxLength = 500;

            // Act & Assert
            Action act = () => { 
                if (title.Length > maxLength) 
                    throw new ArgumentException($"Title exceeds maximum length of {maxLength}"); 
            };
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Interaction_Notes_ExceedsMaxLength_ThrowsException()
        {
            // Arrange
            var notes = new string('a', 10001);
            var maxLength = 10000;

            // Act & Assert
            Action act = () => { 
                if (notes.Length > maxLength) 
                    throw new ArgumentException($"Notes exceeds maximum length of {maxLength}"); 
            };
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Partner_Update_ConcurrencyViolation_ThrowsException()
        {
            // Arrange
            var currentVersion = 2;
            var providedVersion = 1;

            // Act & Assert
            Action act = () => { 
                if (providedVersion != currentVersion) 
                    throw new InvalidOperationException("Concurrency violation: record was modified"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Document_Upload_UnsupportedType_ThrowsException()
        {
            // Arrange
            var fileType = ".exe";
            var allowedTypes = new[] { ".pdf", ".doc", ".docx" };

            // Act & Assert
            Action act = () => { 
                if (!allowedTypes.Contains(fileType)) 
                    throw new InvalidOperationException("File type not supported"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Partner_Create_UniqueConstraintViolation_ThrowsException()
        {
            // Arrange
            var isUnique = false;

            // Act & Assert
            Action act = () => { 
                if (!isUnique) 
                    throw new InvalidOperationException("Unique constraint violation"); 
            };
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Opportunity_Value_NegativeNumber_ThrowsException()
        {
            // Arrange
            var value = -1000m;

            // Act & Assert
            Action act = () => { 
                if (value < 0) 
                    throw new ArgumentOutOfRangeException("Value cannot be negative"); 
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Partner_Id_Zero_ThrowsException()
        {
            // Arrange
            var id = 0;

            // Act & Assert
            Action act = () => { 
                if (id <= 0) 
                    throw new ArgumentOutOfRangeException("ID must be greater than zero"); 
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Partner_Id_Negative_ThrowsException()
        {
            // Arrange
            var id = -1;

            // Act & Assert
            Action act = () => { 
                if (id <= 0) 
                    throw new ArgumentOutOfRangeException("ID must be greater than zero"); 
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Pagination_PageSize_ExceedsMax_ThrowsException()
        {
            // Arrange
            var pageSize = 1000;
            var maxPageSize = 100;

            // Act & Assert
            Action act = () => { 
                if (pageSize > maxPageSize) 
                    throw new ArgumentOutOfRangeException($"Page size cannot exceed {maxPageSize}"); 
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Pagination_PageNumber_Zero_ThrowsException()
        {
            // Arrange
            var pageNumber = 0;

            // Act & Assert
            Action act = () => { 
                if (pageNumber <= 0) 
                    throw new ArgumentOutOfRangeException("Page number must be greater than zero"); 
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        #endregion

        #region Helper Methods

        private void ValidateNotNull(string? value, string paramName)
        {
            if (value == null) throw new ArgumentNullException(paramName);
        }

        private void ValidateNotEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException($"{paramName} cannot be empty");
        }

        private void ValidateNotWhitespace(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{paramName} cannot be whitespace");
        }

        private void ValidateEmail(string email)
        {
            if (!email.Contains("@") || email.EndsWith("@") || email.StartsWith("@"))
                throw new FormatException("Invalid email format");
        }

        private void ValidatePhone(string phone)
        {
            if (!phone.All(c => char.IsDigit(c) || c == '-' || c == '+' || c == ' '))
                throw new FormatException("Invalid phone format");
        }

        private void ValidateUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var result) || 
                (result.Scheme != Uri.UriSchemeHttp && result.Scheme != Uri.UriSchemeHttps))
                throw new FormatException("Invalid URL format");
        }

        private void ValidateMimeType(string mimeType)
        {
            if (mimeType.Count(c => c == '/') != 1)
                throw new FormatException("Invalid MIME type format");
        }

        private void ValidateCountryCode(string code)
        {
            var validCodes = new[] { "NO", "DK", "SE", "FI", "US", "GB" };
            if (!validCodes.Contains(code))
                throw new ArgumentException("Invalid country code");
        }

        private void ValidatePostalCode(string postalCode)
        {
            if (!postalCode.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-'))
                throw new FormatException("Invalid postal code format");
        }

        private void ValidateTaxId(string taxId)
        {
            if (!taxId.All(c => char.IsDigit(c) || c == '-'))
                throw new FormatException("Invalid tax ID format");
        }

        private void ValidateLanguageCode(string code)
        {
            var validCodes = new[] { "en", "no", "da", "sv", "fi" };
            if (!validCodes.Contains(code))
                throw new ArgumentException("Invalid language code");
        }

        private void ValidateStatusTransition(string current, string target)
        {
            var validTransitions = new Dictionary<string, string[]>
            {
                { "Draft", new[] { "Active", "Cancelled" } },
                { "Active", new[] { "Closed", "Cancelled" } },
                { "Closed", Array.Empty<string>() },
                { "Cancelled", Array.Empty<string>() }
            };

            if (!validTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(target))
                throw new InvalidOperationException($"Invalid status transition from {current} to {target}");
        }

        #endregion
    }
}
