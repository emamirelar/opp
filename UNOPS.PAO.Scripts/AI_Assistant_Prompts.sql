TRUNCATE TABLE public."AiPrompt";

INSERT INTO public."AiPrompt"("Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", "GenerationConfig", "Location", "Model", "Project") VALUES
('contacts_summary', 'I am providing an array of objects containing contact information, partner information and interaction history. Each object will have the contact details, partner detail and interaction detail in a flat structure. I need you to generate a summary in Markdown format, using the following template:

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

{promptData}

Please provide the generated Markdown summary based on these instructions. If any detail that you are instructed to provide is unavailable, mention that this detail is unavailable."""',
NOW(), 'Contacts', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }'
, 'europe-west3', 'gemini-1.5-flash-001', 'unops-partneropportunity'),
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

{promptData}

Please provide the generated Markdown summary based on these instructions. Add additional line space after each detail. If any detail that you are instructed to provide is unavailable, do not include that in the response. The final response from you should give me a quick summary of the partner. Do not assume any detail.', NOW(), 'Partners'
, 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west3', 'gemini-1.5-flash-001', 'unops-partneropportunity'),
('partner_risk_profile', 'I need to determine the risk profile of a partner based on their involvement in one or more projects. I will provide a JSON object containing partner details in the partners array and project details in the projects array. A partner may be involved in multiple projects, linked by the PartnerId field in the projects array matching the Id field in the partners array.

**Risk Factors to Consider:**

  **Financial Risk:** Consider financial risk based on budget overruns (ExpenditureAmount exceeding BudgetAmount), project delays potentially leading to increased costs.
  **Project Execution Risk:** Assess risk related to project delays (StartDate vs. current date, EndDate in the future but project Status is "Delayed"), project status (Cancelled projects are high risk), and project stage (projects in early stages may have higher uncertainty).  Projects with a status of "Cancelled" should be considered high risk.
  **Reputational Risk:** Consider the potential for reputational damage based on project failures or delays.
  **Compliance Risk:** Consider the compliance risk based on DDRequired and NewEngagement fields.

**Desired Output Format:**

**Name:** [Partner Name] (Get this from the partners property from the JSON) (give 2 line breaks)

Provide a Markdown-formatted summary of risk profile of partner, including:

  Overall Risk Assessment (High, Medium, Low).
  A brief explanation of the factors contributing to the risk assessment.
  Specific examples from the provided data that support the assessment (e.g., "Project X has a significant budget overrun, with expenditure exceeding the budget by Y amount").
  Recommendations for mitigating the identified risks.
  Table summarizing the risk with impact and likelihood (add more tab spaces in this table to avoid overwriting of text)

**Constraints/Assumptions:**

  The partner data does not include information on their financial performance beyond the project level. Therefore, financial risk assessment will be limited to project-specific budget overruns.
  If EndDate of a Project is missing, assume it is ongoing and calculate the duration from the StartDate to the current date.
  Prioritize risks from projects with larger budget amounts.
  If DDRequired is "Yes" and NewEngagement is "Not Allowed" this would indicate higher risk for the partner.

Now, here is the JSON data:

{promptData}

Please provide the generated Markdown summary based on these instructions. Add additional line space after each detail. If any detail that you are instructed to provide is unavailable, do not include that in the response. Do not assume any detail.'
, NOW(), 'Partners', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west3', 'gemini-1.5-flash-001', 'unops-partneropportunity'),
('general_information', '{promptData}

Strictly return the response in JSON format as below - 

{Category: "General", ResponseType: "INFORMATION", Message: "Add your response here"}', NOW(), 'General', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west3', 'gemini-1.5-flash-001', 'unops-partneropportunity'),
('entity_intent_detection', 'I am going to send you a message from the user. Your task is to extract the most accurate and closest entity and intent of the user. NOTE that the same word or set of words may be associated with MORE THAN ONE entity. Use your knowledge to extract the right entity.

Entity lists can be: 
Contact
Parter
PartnerTree
Interaction
General

Intent can be:
Action
Information

It is considered to be General if you are not able to derive any entity. 
If the Entity is General, the intent should always be considered as Information.
It is considered an Action if there is anything related to creation or updation or deletion of an entity other than General.


Result should be strictly in JSON format as follows:
{
	Entity: Name of the entity derived from the list of entities provided
	Intent: Derived intent
	Message: Add a response message to the user
	Type: Derive the type by concatenating Entity and Intent with _ (all in lowercase)
	Forward: If the intent is Action but there is no information about the entity provided in the prompt or the Intent is Information with Entity other than General, then send it as false. Otherwise send it as true.
}

Consider the following example:

Prompt: Can you create a contact for me?
Response: 
{
	Entity: ''Contact'',
	Intent: ''Action'',
	Message: ''Sure, can you give me the details of the contact.''
	Type: ''contact_action''
	Forward: ''No''
}

Prompt: Anusha Swaminathan, UNOPS, anushas@unops.org, 12345
Response:
{
	Entity: ''Contact'',
	Intent: ''Information'',
	Message: ''These look like details of a Contact. Do you want to create a contact with these details?''
	Type: ''contact_information''
	Forward: ''No''
}

Prompt: Yes (continuation of previous chat)
Response: 
{
	Entity: ''Contact'',
	Intent: ''Action'',
	Message: ''Action completed successfully.''
	Type: ''contact_action''
	Forward: ''Yes''
}

Make sure to refer to the complete conversation to understand the current context. With the above instruction and examples, following is the prompt from the user:
Prompt: {promptData}', NOW(), 'EntityDetection', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west3', 'gemini-1.5-flash-001', 'unops-partneropportunity'), 
('contact_action', 'I am sending you some data/information in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

Raw Data:
{promptData}

JSON format:
{"Message": "Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message", "Category": "Contact", ResponseType: "Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION", "salutation": ", "firstName": ", "middleName": "", "lastName": "", "suffix": "", "title": "", "pronouns": "", "birthDate": "", "email": "", "phone": "", "mobile": "", "otherPhone": "", "fax": "", "partner": "", "department": "", "description": "", "status": "", "contactNumber": "", "assistant": "", "assistantPhone": "", "assistantEmail": "", "mailingStreet": "", "mailingStreet2": "", "mailingCity": "", "mailingStateProvince": "", "mailingPostalCode": "", "mailingCountry": "" }

Somethings to consider about the JSON format above are:
partner is the organization where the contact works"', NOW(), 'Contacts', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west3', 'gemini-1.5-flash-001', 'unops-partneropportunity'),
('partner_action', 'I am sending you partner data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

Raw Data:
{promptData}

JSON format:
{ "Message": "Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message", "Category": "Partner", ResponseType: "Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION", "name": "", "status": "", "newEngagement": "", "phone": "", "website": "", "shortName": "", "internalReportingLevel": "", "externalReportingLevel": "", "pooledFund": "", "ddRequired": "", "ddeacDone": "", "eacReference": "", "globalKeyAccount": "", "unSecretariatEntity": "", "levyPotentiallyApplies": "", "reasonForLevyNotApplying": "", "levyTreatment": "", "scope": "", "address1Street": "", "address1Street2": "", "address1City": "", "address1StateProvince": "", "address1PostalCode": "", "address1Country": "", "address2Street": "", "address2Street2": "", "address2City": "", "address2StateProvince": "", "address2PostalCode": "", "address2Country": "" }

Somethings to consider about the JSON format above are:
Acceptable values for "newEngagement" are: "Allowed", "Not Allowed"
Accpetable values for "internalReportingLevel" are: "1", "2", "3", "4", "5", "6"
Accpetable values for "externalReportingLevel" are: "1", "2", "3", "4", "5", "6"
Accpetable values for "pooledFund" are: "Yes", "No"
Accpetable values for "ddRequired" are: "Yes", "No"
Accpetable values for "ddeacDone" are: "Yes", "No"
Accpetable values for "globalKeyAccount" are: true, false
Accpetable values for "unSecretariatEntity" are: true, false 
Accpetable values for "levyPotentiallyApplies" are: "Potentially does not apply", "Does not apply", "Potentially applies"
Accpetable values for "reasonForLevyNotApplying" are: "3a) Vertical Fund", "3d) International Financial Institution", "3c) Programme Country", "4) Pooled Fund", "3b) Funds from UN entity", "3a / 4) Vertical Fund / Pooled Fund", "6) Thematic Fund"
Accpetable values for "levyTreatment" are: "Please consult funding source", "UNOPS administers", "Funding source administers directly (no changes required to the partner agreement)", "N/A"
Accpetable values for "scope" are: "Global", "Regional", "Local"', NOW(), 'Partners', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west3', 'gemini-1.5-flash-001', 'unops-partneropportunity'),
('partnertree_action', 'I am sending you partner tree (level) data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

Raw Data:
{promptData}

JSON format:
{ ""Message"": ""Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message"", ""Category"": ""PartnerTree"", ResponseType: ""Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION"", ""description"": """", ""code"": """", ""type"": """", ""parent"": """", ""name"": """" }

Somethings to consider about the JSON format above are:
""code"" looks like an ID field but text which will be similar to ""ACADEMIC_TRAINING_RESEARC"". If you cannot find a data in such a format, autogenerate a code of the similar kind based on the name and description you extract.
""parent"" is also look-alike of code but the code of the parent. If you cannot find it in the data, leave it blank. If parent is left blank, consider ""type"" as Level_1 and mention it in the Message.
""type"" can be Level_1, Level_2, Level_3 or Level_4. Level_1 will always have parent as blank.
', NOW(), 'PartnerTrees', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west3', 'gemini-1.5-flash-001', 'unops-partneropportunity');