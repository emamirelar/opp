using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.UNOPSDomain.Entities;

using System.Collections.Generic;
using UNOPS.PAO.Domain.Entities;

public class UNOPSContact : Domain.Entities.Contact
{
    public string ContactNumber { get; set; } = string.Empty;
    //public List<Document> Documents { get; set; }
}