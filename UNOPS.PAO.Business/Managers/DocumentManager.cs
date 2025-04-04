using AutoMapper;
using Microsoft.CodeAnalysis;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Managers;

public class DocumentManager : IDocumentManager
{
    private IMapper _mapper;
    private DataRepository<Document> _documentRepository;

    public DocumentManager(IMapper mapper, AppDbContext context)
    {
        _mapper = mapper;
        _documentRepository = new DataRepository<Document>(context);
    }

    public IEnumerable<DocumentModel> ListDocumentsAsync(string entityName, int entityId)
    {
        return _documentRepository
            .GetAll(["DocumentRelationships", "DocumentType"])
            .Where(x =>
                !x.IsDeleted &&
                x.Type != "folder" &&
                x.DocumentRelationships.Any(y => y.EntityType == entityName && y.EntityId == entityId)
            )
            .Select(_mapper.Map<DocumentModel>);
    }

    public async Task<DocumentModel?> GetDocumentByIdAsync(int documentId)
    {
        var item = await _documentRepository.GetByIdAsync(documentId, ["DocumentType"]);

        if (item == null)
        {
            return default;
        }

        return _mapper.Map<DocumentModel>(item);
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

    public async Task<DocumentModel> UpdateDocumentAsync(UpdateDocumentRequest request)
    {
        var entity = await _documentRepository.GetByIdAsync(request.Id);

        if (entity == null)
        {
            return default;
        }

        _mapper.Map(request, entity);

        await _documentRepository.UpdateAsync(entity);

        return _mapper.Map<DocumentModel>(entity);
    }
}