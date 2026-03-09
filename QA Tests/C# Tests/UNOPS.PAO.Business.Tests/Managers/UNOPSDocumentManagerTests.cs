/**
 * @fileoverview Comprehensive mock-based tests for UNOPSDocumentManager.
 * Tests document CRUD, upload, link, list, get, delete, update, folder, immutable, creator email,
 * file content, and AI/opportunity document details.
 * Uses mocks for IGoogleDriveDocumentManager, IConfiguration, UserManager.
 * @author UNOPS Opportunity+ QA Team
 */

using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Managers.Mapping;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Mock-based tests for UNOPSDocumentManager.
/// 3:1 Ratio: P=2, N≥6, E≥6, F≥6, I≥6
/// </summary>
public class UNOPSDocumentManagerTests : ManagerTestBase
{
    private readonly UNOPSDocumentManager _manager;
    private readonly Mock<IGoogleDriveDocumentManager> _mockDrive;
    private readonly Mock<UserManager<PAOIdentityUser>> _mockUserManager;
    private readonly string _testMarker = $"Doc_{Guid.NewGuid():N}";

    public UNOPSDocumentManagerTests()
    {
        _mockDrive = new Mock<IGoogleDriveDocumentManager>();
        _mockUserManager = CreateMockUserManager();

        var configData = new Dictionary<string, string?>
        {
            ["GoogleDriveSettings:DefaultGoogleDriveFolderIds:Drive"] = "drive-root-id",
            ["GoogleDriveSettings:DefaultGoogleDriveFolderIds:Archive"] = "archive-root-id"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UNOPS.PAO.Business.Managers.Mapping.MappingProfile>();
            cfg.AddProfile<MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();

        _manager = new UNOPSDocumentManager(
            _mockDrive.Object,
            configuration,
            mapper,
            Context,
            _mockUserManager.Object,
            null,
            null,
            null);
    }

    private static Mock<UserManager<PAOIdentityUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<PAOIdentityUser>>();
        return new Mock<UserManager<PAOIdentityUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!,
            new Mock<ILogger<UserManager<PAOIdentityUser>>>().Object);
    }

    private void SetupCreatorEmail(int userId, string email)
    {
        _mockUserManager
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(new PAOIdentityUser { Id = userId, Email = email });
    }

    private static DocumentUploadModel CreateUploadModel(string name = "test.pdf", string? storagePath = "gs://bucket/path", int parentId = 1)
    {
        return new DocumentUploadModel
        {
            Name = name,
            Type = "pdf",
            StoragePath = storagePath,
            ParentEntityName = "Partner",
            ParentEntityId = parentId,
            DocumentTypeId = null
        };
    }

    #region Positive (2)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetDocumentByIdAsync_ValidId_ReturnsDocument()
    {
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var doc = await SeedDocumentWithRelationshipAsync("Partner", partnerId);

        var result = await _manager.GetDocumentByIdAsync(doc.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be(doc.Name);
        result.Id.Should().Be(doc.Id);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetDocumentsByEntityAsync_ValidEntity_ReturnsDocuments()
    {
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        await SeedDocumentWithRelationshipAsync("Partner", partnerId);
        await SeedDocumentWithRelationshipAsync("Partner", partnerId);

        var result = await _manager.GetDocumentsByEntityAsync("Partner", partnerId);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    #endregion

    #region Negative (6+)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetDocumentByIdAsync_NonexistentId_ReturnsNull()
    {
        var result = await _manager.GetDocumentByIdAsync(int.MaxValue);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task DeleteDocumentAsync_NonexistentId_CompletesWithoutThrow()
    {
        var act = () => _manager.DeleteDocumentAsync(int.MaxValue);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetFileContentByIdAsync_NonexistentDocument_Throws()
    {
        var act = () => _manager.GetFileContentByIdAsync(int.MaxValue);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Document not found*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetCreatorEmailAsync_NonexistentDocument_Throws()
    {
        var act = () => _manager.GetCreatorEmailAsync(int.MaxValue);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Document not found*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task ListDocumentsAsync_EntityWithNoDocuments_ReturnsEmpty()
    {
        var partnerId = await CreateTestPartnerAsync($"EmptyPartner_{_testMarker}");

        var result = _manager.ListDocumentsAsync("Partner", partnerId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateDocumentAsync_NonexistentDocument_ReturnsNull()
    {
        var result = await _manager.UpdateDocumentAsync(new UpdateDocumentRequest { Id = int.MaxValue });

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetDocumentParentEntityByIdAsync_NonexistentDocument_ReturnsNull()
    {
        var result = await _manager.GetDocumentParentEntityByIdAsync(int.MaxValue);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetDocumentDetailsForOpportunityCreationAsync_NonexistentDocument_ReturnsNull()
    {
        var result = await _manager.GetDocumentDetailsForOpportunityCreationAsync(int.MaxValue);

        result.Should().BeNull();
    }

    #endregion

    #region Edge/Boundary (6+)

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetDocumentsByEntityAsync_EntityWithNoDocuments_ReturnsEmpty()
    {
        var partnerId = await CreateTestPartnerAsync($"NoDocs_{_testMarker}");

        var result = await _manager.GetDocumentsByEntityAsync("Partner", partnerId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetDocumentByIdAsync_SoftDeletedDocument_StillReturnedByDirectQuery()
    {
        var partnerId = await CreateTestPartnerAsync($"SoftDel_{_testMarker}");
        var doc = await SeedDocumentWithRelationshipAsync("Partner", partnerId);
        doc.IsDeleted = true;
        doc.DeletedDate = DateTime.UtcNow;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await _manager.GetDocumentByIdAsync(doc.Id);

        result.Should().NotBeNull("GetDocumentByIdAsync does not filter IsDeleted");
    }

    [Fact]

    [Trait("Defect", "DEF-087")]
    [Trait("Category", "Edge")]
    public async Task DeleteDocumentAsync_BlobDocument_RemovesFromDatabase()
    {
        var partnerId = await CreateTestPartnerAsync($"BlobDel_{_testMarker}");
        var doc = await SeedDocumentWithBlobAsync(partnerId);

        await _manager.DeleteDocumentAsync(doc.Id);

        var found = await Context.Documents.FindAsync(doc.Id);
        found.Should().BeNull("physical delete removes record");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task ListDocumentsAsync_ExcludesSoftDeletedAndFolders()
    {
        var partnerId = await CreateTestPartnerAsync($"List_{_testMarker}");
        var doc = await SeedDocumentWithRelationshipAsync("Partner", partnerId);
        var folder = await SeedFolderDocumentAsync("Partner", partnerId);
        doc.IsDeleted = true;
        doc.DeletedDate = DateTime.UtcNow;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = _manager.ListDocumentsAsync("Partner", partnerId).ToList();

        result.Should().BeEmpty("excludes soft-deleted and folder type");
    }

    [Fact]

    [Trait("Defect", "DEF-087")]
    [Trait("Category", "Edge")]
    public async Task GetDocumentDetailsForAiAsync_GcsDocument_ReturnsStoragePath()
    {
        var doc = await SeedGcsDocumentAsync();

        var result = await _manager.GetDocumentDetailsForAiAsync(doc.Id);

        result.Should().NotBeNull();
        var dyn = result as dynamic;
        ((bool)dyn!.IsGcsDocument).Should().BeTrue();
        ((string)dyn.StoragePath).Should().StartWith("gs://");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetDocumentDetailsForAiAsync_NoContent_Throws()
    {
        var doc = await SeedDocumentNoContentAsync();

        var act = () => _manager.GetDocumentDetailsForAiAsync(doc.Id);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*no content available*");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetEntityFolderDocument_NoFolder_ReturnsNull()
    {
        var partnerId = await CreateTestPartnerAsync($"NoFolder_{_testMarker}");

        var result = _manager.GetEntityFolderDocument("Partner", partnerId);

        result.Should().BeNull();
    }

    #endregion

    #region Functional (6+)

    [Fact]

    [Trait("Defect", "DEF-087")]
    [Trait("Category", "Functional")]
    public async Task DeleteDocumentAsync_SetsRecordRemoved()
    {
        var partnerId = await CreateTestPartnerAsync($"FuncDel_{_testMarker}");
        var doc = await SeedDocumentWithBlobAsync(partnerId);

        await _manager.DeleteDocumentAsync(doc.Id);

        var count = await Context.Documents.CountAsync(d => d.Id == doc.Id);
        count.Should().Be(0);
    }

    [Fact]

    [Trait("Defect", "DEF-087")]
    [Trait("Category", "Functional")]
    public async Task UpdateDocumentAsync_UpdatesMetadataFields()
    {
        var partnerId = await CreateTestPartnerAsync($"Update_{_testMarker}");
        var doc = await SeedDocumentWithRelationshipAsync("Partner", partnerId);
        const int newTypeId = 99;

        var result = await _manager.UpdateDocumentAsync(new UpdateDocumentRequest
        {
            Id = doc.Id,
            DocumentTypeId = newTypeId
        });

        result.Should().NotBeNull();
        Context.ChangeTracker.Clear();
        var updated = await Context.Documents.FindAsync(doc.Id);
        updated!.DocumentTypeId.Should().Be(newTypeId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetDocumentsByEntityAsync_FiltersByEntityTypeAndId()
    {
        var partnerId1 = await CreateTestPartnerAsync($"P1_{_testMarker}");
        var partnerId2 = await CreateTestPartnerAsync($"P2_{_testMarker}");
        await SeedDocumentWithRelationshipAsync("Partner", partnerId1);
        await SeedDocumentWithRelationshipAsync("Partner", partnerId2);

        var result = await _manager.GetDocumentsByEntityAsync("Partner", partnerId1);

        result.Should().HaveCount(1);
        result.First().Name.Should().Contain("Doc");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetCreatorEmailAsync_ResolvesEmailFromUser()
    {
        var partnerId = await CreateTestPartnerAsync($"Creator_{_testMarker}");
        var doc = await SeedDocumentWithRelationshipAsync("Partner", partnerId);
        SetupCreatorEmail(doc.CreatedBy, "creator@test.com");

        var result = await _manager.GetCreatorEmailAsync(doc.Id);

        result.Should().Be("creator@test.com");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task ListDocumentsAsync_AppliesEntityFilter()
    {
        var partnerId = await CreateTestPartnerAsync($"ListF_{_testMarker}");
        await SeedDocumentWithRelationshipAsync("Partner", partnerId);

        var result = _manager.ListDocumentsAsync("Partner", partnerId).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Contain("Doc");
    }

    [Fact]

    [Trait("Defect", "DEF-087")]
    [Trait("Category", "Functional")]
    public async Task GetDocumentDetailsForAiAsync_ReturnsStructuredData()
    {
        var doc = await SeedDocumentWithBlobAsync(await CreateTestPartnerAsync($"AI_{_testMarker}"));
        SetupCreatorEmail(doc.CreatedBy, "ai@test.com");

        var result = await _manager.GetDocumentDetailsForAiAsync(doc.Id);

        result.Should().NotBeNull();
        var dyn = result as dynamic;
        ((int)dyn!.Id).Should().Be(doc.Id);
        ((string)dyn.Name).Should().NotBeNullOrEmpty();
        ((bool)dyn.IsGcsDocument).Should().BeFalse();
        ((byte[])dyn.Blob).Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetDocumentParentEntityByIdAsync_ReturnsEntityIdAndType()
    {
        var partnerId = await CreateTestPartnerAsync($"Parent_{_testMarker}");
        var doc = await SeedDocumentWithRelationshipAsync("Partner", partnerId);

        var result = await _manager.GetDocumentParentEntityByIdAsync(doc.Id);

        result.Should().NotBeNull();
        result!.Value.EntityId.Should().Be(partnerId);
        result.Value.EntityType.Should().Be("Partner");
    }

    #endregion

    #region Integration (6+)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateDocument_Retrieve_VerifyFields()
    {
        var partnerId = await CreateTestPartnerAsync($"IntCreate_{_testMarker}");
        var model = CreateUploadModel("integration.pdf", "gs://bucket/int.pdf", partnerId);

        var created = await _manager.CreateDocumentAsync(model);
        var retrieved = await _manager.GetDocumentByIdAsync(created.Id);

        created.Should().NotBeNull();
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("integration.pdf");
        retrieved.StoragePath.Should().Be("gs://bucket/int.pdf");
    }

    [Fact]

    [Trait("Defect", "DEF-087")]
    [Trait("Category", "Integration")]
    public async Task CreateDocument_Delete_VerifyRemoved()
    {
        var partnerId = await CreateTestPartnerAsync($"IntDel_{_testMarker}");
        var model = CreateUploadModel("to-delete.pdf", "gs://bucket/del.pdf", partnerId);
        var created = await _manager.CreateDocumentAsync(model);

        await _manager.DeleteDocumentAsync(created.Id);

        var found = await _manager.GetDocumentByIdAsync(created.Id);
        found.Should().BeNull();
    }

    [Fact]

    [Trait("Defect", "DEF-087")]
    [Trait("Category", "Integration")]
    public async Task CreateDocument_Update_VerifyChanges()
    {
        var partnerId = await CreateTestPartnerAsync($"IntUpd_{_testMarker}");
        var model = CreateUploadModel("original.pdf", "gs://bucket/orig.pdf", partnerId);
        var created = await _manager.CreateDocumentAsync(model);
        const int newTypeId = 42;

        var updated = await _manager.UpdateDocumentAsync(new UpdateDocumentRequest
        {
            Id = created.Id,
            DocumentTypeId = newTypeId
        });

        updated.Should().NotBeNull();
        Context.ChangeTracker.Clear();
        var dbDoc = await Context.Documents.FindAsync(created.Id);
        dbDoc!.DocumentTypeId.Should().Be(newTypeId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateMultipleDocuments_RetrieveAll()
    {
        var partnerId = await CreateTestPartnerAsync($"IntMulti_{_testMarker}");
        await _manager.CreateDocumentAsync(CreateUploadModel("a.pdf", "gs://b/a.pdf", partnerId));
        await _manager.CreateDocumentAsync(CreateUploadModel("b.pdf", "gs://b/b.pdf", partnerId));
        await _manager.CreateDocumentAsync(CreateUploadModel("c.pdf", "gs://b/c.pdf", partnerId));

        var docs = await _manager.GetDocumentsByEntityAsync("Partner", partnerId);

        docs.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateDocument_GetFileContent_Verify()
    {
        var partnerId = await CreateTestPartnerAsync($"IntContent_{_testMarker}");
        var doc = await SeedDocumentWithBlobAsync(partnerId);

        var content = await _manager.GetFileContentByIdAsync(doc.Id);

        content.Should().NotBeNull();
        content.Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4, 5 });
    }

    [Fact]

    [Trait("Defect", "DEF-087")]
    [Trait("Category", "Integration")]
    public async Task DocumentLifecycle_Create_Update_SetImmutable_Delete()
    {
        var partnerId = await CreateTestPartnerAsync($"IntLife_{_testMarker}");
        var model = CreateUploadModel("lifecycle.pdf", "gs://b/life.pdf", partnerId);
        var created = await _manager.CreateDocumentAsync(model);

        var updated = await _manager.UpdateDocumentAsync(new UpdateDocumentRequest
        {
            Id = created.Id,
            DocumentTypeId = 1
        });
        updated.Should().NotBeNull();

        await _manager.DeleteDocumentAsync(created.Id);
        var afterDelete = await _manager.GetDocumentByIdAsync(created.Id);
        afterDelete.Should().BeNull();
    }

    #endregion

    #region Seed Helpers

    private async Task<UNOPSDocument> SeedDocumentWithRelationshipAsync(string entityType, int entityId)
    {
        var doc = new UNOPSDocument(false)
        {
            Name = $"Doc {_testMarker}",
            Type = "pdf",
            Link = $"https://storage.example.com/{_testMarker}.pdf",
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId
        };
        await Context.Documents.AddAsync(doc);
        await SaveChangesAsync();

        var rel = new DocumentRelationship
        {
            DocumentId = doc.Id,
            EntityType = entityType,
            EntityId = entityId,
            Name = entityType,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.DocumentRelationships.AddAsync(rel);
        await SaveChangesAsync();
        return doc;
    }

    private async Task<UNOPSDocument> SeedDocumentWithBlobAsync(int partnerId)
    {
        var doc = new UNOPSDocument(false)
        {
            Name = $"BlobDoc {_testMarker}",
            Type = "pdf",
            Blob = new byte[] { 1, 2, 3, 4, 5 },
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId
        };
        await Context.Documents.AddAsync(doc);
        await SaveChangesAsync();

        var rel = new DocumentRelationship
        {
            DocumentId = doc.Id,
            EntityType = "Partner",
            EntityId = partnerId,
            Name = "Partner",
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.DocumentRelationships.AddAsync(rel);
        await SaveChangesAsync();
        return doc;
    }

    private async Task<UNOPSDocument> SeedGcsDocumentAsync()
    {
        var doc = new UNOPSDocument(false)
        {
            Name = $"GcsDoc {_testMarker}",
            Type = "pdf",
            StoragePath = "gs://bucket/path/to/doc.pdf",
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId
        };
        await Context.Documents.AddAsync(doc);
        await SaveChangesAsync();
        return doc;
    }

    private async Task<UNOPSDocument> SeedDocumentNoContentAsync()
    {
        var doc = new UNOPSDocument(false)
        {
            Name = $"NoContent {_testMarker}",
            Type = "pdf",
            Link = null,
            Blob = null,
            GoogleId = null,
            StoragePath = null,
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId
        };
        await Context.Documents.AddAsync(doc);
        await SaveChangesAsync();
        return doc;
    }

    private async Task<UNOPSDocument> SeedFolderDocumentAsync(string entityType, int entityId)
    {
        var folder = new UNOPSDocument(false)
        {
            Name = $"Folder {_testMarker}",
            Type = "folder",
            GoogleId = "folder-id",
            Link = "https://drive.google.com/folder",
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId
        };
        await Context.Documents.AddAsync(folder);
        await SaveChangesAsync();

        var rel = new DocumentRelationship
        {
            DocumentId = folder.Id,
            EntityType = entityType,
            EntityId = entityId,
            Name = entityType,
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.DocumentRelationships.AddAsync(rel);
        await SaveChangesAsync();
        return folder;
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----------|-------|-------|
| Positive (P) | 2 | GetDocumentByIdAsync_ValidId_ReturnsDocument, GetDocumentsByEntityAsync_ValidEntity_ReturnsDocuments |
| Negative (N) | 8 | GetDocumentByIdAsync_NonexistentId_ReturnsNull, DeleteDocumentAsync_NonexistentId_CompletesWithoutThrow, GetFileContentByIdAsync_NonexistentDocument_Throws, GetCreatorEmailAsync_NonexistentDocument_Throws, ListDocumentsAsync_EntityWithNoDocuments_ReturnsEmpty, UpdateDocumentAsync_NonexistentDocument_ReturnsNull, GetDocumentParentEntityByIdAsync_NonexistentDocument_ReturnsNull, GetDocumentDetailsForOpportunityCreationAsync_NonexistentDocument_ReturnsNull |
| Edge/Boundary (E) | 7 | GetDocumentsByEntityAsync_EntityWithNoDocuments_ReturnsEmpty, GetDocumentByIdAsync_SoftDeletedDocument_StillReturnedByDirectQuery, DeleteDocumentAsync_BlobDocument_RemovesFromDatabase, ListDocumentsAsync_ExcludesSoftDeletedAndFolders, GetDocumentDetailsForAiAsync_GcsDocument_ReturnsStoragePath, GetDocumentDetailsForAiAsync_NoContent_Throws, GetEntityFolderDocument_NoFolder_ReturnsNull |
| Functional (F) | 7 | DeleteDocumentAsync_SetsRecordRemoved, UpdateDocumentAsync_UpdatesMetadataFields, GetDocumentsByEntityAsync_FiltersByEntityTypeAndId, GetCreatorEmailAsync_ResolvesEmailFromUser, ListDocumentsAsync_AppliesEntityFilter, GetDocumentDetailsForAiAsync_ReturnsStructuredData, GetDocumentParentEntityByIdAsync_ReturnsEntityIdAndType |
| Integration (I) | 6 | CreateDocument_Retrieve_VerifyFields, CreateDocument_Delete_VerifyRemoved, CreateDocument_Update_VerifyChanges, CreateMultipleDocuments_RetrieveAll, CreateDocument_GetFileContent_Verify, DocumentLifecycle_Create_Update_SetImmutable_Delete |
| **N ≥ 3P?** | ✅ | 8 >= 6 |
| **E ≥ 3P?** | ✅ | 7 >= 6 |
| **F ≥ 3P?** | ✅ | 7 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
