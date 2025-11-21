namespace UNOPS.PAO.Models;

public class OpportunityDeliverableModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int? OutputId { get; set; }
    public string? OutputName { get; set; }
    public string? OutputDescription { get; set; }
    public string? OutputGroup { get; set; }
    public string? OutputSubGroup { get; set; }
    public string? OutputServiceLine { get; set; }
    public string? UnitCode { get; set; }
    public string? ProjectCategoryCode { get; set; }
    public decimal? Quantity { get; set; }
    public string? Notes { get; set; }
}

