namespace UNOPS.PAO.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

public class GeminiAssistantRequest
{
    public string Message { get; set; }
    public Guid sessionId { get; set; }
}