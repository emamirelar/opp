using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Models.EntityConfiguration;

public class CreateEntityConfigurationRequest
{
    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string TableName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool EnableChangeLog { get; set; } = false;
}

public class UpdateEntityConfigurationRequest
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string TableName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool EnableChangeLog { get; set; } = false;
}

public class CreateEntityFieldRequest
{
    [Required]
    public int EntityManagerId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string DataType { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsRequired { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether change logging is enabled for this field
    /// </summary>
    public bool EnableChangeLog { get; set; } = false;
    
    [StringLength(255)]
    public string? DefaultValue { get; set; }
    
    public int? MaxLength { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool ShowInListView { get; set; } = false;
    
    public int? ListViewOrder { get; set; }
    
    [StringLength(200)]
    public string? RelatedDisplayProperty { get; set; }
    
    [StringLength(200)]
    public string? DisplayFieldPath { get; set; }
    
    [StringLength(500)]
    public string? DisplayTemplate { get; set; }
    
    [StringLength(200)]
    public string? ListViewLabel { get; set; }
    
    [StringLength(50)]
    public string? ListViewType { get; set; }
    
    [StringLength(20)]
    public string? ListViewWidth { get; set; }
    
    public bool? ListViewEllipsis { get; set; }
    
    public bool? ListViewSortable { get; set; }
    
    [StringLength(200)]
    public string? FirstLetterFallbackField { get; set; }
    
    [StringLength(1000)]
    public string? HelperText { get; set; }
}

public class UpdateEntityFieldRequest
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public int EntityManagerId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string DataType { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsRequired { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether change logging is enabled for this field
    /// </summary>
    public bool EnableChangeLog { get; set; } = false;
    
    [StringLength(255)]
    public string? DefaultValue { get; set; }
    
    public int? MaxLength { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool ShowInListView { get; set; } = false;
    
    public int? ListViewOrder { get; set; }
    
    [StringLength(200)]
    public string? RelatedDisplayProperty { get; set; }
    
    [StringLength(200)]
    public string? DisplayFieldPath { get; set; }
    
    [StringLength(500)]
    public string? DisplayTemplate { get; set; }
    
    [StringLength(200)]
    public string? ListViewLabel { get; set; }
    
    [StringLength(50)]
    public string? ListViewType { get; set; }
    
    [StringLength(20)]
    public string? ListViewWidth { get; set; }
    
    public bool? ListViewEllipsis { get; set; }
    
    public bool? ListViewSortable { get; set; }
    
    [StringLength(200)]
    public string? FirstLetterFallbackField { get; set; }
    
    [StringLength(1000)]
    public string? HelperText { get; set; }
}

public class SaveEntityConfigurationRequest
{
    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public List<EntityFieldConfigurationDto> Fields { get; set; } = new();
}

public class EntityFieldConfigurationDto
{
    public int? Id { get; set; } // Null for new fields
    
    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string DataType { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsRequired { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether change logging is enabled for this field
    /// </summary>
    public bool EnableChangeLog { get; set; } = false;
    
    [StringLength(255)]
    public string? DefaultValue { get; set; }
    
    public int? MaxLength { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool ShowInListView { get; set; } = false;
    
    public int? ListViewOrder { get; set; }
    
    [StringLength(200)]
    public string? RelatedDisplayProperty { get; set; }
    
    [StringLength(200)]
    public string? DisplayFieldPath { get; set; }
    
    [StringLength(500)]
    public string? DisplayTemplate { get; set; }
    
    [StringLength(200)]
    public string? ListViewLabel { get; set; }
    
    [StringLength(50)]
    public string? ListViewType { get; set; }
    
    [StringLength(20)]
    public string? ListViewWidth { get; set; }
    
    public bool? ListViewEllipsis { get; set; }
    
    public bool? ListViewSortable { get; set; }
    
    [StringLength(200)]
    public string? FirstLetterFallbackField { get; set; }
    
    [StringLength(1000)]
    public string? HelperText { get; set; }
}

public class EntityConfigurationDetailsResponse
{
    public int? Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string? TableName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool EnableChangeLog { get; set; }
    public List<EntityFieldConfigurationDto> Fields { get; set; } = new();
}

public class RelatedFieldOptionDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsTemplate { get; set; } = false;
    public string? TemplatePattern { get; set; }
    public string? FieldPath { get; set; }
}

public class ListViewColumnDto
{
    public string Field { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public bool Sortable { get; set; } = true;
    public string? Width { get; set; }
    public bool Ellipsis { get; set; } = false;
    public string? TemplatePattern { get; set; }
    public string? DisplayFieldPath { get; set; }
    public string? FirstLetterFallbackField { get; set; }
    public string? HelperText { get; set; }
    
    // Thumbnail configuration properties
    public string? ThumbnailSize { get; set; }
    public string? ThumbnailShape { get; set; }
    public bool? ThumbnailBorder { get; set; }
    public string? ThumbnailFallback { get; set; }
} 