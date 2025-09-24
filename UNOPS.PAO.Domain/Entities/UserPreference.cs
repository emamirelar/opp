using UNOPS.PAO.Domain.Infrastructure;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Plain class for global filter settings - not an entity, used for JSON serialization
/// </summary>
public class GlobalFilters
{
    /// <summary>
    /// Organization Unit ID for filtering data
    /// </summary>
    public int? OrgUnitId { get; set; }

    /// <summary>
    /// Organization Unit Name (read-only, populated from OrganizationHierarchy)
    /// </summary>
    public string? OrgUnitName { get; set; }

    /// <summary>
    /// Filter to show only records where the current user is involved (created by OR last updated by)
    /// </summary>
    public bool RelatedToMe { get; set; }

    /// <summary>
    /// Filter records by date on this specific date (single date mode)
    /// Applies to both created date AND last updated date
    /// </summary>
    public DateTime? DateOn { get; set; }

    /// <summary>
    /// Filter records by date from this date (inclusive)
    /// Applies to both created date AND last updated date
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Filter records by date to this date (inclusive)  
    /// Applies to both created date AND last updated date
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// User's preferred language code (e.g., "en", "fr", "es")
    /// </summary>
    public string PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// User's theme preference (e.g., "light", "dark")
    /// </summary>
    public string Theme { get; set; } = "light";
}

public class UserPreference : ModifiableDeletableEntity
{
    /// <summary>
    /// Auto-incrementing primary key
    /// </summary>
    public new int Id { get; set; }

    /// <summary>
    /// Foreign key to UserProfile.UserId
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property to UserProfile
    /// </summary>
    [JsonIgnore]
    public UserProfile? UserProfile { get; set; }

    /// <summary>
    /// JSON containing global filter preferences (e.g., OrgUnit, CreatedByMe, etc.)
    /// </summary>
    public string? GlobalFilterJson { get; set; }

    /// <summary>
    /// Strongly typed global filters - not mapped to database
    /// </summary>
    [JsonIgnore]
    public GlobalFilters? GlobalFilters
    {
        get
        {
            if (string.IsNullOrEmpty(GlobalFilterJson))
                return new GlobalFilters();
            
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<GlobalFilters>(GlobalFilterJson);
            }
            catch
            {
                return new GlobalFilters();
            }
        }
        set
        {
            if (value == null)
            {
                GlobalFilterJson = null;
            }
            else
            {
                try
                {
                    GlobalFilterJson = System.Text.Json.JsonSerializer.Serialize(value);
                }
                catch
                {
                    GlobalFilterJson = null;
                }
            }
        }
    }

    /// <summary>
    /// JSON containing other user-specific settings
    /// (e.g., visible columns, collapsed sections, etc.)
    /// </summary>
    public string? AdditionalSettingsJson { get; set; }
}