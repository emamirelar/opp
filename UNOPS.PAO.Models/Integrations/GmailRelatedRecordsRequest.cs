using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace UNOPS.PAO.Models.Integrations;
public class GmailRelatedRecordsRequest
{
    public List<string> EmailAddresses { get; set; }
    public List<int>? partnerIds { get; set; }
}