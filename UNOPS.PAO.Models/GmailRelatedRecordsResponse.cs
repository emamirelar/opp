using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace UNOPS.PAO.Models;
public class GmailRelatedRecordsResponse
{
    public List<GmailRelatedContact> Contacts { get; set; }
    public List<GmailRelatedPartner> Partners { get; set; }
    public List<GmailRelatedUser> Users { get; set; }
    public List<string> UnmatchedEmails { get; set; }
}

public class GmailRelatedContact
{
    public string Name { get; set; }
    public string Title { get; set; }
    public string PartnerName { get; set; }
    public int Id { get; set; }
}

public class  GmailRelatedPartner
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class GmailRelatedUser
{
    public string Name { get; set; }
    public string Title { get; set; }
}