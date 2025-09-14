/**
  * This function serves as the entry point for the Gmail Add-on homepage.
 *
 * @param {Object} e The event object, which is currently unused but can
 * contain contextual information in more complex add-ons.
 * @return {GoogleAppsScript.Card_Service.Card} The constructed card object.
 */
function createGmailAddonCard(e) {
  // Define the UI structure based on the Figma UI Kit JSON.
  // This configuration has been updated to include images.
  const uiConfig = {
    "header": {
      "title": "Opportunity+",
      "subtitle": "Linking Partners",
      "imageUrl": "https://i.ibb.co/qLSDKBmW/Opportunity-Logo-Graphic1000px.png",
      "imageType": "CIRCLE"
    },
    "sections": [
      {
        "header": "", // Empty header for this section
        "widgets": [
          {
            "image": {
              "imageUrl": "https://i.ibb.co/KxWD5kmf/1.jpg",
              "altText": "Step 1"
            }
          }
        ]
      },
      {
        "header": "", // Empty header for this section
        "widgets": [
          {
            "image": {
              "imageUrl": "https://i.ibb.co/tVhXtqz/2.jpg",
              "altText": "Step 2-1"
            }
          }
        ]
      },
      {
        "header": "", // Empty header for this section
        "widgets": [
          {
            "image": {
              "imageUrl": "https://i.ibb.co/NdZX0SyN/2-2.jpg",
              "altText": "Step 2-2"
            }
          }
        ]
      },
      {
        "header": "", // Empty header for this section
        "widgets": [
          {
            "image": {
              "imageUrl": "https://i.ibb.co/dsfHTgwH/3.jpg",
              "altText": "Step 3"
            }
          }
        ]
      }
    ]
  };

  // Build the CardHeader using the updated configuration
  const header = CardService.newCardHeader()
      .setTitle(uiConfig.header.title)
      .setSubtitle(uiConfig.header.subtitle)
      .setImageUrl(uiConfig.header.imageUrl);

  // Set image type if specified, defaulting to SQUARE if not CIRCLE or unspecified
  if (uiConfig.header.imageType === "CIRCLE") {
    header.setImageStyle(CardService.ImageStyle.CIRCLE);
  } else {
    header.setImageStyle(CardService.ImageStyle.SQUARE);
  }

  // Initialize CardBuilder with the created header
  const cardBuilder = CardService.newCardBuilder()
      .setHeader(header);

  // Iterate through sections defined in the uiConfig and build them
  uiConfig.sections.forEach(sectionConfig => {
    const section = CardService.newCardSection();

    // Set section header if it exists and is not empty
    if (sectionConfig.header) {
      section.setHeader(sectionConfig.header);
    }

    // Set collapsible properties if present (though not in the current JSON, kept for flexibility)
    if (sectionConfig.collapsible) {
      section.setCollapsible(true);
      if (typeof sectionConfig.uncollapsibleWidgetsCount === 'number') {
        section.setNumUncollapsibleWidgets(sectionConfig.uncollapsibleWidgetsCount);
      }
    }

    // Iterate through widgets within each section and add them
    sectionConfig.widgets.forEach(widgetConfig => {
      if (widgetConfig.image) {
        // Handle image widgets
        const imageConfig = widgetConfig.image;
        const image = CardService.newImage()
            .setImageUrl(imageConfig.imageUrl);
        // Set alt text if provided for accessibility
        if (imageConfig.altText) {
          image.setAltText(imageConfig.altText);
        }
        section.addWidget(image);
      }
      // Removed textInput handling and onChangeAction as they are not in the new JSON.
      // Add more widget types (e.g., buttons, decoratedText) here if your JSON expands in the future.
    });
    cardBuilder.addSection(section); // Add the constructed section to the card
  });

  return cardBuilder.build(); // Build and return the complete card
}

/**
 * This function serves as the default entry point for the Gmail Add-on's homepage.
 * It calls createGmailAddonCard to construct and return the add-on's UI.
 *
 * @param {Object} e The event object, containing contextual information about the add-on invocation.
 * @return {GoogleAppsScript.Card_Service.Card} The constructed card to display in the Gmail Add-on sidebar.
 */
function onHomePageOpen(e) {
  // Simply return the card created by our main function
  return createGmailAddonCard(e);
}
