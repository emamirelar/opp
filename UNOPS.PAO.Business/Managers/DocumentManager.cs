using AutoMapper;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Helpers;

namespace UNOPS.PAO.Business.Managers;

public class DocumentManager: IDocumentManager
{
    private IMapper _mapper;
    private DataRepository<Document> _documentRepository;

    public DocumentManager(IMapper mapper, AppDbContext context)
    {
        _mapper = mapper;
        _documentRepository = new DataRepository<Document>(context);
    }
    
    private Document MapModelToEntity(ExtensibleModel model)
    {
        var entity = _mapper.Map(model, new Document());
        return entity;
    }
    
    private static DocumentModel MapEntityToModel(Document entity, IMapper mapper)
    {
        var result = mapper.Map<Document, DocumentModel>(entity);
        return result;
    }

    public async Task<DocumentModel> CreateDocumentAsync(DocumentUploadModel model)
    {
        var document = await UploadDocumentAsync(model);
        
        var entity = MapModelToEntity(document);
        await _documentRepository.AddAsync(entity);
        return _mapper.Map<DocumentModel>(entity);
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

    public Task DeleteDocumentAsync(int userId, int documentId)
    {
        throw new NotImplementedException();
    }

    public Task<DocumentModel> UploadDocumentAsync(DocumentUploadModel model)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> DownloadDocumentAsync(string documentLink)
    {
        throw new NotImplementedException();
    }

    public Task<DocumentModel> LinkDocumentAsync(DocumentLinkModel model)
    {
        throw new NotImplementedException();
    }
}