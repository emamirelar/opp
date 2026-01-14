using Google.Apis.Auth.OAuth2;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UNOPS.PAO.DataAccess.Services;

/// <summary>
/// Provides IAM authentication for Cloud SQL connections.
/// Generates OAuth2 access tokens for use as database passwords.
/// </summary>
public static class CloudSqlIamAuthProvider
{
    private static GoogleCredential? _credential;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    
    /// <summary>
    /// Gets or sets whether IAM authentication is enabled.
    /// When false, returns null to use standard password authentication.
    /// </summary>
    public static bool IsEnabled { get; set; } = false;
    
    /// <summary>
    /// Password callback for Npgsql that provides OAuth2 access tokens for IAM authentication.
    /// </summary>
    /// <param name="host">Database host</param>
    /// <param name="port">Database port</param>
    /// <param name="database">Database name</param>
    /// <param name="username">Database username (IAM user email)</param>
    /// <returns>OAuth2 access token or null if IAM auth is disabled</returns>
    public static string? ProvidePassword(string host, int port, string database, string username)
    {
        if (!IsEnabled)
            return null;
            
        return GetAccessTokenAsync().GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Async password callback for Npgsql.
    /// </summary>
    public static async ValueTask<string?> ProvidePasswordAsync(string host, int port, string database, string username, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
            return null;
            
        return await GetAccessTokenAsync();
    }
    
    /// <summary>
    /// Gets a fresh OAuth2 access token using Application Default Credentials.
    /// </summary>
    private static async Task<string> GetAccessTokenAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            // Get Application Default Credentials (uses gcloud auth for local development)
            _credential ??= await GoogleCredential.GetApplicationDefaultAsync();
            
            // Scope required for Cloud SQL IAM authentication
            var scopedCredential = _credential.CreateScoped("https://www.googleapis.com/auth/sqlservice.login");
            
            // Get access token
            var token = await scopedCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            
            return token;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    /// <summary>
    /// Clears cached credentials. Useful for testing or credential rotation.
    /// </summary>
    public static void ClearCredentials()
    {
        _credential = null;
    }
}

