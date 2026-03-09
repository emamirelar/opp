/**
 * @fileoverview Related Section, Documents & Search test specification.
 * Covers PNO-810 (Related people descriptions), PNO-806 (.docx conversion),
 * PNO-812 (Opportunity search filter), PNO-1216 (Document upload).
 * @author UNOPS Opportunity+ QA Team
 *
 * Requirements validated:
 * - PNO-810: RELATED Section people titles match Directory (data from BigQuery) -> DEF-200
 * - PNO-806: .docx document conversion works -> PositiveTests, GetFileType
 * - PNO-812: Search filter retrieves opportunities -> Search tests
 * - PNO-1216: Document upload from G Drive (Word/PDF) -> DEF-199
 *
 * Defects found:
 * - DEF-199: Document upload from Google Drive fails for Word/PDF (PNO-1216)
 * - DEF-200: RELATED Section people titles may differ from Directory (PNO-810)
 */

namespace UNOPS.PAO.Business.Tests.RelatedDocsAndSearch;

/// <summary>
/// Specification constants for Related Section, Documents &amp; Search tests.
/// Requirements: People listed with correct descriptions/roles (PNO-810);
/// .docx conversion works (PNO-806); Search filter retrieves opportunities (PNO-812);
/// Document upload handles various file types (PNO-1216).
/// </summary>
public static class RelatedDocsAndSearchSpec
{
    /// <summary>Opportunity API base path.</summary>
    public const string OpportunityBase = "/api/opportunity";

    /// <summary>Document link endpoint (PNO-1216, DEF-199).</summary>
    public const string DocumentLinkUrl = "/api/document/link";

    /// <summary>Relevant people endpoint (PNO-810, DEF-200).</summary>
    public static string RelevantPeopleUrl(int id, int maxResults = 6) =>
        $"{OpportunityBase}/{id}/relevant-people?maxResults={maxResults}";

    /// <summary>Document API base path pattern: /api/document/{entityName}/{entityId}.</summary>
    public static string DocumentByEntity(string entityName, int entityId) =>
        $"/api/document/{entityName}/{entityId}";

    /// <summary>Opportunity documents endpoint.</summary>
    public static string OpportunityDocuments(int opportunityId) =>
        DocumentByEntity("Opportunity", opportunityId);

    /// <summary>Search endpoint with query parameter.</summary>
    public static string SearchUrl(string query, int page = 1, int pageSize = 10) =>
        $"{OpportunityBase}/search?query={Uri.EscapeDataString(query)}&page={page}&pageSize={pageSize}";

    /// <summary>Search-fields endpoint.</summary>
    public const string SearchFieldsUrl = OpportunityBase + "/search-fields";

    /// <summary>Allowed document extensions per PNO-806, PNO-1216.</summary>
    public static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };

    /// <summary>MIME type for .docx (PNO-806).</summary>
    public const string DocxMimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    /// <summary>MIME type for .pdf.</summary>
    public const string PdfMimeType = "application/pdf";

    /// <summary>Minimum search query length (PNO-812).</summary>
    public const int MinSearchQueryLength = 1;
}
