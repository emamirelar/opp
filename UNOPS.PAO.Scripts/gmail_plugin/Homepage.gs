function onHomePageOpen(e) {
  var mainCard = CardService.newCardBuilder()
    .setHeader(
      CardService.newCardHeader()
        .setTitle("Opportunity+") 
        .setSubtitle("Home")
        .setImageStyle(CardService.ImageStyle.CIRCLE)
        .setImageUrl(ICON_URL)
    )
    .addSection(
      CardService.newCardSection()
        .setHeader("<b><font color=\"#005073\">Welcome</font></b>")
        .addWidget(
          CardService.newTextParagraph()
            .setText("Open an email to see related Contacts, Partners, and more.")
        )
    )
    .build();
  return mainCard;
}