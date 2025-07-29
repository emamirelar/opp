using System.ComponentModel;
using Google.Api.Gax;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace UNOPS.PAO.GoogleServices;

public class GoogleSecretManagerConfigurationProvider : ConfigurationProvider
{
    private string? ProjectId { get; set; }
    private SecretManagerServiceClient Client { get; set; }
    private readonly IMemoryCache _cache;
    // Secrets get cached for 1 hour so we don't call the secret manager api on every request, though this could be much longer as they rarely change
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleSecretManagerConfigurationProvider"/> class.
    /// 
    /// <param name="projectId">
    /// The Google Cloud project ID to use if the default from <see cref="Platform.Instance()"/> is not available.
    /// </param>
    public GoogleSecretManagerConfigurationProvider(string projectId)
    {
        ProjectName project = new ProjectName(projectId);
        ProjectId = project.ProjectId;
        Client = SecretManagerServiceClient.Create();
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleSecretManagerConfigurationProvider"/> class.
    /// 
    /// This constructor sets the properties <see cref="Client"/> and <see cref="ProjectId"/>.
    /// It first attempts to use the default project ID from <see cref="Platform.Instance()"/>.
    /// </summary>
    public GoogleSecretManagerConfigurationProvider()
    {
        Client = SecretManagerServiceClient.Create();
        var platform = Platform.Instance();
        if (platform != null)
            ProjectId = platform.ProjectId;
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public string? GetSecretVersion(string secretId, string? secretVersion = "latest")
    {
        var secretVersionName = new SecretVersionName(ProjectId, secretId, secretVersion);
        try
        {
            return AccessSecretVersion(secretVersionName);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private string? AccessSecretVersion(SecretVersionName secret)
    {
        var cacheKey = $"{secret.ProjectId}:{secret.SecretId}:{secret.SecretVersionId}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedResult))
        {
            return cachedResult;
        }

        var result = Client.AccessSecretVersion(secret);
        var secretValue = result?.Payload.Data.ToStringUtf8();
        
        // Cache the result with 1 hour expiration
        if (secretValue != null)
        {
            _cache.Set(cacheKey, secretValue, _cacheExpiration);
        }

        return secretValue;
    }
}