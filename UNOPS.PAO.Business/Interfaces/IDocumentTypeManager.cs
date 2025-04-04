using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Document;

namespace UNOPS.PAO.Business.Interfaces;
public interface IDocumentTypeManager
{
    PaginationResponse<DocumentTypeModel> GetDocumentTypesAsync(DocumentTypeRequestParameters request);
}
