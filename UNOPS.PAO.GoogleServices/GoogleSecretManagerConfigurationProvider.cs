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
    /// <param name="credentialsSecretName">
    /// The name of the secret containing the service account JSON.
    /// </param>
    public GoogleSecretManagerConfigurationProvider(string projectId, string credentialsSecretName)
    {
        ProjectName project = new ProjectName(projectId);
        ProjectId = project.ProjectId;

        var secretManagerClient = SecretManagerServiceClient.Create();
        var secretVersionName = new SecretVersionName(projectId, credentialsSecretName, "latest");
        var secret = secretManagerClient.AccessSecretVersion(secretVersionName);
        var credentialsJson = secret.Payload.Data.ToStringUtf8();

        GoogleCredential credential = GoogleCredential.FromJson(credentialsJson);
        Client = new SecretManagerServiceClientBuilder
        {
            ChannelCredentials = credential.ToChannelCredentials()
        }.Build();
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