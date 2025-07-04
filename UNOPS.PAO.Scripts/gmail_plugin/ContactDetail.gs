/**
 * Action handler for a contact chip being clicked.
 * This function retrieves the contact JSON string passed as a parameter.
 *
 * @param {Object} e The event object, containing parameters set by the action.
 * @return {GoogleAppsScript.Card_Service.ActionResponse} A response to update UI or show notification.
 */
function onContactChipSelected(e) {
  const contact = e.parameters ? JSON.parse(e.parameters.contact) : undefined;

  const personIconImage = CardService.newIconImage().setMaterialIcon(
    CardService.newMaterialIcon().setName('person'),
  );

  // Create a new card builder
  var card = CardService.newCardBuilder();

  //Header
  if(contact.profilePictureUrl && contact.title) {
    card.setHeader(
      CardService.newCardHeader()
        .setTitle(contact.name)
        .setSubtitle(contact.title)
        .setImageStyle(CardService.ImageStyle.CIRCLE)
        .setImageUrl(contact.profilePictureUrl)
    );
  }
  else if(contact.profilePictureUrl) {
    card.setHeader(
      CardService.newCardHeader()
        .setTitle(contact.name)
        .setSubtitle("Contact")
        .setImageStyle(CardService.ImageStyle.CIRCLE)
        .setImageUrl(contact.profilePictureUrl)
    );
  }
  else if(contact.title) {
    card.setHeader(
      CardService.newCardHeader()
        .setTitle(contact.name)
        .setSubtitle(contact.title)
        .setImageStyle(CardService.ImageStyle.CIRCLE)
        .setImageUrl(ICON_URL)
    );
  }
  else {
    card.setHeader(
      CardService.newCardHeader()
        .setTitle(contact.name)
        .setSubtitle("Contact")
        .setImageStyle(CardService.ImageStyle.CIRCLE)
        .setImageUrl(ICON_URL)
      );
  }

  //Contact Details
  const detailSection =
        CardService.newCardSection()
          .setHeader('Contact Details')
          .setCollapsible(false);

  if(contact.emailAddress) {
    detailSection.addWidget(
            CardService.newDecoratedText()
              .setStartIcon(CardService.newIconImage().setMaterialIcon(CardService.newMaterialIcon().setName('email')))
              .setText(contact.emailAddress)
              .setBottomLabel('Email')
          );
  }

  if(contact.location) {
    detailSection.addWidget(
            CardService.newDecoratedText()
              .setStartIcon(CardService.newIconImage().setMaterialIcon(CardService.newMaterialIcon().setName('location_on')))
              .setText(contact.location)
              .setBottomLabel('Mailing')
          );
  }

  if(contact.phone) {
    detailSection.addWidget(
            CardService.newDecoratedText()
              .setStartIcon(CardService.newIconImage().setMaterialIcon(CardService.newMaterialIcon().setName('call')))
              .setText(contact.phone)
              .setBottomLabel('Phone')
          );
  }

  if(contact.emailAddress || contact.location || contact.phone) {
    card.addSection(detailSection);
  }

  if(contact.interactions && contact.interactions.length > 0) {
    //Recent interactions
    const interactionSection =
          CardService.newCardSection()
            .setHeader('Recent interactions')
            .setCollapsible(false);

    // Sort interactions by date (most recent first) and take first 5
    const sortedInteractions = contact.interactions
        .filter(interaction => interaction.canRead)
        .sort((a, b) => new Date(b.date) - new Date(a.date))
        .slice(0, 5);
            
    sortedInteractions.forEach(function(interaction) {
        // Format the date
        const interactionDate = new Date(interaction.date);
        const formattedDate = `${interactionDate.getMonth() + 1}/${interactionDate.getDate()}/${interactionDate.getFullYear()}`;
        
        // Determine the interaction type label
        let typeLabel = interaction.type;
        if (interaction.type === 'Email') {
          typeLabel = 'Sent';
        } else if (interaction.type === 'EmailReceived') {
          typeLabel = 'Received';
        } else if (interaction.type === 'Call') {
          typeLabel = 'Call';
        } else if (interaction.type === 'Meeting') {
          typeLabel = 'Meeting';
        }

        interactionSection.addWidget(
          CardService.newDecoratedText()
            .setStartIcon(CardService.newIconImage().setMaterialIcon(CardService.newMaterialIcon().setName('mail')))
            .setText(interaction.description)
            .setTopLabel(`${typeLabel} ${formattedDate}`)
        );
      });
      card.addSection(interactionSection);
  }

  //Back Section
  const backSection =
          CardService.newCardSection();

  const customBackButton = CardService.newTextButton()
      .setText('Back to list')
      .setTextButtonStyle(CardService.TextButtonStyle.OUTLINED) 
      .setMaterialIcon(CardService.newMaterialIcon().setName('arrow_back'))
      .setBackgroundColor("#e6e6e6")
      .setOnClickAction(
          CardService.newAction()
              .setFunctionName('onClickBackButton')
      );

  backSection.addWidget(customBackButton);

  card.addSection(backSection);

  return card.build();

}

// Handler functions
function onClickBackButton(e) {
  // Create a navigation object that pops the current card off the stack
  const navigation = CardService.newNavigation().popCard();

  // Return an ActionResponse with the navigation
  return CardService.newActionResponseBuilder()
      .setNavigation(navigation)
      .build();
}