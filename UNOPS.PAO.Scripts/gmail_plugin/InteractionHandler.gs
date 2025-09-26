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

function getMappedInteractionData(messageData, relatedRecords = null) {
  // Log the incoming data for debugging
    
    Logger.log('Message Data Get Mapped Interaction Data: ' + JSON.stringify(messageData));
    const threadId = messageData.threadId;
    const messageId = messageData.messageId;

    // Extract all email addresses
    const allEmails = [
      extractEmailAddress(messageData.sender), // Extract email from sender
      ...extractEmailAddresses(messageData.to),
      ...extractEmailAddresses(messageData.cc),
      ...extractEmailAddresses(messageData.bcc)
    ].filter(email => email); // Remove null/undefined

    const uniqueEmails = removeDuplicatesUsingSet(allEmails);

    Logger.log('All extracted emails: ' + JSON.stringify(uniqueEmails));

    // Extract IDs from related records if available
    const contactIds = relatedRecords?.contacts?.map(contact => contact.id) || [];
    const partnerIds = relatedRecords?.partners?.map(partner => partner.id) || [];
    const userIds = relatedRecords?.users?.map(user => user.id) || [];

    Logger.log('Extracted Contact IDs: ' + JSON.stringify(contactIds));
    Logger.log('Extracted Partner IDs: ' + JSON.stringify(partnerIds));
    Logger.log('Extracted User IDs: ' + JSON.stringify(userIds));

    // Extract email data
    const interactionData = {
      Type: 'Email',
      Date: new Date(messageData.date).toISOString(), // Convert to ISO format
      Subject: messageData.subject,
      Description: messageData.currentMessageBody,
      EmailAddresses: uniqueEmails,
      //ContactId: contactIds.length > 0 ? contactIds[0] : 0, // Use first contact as primary contact
      ContactIds: contactIds,
      PartnerIds: partnerIds,
      UserIds: userIds,
      Location: 'Email',
      GmailThreadId: threadId,
      GmailMessageId: messageId,
      ConfirmDuplicateCreation: false
    };
    Logger.log('Final interaction data: ' + JSON.stringify(interactionData));
    return interactionData;
}

/**
 * Creates an Interaction based on email data
 * @param {Object} e - The event object containing parameters
 * @returns {Object} The created Interaction
 */
function createInteractionClicked(e) {
  try {
    // Check if interaction already exists
    //const existingInteraction = findExistingInteraction(threadId);
    const messageData = JSON.parse(e.parameters.messageData);
    const relatedRecords = JSON.parse(e.parameters.relatedRecords);
    
    Logger.log('Using passed related records: ' + JSON.stringify(relatedRecords));
    
    // Create the interaction data with the IDs from passed related records
    const interactionData = getMappedInteractionData(messageData, relatedRecords);

    // Create new interaction
    const createdInteraction = createInteraction(interactionData);
    
    // Update messageData with the newly created interaction to prevent duplicates
    messageData.existingInteraction = createdInteraction;
    
    Logger.log('Created interaction: ' + JSON.stringify(createdInteraction));
    
    // Rebuild the card with updated messageData
    const updatedCard = buildOpportunityPlusCard(relatedRecords, messageData);
    return updatedCard;
  } catch (error) {
    Logger.log('Error creating/updating interaction: ' + error);
    throw error;
  }
}

/**
 * Finds an existing interaction by source ID
 * @param {string} threadId - The email thread ID
 * @param {string} messageId - The email message ID
 * @returns {Object|null} The existing interaction or null
 */
function findExistingInteraction(threadId, messageId) {
  try {

    const findRequestData = {
      GmailThreadId: threadId,
      GmailMessageId: messageId
    };

    const response = UrlFetchApp.fetch(`${INTERACTION_API_ENDPOINT}/find`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${getAccessToken()}`,
        'Content-Type': 'application/json'
      },
      payload: JSON.stringify(findRequestData)
    });

    return JSON.parse(response.getContentText());
  } catch (error) {
    Logger.log('Error finding existing interaction: ' + error);
    return null;
  }
}

/**
 * Finds an existing related records
 * @param {string[]} EmailAddresses - The list of email addresses
 * @returns {Object|null} The existing related records or null
 */
function findRelatedRecords(emailAddresses) {
  try {
    const findRequestData = {
      EmailAddresses: emailAddresses
    };
    Logger.log('INTERACTION_API_ENDPOINT: ' + INTERACTION_API_ENDPOINT);
    const response = UrlFetchApp.fetch(`${INTERACTION_API_ENDPOINT}/find-related-records`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${getAccessToken()}`,
        'Content-Type': 'application/json'
      },
      payload: JSON.stringify(findRequestData)
    });

    return JSON.parse(response.getContentText());
  } catch (error) {
    Logger.log('Error finding related records: ' + error);
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

function handleCreateContact(emailAddress) {
  Logger.log(emailAddress);
}