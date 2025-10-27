namespace UNOPS.PAO.Models.AI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

public class GeminiAccessibilityRequest
{
    public string SessionId { get; set; }
    public bool? TextToSpeech { get; set; } = false;
}