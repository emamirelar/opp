namespace UNOPS.PAO.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

public class ExternalPartnerTreeModel : ExtensibleModel
{
	public int Id { get; set; }
    public string? Name { get; set; }
    public string? Department { get; set; }
    public string? Code { get; set; }
    public string? Type { get; set; }
    public string? Parent { get; set; }
}