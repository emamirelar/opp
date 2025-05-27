function onHomePageOpen(e){
 
  var mainCard = CardService.newCardBuilder()
                .setHeader(
                  CardService.newCardHeader()
                .setTitle("Content Extractor")
                .setImageStyle(CardService.ImageStyle.CIRCLE)
                )
                
                .addSection(CardService.newCardSection()
                    .setHeader("<b><color='#000000'>Contents Extracted</font></b>")
                    .addWidget(CardService.newTextParagraph()
                        .setText(homePageMsg)
                    )
                )                            
                .build()
  return mainCard;
}