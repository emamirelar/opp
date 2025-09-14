namespace UNOPS.PAO.UNOPSBusiness.Interfaces;

/// <summary>
/// Service for generating URLs for entities and getting current host information
/// </summary>
public interface IUrlService
{
    /// <summary>
    /// Build URL to a specific entity page
    /// </summary>
    /// <param name="entityType">Type of entity (partner, contact, etc.)</param>
    /// <param name="entityId">ID of the entity</param>
    /// <returns>Full URL to the entity page</returns>
    string BuildEntityUrl(string entityType, int entityId);
    
    /// <summary>
    /// Get the current host URL (useful for other services)
    /// </summary>
    /// <returns>Current host URL</returns>
    string GetCurrentHostUrl();
}
