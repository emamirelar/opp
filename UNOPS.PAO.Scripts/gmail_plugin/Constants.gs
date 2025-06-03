const homePageMsg = "<font-family='Open sans normal'> 1. Open an email in Gmail. \n2. The extracted email contents can be seen in the add-on.</font>";


// API Endpoints
const API_BASE_URL = 'https://swift-legible-raven.ngrok-free.app/api'; //temp url
const AUTH_ENDPOINT = `${API_BASE_URL}/auth/google`;
const CONTACT_ENDPOINT = `${API_BASE_URL}/api/contact`;
const INTERACTION_API_ENDPOINT = `${API_BASE_URL}/gmail-addon/interactions`;
const OPPORTUNITY_PLUS_ENDPOINT = `https://localhost:44426/`;
  
// Manifest document : https://developers.google.com/apps-script/manifest
// https://developers.google.com/apps-script/manifest/allowlist-url
// https://developers.google.com/apps-script/reference/card-service
//oAuth2 Library Script Id = '1B7FSrk5Zi6L1rSxxTDgDEUsPzlukDsi4KGuTMorsTQHhGBzBkMun4iDF';
// Using Secret Manager API : Role Secret Manager Secret Accessor role.