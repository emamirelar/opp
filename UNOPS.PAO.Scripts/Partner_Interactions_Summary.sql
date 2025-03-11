DELETE FROM public."AiPrompt"
WHERE "Type" = 'partner_interactions_summary';
INSERT INTO public."AiPrompt" ("Type", "Prompt", "CreatedAt", "Name", "Status") VALUES
('partner_interactions_summary', 'I am providing a JSON object containing partner information, contact information and interaction history. The Each object will have the contact details with the property "contacts", partner detail with the property "partners" and interaction detail in "interactions" in a flat structure. I need you to generate a summary in Markdown format, using the following template:


**Name:** [Partner Name] (Get this from the partners property from the JSON) (give 2 line breaks)
**Available from:** [CreatedDate] (Convert this to DD/MM/YYYY)

**Key Interactions:**

Use the details from the "interactions" property of the JSON to summarize about all the interactions that happened/is about to happen. Show the details in bullet points for better readability.
For example, the summary of an interaction should look like this: 

- **Interaction Type - Chat:** On 19/09/2024, a chat interaction was initiated by **Beth Hayes**. The chat discussed the need for a significant reduction in energy-efficient procedures, which could be achieved through enterprise-level investments. (give 2 line breaks after each interaction summary)

(After finishing all the interaction summary, give 2 line breaks)

**Overall Summary:**

[A detailed paragraph providing an overview of the contact based on the interactions and partners, highlighting key themes, and sentiment.]

Instructions:
 
Markdown Formatting: Ensure the entire summary is correctly formatted in Markdown.  
Focus: The primary focus of the summary should be to understand the general topics discussed and the tone of any interactions.  
If there are more than one contact or interaction, mention about it.  
Also, mention some details about the partner.


JSON Data:

{jsonData}

Please provide the generated Markdown summary based on these instructions. Add additional line space after each detail. If any detail that you are instructed to provide is unavailable, do not include that in the response. The final response from you should give me a quick summary of the partner. Do not assume any detail.', NOW(), 'Partners', 1);

DELETE FROM public."AiScreenMapping"
WHERE "Type" = 'partner_interactions_summary';
INSERT INTO public."AiScreenMapping" ("Type", "TableName", "ComparisonKey", "RelatedEntity", "RelatedEntityKey", "QueryConditions", "Order", "CreatedAt", "Name", "Status") VALUES 
('partner_interactions_summary', 'Partners', 'Id', 'Contacts', 'PartnerId', NULL, 1, NOW(), 'Partners', 1),
('partner_interactions_summary', 'Contacts', 'Id', 'Interactions', 'ContactId', NULL, 2, NOW(), 'Partners', 1);