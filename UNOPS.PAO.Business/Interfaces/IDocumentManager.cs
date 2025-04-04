using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Interfaces;

public interface IDocumentManager
{
    IEnumerable<DocumentModel> ListDocumentsAsync(string entityName, int entityId);
    Task<DocumentModel?> GetDocumentByIdAsync(int documentId);
    Task<DocumentModel> UpdateDocumentAsync(UpdateDocumentRequest request);
    Task<(int EntityId, string EntityType)?> GetDocumentParentEntityByIdAsync(int documentId);
}