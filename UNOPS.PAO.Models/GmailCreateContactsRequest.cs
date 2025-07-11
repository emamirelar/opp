using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace UNOPS.PAO.Models;
public class GmailCreateRecordsRequest
{
    public List<GmailSelectedContact> SelectedContacts { get; set; } = new List<GmailSelectedContact>();
}

public class GmailSelectedContact
{
    public string EmailAddress { get; set; }
    public string PartnerName { get; set; }
    public int? PartnerId { get; set; }
}