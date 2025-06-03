/**
 * - Fetching Gmail message & thread data
 * - Extracting attachments and formatting message content
 * - Rendering the Gmail add-on card UI with navigation and interactivity
 */

function createCard(e) {
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

  const relatedRecords = findRelatedRecords(allEmails);

  const relatedRecordsCard = buildRelatedRecords(relatedRecords, messageData, false);
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

        // Build conversation content
        const sender = msg.getFrom();
        const date = Utilities.formatDate(msg.getDate(), Session.getScriptTimeZone(), 'dd-MMM-yyyy HH:mm');
        const subject = msg.getSubject();
        const body = msg.getPlainBody(); 
        const to = msg.getTo();
        const cc = msg.getCc();
        const bcc = msg.getBcc();

        /*fullConversationContent += `<b>From:</b> ${sender}<br><b>To:</b> ${to}<br><b>cc:</b> ${cc}<br><b>bcc:</b> ${bcc}<br><b>Date:</b> ${date}<br><b>Subject:</b> ${subject}<br><br>${body.replace(/\n/g, '<br>')}<br><hr><br>`;*/

        fullConversationContent += `${body.replace(/\n/g, '<br>')}<br><hr><br>`;

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

function buildMessageCard(data, currentPage) {
  const buttonText = data.existingInteraction ? "Update Interaction" : "Create Interaction";  
  const header = CardService.newCardHeader()
    .setTitle("📥 Extracted Email Content")
    .setSubtitle(`Page ${currentPage}`)
    .setImageStyle(CardService.ImageStyle.CIRCLE);

  const subjectWidget = CardService.newDecoratedText()
    .setTopLabel('Subject')
    .setText(data.subject)
    .setWrapText(true);

  const senderWidget = CardService.newDecoratedText()
    .setTopLabel('From')
    .setText(data.sender)
    .setWrapText(true);

  const toWidget = CardService.newDecoratedText()
    .setTopLabel('To')
    .setText(data.to)
    .setWrapText(true);

  const ccWidget = CardService.newDecoratedText()
    .setTopLabel('Cc')
    .setText(data.cc)
    .setWrapText(true);

  const bccWidget = CardService.newDecoratedText()
    .setTopLabel('Bcc')
    .setText(data.bcc)
    .setWrapText(true);

  const dateWidget = CardService.newDecoratedText()
    .setTopLabel('Received On')
    .setText(data.date);

  const attachmentWidget = CardService.newDecoratedText()
    .setTopLabel('Attachments')
    .setWrapText(true)
    .setText(data.attachments && data.attachments.length ? data.attachments.join('<br>') : "None");

  const bodyWidget = CardService.newDecoratedText()
    .setTopLabel('Full Conversation')
    .setText(data.body)
    .setWrapText(true);

  const createInteractionButton = CardService.newTextButton()
    .setText(buttonText)
    .setOnClickAction(CardService.newAction()
    .setFunctionName("handleCreateUpdateInteraction")
    .setParameters({ messageData: JSON.stringify(data) }));

 const interactionSection = CardService.newCardSection()
    .setHeader("🔄 Opportunity+ Integration")
    .addWidget(CardService.newTextParagraph().setText("Create or update an Interaction in Opportunity+ based on this email"))
    .addWidget(createInteractionButton);

  const prevButton = CardService.newTextButton()
    .setText("⬅️ Previous")
    .setOnClickAction(CardService.newAction().setFunctionName("prevPage"));

  const nextButton = CardService.newTextButton()
    .setText("➡️ Next")
    .setOnClickAction(CardService.newAction().setFunctionName("nextPage"));

  const buttonSet = CardService.newButtonSet()
    .addButton(prevButton)
    .addButton(nextButton);

  /*const navigationSection = CardService.newCardSection()
    .addWidget(CardService.newTextParagraph().setText("<b>Navigation</b>"))
    .addWidget(buttonSet);*/

  const card = CardService.newCardBuilder()
    .setHeader(header)
    .addSection(CardService.newCardSection()
      .setHeader("📄 Message Details")
      .addWidget(subjectWidget)
      .addWidget(senderWidget)
      .addWidget(toWidget)
      .addWidget(ccWidget)
      .addWidget(bccWidget)
      .addWidget(dateWidget)
      .setCollapsible(true))
    .addSection(CardService.newCardSection()
      .setHeader("📎 Attachments")
      .addWidget(attachmentWidget)
      .setCollapsible(true))
    .addSection(CardService.newCardSection()
      .setHeader("📝 Message Content")
      .addWidget(bodyWidget)
      .setCollapsible(true))
    .addSection(interactionSection)
    //.addSection(navigationSection)
    .build();

  return card;
}

function handleCreateUpdateInteraction(e) {
  try {
    const messageData = JSON.parse(e.parameters.messageData);
    const result = createOrUpdateInteraction(messageData);

    return result;
    /*return CardService.newCardBuilder()
      .addSection(CardService.newCardSection()
      .addWidget(CardService.newTextParagraph()
      .setText(`✅ Interaction ${messageData.existingInteraction ? 'updated' : 'created'} successfully!`)))
      .build();*/

  } catch (error) {
    return CardService.newCardBuilder()
      .addSection(CardService.newCardSection()
      .addWidget(CardService.newTextParagraph()
      .setText(`❌ Error: ${error.message}`)))
      .build();
  }
}