namespace UNOPS.PAO.Models.PartnerTrees;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Shared;

public class PartnerTreeRequest : ExtensibleModel
{
    public string Description { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Type { get; set; }
    public string? Parent { get; set; }
    public string? PartnerCategoryCode { get; set; }
    public string? PartnerGroupCode { get; set; }
}