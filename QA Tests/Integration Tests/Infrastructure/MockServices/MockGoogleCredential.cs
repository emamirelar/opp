using Google.Apis.Auth.OAuth2;

namespace UNOPS.PAO.IntegrationTests.Infrastructure.MockServices;

/// <summary>
/// Provides a mock Google Credential for testing without actual GCP access
/// </summary>
public static class MockGoogleCredential
{
    /// <summary>
    /// Creates a fake Google Credential for testing purposes
    /// </summary>
    public static GoogleCredential Create()
    {
        // Create a minimal fake credential that won't make actual API calls
        // This uses an access token credential which doesn't require real GCP setup
        return GoogleCredential.FromAccessToken("fake-access-token-for-testing");
    }
}

