namespace UNOPS.PAO.Models.AI;

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

public class GeminiFileRequest
{
    public IFormFile? File { get; set; }
    public string? Type { get; set; }
}