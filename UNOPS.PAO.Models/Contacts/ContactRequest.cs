namespace UNOPS.PAO.Models.Contacts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Shared;

public class ContactRequest : ExtensibleModel
{
    public string? Salutation { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty; // Required field
    public string? Suffix { get; set; }
    public string Title { get; set; } = string.Empty; // Required field
    public string? Department { get; set; }
    public string? Description { get; set; }
    public string Email { get; set; } = string.Empty; // Required field
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Assistant { get; set; }
    public string? AssistantPhone { get; set; }
    public string? AssistantEmail { get; set; }    
    public string? Status { get; set; } = "Active";
    public string? MailingStreet { get; set; }
    public string? MailingStreet2 { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingStateProvince { get; set; }
    public string? MailingPostalCode { get; set; }
    public string? MailingCountry { get; set; }
    public int PartnerId { get; set; }
    
    /// <summary>
    /// Flag to bypass duplicate detection when user confirms creation despite duplicates
    /// </summary>
    public bool ConfirmDuplicateCreation { get; set; } = false;
    /// <summary>
    /// Organization Unit IDs that this contact should be associated with
    /// </summary>
    public List<int>? OrganizationHierarchyIds { get; set; }
}