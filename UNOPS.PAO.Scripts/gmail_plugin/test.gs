/*function onGmailMessageOpen(e) {
  console.log(e);
  var mainCard = createCard(e);
  return mainCard;
}*/

// function onCalendarEventOpen(e) {
//   console.log(e);
//   var mainCard = createCard(e);
//   return mainCard;
// }

// function onGmailMessageOpen(e) {
function createCardOrig(e){ 
    var accessToken = e.gmail.accessToken;
    GmailApp.setCurrentMessageAccessToken(accessToken);

    // Collecting metadata
    var messageId = e.gmail.messageId;

    var properties = PropertiesService.getUserProperties();
    properties.setProperty('originalE', JSON.stringify(e));

    var threadId = e.gmail.threadId;
    var message = GmailApp.getMessageById(messageId);
    var currThread = GmailApp.getThreadById(threadId);
    var threadMsgs = currThread.getMessages();
    var nameArr = [];
    var attachmentList = [];
    var currentPage = 1;

    //for (var t = 0; t < threadMsgs.length; t++) {
    var mailAttachments = threadMsgs[0].getAttachments({
            includeInlineImages : false,
            includeAttachments: true
    });

      if (mailAttachments.length > 0){    
        for (let a=0; a < mailAttachments.length; a++){
          if (a == 0) {
            nameArr.push(a+1+'. '+mailAttachments[a].getName());
            attachmentList.push(mailAttachments[a].getName());
          }
          else {
            nameArr.push('\n'+(a+1)+'. '+mailAttachments[a].getName());
            attachmentList.push(mailAttachments[a].getName());
          }
        }  
        nameArr.join(',').toString();
      }
      else {
        nameArr = "None";
      }

    var sender = message.getFrom(); //threadMsgs[0].getFrom();
    var subject = message.getSubject();
    var copied = message.getCc();
    if (copied == undefined ){
      copied = ""
    }

    var msgDate = Utilities.formatDate(new Date(message.getDate()), 'Europe/Copenhagen', 'dd-MMM-yyyy');
    // var emailBody = message.getBody();
    var emailBody = message.getPlainBody();


    var subjectWidget = CardService.newDecoratedText()
                      .setTopLabel('Email Subject : ')
                      .setText(subject)
                      .setWrapText(true)

    var senderWidget = CardService.newDecoratedText()
                      .setTopLabel('From : ')
                      .setText(sender)
                      .setWrapText(true)

    var ccWidget = CardService.newDecoratedText()
                      .setTopLabel('Cc : ')
                      .setText(copied)  
                      .setWrapText(true)

    var attachmentWidget = CardService.newDecoratedText()
                      //.setTopLabel('Attachments : ')
                      .setText(nameArr)  
                      .setWrapText(true)
                      

    var msgDateWidget = CardService.newDecoratedText()
                      .setTopLabel('Received On : ')
                      .setText(msgDate)   

    const extractedText = "This is the extracted content from the email.";

    var msgContentWidget = CardService.newDecoratedText()
                          .setTopLabel('Message Content')
                          .setWrapText(true)
                          .setText(emailBody)
    
  // Create a card with a card section and widgets.                                           
      var msgCard = CardService.newCardBuilder()
        .setHeader(CardService.newCardHeader()
                .setTitle(" ")
                .setImageStyle(CardService.ImageStyle.CIRCLE)
                // .setImageUrl('https://cdn.pixabay.com/photo/2018/11/10/19/11/airline-3807267_1280.png')
                .setSubtitle("Contents")
                )       
        .addSection(CardService.newCardSection()
            .setHeader("<b>Message Details</b>")
            .addWidget(subjectWidget)
            .addWidget(senderWidget)
            .addWidget(ccWidget)
            .addWidget(msgDateWidget)
            .setCollapsible(true)
            )

         .addSection(CardService.newCardSection()
            .setHeader("<b>Message Content</b>")
            .addWidget(msgContentWidget)
            .setCollapsible(true))
          
         .addSection(CardService.newCardSection()
            .setHeader("<b>Attachments</b>")
            .addWidget(attachmentWidget)
        )    

        .build();  
   
    return [msgCard];
}