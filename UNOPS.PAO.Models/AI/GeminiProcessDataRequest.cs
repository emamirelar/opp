namespace UNOPS.PAO.Models.AI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

public class GeminiProcessDataRequest
{
    public int Id { get; set; }
    public string? Type { get; set; }
    public string? Message { get; set; }
}