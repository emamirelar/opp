namespace UNOPS.PAO.Models.Opportunities;

public class RelatedItemsModel
{
    public List<RelatedContactModel> Contacts { get; set; } = new();
    public List<RelatedPartnerModel> Partners { get; set; } = new();
    public List<RelatedInteractionModel> Interactions { get; set; } = new();
}

public class RelatedContactModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? JobTitle { get; set; }
    public string? LogoUrl { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
}

public class RelatedPartnerModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? PartnerType { get; set; }
    public string? Country { get; set; }
}

public class RelatedInteractionModel
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? InteractionType { get; set; }
    public DateTime? InteractionDate { get; set; }
    public string? Description { get; set; }
    public int? PartnerId { get; set; }
    public string? PartnerName { get; set; }
}

