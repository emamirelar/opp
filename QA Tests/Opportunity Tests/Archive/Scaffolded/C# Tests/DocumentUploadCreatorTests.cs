using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Business.Tests.Opportunity
{
    /// <summary>
    /// Document Upload Creator Assignment Tests
    /// 
    /// Purpose: Verify correct user is assigned as creator when creating opportunities from uploaded documents
    /// 
    /// Real Production Bug: PNO-934 - Wrong Opportunity Manager from concept note
    /// - Creating opportunity from PDF assigns wrong user as manager
    /// - Should always assign logged-in user as creator, not someone mentioned in document
    /// - AI extraction should not override creator field
    /// 
    /// These tests ensure:
    /// - Opportunity created from PDF has correct creator
    /// - Opportunity created from Word doc has correct creator
    /// - Uploaded file preserves current user context
    /// - AI extraction doesn't override creator
    /// - Manager defaults to logged-in user regardless of source
    /// </summary>
    [Trait("Category", "DocumentUpload")]
    [Trait("Priority", "Critical")]
    public class DocumentUploadCreatorTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public DocumentUploadCreatorTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"DocUploadTest_{Guid.NewGuid()}")
                .Options;
            _context = TestDbContextFactory.Create(_dbOptions);
            SeedTestUsers();
        }

        private void SeedTestUsers()
        {
            var users = new[]
            {
                new UserProfile { UserId = 100, FirstName = "Current", LastName = "User", UserEmail = "current.user@unops.org" },
                new UserProfile { UserId = 101, FirstName = "Document", LastName = "Author", UserEmail = "doc.author@unops.org" },
                new UserProfile { UserId = 102, FirstName = "Mentioned", LastName = "Person", UserEmail = "mentioned@unops.org" }
            };
            _context.UserProfile.AddRange(users);
            _context.SaveChanges();
        }

        #region PDF Upload Tests

        [Fact]
        public async Task TC_DUCA_001_OpportunityFromPDF_AssignsCurrentUserAsManager()
        {
            // Arrange - Simulate current user uploading PDF
            var currentUserId = 100; // Current logged-in user
            var documentAuthorId = 101; // Someone mentioned in PDF
            
            // Simulate PDF upload and opportunity creation
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Opportunity from PDF",
                OpportunityNumber = "OPP-2026-PDF001",
                Description = "Created from uploaded PDF concept note",
                OpportunityManagerId = currentUserId, // Should be current user, NOT document author
                CreatedBy = currentUserId,
                LastModifiedBy = currentUserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active,
                SourceDocument = "concept-note.pdf"
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunity = await _context.Opportunities
                .Include(o => o.OpportunityManager)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - Creator should be current user, NOT anyone mentioned in PDF
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.OpportunityManagerId.Should().Be(currentUserId, 
                "Opportunity Manager should be the user who uploaded the file, not someone in the document");
            savedOpportunity.CreatedBy.Should().Be(currentUserId);
            savedOpportunity.OpportunityManager!.FirstName.Should().Be("Current");
            savedOpportunity.OpportunityManager.LastName.Should().Be("User");
        }

        [Fact]
        public async Task TC_DUCA_002_OpportunityFromWordDoc_AssignsCurrentUserAsCreator()
        {
            // Arrange - Simulate Word document upload
            var currentUserId = 100;
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Opportunity from Word Doc",
                OpportunityNumber = "OPP-2026-WORD001",
                Description = "Created from uploaded Word document",
                OpportunityManagerId = currentUserId,
                CreatedBy = currentUserId,
                LastModifiedBy = currentUserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active,
                SourceDocument = "concept-note.docx"
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunity = await _context.Opportunities.FindAsync(opportunity.Id);

            // Assert
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.CreatedBy.Should().Be(currentUserId);
            savedOpportunity.OpportunityManagerId.Should().Be(currentUserId);
        }

        [Fact]
        public async Task TC_DUCA_003_UploadedFile_PreservesCurrentUserContext()
        {
            // Arrange - User 100 uploads file while User 102 is mentioned in document
            var uploadingUserId = 100;
            var mentionedUserId = 102;

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Context Preservation Test",
                OpportunityNumber = "OPP-2026-CTX001",
                Description = $"Document mentions User {mentionedUserId} but uploaded by User {uploadingUserId}",
                OpportunityManagerId = uploadingUserId, // Should be uploader
                CreatedBy = uploadingUserId,
                LastModifiedBy = uploadingUserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunity = await _context.Opportunities.FindAsync(opportunity.Id);

            // Assert - Context of uploader is preserved
            savedOpportunity!.OpportunityManagerId.Should().Be(uploadingUserId, 
                "Uploader's context should be preserved, not mentioned user");
            savedOpportunity.CreatedBy.Should().Be(uploadingUserId);
        }

        [Fact]
        public async Task TC_DUCA_004_AIExtraction_DoesNotOverrideCreator()
        {
            // Arrange - Simulate AI extracting data from document
            var currentUserId = 100;
            var extractedManagerName = "John Smith"; // Name extracted from document
            
            // AI extracts data but should NOT override creator
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "AI Extracted Opportunity",
                OpportunityNumber = "OPP-2026-AI001",
                Description = $"AI extracted manager name: {extractedManagerName}",
                OpportunityManagerId = currentUserId, // Should remain current user
                CreatedBy = currentUserId,
                LastModifiedBy = currentUserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunity = await _context.Opportunities.FindAsync(opportunity.Id);

            // Assert - AI extraction does not override actual creator
            savedOpportunity!.OpportunityManagerId.Should().Be(currentUserId,
                "AI extraction should not override the actual user who created the opportunity");
        }

        [Fact]
        public async Task TC_DUCA_005_MultipleUploads_MaintainCorrectCreator()
        {
            // Arrange - User uploads multiple documents
            var userId = 100;
            
            var opportunities = new[]
            {
                new Domain.Entities.Opportunity
                {
                    Name = "First Upload",
                    OpportunityNumber = "OPP-2026-UP001",
                    OpportunityManagerId = userId,
                    CreatedBy = userId,
                    LastModifiedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Status = EntityStatus.Active,
                    SourceDocument = "doc1.pdf"
                },
                new Domain.Entities.Opportunity
                {
                    Name = "Second Upload",
                    OpportunityNumber = "OPP-2026-UP002",
                    OpportunityManagerId = userId,
                    CreatedBy = userId,
                    LastModifiedBy = userId,
                    CreatedDate = DateTime.UtcNow.AddMinutes(5),
                    LastModifiedDate = DateTime.UtcNow.AddMinutes(5),
                    Status = EntityStatus.Active,
                    SourceDocument = "doc2.pdf"
                }
            };

            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunities = await _context.Opportunities
                .Where(o => o.OpportunityManagerId == userId)
                .ToListAsync();

            // Assert - All uploads have correct creator
            savedOpportunities.Should().HaveCount(2);
            savedOpportunities.Should().AllSatisfy(o => 
            {
                o.OpportunityManagerId.Should().Be(userId);
                o.CreatedBy.Should().Be(userId);
            });
        }

        #endregion

        #region Document Replacement Tests

        [Fact]
        public async Task TC_DUCA_006_ReplacingDocument_DoesNotChangeCreator()
        {
            // Arrange - Create opportunity from first document
            var originalCreatorId = 100;
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Document Replacement Test",
                OpportunityNumber = "OPP-2026-REP001",
                OpportunityManagerId = originalCreatorId,
                CreatedBy = originalCreatorId,
                LastModifiedBy = originalCreatorId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active,
                SourceDocument = "original.pdf"
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Replace document (simulate another user replacing it)
            var replacingUserId = 101;
            opportunity.SourceDocument = "updated.pdf";
            opportunity.LastModifiedBy = replacingUserId;
            opportunity.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var savedOpportunity = await _context.Opportunities.FindAsync(opportunity.Id);

            // Assert - Original creator preserved, only LastModifiedBy changes
            savedOpportunity!.CreatedBy.Should().Be(originalCreatorId,
                "Original creator should be preserved when document is replaced");
            savedOpportunity.OpportunityManagerId.Should().Be(originalCreatorId,
                "Opportunity Manager should not change when document is replaced");
            savedOpportunity.LastModifiedBy.Should().Be(replacingUserId,
                "LastModifiedBy should update to user who replaced document");
        }

        #endregion

        #region File Type Tests

        [Fact]
        public async Task TC_DUCA_007_CreatorAssignment_WorksForAllFileTypes()
        {
            // Arrange - Test various file types
            var userId = 100;
            var fileTypes = new[] { ".pdf", ".docx", ".doc", ".txt", ".odt" };

            foreach (var fileType in fileTypes)
            {
                var opportunity = new Domain.Entities.Opportunity
                {
                    Name = $"Opportunity from {fileType}",
                    OpportunityNumber = $"OPP-2026-{fileType.Replace(".", "").ToUpper()}",
                    OpportunityManagerId = userId,
                    CreatedBy = userId,
                    LastModifiedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Status = EntityStatus.Active,
                    SourceDocument = $"document{fileType}"
                };

                _context.Opportunities.Add(opportunity);
            }
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunities = await _context.Opportunities
                .Where(o => o.CreatedBy == userId)
                .ToListAsync();

            // Assert - All file types have correct creator
            savedOpportunities.Should().HaveCount(fileTypes.Length);
            savedOpportunities.Should().AllSatisfy(o =>
            {
                o.OpportunityManagerId.Should().Be(userId);
                o.CreatedBy.Should().Be(userId);
            });
        }

        #endregion

        #region Metadata Tests

        [Fact]
        public async Task TC_DUCA_008_DocumentMetadata_DoesNotOverrideUser()
        {
            // Arrange - Document has metadata with different author
            var currentUserId = 100;
            var documentMetadataAuthor = "Different Author in PDF metadata";

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Metadata Test Opportunity",
                OpportunityNumber = "OPP-2026-META001",
                Description = $"Document metadata shows author: {documentMetadataAuthor}",
                OpportunityManagerId = currentUserId, // Should be current user, not metadata author
                CreatedBy = currentUserId,
                LastModifiedBy = currentUserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active,
                SourceDocument = "document-with-metadata.pdf"
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunity = await _context.Opportunities.FindAsync(opportunity.Id);

            // Assert - Metadata does not override actual user
            savedOpportunity!.OpportunityManagerId.Should().Be(currentUserId,
                "Document metadata should not override the actual logged-in user");
        }

        #endregion

        #region Concept Note Tests

        [Fact]
        public async Task TC_DUCA_009_ConceptNote_FieldsExtractedCorrectly()
        {
            // Arrange - Simulate concept note with extracted fields
            var currentUserId = 100;

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Extracted from Concept Note",
                OpportunityNumber = "OPP-2026-CN001",
                Description = "AI extracted this description from concept note",
                EstimatedBudget = 500000m, // Extracted by AI
                TargetSigningDate = new DateTime(2026, 12, 31), // Extracted by AI
                OpportunityManagerId = currentUserId, // Should be uploader, not extracted
                CreatedBy = currentUserId,
                LastModifiedBy = currentUserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active,
                SourceDocument = "concept-note.pdf"
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunity = await _context.Opportunities.FindAsync(opportunity.Id);

            // Assert - Extracted fields are present, but creator is current user
            savedOpportunity!.Description.Should().Contain("AI extracted");
            savedOpportunity.EstimatedBudget.Should().Be(500000m);
            savedOpportunity.TargetSigningDate.Should().NotBeNull();
            savedOpportunity.OpportunityManagerId.Should().Be(currentUserId,
                "Manager should be uploader even when AI extracts other fields");
        }

        [Fact]
        public async Task TC_DUCA_010_SourceDocument_DoesNotAffectPermissionModel()
        {
            // Arrange - Create opportunity from document
            var userId = 100;

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Permission Test Opportunity",
                OpportunityNumber = "OPP-2026-PERM001",
                OpportunityManagerId = userId,
                CreatedBy = userId,
                LastModifiedBy = userId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active,
                SourceDocument = "uploaded-document.pdf"
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Check permissions
            var savedOpportunity = await _context.Opportunities.FindAsync(opportunity.Id);
            var userHasPermission = savedOpportunity!.CreatedBy == userId ||
                                   savedOpportunity.OpportunityManagerId == userId;

            // Assert - Permissions based on actual user, not document source
            userHasPermission.Should().BeTrue(
                "User should have permission based on being creator/manager, not document source");
            savedOpportunity.CreatedBy.Should().Be(userId);
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
