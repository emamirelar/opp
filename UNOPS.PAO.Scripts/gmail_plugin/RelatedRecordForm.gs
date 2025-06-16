/**
 * Creates a Gmail add-on that displays "Contact" and "Partners" information,
 * mimicking the provided screenshot.
 *
 * @param {Object} e The event object, containing information about the context
 * in which the add-on is running.
 * @return {CardService.Card} The card to be displayed in the Gmail add-on.
 */
function buildRelatedRecords(relatedRecords, messageData) {

  // Create a new card builder
  var card = CardService.newCardBuilder();

  card.setHeader(
    CardService.newCardHeader()
      .setTitle("Opportunity+")
      .setSubtitle("Related Records")
      .setImageStyle(CardService.ImageStyle.CIRCLE)
      .setImageUrl(ICON_URL)
  );


  if (relatedRecords.contacts.length > 0) {
    // --- Contact Section ---
    var contactData = relatedRecords.contacts;

    var contactSection = CardService.newCardSection()
        .setHeader("<b><font color=\"#005073\">Contacts (" + contactData.length + ")</font></b>")
        .setCollapsible(true);

    contactData.forEach(function(contact) {
      contactSection.addWidget(CardService.newDecoratedText()
        // CORRECTED: Set icon directly using setIcon with CardService.Icon enum
        .setIcon(CardService.Icon.PERSON)
        .setText(contact.name)
        .setButton(CardService.newTextButton()
          .setText("View Contact")
          .setTextButtonStyle(CardService.TextButtonStyle.TEXT)
          .setOpenLink(CardService.newOpenLink().setUrl(`${OPPORTUNITY_PLUS_ENDPOINT}/partnerships/contacts/${contact.id}`))
        )
        .setWrapText(true)
      );

      contactSection.addWidget(CardService.newTextParagraph()
        .setText("<font color=\"#555555\">Title: " + contact.title + "<br>Partner Name: " + contact.partnerName + "</font>")
      );

      if (contactData.length > 1 && contactData.indexOf(contact) < contactData.length - 1) {
        contactSection.addWidget(CardService.newDivider());
      }
    });
    card.addSection(contactSection);
  }

  if(relatedRecords.partners.length > 0) {
    // --- Partners Section ---
    var partnersData = relatedRecords.partners;

    var partnersSection = CardService.newCardSection()
        .setHeader("<b><font color=\"#005073\">Partners (" + partnersData.length + ")</font></b>")
        .setCollapsible(true);

    partnersData.forEach(function(partner) {
      partnersSection.addWidget(CardService.newDecoratedText()
        // CORRECTED: Set icon directly using setIcon with CardService.Icon enum
        .setIcon(CardService.Icon.BUILDING)
        .setText(partner.name)
        .setButton(CardService.newTextButton()
          .setText("View Partner")
          .setTextButtonStyle(CardService.TextButtonStyle.TEXT)
          .setOpenLink(CardService.newOpenLink().setUrl(`${OPPORTUNITY_PLUS_ENDPOINT}/partnerships/partners/${partner.id}`))
        )
        .setWrapText(true)
      );

      partnersSection.addWidget(CardService.newTextParagraph()
        .setText("<font color=\"#555555\">Partner Code: " + partner.partnerCode + "<br> Phone: " + partner.phone + "</font>")
      );

      if (partnersData.length > 1 && partnersData.indexOf(partner) < partnersData.length - 1) {
        partnersSection.addWidget(CardService.newDivider());
      }
    });
    card.addSection(partnersSection);
  }

  if(relatedRecords.unmatchedEmails.length > 0) {
    // --- Unmatched Emails Section ---
    var unmatchedEmailsData = relatedRecords.unmatchedEmails;

    var unmatchedEmailsSection = CardService.newCardSection()
        .setHeader("<b><font color=\"#005073\">Unmatched Emails (" + unmatchedEmailsData.length + ")</font></b>")
        .setCollapsible(true);
    
    unmatchedEmailsData.forEach(function(unmatchedEmail) {
      unmatchedEmailsSection.addWidget(CardService.newDecoratedText()
        .setIcon(CardService.Icon.EMAIL)
        .setText(unmatchedEmail)
        .setButton(CardService.newTextButton()
          .setText("Create Contact")
          .setTextButtonStyle(CardService.TextButtonStyle.TEXT)
          .setOnClickAction(CardService.newAction()
          .setFunctionName("handleCreateContact")
          .setParameters({ emailAddress: unmatchedEmail }))
        )
        .setWrapText(true)
      );

      if (unmatchedEmailsData.length > 1 && unmatchedEmailsData.indexOf(unmatchedEmail) < unmatchedEmailsData.length - 1) {
        unmatchedEmailsSection.addWidget(CardService.newDivider());
      }
    });
    card.addSection(unmatchedEmailsSection);
  }

  const createUpdatebuttonText = "View Interaction Form";


  Logger.log('Message Data Build Related Records: ' + JSON.stringify(messageData));
  
  const interactionButton = CardService.newTextButton()
    .setText(createUpdatebuttonText)
    .setTextButtonStyle(CardService.TextButtonStyle.FILLED)
    .setBackgroundColor('#005073')
    .setOnClickAction(CardService.newAction()
      .setFunctionName("buildInteractionCard")
      .setParameters({ messageData: JSON.stringify(messageData) }));

  const interactionSection = CardService.newCardSection()
                                          .addWidget(interactionButton);

  card.addSection(interactionSection);                                          

  return card.build();
}

/**
 * The entry point for the Gmail add-on.
 * This function is typically configured as a contextual trigger in the
 * add-on's manifest (appsscript.json).
 
function onGmailMessageOpen(e) {
  return buildRelatedRecords(e);
}
*/