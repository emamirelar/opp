const homePageMsg = "<font-family='Open sans normal'> 1. Open an email in Gmail. \n2. The extracted email contents can be seen in the add-on.</font>";

/**
 * Checks if the current script execution is from a development/test deployment.
 * @returns {boolean} True if in debug mode, false otherwise.
 */
function isDebugMode() {
  try {
    const scriptUrl = ScriptApp.getService().getUrl();
    if(scriptUrl != null) {
      return false;
    }
    //return true if debug mode. script url is null only in debug mode.
    return true;
  } catch (e) {
    Logger.log("isDebugMode: ScriptApp.getService().getUrl() returned null or threw an error: " + e.message);
    return true; // Default to debug mode if context is unclear/problematic
  }
}

/**
 * Returns the base URL based on the current deployment environment.
 * @returns {string} The base URL.
 */
function getBaseUrl() {
  if (isDebugMode()) {
      return 'https://swift-legible-raven.ngrok-free.app'; //temp url
  } else {
    const propertiesService = PropertiesService.getScriptProperties()
    const baseUrl = propertiesService.getProperty('OPPORTUNITY_PLUS_BASEURL');
    return baseUrl;
  }
}

/**
 * Returns the base API URL based on the current deployment environment.
 * @returns {string} The base API URL.
 */
function getApiBaseUrl() {
  return getBaseUrl() + '/api';
}

const API_BASE_URL = getApiBaseUrl();
//const API_BASE_URL = 'https://test-opportunityplus.unops.org/api';
const AUTH_ENDPOINT = `${API_BASE_URL}/gmail-addon/auth`;
const CONTACT_ENDPOINT = `${API_BASE_URL}/api/contact`;
const INTERACTION_API_ENDPOINT = `${API_BASE_URL}/gmail-addon/interactions`;
const CREATE_RECORDS_ENDPOINT = `${API_BASE_URL}/gmail-addon/create-records`;
const OPPORTUNITY_PLUS_ENDPOINT = `https://localhost:44426/#`;
const ICON_URL = 'https://storage.googleapis.com/opportunity_plus_logo/Opportunity%20Logo%20Graphic1000px.png';
const CONTACT_READ_ERROR_MSG = 'Insufficient permission to view';
const PARTNER_READ_ERROR_MSG = 'Insufficient permission to view';
const USER_READ_ERROR_MSG = 'Insufficient permission to view';
const RELATED_RECORDS_ERROR_MSG = 'There was an error retrieving the data';
const EMPTY_MSG = '';
  
// Manifest document : https://developers.google.com/apps-script/manifest
// https://developers.google.com/apps-script/manifest/allowlist-url
// https://developers.google.com/apps-script/reference/card-service
//oAuth2 Library Script Id = '1B7FSrk5Zi6L1rSxxTDgDEUsPzlukDsi4KGuTMorsTQHhGBzBkMun4iDF';
// Using Secret Manager API : Role Secret Manager Secret Accessor role.