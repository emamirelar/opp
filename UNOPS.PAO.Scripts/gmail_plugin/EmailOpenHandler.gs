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
  
  const existingInteraction = findExistingInteraction(messageData.threadId);
  messageData.existingInteraction = existingInteraction;

  // Extract all email addresses
  const allEmails = [
      extractEmailAddress(messageData.sender), // Extract email from sender
      ...extractEmailAddresses(messageData.to),
      ...extractEmailAddresses(messageData.cc),
      ...extractEmailAddresses(messageData.bcc)
    ].filter(email => email); // Remove null/undefined

  const uniqueEmails = removeDuplicatesUsingSet(allEmails);

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

        Logger.log("Fallback attachments count: " + attachments.length);

        if (attachments.length > 0) {
          attachments.forEach((att, index) => {
            const name = att.getName();
            Logger.log("name = "+name);
            if (name) {
              attachmentNames.push(`${attachmentNames.length + 1}. ${name}`);
            }
          });
        }
        Logger.log(`Message #${i + 1} - Attachments found: ${attachments.length}`);

        const rawBody = msg.getPlainBody();
        const cleanedBody = cleanEmailBody(rawBody);

        // Build conversation content
        const sender = msg.getFrom();
        const date = Utilities.formatDate(msg.getDate(), Session.getScriptTimeZone(), 'dd-MMM-yyyy HH:mm');
        const subject = msg.getSubject();
        const body = msg.getPlainBody(); 
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
      messageId: messageId
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