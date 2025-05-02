using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Utilities.Helpers;

public static class DocumentExtensions
{
    private static readonly Dictionary<DocumentParentEntityType, Type> EntityTypeMap = new()
    {
        { DocumentParentEntityType.Contact, typeof(Contact) },
        { DocumentParentEntityType.Partner, typeof(Partner) },
        { DocumentParentEntityType.Interaction, typeof(Interaction) }
    };

    public static string GetEntityTypeName(this DocumentParentEntityType parentEntityType)
    {
        if (EntityTypeMap.TryGetValue(parentEntityType, out Type? entityType))
        {
            return entityType.Name;
        }
        
        throw new ArgumentException($"Unknown parent entity type: {parentEntityType}");
    }
}