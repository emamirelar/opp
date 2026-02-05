// Helper function to determine if an unmatched email can be selected
function canSelectUnmatchedEmail(unmatchedEmailObj, relatedRecords) {
  const canCreateContacts = relatedRecords.canCreateContacts;
  const canCreatePartners = relatedRecords.canCreatePartners;
  const partnerId = unmatchedEmailObj.partnerId;
  
  // Must be able to create contacts AND either create partners OR have existing partner
  return canCreateContacts && (canCreatePartners || (partnerId !== null && partnerId !== undefined));
}

function buildOpportunityPlusCard(relatedRecords, messageData, checkboxStates) {
  
  //Icon Images
  const partnerIconImage = CardService.newIconImage().setMaterialIcon(
    CardService.newMaterialIcon().setName('corporate_fare'),
  );

  const contactIconImage = CardService.newIconImage().setMaterialIcon(
    CardService.newMaterialIcon().setName('contacts'),
  );

  const userIconImage = CardService.newIconImage().setMaterialIcon(
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
    var userData = relatedRecords.users || [];
    var unmatchedEmailsData = relatedRecords.unmatchedEmails;

    Logger.log('relatedRecords: ' + JSON.stringify(relatedRecords));

    const unmatchedEmailsDataLength = unmatchedEmailsData ? unmatchedEmailsData.length : 0;
    
    // Count selectable records
    let selectableRecordsCount = 0;
    if (unmatchedEmailsData) {
      unmatchedEmailsData.forEach(function(unmatchedEmailObj) {
        if (canSelectUnmatchedEmail(unmatchedEmailObj, relatedRecords)) {
          selectableRecordsCount++;
        }
      });
    }

    Logger.log('unmatchedEmailsDataLength: ' + JSON.stringify(unmatchedEmailsDataLength));
    Logger.log('selectableRecordsCount: ' + JSON.stringify(selectableRecordsCount));


    const collapseButton =
        CardService.newTextButton()
          .setMaterialIcon(CardService.newMaterialIcon().setName('keyboard_arrow_up'))
          .setTextButtonStyle(CardService.TextButtonStyle.BORDERLESS)
          .setText('Hide Unknown');

    const expandButton =
        CardService.newTextButton()
          .setMaterialIcon(CardService.newMaterialIcon().setName('keyboard_arrow_down'))
          .setTextButtonStyle(CardService.TextButtonStyle.BORDERLESS)
          .setText('Select And Add Unknown ' + `(${selectableRecordsCount}/${unmatchedEmailsDataLength})`);

    const numUncollapsed = checkboxStates ? unmatchedEmailsDataLength + 2 : 0;

    const dontKnowSection =
        CardService.newCardSection()
          .setHeader('What We Don\'t Know')
          .setCollapsible(true)
          .setNumUncollapsibleWidgets(numUncollapsed)
          .setCollapseControl(
              CardService.newCollapseControl()
                  .setHorizontalAlign(CardService.HorizontalAlignment.START)
                  .setCollapseButton(collapseButton)
                  .setExpandButton(expandButton),
          );
    
    if(unmatchedEmailsData.length > 0) {
      Logger.log('In unmatchedEmailsData');
      unmatchedEmailsData.forEach(function(unmatchedEmailObj) {
        var emailAddress = unmatchedEmailObj.unmatchedEmail;
        var partnerId = unmatchedEmailObj.partnerId;
        var partnerName = partnerId !== undefined && partnerId !== null ? unmatchedEmailObj.partnerName : unmatchedEmailObj.partnerName + ' (Powered By AI)';
        
        // Check if this record can be selected
        var isSelectable = canSelectUnmatchedEmail(unmatchedEmailObj, relatedRecords);
        
        // Create the decorated text widget
        const decoratedText = CardService.newDecoratedText()
          .setStartIcon(contactIconImage)
          .setTopLabel(partnerName)
          .setText(emailAddress);
        
        // Only add switch control for selectable records
        if (isSelectable) {
          // Determine checkbox state - use checkboxStates if provided, otherwise default to false
          var checkboxValue = false;
          if (checkboxStates && checkboxStates['cb' + emailAddress] !== undefined) {
            checkboxValue = checkboxStates['cb' + emailAddress];
            Logger.log('checkboxStates[cb' + emailAddress + ']: ' + checkboxStates['cb' + emailAddress]);
            Logger.log('checkboxValue: ' + checkboxValue);
          }
          
          // Create and add switch control
          const switchControl = CardService.newSwitch()
            .setFieldName('cb' + emailAddress)
            .setValue(String(checkboxValue))
            .setSelected(checkboxValue)
            .setControlType(CardService.SwitchControlType.CHECK_BOX);
          
          decoratedText.setSwitchControl(switchControl);
        }
        // For non-selectable records, no switch control is added

        Logger.log('decoratedText: ' + decoratedText);

        dontKnowSection.addWidget(decoratedText);
      });

      // Determine if "Select all" should be checked based on checkboxStates
      // Only consider selectable records
      var selectAllState = false;
      if (checkboxStates && selectableRecordsCount > 0) {
        // Check if all selectable checkboxes are selected
        var allSelectableSelected = true;
        for (var i = 0; i < unmatchedEmailsData.length; i++) {
          var unmatchedEmailObj = unmatchedEmailsData[i];
          if (canSelectUnmatchedEmail(unmatchedEmailObj, relatedRecords)) {
            var emailAddress = unmatchedEmailObj.unmatchedEmail;
            if (!checkboxStates['cb' + emailAddress] || checkboxStates['cb' + emailAddress] === false) {
              allSelectableSelected = false;
              break;
            }
          }
        }
        selectAllState = allSelectableSelected;
      }

      // Only show "Select all" if there are selectable records
      if (selectableRecordsCount > 0) {
        dontKnowSection.addWidget(
          CardService.newSelectionInput()
          .setType(CardService.SelectionInputType.SWITCH)
          .setFieldName('select_all')
          .addItem('Select all', 'ALL', selectAllState)
          .setOnChangeAction(
            CardService.newAction()
                    .setFunctionName('handleSelectAll')
                    .setParameters({ 
                      relatedRecords: JSON.stringify(relatedRecords),
                      messageData: JSON.stringify(messageData),
                      totalCheckboxes: unmatchedEmailsData.length.toString(),
                      selectableCount: selectableRecordsCount.toString()
                    })
          )
        );
      }

      dontKnowSection.addWidget(
        CardService.newButtonSet()
          .addButton(
            CardService.newTextButton()
            .setText('Add Selected')
            .setMaterialIcon(CardService.newMaterialIcon().setName('add'))
            .setBackgroundColor('#adedff')
            .setTextButtonStyle(CardService.TextButtonStyle.FILLED_TONAL)
            .setDisabled(selectableRecordsCount === 0)
            .setOnClickAction(
            CardService.newAction()
              .setFunctionName('handleAddSelected')
              .setParameters({ 
                relatedRecords: JSON.stringify(relatedRecords),
                messageData: JSON.stringify(messageData)
              })
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
          .setText('Show All ' + `(${contactData.length + partnerData.length + userData.length})`);

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
              .setStartIcon(contactIconImage)
              .setTopLabel(contact.emailAddress)
              .setText(contact.name)
          );
        }
        else {
          weKnowSection.addWidget(
            CardService.newDecoratedText()
              .setStartIcon(contactIconImage)
              .setTopLabel(contact.emailAddress)
              .setText(`${CONTACT_READ_ERROR_MSG}`)
          );
        }
      });
    }

    if(userData.length > 0) {
      userData.forEach(function(user) {
        if(user.canRead) {
          weKnowSection.addWidget(
            CardService.newDecoratedText()
              .setStartIcon(userIconImage)
              .setTopLabel('Opportunity+ User')
              .setText(user.name)
          );
        }
        else {
          weKnowSection.addWidget(
            CardService.newDecoratedText()
              .setStartIcon(userIconImage)
              .setTopLabel('Opportunity+ User')
              .setText(`${USER_READ_ERROR_MSG}`)
          );
        }
      });
    }

    //The screen does not load if a section does not have atleast one widget
    if(partnerData.length == 0 && contactData.length == 0 && userData.length == 0) {
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
                                          .setIcon(contactIconImage).setOnClickAction(
                                                CardService.newAction()
                                                    .setFunctionName('onContactChipSelected')
                                                    .setParameters({ contact: JSON.stringify(contact) }) // Pass the object JSON as a parameter
                                          );
          chipList.addChip(currentChip);
        }
        else {
          const currentChip = CardService.newChip()
                                          .setLabel(contact.emailAddress)
                                          .setIcon(contactIconImage)
                                          .setDisabled(true);
          chipList.addChip(currentChip);
        }
      });
    }

    if(userData.length > 0) {
      userData.forEach(function(user) {
        if(user.canRead) {
          const currentChip = CardService.newChip()
                                          .setLabel(user.name)
                                          .setIcon(userIconImage)
                                          .setOnClickAction(
                                                CardService.newAction()
                                                    .setFunctionName('onUserChipSelected')
                                                    .setParameters({ user: JSON.stringify(user) }) // Pass the object JSON as a parameter
                                          );
          chipList.addChip(currentChip);
        }
        else {
          const currentChip = CardService.newChip()
                                          .setLabel(user.name || 'Opportunity+ User')
                                          .setIcon(userIconImage)
                                          .setDisabled(true);
          chipList.addChip(currentChip);
        }
      });
    }

    weKnowSection.addWidget(chipList);

    card.addSection(weKnowSection);

    // Add fixed footer with conditional button based on existing interaction
    if(messageData.existingInteraction) {
      const viewUrl = `${getBaseUrl()}/partnerships/interactions/${messageData.existingInteraction.id}`;

      const footerButton = CardService.newTextButton()
            .setText('View')
            .setMaterialIcon(CardService.newMaterialIcon().setName('open_in_new'))
            .setTextButtonStyle(CardService.TextButtonStyle.FILLED)
            .setBackgroundColor('#006699')
            .setOpenLink(CardService.newOpenLink()
              .setUrl(viewUrl)
              .setOpenAs(CardService.OpenAs.FULL_SIZE)
            );      

      card.setFixedFooter(
        CardService.newFixedFooter()
          .setPrimaryButton(footerButton)
      );
    }
    else {
      const footerButton = CardService.newTextButton()
          .setText('Log this')
          .setBackgroundColor('#006699')
          .setDisabled(!relatedRecords.canCreateInteractions)
          .setOnClickAction(
            CardService.newAction()
              .setFunctionName('createInteractionClicked')
              .setParameters({ 
                messageData: JSON.stringify(messageData),
                relatedRecords: JSON.stringify(relatedRecords)
              })
          );
      card.setFixedFooter(
        CardService.newFixedFooter()
          .setPrimaryButton(footerButton)
      );
    }
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
  try {
    // Get form inputs to see which checkboxes are selected
    var formInputs = e.formInputs || {};
    
    // Get the parameters passed from the action
    var relatedRecords = JSON.parse(e.parameters.relatedRecords);
    var messageData = JSON.parse(e.parameters.messageData);
    
    // Create a mapping from email addresses to their parsed name information
    var emailToNameInfo = {};
    if (messageData.parsedEmailNames) {
      messageData.parsedEmailNames.forEach(function(parsedEmail) {
        emailToNameInfo[parsedEmail.email.toLowerCase()] = {
          firstName: parsedEmail.firstName || '',
          middleName: parsedEmail.middleName || '',
          lastName: parsedEmail.lastName || ''
        };
      });
    }
    
    Logger.log('Email to name info mapping: ' + JSON.stringify(emailToNameInfo));
    
    // Collect selected emails - only process selectable records
    var selectedEmails = [];
    var unmatchedEmailsData = relatedRecords.unmatchedEmails;
    Logger.log('unmatchedEmailsData: ' + JSON.stringify(unmatchedEmailsData));
    
    for (var i = 0; i < unmatchedEmailsData.length; i++) {
      var unmatchedEmailObj = unmatchedEmailsData[i];
      var emailAddress = unmatchedEmailObj.unmatchedEmail;
      var fieldName = 'cb' + emailAddress;
      
      // Only process if the record is selectable and checkbox is selected
      if (canSelectUnmatchedEmail(unmatchedEmailObj, relatedRecords) && 
          formInputs[fieldName] && formInputs[fieldName].length > 0) {
        // Use pre-parsed name information
        var nameInfo = emailToNameInfo[emailAddress.toLowerCase()] || { firstName: '', middleName: '', lastName: '' };
        
        Logger.log('For email ' + emailAddress + ', using parsed name info: ' + JSON.stringify(nameInfo));
        
        selectedEmails.push({
          emailAddress: emailAddress,
          partnerName: unmatchedEmailObj.partnerName || '',
          partnerId: unmatchedEmailObj.partnerId !== undefined && unmatchedEmailObj.partnerId !== null 
                    ? unmatchedEmailObj.partnerId 
                    : null,
          firstName: nameInfo.firstName,
          middleName: nameInfo.middleName,
          lastName: nameInfo.lastName
        });
      }
    }
    
    if (selectedEmails.length === 0) {
      return CardService.newActionResponseBuilder()
        .setNotification(CardService.newNotification()
          .setText('Please select at least one email to add'))
        .build();
    }
    
    Logger.log('Selected emails: ' + JSON.stringify(selectedEmails));
    
    // Send selected emails to backend
    return createContactsFromSelectedEmails(selectedEmails, messageData);
    
  } catch (error) {
    Logger.log('Error in handleAddSelected: ' + error.toString());
    return CardService.newActionResponseBuilder()
      .setNotification(CardService.newNotification()
        .setText('Error processing selected items: ' + error.toString()))
      .build();
  }
}

function handleSelectAll(e) {
  try {
    // Get the state of the "Select all" switch
    var formInputs = e.formInputs || {};
    var selectAllState = formInputs['select_all'] && formInputs['select_all'].length > 0;
    
    Logger.log('selectAllState: ' + selectAllState);
    Logger.log('formInputs: ' + JSON.stringify(formInputs));
    // Get the parameters passed from the action
    var relatedRecords = JSON.parse(e.parameters.relatedRecords);
    var messageData = JSON.parse(e.parameters.messageData);
    var totalCheckboxes = parseInt(e.parameters.totalCheckboxes);
    
    Logger.log('totalCheckboxes: ' + totalCheckboxes);
    // Create checkbox states object
    var checkboxStates = {};
    
    // Set checkboxes to the same state as "Select all" only for selectable records
    var unmatchedEmailsData = relatedRecords.unmatchedEmails;
    for (var i = 0; i < unmatchedEmailsData.length; i++) {
      var unmatchedEmailObj = unmatchedEmailsData[i];
      var emailAddress = unmatchedEmailObj.unmatchedEmail;
      
      // Only set state for selectable records (non-selectable records don't have checkboxes)
      if (canSelectUnmatchedEmail(unmatchedEmailObj, relatedRecords)) {
        checkboxStates['cb' + emailAddress] = selectAllState;
      }
    }
    
    Logger.log('checkboxStates: ' + JSON.stringify(checkboxStates));
    // Rebuild the card with updated checkbox states
    var updatedCard = buildOpportunityPlusCard(relatedRecords, messageData, checkboxStates);
    
    // Return navigation to the updated card
    return CardService.newActionResponseBuilder()
      .setNavigation(CardService.newNavigation().updateCard(updatedCard))
      .build();
      
  } catch (error) {
    Logger.log('Error in handleSelectAll: ' + error.toString());
    return CardService.newActionResponseBuilder()
      .setNotification(CardService.newNotification()
        .setText('Error updating selections: ' + error.toString()))
      .build();
  }
}

function handleChipClick(e) {
  return CardService.newActionResponseBuilder()
    .setNotification(CardService.newNotification()
      .setText('Chip clicked: ' + e.parameters.chipId))
    .build();
}