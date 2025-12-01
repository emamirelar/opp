using System.Threading.Tasks;

namespace UNOPS.PAO.Business.Interfaces;

/// <summary>
/// Interface for image generation operations using AI
/// </summary>
public interface IImageGenerationManager
{
    /// <summary>
    /// Generate banner and thumbnail images for an opportunity
    /// </summary>
    /// <param name="opportunityName">The name of the opportunity</param>
    /// <param name="opportunityDescription">The description of the opportunity</param>
    /// <param name="countries">Comma-separated list of countries for this opportunity</param>
    /// <param name="intendedImpact">The intended impact and outcomes</param>
    /// <param name="initiativeType">The type of initiative (e.g., infrastructure, capacity building)</param>
    /// <returns>Tuple containing base64-encoded banner and thumbnail images</returns>
    Task<(string? bannerBase64, string? thumbnailBase64)> GenerateOpportunityImagesAsync(
        string opportunityName,
        string opportunityDescription,
        string? countries = null,
        string? intendedImpact = null,
        string? initiativeType = null);
}

