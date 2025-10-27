namespace UNOPS.PAO.Models.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

public class BulkUploadRequest
{
    public string Type { get; set; } = string.Empty;

    public List<object> Records { get; set; } = new();

    public bool IsUpdate { get; set; } = false;
}