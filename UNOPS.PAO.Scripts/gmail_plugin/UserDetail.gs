/**
 * Action handler for a user chip being clicked.
 * This function retrieves the user JSON string passed as a parameter.
 *
 * @param {Object} e The event object, containing parameters set by the action.
 * @return {GoogleAppsScript.Card_Service.ActionResponse} A response to update UI or show notification.
 */
function onUserChipSelected(e) {
  const user = e.parameters ? JSON.parse(e.parameters.user) : undefined;

  const userIconImage = CardService.newIconImage().setMaterialIcon(
    CardService.newMaterialIcon().setName('account_circle'),
  );

  // Create a new card builder
  var card = CardService.newCardBuilder();

  //Header
  if(user.email && user.email.trim() !== '') {
    card.setHeader(
      CardService.newCardHeader()
        .setTitle(user.name)
        .setSubtitle(user.email)
        .setImageStyle(CardService.ImageStyle.CIRCLE)
        .setImageUrl(ICON_URL)
    );
  }
  else {
    card.setHeader(
      CardService.newCardHeader()
        .setTitle(user.name)
        .setSubtitle("Opportunity+ User")
        .setImageStyle(CardService.ImageStyle.CIRCLE)
        .setImageUrl(ICON_URL)
      );
  }

  //User Details
  const detailSection =
        CardService.newCardSection()
          .setHeader('User Details')
          .setCollapsible(false);

  if(user.name && user.name.trim() !== '') {
    detailSection.addWidget(
      CardService.newDecoratedText()
        .setStartIcon(CardService.newIconImage().setMaterialIcon(CardService.newMaterialIcon().setName('account_circle')))
        .setText(user.name)
        .setBottomLabel('Name')
    );
  }

  if(user.email && user.email.trim() !== '') {
    detailSection.addWidget(
      CardService.newDecoratedText()
        .setStartIcon(CardService.newIconImage().setMaterialIcon(CardService.newMaterialIcon().setName('email')))
        .setText(user.email)
        .setBottomLabel('Email')
    );
  }

  if(user.orgUnit && user.orgUnit.trim() !== '') {
    detailSection.addWidget(
      CardService.newDecoratedText()
        .setStartIcon(CardService.newIconImage().setMaterialIcon(CardService.newMaterialIcon().setName('work')))
        .setText(user.orgUnit)
        .setBottomLabel('Org Unit')
    );
  }

  card.addSection(detailSection);

  // Note: Users typically don't have interactions in the same way contacts do
  // If in the future users have interaction history, it can be added here

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

// Handler functions (shared with ContactDetail.gs)
function onClickBackButton(e) {
  // Create a navigation object that pops the current card off the stack
  const navigation = CardService.newNavigation().popCard();

  // Return an ActionResponse with the navigation
  return CardService.newActionResponseBuilder()
      .setNavigation(navigation)
      .build();
}