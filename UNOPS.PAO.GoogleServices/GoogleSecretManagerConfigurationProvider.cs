using System.ComponentModel;
using Google.Api.Gax;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Microsoft.Extensions.Configuration;

namespace UNOPS.PAO.GoogleServices;

public class GoogleSecretManagerConfigurationProvider : ConfigurationProvider
{
    private string? ProjectId { get; set; }
    private SecretManagerServiceClient Client { get; set; }

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
        var result = Client.AccessSecretVersion(secret);

        // Convert the payload to a string. Payloads are bytes by default.
        return result?.Payload.Data.ToStringUtf8();
    }
}