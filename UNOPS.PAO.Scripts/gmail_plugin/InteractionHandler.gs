/**
 * Handles the creation and updating of Interactions based on email data
 */

// API endpoint for Interactions
const INTERACTION_API_ENDPOINT = `${API_BASE_URL}/gmail-addon/interactions`;

/**
 * Cleans the email body to remove headers and HTML formatting
 * @param {string} body - The raw email body
 * @param {string} subject - The email subject
 * @returns {string} Cleaned email body
 */
function cleanEmailBody(body, subject) {
  if (!body) return '';
  
  // Remove HTML tags
  let cleanBody = body.replace(/<[^>]*>/g, '');
  
  // Remove common email headers (case insensitive)
  const headersToRemove = [
    /From:.*?(?=\n|$)/i,
    /To:.*?(?=\n|$)/i,
    /cc:.*?(?=\n|$)/i,
    /bcc:.*?(?=\n|$)/i,
    /Date:.*?(?=\n|$)/i,
    /Subject:.*?(?=\n|$)/i,
    /Sent:.*?(?=\n|$)/i,
    /Received:.*?(?=\n|$)/i,
    /Reply-To:.*?(?=\n|$)/i,
    /Return-Path:.*?(?=\n|$)/i,
    /Delivered-To:.*?(?=\n|$)/i,
    /X-.*?(?=\n|$)/i,
    /MIME-Version:.*?(?=\n|$)/i,
    /Content-Type:.*?(?=\n|$)/i,
    /Content-Transfer-Encoding:.*?(?=\n|$)/i
  ];
  
  headersToRemove.forEach(header => {
    cleanBody = cleanBody.replace(header, '');
  });
  
  // Remove multiple newlines
  cleanBody = cleanBody.replace(/\n{3,}/g, '\n\n');
  
  // Remove leading/trailing whitespace
  cleanBody = cleanBody.trim();
  
  // Add subject at the beginning if not already present
  if (!cleanBody.includes(subject)) {
    cleanBody = `Subject: ${subject}\n\n${cleanBody}`;
  }
  
  return cleanBody;
}

/**
 * Extracts email address from a string that might contain full name
 * @param {string} emailString - String containing email address
 * @returns {string} Extracted email address
 */
function extractEmailAddress(emailString) {
  if (!emailString) return '';
  
  // First try to find email in angle brackets
  const angleBracketMatch = emailString.match(/<([^>]+)>/);
  if (angleBracketMatch) {
    return angleBracketMatch[1];
  }
  
  // If no angle brackets, try to find email directly
  const match = emailString.match(/[\w.+-]+@[\w-]+\.[\w.-]+/);
  return match ? match[0] : '';
}

/**
 * Extracts multiple email addresses from a string
 * @param {string} emailString - String containing multiple email addresses
 * @returns {Array} Array of extracted email addresses
 */
function extractEmailAddresses(emailString) {
  if (!emailString) return [];
  
  // Handle both string and array inputs
  if (Array.isArray(emailString)) {
    return emailString.map(email => extractEmailAddress(email)).filter(email => email);
  }
  
  // Handle comma-separated string
  const emails = emailString.split(',').map(email => email.trim());
  return emails.map(email => extractEmailAddress(email)).filter(email => email);
}

/**
 * Creates or updates an Interaction based on email data
 * @param {Object} messageData - The email message data
 * @returns {Object} The created/updated Interaction
 */
function createOrUpdateInteraction(messageData) {
  try {
    // Log the incoming data for debugging
    Logger.log('Message Data: ' + JSON.stringify(messageData));

    // Extract all email addresses
    const allEmails = [
      extractEmailAddress(messageData.sender), // Extract email from sender
      ...extractEmailAddresses(messageData.to),
      ...extractEmailAddresses(messageData.cc),
      ...extractEmailAddresses(messageData.bcc)
    ].filter(email => email); // Remove null/undefined

    Logger.log('All extracted emails: ' + JSON.stringify(allEmails));

    // Extract email data
    const interactionData = {
      Type: 'Email',
      Date: new Date(messageData.date).toISOString(), // Convert to ISO format
      Subject: messageData.subject,
      data: cleanEmailBody(messageData.body, messageData.subject),
      EmailAddresses: allEmails,
      ContactId: 0,
      Location: 'Email',
      /*documents: messageData.attachments?.filter(attachment => attachment && attachment.name)?.map(attachment => ({
        name: attachment.name,
        url: attachment.url,
        type: attachment.type
      })) || []*/
    };

    Logger.log('Final interaction data: ' + JSON.stringify(interactionData));

    // Check if interaction already exists
    const existingInteraction = findExistingInteraction(messageData.messageId);
    
    if (existingInteraction) {
      // Update existing interaction
      return updateInteraction(existingInteraction.id, interactionData);
    } else {
      // Create new interaction
      return createInteraction(interactionData);
    }
  } catch (error) {
    Logger.log('Error creating/updating interaction: ' + error);
    throw error;
  }
}

/**
 * Finds an existing interaction by source ID
 * @param {string} sourceId - The email message ID
 * @returns {Object|null} The existing interaction or null
 */
function findExistingInteraction(sourceId) {
  try {
    const response = UrlFetchApp.fetch(`${INTERACTION_API_ENDPOINT}?sourceId=${sourceId}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${getAccessToken()}`,
        'Content-Type': 'application/json'
      }
    });
    
    const result = JSON.parse(response.getContentText());
    return result.data && result.data.length > 0 ? result.data[0] : null;
  } catch (error) {
    Logger.log('Error finding existing interaction: ' + error);
    return null;
  }
}

/**
 * Creates a new interaction
 * @param {Object} interactionData - The interaction data
 * @returns {Object} The created interaction
 */
function createInteraction(interactionData) {
  try {
    const response = UrlFetchApp.fetch(INTERACTION_API_ENDPOINT, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${getAccessToken()}`,
        'Content-Type': 'application/json'
      },
      payload: JSON.stringify(interactionData)
    });
    
    return JSON.parse(response.getContentText());
  } catch (error) {
    Logger.log('Error creating interaction: ' + error);
    throw error;
  }
}

/**
 * Updates an existing interaction
 * @param {string} interactionId - The ID of the interaction to update
 * @param {Object} interactionData - The updated interaction data
 * @returns {Object} The updated interaction
 */
function updateInteraction(interactionId, interactionData) {
  try {
    const response = UrlFetchApp.fetch(`${INTERACTION_API_ENDPOINT}/${interactionId}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${getAccessToken()}`,
        'Content-Type': 'application/json'
      },
      payload: JSON.stringify(interactionData)
    });
    
    return JSON.parse(response.getContentText());
  } catch (error) {
    Logger.log('Error updating interaction: ' + error);
    throw error;
  }
}

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
  if (refreshToken) {
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
  }

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
    
    // Authenticate with our API using the user's email
    const response = UrlFetchApp.fetch(AUTH_ENDPOINT, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      payload: JSON.stringify({
        provider: 'UNOPS.PAO',
        email: userEmail
      })
    });

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

/**
 * Gets contact information by ID
 * @param {string} contactId - The ID of the contact to retrieve
 * @returns {Object} The contact information
 */
function getContactById(contactId) {
  try {
    const response = UrlFetchApp.fetch(`${CONTACT_ENDPOINT}/${contactId}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${getAccessToken()}`,
        'Content-Type': 'application/json'
      }
    });
    
    return JSON.parse(response.getContentText());
  } catch (error) {
    Logger.log('Error getting contact: ' + error);
    throw error;
  }
} 