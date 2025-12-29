using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

/// <summary>
/// Manager for generating images using Gemini's image generation API
/// </summary>
public class ImageGenerationManager : IImageGenerationManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImageGenerationManager> _logger;

    private const string GEMINI_IMAGE_MODEL = "gemini-2.5-flash-image";
    
    // UNOPS Design System Colors
    private const string UNOPS_COLORS = @"
        Primary Colors:
        - UNOPS Blue: #0092d1
        - UNOPS Midnight Blue: #004976
        - UNOPS Deep Sea: #0f172a
        
        Accent Colors:
        - Yellow: #f8ea44
        - Orange: #e85c0e
        - Cherry: #991e66
        - Lime: #c4d600
        - Teal: #00a997
        - Ocean: #4ec3e0
        
        Surfaces:
        - Primary: #ffffff
        - Secondary: #f8fafc
        - Cool: #f6f9fc
        - Fresh: #f5fbfd
        - Mint: #ecf8f7
    ";

    public ImageGenerationManager(
        IConfiguration configuration,
        ILogger<ImageGenerationManager> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Generate banner and thumbnail images for an opportunity
    /// </summary>
    public async Task<(string? bannerBase64, string? thumbnailBase64)> GenerateOpportunityImagesAsync(
        string opportunityName,
        string opportunityDescription,
        string? countries = null,
        string? intendedImpact = null,
        string? initiativeType = null)
    {
        try
        {
            _logger.LogInformation("Generating images for opportunity: {OpportunityName} in countries: {Countries}", 
                opportunityName, countries ?? "Not specified");

            // Generate banner image (16:9 aspect ratio, 1376x768)
            var bannerPrompt = CreateBannerPrompt(opportunityName, opportunityDescription, countries, intendedImpact, initiativeType);
            var bannerBase64 = await GenerateImageAsync(bannerPrompt, "16:9");

            // Generate thumbnail image (1:1 aspect ratio, 1024x1024)
            var thumbnailPrompt = CreateThumbnailPrompt(opportunityName, opportunityDescription, countries, intendedImpact, initiativeType);
            var thumbnailBase64 = await GenerateImageAsync(thumbnailPrompt, "1:1");

            _logger.LogInformation("Successfully generated images for opportunity: {OpportunityName}", opportunityName);

            // Add data URI prefix to make them ready for direct use in HTML
            var bannerDataUri = !string.IsNullOrEmpty(bannerBase64) ? $"data:image/png;base64,{bannerBase64}" : null;
            var thumbnailDataUri = !string.IsNullOrEmpty(thumbnailBase64) ? $"data:image/png;base64,{thumbnailBase64}" : null;

            return (bannerDataUri, thumbnailDataUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating images for opportunity: {OpportunityName}", opportunityName);
            throw;
        }
    }

    /// <summary>
    /// Create the banner image prompt based on opportunity details
    /// </summary>
    private string CreateBannerPrompt(string opportunityName, string opportunityDescription, string? countries, string? intendedImpact, string? initiativeType)
    {
        var contextDetails = new StringBuilder();
        contextDetails.AppendLine($"Opportunity Name: {opportunityName}");
        contextDetails.AppendLine($"Description: {opportunityDescription}");
        
        if (!string.IsNullOrWhiteSpace(countries))
            contextDetails.AppendLine($"Countries/Regions: {countries}");
        
        if (!string.IsNullOrWhiteSpace(initiativeType))
            contextDetails.AppendLine($"Initiative Type: {initiativeType}");
        
        if (!string.IsNullOrWhiteSpace(intendedImpact))
            contextDetails.AppendLine($"Intended Impact: {intendedImpact}");

        return $@"Create a flowing watercolor banner image for a UNOPS partnership opportunity with the following details:

{contextDetails}

Design Requirements:
- Art Style: Fluid watercolor painting with flowing, blending colors that merge and transition naturally across the canvas
- Use UNOPS color palette: {UNOPS_COLORS}
- Background: MUST be solid rgb(248, 250, 252) - a soft off-white/pale blue-grey
- Portray the ESSENCE of this specific opportunity, countries, and intended impact through expressive, painterly imagery
- If countries are specified, incorporate recognizable geographical or cultural elements with artistic interpretation
- Use flowing, organic watercolor brushstrokes with gouache for areas of stronger color
- Apply soft color blending and natural transitions between hues
- Incorporate thematic elements: landscapes, people, development symbols, natural elements, collaboration motifs
- Use UNOPS blues and teals as primary colors with warm accent colors (yellows, oranges, teals)
- Composition should feel organic, warm, and hopeful - artistic yet professional for a UN organization
- Create depth through layered washes and color transparency
- Include soft edges and natural color bleeding typical of watercolor techniques
- DO NOT include any text, words, or written characters
- Evoke themes through flowing colors: connection (colors flowing together), hope (warm flowing hues), growth (organic color expansion), collaboration (color harmony)
- Style: Flowing watercolor, fluid colors, soft blending, organic movement, transparent washes, luminous color flows
- Mood: Hopeful, warm, fluid, dynamic, optimistic, gentle, collaborative, alive with flowing color
- Visual language: Flowing colors, soft transitions, color bleeding, transparent overlays, fluid movement, natural color merging
- IMPORTANT: Background MUST be rgb(248, 250, 252) throughout
- IMPORTANT: Fill the ENTIRE 16:9 canvas edge-to-edge with no black borders or letterboxing
- IMPORTANT: Emphasize FLOW - colors should move across the canvas like water flowing naturally
- 16:9 aspect ratio banner format";
    }

    /// <summary>
    /// Create the thumbnail image prompt based on opportunity details
    /// </summary>
    private string CreateThumbnailPrompt(string opportunityName, string opportunityDescription, string? countries, string? intendedImpact, string? initiativeType)
    {
        var contextDetails = new StringBuilder();
        contextDetails.AppendLine($"Opportunity Name: {opportunityName}");
        contextDetails.AppendLine($"Description: {opportunityDescription}");
        
        if (!string.IsNullOrWhiteSpace(countries))
            contextDetails.AppendLine($"Countries/Regions: {countries}");
        
        if (!string.IsNullOrWhiteSpace(initiativeType))
            contextDetails.AppendLine($"Initiative Type: {initiativeType}");
        
        if (!string.IsNullOrWhiteSpace(intendedImpact))
            contextDetails.AppendLine($"Intended Impact: {intendedImpact}");

        return $@"Design a simple, iconic logo for a UNOPS partnership opportunity with the following details:

{contextDetails}

Design Requirements:
- Art Style: Simple shapes and curves - minimalist geometric design with clean forms
- Use UNOPS color palette: {UNOPS_COLORS}
- Color Palette: Use EXACTLY 4 colors maximum:
  * TWO primary color (choose two): UNOPS Blue #0092d1 OR Midnight Blue #004976
  * TWO accent colors from: Teal #00a997, Ocean #4ec3e0, Yellow #f8ea44, Orange #e85c0e, Cherry #991e66, Lime #c4d600
- Background: MUST be TRANSPARENT (alpha channel, no solid background)
- Design using SIMPLE SHAPES: circles, ovals, arcs, crescents, triangles, rectangles, rounded rectangles
- Use SIMPLE CURVES: smooth arcs, gentle waves, flowing S-curves, circular segments
- Maximum 2-4 basic shapes combined - EXTREME simplicity required
- Shapes should overlap or connect to create meaningful composition
- Use bold, solid fills (no gradients) - each shape filled with one of your 3 colors
- If countries are specified, use simplified iconic shapes that represent them symbolically
- Think: modern app icon, brand logo, simple badge, iconic symbol
- Symbolically represent the opportunity's theme through shape combination
- Keep every element LARGE and BOLD - must be instantly recognizable at 48px size
- NO fine details, NO thin lines, NO complex patterns
- DO NOT include any text, words, letters, or written characters
- Style: Minimalist icon, simple geometry, bold shapes, clean curves, modern flat design
- Mood: Clear, professional, modern, confident, uncluttered, instantly recognizable
- Logo principles: Maximum simplicity, crystal clarity, bold forms, perfect scalability
- CRITICAL: Logo will be displayed at 48px x 48px - every element must be clearly visible at tiny sizes
- CRITICAL: Use thick, bold shapes (nothing thinner than 15% of canvas width)
- CRITICAL: Logo should FILL THE CANVAS with NO PADDING - maximize logo size to use entire square
- CRITICAL: The logo design must extend close to all edges of the canvas for maximum visibility
- IMPORTANT: Background MUST be ISOLATED on SOLID WHITE BACKGROUND with no other elements or colors
- IMPORTANT: Use EXACTLY 3 colors - one primary base color + two accent colors
- IMPORTANT: Each shape should be large enough to be clearly distinguished at 48px
- IMPORTANT: NO empty space or padding around the logo - fill the entire 1:1 canvas
- 1:1 square aspect ratio for logo format";
    }

    /// <summary>
    /// Generate a single image using Gemini API - EXACT same pattern as AiContextualService.CallGeminiApi
    /// </summary>
    private async Task<string?> GenerateImageAsync(string prompt, string aspectRatio)
    {
        try
        {
            var projectId = _configuration["AISettings:ProjectId"];
            var location = _configuration["AISettings:Location"];
            // var location = "us-central1";
            var url = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/publishers/google/models/{GEMINI_IMAGE_MODEL}:generateContent";

            // Get access token using EXACT same method as AiContextualService
            string accessToken = await GetAccessTokenAsync();

            // Prepare request body
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    responseModalities = new[] { "IMAGE" },
                    imageConfig = new
                    {
                        aspectRatio = aspectRatio,
                        imageSize = "1K"
                    }
                }
            };

            string jsonRequest = JsonConvert.SerializeObject(requestBody);
            
            // Call Gemini API using EXACT same pattern as AiContextualService
            string responseContent = await CallGeminiApiAsync(url, jsonRequest, accessToken);

            // Parse response to extract base64 image
            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            if (jsonResponse?.candidates != null && jsonResponse.candidates.Count > 0)
            {
                var candidate = jsonResponse.candidates[0];
                if (candidate?.content?.parts != null && candidate.content.parts.Count > 0)
                {
                    foreach (var part in candidate.content.parts)
                    {
                        if (part?.inlineData?.data != null)
                        {
                            string base64Data = part.inlineData.data.ToString();
                            _logger.LogInformation("Successfully generated image with aspect ratio: {AspectRatio}", aspectRatio);
                            return base64Data;
                        }
                    }
                }
            }

            _logger.LogWarning("No image data found in Gemini response");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image with Gemini API");
            throw;
        }
    }

    /// <summary>
    /// Get access token - EXACT same implementation as AiContextualService
    /// </summary>
    private static async Task<string> GetAccessTokenAsync()
    {
        GoogleCredential credential = await GoogleCredential.GetApplicationDefaultAsync();
        credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }

    /// <summary>
    /// Call Gemini API with retry logic - EXACT same implementation as AiContextualService
    /// </summary>
    private static async Task<string> CallGeminiApiAsync(string url, string jsonRequest, string accessToken, int maxRetries = 5)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // Retry after a delay in case of an error response
                    TimeSpan waitTime = TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(new Random().Next(0, 1000));
                    Console.WriteLine($"Gemini API error. Retrying in {waitTime.TotalSeconds:F2} seconds (Attempt {attempt + 1}/{maxRetries})");
                    await Task.Delay(waitTime);
                }
            }
        }
        // Respond with the most recent error after max retries are reached
        var errorContent = await response.Content.ReadAsStringAsync();
        throw new Exception($"Gemini API returned error after {maxRetries} attempts: {response.StatusCode} - {errorContent}");
    }
}

