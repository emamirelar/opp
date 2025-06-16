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
    public string? ContactPermission { get; set; }
    public string? PartnerPermission { get; set; }
    public string? UserPermission { get; set; }

    public GmailRelatedRecordsResponse()
    {
        Contacts = new List<GmailRelatedContact>();
        Partners = new List<GmailRelatedPartner>();
        Users = new List<GmailRelatedUser>();
        UnmatchedEmails = new List<string>();
    }
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
    public string PartnerCode { get; set; }
    public string Phone { get; set; }
}

public class GmailRelatedUser
{
    public string Name { get; set; }
    public string Title { get; set; }
}