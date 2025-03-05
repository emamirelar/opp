namespace UNOPS.PAO.Models;

public class PartnerTreeModel
{
    public PartnerTreeDataModel Data { get; set; } = new PartnerTreeDataModel();
    public List<PartnerTreeModel> Children { get; set; } = new List<PartnerTreeModel>();
}

public class PartnerTreeDataModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public string Type { get; set; }
    public string? Parent { get; set; }
    public string Status { get; set; } // Add Status property
}