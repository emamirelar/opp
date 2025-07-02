/*
* CURRENTLY NOT USED
*/

/*
function buildInteractionCard(e) {
  const messageData = JSON.parse(e.parameters.messageData);
  const interaction = getMappedInteractionData(messageData); // Assumed to return an object with Subject, EmailAddresses, Data

  var card = CardService.newCardBuilder();

  card.setHeader(
    CardService.newCardHeader()
      .setTitle("Opportunity+")
      .setSubtitle("Interaction Details")
      .setImageStyle(CardService.ImageStyle.CIRCLE)
      .setImageUrl(ICON_URL)
  );

  // --- Main Section for Form Fields ---
  var formSection = CardService.newCardSection();

  // Subject Field
  formSection.addWidget(CardService.newTextParagraph()
    .setText("<b><font color=\"#005073\">Subject</font></b>"));
  formSection.addWidget(CardService.newTextParagraph()
    .setText(interaction && interaction.Subject ? interaction.Subject : ''));

  // Email Addresses Field
  formSection.addWidget(CardService.newTextParagraph()
    .setText("<b><font color=\"#005073\">Email Addresses</font></b>"));
  formSection.addWidget(CardService.newTextParagraph()
    .setText(interaction && interaction.EmailAddresses ? (interaction.EmailAddresses || []).join(', ') : ''));

  // Description Field
  formSection.addWidget(CardService.newDecoratedText()
    .setText("<b><font color=\"#005073\">Description</font></b>")); // Apply brand color to label
  formSection.addWidget(CardService.newTextInput()
    .setFieldName('description')
    // Remove setTitle here
    .setValue(interaction && interaction.Description ? interaction.Description : '')
    .setMultiline(true)); // Keep multiline for description

  card.addSection(formSection);

  // --- Button Section (often separate for better visual hierarchy) ---
  var buttonSection = CardService.newCardSection();

  var actionButton = CardService.newTextButton()
      .setText(messageData.existingInteraction ? 'Update Interaction' : 'Create Interaction')
      .setTextButtonStyle(CardService.TextButtonStyle.FILLED) // Gives it a prominent, filled look
      .setBackgroundColor('#005073')
      .setOnClickAction(CardService.newAction()
          .setFunctionName("createOrUpdateInteraction")
          .setParameters({ messageData: JSON.stringify(messageData) }));

  buttonSection.addWidget(actionButton);
  card.addSection(buttonSection);

  return card.build();
}*/