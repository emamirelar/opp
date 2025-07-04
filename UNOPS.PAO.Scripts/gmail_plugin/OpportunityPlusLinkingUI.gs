function buildOpportunityPlusCard(relatedRecords, messageData) {
  
  //Icon Images
  const partnerIconImage = CardService.newIconImage().setMaterialIcon(
    CardService.newMaterialIcon().setName('corporate_fare'),
  );

  const personIconImage = CardService.newIconImage().setMaterialIcon(
    CardService.newMaterialIcon().setName('person'),
  );
  
  // Create a new card builder
  var card = CardService.newCardBuilder();

  card.setHeader(
    CardService.newCardHeader()
      .setTitle("Opportunity+")
      .setSubtitle("Related Records")
      .setImageStyle(CardService.ImageStyle.CIRCLE)
      .setImageUrl(ICON_URL)
  );

  if(relatedRecords) {
    var contactData = relatedRecords.contacts;
    var partnerData = relatedRecords.partners;
    var unmatchedEmailsData = relatedRecords.unmatchedEmails;

    Logger.log('relatedRecords: ' + JSON.stringify(relatedRecords));

    const unmatchedEmailsDataLength = unmatchedEmailsData ? unmatchedEmailsData.length : 0;

    Logger.log('unmatchedEmailsDataLength: ' + JSON.stringify(unmatchedEmailsDataLength));


    const collapseButton =
        CardService.newTextButton()
          .setMaterialIcon(CardService.newMaterialIcon().setName('keyboard_arrow_up'))
          .setTextButtonStyle(CardService.TextButtonStyle.BORDERLESS)
          .setText('Hide Unknown');

    const expandButton =
        CardService.newTextButton()
          .setMaterialIcon(CardService.newMaterialIcon().setName('keyboard_arrow_down'))
          .setTextButtonStyle(CardService.TextButtonStyle.BORDERLESS)
          .setText('Select And Add Unknown ' + `(${unmatchedEmailsDataLength})`);

    const dontKnowSection =
        CardService.newCardSection()
          .setHeader('What We Don\'t Know')
          .setCollapsible(true)
          .setNumUncollapsibleWidgets(0)
          .setCollapseControl(
              CardService.newCollapseControl()
                  .setHorizontalAlign(CardService.HorizontalAlignment.START)
                  .setCollapseButton(collapseButton)
                  .setExpandButton(expandButton),
          );
    
    var unmatchedCounter = 1;
    if(unmatchedEmailsData.length > 0) {
      Logger.log('In unmatchedEmailsData');
      unmatchedEmailsData.forEach(function(unmatchedEmailObj) {
        var emailAddress = unmatchedEmailObj.unmatchedEmail;
        var partnerName = unmatchedEmailObj.partnerName;
        var partnerId = unmatchedEmailObj.partnerId;
                      
        // Add widgets to section 1
        const decoratedText = CardService.newDecoratedText()
          .setStartIcon(personIconImage)
          .setTopLabel(partnerName)
          .setText(emailAddress)
          .setSwitchControl(
            CardService.newSwitch()
              .setFieldName('checkbox' + `${unmatchedCounter}`)
              .setValue('false')
              .setControlType(CardService.SwitchControlType.CHECK_BOX)
            );

        dontKnowSection.addWidget(decoratedText);

        unmatchedCounter += 1;
      });

      dontKnowSection.addWidget(
        CardService.newSelectionInput()
        .setType(CardService.SelectionInputType.SWITCH)
        .setFieldName('select_all')
        .addItem('Select all', 'ALL', false)
        .setOnChangeAction(
          CardService.newAction()
                  .setFunctionName('handleSelectAll')
        )
      );

      dontKnowSection.addWidget(
        CardService.newButtonSet()
          .addButton(
            CardService.newTextButton()
              .setText('Add Selected')
              .setMaterialIcon(CardService.newMaterialIcon().setName('add'))
              .setBackgroundColor('#adedff')
              .setTextButtonStyle(CardService.TextButtonStyle.FILLED_TONAL)
              .setOnClickAction(
                CardService.newAction()
                  .setFunctionName('handleAddSelected')
              )
          )
      );
    }
    else {
      //The screen does not load if a section does not have atleast one widget
      dontKnowSection.addWidget(CardService.newTextParagraph()
            .setText("<font color=\"#555555\">" + `${EMPTY_MSG}` + "</font>")
          );
    }
    card.addSection(dontKnowSection);

    //What We Know
    const weKnowCollapseButton =
        CardService.newTextButton()
          .setMaterialIcon(CardService.newMaterialIcon().setName('keyboard_arrow_up'))
          .setTextButtonStyle(CardService.TextButtonStyle.BORDERLESS)
          .setText('Hide');

    const weKnowExpandButton =
        CardService.newTextButton()
          .setMaterialIcon(CardService.newMaterialIcon().setName('keyboard_arrow_down'))
          .setTextButtonStyle(CardService.TextButtonStyle.BORDERLESS)
          .setText('Show All ' + `(${contactData.length + partnerData.length})`);

    const weKnowSection =
        CardService.newCardSection()
          .setHeader('What We Know')
          .setCollapsible(true)
          .setNumUncollapsibleWidgets(3)
          .setCollapseControl(
              CardService.newCollapseControl()
                  .setHorizontalAlign(CardService.HorizontalAlignment.START)
                  .setCollapseButton(weKnowCollapseButton)
                  .setExpandButton(weKnowExpandButton),
          );

    if(partnerData.length > 0) {
      partnerData.forEach(function(partner) {
        if(partner.canRead) {
          weKnowSection.addWidget(
            CardService.newDecoratedText()
              .setStartIcon(partnerIconImage)
              .setTopLabel('Partner')
              .setText(partner.name)
          );
        }
        else {
          weKnowSection.addWidget(
            CardService.newDecoratedText()
              .setStartIcon(partnerIconImage)
              .setTopLabel('Partner')
              .setText(partner.name)
          );
        }
      });
    }

    if(contactData.length > 0) {
      contactData.forEach(function(contact) {
        if(contact.canRead) {
          weKnowSection.addWidget(
            CardService.newDecoratedText()
              .setStartIcon(personIconImage)
              .setTopLabel(contact.emailAddress)
              .setText(contact.name)
          );
        }
        else {
          weKnowSection.addWidget(
            CardService.newDecoratedText()
              .setStartIcon(personIconImage)
              .setTopLabel(contact.emailAddress)
              .setText(`${CONTACT_READ_ERROR_MSG}`)
          );
        }
      });
    }

    //The screen does not load if a section does not have atleast one widget
    if(partnerData.length == 0 && contactData.length == 0) {
      weKnowSection.addWidget(CardService.newTextParagraph()
            .setText("<font color=\"#555555\">" + `${EMPTY_MSG}` + "</font>")
          );
    }

    //Chip Section
    const chipList = CardService.newChipList()
                      .setLayout(CardService.ChipListLayout.WRAPPED);

    if(partnerData.length > 0) {
      partnerData.forEach(function(partner) {
        if(partner.canRead) {
          const currentChip = CardService.newChip()
                                          .setLabel(partner.name)
                                          .setIcon(partnerIconImage)
                                          .setOnClickAction(
                                                CardService.newAction()
                                                    .setFunctionName('onPartnerChipSelected')
                                                    .setParameters({ partner: JSON.stringify(partner) }) // Pass the object JSON as a parameter
                                          );
          chipList.addChip(currentChip);
        }
        else {
          const currentChip = CardService.newChip()
                                          .setLabel(partner.name)
                                          .setIcon(partnerIconImage)
                                          .setDisabled(true);
          chipList.addChip(currentChip);
        }
      });
    }

    if(contactData.length > 0) {
      contactData.forEach(function(contact) {
        if(contact.canRead) {
          const currentChip = CardService.newChip()
                                          .setLabel(contact.name)
                                          .setIcon(personIconImage).setOnClickAction(
                                                CardService.newAction()
                                                    .setFunctionName('onContactChipSelected')
                                                    .setParameters({ contact: JSON.stringify(contact) }) // Pass the object JSON as a parameter
                                          );
          chipList.addChip(currentChip);
        }
        else {
          const currentChip = CardService.newChip()
                                          .setLabel(contact.emailAddress)
                                          .setIcon(personIconImage)
                                          .setDisabled(true);
          chipList.addChip(currentChip);
        }
      });
    }

    weKnowSection.addWidget(chipList);

    card.addSection(weKnowSection);

    // Add fixed footer
    card.setFixedFooter(
      CardService.newFixedFooter()
        .setPrimaryButton(
          CardService.newTextButton()
            .setText('Log this')
            .setBackgroundColor('#006699')
            .setOnClickAction(
              CardService.newAction()
                .setFunctionName('createOrUpdateInteraction')
                .setParameters({ messageData: JSON.stringify(messageData) })
            )
        )
    );
  }
  else {
    var errorSection = CardService.newCardSection()
      .setHeader("<b><font color=\"#005073\">Error</font></b>");

      errorSection.addWidget(CardService.newTextParagraph()
            .setText("<font color=\"#555555\">" + `${RELATED_RECORDS_ERROR_MSG}` + "</font>")
          );
      card.addSection(errorSection);
  }
  return card.build();
}

// Handler functions
function onClickPrimaryButton(e) {
  return CardService.newActionResponseBuilder()
    .setNotification(CardService.newNotification()
      .setText('Action logged successfully'))
    .build();
}

function handleAddSelected(e) {
  return CardService.newActionResponseBuilder()
    .setNotification(CardService.newNotification()
      .setText('Selected items added'))
    .build();
}

function handleSelectAll(e) {
  return CardService.newActionResponseBuilder()
    .setNotification(CardService.newNotification()
      .setText('Selected all clicked'))
    .build();
}

function handleChipClick(e) {
  return CardService.newActionResponseBuilder()
    .setNotification(CardService.newNotification()
      .setText('Chip clicked: ' + e.parameters.chipId))
    .build();
}