using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models.Documents;
public class DocumentTypeRequestParameters : RequestParameters
{
    public string EntityType { get; set; }
}
