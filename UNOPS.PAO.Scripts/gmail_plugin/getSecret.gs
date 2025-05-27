/* 
Accessing Secret credentials via GCP Secret Manager
*/
function getSecret(secretName) {
  const projectId = 'unops-partneropportunity';
  const url = `https://secretmanager.googleapis.com/v1/projects/${projectId}/secrets/${secretName}/versions/latest:access`;

  const response = UrlFetchApp.fetch(url, {
    method: 'GET',
    muteHttpExceptions: true,
    headers: {
      Authorization: `Bearer ${ScriptApp.getOAuthToken()}`
    }
  });

  if (response.getResponseCode() === 200) {
    const json = JSON.parse(response.getContentText());
    const decoded = Utilities.base64Decode(json.payload.data);
    return Utilities.newBlob(decoded).getDataAsString();
  } else {
    Logger.log(`Failed to fetch secret: ${response.getContentText()}`);
    throw new Error('Unable to access secret');
  }
}

function accessSecret() {
  const secretContent = getSecret('QA_Gmail_Plugin_Secret');
  Logger.log(secretContent);
}
