using AutoMapper;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Helpers;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Utilities.Helpers;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class DocumentManager : IDocumentManager
{
    private IMapper _mapper;
    private BaseRepository<UNOPSDocument> _documentRepository;
    private UNOPSAppDbContext _unopsAppDbContext;
    private readonly IGoogleDriveDocumentManager _driveManager;
    private readonly IConfigurationSection _driveConfig;
    private IDocumentManager _iunopsDocumentManagerImplementation;

    public DocumentManager(IGoogleDriveDocumentManager driveManager, IConfiguration configuration, IMapper mapper, UNOPSAppDbContext context)
    {
        _mapper = mapper;
        _unopsAppDbContext = context;
        _documentRepository = new BaseRepository<UNOPSDocument>(context);
        _driveManager = driveManager;
        _driveConfig = configuration.GetSection($"GoogleDriveSettings:DefaultGoogleDriveFolderIds");
    }

    private UNOPSDocument MapModelToEntity(DocumentModel model)
    {
        var entity = _mapper.Map(model, new UNOPSDocument(model.LinkedFile));
        return entity;
    }
    
    private static DocumentModel MapEntityToModel(UNOPSDocument entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSDocument, DocumentModel>(entity);
        return result;
    }
    
    public async Task<DocumentModel> UploadDocumentAsync(DocumentUploadModel model)
    {
        var fileType = model.File.GetFileType();
        var parentEntityFolderRoot = _driveConfig.GetSection(model.ParentEntityType.ToString()).Value;
        if (String.IsNullOrEmpty(parentEntityFolderRoot))
            throw new Exception("Please provide root location in appsettings.");

        var entityFolderId =
            await _driveManager.CreateFolderAsync(model.ParentEntityId.ToString(), parentEntityFolderRoot);
        
        var webViewLink = await _driveManager.UploadFileAsync(
            model.File,
            model.Name,
            entityFolderId
        );
        
        return new DocumentModel
        {
            Name = model.Name,
            Type = fileType,
            Link = webViewLink
        };
    }

    public Task<Stream> DownloadDocumentAsync(string documentLink)
    {
        throw new NotImplementedException();
    }

    public async Task<DocumentModel> LinkDocumentAsync(DocumentLinkModel model)
    {
        var document = new DocumentModel
        {
            Name = model.Name,
            Link = model.Link,
            Type = model.Type
        };
        
        var entity = MapModelToEntity(document);
        await _documentRepository.AddAsync(entity);
        await HandleDocumentRelationships(entity, model);
        return _mapper.Map<DocumentModel>(entity);
    }

    public async Task<DocumentModel> CreateDocumentAsync(DocumentUploadModel model)
    {
        var document = await UploadDocumentAsync(model);
        
        var entity = MapModelToEntity(document);
        await _documentRepository.AddAsync(entity);
        await HandleDocumentRelationships(entity, model);

        return _mapper.Map<DocumentModel>(entity);
    }

    private async Task HandleDocumentRelationships(UNOPSDocument entity, DocumentBaseCreateModel model)
    {
        if (model.ParentEntityType == DocumentParentEntityType.Archive)
            return;
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

    public IEnumerable<DocumentModel> ListDocumentsAsync()
    {
        return _documentRepository
            .GetAll()
            .Select(_mapper.Map<DocumentModel>);
    }

    public IEnumerable<DocumentModel> ListUserDocumentsAsync(int userId)
    {
        return _documentRepository
            .GetAll()
            .Where(x => x.CreatedBy == userId)
            .Select(_mapper.Map<DocumentModel>);
    }

    public async Task<DocumentModel?> GetDocumentByIdAsync(int userId, int documentId)
    {
        var item = await _documentRepository.GetByIdAsync(documentId);

        if (item == null || item.CreatedBy != userId)
        {
            return default;
        }

        return MapEntityToModel(item, _mapper);
    }

    public async Task DeleteDocumentAsync(int userId, int documentId)
    {
        var entity = await _documentRepository.GetByIdAsync(documentId);

        if (entity != null && entity.CreatedBy == userId)
        {
            await _documentRepository.Delete(entity);
            var archiveFolderRoot = _driveConfig.GetSection(DocumentParentEntityType.Archive.ToString()).Value;
            if (String.IsNullOrEmpty(archiveFolderRoot))
                throw new Exception("Please provide Archive root location in appsettings.");
            
            await _driveManager.ArchiveFileAsync(entity.Link.GetFileIdFromDriveLink(), archiveFolderRoot);
        }
    }
}