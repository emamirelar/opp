using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.Documents;

namespace UNOPS.PAO.Business.Mapping;

/// <summary>
/// AutoMapper value resolver to automatically load Documents for any entity
/// This resolver queries the DocumentRelationship table and maps to DocumentModel
/// </summary>
public class EntityDocumentValueResolver : IValueResolver<object, object, List<DocumentModel>>
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public EntityDocumentValueResolver(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public List<DocumentModel> Resolve(object source, object destination, List<DocumentModel> destMember, ResolutionContext context)
    {
        // Extract entity type and ID from the source object
        var entityType = GetEntityType(source);
        var entityId = GetEntityId(source);

        if (string.IsNullOrEmpty(entityType) || entityId == 0)
        {
            return new List<DocumentModel>();
        }

        // Query Documents directly with a join to DocumentRelationships
        var documents = _context.Documents
            .Where(d => !d.IsDeleted && 
                _context.DocumentRelationships.Any(dr => 
                    dr.DocumentId == d.Id && 
                    dr.EntityType == entityType && 
                    dr.EntityId == entityId && 
                    !dr.IsDeleted))
            .Include(d => d.DocumentType)
            .ToList();

        // Map to DocumentModel
        return documents.Select(doc => new DocumentModel
        {
            Id = doc.Id,
            Name = doc.Name,
            Type = doc.Type,
            Link = doc.Link,
            GoogleId = doc.GoogleId,
            Blob = doc.Blob,
            StoragePath = doc.StoragePath,
            CreatedBy = doc.CreatedBy,
            CreatedDate = doc.CreatedDate,
            LastModifiedBy = doc.LastModifiedBy,
            LastModifiedDate = doc.LastModifiedDate,
            DocumentType = doc.DocumentType != null ? new DocumentTypeModel
            {
                Id = doc.DocumentType.Id,
                Name = doc.DocumentType.Name,
                EntityType = doc.DocumentType.EntityType
            } : null!
        }).ToList();
    }

    private string GetEntityType(object source)
    {
        return source.GetType().Name switch
        {
            nameof(OrganizationHierarchy) => "OrganizationHierarchy",
            nameof(Country) => "Country",
            nameof(Partner) => "Partner",
            nameof(Contact) => "Contact",
            nameof(Interaction) => "Interaction",
            // Add other entity types as needed
            _ => source.GetType().Name
        };
    }

    private int GetEntityId(object source)
    {
        var idProperty = source.GetType().GetProperty("Id");
        if (idProperty != null)
        {
            var value = idProperty.GetValue(source);
            if (value is int intValue)
                return intValue;
        }
        return 0;
    }
}

