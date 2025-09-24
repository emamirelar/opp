using System.Collections.Generic;
using System;
using UNOPS.PAO.Domain.Infrastructure;
using Newtonsoft.Json;


namespace UNOPS.PAO.Domain.Entities;

public class Contact : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    public string? Salutation { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public string? Suffix { get; set; }
    public required string Title { get; set; }
    public string? Department { get; set; }
    public string? Description { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Assistant { get; set; }
    public string? AssistantPhone { get; set; }
    public string? AssistantEmail { get; set; }
    public string? MailingStreet { get; set; }
    public string? MailingStreet2 { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingStateProvince { get; set; }
    public string? MailingPostalCode { get; set; }
    public string? MailingCountry { get; set; }
    public string? ProfilePictureUrl { get; set; }
    [JsonIgnore]  // Prevents circular reference in serialization
    public virtual ICollection<Interaction>? Interactions { get; set; }
    
    public Contact()
    {
        Interactions = new HashSet<Interaction>();
    }
    
    [JsonIgnore]  // Prevents circular reference in serialization
    public virtual Partner? Partner { get; set; }
    public int PartnerId { get; set; }
    public List<Document>? Documents { get; set; }
}