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
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities.Common;
using UNOPS.PAO.Utilities.Helpers;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSDocumentManager : IDocumentManager
{
    private IMapper _mapper;
    private BaseRepository<UNOPSDocument> _documentRepository;
    private UNOPSAppDbContext _unopsAppDbContext;
    private readonly IGoogleDriveDocumentManager _driveManager;
    private readonly IConfigurationSection _driveConfig;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly BaseRepository<UNOPSContact> _contactRepository;
    private readonly BaseRepository<UNOPSPartner> _partnerRepository;
    //private readonly DataRepository<Project> _projectManager;

    public UNOPSDocumentManager(
        IGoogleDriveDocumentManager driveManager,
        IConfiguration configuration,
        IMapper mapper,
        UNOPSAppDbContext context,
        UserManager<PAOIdentityUser> userManager
        )
    {
        _mapper = mapper;
        _unopsAppDbContext = context;
        _documentRepository = new BaseRepository<UNOPSDocument>(context);
        _driveManager = driveManager;
        _driveConfig = configuration.GetSection($"GoogleDriveSettings:DefaultGoogleDriveFolderIds");
        _userManager = userManager;
        //_projectManager = new DataRepository<Project>(context); ;
        _contactRepository = new BaseRepository<UNOPSContact>(context);
        _partnerRepository = new BaseRepository<UNOPSPartner>(context);
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
        var folderDocument = _documentRepository
            .GetAll()
            //.NotDeleted()
            .Where(x => x.Type == "folder" && x.GoogleId == googleId && !x.IsDeleted)
            .FirstOrDefault();

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

        await _unopsAppDbContext.DocumentRelationships.AddAsync(docRelationship);
        await _unopsAppDbContext.SaveChangesAsync();
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

            default:
                throw new Exception("Invalid entity type.");
        }
    }

    private async Task<int> GetParentEntityId(string entityType, int entityId)
    {
        switch (entityType)
        {
            case "Contact":
                var contact = await _contactRepository.GetByIdAsync(entityId, ["Partner"]);
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
                var contact = await _contactRepository.GetByIdAsync(entityId);
                if (contact == null)
                {
                    throw new Exception("Contact not found.");
                }
                return contact.Name;

            case "Partner":
                var partner = await _partnerRepository.GetByIdAsync(entityId);
                if (partner == null)
                {
                    throw new Exception("Partner not found.");
                }
                return partner.Name;

            default:
                throw new Exception("Invalid entity type.");
        }
    }

    public async Task<DocumentModel> CreateDocumentAsync(DocumentUploadModel model)
    {
        var parentFolder = await EnsureFolderStructure(model.ParentEntityType.ToString(), model.ParentEntityId);

        var documentModel = await UploadDocumentAsync(model, parentFolder["id"]);

        var documentEntity = _mapper.Map<UNOPSDocument>(documentModel);
        documentEntity.LinkedFile = false;
        documentEntity.DocumentTypeId = model.DocumentTypeId;

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

        await _unopsAppDbContext.DocumentRelationships.AddAsync(docRelationship);
        await _unopsAppDbContext.SaveChangesAsync();
    }

    public IEnumerable<DocumentModel> ListDocumentsAsync(string entityName, int entityId)
    {
        return _documentRepository
            .GetAll(["DocumentRelationships", "DocumentType"])
            .Where(x =>
                !x.IsDeleted &&
                x.Type != "folder" &&
                x.DocumentRelationships.Any(y => y.EntityType == entityName && y.EntityId == entityId))
            .Select(MapDocumentModel);
    }

    public async Task<(int EntityId, string EntityType)?> GetDocumentParentEntityByIdAsync(int documentId)
    {
        var item = await _documentRepository.GetByIdAsync(documentId, new[] { "DocumentRelationships" });

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
        var item = await _documentRepository.GetByIdAsync(documentId, ["DocumentType"]);

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
            if (entity.LinkedFile == false)
            {
                var archiveFolderRoot = _driveConfig.GetSection(DocumentParentEntityType.Archive.ToString()).Value;

                if (string.IsNullOrEmpty(archiveFolderRoot))
                {
                    throw new Exception("Please provide Archive root location in appsettings.");
                }

                await _driveManager.ArchiveFileAsync(entity.GoogleId, archiveFolderRoot);
            }

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
        return _documentRepository
            .GetAll(["DocumentRelationships", "DocumentType"])
            .Where(x =>
                !x.IsDeleted &&
                x.Type == "folder" &&
                x.DocumentRelationships.Any(y => y.EntityType == entityName && y.EntityId == entityId))
            .FirstOrDefault();
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

                await _unopsAppDbContext.DocumentRelationships.AddAsync(docRelationship);

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
        var document = await _documentRepository.GetByIdAsync(documentId);
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
        var contents = await _driveManager.GetFileStream(docGoogleId, userToImpersonate);

        return contents.ToArray();
    }
}
