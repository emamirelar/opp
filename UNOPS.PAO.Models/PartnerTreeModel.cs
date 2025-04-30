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
    public bool PartnerCategoryEditable { get; set; }
    public bool PartnerGroupEditable { get; set; }
    
    private string? _partnerCategoryCode;
    public string? PartnerCategoryCode 
    { 
        get => string.IsNullOrEmpty(_partnerCategoryCode) ? Code : _partnerCategoryCode;
        set => _partnerCategoryCode = value; 
    }

    private string? _partnerCategoryName;

    public string? PartnerCategoryName 
    { 
        get => string.IsNullOrEmpty(_partnerCategoryName) ? Name : _partnerCategoryName;
        set => _partnerCategoryName = value; 
    }
    
    private string? _partnerGroupCode;
    public string? PartnerGroupCode 
    { 
        get => string.IsNullOrEmpty(_partnerGroupCode) ? Code : _partnerGroupCode;
        set => _partnerGroupCode = value; 
    }

    private string? _partnerGroupName;

    public string? PartnerGroupName 
    { 
        get => string.IsNullOrEmpty(_partnerGroupName) ? Name : _partnerGroupName;
        set => _partnerGroupName = value; 
    }
}