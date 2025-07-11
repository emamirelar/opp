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

/**
 * Authenticates the user and gets initial tokens
 * @returns {string} The access token
 */
function authenticate() {
  try {
    // Get the current user's email from Google Apps Script
    const userEmail = Session.getActiveUser().getEmail();

    const idToken = ScriptApp.getIdentityToken(); 
    Logger.log('idToken: ' + idToken);

    if (!idToken) {
      throw new Error("Could not obtain Google ID Token.");
    }
    
    // For IAP-protected endpoints, send token in Authorization header
    const response = UrlFetchApp.fetch(AUTH_ENDPOINT, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${idToken}`,  // Add this for IAP
        'X-Goog-IAP-JWT-Assertion': idToken    // Alternative IAP header
      },
      payload: JSON.stringify({
        provider: 'UNOPS.PAO',
        idToken: idToken
      })
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