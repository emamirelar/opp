INSERT INTO public."AiPrompt" ("Type", "Prompt", "CreatedAt", "Name", "Status") VALUES
(1, 'I am providing an array of objects containing contact information, partner information and interaction history. Each object will have the contact details, partner detail and interaction detail in a flat structure. I need you to generate a summary in Markdown format, using the following template:

## Contact Summary

**Name:** [Contact Name]  
**Email:** [Contact Email]

**Key Interactions:**

*   **[Date of Interaction] - [Type of Interaction]:** [A brief summary of the interaction. 1-2 sentences.]

**Key Information about Partners**
* Partner Name

**Overall Summary:**

[A detailed paragraph providing an overview of the contact based on the interactions and partners, highlighting key themes, and sentiment.]

Instructions:

Extract the contacts name from the field Contacts_Name in the object. Similarly, all the Contact related fields will be prefixed with Contacts_. Partner related fields will be prefixed with Partners_ and Interaction related ones with Interactions_. .  

Extract the Date and use it for [Date of Interaction]. Format the timestamp in a human readable format.  
Determine the [Type of Interaction] based on the Type field. Use the following mapping:  
3: """"Note""""  
Decode the Base64 encoded Data field in the Interactions data.  
Create a brief 1-2 sentence summary of the interaction using the decoded Data and populate the [A brief summary of the interaction. 1-2 sentences.] field.  
Overall Summary: Based on all the interactions, create a 6-15 sentence paragraph providing an overall summary of the contact. Include key themes, sentiment (if discernible from the interaction data), and any potential needs or concerns that emerge from the interactions.  
Markdown Formatting: Ensure the entire summary is correctly formatted in Markdown.  
Focus: The primary focus of the summary should be to understand the general topics discussed and the tone of any interactions.  
If there are more than one contact or interaction, mention about it.  
Also, mention some details about the partner.

JSON Data:

{jsonData}

Please provide the generated Markdown summary based on these instructions. If any detail that you are instructed to provide is unavailable, mention that this detail is unavailable."""', '2025-03-09 23:37:52.959573+01', 'Contacts', 1),

(2, 'I am providing an array of objects containing contact information, partner information and interaction history. Each object will have the contact details, partner detail and interaction detail in a flat structure. I need you to generate a summary in Markdown format, using the following template:

## Contact Summary

**Name:** [Contact Name]  
**Email:** [Contact Email]

**Key Interactions:**

*   **[Date of Interaction] - [Type of Interaction]:** [A brief summary of the interaction. 1-2 sentences.]

**Key Information about Partners**
* Partner Name

**Overall Summary:**

[A detailed paragraph providing an overview of the contact based on the interactions and partners, highlighting key themes, and sentiment.]

Instructions:

Extract the contacts name from the field Contacts_Name in the object. Similarly, all the Contact related fields will be prefixed with Contacts_. Partner related fields will be prefixed with Partners_ and Interaction related ones with Interactions_. .  

Extract the Date and use it for [Date of Interaction]. Format the timestamp in a human readable format.  
Determine the [Type of Interaction] based on the Type field. Use the following mapping:  
3: """"Note""""  
Decode the Base64 encoded Data field in the Interactions data.  
Create a brief 1-2 sentence summary of the interaction using the decoded Data and populate the [A brief summary of the interaction. 1-2 sentences.] field.  
Overall Summary: Based on all the interactions, create a 6-15 sentence paragraph providing an overall summary of the contact. Include key themes, sentiment (if discernible from the interaction data), and any potential needs or concerns that emerge from the interactions.  
Markdown Formatting: Ensure the entire summary is correctly formatted in Markdown.  
Focus: The primary focus of the summary should be to understand the general topics discussed and the tone of any interactions.  
If there are more than one contact or interaction, mention about it.  
Also, mention some details about the partner.

JSON Data:

{jsonData}

Please provide the generated Markdown summary based on these instructions. If any detail that you are instructed to provide is unavailable, mention that this detail is unavailable."""', '2025-03-09 23:37:52.959573+01', 'Partners', 1);

INSERT INTO public."AiScreenMapping" ("Type", "TableName", "ComparisonKey", "RelatedEntity", "RelatedEntityKey", "QueryConditions", "Order", "CreatedAt", "Name", "Status") VALUES 
('contacts_summary', 'Contacts', 'Id', 'Interactions', 'ContactId', NULL, 1, NOW(), 'Contacts', 1),
('contacts_summary', 'Contacts', 'PartnerId', 'Partners', 'Id', NULL, 2, NOW(), 'Contacts', 1),
('partner_interactions_summary', 'Partners', 'Id', 'Contacts', 'PartnerId', NULL, 1, NOW(), 'Partners', 1),
('partner_interactions_summary', 'Contacts', 'Id', 'Interactions', 'ContactId', NULL, 2, NOW(), 'Partners', 1);