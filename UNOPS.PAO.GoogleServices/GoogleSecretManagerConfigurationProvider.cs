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
    /// Initializes a new instance of the <see cref="GoogleSecretManagerConfigurationProvider"/> class
    /// with the specified project ID.
    /// </summary>
    /// <param name="projectId">
    /// The Google Cloud project ID to use if the default from <see cref="Platform.Instance()"/> is not available.
    /// </param>
    public GoogleSecretManagerConfigurationProvider(string? projectId = null)
    {
        try
        {
            Client = SecretManagerServiceClient.Create();
            var platform = Platform.Instance();
            if (platform != null && platform.ProjectId != null)
            {
                ProjectId = platform.ProjectId;
            }
            else if (projectId != null)
            {
                ProjectName project = new ProjectName(projectId);
                ProjectId = project.ProjectId;
            }
        }
        catch (Exception exception)
        {
            throw new WarningException($"Error occurred when trying to setup the Secret Manager Service Client. {exception}, {exception.StackTrace}");
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleSecretManagerConfigurationProvider"/> class
    /// with custom credentials.
    /// </summary>
    /// <param name="projectId">
    /// The Google Cloud project ID to use.
    /// </param>
    /// <param name="credential">
    /// The Google credentials to use for authentication.
    /// </param>
    public GoogleSecretManagerConfigurationProvider(string projectId, GoogleCredential credential)
    {
        ProjectName project = new ProjectName(projectId);
        ProjectId = project.ProjectId;
        Client = new SecretManagerServiceClientBuilder
        {
            ChannelCredentials = credential.ToChannelCredentials()
        }.Build();
    }

    /// <summary>
    /// Builds a GoogleCredential from a secret containing JSON credentials.
    /// </summary>
    /// <param name="credentialsSecretName">
    /// The name of the secret containing the service account JSON.
    /// </param>
    /// <returns>GoogleCredential built from the JSON credentials.</returns>
    public GoogleCredential BuildCredentialFromSecret(string credentialsSecretName)
    {
        var secretVersionName = new SecretVersionName(ProjectId, credentialsSecretName, "latest");
        var secret = Client.AccessSecretVersion(secretVersionName);
        var credentialsJson = secret.Payload.Data.ToStringUtf8();
        return GoogleCredential.FromJson(credentialsJson);
    }

    /// <summary>
    /// Creates a new instance of GoogleSecretManagerConfigurationProvider with credentials from a secret.
    /// </summary>
    /// <param name="projectId">The Google Cloud project ID.</param>
    /// <param name="credentialsSecretName">The name of the secret containing the service account JSON.</param>
    /// <returns>A new instance with the specified credentials.</returns>
    public static GoogleSecretManagerConfigurationProvider CreateWithCredentialsFromSecret(string projectId, string credentialsSecretName)
    {
        var provider = new GoogleSecretManagerConfigurationProvider(projectId);
        var credential = provider.BuildCredentialFromSecret(credentialsSecretName);
        return new GoogleSecretManagerConfigurationProvider(projectId, credential);
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