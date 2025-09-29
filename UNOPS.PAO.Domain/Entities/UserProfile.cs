using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UNOPS.PAO.Domain.Entities;
public class UserProfile : ModifiableDeletableEntity
{
    /// <summary>
    /// The ID of the user this profile belongs to (matches PAOUser.Id)
    /// </summary>
    public int UserId { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    /// <summary>
    /// User's email address
    /// </summary>
    public string? UserEmail { get; set; }
    
    /// <summary>
    /// Organizational unit the user belongs to
    /// </summary>
    public string? OrgUnit { get; set; }
    
    /// <summary>
    /// ID of the user's supervisor
    /// </summary>
    public int? SupervisorId { get; set; }
    
    /// <summary>
    /// Region where the user works
    /// </summary>
    public string? DutyStation { get; set; }
    
    /// <summary>
    /// User's position/job title
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Computed full name from FirstName and LastName
    /// </summary>
    public new string Name
    {
        get
        {
            if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
            {
                return $"{FirstName} {LastName}".Trim();
            }
            else if (!string.IsNullOrEmpty(FirstName))
            {
                return FirstName;
            }
            else if (!string.IsNullOrEmpty(LastName))
            {
                return LastName;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Navigation property to UserPreference
    /// </summary>
    [JsonIgnore]
    public UserPreference? UserPreference { get; set; }
}
