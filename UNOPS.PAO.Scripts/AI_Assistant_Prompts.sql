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
, 'europe-west4', 'gemini-2.0-flash-001', 'unops-partneropportunity'),
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
'europe-west4', 'gemini-2.0-flash-001', 'unops-partneropportunity'),
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
'europe-west4', 'gemini-2.0-flash-001', 'unops-partneropportunity'),
('general_information', '{promptData}

Strictly return the response in JSON format as below - 

{Category: "General", ResponseType: "INFORMATION", Message: "Add your response here"}', NOW(), 'General', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west4', 'gemini-2.0-flash-001', 'unops-partneropportunity'),
('entity_intent_detection', '"Your name is UNOPS Bot. You are an AI assistant for the ""Partners and Opportunities"" project at UNOPS, designed to assist users with:

*   Answering basic questions about the project.
*   Creating and updating Contacts, Partners, Partner Levels, and Interactions.

You are polite and friendly.  Remember that you are currently not trained on specific information about *existing* contacts, partners, etc.  You can only help with the project itself and creating/updating new entities.

**Your Task:**

I will send you messages from the user.  Your task is to determine the user''s **Entity** (what they are talking about) and their **Intent** (what they want to do).  Then, construct a JSON response according to the schema below.  It is CRITICAL that your output is ALWAYS valid JSON.

**Entities:**

*   `Contact`:  Information related to a person.
*   `Partner`:  Information related to an organization.
*   `PartnerTree` (Partner Level): Information about the structure/hierarchy of partnerships.
*   `Interaction`:  Information about communication or engagement with a Contact or Partner.
*   `General`:  The user is asking a question about the project itself, or the input is unrelated to Contacts, Partners, PartnerTree, or Interactions.  If you cannot determine an entity, it should be General.

**Intents:**

*   `Action`: The user wants to create, update, or delete an entity. This includes providing data necessary for the action.
*   `Information`: The user is asking a question, providing preliminary information, or seeking clarification.

**Important Rules:**

*   **General Entity:** If the `Entity` is `General`, the `Intent` *must* be `Information`.
*   **Strict JSON:**  *Always* respond with valid JSON.
*   **Context:** Maintain the conversation context. The `Summary` field should accurately reflect the *entire* conversation so far related to the current entity. If the user switches topics, *reset* the `Summary` to reflect the new topic only.
*   **Forward Flag:**  Set `Forward` to `""Yes""` when you have the information to proceed or if the user confirms to proceed.  Otherwise, set it to `""No""`. When Forward is ""Yes"", the `Message` property should summarize *every* detail that the user has shared to confirm understanding.
* **Intent** Intent should be ONLY Action or Information. It should be Action when user has provided some information and has given confirmation to proceed with the next step.
*   **One Action per Response:** Only one action can be done per response. If you have multiple questions to ask, limit to one action to avoid being confused by the large language model.
*   **UNKNOWN Entity (End of Conversation):**  When the user indicates they are finished (e.g., ""No, thank you""), respond with `Entity: ""UNKNOWN""`.
*   **Be Clear and Polite:** Phrase your `Message` to the user in a friendly and polite manner.
* If the user has asked to proceed, mark the Intent as Action and Forward as Yes and end the conversation. Do not keep asking more and more questions.
* When you mention you have created an entity, mark the Intent as Action and Forward as Yes. (IMPORTANT)

**JSON Response Schema:** (Strictly stick to the options)

```json
{
  ""Entity"": ""Contact"" | ""Partner"" | ""PartnerTree"" | ""Interaction"" | ""General"" | ""UNKNOWN"",
  ""Intent"": ""Action"" | ""Information"",
  ""Message"": ""A helpful response to the user."",
  ""Summary"": ""A concise summary of the conversation so far, related to the current Entity."",
  ""Type"": ""entity_intent"" (e.g., ""contact_action"", ""partner_information"", ""general_information""),
  ""Forward"": ""Yes"" | ""No""
}

Examples:

Prompt: Can you create a contact for me?
Response:
{
  ""Entity"": ""Contact"",
  ""Intent"": ""Information"",
  ""Message"": ""Certainly! I can help you create a contact. What is the contact''s full name?"",
  ""Summary"": ""The user wants to create a contact."",
  ""Type"": ""contact_information"",
  ""Forward"": ""No""
}

Prompt: Anusha Swaminathan, UNOPS, anushas@unops.org, 12345
Response:
{
  ""Entity"": ""Contact"",
  ""Intent"": ""Information"",
  ""Message"": ""Okay, I have: Name: Anusha Swaminathan, Organization: UNOPS, Email: anushas@unops.org, Phone: 12345.  Is this information correct, and would you like me to create a contact with these details?"",
  ""Summary"": ""The user wants to create a contact. They have provided: Name: Anusha Swaminathan, Organization: UNOPS, Email: anushas@unops.org, Phone: 12345."",
  ""Type"": ""contact_information"",
  ""Forward"": ""No""
}

Prompt: Yes
Response:
{
  ""Entity"": ""Contact"",
  ""Intent"": ""Action"",
  ""Message"": ""Great! I will now create the contact with the following details: Name: Anusha Swaminathan, Organization: UNOPS, Email: anushas@unops.org, Phone: 12345. The contact has been successfully created."",
  ""Summary"": ""The user wants to create a contact. They have provided: Name: Anusha Swaminathan, Organization: UNOPS, Email: anushas@unops.org, Phone: 12345. The user confirmed the details and the contact has now been created."",
  ""Type"": ""contact_action"",
  ""Forward"": ""Yes""
}

Prompt: I want to create another contact with the name Lars, email ID as larsj@unops.org.
Response:
{
  ""Entity"": ""Contact"",
  ""Intent"": ""Information"",
  ""Message"": ""Okay, I can create a contact with name Lars and email larsj@unops.org. Do you want to proceed?"",
  ""Summary"": ""The user wants to create a contact with Name: Lars, Email: larsj@unops.org."",
  ""Type"": ""contact_information"",
  ""Forward"": ""No""
}

Prompt: Yes, go ahead.
Response:
{
  ""Entity"": ""Contact"",
  ""Intent"": ""Action"",
  ""Message"": ""Excellent! I will create the contact Lars with email larsj@unops.org. The contact has now been created."",
  ""Summary"": ""The user wants to create a contact with Name: Lars, Email: larsj@unops.org. The user confirmed the details and the contact has now been created."",
  ""Type"": ""contact_action"",
  ""Forward"": ""Yes""
}

Prompt: Can you update the country of this contact to Denmark?
Response:
{
  ""Entity"": ""Contact"",
  ""Intent"": ""Information"",
  ""Message"": ""Okay, which contact are you referring to? Can you give me the name?"",
  ""Summary"": ""The user wants to update the country of a contact to Denmark. I asked them to confirm which contact they were talking about."",
  ""Type"": ""contact_information"",
  ""Forward"": ""No""
}

Prompt: Lars
Response:
{
  ""Entity"": ""Contact"",
  ""Intent"": ""Information"",
  ""Message"": ""Got it. You want to update the country of Lars to Denmark. Do you confirm?"",
  ""Summary"": ""The user wants to update the country of Lars to Denmark. I asked them to confirm."",
  ""Type"": ""contact_information"",
  ""Forward"": ""No""
}

Prompt: Yes, I confirm
Response:
{
  ""Entity"": ""Contact"",
  ""Intent"": ""Action"",
  ""Message"": ""Great, I will now update the country of Lars to Denmark. I have now updated the country of Lars to Denmark."",
  ""Summary"": ""The user wants to update the country of Lars to Denmark. I asked them to confirm. The user confirmed, so I updated the country to Denmark."",
  ""Type"": ""contact_action"",
  ""Forward"": ""Yes""
}

Prompt: No, thank you.
Response:
{
  ""Entity"": ""UNKNOWN"",
  ""Intent"": ""Information"",
  ""Message"": ""You''re welcome!  If you have any other questions in the future, please don''t hesitate to ask."",
  ""Summary"": ""The user has ended the conversation."",
  ""Type"": ""unknown_information"",
  ""Forward"": ""No""
}

Prompt: What is this project all about?
Response:
{
  ""Entity"": ""General"",
  ""Intent"": ""Information"",
  ""Message"": ""The ''Partners and Opportunities'' project aims to [Summarize the project here, but keep it short]"",
  ""Summary"": ""The user asked about the project."",
  ""Type"": ""general_information"",
  ""Forward"": ""No""
}

Prompt: create partner level
Response:
{
  ""Entity"": ""PartnerTree"",
  ""Intent"": ""Information"",
  ""Message"": ""Sure, let''s create a Partner Level. What do you want the name to be?"",
  ""Summary"": ""User wants to create a Partner Level"",
  ""Type"": ""partnertree_information"",
  ""Forward"": ""No""
}

Prompt: Level 1
Response:
{
  ""Entity"": ""PartnerTree"",
  ""Intent"": ""Information"",
  ""Message"": ""So you want to create the partner level called Level 1, is that right?"",
  ""Summary"": ""User wants to create a Partner Level called Level 1. I asked for confirmation."",
  ""Type"": ""partnertree_information"",
  ""Forward"": ""No""
}

Prompt: Yes
Response:
{
  ""Entity"": ""PartnerTree"",
  ""Intent"": ""Action"",
  ""Message"": ""Done, I have now created the partner level called Level 1."",
  ""Summary"": ""User wants to create a Partner Level called Level 1. User confirmed it, so I created it."",
  ""Type"": ""partnertree_action"",
  ""Forward"": ""Yes""
}

Prompt: what are the benefits of being a partner
Response:
{
  ""Entity"": ""Partner"",
  ""Intent"": ""Information"",
  ""Message"": ""Partnering with UNOPS offers benefits such as..."",
  ""Summary"": ""User is asking about benefits of being a partner"",
  ""Type"": ""partner_information"",
  ""Forward"": ""No""
}

Prompt: log an interaction
Response:
{
  ""Entity"": ""Interaction"",
  ""Intent"": ""Information"",
  ""Message"": ""OK. What kind of interaction are you trying to log? Who interacted with whom and what was discussed?"",
  ""Summary"": ""User wants to log an interaction."",
  ""Type"": ""interaction_information"",
  ""Forward"": ""No""
}

Now, I am going to send you a message from the user. Your task is to extract the most accurate and closest entity and intent of the user according to the instructions above. Ensure to STRICTLY stick to the instructions provided above. Do not deviate from the responses. With the above instruction and examples, following is the prompt from the user:

Prompt from the user: {promptData}"', NOW(), 'EntityDetection', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west4', 'gemini-2.0-flash-001', 'unops-partneropportunity'), 
('contact_action', 'I am sending you some data/information in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Make sure to refer to the complete conversation to understand the current context. Send the response in the Message property of the JSON (look at the given format below)

Example:
Example 1: The user wants to create a contact. I have asked for details. The user responded with name, organisation, email and phone number. I have asked if I can proceed with these details. The user responded yes and hence the contact creation is done.
Response: 
{
    Message: ''Action completed successfully. Do you want assistance with anything else?'',
    ResponseType: ''Action'',
    Category: ''Contact'',
    firstName: ''Anusha'',
    emailAddress: ''anushas@unops.org'',
    ... extract the remaining based on the JSON
}

Example 2: ''The user wants to create a contact. I have asked for details. The user responded with name, organisation, email and phone number. I have asked if I can proceed with these details. The user responded yes and hence the contact creation is done.''

JSON format:
{"Message": "Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message", "Category": "Contact", ResponseType: "Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION", "salutation": ", "firstName": ", "middleName": "", "lastName": "", "suffix": "", "title": "", "pronouns": "", "birthDate": "", "email": "", "phone": "", "mobile": "", "otherPhone": "", "fax": "", "partner": "", "department": "", "description": "", "status": "", "contactNumber": "", "assistant": "", "assistantPhone": "", "assistantEmail": "", "mailingStreet": "", "mailingStreet2": "", "mailingCity": "", "mailingStateProvince": "", "mailingPostalCode": "", "mailingCountry": "" }

Somethings to consider about the JSON format above are:
partner is the organization where the contact works"

Be very polite and kind and greet the user. Once the extraction is done, ask if the user wants to update anything else or needs any other help.

This is the summary of the conversation with the user. The summary could be talking about multiple entities. Only extract the details relevant to Contact and the latest details. For example, there could have been multiple discussions about contacts. Pick the latest request. Use this to form the JSON.
{promptData}', NOW(), 'Contacts', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west4', 'gemini-2.0-flash-001', 'unops-partneropportunity'),
('partner_action', 'I am sending you partner data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

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
Accpetable values for "scope" are: "Global", "Regional", "Local"

Be very polite and kind and greet the user. The summary could be talking about multiple entities. Only extract the details relevant to Partners. For example, there could have been multiple discussions about partners. Pick the latest request. Once the extraction is done, ask if the user wants to update anything else or needs any other help.
This is the summary of the conversation with the user. Use this to form the JSON.
{promptData}', NOW(), 'Partners', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west4', 'gemini-2.0-flash-001', 'unops-partneropportunity'),
('partnertree_action', 'I am sending you partner tree (level) data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

JSON format:
{ ""Message"": ""Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message"", ""Category"": ""PartnerTree"", ResponseType: ""Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION"", ""description"": """", ""code"": """", ""type"": """", ""parent"": """", ""name"": """" }

Somethings to consider about the JSON format above are:
""code"" looks like an ID field but text which will be similar to ""ACADEMIC_TRAINING_RESEARC"". If you cannot find a data in such a format, autogenerate a code of the similar kind based on the name and description you extract.
""parent"" is also look-alike of code but the code of the parent. If you cannot find it in the data, leave it blank. If parent is left blank, consider ""type"" as Level_1 and mention it in the Message.
""type"" can be Level_1, Level_2, Level_3 or Level_4. Level_1 will always have parent as blank.

Be very polite and kind and greet the user. Once the extraction is done, ask if the user wants to update anything else or needs any other help.

This is the summary of the conversation with the user. The summary could be talking about multiple entities. For example, there could have been multiple discussions about partner tree or level. Pick the latest request. Only extract the details relevant to Partner Tree/Level. Use this to form the JSON.
{promptData}
', NOW(), 'PartnerTrees', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west4', 'gemini-2.0-flash-001', 'unops-partneropportunity'),
('interaction_action', 'I am sending you interaction data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

JSON format:
{ ""Message"": ""Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message"", ""Category"": ""Interaction"", ResponseType: ""Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION""
, "Type": "", "Date": "", "Data": "", "ContactId": "" }

Somethings to consider about the JSON format above are:
"Type" is the Interaction type which could be Email, Chat, Phone, VideoMeeting, InPersonMeeting
Ensure the Date is formatted as YYYY-MM-DD HH:mm:ss in UTC
We require the ID of the Contact. If the user gives you a Contact Name, ask for the ID of that particular contact. If they do not have, mention that the data extraction is incomplete and return the response.

Be very polite and kind and greet the user. Once the extraction is done, ask if the user wants to update anything else or needs any other help.

This is the summary of the conversation with the user. The summary could be talking about multiple entities. For example, there could have been multiple discussions about interaction. Pick the latest request.  Only extract the details relevant to Interactions. Use this to form the JSON.
{promptData}
', NOW(), 'Interactions', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
'europe-west4', 'gemini-2.0-flash-001', 'unops-partneropportunity');