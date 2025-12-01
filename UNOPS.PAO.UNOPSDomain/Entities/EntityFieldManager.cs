using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.UNOPSDomain.Entities;

public class EntityFieldManager : ModifiableDeletableEntity
{
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
    
    /// <summary>
    /// Order of the field in list view. Null when ShowInListView is false, otherwise contains the display order in list views.
    /// </summary>
    public int? ListViewOrder { get; set; }
    
    /// <summary>
    /// For relationship fields (DataType is an entity), specifies which property of the related entity to display in list views.
    /// Examples: "Name", "ShortName", "FirstName,LastName", "Name (ShortName)"
    /// </summary>
    [StringLength(200)]
    public string? RelatedDisplayProperty { get; set; }
    
    /// <summary>
    /// Field path for accessing the value in list views. Examples: "partner.name", "fullName", "createdByName"
    /// </summary>
    [StringLength(200)]
    public string? DisplayFieldPath { get; set; }
    
    /// <summary>
    /// Template pattern for combining multiple fields. Examples: "{firstName} {lastName}", "{name} ({shortName})"
    /// </summary>
    [StringLength(500)]
    public string? DisplayTemplate { get; set; }
    
    /// <summary>
    /// Custom label for the list view column. If null, uses default field name translation.
    /// </summary>
    [StringLength(200)]
    public string? ListViewLabel { get; set; }
    
    /// <summary>
    /// Type of list view column: text, avatar, template, multiple-avatars
    /// </summary>
    [StringLength(50)]
    public string? ListViewType { get; set; }
    
    /// <summary>
    /// Column width in list view (e.g., "15%", "200px")
    /// </summary>
    [StringLength(20)]
    public string? ListViewWidth { get; set; }
    
    /// <summary>
    /// Whether to show ellipsis for long text in list view
    /// </summary>
    public bool? ListViewEllipsis { get; set; }
    
    /// <summary>
    /// Whether the column is sortable in list view
    /// </summary>
    public bool? ListViewSortable { get; set; }
    
    /// <summary>
    /// Field to use as fallback for generating initials when avatar image is not available.
    /// Used primarily with 'multiple-avatars' type. Examples: "first5ContactsByDate.firstName", "firstName"
    /// </summary>
    [StringLength(200)]
    public string? FirstLetterFallbackField { get; set; }
    
    /// <summary>
    /// Helper text to assist users with field completion. Displayed as additional guidance in forms.
    /// </summary>
    [StringLength(1000)]
    public string? HelperText { get; set; }
    
    /// <summary>
    /// Thumbnail size for 'thumbnail' type columns (e.g., '32px', '48px', '64px', '80px', '96px', '128px')
    /// </summary>
    [StringLength(20)]
    public string? ThumbnailSize { get; set; }
    
    /// <summary>
    /// Thumbnail shape/border-radius for 'thumbnail' type columns ('square', 'rounded', 'rounded-lg', 'rounded-xl')
    /// </summary>
    [StringLength(20)]
    public string? ThumbnailShape { get; set; }
    
    /// <summary>
    /// Whether to show a border around thumbnails (default: true)
    /// </summary>
    public bool? ThumbnailBorder { get; set; }
    
    /// <summary>
    /// Fallback image URL for 'thumbnail' type columns when field value is empty
    /// </summary>
    [StringLength(500)]
    public string? ThumbnailFallback { get; set; }
    
    // Foreign key to EntityManager
    public int EntityManagerId { get; set; }
    
    // Navigation property to parent entity
    [ForeignKey("EntityManagerId")]
    public virtual EntityManager EntityManager { get; set; } = null!;
} 