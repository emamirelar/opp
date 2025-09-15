-- AI Prompts configuration
-- This script manages AI prompt definitions with environment variable substitution
-- Parameter: {{PROJECT_ID}} will be replaced by ScriptRunner

DO $$
BEGIN
    -- Clear existing data and reset
    TRUNCATE TABLE public."AiPrompt" RESTART IDENTITY CASCADE;
    RAISE NOTICE 'AI prompts table cleared, inserting fresh data';

    -- Insert contact_interactions_summary prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'contact_interactions_summary',
        'I am providing a contact name. I need you to generate a summary in Markdown format, using the following template:

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

Data: {promptData}',
        NOW(),
        'Contact',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetContactWithInteractionsAsync',
        'Generates a comprehensive summary of contact information including partner details and interaction history in a structured format.',
        true
    );

    -- Insert partner_priorities prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'partner_priorities',
        'I am providing a JSON object containing partner information and the fact that I work in Senegal. 
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

STRICTLY do not use the word "markdown" when you convert the final result to Markdown. Please provide the generated Markdown summary based on these instructions. Add additional line space after each detail. If any detail that you are instructed to provide is unavailable, do not include that in the response. Do not assume any detail. Please do not include "```markdown\n" in the response.',
        NOW(),
        'partner_priorities',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]',
        '[{"googleSearch":{}}]',
        'GetBasicPartnerDetailsAsync',
        'Give an overview of partner priorities',
        true
    );

    -- Insert general_information prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'general_information',
        'Strictly return the response in JSON format as below - 

{Category: "General", ResponseType: "INFORMATION", Message: "Add your response here"}',
        NOW(),
        'AiAssistant',
        1,
        '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }',
        '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 65535 }',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        NULL,
        '',
        'Returns a general response in JSON format.',
        true
    );

    -- Insert bulk_partner_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'bulk_partner_action',
        'You are an AI assistant processing partner data from Google Sheets for UNOPS. Convert each row into the exact JSON structure below. Only include non-empty fields to keep JSON compact.

**MANDATORY FIELDS:** name, partnerShortDescription, status (default: "Active")

**HEADER MAPPING:**
"ID"/"Partner ID" → id (number, only if present)
"Partner Name"/"Organization"/"Company" → name
"Short Name"/"Acronym"/"Abbreviation" → partnerShortDescription
"Long Description" → partnerLongDescription
"Status" → status ("Draft", "Active", "Closed", "Archived")
"Partner Category" → partnerCategoryId (number)
"Liaison Office" → liaisonOfficeId (number)
"Partner Focal Point" → partnerFocalPointUserId (number)
"ERP Dimension" → erpDimValue (number)

**ESSENTIAL PARTNER JSON FORMAT:**
{"name": "", "partnerShortDescription": "", "partnerLongDescription": "", "status": "Active", "partnerCategoryId": null, "liaisonOfficeId": null, "partnerFocalPointUserId": null, "erpDimValue": null, "dependents": ["partnerCategoryId", "liaisonOfficeId", "partnerFocalPointUserId"], "validationError": ""}

**RULES:**
- Set validationError for missing mandatory fields
- Map text names to ID fields, include in dependents for resolution
- Omit null/empty fields to keep JSON compact
- Default status to "Active"
- Default ID fields to null
- Only include "id" field in JSON output if ID column is present in source data
- Focus on essential fields only: name, partnerShortDescription, partnerLongDescription, status, and key ID references

**RESPONSE FORMAT:**
{"Message":"Partner data processed successfully.","Category":"Partner","ResponseType":"Action","records":[...]}

Return compact single-line JSON. If more input needed, set ResponseType to "Information".

Input data: {promptData}',
        NOW(),
        'Partner',
        1,
        '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }',
        '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 65535 }',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        NULL,
        '',
        'Processes bulk partner data from arrays or objects, converting them into structured JSON format with validation of acceptable values and automatic field mapping.',
        false
    );

    -- Insert partnertree_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'partnertree_action',
        'I am sending you partner tree (level) data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

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
{promptData}',
        NOW(),
        'PartnerTree',
        1,
        '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }',
        '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 65535 }',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        NULL,
        '',
        'Extracts partner tree (level) information from raw data and formats it into structured JSON with auto-generated codes and hierarchical type determination.',
        false
    );

    -- Insert domain_organization_lookup prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'domain_organization_lookup',
        'I am providing a JSON array containing email domains. For each domain, identify the most likely organization or company name that uses that domain.

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
Example output: [{"domain": "microsoft.com", "organization": "Microsoft Corporation"}, {"domain": "google.com", "organization": "Google Inc."}, {"domain": "unknowndomain123.com", "organization": "Unknown"}]',
        NOW(),
        'Domain Organization Lookup',
        1,
        '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }',
        '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]',
        NULL,
        'GetPartnerNamesFromGeminiAsync',
        'Batch lookup of organization names from email domains using Gemini AI',
        false
    );

    -- Insert partner_interactions_summary prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'partner_interactions_summary',
        'I am providing a partner name.

##Summary of key interactions
Provide an introductory paragraph of interactions related to the partner in the last month. Highlight any key high-level interactions.

For example, using the following format, generate a summary of an interaction that looks like this: 

On 19/09/2024, Beth Hayes from org unit (in bold) had a Type of interaction. It was discussed the need for a significant reduction in energy-efficient procedures, which could be achieved through enterprise-level investments. (If the interaction is related to a project please indicate the country and number, and if not say "not related to a specific project").**See more**  (text contains the word "See more" with a hyperlink to open the specific interaction record). (give line breaks after each interaction summary)

##List of interactions

####Interactions relevant to [org unit]

Highlight key recent interactions 

There have been several recent interactions between UNOPS and [Partner]
[Date of interaction]: A high-level meeting between UNOPS'' [Personnel name, Personnel title] and the World Bank''s [Contact name, contact title] to discuss ongoing projects. 
[Date of interaction]: a meeting between the World Bank and UNOPS'' project teams to discuss project [Engagement name, engagement code] and project process, where key milestones such as timely delivery of supplies were identified. 
[Date of interaction]: high-level meeting between the World Bank''s [Contact name, contact title], and the UNOPS delegation at the [Event name]
.**See more**  (text contains the word "See more" with a hyperlink to open the specific interaction record)

If there are no interactions with the partner please state "Currently, there are not interactions available in Opportunity+ with [Partner]
 
{promptData}

',
        NOW(),
        'Partner',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.7,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[{"googleSearch":{}}]',
        'GetPartnerWithContactsAndInteractionsAsync',
        'Creates detailed partner interaction summaries with contact details, interaction history, and overall partnership assessment in structured Markdown format.',
        true
    );

    -- Insert interaction_summary prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'interaction_summary',
        'I am providing interaction data. Please provide a concise summary of the interaction in the following structure in a bullet point list. Please do not include a lead in sentence. 


Date of interation:
Key people involved:
Key points:
Follow up needed:

Data: {promptData}',
        NOW(),
        'Interaction',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.2,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetInteractionDetailsAsync',
        'Generates a comprehensive summary of interaction details including participants, content, context, and outcomes in a structured Markdown format.',
        true
    );

    -- Insert interaction_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'interaction_action',
        'I am sending you interaction data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

JSON format:
{ "Message": "Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message", "Category": "Interaction", ResponseType: "Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION", "type": "", "date": "", "subject": "", "description": "", "contactId": "", "status": "Active", "emailAddresses": [], dependents: ["contactId"]  }

Some things to consider about the JSON format above are:
"type" is the Interaction type which could be Email,Chat,Call,VirtualMeeting,InPersonMeeting,Other
"date" Ensure the date is formatted as ISO 8601 timestamp
"subject" Brief summary or title of the interaction
"description" Detailed content of the interaction
* Interaction can be linked to a contact. The JSON must have a property called contactId. Generally, the user will not know the ID of the Contact and hence will pass it as a Name. 
Put the name in the "contactId" property value and add contactId to the dependents property as an array. for example, dependents: ["contactId"]
* Only include "id" field in JSON output if ID is present in source data
* Focus on essential fields only: type, date, subject, description, contactId, status, emailAddresses
* "emailAddresses" should be an array of email addresses extracted from the interaction content. If no email addresses are found, use an empty array []

**HEADER MAPPING:**
"ID"/"Interaction ID" → id (number, only if present)
"Type" → type
"Date" → date
"Subject" → subject
"Description" → description
"Contact" → contactId
"Location" -> location

STRICTLY do not use the word "markdown" while converting the final response to the final JSON.

Be very polite and kind and greet the user. Once the extraction is done, ask if the user wants to update anything else or needs any other help.

The prompt could be an extracted text from an audio or an image OR could be a summary of the conversation with the user. The summary could be talking about multiple entities. Only extract the details relevant to Interactions and the latest details. For example, there could have been multiple discussions about Interactions. Pick the latest request. Use this to form the JSON. Whether the prompt is an extracted text or a summary will be highlighted before the message begins (for example: Summary: <summary> OR Extracted text: <extracted text>)

Prompt: 
{promptData}',
        NOW(),
        'Interaction',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetInteractionDetailsAsync',
        'Retrieves and summarizes interaction information in bullet points for easy understanding and reference.',
        false
    );

    -- Insert partner_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'partner_action',
        'You are an AI assistant processing partner data. Extract partner information from the provided data and return ONLY a valid JSON object. Do not include any conversation, greetings, explanations, or markdown formatting. Return ONLY the raw JSON.

JSON format:
{"Message": "Data extracted successfully", "Category": "Partner", "ResponseType": "Action", "name": "", "partnerShortDescription": "", "partnerLongDescription": "", "status": "Active", "partnerCategoryId": null, "liaisonOfficeId": null, "partnerFocalPointUserId": null, "erpDimValue": null, "dependents": ["partnerCategoryId", "liaisonOfficeId", "partnerFocalPointUserId"]}

**RULES:**
- Required fields: name, partnerShortDescription, status
- Status: Default to "Active"
- Partner: Set ID fields as string names, include in dependents for ID resolution
- Omit null/empty fields from JSON to keep it compact
- Always set status to "Active"
- Default ID fields to null
- Only include "id" field in JSON output if ID is present in source data
- Focus on essential fields only: name, partnerShortDescription, partnerLongDescription, status, and key ID references

**HEADER MAPPING:**
"ID"/"Partner ID" → id (number, only if present)
"Partner Name"/"Organization"/"Company" → name
"Short Name"/"Acronym"/"Abbreviation" → partnerShortDescription
"Long Description" → partnerLongDescription
"Partner Category" → partnerCategoryId
"Liaison Office" → liaisonOfficeId
"Partner Focal Point" → partnerFocalPointUserId
"ERP Dimension" → erpDimValue

Return compact single-line JSON without line breaks or unnecessary whitespace.

Input data: {promptData}',
        NOW(),
        'Partner',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        '',
        'Extracts partner information from raw data or conversation summaries and formats it into structured JSON for partner creation or updates with validation of acceptable values.',
        false
    );

    -- Insert partner_funding_test prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'partner_funding_test',
        '{promptData}',
        NOW(),
        'Partner',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetBasicPartnerDetailsAsync',
        'partner_funding_test',
        true
    );

    -- Insert partner_news prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'partner_news',
        'I am providing a JSON object containing partner information and that I work in [Org Unit]. Identify the name of the partner from that JSON data and find the latest development news articles on the partner relevant to somebody working at UNOPS. Please use sources such as Google News as well as development news sites such as Devex and donor tracker. 

Give a summary of the 10 most recent partner news stories, relevant to my organisation unit in UNOPS.

PLease include the lead in sentence
"Here are the 10 most recent news stories concerning [partner], relevant to UNOPS"

For each news story, please include, the following details
headline, 
short summary of the story, 
date of publication 
news source. 
a link to the specific full news story embedded in a hyperlink icon 

Please include a line between each story

Data: {promptData}
',
        NOW(),
        'Partner',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.1,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]',
        '[{"googleSearch":{}}]',
        'GetBasicPartnerDetailsAsync',
        'Searches for and summarizes the latest news articles about a partner organization, identifying current focus areas and trends from recent developments.',
        true
    );

    -- Insert partner_opportunity_funding_test prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'partner_opportunity_funding_test',
        'Looking to the partner''s official channels, please find 5 current opportunities for project funding in the Africa region for UNOPS as an implementing partner. PLease include details of the opportunity and a link for further investigation.

Data: {promptData}',
        NOW(),
        'Partner',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetBasicPartnerDetailsAsync',
        'Partner opportunity funding test',
        true
    );

    -- Insert contact_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'contact_action',
        'You are an AI assistant processing contact data. Extract contact information from the provided data and return ONLY a valid JSON object. Do not include any conversation, greetings, explanations, or markdown formatting. Return ONLY the raw JSON.

JSON format:
{"Message": "Data extracted successfully", "Category": "Contact", "ResponseType": "Action", "salutation": "", "firstName": "", "lastName": "", "title": "", "email": "", "phone": "", "mobile": "", "status": "Active", "partnerId": "", "department": "", "dependents": ["partnerId"]}

**RULES:**
- Required fields: lastName, email, title, partnerId
- Salutation: Auto-detect from Mr., Ms., Mrs., Dr., Prof., Sir, Madam
- Partner: Set partnerId as string name, include "partnerId" in dependents for ID resolution
- Omit null/empty fields from JSON to keep it compact
- Always set status to "Active"
- Set validationError for missing required fields
- Only include "id" field in JSON output if ID is present in source data
- Focus on essential fields only: name components, title, email, phone, partnerId, department

**HEADER MAPPING:**
"ID"/"Contact ID" → id (number, only if present)
"Full Name"/"Name"/"Contact Name" → firstName + lastName
"Email"/"Email Address"/"E-mail" → email
"Phone"/"Phone Number"/"Telephone" → phone
"Mobile"/"Cell Phone"/"Mobile Number" → mobile
"Company"/"Organization"/"Partner"/"Employer" → partnerId
"Job Title"/"Position"/"Role" → title
"Department"/"Division"/"Unit" → department

Return compact single-line JSON without line breaks or unnecessary whitespace.

Input data: {promptData}',
        NOW(),
        'Contact',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        '',
        'Extracts contact information from raw data or conversation summaries and formats it into structured JSON for contact creation or updates.',
        false
    );

    -- Insert test_partner_info prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'test_partner_info',
        'Summarize this partner 
{promptData}',
        NOW(),
        'Partner',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetBasicPartnerDetailsAsync',
        'This is a test prompt for QA ',
        true
    );

    -- Insert bulk_interaction_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'bulk_interaction_action',
        'You are an AI assistant that processes interaction data for bulk import. You will receive interaction data as an array of arrays (with optional header) or an array of objects, or text extracted from audio/image.

Convert each item into the exact JSON structure shown below. Only include non-empty fields.

**Required fields:** type, date, subject
**Validation rules:**
- Map contact names to contactIds (keep as text if name, number if ID)
- Map partner names to partnerIds (keep as text if name, number if ID)
- Map user names to userIds (keep as text if name, number if ID)
- Map emailAddresses to emailAddresses
- Map location / country you find to location
- Format date as ISO 8601 timestamp (YYYY-MM-DDTHH:mm:ss.sssZ)
- Default status to "Active"
- Include dependents for all ID fields that are text names
- Based on the context of the message, auto-detect the date.
- Put one of the contactIds into contactId
- Only include "id" field in JSON output if ID column is present in source data
- Focus on essential fields only: type, date, subject, description, contactId, status, emailAddresses
- Extract email addresses from the interaction content and populate emailAddresses as an array of strings

**Interaction types:** Email,Chat,Call,VirtualMeeting,InPersonMeeting,Other (USE THE EXACT WORD WITHOUT SPACES)

**HEADER MAPPING:**
"ID"/"Interaction ID" → id (number, only if present)
"Type" → type
"Date" → date
"Subject" → subject
"Description" → description
"Contact" → contactIds
"Status" → status
"Location" -> location
EmailAddresses -> EmailAddresses
User ID (any user info) -> userIds
Org Unit / Organisation Unit  -> organizationHierarchyIds

Include a "name" field that summarizes the interaction in 5-6 words.


If you cannot find the match, assign the closest match to it.

**ESSENTIAL INTERACTION JSON FORMAT:**
{"type": "", "date": "", "subject": "", "description": "", "status": "Active", "contactIds": [], "emailAddresses": [],  location: "", userIds: [], "name": "", "organizationHierarchyIds": [], "dependents": ["contactIds", "userIds", "organizationHierarchyIds"], "validationError": ""}

**Response format:** {"Message":"Action completed successfully.", "Category":"Interaction", "ResponseType":"Action", "records":[...]}

Return compact single-line JSON. If more input needed, set ResponseType to "Information".

Input data: {promptData}',
        NOW(),
        'Interaction',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        '',
        'Processes bulk interaction data from arrays or objects, converting them into structured JSON format with automatic date parsing, field mapping, and validation of interaction types.',
        true
    );

    -- Insert bulk_contact_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'bulk_contact_action',
        'You are an AI assistant processing contact data from Google Sheets for UNOPS. Convert each row into the exact JSON structure below. Only include non-empty fields. Required: lastName, email, title, partnerId.

**HEADER MAPPING:**
"ID"/"Contact ID" → id (number, only if present)
"Full Name"/"Name"/"Contact Name" → firstName + lastName + name (computed as full name)
"Email"/"Email Address"/"E-mail" → email
"Phone"/"Phone Number"/"Telephone" → phone
"Mobile"/"Cell Phone"/"Mobile Number" → mobile
"Company"/"Organization"/"Partner"/"Employer" → partnerId (string, add to dependents)
"Job Title"/"Position"/"Role" → title
"Department"/"Division"/"Unit" → department

**SALUTATION DETECTION:**
Auto-detect from: Mr., Ms., Mrs., Dr., Prof., Sir, Madam

**ESSENTIAL CONTACT JSON FORMAT:**
{"salutation": "", "firstName": "", "lastName": "", "name": "", "title": "", "department": "", "email": "", "phone": "", "mobile": "", "partnerId": "", "dependents": ["partnerId"], "validationError": ""}

**RULES:**
- Set validationError for missing required fields (lastName, email, title, partnerId)
- Validate email format
- Set partnerId as string name, include "partnerId" in dependents for ID resolution
- Omit null/empty fields from JSON to keep it compact
- Compute name field as concatenation of salutation + firstName + lastName
- Only include "id" field in JSON output if ID column is present in source data
- Focus on essential fields only: name components, title, email, phone, partnerId, department

**RESPONSE FORMAT:**
{"Message":"Contact data processed successfully.","Category":"Contact","ResponseType":"Action","records":[...]}

Return compact single-line JSON. If more input needed, set ResponseType to "Information".

Input data: {promptData}',
        NOW(),
        'Contact',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        '',
        'Processes bulk contact data from arrays or objects, converting them into structured JSON format with automatic name parsing and partner linking.',
        false
    );

    -- Insert parnter_category_interactions_summary prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'parnter_category_interactions_summary',
        'I am providing a partner category name, please provide a summary of recent interactions for partners unders this partner category. 

##Summary of key interactions
Provide an introductory paragraph of interactions related to the partner in the last month. Highlight any key high-level interactions.

For example, using the following format, generate a summary of an interaction that looks like this: 

On 19/09/2024, Beth Hayes from org unit (in bold) had a Type of interaction. It was discussed the need for a significant reduction in energy-efficient procedures, which could be achieved through enterprise-level investments. (If the interaction is related to a project please indicate the country and number, and if not say "not related to a specific project").**See more**  (text contains the word "See more" with a hyperlink to open the specific interaction record). (give line breaks after each interaction summary)

##List of interactions

####Interactions relevant to [org unit]

Highlight key recent interactions 

There have been several recent interactions between UNOPS and [Partner]
[Date of interaction]: A high-level meeting between UNOPS'' [Personnel name, Personnel title] and the World Bank''s [Contact name, contact title] to discuss ongoing projects. 
[Date of interaction]: a meeting between the World Bank and UNOPS'' project teams to discuss project [Engagement name, engagement code] and project process, where key milestones such as timely delivery of supplies were identified. 
[Date of interaction]: high-level meeting between the World Bank''s [Contact name, contact title], and the UNOPS delegation at the [Event name]
.**See more**  (text contains the word "See more" with a hyperlink to open the specific interaction record)

If there are no interactions with the partner please state "Currently, there are not interactions available in Opportunity+ with [Partner Category]
 
{promptData}

',
        NOW(),
        'Interaction',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":8192}',
        'europe-west4',
        'gemini-2.0-flash-001',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetBasicPartnerCategoryDetailsAsync',
        'Creates detailed interaction summaries for a partner ctegory with contact details, interaction history, and overall partnership assessment in structured Markdown format.',
        true
    );

    -- Insert patner_category prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'patner_category',
        'I am providing a partner category with a list of associated partners.

##Summary of key interactions
Provide an introductory paragraph of interactions related to the partner in the last month. Highlight any key high-level interactions.

For example, using the following format, generate a summary of an interaction that looks like this: 

On 19/09/2024, Beth Hayes from org unit (in bold) had a Type of interaction. It was discussed the need for a significant reduction in energy-efficient procedures, which could be achieved through enterprise-level investments. (If the interaction is related to a project please indicate the country and number, and if not say "not related to a specific project").**See more**  (text contains the word "See more" with a hyperlink to open the specific interaction record). (give line breaks after each interaction summary)

##List of interactions

####Interactions relevant to [org unit]

Highlight key recent interactions 

There have been several recent interactions between UNOPS and [Partner]
[Date of interaction]: A high-level meeting between UNOPS'' [Personnel name, Personnel title] and the World Bank''s [Contact name, contact title] to discuss ongoing projects. 
[Date of interaction]: a meeting between the World Bank and UNOPS'' project teams to discuss project [Engagement name, engagement code] and project process, where key milestones such as timely delivery of supplies were identified. 
[Date of interaction]: high-level meeting between the World Bank''s [Contact name, contact title], and the UNOPS delegation at the [Event name]
.**See more**  (text contains the word "See more" with a hyperlink to open the specific interaction record)

If there are no interactions with the partner please state "Currently, there are not interactions available in Opportunity+ with [Partner Category]
 
{promptData}

',
        NOW(),
        'patner_category',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":8192}',
        'europe-west4',
        'gemini-2.0-flash-001',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetBasicPartnerDetailsAsync',
        'Creates detailed partner category  interaction summaries with contact details, interaction history, and overall partnership assessment in structured Markdown format.',
        true
    );

    -- Insert partner_group_news prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'partner_group_news',
        'I am providing a JSON object containing partner group and it''s related partner entities and that I work in [Org Unit]. Identify the name of the partners linked to this group from that data and find the latest development news articles on the partner relevant to somebody working at UNOPS. Please use sources such as Google News as well as development news sites such as Devex and donor tracker. 

Give a summary of the 10 most recent partner news stories, relevant to my organisation unit in UNOPS.

PLease include the lead in sentence
"Here are the 10 most recent news stories concerning [partner], relevant to UNOPS"

For each news story, please include, the following details
headline, 
short summary of the story, 
date of publication 
news source. 
a link to the specific full news story embedded in a hyperlink icon 

Please include a line between each story

Data: {promptData}
',
        NOW(),
        'Partner',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]',
        '[{"googleSearch":{}}]',
        'GetBasicPartnerDetailsAsync',
        'Searches for and summarizes the latest news articles about a partner group, identifying current focus areas and trends from recent developments.',
        true
    );

    -- Insert partner_group_interactions_summary prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'partner_group_interactions_summary',
        'I am providing a partner group name, please provide a summary of recent interactions for partners related to this partner category. 

##Summary of key interactions
Provide an introductory paragraph of interactions related to the partner in the last month. Highlight any key high-level interactions.

For example, using the following format, generate a summary of an interaction that looks like this: 

On 19/09/2024, Beth Hayes from org unit (in bold) had a Type of interaction. It was discussed the need for a significant reduction in energy-efficient procedures, which could be achieved through enterprise-level investments. (If the interaction is related to a project please indicate the country and number, and if not say "not related to a specific project").**See more**  (text contains the word "See more" with a hyperlink to open the specific interaction record). (give line breaks after each interaction summary)

##List of interactions

####Interactions relevant to [org unit]

Highlight key recent interactions 

There have been several recent interactions between UNOPS and [Partner]
[Date of interaction]: A high-level meeting between UNOPS'' [Personnel name, Personnel title] and the World Bank''s [Contact name, contact title] to discuss ongoing projects. 
[Date of interaction]: a meeting between the World Bank and UNOPS'' project teams to discuss project [Engagement name, engagement code] and project process, where key milestones such as timely delivery of supplies were identified. 
[Date of interaction]: high-level meeting between the World Bank''s [Contact name, contact title], and the UNOPS delegation at the [Event name]
.**See more**  (text contains the word "See more" with a hyperlink to open the specific interaction record)

If there are no interactions with the partner please state "Currently, there are not interactions available in Opportunity+ with [Partner Group]
 
{promptData}

',
        NOW(),
        'Partner',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetBasicPartnerDetailsAsync',
        'Creates detailed interaction summaries for a partner group with contact details, interaction history, and overall partnership assessment in structured Markdown format.',
        true
    );

    -- Insert partner_category_news prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'partner_category_news',
        'I am providing a JSON object containing partner category and it''s related partner entities and that I work in [Org Unit]. Identify the name of the partners linked to this category from that data and find the latest development news articles on the partner relevant to somebody working at UNOPS. Please use sources such as Google News as well as development news sites such as Devex and donor tracker. 

Give a summary of the 10 most recent partner news stories, relevant to my organisation unit in UNOPS.

PLease include the lead in sentence
"Here are the 10 most recent news stories concerning [partner], relevant to UNOPS"

For each news story, please include, the following details
headline, 
short summary of the story, 
date of publication 
news source. 
a link to the specific full news story embedded in a hyperlink icon 

Please include a line between each story

Data: {promptData}
',
        NOW(),
        'Partner',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]',
        '[{"googleSearch":{}}]',
        'GetBasicPartnerDetailsAsync',
        'Searches for and summarizes the latest news articles about a partner category, identifying current focus areas and trends from recent developments.',
        true
    );

    -- Insert qa_partner_news prompt
    INSERT INTO public."AiPrompt" (
        "Type", "Prompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "PromptFunction", "Description", "AdminCanChange"
    ) VALUES (
        'qa_partner_news',
        'QA TESTING
 {promptData} ',
        NOW(),
        'Partner',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetBasicPartnerDetailsAsync',
        'qa_partner_news',
        true
    );

    RAISE NOTICE 'AI prompts setup complete with % records', (SELECT COUNT(*) FROM public."AiPrompt");
END $$;
