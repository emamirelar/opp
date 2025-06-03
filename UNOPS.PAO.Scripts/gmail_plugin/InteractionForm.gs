function buildInteractionCard(interaction, isUpdate) {
  var card = CardService.newCardBuilder();
  var section = CardService.newCardSection();

  section.addWidget(CardService.newTextInput()
    .setFieldName('subject')
    .setTitle('Subject')
    .setValue(interaction ? interaction.Subject : '')
    .setMultiline(false));

  section.addWidget(CardService.newTextInput()
    .setFieldName('emailAddresses')
    .setTitle('Email Addresses')
    .setValue(interaction ? (interaction.EmailAddresses || []).join(', ') : '')
    .setMultiline(false));

  section.addWidget(CardService.newTextInput()
    .setFieldName('description')
    .setTitle('Description')
    .setValue(interaction ? interaction.data : '')
    .setMultiline(true));

  var button = CardService.newTextButton()
    .setText(isUpdate ? 'Update Interaction' : 'Create Interaction')
    .setOnClickAction(CardService.newAction()
      .setFunctionName(isUpdate ? 'onUpdateInteraction' : 'onCreateInteraction'));

  section.addWidget(button);

  card.addSection(section);
  return card.build();
}