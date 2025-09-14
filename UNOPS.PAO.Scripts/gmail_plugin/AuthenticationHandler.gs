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

function getIAPToken() {
    const propertiesService = PropertiesService.getScriptProperties()
    const apiKey = propertiesService.getProperty('IDENTITY_TOOLKIT_API_KEY')
    const res = UrlFetchApp.fetch('https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key='+apiKey, {
        method: 'POST',
        payload: JSON.stringify({
            // TODO: Should likely have the base url as propery
            requestUri: getBaseUrl(),
            postBody: 'access_token='+ScriptApp.getOAuthToken()+'&providerId=google.com',
            returnSecureToken: true,
            returnIdpCredential: true
        }),
        contentType: 'application/json',
        muteHttpExceptions: true
    })
    const responseData = JSON.parse(res)
    const idToken = responseData?.idToken

    return idToken
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
  const url="https://test-opportunityplus.unops.org/user/claims"
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
  const url="https://test-opportunityplus.unops.org/user/claims"
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