using AutoMapper;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.Models.Documents;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSDocumentManager : BaseUNOPSManager, IDocumentManager
{
    private BaseRepository<UNOPSDocument> _documentRepository;
    private readonly IGoogleDriveDocumentManager _driveManager;
    private readonly IConfigurationSection _driveConfig;
    private readonly BaseRepository<UNOPSContact> _contactRepository;
    private readonly BaseRepository<UNOPSPartner> _partnerRepository;
    private readonly BaseRepository<UNOPSPartnerTree> _partnerTreeRepository;

    public UNOPSDocumentManager(
        IGoogleDriveDocumentManager driveManager,
        IConfiguration configuration,
        IMapper mapper,
        UNOPSAppDbContext context,
        UserManager<PAOIdentityUser> userManager,
        IServiceProvider serviceProvider = null,
        IPermissionService permissionService = null,
        IHttpContextAccessor httpContextAccessor = null
        ) : base(mapper, context, configuration, userManager, "Document", permissionService, httpContextAccessor)
    {
        _documentRepository = new BaseRepository<UNOPSDocument>(context, configuration, serviceProvider);
        _driveManager = driveManager;
        _driveConfig = configuration.GetSection($"GoogleDriveSettings:DefaultGoogleDriveFolderIds");
        _contactRepository = new BaseRepository<UNOPSContact>(context, configuration, serviceProvider);
        _partnerRepository = new BaseRepository<UNOPSPartner>(context, configuration, serviceProvider);
        _partnerTreeRepository = new BaseRepository<UNOPSPartnerTree>(context, configuration, serviceProvider);
    }

    /// <summary>
    /// Implementation of required BaseUNOPSManager method
    /// </summary>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal? user = null)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only query
        var document = await _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == entityId);
        if (document == null) return null;
        
        return MapDocumentModel(document);
    }

    private DocumentModel MapDocumentModel(UNOPSDocument entity)
    {
        DocumentModel result = _mapper.Map<DocumentModel>(entity);

        result.Extensions = new Dictionary<string, object?>()
            {
                { "Origin", entity.LinkedFile ? "Link" : "Upload" }
            };

        return result;
    }

    private async Task EnsureFolderDocument(string googleId, string link, string folderName, string entityType, int entityId)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only check
        var folderDocument = await _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .Where(x => x.Type == "folder" && x.GoogleId == googleId && !x.IsDeleted)
            .FirstOrDefaultAsync();

        if (folderDocument != null)
        {
            return;
        }

        folderDocument = new UNOPSDocument()
        {
            Type = "folder",
            GoogleId = googleId,
            Link = link,
            Name = folderName
        };

        await _documentRepository.AddAsync(folderDocument);

        var docRelationship = new DocumentRelationship
        {
            Document = folderDocument,
            EntityId = entityId,
            Name = entityType,
            EntityType = entityType
        };

        await _context.DocumentRelationships.AddAsync(docRelationship);
        await _context.SaveChangesAsync();
    }

    private async Task<Dictionary<string, string>> EnsureFolderStructure(string entityType, int entityId)
    {
        var folder = GetEntityFolderDocument(entityType, entityId);

        if (folder != null)
        {
            return new Dictionary<string, string>
            {   { "id", folder.GoogleId },
                { "webViewLink", folder.Link }
            };
        }

        var folderName = await GetEntityName(entityType, entityId);

        Dictionary<string, string> partnerFolder;
        Dictionary<string, string> contactFolder;
        
        switch (entityType)
        {
            case "Partner":
                var driveId = _driveConfig.GetSection("Drive").Value;
                if (string.IsNullOrEmpty(driveId))
                {
                    throw new Exception("Please provide root location in appsettings.");
                }

                partnerFolder = await _driveManager.CreateFolderAsync(folderName, driveId);
                await EnsureFolderDocument(partnerFolder["id"], partnerFolder["webViewLink"], folderName, entityType, entityId);

                return partnerFolder;

            case "Contact":
                var partnerId = await GetParentEntityId(entityType, entityId);
                partnerFolder = await EnsureFolderStructure("Partner", partnerId);
                contactFolder = await _driveManager.CreateFolderAsync(folderName, partnerFolder["id"]);
                await EnsureFolderDocument(contactFolder["id"], contactFolder["webViewLink"], folderName, entityType, entityId);

                return contactFolder;

            case "PartnerTree":
                var partnerTreeDriveId = _driveConfig.GetSection("Drive").Value;
                if (string.IsNullOrEmpty(partnerTreeDriveId))
                {
                    throw new Exception("Please provide root location in appsettings.");
                }

                var partnerTreeFolder = await _driveManager.CreateFolderAsync(folderName, partnerTreeDriveId);
                await EnsureFolderDocument(partnerTreeFolder["id"], partnerTreeFolder["webViewLink"], folderName, entityType, entityId);

                return partnerTreeFolder;

            default:
                throw new Exception("Invalid entity type.");
        }
    }

    private async Task<int> GetParentEntityId(string entityType, int entityId)
    {
        switch (entityType)
        {
            case "Contact":
                // ✅ OPTIMIZED: Added AsNoTracking for read-only query
                var contact = await _context.Set<UNOPSContact>()
                    .AsNoTracking()
                    .Include(c => c.Partner)
                    .FirstOrDefaultAsync(c => c.Id == entityId);
                if (contact == null)
                {
                    throw new Exception("Contact not found.");
                }
                return contact.Partner.Id;

            //Commenting as Partner does not have a Parent yet
            /*case "Partner":
                var partner = await _partnerRepository.GetByIdAsync(entityId);
                if (partner == null)
                {
                    throw new Exception("Partner not found.");
                }
                return partner.Id;*/

            default:
                throw new Exception("Invalid entity type.");
        }
    }

    private async Task<string> GetEntityName(string entityType, int entityId)
    {
        switch (entityType)
        {
            /*case "Project":
                var project = await _projectManager.GetByIdAsync(entityId);
                if (project == null)
                {
                    throw new Exception("Project not found.");
                }
                return project.Name;*/

            case "Contact":
                // ✅ OPTIMIZED: Added AsNoTracking for read-only query
                var contact = await _context.Set<UNOPSContact>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == entityId);
                if (contact == null)
                {
                    throw new Exception("Contact not found.");
                }
                return contact.Name;

            case "Partner":
                // ✅ OPTIMIZED: Added AsNoTracking for read-only query
                var partner = await _context.Set<UNOPSPartner>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == entityId);
                if (partner == null)
                {
                    throw new Exception("Partner not found.");
                }
                return partner.Name;

            case "PartnerTree":
                // ✅ OPTIMIZED: Added AsNoTracking for read-only query
                var partnerTree = await _context.Set<UNOPSPartnerTree>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(pt => pt.Id == entityId);
                if (partnerTree == null)
                {
                    throw new Exception("PartnerTree not found.");
                }
                return partnerTree.Name;

            default:
                throw new Exception("Invalid entity type.");
        }
    }

    public async Task<DocumentModel> CreateDocumentAsync(DocumentUploadModel model)
    {
        byte[] fileBytes = null;
        
        // If StoragePath is provided (GCS upload), don't store blob
        // Otherwise, store file as blob in database
        if (string.IsNullOrEmpty(model.StoragePath))
        {
            using (var memoryStream = new MemoryStream())
            {
                await model.File.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }
        }

        var fileType = model.Type ?? "";
        if (model.File != null)
        {
            fileType = model.File.GetFileType();
        }
        var fileName = string.IsNullOrWhiteSpace(model.Name) ? model.File.FileName : model.Name;

        var documentEntity = new UNOPSDocument
        {
            Name = fileName,
            Type = fileType,
            Blob = fileBytes, // Will be null if using GCS storage
            Link = model.Link, // Will be populated if sourced from Google Drive
            GoogleId = model.GoogleId, // Will be populated if sourced from Google Drive
            StoragePath = model.StoragePath, // Use the GCS path if provided
            LinkedFile = !string.IsNullOrEmpty(model.Link), // True if it's a Drive-sourced file
            DocumentTypeId = model.DocumentTypeId,
            AITranscribed = model.AITranscribed ?? false
        };

        await _documentRepository.AddAsync(documentEntity);
        await HandleDocumentRelationships(documentEntity, model);

        return MapDocumentModel(documentEntity);
    }

    public async Task<DocumentModel> UploadDocumentAsync(DocumentUploadModel model, string entityFolderId)
    {
        var fileType = model.File.GetFileType();

        var result = await _driveManager.UploadFileAsync(
            model.File,
            model.Name,
            entityFolderId
        );

        return new DocumentModel
        {
            Name = model.Name,
            Type = fileType,
            Link = result["webViewLink"],
            GoogleId = result["id"]
        };
    }

    public async Task<DocumentModel> LinkDocumentAsync(DocumentLinkModel model)
    {
        var document = new DocumentModel
        {
            Name = model.Name,
            Link = model.Link,
            Type = model.Type,
            GoogleId = model.GoogleId
        };

        var entity = _mapper.Map<UNOPSDocument>(model);
        entity.LinkedFile = true;
        entity.DocumentTypeId = model.DocumentTypeId;

        await _documentRepository.AddAsync(entity);
        await HandleDocumentRelationships(entity, model);
        return _mapper.Map<DocumentModel>(entity);
    }

    private async Task HandleDocumentRelationships(UNOPSDocument entity, DocumentBaseCreateModel model)
    {
        if (model.ParentEntityType == DocumentParentEntityType.Archive)
        {
            return;
        }

        var docRelationship = new DocumentRelationship
        {
            Document = entity,
            EntityId = model.ParentEntityId,
            Name = model.ParentEntityType.ToString(),
            EntityType = model.ParentEntityType.GetEntityTypeName()
        };

        await _context.DocumentRelationships.AddAsync(docRelationship);
        await _context.SaveChangesAsync();
    }

    public IEnumerable<DocumentModel> ListDocumentsAsync(string entityName, int entityId)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only query
        return _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .Include(d => d.DocumentRelationships)
            .Include(d => d.DocumentType)
            .Where(x =>
                !x.IsDeleted &&
                x.Type != "folder" &&
                x.DocumentRelationships.Any(y => y.EntityType == entityName && y.EntityId == entityId))
            .AsEnumerable()
            .Select(MapDocumentModel);
    }

    public async Task<(int EntityId, string EntityType)?> GetDocumentParentEntityByIdAsync(int documentId)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only query
        var item = await _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .Include(d => d.DocumentRelationships)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (item == null)
        {
            return null;
        }

        var documentRelationship = item.DocumentRelationships.SingleOrDefault();

        if (documentRelationship == null)
        {
            return null;
        }

        return (documentRelationship.EntityId, documentRelationship.EntityType);
    }

    public async Task<DocumentModel?> GetDocumentByIdAsync(int documentId)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only query
        var item = await _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .Include(d => d.DocumentType)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (item == null)
        {
            return default;
        }

        return MapDocumentModel(item);
    }

    public async Task DeleteDocumentAsync(int documentId)
    {
        var entity = await _documentRepository.GetByIdAsync(documentId);

        if (entity != null)
        {
            // Only try to archive if it's a Google Drive file (has GoogleId and is not a linked file)
            if (entity.LinkedFile == false && !string.IsNullOrEmpty(entity.GoogleId))
            {
                var archiveFolderRoot = _driveConfig.GetSection(DocumentParentEntityType.Archive.ToString()).Value;

                if (string.IsNullOrEmpty(archiveFolderRoot))
                {
                    throw new Exception("Please provide Archive root location in appsettings.");
                }

                await _driveManager.ArchiveFileAsync(entity.GoogleId, archiveFolderRoot);
            }
            // If it's a blob document (no GoogleId), just delete it from database
            // No archiving needed for blob documents

            await _documentRepository.Delete(entity);
        }
    }

    public async Task<DocumentModel> UpdateDocumentAsync(UpdateDocumentRequest request)
    {
        var entity = await _documentRepository.GetByIdAsync(request.Id);

        if (entity == null)
        {
            return default;
        }

        _mapper.Map(request, entity);

        await _documentRepository.UpdateAsync(entity);

        return MapDocumentModel(entity);
    }

    public UNOPSDocument? GetEntityFolderDocument(string entityName, int entityId)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only query
        return _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .Include(d => d.DocumentRelationships)
            .Include(d => d.DocumentType)
            .Where(x =>
                !x.IsDeleted &&
                x.Type == "folder" &&
                x.DocumentRelationships.Any(y => y.EntityType == entityName && y.EntityId == entityId))
            .FirstOrDefault();
    }

    public async Task<IEnumerable<DocumentModel>> GetDocumentsByEntityAsync(string entityName, int entityId)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only query
        var documents = await _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .Include(d => d.DocumentRelationships)
            .Include(d => d.DocumentType)
            .Where(x =>
                !x.IsDeleted &&
                x.Type != "folder" &&
                x.DocumentRelationships.Any(y => y.EntityType == entityName && y.EntityId == entityId))
            .ToListAsync();

        return documents.Select(doc => MapDocumentModel(doc));
    }

    public async Task SetImmutableDocuments(string entityName, int entityId)
    {
        var docs = _documentRepository
            .GetAll(["DocumentRelationships", "DocumentType"])
            .Where(x =>
                !x.IsDeleted &&
                x.Type != "folder" &&
                x.DocumentRelationships.Any(y => y.EntityType == entityName && y.EntityId == entityId))
            .ToList();

        var result = await EnsureFolderStructure(entityName, entityId);
        var parentFolderId = result["id"];

        foreach (var doc in docs)
        {
            UNOPSDocument? entity;
            string docId = doc.GoogleId;

            switch (doc.Type)
            {
                case "application/vnd.google-apps.document":
                    var docx = await ConvertDocument(doc, docId,
                        parentFolderId,
                        $"{doc.Name}.docx",
                        "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

                    entity = _mapper.Map<UNOPSDocument>(docx);

                    break;
                default:
                    if (doc.LinkedFile)
                    {
                        var copy = await CopyDocument(doc, docId, parentFolderId);
                        entity = _mapper.Map<UNOPSDocument>(copy);
                        entity.DocumentTypeId = doc.DocumentTypeId;
                    }
                    else
                    {
                        entity = null;
                    }

                    break;
            }

            if (entity != null)
            {
                entity.LinkedFile = false;

            await _documentRepository.AddAsync(entity);

            var docRelationship = new DocumentRelationship
            {
                Document = entity,
                EntityId = entityId,
                Name = doc.Name,
                EntityType = entityName
            };

            await _context.DocumentRelationships.AddAsync(docRelationship);

            // delete copied document
            await DeleteDocumentAsync(doc.Id);
                //break;
            }
        }
    }

    private async Task<DocumentModel> ConvertDocument(UNOPSDocument doc, string docId, string parentFolderId, string newName, string newMimeType)
    {
        var result = await _driveManager.ExportFileAsync(
            docId,
            newName,
            parentFolderId,
            newMimeType,
            await GetCreatorEmailAsync(doc.Id)

        );

        return new DocumentModel
        {
            Name = newName,
            Type = doc.Type,
            Link = result["webViewLink"],
            GoogleId = result["id"]
        };
    }

    private async Task<DocumentModel> CopyDocument(UNOPSDocument doc, string docId, string parentFolderId)
    {
        var result = _driveManager.CopyFile(
            docId,
            doc.Name,
            parentFolderId,
            doc.Type ?? "application/octet-stream",
            await GetCreatorEmailAsync(doc.Id) // Passing the impersonation user information
        );

        return new DocumentModel
        {
            Name = doc.Name,
            Type = doc.Type,
            Link = result["webViewLink"],
            GoogleId = result["id"]
        };
    }

    /// <summary>
    /// Retrieves the email address of the user that created the document using the CreatedBy value.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <returns>The email address of the creator.</returns>
    public async Task<string> GetCreatorEmailAsync(int documentId)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only query
        var document = await _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null)
        {
            throw new Exception("Document not found.");
        }
        var user = await _userManager.FindByIdAsync(document.CreatedBy.ToString());
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        return user.Email;
    }

    public async Task<byte[]> GetFileContentAsync(string docGoogleId, string userToImpersonate)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only query
        // First check if document has blob data (local storage)
        var document = await _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.GoogleId == docGoogleId);

        if (document?.Blob != null && document.Blob.Length > 0)
        {
            return document.Blob;
        }

        // Fallback to Google Drive if no blob data
        var contents = await _driveManager.GetFileStream(docGoogleId, userToImpersonate);
        return contents.ToArray();
    }

    public async Task<byte[]> GetFileContentByIdAsync(int documentId)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only query
        var document = await _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId);
        
        if (document == null)
        {
            throw new Exception("Document not found.");
        }

        // If blob exists, return it
        if (document.Blob != null && document.Blob.Length > 0)
        {
            return document.Blob;
        }

        // If GoogleId exists, fetch from Google Drive
        if (!string.IsNullOrWhiteSpace(document.GoogleId))
        {
            var userEmail = await GetCreatorEmailAsync(documentId);
            var contents = await _driveManager.GetFileStream(document.GoogleId, userEmail);
            return contents.ToArray();
        }

        throw new Exception("Document has no content available.");
    }

    /// <summary>
    /// Retrieves document details including type and content for AI processing.
    /// This method is called dynamically by the Gemini service as a DataRetrievalMethod.
    /// For GCS documents, returns the storage path for AI service to access directly.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Object containing document type and blob data (or GCS path for cloud-stored documents)</returns>
    public async Task<object> GetDocumentDetailsForAiAsync(int id)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only AI query
        var document = await _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .Include(d => d.DocumentType)
            .FirstOrDefaultAsync(d => d.Id == id);
        
        if (document == null)
        {
            throw new Exception("Document not found.");
        }

        byte[]? contentBytes = null;

        // Check if document is stored in Google Cloud Storage
        var isGcsDocument = !string.IsNullOrWhiteSpace(document.StoragePath) && document.StoragePath.StartsWith("gs://");
        
        if (isGcsDocument)
        {
            // For GCS documents, return the storage path for AI service to access directly
            // The Gemini AI service has direct access to GCS and can read the document content
            return new
            {
                Id = document.Id,
                Name = document.Name,
                Type = document.Type,
                DocumentType = document.DocumentType?.Name ?? "Unknown",
                StoragePath = document.StoragePath,
                IsGcsDocument = true,
                Size = 0 // Size not available without fetching from GCS
            };
        }

        // Get document content (blob or from Google Drive)
        if (document.Blob != null && document.Blob.Length > 0)
        {
            contentBytes = document.Blob;
        }
        else if (!string.IsNullOrWhiteSpace(document.GoogleId))
        {
            try
            {
                var userEmail = await GetCreatorEmailAsync(id);
                var contents = await _driveManager.GetFileStream(document.GoogleId, userEmail);
                contentBytes = contents.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve document content from Google Drive: {ex.Message}");
            }
        }

        if (contentBytes == null || contentBytes.Length == 0)
        {
            throw new Exception("Document has no content available. The document may not have been properly uploaded or saved.");
        }

        return new
        {
            Id = document.Id,
            Name = document.Name,
            Type = document.Type,
            DocumentType = document.DocumentType?.Name ?? "Unknown",
            Blob = contentBytes,
            Size = contentBytes.Length,
            IsGcsDocument = false
        };
    }

    /// <summary>
    /// Data retrieval method for AI prompts - Gets comprehensive document details for opportunity creation
    /// This method is called by the Gemini Manager for document-based opportunity proposals
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Dictionary containing all document details formatted for AI prompt placeholders</returns>
    public async Task<Dictionary<string, object>> GetDocumentDetailsForOpportunityCreationAsync(int id)
    {
        // ✅ OPTIMIZED: Added AsNoTracking for read-only AI query
        var document = await _context.Set<UNOPSDocument>()
            .AsNoTracking()
            .Include(d => d.DocumentType)
            .Include(d => d.DocumentRelationships)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (document == null)
        {
            return null;
        }

        // Extract text content if available (from previous extraction or transcription)
        string extractedText = "";
        
        // Check if document has extracted text stored in GCS
        if (!string.IsNullOrWhiteSpace(document.StoragePath) && document.StoragePath.StartsWith("gs://"))
        {
            // For GCS documents, the storage path contains the extracted text
            // The AI service will access it directly via the gs:// URI
            extractedText = $"[Text available at: {document.StoragePath}]";
        }

        // Get related entities (opportunities, partners, etc.)
        var relatedEntitiesList = document.DocumentRelationships?
            .Where(dr => !dr.IsDeleted)
            .Select(dr => new
            {
                entityType = dr.Name ?? "",
                entityId = dr.EntityId
            }).ToList();
        
        var relatedEntities = relatedEntitiesList != null ? (object)relatedEntitiesList : new List<object>();

        // Build comprehensive document details
        var details = new Dictionary<string, object>
        {
            ["id"] = document.Id,
            ["name"] = document.Name ?? string.Empty,
            ["description"] = string.Empty, // UNOPSDocument doesn't have Description property
            ["type"] = document.Type ?? string.Empty,
            ["documentType"] = document.DocumentType?.Name ?? string.Empty,
            ["link"] = document.Link ?? string.Empty,
            ["googleId"] = document.GoogleId ?? string.Empty,
            ["storagePath"] = document.StoragePath ?? string.Empty,
            ["extractedText"] = extractedText,
            ["uploadDate"] = document.CreatedDate.ToString("yyyy-MM-dd"),
            ["relatedEntities"] = relatedEntities,
            
            // Metadata
            ["hasContent"] = !string.IsNullOrWhiteSpace(document.GoogleId) || 
                            (document.Blob != null && document.Blob.Length > 0) ||
                            !string.IsNullOrWhiteSpace(document.StoragePath),
            ["isLinked"] = document.LinkedFile,
            ["isGcsDocument"] = !string.IsNullOrWhiteSpace(document.StoragePath) && document.StoragePath.StartsWith("gs://"),
            ["fileSize"] = document.Blob?.Length ?? 0,
            ["mimeType"] = GetMimeTypeFromFileType(document.Type ?? "")
        };

        return details;
    }

    /// <summary>
    /// Helper method to get MIME type from file type
    /// </summary>
    private string GetMimeTypeFromFileType(string fileType)
    {
        return fileType.ToLower() switch
        {
            "pdf" => "application/pdf",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xls" => "application/vnd.ms-excel",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ppt" => "application/vnd.ms-powerpoint",
            "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "txt" => "text/plain",
            "csv" => "text/csv",
            "json" => "application/json",
            "xml" => "application/xml",
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
