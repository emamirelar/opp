/**
 * @fileoverview Partner, Contact & Logo feature specification.
 * Covers PNO-148 (Logo display), PNO-797 (Contacts page duplicate key), PNO-933 (Mass import org unit mapping).
 * @author UNOPS Opportunity+ QA Team
 */

namespace UNOPS.PAO.Business.Tests.PartnerContactLogo;

/// <summary>
/// Specification for Partner, Contact and Logo feature tests.
/// Requirements: PNO-148, PNO-797, PNO-933.
/// </summary>
public static class PartnerContactLogoSpec
{
    public const string PartnerEntity = "Partner";
    public const string ContactEntity = "Contact";

    #region API Endpoints

    public static string GetPartnerEndpoint(int id) => $"/api/partner/{id}";
    public static string GetPartnersEndpoint => "/api/partner?pageIndex=1&pageSize=10";
    public static string GetPartnerLogoEndpoint(int id) => $"/api/partner/{id}/logo";
    public static string PostPartnerLogoEndpoint(int id) => $"/api/partner/{id}/logo";

    public static string GetContactEndpoint(int id) => $"/api/contact/{id}";
    public static string GetContactsEndpoint => "/api/contact?pageIndex=1&pageSize=10";
    public static string GetContactPhotoEndpoint(int id) => $"/api/contact/{id}/photo";
    public static string PutContactPhotoEndpoint(int id) => $"/api/contact/{id}/photo";

    public static string GetContactImportEndpoint => "/api/contact/import";

    #endregion

    #region PNO-148 Requirements (Logo Display)

    /// <summary>
    /// PNO-148: Partner and Contact logos must display correctly.
    /// - Logo/photo appears immediately after upload (no page refresh)
    /// - Correct aspect ratio in details page
    /// - Logo displayed in Advanced Search results
    /// - Logo displayed in Global Search results (when applicable)
    /// </summary>
    public static class PNO148
    {
        public const string LogoImmediateDisplay = "Logo appears immediately after upload without refresh";
        public const string CorrectAspectRatio = "Logo maintains correct aspect ratio";
        public const string DetailsPageDisplay = "Logo displays on partner/contact details page";
        public const string SearchResultsDisplay = "Logo displays in search results (advanced and global)";
    }

    #endregion

    #region PNO-797 Requirements (Contacts Page Duplicate Key)

    /// <summary>
    /// PNO-797: Contacts page must load without Error 400 "An item with the same key has already been added".
    /// Root cause: ToDictionary(u => u.UserId) or ToDictionary(o => o.Code) throws when duplicates exist.
    /// Fix: Use GroupBy().ToDictionary(g => g.Key, g => g.First()) to handle duplicates.
    /// </summary>
    public static class PNO797
    {
        public const string ContactsPageLoads = "Contacts page loads successfully for all user roles";
        public const string NoDuplicateKeyError = "No 'An item with the same key has already been added' exception";
        public const string PaginationWorks = "Contact list pagination returns valid records";
    }

    #endregion

    #region PNO-933 Requirements (Mass Import Org Unit Mapping)

    /// <summary>
    /// PNO-933: Mass import of Contacts must map Org Unit correctly.
    /// - Org Unit column recognized in import dialog
    /// - Org Unit persisted to OrganizationUnitRelationship (not just Department field)
    /// - Different org units correctly identified and mapped during import
    /// </summary>
    public static class PNO933
    {
        public const string OrgUnitInImportDialog = "Org Unit column appears in mass import mapping dialog";
        public const string OrgUnitPersisted = "Imported contacts have Org Unit in OrganizationUnitRelationship";
        public const string OrgUnitNotInDepartment = "Org Unit not incorrectly placed in Department field";
        public const string MultipleOrgUnitsMapped = "Different org units correctly mapped per contact";
    }

    #endregion

    #region Field Constraints

    public const int LogoMaxSizeBytes = 1_048_576; // 1MB
    public static readonly string[] AllowedLogoExtensions = { "jpg", "jpeg", "png", "webp" };
    public const int ProfilePictureMaxSizeBytes = 1_048_576; // 1MB

    #endregion
}
