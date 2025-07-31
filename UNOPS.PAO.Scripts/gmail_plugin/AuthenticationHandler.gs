/**
 * Gets the access token for API authentication
 * @returns {string} The access token
 */
function getAccessToken() {
  const scriptProperties = PropertiesService.getScriptProperties();
  const accessToken = scriptProperties.getProperty('accessToken');
  const refreshToken = scriptProperties.getProperty('refreshToken');
  const expiresAt = scriptProperties.getProperty('expiresAt');

  // If we have a valid access token, return it
  if (accessToken && expiresAt && new Date(expiresAt) > new Date()) {
    return accessToken;
  }

  // If we have a refresh token, use it to get a new access token
  //TO-DO: uncomment after refresh token logic is implemented on backend
  /*if (refreshToken) {
    try {
      const response = UrlFetchApp.fetch(`${API_BASE_URL}/api/auth/refresh`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        payload: JSON.stringify({ refreshToken: refreshToken })
      });

      const result = JSON.parse(response.getContentText());
      
      // Store the new tokens
      scriptProperties.setProperty('accessToken', result.accessToken);
      scriptProperties.setProperty('refreshToken', result.refreshToken);
      scriptProperties.setProperty('expiresAt', result.expiresAt);

      return result.accessToken;
    } catch (error) {
      Logger.log('Error refreshing token: ' + error);
      // If refresh fails, we need to re-authenticate
      return authenticate();
    }
  }*/

  // If we don't have any tokens, we need to authenticate
  return authenticate();
}

function getIAPToken() {
    const propertiesService = PropertiesService.getScriptProperties()
    const apiKey = propertiesService.getProperty('IDENTITY_TOOLKIT_API_KEY')
    const hostName = propertiesService.getProperty('OPPORTUNITY_PLUS_HOSTNAME')
    const res = UrlFetchApp.fetch('https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key='+apiKey, {
        method: 'POST',
        payload: JSON.stringify({
            // TODO: Should likely have the base url as propery
            requestUri: 'https://'+hostName,
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
 * Authenticates the user and gets initial tokens
 * @returns {string} The access token
 */
function authenticate() {
  try {
    // Get the current user's email from Google Apps Script
    const userEmail = Session.getActiveUser().getEmail();
    const clientId = "75832219314-oreep87dp8vssaseg7j8kvrusreqd70e.apps.googleusercontent.com";

    const idToken = getIAPToken(); 
    Logger.log('idToken: ' + idToken);

    if (!idToken) {
      throw new Error("Could not obtain Google ID Token.");
    }
    
    Logger.log('AUTH_ENDPOINT: ' + AUTH_ENDPOINT);
    // For IAP-protected endpoints, send token in Authorization header
    const response = UrlFetchApp.fetch(AUTH_ENDPOINT, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${idToken}`,
        'X-Client-ID': clientId
      },
      payload: JSON.stringify({
        provider: 'UNOPS.PAO',
        idToken: idToken
      }),
      muteHttpExceptions: true
    });

    Logger.log('response: ' + response);
    const result = JSON.parse(response.getContentText());
    
    // Store the tokens
    const scriptProperties = PropertiesService.getScriptProperties();
    scriptProperties.setProperty('accessToken', result.accessToken);
    scriptProperties.setProperty('refreshToken', result.refreshToken);
    scriptProperties.setProperty('expiresAt', result.expiresAt);

    return result.accessToken;
  } catch (error) {
    Logger.log('Error authenticating: ' + error);
    throw new Error('Authentication failed: ' + error.message);
  }
}