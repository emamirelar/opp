namespace UNOPS.PAO.Models.AI;

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

    public string? SheetName { get; set; }

    public int CurrentUserId { get; set; } // User who initiated the analysis

    public bool IsUpdate { get; set; } = false;
}