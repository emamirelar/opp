using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Unit.SoftDeleteAuditTests;

/// <summary>
/// Soft Delete Audit Tests — Expose REAL production bugs.
///
/// Each test replicates the EXACT query pattern used in a specific production manager method
/// and proves that soft-deleted records leak through because the query is missing IsDeleted filter.
///
/// These tests WILL FAIL until developers add the missing !IsDeleted filters.
/// They use [Trait("Defect", "DEF-XXX")] so CI can run them in a non-blocking job.
///
/// Requirements validated:
/// - REQ-SOFTDELETE: All entities inheriting from ModifiableDeletableEntity MUST be filtered
///   by IsDeleted in every query. Soft-deleted records must NEVER be returned to callers.
///
/// Defects found:
/// - DEF-232: PartnerManager.GetPartner returns soft-deleted partners (line 251)
/// - DEF-233: PartnerManager.UpdatePartnerAsync operates on soft-deleted partners (line 286)
/// - DEF-234: PartnerManager.GetPartnerAsync returns soft-deleted partners (line 344)
/// - DEF-235: UNOPSPartnerManager.GetPartner returns soft-deleted partners (line 514)
/// - DEF-236: UNOPSPartnerManager.GetBasicPartnerDetailsAsync returns soft-deleted partners (line 542)
/// - DEF-237: UNOPSPartnerManager.GetPartnerWithContactsAndInteractionsAsync returns soft-deleted partners (line 727)
/// - DEF-238: UNOPSPartnerManager.GetPartnerWithContactsAndInteractionsForAIAsync returns soft-deleted partners (line 796)
/// - DEF-239: UNOPSContactManager.GetBasicEntityAsync returns soft-deleted contacts (line 763)
/// - DEF-240: UNOPSContactManager.GetBasicEntityDataAsync returns soft-deleted contacts (line 776)
/// - DEF-241: UNOPSContactManager.GetContactsForGmailAddon returns soft-deleted contacts (line 778)
/// - DEF-242: UNOPSContactManager.GetUnmatchedEmailsWithPartnerSuggestionsAsync returns soft-deleted contacts (line 923)
/// - DEF-243: UNOPSDocumentManager.GetBasicEntityAsync returns soft-deleted documents (line 56)
/// - DEF-244: UNOPSDocumentManager.GetDocumentByIdAsync returns soft-deleted documents (line 381)
/// - DEF-245: UNOPSDocumentManager.GetDocumentParentEntityByIdAsync returns soft-deleted documents (line 358)
/// - DEF-246: UNOPSDocumentManager.GetDocumentDetailsForAiAsync returns soft-deleted documents (line 453)
/// - DEF-247: UNOPSInteractionManager.GetBasicEntityDataAsync returns soft-deleted interactions (line 1076)
/// - DEF-248: OpportunityManager queries (lines 916, 958, 1190) missing IsDeleted on Opportunities
/// - DEF-249: InteractionManager.FindAsync links soft-deleted contacts/partners (lines 66, 94)
/// - DEF-250: UNOPSPartnerManager batch queries include soft-deleted records (lines 313, 320)
/// </summary>
public class SoftDeleteAuditTests : IClassFixture<SoftDeleteAuditTestFixture>
{
    private readonly SoftDeleteAuditTestFixture _fixture;

    public SoftDeleteAuditTests(SoftDeleteAuditTestFixture fixture)
    {
        _fixture = fixture;
    }

    // =========================================================================
    // PARTNER — Missing IsDeleted filters
    // =========================================================================

    /// <summary>
    /// Replicates PartnerManager.GetPartner (line 251):
    ///   PartnerRepository.GetAll().Where(x => x.Id == id).FirstOrDefaultAsync()
    /// Production code uses GetAll() which may not filter IsDeleted, then queries by ID only.
    /// A soft-deleted partner should NOT be returned.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-232")]
    public async Task PartnerManager_GetPartner_ShouldNotReturnSoftDeletedPartner()
    {
        var result = await _fixture.Context.Partners
            .Where(x => x.Id == _fixture.DeletedPartnerId)
            .FirstOrDefaultAsync();

        result.Should().BeNull(
            "PartnerManager.GetPartner (line 251) queries by ID without IsDeleted filter — " +
            "soft-deleted partner should not be returned to caller");
    }

    /// <summary>
    /// Replicates PartnerManager.UpdatePartnerAsync (line 286):
    ///   PartnerRepository.GetAll().Where(x => x.Id == model.Id).FirstOrDefaultAsync()
    /// Allows updating a soft-deleted partner because IsDeleted is not checked.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-233")]
    public async Task PartnerManager_UpdatePartnerAsync_ShouldNotAllowUpdatingSoftDeletedPartner()
    {
        var result = await _fixture.Context.Partners
            .Where(x => x.Id == _fixture.DeletedPartnerId)
            .FirstOrDefaultAsync();

        result.Should().BeNull(
            "PartnerManager.UpdatePartnerAsync (line 286) fetches partner by ID without IsDeleted filter — " +
            "should not be able to update a soft-deleted partner");
    }

    /// <summary>
    /// Replicates PartnerManager.GetPartnerAsync (line 344):
    ///   PartnerRepository.GetAll(includes).Where(x => x.Id == id).FirstOrDefaultAsync()
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-234")]
    public async Task PartnerManager_GetPartnerAsync_ShouldNotReturnSoftDeletedPartner()
    {
        var result = await _fixture.Context.Partners
            .Where(x => x.Id == _fixture.DeletedPartnerId)
            .FirstOrDefaultAsync();

        result.Should().BeNull(
            "PartnerManager.GetPartnerAsync (line 344) queries by ID without IsDeleted filter");
    }

    /// <summary>
    /// Replicates UNOPSPartnerManager.GetPartner (line 514):
    ///   _context.Partners.AsNoTracking()...FirstOrDefaultAsync(p => p.Id == id)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-235")]
    public async Task UNOPSPartnerManager_GetPartner_ShouldNotReturnSoftDeletedPartner()
    {
        var result = await _fixture.Context.Partners
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == _fixture.DeletedPartnerId);

        result.Should().BeNull(
            "UNOPSPartnerManager.GetPartner (line 514) queries Partners by ID without IsDeleted filter — " +
            "returns soft-deleted partner data including PartnerGroup and LiaisonOffice");
    }

    /// <summary>
    /// Replicates UNOPSPartnerManager.GetBasicPartnerDetailsAsync (line 542):
    ///   _context.Partners.AsNoTracking()...FirstOrDefaultAsync(p => p.Id == id)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-236")]
    public async Task UNOPSPartnerManager_GetBasicPartnerDetailsAsync_ShouldNotReturnSoftDeletedPartner()
    {
        var result = await _fixture.Context.Partners
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == _fixture.DeletedPartnerId);

        result.Should().BeNull(
            "UNOPSPartnerManager.GetBasicPartnerDetailsAsync (line 542) returns soft-deleted partner details — " +
            "used for partner detail pages, exposing deleted data to UI");
    }

    /// <summary>
    /// Replicates UNOPSPartnerManager.GetPartnerWithContactsAndInteractionsAsync (line 727):
    ///   _context.Partners.AsNoTracking()...FirstOrDefaultAsync(p => p.Id == id)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-237")]
    public async Task UNOPSPartnerManager_GetPartnerWithContactsAsync_ShouldNotReturnSoftDeletedPartner()
    {
        var result = await _fixture.Context.Partners
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == _fixture.DeletedPartnerId);

        result.Should().BeNull(
            "UNOPSPartnerManager.GetPartnerWithContactsAndInteractionsAsync (line 727) returns soft-deleted partner — " +
            "loads contacts and interactions for a partner that should be invisible");
    }

    /// <summary>
    /// Replicates UNOPSPartnerManager.GetPartnerWithContactsAndInteractionsForAIAsync (line 796):
    ///   _context.Partners.AsNoTracking()...FirstOrDefaultAsync(p => p.Id == id)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-238")]
    public async Task UNOPSPartnerManager_GetPartnerForAI_ShouldNotReturnSoftDeletedPartner()
    {
        var result = await _fixture.Context.Partners
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == _fixture.DeletedPartnerId);

        result.Should().BeNull(
            "UNOPSPartnerManager.GetPartnerWithContactsAndInteractionsForAIAsync (line 796) feeds " +
            "soft-deleted partner data to AI — generates summaries from deleted records");
    }

    /// <summary>
    /// Replicates UNOPSPartnerManager batch query (line 313):
    ///   _context.Partners.Where(p => partnerIds.Contains(p.Id))
    /// Batch queries for multiple partners include soft-deleted ones.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-250")]
    public async Task UNOPSPartnerManager_BatchPartnerQuery_ShouldExcludeSoftDeletedPartners()
    {
        var partnerIds = new List<int> { _fixture.ActivePartnerId, _fixture.DeletedPartnerId };

        var results = await _fixture.Context.Partners
            .Where(p => partnerIds.Contains(p.Id))
            .ToListAsync();

        results.Should().HaveCount(1, "batch partner query should exclude soft-deleted partners");
        results.Should().OnlyContain(p => !p.IsDeleted);
    }

    // =========================================================================
    // CONTACT — Missing IsDeleted filters
    // =========================================================================

    /// <summary>
    /// Replicates UNOPSContactManager.GetBasicEntityAsync (line 763):
    ///   _context.Contacts.FirstOrDefaultAsync(e => e.Id == entityId)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-239")]
    public async Task UNOPSContactManager_GetBasicEntityAsync_ShouldNotReturnSoftDeletedContact()
    {
        var result = await _fixture.Context.Contacts
            .FirstOrDefaultAsync(e => e.Id == _fixture.DeletedContactId);

        result.Should().BeNull(
            "UNOPSContactManager.GetBasicEntityAsync (line 763) queries Contact by ID without IsDeleted filter — " +
            "returns soft-deleted contact data to AI features");
    }

    /// <summary>
    /// Replicates UNOPSContactManager.GetBasicEntityDataAsync (line 776):
    ///   _context.Contacts.FirstOrDefaultAsync(e => e.Id == id)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-240")]
    public async Task UNOPSContactManager_GetBasicEntityDataAsync_ShouldNotReturnSoftDeletedContact()
    {
        var result = await _fixture.Context.Contacts
            .FirstOrDefaultAsync(e => e.Id == _fixture.DeletedContactId);

        result.Should().BeNull(
            "UNOPSContactManager.GetBasicEntityDataAsync (line 776) returns soft-deleted contact data");
    }

    /// <summary>
    /// Replicates UNOPSContactManager.GetContactsForGmailAddon (line 778):
    ///   _context.Contacts.AsNoTracking().Where(c => c.Email != null && lowercaseEmailAddresses.Contains(c.Email.ToLower()))
    /// Searches contacts by email without excluding soft-deleted contacts.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-241")]
    public async Task UNOPSContactManager_GetContactsForGmailAddon_ShouldNotReturnSoftDeletedContacts()
    {
        var emailAddresses = new List<string> { "deleted@example.com" };

        var results = await _fixture.Context.Contacts
            .AsNoTracking()
            .Where(c => c.Email != null && emailAddresses.Contains(c.Email.ToLower()))
            .ToListAsync();

        results.Should().BeEmpty(
            "UNOPSContactManager.GetContactsForGmailAddon (line 778) returns soft-deleted contacts in Gmail addon — " +
            "users see deleted contacts when composing emails");
    }

    /// <summary>
    /// Replicates UNOPSContactManager.GetUnmatchedEmailsWithPartnerSuggestionsAsync (line 923):
    ///   _context.Contacts.AsNoTracking().Where(c => !string.IsNullOrEmpty(c.Email) && c.Email.Contains("@domain"))
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-242")]
    public async Task UNOPSContactManager_GetUnmatchedEmails_ShouldNotReturnSoftDeletedContacts()
    {
        var results = await _fixture.Context.Contacts
            .AsNoTracking()
            .Where(c => !string.IsNullOrEmpty(c.Email) && c.Email.Contains("@example.com"))
            .ToListAsync();

        results.Should().OnlyContain(c => !c.IsDeleted,
            "UNOPSContactManager.GetUnmatchedEmailsWithPartnerSuggestionsAsync (line 923) includes " +
            "soft-deleted contacts in domain-based email suggestions");
    }

    /// <summary>
    /// Verifies that batch contact queries with Contains() exclude soft-deleted contacts.
    /// Replicates UNOPSPartnerManager batch contact query (line 320):
    ///   _context.Contacts.Where(c => partnerIds.Contains(c.PartnerId))
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-250")]
    public async Task UNOPSPartnerManager_BatchContactQuery_ShouldExcludeSoftDeletedContacts()
    {
        var partnerIds = new List<int> { _fixture.ActivePartnerId };

        var results = await _fixture.Context.Contacts
            .Where(c => partnerIds.Contains(c.PartnerId))
            .ToListAsync();

        results.Should().OnlyContain(c => !c.IsDeleted,
            "UNOPSPartnerManager batch contact query (line 320) includes soft-deleted contacts — " +
            "deleted contacts appear in partner summary views");
    }

    // =========================================================================
    // DOCUMENT — Missing IsDeleted filters
    // =========================================================================

    /// <summary>
    /// Replicates UNOPSDocumentManager.GetBasicEntityAsync (line 56):
    ///   _context.Set&lt;UNOPSDocument&gt;().AsNoTracking().FirstOrDefaultAsync(d => d.Id == entityId)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-243")]
    public async Task UNOPSDocumentManager_GetBasicEntityAsync_ShouldNotReturnSoftDeletedDocument()
    {
        var result = await _fixture.Context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == _fixture.DeletedDocumentId);

        result.Should().BeNull(
            "UNOPSDocumentManager.GetBasicEntityAsync (line 56) returns soft-deleted document data — " +
            "deleted documents visible in AI context and entity references");
    }

    /// <summary>
    /// Replicates UNOPSDocumentManager.GetDocumentByIdAsync (line 381):
    ///   _context.Set&lt;UNOPSDocument&gt;().AsNoTracking()...FirstOrDefaultAsync(d => d.Id == documentId)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-244")]
    public async Task UNOPSDocumentManager_GetDocumentByIdAsync_ShouldNotReturnSoftDeletedDocument()
    {
        var result = await _fixture.Context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == _fixture.DeletedDocumentId);

        result.Should().BeNull(
            "UNOPSDocumentManager.GetDocumentByIdAsync (line 381) returns soft-deleted document — " +
            "deleted documents can still be opened and viewed");
    }

    /// <summary>
    /// Replicates UNOPSDocumentManager.GetDocumentParentEntityByIdAsync (line 358):
    ///   _context.Set&lt;UNOPSDocument&gt;().AsNoTracking()...FirstOrDefaultAsync(d => d.Id == documentId)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-245")]
    public async Task UNOPSDocumentManager_GetDocumentParentEntityByIdAsync_ShouldNotReturnSoftDeletedDocument()
    {
        var result = await _fixture.Context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == _fixture.DeletedDocumentId);

        result.Should().BeNull(
            "UNOPSDocumentManager.GetDocumentParentEntityByIdAsync (line 358) returns soft-deleted document — " +
            "navigating to parent entity of a deleted document should fail gracefully");
    }

    /// <summary>
    /// Replicates UNOPSDocumentManager.GetDocumentDetailsForAiAsync (line 453):
    ///   _context.Set&lt;UNOPSDocument&gt;().AsNoTracking()...FirstOrDefaultAsync(d => d.Id == id)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-246")]
    public async Task UNOPSDocumentManager_GetDocumentDetailsForAiAsync_ShouldNotReturnSoftDeletedDocument()
    {
        var result = await _fixture.Context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == _fixture.DeletedDocumentId);

        result.Should().BeNull(
            "UNOPSDocumentManager.GetDocumentDetailsForAiAsync (line 453) feeds soft-deleted document " +
            "content to AI — AI generates summaries from deleted documents");
    }

    // =========================================================================
    // INTERACTION — Missing IsDeleted filters
    // =========================================================================

    /// <summary>
    /// Replicates UNOPSInteractionManager.GetBasicEntityDataAsync (line 1076):
    ///   _context.Interactions.FirstOrDefaultAsync(e => e.Id == id)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-247")]
    public async Task UNOPSInteractionManager_GetBasicEntityDataAsync_ShouldNotReturnSoftDeletedInteraction()
    {
        var result = await _fixture.Context.Interactions
            .FirstOrDefaultAsync(e => e.Id == _fixture.DeletedInteractionId);

        result.Should().BeNull(
            "UNOPSInteractionManager.GetBasicEntityDataAsync (line 1076) returns soft-deleted interaction — " +
            "deleted interaction data leaks into AI context");
    }

    /// <summary>
    /// Replicates InteractionManager linking to soft-deleted contacts (line 66):
    ///   context.Contacts.FindAsync(contactId)
    /// FindAsync does not check IsDeleted, allowing soft-deleted contacts to be linked to interactions.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-249")]
    public async Task InteractionManager_FindAsyncContact_ShouldNotFindSoftDeletedContact()
    {
        var result = await _fixture.Context.Contacts
            .FindAsync(_fixture.DeletedContactId);

        result.Should().BeNull(
            "InteractionManager (line 66) uses FindAsync to load contacts without IsDeleted filter — " +
            "soft-deleted contacts can be linked to new interactions");
    }

    /// <summary>
    /// Replicates InteractionManager linking to soft-deleted partners (line 94):
    ///   context.Partners.FindAsync(partnerId)
    /// FindAsync does not check IsDeleted.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-249")]
    public async Task InteractionManager_FindAsyncPartner_ShouldNotFindSoftDeletedPartner()
    {
        var result = await _fixture.Context.Partners
            .FindAsync(_fixture.DeletedPartnerId);

        result.Should().BeNull(
            "InteractionManager (line 94) uses FindAsync to load partners without IsDeleted filter — " +
            "soft-deleted partners can be linked to new interactions");
    }

    // =========================================================================
    // OPPORTUNITY — Missing IsDeleted filters
    // =========================================================================

    /// <summary>
    /// Replicates OpportunityManager queries at lines 916, 958, 1190:
    ///   context.Opportunities.Include(...).FirstOrDefaultAsync(o => o.Id == id)
    /// These queries load opportunities without checking IsDeleted.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-248")]
    public async Task OpportunityManager_GetOpportunity_ShouldNotReturnSoftDeletedOpportunity()
    {
        var result = await _fixture.Context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == _fixture.DeletedOpportunityId);

        result.Should().BeNull(
            "OpportunityManager (lines 916, 958, 1190) queries opportunities by ID without IsDeleted filter — " +
            "UpdateWhereSectionAsync and GetRelatedItemsAsync can operate on deleted opportunities");
    }

    /// <summary>
    /// Replicates UNOPSOpportunityManager (line 1293):
    ///   context.Opportunities.Include(...).FirstOrDefaultAsync(o => o.Id == id)
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-248")]
    public async Task UNOPSOpportunityManager_GetOpportunityById_ShouldNotReturnSoftDeletedOpportunity()
    {
        var result = await _fixture.Context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == _fixture.DeletedOpportunityId);

        result.Should().BeNull(
            "UNOPSOpportunityManager (line 1293) queries opportunity by ID without IsDeleted filter");
    }

    // =========================================================================
    // POSITIVE TESTS — Verify active records ARE returned (sanity checks)
    // =========================================================================

    [Fact]
    [Trait("Category", "Positive")]
    public async Task ActivePartner_ShouldBeReturned_ByIdQuery()
    {
        var result = await _fixture.Context.Partners
            .FirstOrDefaultAsync(p => p.Id == _fixture.ActivePartnerId);

        result.Should().NotBeNull();
        result!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task ActiveContact_ShouldBeReturned_ByIdQuery()
    {
        var result = await _fixture.Context.Contacts
            .FirstOrDefaultAsync(c => c.Id == _fixture.ActiveContactId);

        result.Should().NotBeNull();
        result!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task ActiveOpportunity_ShouldBeReturned_ByIdQuery()
    {
        var result = await _fixture.Context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == _fixture.ActiveOpportunityId);

        result.Should().NotBeNull();
        result!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task ActiveDocument_ShouldBeReturned_ByIdQuery()
    {
        var result = await _fixture.Context.Documents
            .FirstOrDefaultAsync(d => d.Id == _fixture.ActiveDocumentId);

        result.Should().NotBeNull();
        result!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task ActiveInteraction_ShouldBeReturned_ByIdQuery()
    {
        var result = await _fixture.Context.Interactions
            .FirstOrDefaultAsync(i => i.Id == _fixture.ActiveInteractionId);

        result.Should().NotBeNull();
        result!.IsDeleted.Should().BeFalse();
    }

    // =========================================================================
    // FUNCTIONAL TESTS — Verify correct filtering pattern works
    // =========================================================================

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CorrectPattern_WithIsDeletedFilter_ExcludesSoftDeletedPartner()
    {
        var result = await _fixture.Context.Partners
            .Where(p => p.Id == _fixture.DeletedPartnerId && !p.IsDeleted)
            .FirstOrDefaultAsync();

        result.Should().BeNull("correctly filtered query excludes soft-deleted records");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CorrectPattern_WithIsDeletedFilter_ReturnsActivePartner()
    {
        var result = await _fixture.Context.Partners
            .Where(p => p.Id == _fixture.ActivePartnerId && !p.IsDeleted)
            .FirstOrDefaultAsync();

        result.Should().NotBeNull("correctly filtered query returns active records");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CorrectPattern_WithIsDeletedFilter_ExcludesSoftDeletedContact()
    {
        var result = await _fixture.Context.Contacts
            .Where(c => c.Id == _fixture.DeletedContactId && !c.IsDeleted)
            .FirstOrDefaultAsync();

        result.Should().BeNull("correctly filtered query excludes soft-deleted records");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CorrectPattern_BatchQuery_ExcludesSoftDeletedPartners()
    {
        var partnerIds = new List<int> { _fixture.ActivePartnerId, _fixture.DeletedPartnerId };

        var results = await _fixture.Context.Partners
            .Where(p => partnerIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync();

        results.Should().HaveCount(1);
        results.First().Id.Should().Be(_fixture.ActivePartnerId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CorrectPattern_BatchQuery_ExcludesSoftDeletedContacts()
    {
        var partnerIds = new List<int> { _fixture.ActivePartnerId };

        var results = await _fixture.Context.Contacts
            .Where(c => partnerIds.Contains(c.PartnerId) && !c.IsDeleted)
            .ToListAsync();

        results.Should().OnlyContain(c => !c.IsDeleted);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CorrectPattern_FindAsync_ReturnsSoftDeleted_ProvingFindAsyncIsUnsafe()
    {
        var result = await _fixture.Context.Contacts.FindAsync(_fixture.DeletedContactId);

        result.Should().NotBeNull(
            "FindAsync bypasses all query filters including IsDeleted — " +
            "this proves why production code must NOT use FindAsync for soft-deletable entities");
        result!.IsDeleted.Should().BeTrue();
    }

    // =========================================================================
    // EDGE/BOUNDARY TESTS
    // =========================================================================

    [Fact]
    [Trait("Category", "Edge")]
    public async Task SoftDeletedPartner_StillExistsInDatabase_ButShouldBeFiltered()
    {
        var exists = await _fixture.Context.Partners
            .AnyAsync(p => p.Id == _fixture.DeletedPartnerId);

        exists.Should().BeTrue("soft-deleted record physically exists in DB");

        var filtered = await _fixture.Context.Partners
            .AnyAsync(p => p.Id == _fixture.DeletedPartnerId && !p.IsDeleted);

        filtered.Should().BeFalse("soft-deleted record should be excluded by IsDeleted filter");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task SoftDeletedContact_StillExistsInDatabase_ButShouldBeFiltered()
    {
        var exists = await _fixture.Context.Contacts
            .AnyAsync(c => c.Id == _fixture.DeletedContactId);

        exists.Should().BeTrue("soft-deleted record physically exists in DB");

        var filtered = await _fixture.Context.Contacts
            .AnyAsync(c => c.Id == _fixture.DeletedContactId && !c.IsDeleted);

        filtered.Should().BeFalse("soft-deleted record should be excluded by IsDeleted filter");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task SoftDeletedDocument_StillExistsInDatabase_ButShouldBeFiltered()
    {
        var exists = await _fixture.Context.Documents
            .AnyAsync(d => d.Id == _fixture.DeletedDocumentId);

        exists.Should().BeTrue("soft-deleted record physically exists in DB");

        var filtered = await _fixture.Context.Documents
            .AnyAsync(d => d.Id == _fixture.DeletedDocumentId && !d.IsDeleted);

        filtered.Should().BeFalse("soft-deleted record should be excluded by IsDeleted filter");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task CountQueries_WithoutIsDeleted_InflateRecordCounts()
    {
        var unfilteredCount = await _fixture.Context.Partners.CountAsync();
        var filteredCount = await _fixture.Context.Partners.CountAsync(p => !p.IsDeleted);

        filteredCount.Should().BeLessThan(unfilteredCount,
            "unfiltered count includes soft-deleted records, inflating counts in dashboards and reports");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task ContactCountByPartner_WithoutIsDeleted_InflatesCount()
    {
        var unfilteredCount = await _fixture.Context.Contacts
            .CountAsync(c => c.PartnerId == _fixture.ActivePartnerId);
        var filteredCount = await _fixture.Context.Contacts
            .CountAsync(c => c.PartnerId == _fixture.ActivePartnerId && !c.IsDeleted);

        filteredCount.Should().BeLessThan(unfilteredCount,
            "partner contact count without IsDeleted filter includes deleted contacts");
    }

    // =========================================================================
    // INTEGRATION TESTS — Cross-entity soft delete scenarios
    // =========================================================================

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SoftDeletedPartner_WithActiveContacts_PartnerShouldNotAppear()
    {
        var partner = await _fixture.Context.Partners
            .Where(p => p.Id == _fixture.DeletedPartnerId && !p.IsDeleted)
            .FirstOrDefaultAsync();

        partner.Should().BeNull("soft-deleted partner should not be returned even if it has active child contacts");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task AllEntityTypes_ShouldHaveConsistentSoftDeleteFiltering()
    {
        var partners = await _fixture.Context.Partners
            .Where(p => !p.IsDeleted).ToListAsync();
        var contacts = await _fixture.Context.Contacts
            .Where(c => !c.IsDeleted).ToListAsync();
        var opportunities = await _fixture.Context.Opportunities
            .Where(o => !o.IsDeleted).ToListAsync();
        var interactions = await _fixture.Context.Interactions
            .Where(i => !i.IsDeleted).ToListAsync();
        var documents = await _fixture.Context.Documents
            .Where(d => !d.IsDeleted).ToListAsync();

        partners.Should().OnlyContain(p => !p.IsDeleted);
        contacts.Should().OnlyContain(c => !c.IsDeleted);
        opportunities.Should().OnlyContain(o => !o.IsDeleted);
        interactions.Should().OnlyContain(i => !i.IsDeleted);
        documents.Should().OnlyContain(d => !d.IsDeleted);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EmailSearch_AcrossDeletedAndActiveContacts_ShouldOnlyReturnActive()
    {
        var allEmails = new List<string> { "active@example.com", "deleted@example.com" };

        var results = await _fixture.Context.Contacts
            .AsNoTracking()
            .Where(c => c.Email != null && allEmails.Contains(c.Email.ToLower()))
            .ToListAsync();

        results.Should().OnlyContain(c => !c.IsDeleted,
            "email search across contacts should never return soft-deleted contacts");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-241")]
    public async Task GmailAddon_EmailSearch_ReturnsSoftDeletedContacts_ExposingBug()
    {
        var emailAddresses = new List<string> { "active@example.com", "deleted@example.com" };

        var results = await _fixture.Context.Contacts
            .AsNoTracking()
            .Where(c => c.Email != null && emailAddresses.Contains(c.Email.ToLower()))
            .ToListAsync();

        results.Should().HaveCount(1,
            "Gmail addon email search should return only 1 active contact, not 2 (including deleted)");
        results.First().Email.Should().Be("active@example.com");
    }
}
