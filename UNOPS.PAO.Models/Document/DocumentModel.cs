using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models.Audit;

namespace UNOPS.PAO.Models;
public class DocumentModel: DocumentBaseModel, IModifiableEntityModel<int, int>
{
    public int Id { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string Link { get; set; }
}