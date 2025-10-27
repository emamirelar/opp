using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UNOPS.PAO.Models.Documents;
public class DocumentTypeModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string EntityType { get; set; }
}
