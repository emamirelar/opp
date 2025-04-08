namespace UNOPS.PAO.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

public class AnalyseFileRequest
{
    public string Type { get; set; } = string.Empty;

    public string FileId { get; set; } = string.Empty;

    public bool IsUpdate { get; set; } = false;
}