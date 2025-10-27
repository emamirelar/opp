using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace UNOPS.PAO.Models.Integrations;
public class GmailCreateRecordsRequest
{
    public List<GmailSelectedEmailModel> SelectedContacts { get; set; } = new List<GmailSelectedEmailModel>();
    public string? GmailThreadId { get; set; }
    public string? GmailMessageId { get; set; }
}

public class GmailSelectedEmailModel
{
    public string EmailAddress { get; set; } = string.Empty;
    public string? PartnerName { get; set; }
    public int? PartnerId { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
}