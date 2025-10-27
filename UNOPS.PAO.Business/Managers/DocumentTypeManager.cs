using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Utilities.Helpers;

namespace UNOPS.PAO.Business.Managers;
public class DocumentTypeManager : IDocumentTypeManager
{
    private IMapper mapper;
    private DataRepository<DocumentType> repository;


    public DocumentTypeManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.repository = new DataRepository<DocumentType>(context);
    }

    public PaginationResponse<DocumentTypeModel> GetDocumentTypesAsync(DocumentTypeRequestParameters request)
    {
        var types = repository
            .GetAll()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrEmpty(request.EntityType))
        {
            types = types
                .Where(x => x.EntityType == request.EntityType);
        }

        return types
            .Paginate(
                mapper.Map<DocumentTypeModel>,
                request
            );
    }
}
