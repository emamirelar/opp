using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Interfaces;

public interface IDocumentManager
{
    Task<DocumentModel> CreateDocumentAsync(DocumentUploadModel model);
    IEnumerable<DocumentModel> ListDocumentsAsync();
    IEnumerable<DocumentModel> ListUserDocumentsAsync(int userId);
    Task<DocumentModel?> GetDocumentByIdAsync(int userId, int documentId);
    Task DeleteDocumentAsync(int userId, int documentId);
    Task<DocumentModel> UploadDocumentAsync(DocumentUploadModel model);
    Task<Stream> DownloadDocumentAsync(string documentLink);
    Task<DocumentModel> LinkDocumentAsync(DocumentLinkModel model);
}