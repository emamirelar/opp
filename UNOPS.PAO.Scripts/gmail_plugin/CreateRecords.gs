function createContactsFromSelectedEmails(selectedEmails, messageData) {
  try {
    var accessToken = getAccessToken();
    
    var payload = {
      selectedContacts: selectedEmails,
      gmailThreadId: messageData.threadId,
      gmailMessageId: messageData.messageId
    };
    
    Logger.log('Sending payload to backend: ' + JSON.stringify(payload));
    
    var response = UrlFetchApp.fetch(CREATE_RECORDS_ENDPOINT, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + accessToken
      },
      payload: JSON.stringify(payload)
    });
    
    var responseCode = response.getResponseCode();
    var responseText = response.getContentText();
    
    Logger.log('Response code: ' + responseCode);
    Logger.log('Response text: ' + responseText);
    
    if (responseCode === 200) {
      var result = JSON.parse(responseText);
      
      // Success - refresh the UI by rebuilding the card with updated data
      return refreshRelatedRecords(messageData);
      
    } else {
      return CardService.newActionResponseBuilder()
        .setNotification(CardService.newNotification()
          .setText('Failed to create contacts. Please try again.'))
        .build();
    }
    
  } catch (error) {
    Logger.log('Error creating contacts: ' + error.toString());
    return CardService.newActionResponseBuilder()
      .setNotification(CardService.newNotification()
        .setText('Error creating contacts: ' + error.toString()))
      .build();
  }
}

function refreshRelatedRecords(messageData) {
  try {
    var accessToken = getAccessToken();
    
    // Re-fetch related records to get updated data
    var emailAddresses = [
      extractEmailAddress(messageData.sender),
      ...extractEmailAddresses(messageData.to),
      ...extractEmailAddresses(messageData.cc),
      ...extractEmailAddresses(messageData.bcc)
    ].filter(email => email);
    
    var uniqueEmails = removeDuplicatesUsingSet(emailAddresses);
    
    var payload = {
      emailAddresses: uniqueEmails
    };
    
    var response = UrlFetchApp.fetch(`${INTERACTION_API_ENDPOINT}/find-related-records`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + accessToken
      },
      payload: JSON.stringify(payload)
    });
    
    if (response.getResponseCode() === 200) {
      var updatedRelatedRecords = JSON.parse(response.getContentText());
      
      // Rebuild the card with fresh data
      var refreshedCard = buildOpportunityPlusCard(updatedRelatedRecords, messageData, null);
      
      return CardService.newActionResponseBuilder()
        .setNotification(CardService.newNotification()
          .setText('Action completed successfully!'))
        .setNavigation(CardService.newNavigation().updateCard(refreshedCard))
        .build();
    } else {
      return CardService.newActionResponseBuilder()
        .setNotification(CardService.newNotification()
          .setText('Action completed, but failed to refresh. Please reload.'))
        .build();
    }
    
  } catch (error) {
    Logger.log('Error refreshing data: ' + error.toString());
    return CardService.newActionResponseBuilder()
      .setNotification(CardService.newNotification()
        .setText('Action completed, but failed to refresh: ' + error.toString()))
      .build();
  }
}