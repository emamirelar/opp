using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace UNOPS.PAO.Models.Integrations;
public class GmailInteractionRequest
{
    public string GmailThreadId { get; set; }
    public string GmailMessageId { get; set; }
}
