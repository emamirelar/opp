const IS_TEST_DEPLOYMENT = false;

/**
 * Returns the base URL based on the current deployment environment.
 * @returns {string} The base URL.
 */
function getBaseUrl() {
  const propertiesService = PropertiesService.getScriptProperties()
  if(IS_TEST_DEPLOYMENT) {
    baseUrl = propertiesService.getProperty('DEV_OPPORTUNITY_PLUS_BASEURL');
  }
  else {
    baseUrl = propertiesService.getProperty('OPPORTUNITY_PLUS_BASEURL');
  }
  Logger.log("baseUrl: " + baseUrl);
  return baseUrl;
}

/**
 * Returns the base API URL based on the current deployment environment.
 * @returns {string} The base API URL.
 */
function getApiBaseUrl() {
  return getBaseUrl() + '/api';
}

const API_BASE_URL = getApiBaseUrl();
const BASE_URL = getBaseUrl();
//const DEV_BASE_URL = 'https://swift-legible-raven.ngrok-free.app';
const USER_CLAIMS_ENDPOINT = `${BASE_URL}/user/claims`;
const AUTH_ENDPOINT = `${API_BASE_URL}/gmail-addon/auth`;
const CONTACT_ENDPOINT = `${API_BASE_URL}/api/contact`;
const INTERACTION_API_ENDPOINT = `${API_BASE_URL}/gmail-addon/interactions`;
const CREATE_RECORDS_ENDPOINT = `${API_BASE_URL}/gmail-addon/create-records`;
const OPPORTUNITY_PLUS_ENDPOINT = `https://localhost:44426/#`;
const ICON_URL = 'https://storage.googleapis.com/opp_plus_logo/Opportunity%20Logo%20Graphic1000px.png';
const CONTACT_READ_ERROR_MSG = 'Insufficient permission to view';
const PARTNER_READ_ERROR_MSG = 'Insufficient permission to view';
const USER_READ_ERROR_MSG = 'Insufficient permission to view';
const RELATED_RECORDS_ERROR_MSG = 'There was an error retrieving the data';
const EMPTY_MSG = '';