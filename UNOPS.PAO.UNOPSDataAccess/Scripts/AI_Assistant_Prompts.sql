TRUNCATE TABLE public."AiPrompt";

-- Reset the sequence to start from 1
ALTER SEQUENCE public."AiPrompt_Id_seq" RESTART WITH 1;



INSERT INTO public."AiPrompt"("Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", "GenerationConfig", "Location", "Model", "Project", "SafetySettings", "ToolsConfig", "PromptFunction", "Description", "AdminCanChange") VALUES
('contact_interactions_summary', 'I am providing a contact name. I need you to generate a summary in Markdown format, using the following template:

## Contact Summary

**Name:** [Contact Name]  
**Email:** [Contact Email]
**Title:** [Contact Title]
**Relationship:** [Brief description of time UNOPS has engaged with the Contact and main things UNOPS has done with the contact]

**Key Interactions:**

*   **[Date of Interaction] - [Type of Interaction]:** [Brief description of interaction]
*   **[Date of Interaction] - [Type of Interaction]:** [Brief description of interaction]

**Partner Information:**

*   **Organization:** [Partner Name]
*   **Status:** [Partner Status]

**Considerations**
** [Summary of any issues identified with the Contact or the Partner] 

**Additional Notes:**

*   [Check if there is a CV linked to the contact]
*   [Any other relevant information about the contact or their interactions]

Please format the response as clean Markdown without code blocks or backticks. Please try to fill in with as much information as available in the contact''s page. Please surface the Partner related to the contact, his/her title, email, and any other information available related to this contact in the system. If any information is missing, simply omit that section.

Data: {promptData}', NOW(), 'Contact', 1, '{"role":"user","parts":[{"text":"{promptData}"}]}', '{"temperature":0.1,"top_p":0.2,"max_output_tokens":8192}', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', NULL, '[]', 'GetContactWithInteractionsAsync', 'Generates a comprehensive summary of contact information including partner details and interaction history in a structured format.', true),

('bulk_partner_action', 'You are an AI assistant. You will receive partner data as an array of arrays (with optional header) or an array of objects. The first row could optionally be headers. Convert each item into the exact JSON structure shown. Include *all* fields present in the input data in the output JSON, unless a field is explicitly specified to be excluded. Only include non-empty fields. Map "partner category" or related terms to partnerCategoryId, else leave blank. If you find a number, return partnerCategoryId as a number (integer). Map "partner office" or related terms to partnerOfficeId, else leave blank. If you find a number, return partnerOfficeId as a number (integer). Always include "dependents":["partnerCategoryId", "partnerOfficeId"] as-is. Do NOT replace it with the partnerId value in the dependents but just "partnerId". It could also be a text extracted from an audio or an image representing partner details. You should use your knowledge and expertise to detect that and find out the partner details.

Partner format: {address1City: null, address1Country: null, address1PostalCode: null,address1StateProvince: null, address1Street: null, address1Street2: null, ddRequired: "Yes/No", ddeacDone: "Yes/No", eacReference: null, globalKeyAccount: true/flase, id: null, levyPotentiallyApplies: "Potentially does not apply/Does not apply/Potentially applies", levyTreatment: "Please consult funding source/UNOPS administers/Funding source administers directly (no changes required to the partner agreement)/N/A", name: null, newEngagement: "Allowed/Not Allowed", logoUrl: null, partnerCategoryId: null, partnerOfficeId: null, phone: null, pooledFund: "Yes/No", reasonForLevyNotApplying: "3a) Vertical Fund/3d) International Financial Institution/3c) Programme Country/4) Pooled Fund/3b) Funds from UN entity/3a / 4) Vertical Fund / Pooled Fund/6) Thematic Fund", shortName: null, status: "Inactive/Active/Locked", unSecretariatEntity: true/false, website: null, dependents: ["partnerCategoryId", "partnerOfficeId"], validationError: ""}

Following are the mandatory fields from the above format - partnerOfficeId, partnerCategoryId, name, status (default to Active), shortName, pooledFund, ddRequired, ddeacDone, levyPotentiallyApplies. *However, if additional fields are present in the input data (beyond these mandatory fields), include them in the output as well.*

Use your knowledge to detect the name and short name. If any other field other than the mandatory field is undetectable, do not send it in the response. If the mandatory fields are missing, strictly send it as null.

Response format: {"Message":"Action completed successfully.","Category":"Partner","ResponseType":"Action","records":[...]}

Return the response in a compact, single-line JSON format without line breaks or unnecessary whitespace. This is critical for successful parsing. If more input is needed, set ResponseType to "Information".

Input data: {promptData}', NOW(), 'Partner', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 8192 }', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', NULL, NULL, '', 'Processes bulk partner data from arrays or objects, converting them into structured JSON format with validation of acceptable values and automatic field mapping.', true),

('partner_priorities', 'I am providing a JSON object containing partner information and the fact that I work in Senegal. 
Identify the name of the partner from that JSON data and using external sources such as google search, identify the key focus areas in international development, potentially available funding or commitments and potential entry points for UNOPS.
In additon provide an overview of crosscutting priorities as "Overarching Considerations:"

PLease use the following structure

**Focus Areas:**
For each of the focus areas, please use the following structure
**[Focus Area]**
**Focus: ** [Provide explanation of the partner''s focus area and thier approach]
**Budget/Expenditure Commitments: ** [Provide an overview of expenditure or commitments that are potentially available to UNOPS]
**Key UNOPS entry points:** [Provide an overview of how this aligns with UNOPS strategy and priorities and key entry points ]
(add 2 line breaks)


JSON Data:
{promptData}

STRICTLY do not use the word "markdown" when you convert the final result to Markdown. Please provide the generated Markdown summary based on these instructions. Add additional line space after each detail. If any detail that you are instructed to provide is unavailable, do not include that in the response. Do not assume any detail. Please do not include "```markdown\n" in the response.', NOW(), 'partner_priorities', 0, '{"role":"user","parts":[{"text":"{promptData}"}]}', '{"temperature":1,"top_p":0.2,"max_output_tokens":8192}', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]', '[{"googleSearch":{}}]', 'GetBasicPartnerDetailsAsync', 'Give an overview of partner priorities', true),

('partner_news', 'I am providing a JSON object containing partner information and that I work in [Org Unit].
Identify the name of the partner from that JSON data and find the latest development news articles on the partner. 
For sources, please use Google News as well as development news sites such as Devex and donor tracker. 
Stories are presented in order of newest to oldest. Try to avoid repeating the same stories from multiple sources.
Do not include any introductory phrases to your answer, just the desired format below and do not include padding between new lines.

**Desired Output Format:**

Here are the most recent news stories for [Partner Name] (bold text, get this from the partners property from the JSON) (give 2 line breaks)
### Global News
 (Text for is underlined Bold 18pt with an icon indicaing that the list can be expanded / collapsed. ) (line break)
This is followed by a list of the 5 most recent global news stories)

### Regional / Local news 
(Text for header is underlined Bold 18pt with with an icon indicaing that the list can be expanded / collapsed. ) (line break)
(This is followed by a list of the 5 most recent news stories releavant to my country. If no news stories are available from the last month my country, then please look news stories in my UN region)

For each news article please use the following layout.
**News Headline** Text is  bright green (add one line break)
**Description** Short summary of the news article (normal text in black), maximum 200 characters. Followed by **[hyperlink]** text contains the word "Read Article" with a hyperlink to open the specific news article in a separate browser page (insert line break)
Source Name - DateOfNews: (in dd mmm YYYY format)

(add 2 line breaks)

After the top 5 stories in each section, please include
**[Hyperlink]** (this text with the phrase "See more stories" with an embedded hyperlink to open a google news page searching for the partner with stories ordered from newest to oldest)

(add 2 line breaks)

JSON Data:
{promptData}

STRICTLY do not use the word "markdown" when you convert the final result to Markdown. Please provide the generated Markdown summary based on these instructions. If any detail that you are instructed to provide is unavailable, do not include that in the response. Do not assume any detail. Please do not include "```markdown\n" in the response.', NOW(), 'Partner', 1, '{"role":"user","parts":[{"text":"{promptData}"}]}', '{"temperature":0.1,"top_p":0.1,"max_output_tokens":8192}', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]', '[{"googleSearch":{}}]', 'GetBasicPartnerDetailsAsync', 'Searches for and summarizes the latest news articles about a partner organization, identifying current focus areas and trends from recent developments.', true),

('general_information', 'Strictly return the response in JSON format as below - 

{Category: "General", ResponseType: "INFORMATION", Message: "Add your response here"}', NOW(), 'AiAssistant', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 8192 }', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', NULL, NULL, '', 'Returns a general response in JSON format.', true),

('partnertree_action', 'I am sending you partner tree (level) data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

JSON format:
{ ""Message"": ""Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message"", ""Category"": ""PartnerTree"", ResponseType: ""Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION"", ""description"": """", ""code"": """", ""type"": """", ""parent"": """", ""name"": """" }

Somethings to consider about the JSON format above are:
""code"" looks like an ID field but text which will be similar to ""ACADEMIC_TRAINING_RESEARC"". If you cannot find a data in such a format, autogenerate a code of the similar kind based on the name and description you extract.
""parent"" is also look-alike of code but the code of the parent. If you cannot find it in the data, leave it blank. If parent is left blank, consider ""type"" as Level_1 and mention it in the Message.
""type"" can be Level_1, Level_2, Level_3 or Level_4. Level_1 will always have parent as blank.

STRICTLY do not use the word "markdown" while converting the final response to the final JSON.

Be very polite and kind and greet the user. Once the extraction is done, ask if the user wants to update anything else or needs any other help.

The prompt could be an extracted text from an audio or an image OR could be a summary of the conversation with the user. The summary could be talking about multiple entities. Only extract the details relevant to Partner level and the latest details. This is just a one time call to you, so your task is to just extract data from the provided information if possible. For example, there could have been multiple discussions about the partner levels. Pick the latest request. Use this to form the JSON. Whether the prompt is an extracted text or a summary will be highlighted before the message begins (for example: Summary: <summary> OR Extracted text: <extracted text>)

Prompt: 
{promptData}', NOW(), 'PartnerTree', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 8192 }', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', NULL, NULL, '', 'Extracts partner tree (level) information from raw data and formats it into structured JSON with auto-generated codes and hierarchical type determination.', true),

('contact_action', 'I am sending you some data/information in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Make sure to refer to the complete conversation to understand the current context. Send the response in the Message property of the JSON (look at the given format below)

Example:
Example 1: The user wants to create a contact. I have asked for details. The user responded with name, organisation, email and phone number. I have asked if I can proceed with these details. The user responded yes and hence the contact creation is done.
Response: 
{
    Message: ''Requested action initiated but please revalidate the details as AI can make mistakes. Do you want assistance with anything else?'',
    ResponseType: ''Action'',
    Category: ''Contact'',
    firstName: ''Anusha'',
    emailAddress: ''anushas@unops.org'',
    ... extract the remaining based on the JSON
}

Example 2: ''The user wants to create a contact. I have asked for details. The user responded with name, organisation, email and phone number. I have asked if I can proceed with these details. The user responded yes and hence the contact creation is done.''

STRICTLY do not use the word "markdown" while converting the final response to the final JSON.
JSON format:
{"Message": "Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message", "Category": "Contact", ResponseType: "Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION", "salutation": "", "firstName": "", "middleName": "", "lastName": "", "suffix": "", "title": "", "pronouns": "", "birthDate": "", "email": "", "phone": "", "mobile": "", "otherPhone": "", "fax": "", "partnerId": "", "department": "", "description": "", "status": "", "contactNumber": "", "assistant": "", "assistantPhone": "", "assistantEmail": "", "mailingStreet": "", "mailingStreet2": "", "mailingCity": "", "mailingStateProvince": "", "mailingPostalCode": "", "mailingCountry": ""
, dependents: ["partnerId"] }

Somethings to consider about the JSON format above are:
* Salutation is from the following list - Mr., Ms., Mrs., Dr., Prof. Based on the content received, auto detect the salutation. If not available, leave it blank.
* partner is the organization where the contact works"
* When there is no Last Name / Email / Partner detail detected, dont send it as empty string but as null.
* Contact can be linked to a Partner. The JSON must have a property called partner. Generally, the user will not know the ID of the Partner and hence will pass it as a Name. 
Put the name in the "partnerId" property value and add "partnerId" to the dependents property as an array. for example, dependents: ["partnerId"] (the string)
*When there is no Title provided by the user, ask them if they want to provide it.

Be very polite and kind and greet the user. Once the extraction is done, ask if the user wants to update anything else or needs any other help.

The prompt could be an extracted text from an audio or an image OR could be a summary of the conversation with the user. The summary could be talking about multiple entities. Only extract the details relevant to Contact and the latest details. For example, there could have been multiple discussions about contacts. Pick the latest request. Use this to form the JSON. Whether the prompt is an extracted text or a summary will be highlighted before the message begins (for example: Summary: <summary> OR Extracted text: <extracted text>)

Prompt: 
{promptData}', NOW(), 'Contact', 1, '{"role":"user","parts":[{"text":"{promptData}"}]}', '{"temperature":0.1,"top_p":0.2,"max_output_tokens":8192}', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', NULL, '[]', '', 'Extracts contact information from raw data or conversation summaries and formats it into structured JSON for contact creation or updates.', true),

('partner_action', 'I am sending you partner data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

JSON format:
{ "Message": "Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message", "Category": "Partner", ResponseType: "Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION", "name": "", "status": "", "newEngagement": "", "phone": "", "website": "", "shortName": "", "internalReportingLevel": "", "externalReportingLevel": "", "pooledFund": "", "ddRequired": "", "ddeacDone": "", "eacReference": "", "globalKeyAccount": "", "unSecretariatEntity": "", "levyPotentiallyApplies": "", "reasonForLevyNotApplying": "", "levyTreatment": "", "scope": "", "address1Street": "", "address1Street2": "", "address1City": "", "address1StateProvince": "", "address1PostalCode": "", "address1Country": "", "address2Street": "", "address2Street2": "", "address2City": "", "address2StateProvince": "", "address2PostalCode": "", "address2Country": ""}

Somethings to consider about the JSON format above are:
Acceptable values for "newEngagement" are: "Allowed", "Not Allowed"
Accepetable values for "internalReportingLevel" are: "1", "2", "3", "4", "5", "6"
Accepetable values for "externalReportingLevel" are: "1", "2", "3", "4", "5", "6"
Accepetable values for "pooledFund" are: "Yes", "No"
Accepetable values for "ddRequired" are: "Yes", "No"
Accepetable values for "ddeacDone" are: "Yes", "No"
Accepetable values for "globalKeyAccount" are: true, false
Accepetable values for "unSecretariatEntity" are: true, false 
Accepetable values for "levyPotentiallyApplies" are: "Potentially does not apply", "Does not apply", "Potentially applies"
Accepetable values for "reasonForLevyNotApplying" are: "3a) Vertical Fund", "3d) International Financial Institution", "3c) Programme Country", "4) Pooled Fund", "3b) Funds from UN entity", "3a / 4) Vertical Fund / Pooled Fund", "6) Thematic Fund"
Accepetable values for "levyTreatment" are: "Please consult funding source", "UNOPS administers", "Funding source administers directly (no changes required to the partner agreement)", "N/A"
Accepetable values for "scope" are: "Global", "Regional", "Local"

STRICTLY do not use the word "markdown" while converting the final response to the final JSON.

Be very polite and kind and greet the user. Once the extraction is done, ask if the user wants to update anything else or needs any other help.

The prompt could be an extracted text from an audio or an image OR could be a summary of the conversation with the user. The summary could be talking about multiple entities. Only extract the details relevant to Partner and the latest details. For example, there could have been multiple discussions about contacts. Pick the latest request. Use this to form the JSON. Whether the prompt is an extracted text or a summary will be highlighted before the message begins (for example: Summary: <summary> OR Extracted text: <extracted text>)

Prompt: 
{promptData}', NOW(), 'Partner', 1, '{"role":"user","parts":[{"text":"{promptData}"}]}', '{"temperature":0.1,"top_p":0.2,"max_output_tokens":8192}', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', NULL, '[]', '', 'Extracts partner information from raw data or conversation summaries and formats it into structured JSON for partner creation or updates with validation of acceptable values.', true),

('bulk_interaction_action', 'You are an AI assistant that processes interaction data for bulk import. You will receive interaction data as an array of arrays (with optional header) or an array of objects, or text extracted from audio/image.

Convert each item into the exact JSON structure shown below. Only include non-empty fields.

**Required fields:** type, date, subject
**Validation rules:**
- Map contact names to contactIds (keep as text if name, number if ID)
- Map partner names to partnerIds (keep as text if name, number if ID)
- Map user names to userIds (keep as text if name, number if ID)
- Format date as ISO 8601 timestamp (YYYY-MM-DDTHH:mm:ss.sssZ)
- Default status to "Active"
- Parse comma-separated emails into emailAddresses array
- Parse comma-separated phones into phoneNumbers array
- Include dependents for all ID fields that are text names
- Based on the context of the message, auto-detect the date.
- IDs starting with B-s are Org units.
- Send the dependents as-is (with the strings and not replace with the Ids). Don''t work on this property at all.
- Put one of the contactIds into contactId

**Interaction types:** "Email", "Chat", "Phone", "VideoMeeting", "InPersonMeeting", "Other"

**Interaction JSON format:**
{"id": null, "type": "", "date": "", "subject": "", "description": "", "status": "Active", "contactId": '''', "contactIds": [], "partnerIds": [], "userIds": [], "emailAddresses": [], "phoneNumbers": [], "location": "", "orgUnitId": null, "dependents": ["contactId", "contactIds", "partnerIds", "userIds", "orgUnitId"], "validationError": ""}

**Response format:** {"Message":"Action completed successfully.", "Category":"Interaction", "ResponseType":"Action", "records":[...]}

Return compact single-line JSON. If more input needed, set ResponseType to "Information".

Input data: {promptData}', NOW(), 'Interaction', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 8192 }', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', NULL, NULL, '', 'Processes bulk interaction data from arrays or objects, converting them into structured JSON format with automatic date parsing, field mapping, and validation of interaction types.', true),

('bulk_contact_action', 'You are an AI assistant. You will receive contact data as an array of arrays (with optional header) or an array of objects. 
The first row could optionally be headers. Convert each item into the exact JSON structure shown. Only include non-empty fields. Required: lastName, email, phone. Map "partner" or related terms to partnerId, else leave blank. If you find a number, return partnerId as a number (integer). Always include "dependents":["partnerId"] as-is. DONOT replace it with the partnerId value in the dependents but just "partnerId". It could also be a text extracted from an audio or an image representing contact details.
You should use your knowledge and expertise to detect that and find out the contact details. 
If the header is Name, use your knowledge to split it into firstName, middleName and lastName.

Contact format: {"id": null, "salutation":"","firstName":"","middleName":"","lastName":"","name":"","suffix":"","title":"","pronouns":"","birthDate":"","email":"","phone":"","mobile":"","otherPhone":"","fax":"","partnerId":"","department":"","description":"","status":"Active","contactNumber":"","assistant":"","assistantPhone":"","assistantEmail":"","mailingStreet":"","mailingStreet2":"","mailingCity":"","mailingStateProvince":"","mailingPostalCode":"","mailingCountry":"","dependents":["partnerId"],"validationError":""}
Salutation is from the following list - Mr., Ms., Mrs., Dr., Prof. Based on the content received, auto detect the salutation. If not available, leave it blank.
* Note that "name" is the concatenation of firstName, middleName and lastName.
* When there is no ID/ Last Name / Email / Partner detail detected, dont send it as empty string but as null.
* Any other field that is not detected, pass it as "".
* Always keep the status Active

Response format: {"Message":"Action completed successfully.","Category":"Contact","ResponseType":"Action","records":[...]}

Return the response in compact single-line JSON without line breaks or unnecessary whitespace. If more input is needed, set ResponseType to "Information".

Input data: {promptData}', NOW(), 'Contact', 1, '{"role":"user","parts":[{"text":"{promptData}"}]}', '{"temperature":0.1,"top_p":0.2,"max_output_tokens":8192}', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', NULL, '[]', '', 'Processes bulk contact data from arrays or objects, converting them into structured JSON format with automatic name parsing and partner linking.', true),

('partner_interactions_summary', 'I am providing a partner name. For example, the user may ask the question "Can you give me a summary of the latest interactions with [Partner]?"

I need you to generate a summary in Markdown format, using the following template:

###Partner Summary
Using the partner name produce a short 3 sentence summary of the organisation based on internal information, do not use any other internal data points from the partner record. 

##Summary of key interactions
Provide an introductory paragraph of interactions related to the partner in the last month. Highlight any key high-level interactions.

For example, using the following format, generate a summary of an interaction that looks like this: 

On 19/09/2024, Beth Hayes from org unit (in bold) had a Type of interaction. It was discussed the need for a significant reduction in energy-efficient procedures, which could be achieved through enterprise-level investments. (If the interaction is related to a project please indicate the country and number, and if not say "not related to a specific project").**See more**  (text contains the word "See more" with a hyperlink to open the specific interaction record). (give line breaks after each interaction summary)

##List of interactions

There have been several recent interactions between UNOPS and [Partner]
[Date of interaction]: A high-level meeting between UNOPS'' [Personnel name, Personnel title] and the World Bank''s [Contact name, contact title] to discuss ongoing projects. 
[Date of interaction]: a meeting between the World Bank and UNOPS'' project teams to discuss project [Engagement name, engagement code] and project process, where key milestones such as timely delivery of supplies were identified. 
[Date of interaction]: high-level meeting between the World Bank''s [Contact name, contact title], and the UNOPS delegation at the [Event name]
.**See more**  (text contains the word "See more" with a hyperlink to open the specific interaction record)

##Considerations
** [Summary of any issues identified with the Contact or the Partner] 

##Portfolio and Pipeline
** [Surface of internal data regarding UNOPS and World Bank Portfolio from dashboard: EA, and delivery key project examples and main regions where we operate, key impact figures beneficiaries, etc).] As a source, please use: OuP, partnerships dashboards, GDrive.
** [Surface of external data regarding World Bank Portfolio: total portfolio, portfolio per regions, and delivery key project examples and other key impact figures]

Instructions:
 
{promptData}

STRICTLY do not use the word "markdown" when you convert the final result to Markdown. Please provide the generated Markdown summary based on these instructions. Add additional line space after each detail. If any detail that you are instructed to provide is unavailable, do not include that in the response. The final response from you should give me a quick summary of the partner. Do not assume any detail. Please do not include "```markdown\n" in the response.', NOW(), 'Partner', 1, '{"role":"user","parts":[{"text":"{promptData}"}]}', '{"temperature":0.7,"top_p":0.2,"max_output_tokens":8192}', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', NULL, '[{"googleSearch":{}}]', 'GetPartnerWithContactsAndInteractionsAsync', 'Creates detailed partner interaction summaries with contact details, interaction history, and overall partnership assessment in structured Markdown format.', true),

('interaction_action', 'I am sending you interaction data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

JSON format:
{ ""Message"": ""Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message"", ""Category"": ""Interaction"", ResponseType: ""Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION"", ""type"": """", ""date"": """", ""data"": """", ""contactId"": """",
, dependents: ["contactId"]  }

Some things to consider about the JSON format above are:
""type"" is the Interaction type which could be ""Email"", ""Chat"", ""Phone"", ""VideoMeeting"", ""InPersonMeeting""
""date"" Ensure the date is formatted as ISO 8601 timestamp
* Interaction can be linked to a contact. The JSON must have a property called contactId. Generally, the user will not know the ID of the Contact and hence will pass it as a Name. 
Put the name in the ""contactId"" property value and add contactId to the dependents property as an array. for example, dependents: ["contactId"]


STRICTLY do not use the word "markdown" while converting the final response to the final JSON.

Be very polite and kind and greet the user. Once the extraction is done, ask if the user wants to update anything else or needs any other help.

The prompt could be an extracted text from an audio or an image OR could be a summary of the conversation with the user. The summary could be talking about multiple entities. Only extract the details relevant to Interactions and the latest details. For example, there could have been multiple discussions about Interactions. Pick the latest request. Use this to form the JSON. Whether the prompt is an extracted text or a summary will be highlighted before the message begins (for example: Summary: <summary> OR Extracted text: <extracted text>)

Prompt: 
{promptData}', NOW(), 'Interaction', 1, '{"role":"user","parts":[{"text":"{promptData}"}]}', '{"temperature":0.1,"top_p":0.2,"max_output_tokens":8192}', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', NULL, '[]', 'GetInteractionDetailsAsync', 'Retrieves and summarizes interaction information in bullet points for easy understanding and reference.', true),

('domain_organization_lookup', 'I am providing a JSON array containing email domains. For each domain, identify the most likely organization or company name that uses that domain.

**Input Format:**
{promptData}

**Desired Output Format:**
Return a JSON array with the same order as input, where each element contains:
{
  "domain": "[original domain]",
  "organization": "[organization name]"
}

**Instructions:**
- For each domain, provide the most likely organization name
- If you cannot determine a likely organization name, use "Unknown" 
- Do not include explanations or additional text
- Return only the JSON array
- Ensure the response is valid JSON format
- Maintain the same order as the input domains

Example input: ["microsoft.com", "google.com", "unknowndomain123.com"]
Example output: [{"domain": "microsoft.com", "organization": "Microsoft Corporation"}, {"domain": "google.com", "organization": "Google Inc."}, {"domain": "unknowndomain123.com", "organization": "Unknown"}]', NOW(), 'Domain Organization Lookup', 1, '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]', NULL, 'GetPartnerNamesFromGeminiAsync', 'Batch lookup of organization names from email domains using Gemini AI', false),

('interaction_summary', 'I am providing interaction data. I need you to generate a comprehensive summary in Markdown format, using the following template:

## Interaction Summary

**Date:** [Interaction Date]  
**Type:** [Interaction Type]  
**Subject:** [Interaction Subject]  
**Status:** [Interaction Status]

**Key Details:**
- **Location:** [Location if available]
- **Duration/Context:** [Any timing or contextual information available]

**Participants:**

**UNOPS Team:**
- [User Name, Title/Role if available]

**External Participants:**
- **[Contact Name]** ([Contact Title]) from [Partner Organization]
- [Additional contacts if multiple]

**Partner Organization(s):**
- **[Partner Name]:** [Partner status and brief context about relationship with UNOPS]

**Discussion Points:**
[Provide a detailed summary of the interaction description, highlighting key topics discussed, decisions made, and important information exchanged]

**Associated Documents:**
[List any documents linked to this interaction]

**Context & Background:**
- **Previous Interactions:** [Brief mention of recent related interactions if context suggests ongoing engagement]
- **Partnership Status:** [Brief assessment of the partnership relationship based on available data]

**Key Outcomes & Next Steps:**
[Identify any action items, follow-up requirements, or next steps mentioned in the interaction]

**Additional Notes:**
[Any other relevant information, concerns, or observations]

Please format the response as clean Markdown without code blocks or backticks. Use the interaction data provided to fill in as much detail as possible. If any information is missing or not available in the data, simply omit that section. Focus on creating a clear, comprehensive summary that captures the essence and importance of this interaction within the broader context of UNOPS partnerships.

Data: {promptData}', NOW(), 'Interaction', 1, '{"role":"user","parts":[{"text":"{promptData}"}]}', '{"temperature":0.7,"top_p":0.2,"max_output_tokens":8192}', 'europe-west4', 'gemini-2.5-flash', 'unops-partneropportunity', NULL, NULL, 'GetInteractionDetailsAsync', 'Generates a comprehensive summary of interaction details including participants, content, context, and outcomes in a structured Markdown format.', true);
