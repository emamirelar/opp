using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.UNOPSDomain.Entities;

using System.Collections.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities.Common;

public class UNOPSContact : Domain.Entities.Contact
{
    public string ContactNumber { get; set; }
    //public List<Document> Documents { get; set; }
}