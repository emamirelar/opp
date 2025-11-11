using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Documents;

namespace UNOPS.PAO.Business.Interfaces;

public interface IDocumentManager
{
    IEnumerable<DocumentModel> ListDocumentsAsync(string entityName, int entityId);
    Task<IEnumerable<DocumentModel>> GetDocumentsByEntityAsync(string entityName, int entityId);
    Task<DocumentModel?> GetDocumentByIdAsync(int documentId);
    Task<DocumentModel> UpdateDocumentAsync(UpdateDocumentRequest request);
    Task<(int EntityId, string EntityType)?> GetDocumentParentEntityByIdAsync(int documentId);
    Task<byte[]> GetFileContentByIdAsync(int documentId);
}