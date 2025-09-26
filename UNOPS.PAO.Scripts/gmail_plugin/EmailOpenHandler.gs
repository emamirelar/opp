/**
 * - Fetching Gmail message & thread data
 * - Extracting attachments and formatting message content
 * - Rendering the Gmail add-on card UI with navigation and interactivity
 */

function onGmailMessageOpen(e) {
  const userProps = PropertiesService.getUserProperties();
  const accessToken = e.gmail.accessToken;

  GmailApp.setCurrentMessageAccessToken(accessToken);

  userProps.setProperty('originalE', JSON.stringify(e));
  const currentPage = parseInt(userProps.getProperty('currentPage')) || 1;

  const messageData = getMessageData(e);
  
  const existingInteraction = findExistingInteraction(messageData.threadId, messageData.messageId);
  messageData.existingInteraction = existingInteraction;

  // Extract all email addresses with their full information and parse names
  const emailsWithNames = extractEmailsWithNamesFromMessage(messageData);
  const uniqueEmails = removeDuplicatesUsingSet(emailsWithNames.map(item => item.email));

  // Store the parsed name information in messageData for later use
  messageData.parsedEmailNames = emailsWithNames;

  const relatedRecords = findRelatedRecords(uniqueEmails);

  Logger.log('Message Data Content Form: ' + JSON.stringify(messageData));

  const relatedRecordsCard = buildOpportunityPlusCard(relatedRecords, messageData, null);
  return relatedRecordsCard;

  //const card = buildMessageCard(messageData, currentPage);
  //return [card];
}

function getMessageData(eventObj) {
  try {
    const messageId = eventObj.gmail.messageId;
    const threadId = eventObj.gmail.threadId;

    const message = GmailApp.getMessageById(messageId);
    const thread = GmailApp.getThreadById(threadId);
    const threadMessages = thread.getMessages();
    const currentMessageBody = cleanEmailBody(message.getBody());

    let fullConversationContent = "";
    let attachmentNames = [];

    for (let i = 0; i < threadMessages.length; i++) {
      try {
        const msg = threadMessages[i];
        if (msg.isDraft()) continue; // skip drafts if needed
        
        // Get attachments
        const attachments = msg.getAttachments({
          includeInlineImages: false,
          includeAttachments: true
        });

        if (attachments.length > 0) {
          attachments.forEach((att, index) => {
            const name = att.getName();
            Logger.log("name = "+name);
            if (name) {
              attachmentNames.push(`${attachmentNames.length + 1}. ${name}`);
            }
          });
        }

        const rawBody = msg.getBody();
        const cleanedBody = cleanEmailBody(rawBody);

        // Build conversation content
        const sender = msg.getFrom();
        const date = Utilities.formatDate(msg.getDate(), Session.getScriptTimeZone(), 'dd-MMM-yyyy HH:mm');
        const subject = msg.getSubject();
        const body = msg.getBody(); 
        const to = msg.getTo();
        const cc = msg.getCc();
        const bcc = msg.getBcc();

        /*fullConversationContent += `<b>From:</b> ${sender}<br><b>To:</b> ${to}<br><b>cc:</b> ${cc}<br><b>bcc:</b> ${bcc}<br><b>Date:</b> ${date}<br><b>Subject:</b> ${subject}<br><br>${body.replace(/\n/g, '<br>')}<br><hr><br>`;*/

        if(i == 0) {
          fullConversationContent += `${cleanedBody}\n------------------`;
        }
        else {
          fullConversationContent += `\n\n${cleanedBody}\n------------------`;
        }

      } catch (err) {
        console.error(`Error processing message #${i}: `, err);
      }
    }

    // const firstMessage = threadMessages[0];

    return {
      sender: threadMessages[0].getFrom(),
      subject: threadMessages[0].getSubject(),
      to: threadMessages[0].getTo(),
      cc: threadMessages[0].getCc() || "",
      bcc: threadMessages[0].getBcc() || "",
      date: Utilities.formatDate(threadMessages[0].getDate(), Session.getScriptTimeZone(), 'dd-MMM-yyyy HH:mm'),
      body: fullConversationContent,
      attachments: attachmentNames.length > 0 ? attachmentNames : ["None"],
      threadId: threadId,
      messageId: messageId,
      currentMessageBody: currentMessageBody
    };

  } catch (error) {
    Logger.log("Error retrieving Gmail data: " + error);
    return {
      sender: "Unavailable",
      subject: "Error retrieving message",
      to: "",
      cc: "",
      bcc: "",
      date: "",
      body: "Could not load message content.",
      attachments: ["None"]
    };
  }
}

/**
 * Cleans an email body by removing common quoted reply patterns.
 * This is a heuristic and might not catch all variations.
 * @param {string} body The raw plain text email body.
 * @returns {string} The cleaned email body.
 */
function cleanEmailBody(body) {
  let cleaned = body;

  // 1. Remove lines starting with ">" (common for quoted text)
  // This needs to be done carefully to preserve actual blockquotes if used by sender.
  // For typical email replies, this is effective.
  cleaned = cleaned.replace(/^>.*(?:\n>.*)*\n?/gm, '');

  // 2. Remove standard reply headers (e.g., "--- Original Message ---", "On [Date], [Sender] wrote:")
  // Common patterns for quoted replies
  const replyHeaderPatterns = [
    /^\s*On\s+.*,\s+.*<.+>\s+wrote:\s*$/im,
    /^\s*Le\s+\w+\.\s+\d{1,2}\s+\w{3}\.\s+\d{4}\s+à\s+\d{2}:\d{2},\s+.*a\s+écrit\s*:\s*$/im, //TO-DO: not working for French. need to handle other languages as well.
    /^\s*From:\s*.*$/im,
    /^\s*Sent:\s*.*$/im,
    /^\s*To:\s*.*$/im,
    /^\s*Cc:\s*.*$/im,
    /^\s*Subject:\s*.*$/im,
    /^\s*---+\s*Original Message\s*---+$/im, // "--- Original Message ---"
    /^\s*-----Original Message-----$/im, // "-----Original Message-----"
    /^\s*\[Quoted text hidden\]\s*$/im,
    /^\s*Begin forwarded message:\s*$/im
  ];

  for (const pattern of replyHeaderPatterns) {
    cleaned = cleaned.replace(pattern, '');
  }

  // 3. Remove excess newlines that might result from removal
  cleaned = cleaned.replace(/\n\s*\n\s*\n/g, '\n\n'); // Reduce multiple newlines to just two
  cleaned = cleaned.trim(); // Trim leading/trailing whitespace

  return cleaned;
}

function removeDuplicatesUsingSet(originalList) {
  return [...new Set(originalList)];
}

/**
 * Extracts emails with parsed name components from message data
 * @param {Object} messageData - The message data object containing sender, to, cc, bcc fields
 * @returns {Array} Array of objects with {email, firstName, middleName, lastName, originalString}
 */
function extractEmailsWithNamesFromMessage(messageData) {
  const emailsWithNames = [];
  const processedEmails = new Set(); // To avoid duplicates
  
  // Helper function to process a single email string
  function processEmailString(emailString, source) {
    if (!emailString || !emailString.trim()) return;
    
    const cleanEmailString = emailString.trim();
    const extractedEmail = extractEmailAddress(cleanEmailString);
    
    if (extractedEmail && !processedEmails.has(extractedEmail.toLowerCase())) {
      processedEmails.add(extractedEmail.toLowerCase());
      
      const nameComponents = extractNameComponents(cleanEmailString);
      
      emailsWithNames.push({
        email: extractedEmail,
        firstName: nameComponents.firstName || '',
        middleName: nameComponents.middleName || '',
        lastName: nameComponents.lastName || '',
        originalString: cleanEmailString,
        source: source // Track where this email came from (sender, to, cc, bcc)
      });
    }
  }
  
  // Process sender
  if (messageData.sender) {
    processEmailString(messageData.sender, 'sender');
  }
  
  // Process 'to' field
  if (messageData.to) {
    const toEmails = messageData.to.split(',');
    toEmails.forEach(email => processEmailString(email, 'to'));
  }
  
  // Process 'cc' field  
  if (messageData.cc) {
    const ccEmails = messageData.cc.split(',');
    ccEmails.forEach(email => processEmailString(email, 'cc'));
  }
  
  // Process 'bcc' field
  if (messageData.bcc) {
    const bccEmails = messageData.bcc.split(',');
    bccEmails.forEach(email => processEmailString(email, 'bcc'));
  }
  
  Logger.log('Extracted emails with names: ' + JSON.stringify(emailsWithNames));
  
  return emailsWithNames;
}

/**
 * Extracts name from email string and splits it into name components
 * @param {string} emailString - String like "John Doe <john.doe@example.com>" or just "john.doe@example.com"
 * @returns {Object} Object with firstName, middleName, lastName properties
 */
function extractNameComponents(emailString) {
  if (!emailString) {
    return { firstName: '', middleName: '', lastName: '' };
  }
  
  let nameString = '';
  
  // Check if email is in format "Name <email@domain.com>"
  const angleBracketMatch = emailString.match(/^(.+?)\s*<[^>]+>$/);
  if (angleBracketMatch) {
    nameString = angleBracketMatch[1].trim();
  } else {
    // If no angle brackets, try to extract name from email prefix
    const emailMatch = emailString.match(/[\w.+-]+@[\w-]+\.[\w.-]+/);
    if (emailMatch) {
      const emailPrefix = emailMatch[0].split('@')[0];
      // Replace common separators with spaces and capitalize
      nameString = emailPrefix.replace(/[._-]/g, ' ')
                              .split(' ')
                              .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
                              .join(' ');
    } else {
      nameString = emailString.trim();
    }
  }
  
  // Clean up the name string (remove quotes, extra spaces)
  nameString = nameString.replace(/['"]/g, '').replace(/\s+/g, ' ').trim();
  
  // Split name into components
  const nameParts = nameString.split(' ').filter(part => part.length > 0);
  
  let firstName = '';
  let middleName = '';
  let lastName = '';
  
  if (nameParts.length === 1) {
    // If only one name part, use it as lastName (required field)
    lastName = nameParts[0];
  } else if (nameParts.length === 2) {
    firstName = nameParts[0];
    lastName = nameParts[1];
  } else if (nameParts.length >= 3) {
    firstName = nameParts[0];
    lastName = nameParts[nameParts.length - 1];
    middleName = nameParts.slice(1, -1).join(' ');
  }
  
  return { 
    firstName: firstName, 
    middleName: middleName, 
    lastName: lastName 
  };
}
