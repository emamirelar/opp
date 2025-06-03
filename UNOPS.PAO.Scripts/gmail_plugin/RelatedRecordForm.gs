/**
 * Creates a Gmail add-on that displays "People" and "Partners" information,
 * mimicking the provided screenshot.
 *
 * @param {Object} e The event object, containing information about the context
 * in which the add-on is running.
 * @return {CardService.Card} The card to be displayed in the Gmail add-on.
 */
function buildRelatedRecords(relatedRecords, interaction, isUpdate) {

  const buttonText = isUpdate ? "Update Interaction" : "Create Interaction";

  const interactionButton = CardService.newTextButton()
    .setText(buttonText)
    .setOnClickAction(CardService.newAction()
    .setFunctionName("handleCreateUpdateInteraction")
    .setParameters({ messageData: JSON.stringify(interaction) }));

  // Create a new card builder
  var card = CardService.newCardBuilder();

  // --- People Section ---
  var peopleSection = CardService.newCardSection()
      .setHeader(`People(` + `${relatedRecords.contacts.size()}` + `)`); // Header for People section

  // Mock data for People.
  var peopleData = [
    {
      name: "Jane Doe",
      title: "Senior Officer",
      partnerName: "UNOPS Personnel",
      profileUrl: `${API_BASE_URL}#/partnerships/contacts/0`
    },
    {
      name: "John Doe",
      title: "Project Manager",
      partnerName: "UNOPS Personnel",
      profileUrl: `${API_BASE_URL}#/partnerships/contacts/0`
    }
  ];

  peopleData.forEach(function(person) {
    peopleSection.addWidget(CardService.newDecoratedText()
      // CORRECTED: Set icon directly using setIcon with CardService.Icon enum
      .setIcon(CardService.Icon.PERSON)
      .setText("<b>" + person.name + "</b>")
      .setButton(CardService.newTextButton()
        .setText("View Profile")
        .setOpenLink(CardService.newOpenLink().setUrl(person.profileUrl))
      )
      .setWrapText(true)
    );

    peopleSection.addWidget(CardService.newTextParagraph()
      .setText("Title: " + person.title + "<br>Partner Name: " + person.partnerName)
    );

    if (peopleData.length > 1 && peopleData.indexOf(person) < peopleData.length - 1) {
      peopleSection.addWidget(CardService.newDivider());
    }
  });
  card.addSection(peopleSection);


  // --- Partners Section ---
  var partnersSection = CardService.newCardSection()
      .setHeader("Partners (2)"); // Header for Partners section

  // Mock data for Partners.
  var partnersData = [
    {
      name: "Accenture",
      partnerOwner: "Jane Doe",
      status: "Active",
      partnerUrl: "https://example.com/unops-personnel"
    },
    {
      name: "Nippon Foundation",
      partnerOwner: "Alice Smith",
      status: "Active",
      partnerUrl: "https://example.com/global-fund"
    }
  ];

  partnersData.forEach(function(partner) {
    partnersSection.addWidget(CardService.newDecoratedText()
      // CORRECTED: Set icon directly using setIcon with CardService.Icon enum
      .setIcon(CardService.Icon.BUILDING)
      .setText("<b>" + partner.name + "</b>")
      .setButton(CardService.newTextButton()
        .setText("View Partner")
        .setOpenLink(CardService.newOpenLink().setUrl(partner.partnerUrl))
      )
      .setWrapText(true)
    );

    partnersSection.addWidget(CardService.newTextParagraph()
      .setText("Partner Owner: " + partner.partnerOwner + "<br>Partner Recipient: " + partner.partnerRecipient)
    );

    if (partnersData.length > 1 && partnersData.indexOf(partner) < partnersData.length - 1) {
      partnersSection.addWidget(CardService.newDivider());
    }
  });
  card.addSection(partnersSection);

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