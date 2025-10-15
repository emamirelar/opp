/**
 * Gets the access token for API authentication
 * Optimized flow: Returns IAP token directly without separate authentication step
 * @returns {string} The IAP token for authentication
 */
function getAccessToken() {
  try {
    // Get IAP token directly - no need for separate authentication endpoint
    const idToken = getIAPToken();
    
    if (!idToken) {
      throw new Error("Could not obtain Google IAP Token.");
    }
    
    Logger.log('Using IAP token for authentication');
    return idToken;
  } catch (error) {
    Logger.log('Error getting IAP token: ' + error);
    throw new Error('Failed to get IAP authentication token: ' + error.message);
  }
}

/**
 * Exchanges a Google ID token for a GCIP token with tenant information
 * 
 * AUTHENTICATION FLOW:
 * 1. Get Google ID token from Apps Script execution context (proves who we are)
 * 2. Exchange it with Identity Toolkit API for a GCIP token (adds tenant context)
 * 3. Return GCIP token that IAP can validate
 * 
 * WHY THIS IS NECESSARY:
 * - IAP is configured to use Identity Platform with multi-tenancy
 * - IAP expects tokens from the "Personnel-ylvvz" tenant specifically
 * - Standard Google ID tokens don't include tenant information
 * - The Identity Toolkit API adds tenant context to our token
 * 
 * CRITICAL: We use ID tokens (identity proof), NOT access tokens (API authorization)
 * - ID tokens contain identity claims (who you are)
 * - Access tokens contain permission scopes (what you can do)
 * - Identity Platform expects identity for authentication, not authorization
 * 
 * @returns {string} GCIP ID token with tenant information for IAP authentication
 */
function getIAPToken() {
    const propertiesService = PropertiesService.getScriptProperties()
    
    // API key for Identity Toolkit - authenticates OUR request to the Identity Platform API
    const apiKey = propertiesService.getProperty('IDENTITY_TOOLKIT_API_KEY')
    
    // Get Google ID token representing the service account running this script
    // This is an identity token (who we are), not an access token (what we can access)
    const googleIdToken = ScriptApp.getIdentityToken();
    
    // Exchange our Google ID token for a GCIP token with tenant information
    // The Identity Toolkit API validates our Google token and issues a new token
    // that includes the tenant ID, which IAP requires
    const res = UrlFetchApp.fetch('https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key='+apiKey, {
        method: 'POST',
        payload: JSON.stringify({
            // Base URL for redirect (required by API but not actually used in server-to-server flow)
            requestUri: getBaseUrl(),
            
            // CRITICAL: Use 'id_token=' not 'access_token='
            // id_token = identity proof (who we are) - CORRECT for authentication
            // access_token = authorization grant (what we can do) - WRONG, causes cross-project errors
            postBody: 'id_token=' + googleIdToken + '&providerId=google.com',
            
            // Request a secure token in the response
            returnSecureToken: true,
            
            // Request identity provider credentials
            returnIdpCredential: true,
            
            // CRITICAL: Specify the tenant ID
            // Our IAP is configured to only accept tokens from the "Personnel-ylvvz" tenant
            // Without this, we get a token from the default tenant which IAP rejects
            tenantId: 'Personnel-ylvvz'
        }),
        contentType: 'application/json',
        
        // Don't throw exceptions on HTTP errors - we'll handle them ourselves
        muteHttpExceptions: true
    })
    
    // Check if the API call succeeded
    if (res.getResponseCode() !== 200) {
        Logger.log('Identity Toolkit API error: ' + res.getContentText());
        throw new Error('Failed to exchange token: ' + res.getContentText());
    }
    
    // Parse the response to extract the GCIP token
    const responseData = JSON.parse(res.getContentText())
    
    // The idToken in the response is now a GCIP token with:
    // - Issuer: securetoken.google.com/unops-identity-platform-dev
    // - Audience: unops-identity-platform-dev  
    // - Tenant: Personnel-ylvvz
    // This is what IAP expects and will validate successfully
    const gcipToken = responseData?.idToken
    
    if (!gcipToken) {
        throw new Error('No idToken in response from Identity Toolkit');
    }
    
    return gcipToken
}

function TestIAPAuthWithScriptToken() {
  const idToken = ScriptApp.getIdentityToken();
  Logger.log('idToken: ' + idToken);
  const body = idToken.split('.')[1];
  const decoded = Utilities.newBlob(Utilities.base64Decode(body)).getDataAsString();
  const payload = JSON.parse(decoded);
  Logger.log(JSON.stringify(payload, null, 2))
  if (!idToken) {
    throw new Error("Could not obtain Google ID Token.");
  }
  const url=USER_CLAIMS_ENDPOINT
  // For IAP-protected endpoints, send token in Authorization header
  const response = UrlFetchApp.fetch(url, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${idToken}`,
    },
    muteHttpExceptions: true
  });

  Logger.log('response: ' + response);
}

function TestIAPAuth() {
  const idToken = getIAPToken();
  Logger.log('idToken: ' + idToken);
  const body = idToken.split('.')[1];
  const decoded = Utilities.newBlob(Utilities.base64Decode(body)).getDataAsString();
  const payload = JSON.parse(decoded);
  Logger.log(JSON.stringify(payload, null, 2))
  if (!idToken) {
    throw new Error("Could not obtain Google ID Token.");
  }
  const url=USER_CLAIMS_ENDPOINT
  // For IAP-protected endpoints, send token in Authorization header
  const response = UrlFetchApp.fetch(url, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${idToken}`,
    },
    muteHttpExceptions: true
  });

  Logger.log('response: ' + response);
}

/**
 * Legacy authenticate function - no longer needed with optimized IAP flow
 * Kept for backward compatibility but now just returns IAP token
 * @returns {string} The IAP token
 * @deprecated Use getAccessToken() directly instead
 */
function authenticate() {
  Logger.log('authenticate() called - redirecting to optimized getAccessToken()');
  return getAccessToken();
}