using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.UNOPSDomain.Entities;

using System.Collections.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities.Common;

public class UNOPSPartner : Domain.Entities.Partner
{
    public string PartnerNumber { get; set; }
    //public List<Document> Documents { get; set; }
}