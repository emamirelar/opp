-- AI Prompts configuration
-- This script manages AI prompt definitions with environment variable substitution
-- Parameter: {{PROJECT_ID}} will be replaced by ScriptRunner

DO $$
BEGIN
    -- Clear existing data and reset
    TRUNCATE TABLE public."AiPrompt" RESTART IDENTITY CASCADE;
    RAISE NOTICE 'AI prompts table cleared, inserting fresh data';

    -- Insert bulk_interaction_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'bulk_interaction_action',
        'You process interaction data for bulk import. You will receive interaction data as an array of arrays (with optional header) or an array of objects, or text extracted from audio/image.

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
"Date" → date (default to today''s date if nothing is present)
"Subject" → subject
"Description" → description
"Contact" → contactIds (could be number or contact names)
"PhoneNumbers" -> Phone numbers that you find
"Status" → status
"Location" -> location
EmailAddresses -> EmailAddresses
User ID (any user info) -> userIds
Org Unit / Organisation Unit  -> organizationHierarchyIds

Include a "name" field that summarizes the interaction in 5-6 words.

If you cannot find the match, assign the closest match to it.

**ESSENTIAL INTERACTION JSON FORMAT:**
{"id": <number>, "type": "", "date": "", "subject": "", "description": "", "status": "Active", "contactIds": [], "emailAddresses": [],  location: "", userIds: [], "phoneNumbers": [], "name": "", "organizationHierarchyIds": [], "dependents": ["contactIds", "userIds", "organizationHierarchyIds"], "validationError": ""}

**Response format:** {"Message":"Action completed successfully.", "Category":"Interaction", "ResponseType":"Action", "records":[...]}

Return compact single-line JSON. If more input needed, set ResponseType to "Information". Send the "dependents" as-is. They are used for mapping purpose. Also, send "id" if and only if it is present. Even though organizationHierarchyIds is returning an array, you should expect only 1 Org unit. If there are more, you pick the last one of that record and put it in the array. Remember to put the date as today''s date if there is NO date you find per record',
        '',
        NOW(),
        'Interaction',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        '',
        'Processes bulk interaction data from arrays or objects, converting them into structured JSON format with automatic date parsing, field mapping, and validation of interaction types.',
        true,
        'Data Import',
        false,
        60
    );

    -- Insert partner_news prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'partner_news',
        'You are a partnerships assistant at the United Nations Office for Project Services (UNOPS). Your job is to scan through the latest news articles using Google Search and come up with articles that are relevant to UNOPS and of specific relevance to the user''s role and the user''s location (duty station and country).

The news articles should be about the partner specified. Find no more than 5 latest articles.

Output Format:
Provide the output in well-formed markdown in the format stated below.
EACH article should be formatted as follows:
# News headline [hyperlink the news headline with the link to the source]
**Publication / Website Name** **Publication Date**
[One or two line summary of the article - use a direct excerpt from Google Search if available]
',
        'The partner is: {partnerName}
Partner Information:
- Organization: {name}
- Status: {status}  
- Partnership Level: {partnerGroup.name}
- Liaison Office: {liaisonOffice.name}
- Total Contacts: {summary.totalContacts}
- Recent Activity: {summary.recentInteractions} interactions in last 30 days

User Information:
- Name: {userProfile.name}
- Position: {userProfile.position}
- Organization Unit: {userProfile.orgUnitName}
- Duty Station: {userProfile.dutyStation}
- Supervisor: {userProfile.supervisor.name}',
        NOW(),
        'Partner',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.1,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]',
        '[{"googleSearch":{}}]',
        'GetBasicPartnerDetailsAsync',
        'Searches for and summarizes the latest news articles about a partner organization, identifying current focus areas and trends from recent developments.',
        true,
        'Partner Management',
        true,
        120
    );

    -- Insert partner_interactions_summary prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'partner_interactions_summary',
        'You are an AI assistant that creates detailed partner interaction summaries. Focus on recent interactions, key personnel, and strategic engagement patterns. Use the following Markdown format:

## Summary of key interactions for [Partner Name]

Provide an introductory paragraph highlighting key interactions from the last month with this partner. Focus on high-level strategic engagements and important developments.

For each significant interaction, use this format:
On [Date], **[UNOPS Personnel Name]** from **[org unit]** had a [Type of interaction]. [Brief description of what was discussed]. (If the interaction is related to a project, indicate the country and project number; if not, say "not related to a specific project"). **[See more](interaction-link)**

*(Include line breaks after each interaction summary)*

## List of interactions

### Interactions relevant to [org unit]

Highlight key recent interactions using this structure:

There have been several recent interactions between UNOPS and [Partner Name]:

**[Date of interaction]:** A high-level meeting between UNOPS'' **[Personnel name, Personnel title]** and [Partner]''s **[Contact name, contact title]** to discuss [topic/purpose].

**[Date of interaction]:** A meeting between [Partner] and UNOPS'' project teams to discuss project **[Engagement name, engagement code]** and project process, where key milestones such as [specific outcomes] were identified.

**[Date of interaction]:** High-level meeting between [Partner]''s **[Contact name, contact title]** and the UNOPS delegation at the **[Event name]**. **[See more](interaction-link)**

### Contact Information
- **Key Contacts:** [List primary contacts with titles]
- **Most Active Contact:** [Name and engagement level]
- **Total Contacts:** [Number]

### Partnership Overview
- **Partnership Level:** [Partnership classification]
- **Engagement Frequency:** [High/Medium/Low based on recent activity]
- **Last Activity:** [Date of most recent interaction]

If there are no interactions with the partner, state: "Currently, there are no interactions available in Opportunity+ with [Partner Name]."

Format the response as clean Markdown without code blocks or backticks.',
        'Create a comprehensive interaction summary for partner "{name}" and their engagement with UNOPS.

**Partner Information:**
- Organization: {name}
- Status: {status}
- Partnership Level: {partnerGroup.name}
- Liaison Office: {liaisonOffice.name}
- Established: {partnership.establishedDate}

**Contact Information:**
- Total Contacts: {summary.totalContacts}
- Active Contacts: {summary.activeContacts}
- Most Active Contact: {summary.mostActiveContact}
- Key Contacts: {engagement.keyContactPoints}

**Interaction History:**
- Total Interactions: {summary.totalInteractions}
- Recent Interactions (30 days): {summary.recentInteractions}
- Last Interaction Date: {summary.lastInteractionDate}
- Average Interactions per Contact: {summary.averageInteractionsPerContact}

**Recent Interactions Details:**
{recentInteractions}

**All Interactions:**
{allInteractions}

**Partnership Details:**
- Engagement Level: {engagement.engagementFrequency}
- Last Activity: {partnership.lastActivity}
- Organization Units Involved: {organizationUnits}

**User Context:**
- Analyst: {userProfile.name} ({userProfile.position})
- Organization Unit: {userProfile.orgUnitName}
- Duty Station: {userProfile.dutyStation}

**Audit Information:**
- Analysis Date: {auditInfo.createdDate}
- Last Updated: {auditInfo.lastModifiedDate}

Focus on partnership activities, collaboration patterns, key personnel involved, and strategic engagement opportunities with this partner.',
        NOW(),
        'Partner',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.7,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[{"googleSearch":{}}]',
        'GetPartnerWithContactsAndInteractionsForAIAsync',
        'Creates detailed partner interaction summaries with contact details, interaction history, and overall partnership assessment in structured Markdown format.',
        true,
        'Partner Management',
        true,
        75
    );

    -- Insert interaction_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'interaction_action',
        'I am sending you interaction data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)

You process interaction data for bulk import. You will receive interaction data as an array of arrays (with optional header) or an array of objects, or text extracted from audio/image.

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
"Date" → date (default to today''s date if nothing is present)
"Subject" → subject
"Description" → description
"Contact" → contactIds (could be number or contact names)
"PhoneNumbers" -> Phone numbers that you find
"Status" → status
"Location" -> location
EmailAddresses -> EmailAddresses
User ID (any user info) -> userIds
Org Unit / Organisation Unit  -> organizationHierarchyIds

Include a "name" field that summarizes the interaction in 5-6 words.

If you cannot find the match, assign the closest match to it.

**ESSENTIAL INTERACTION JSON FORMAT:**
{"id": <number>, "type": "", "date": "", "subject": "", "description": "", "status": "Active", "contactIds": [], "emailAddresses": [],  location: "", userIds: [], "phoneNumbers": [], "name": "", "organizationHierarchyIds": [], "dependents": ["contactIds", "userIds", "organizationHierarchyIds"], "validationError": ""}

**Response format:** {"Message":"Action completed successfully.", "Category":"Interaction", "ResponseType":"Action", "data":[...]}

Return compact single-line JSON. If more input needed, set ResponseType to "Information". Send the "dependents" as-is. They are used for mapping purpose. Also, send "id" if and only if it is present. Even though organizationHierarchyIds is returning an array, you should expect only 1 Org unit. If there are more, you pick the last one of that record and put it in the array. Remember to put the date as today''s date if there is NO date you find per record

The prompt could be an extracted text from an audio or an image OR could be a summary of the conversation with the user. The summary could be talking about multiple entities. Only extract the details relevant to Interactions and the latest details. For example, there could have been multiple discussions about Interactions. Pick the latest request. Use this to form the JSON. Whether the prompt is an extracted text or a summary will be highlighted before the message begins (for example: Summary: <summary> OR Extracted text: <extracted text>)',
        '',
        NOW(),
        'Interaction',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetInteractionDetailsAsync',
        'Retrieves and summarizes interaction information in bullet points for easy understanding and reference.',
        true,
        'Interaction Management',
        false,
        60
    );

    -- Insert contact_interactions_summary prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'contact_interactions_summary',
        'You are an AI assistant that generates comprehensive contact summaries in Markdown format. Use the following template structure:

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

Format the response as clean Markdown without code blocks or backticks. Fill in with as much information as available. Surface the Partner related to the contact, their title, email, and any other relevant information. If any information is missing, simply omit that section.',
        'Generate a comprehensive summary for contact {fullName} ({email}) from {partner.name}.

**Contact Information:**
- Full Name: {salutation} {firstName} {middleName} {lastName} {suffix}
- Title: {title}
- Department: {department}
- Email: {email}
- Phone: {phone}
- Mobile: {mobile}
- Status: {status}
- Description: {description}

**Partner Organization:**
- Organization: {partner.name}
- Partner Status: {partner.status}
- Partner Group: {partner.partnerGroup}

**Contact Details:**
- Profile Picture: {contactDetails.hasProfilePicture}
- Mailing Address: {mailingAddress.fullAddress}

**Assistant Information:** {assistant}

**Documents & Attachments:**
- Total Documents: {summary.totalDocuments}
- Has CV/Resume: {summary.hasCV}
- Document Details: {documents}

**Interaction History:**
- Total Interactions: {summary.totalInteractions}
- Recent Interactions (30 days): {summary.recentInteractions}
- Last Interaction: {summary.lastInteractionDate}
- Interaction Details: {interactions}

**Audit Information:**
- Created: {auditInfo.createdDate}
- Last Modified: {auditInfo.lastModifiedDate}

Please create a comprehensive summary including their complete profile, interaction history, partner relationship details, document attachments, and any relevant notes about their engagement with UNOPS. Pay special attention to CV/resume documents and recent interaction patterns.',
        NOW(),
        'Contact',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetContactWithInteractionsAsync',
        'Generates a comprehensive summary of contact information including partner details and interaction history in a structured format.',
        true,
        'Contact Management',
        true,
        30
    );

    -- Insert user_role_import prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'user_role_import',
        'You are an AI assistant for processing user role assignment data. You will receive user role data as an array of arrays (with optional header) or an array of objects or just plain text with information. If they are in an ordered form, first row could optionally be headers. Convert each item into the exact JSON structure shown below. Use your best knowledge to determine how the data is sent.The data represents user-role assignments for the current application. Map user information (name, email, username) to userId and role information (role names) to roleIds.


Expected input columns may include:

- User information: user name, email, username, first name, last name

- Role information: role name, role names (comma-separated), roles - anything that looks like a role

User Role Assignment format: {
  userId: null,
  roleIds: [],
  dependents: ["userId", "roleIds"],
  validationError: ""
}

If you find a user name / name / user id, put them in the userId key (only the user information). Whatever looks like the role should go into roleIds. DONOT modify the dependents key''s value. It needs to be sent as-is as it will be used for mapping.


Response format: {"Message":"User role assignments processed successfully.","Category":"UserRole","ResponseType":"Action","records":[...]}


Return only the response in a compact, single-line JSON format without line breaks or unnecessary whitespace, with no other explanation. This is critical for successful parsing. If more input is needed, set ResponseType to "Information". YOU ARE EXPECTED TO ONLY RETURN THE FINAL JSON.',
        '',
        NOW(),
        'UserRoleImport',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        ' ',
        'User Role Import prompt',
        true,
        'User Management',
        false,
        60
    );

    -- Insert domain_organization_lookup prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'domain_organization_lookup',
        'You are an AI assistant that performs batch lookup of organization names from email domains using knowledge of common domain-to-organization mappings.

Your task is to identify the most likely organization or company name that uses each provided domain.

**Output Requirements:**
- Return ONLY a valid JSON array
- Maintain the exact same order as the input domains
- Use the exact format specified below
- Do not include any explanations, markdown, or additional text
- Do not use code blocks or backticks

**JSON Format:**
Each element must contain exactly these fields:
{
"domain": "[original domain exactly as provided]",
"organization": "[organization name or ''Unknown'']"
}


**Lookup Rules:**
- For well-known domains (microsoft.com, google.com, etc.), provide the official organization name
- For government domains (.gov, .mil), identify the specific agency or department
- For academic domains (.edu), provide the institution name
- For unknown or unclear domains, use exactly "Unknown"
- For personal/generic domains (gmail.com, yahoo.com), use the service provider name
- Prioritize official/legal organization names over brand names when possible

**Examples:**
- microsoft.com → "Microsoft Corporation"
- google.com → "Google LLC" 
- harvard.edu → "Harvard University"
- state.gov → "U.S. Department of State"
- unknowndomain123.com → "Unknown"',
        'Perform organization lookup for the following domain analysis data:

**Contact Context:**
- Contact ID: {contactId}
- Total Domains Found: {summary.total}
- Resolved Domains: {summary.resolved}
- Unresolved Domains: {summary.unresolved}
- Success Rate: {summary.successRate}

**Domain List:**
{domains}

**Search Results:**
{searchResults}

**Domain Analysis Details:**
{domainAnalysis}

**Search Metadata:**
- Search Date: {searchMetadata.searchDate}
- Search Method: {searchMetadata.searchMethod}

**User Context:**
- Analyst: {userProfile.name} ({userProfile.position})
- Organization Unit: {userProfile.orgUnitName}
- Duty Station: {userProfile.dutyStation}

For each domain in the analysis, provide the most likely organization name. Focus on the domains list and return the JSON array mapping each domain to its corresponding organization.',
        NOW(),
        'Contact',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":2048}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]',
        '[]',
        'GetPartnerNamesFromGeminiAsync',
        'Batch lookup of organization names from email domains using Gemini AI',
        true,
        'Data Processing',
        true,
        240
    );

    -- Insert interaction_summary prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'interaction_summary',
        'You are an AI assistant that generates concise interaction summaries in a structured bullet point format. Focus on extracting key information and actionable items. Use the following structure:

**Date of interaction:** [Date]
**Key people involved:** [Contacts and their organizations]
**Key points:** [Main discussion points and decisions]
**Follow up needed:** [Action items and next steps]

Do not include a lead-in sentence. Extract specific details from the interaction data and populate each section accordingly.',
        'Provide a concise summary of the interaction "{subject}" that took place on {date} at {time} with the following participants:

**Interaction Details:**
- Subject: {subject}
- Date & Time: {date} at {time}
- Type: {type}
- Location: {location}
- Status: {status}

**Participants:**
- Contacts: {contactNames} from {partnerNames}
- UNOPS Staff: {userNames}
- Organization Units: {organizationUnits}

**Communication Info:**
- Email Addresses: {emailAddresses}
- Phone Numbers: {phoneNumbers}

**Documents:** {summary.totalDocuments} attached documents
**Description:** {description}

Focus on extracting key discussion points, decisions made, and any follow-up actions needed from the interaction description.',
        NOW(),
        'Interaction',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.2,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetInteractionDetailsForAIAsync',
        'Generates a comprehensive summary of interaction details including participants, content, context, and outcomes in a structured Markdown format.',
        true,
        'Interaction Management',
        true,
        45
    );

    -- Insert patner_category prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'patner_category',
        'You are an AI assistant that creates detailed partner category interaction summaries with contact details, interaction history, and overall partnership assessment in structured Markdown format.',
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
 
{promptData}',
        NOW(),
        'PartnerTree',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":8192}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetBasicPartnerDetailsAsync',
        'Creates detailed partner category  interaction summaries with contact details, interaction history, and overall partnership assessment in structured Markdown format.',
        true,
        'Partner Management',
        true,
        90
    );

    -- Insert partner_category_interactions_summary prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'partner_category_interactions_summary',
        'You are a partnerships assistant at the United Nations Office for Project Services (UNOPS) that creates detailed partner interaction summaries with contact details, interaction history, and overall partnership assessment in structured Markdown format.

Focus on recent interactions, key personnel, and strategic engagement patterns.

Output Format:
Provide the output in well-formed Markdown using the following structure:

## Summary of key interactions
[Introductory paragraph of interactions related to the partner in the last month, highlighting key high-level interactions]

For each interaction, use this format:
On [Date], [UNOPS Personnel] from **[org unit]** had a [Type of interaction]. [Brief description]. (If project-related, indicate country and number, otherwise say "not related to a specific project"). **See more** [hyperlinked]

## List of interactions

#### Interactions relevant to [orgunitname]
[Highlight key recent interactions in chronological format]

If no interactions exist, state: "Currently, there are no interactions available in Opportunity+ with [Partner Name]"',
        'The partner is: {partnerName}
Partner Information:
- Organization: {name}
- Status: {status}
- Partnership Level: {partnerGroup.name}
- Liaison Office: {liaisonOffice.name}

Contact Information:
- Total Contacts: {summary.totalContacts}
- Active Contacts: {summary.activeContacts}
- Most Active Contact: {summary.mostActiveContact}

Interaction History:
- Total Interactions: {summary.totalInteractions}
- Recent Interactions (30 days): {summary.recentInteractions}
- Last Interaction Date: {summary.lastInteractionDate}
- Average Interactions per Contact: {summary.averageInteractionsPerContact}

Recent Interactions Details:
{recentInteractions}

User Information:
- Name: {userProfile.name}
- Position: {userProfile.position}
- Organization Unit: {userProfile.orgUnitName}
- Duty Station: {userProfile.dutyStation}',
        NOW(),
        'PartnerTree',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":8192}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetBasicPartnerCategoryDetailsAsync',
        'Creates detailed interaction summaries for a partner category with contact details, interaction history, and overall partnership assessment in structured Markdown format.',
        true,
        'Partner Management',
        true,
        90
    );

    -- Insert partner_category_news prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'partner_category_news',
        'You are a partnerships assistant at the United Nations Office for Project Services (UNOPS) that creates detailed partner interaction summaries with contact details, interaction history, and overall partnership assessment in structured Markdown format.

Focus on recent interactions, key personnel, and strategic engagement patterns.

Output Format:
Provide the output in well-formed Markdown using the following structure:

## Summary of key interactions
[Introductory paragraph of interactions related to the partner in the last month, highlighting key high-level interactions]

For each interaction, use this format:
On [Date], [UNOPS Personnel] from **[org unit]** had a [Type of interaction]. [Brief description]. (If project-related, indicate country and number, otherwise say "not related to a specific project"). **See more** [hyperlinked]

## List of interactions

#### Interactions relevant to [org unit]
[Highlight key recent interactions in chronological format]

If no interactions exist, state: "Currently, there are no interactions available in Opportunity+ with [Partner Name]"',
        'The partner is: {partnerName}
Partner Information:
- Organization: {name}
- Status: {status}
- Partnership Level: {partnerGroup.name}
- Liaison Office: {liaisonOffice.name}

Contact Information:
- Total Contacts: {summary.totalContacts}
- Active Contacts: {summary.activeContacts}
- Most Active Contact: {summary.mostActiveContact}

Interaction History:
- Total Interactions: {summary.totalInteractions}
- Recent Interactions (30 days): {summary.recentInteractions}
- Last Interaction Date: {summary.lastInteractionDate}
- Average Interactions per Contact: {summary.averageInteractionsPerContact}

Recent Interactions Details:
{recentInteractions}

User Information:
- Name: {userProfile.name}
- Position: {userProfile.position}
- Organization Unit: {userProfile.orgUnitName}
- Duty Station: {userProfile.dutyStation}',
        NOW(),
        'PartnerTree',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]',
        '[{"googleSearch":{}}]',
        'GetPartnerCategoryNewsDetailsAsync',
        'Searches for and summarizes the latest news articles about a partner category, identifying current focus areas and trends from recent developments.',
        true,
        'Partner Management',
        true,
        150
    );

    -- Insert partner_group_news prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'partner_group_news',
        'You are a partnerships assistant at the United Nations Office for Project Services (UNOPS) that searches for and summarizes the latest news articles about partner organizations in a specific partner group.

Your job is to scan through the latest news articles using Google Search and come up with articles that are relevant to UNOPS and of specific relevance to the user''s role and the user''s location (duty station and country).

Use sources such as Google News as well as development news sites such as Devex and Donor Tracker to find the most recent and relevant news stories.

Output Format:
Provide the output in well-formed Markdown using the following structure:

**Lead-in sentence:** "Here are the [number] most recent news stories concerning partners in [Group Name], relevant to UNOPS"

For EACH article, use this format:
## [News Headline] [🔗](link-to-article)
**[Publication/Website Name]** | **[Publication Date]**

[One or two line summary of the article - use direct excerpts from the source when available]

---

Requirements:
- Find no more than 10 latest articles
- Focus on development-related news relevant to UNOPS mandate
- Include publication date, source, and direct link
- Separate each story with a horizontal line (---)
- Use hyperlink icons (🔗) for article links
- Prioritize recent articles (last 30 days preferred)

Do not include markdown code blocks or backticks in the response.',
        'Find and summarize the latest development news articles for partners in the partner group "{groupName}".

**Group Information:**
- Group Name: {groupName}
- Group Code: {groupCode}
- Group Type: {groupType}
- Total Partners: {partnerCount}

**Partners in Group:**
{partnerNames}

**Partner Details:**
{partners}

**User Context:**
- Name: {userProfile.name}
- Position: {userProfile.position}
- Organization Unit: {userProfile.orgUnitName}
- Duty Station: {userProfile.dutyStation}
- Country Context: {userProfile.dutyStationCountry}

**Search Context:**
- Focus Areas: {searchContext.focusAreas}
- News Sources: {searchContext.newsSources}
- Timeframe: {searchContext.timeframe}
- Relevance Criteria: {searchContext.relevance}

**Summary Statistics:**
- Total Partners to Search: {summary.totalPartners}
- Active Partners: {summary.activePartners}
- Search Date: {searchMetadata.searchDate}

**Audit Information:**
- Request Date: {auditInfo.createdDate}

Identify the partner names from the group data and find the latest development news articles relevant to somebody working at UNOPS in {userProfile.orgUnitName}. Focus on news that relates to international development, humanitarian work, infrastructure projects, procurement, or other areas aligned with UNOPS mandate.',
        NOW(),
        'PartnerTree',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]',
        '[{"googleSearch":{}}]',
        'GetPartnerGroupNewsDetailsAsync',
        'Searches for and summarizes the latest news articles about a partner group, identifying current focus areas and trends from recent developments.',
        true,
        'Partner Management',
        true,
        150
    );

    -- Insert partner_group_interactions_summary prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'partner_group_interactions_summary',
        'You are an AI assistant that creates detailed interaction summaries for partner groups. Focus on recent interactions, key personnel, and strategic engagement patterns. Use the following Markdown format:

## Summary of key interactions for [Group Name]

Provide an introductory paragraph highlighting key interactions from the last month with partners in this group. Focus on high-level strategic engagements and partnership activities.

## Recent Interactions by Partner

For each partner with recent interactions, use this format:

**[Partner Name]**
- **[Date]**: [Type] with [Contact Name] ([Contact Title]) - [Subject]
  - Key discussion: [Description]
  - Project context: [Project Info or "not related to a specific project"]
  - **[See more](interaction-link)**

## Partnership Analysis

### Collaboration Patterns
- **Most Active Partners:** [Partner Names with interaction counts]
- **Engagement Frequency:** [Analysis of interaction patterns]
- **Key Personnel:** [Most engaged contacts and their roles]

### Strategic Opportunities
- **Emerging Partnerships:** [New or growing relationships]
- **Collaboration Areas:** [Common themes and focus areas]
- **Follow-up Actions:** [Identified next steps and opportunities]

## Activity Summary
- **Total interactions in last 30 days:** [Count]
- **Partner engagement rate:** [Percentage of active partners]
- **Common interaction types:** [Most frequent types]
- **Geographic focus:** [Key regions or countries if applicable]

If no recent interactions are available, state: "Currently, there are no recent interactions available in Opportunity+ with partners in the [Group Name] group."

Format the response as clean Markdown without code blocks or backticks. Focus on actionable insights and strategic partnership development opportunities.',
        'Create a comprehensive interaction summary for the partner group "{groupName}" which includes {partnerCount} partners and their interaction history.

**Group Information:**
- Group: {groupName}
- Code: {groupCode}
- Type: {groupType}
- Total Partners: {partnerCount}
- Active Partners: {activePartners}

**Partners in Group:**
{partnerNames}

**Recent Activity (Last 30 Days):**
- Total Interactions: {summary.recentInteractions}
- Most Active Partners: {summary.mostActivePartners}
- Common Interaction Types: {summary.commonInteractionTypes}
- Last Interaction: {summary.lastInteractionDate}

**Detailed Recent Interactions:**
{recentInteractions}

**User Context:**
- Analyst: {userProfile.name} ({userProfile.position})
- Organization Unit: {userProfile.orgUnitName}
- Duty Station: {userProfile.dutyStation}

**Audit Information:**
- Analysis Date: {auditInfo.createdDate}
- Last Updated: {auditInfo.lastModifiedDate}

Please provide a comprehensive summary of recent interactions with partners in this group. Focus on partnership activities, collaboration patterns, key personnel involved, and strategic engagement opportunities.',
        NOW(),
        'PartnerTree',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        'GetBasicPartnerGroupDetailsAsync',
        'Creates detailed interaction summaries for a partner group with contact details, interaction history, and overall partnership assessment in structured Markdown format.',
        true,
        'Partner Management',
        true,
        90
    );

    -- Insert contact_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'contact_action',
        'You are processing contact data from Google Sheets for UNOPS. Convert each row into the exact JSON structure below. Only include non-empty fields. Required: lastName, email, title, partnerId.

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
{"id": <number if exists>, "salutation": "", "firstName": "", "lastName": "", "name": "", "title": "", "department": "", "email": "", "phone": "", "mobile": "", "partnerId": "", "dependents": ["partnerId"], "validationError": ""}

**RULES:**
- Set validationError for missing required fields (lastName, email, title, partnerId)
- Validate email format
- Set partnerId as string name, include "partnerId" in dependents for ID resolution
- Omit null/empty fields from JSON to keep it compact
- Compute name field as concatenation of salutation + firstName + lastName
- Only include "id" field in JSON output if ID column is present in source data
- Focus on essential fields only: name components, title, email, phone, partnerId, department

**RESPONSE FORMAT:**
{"Message":"Contact data processed successfully.","Category":"Contact","ResponseType":"Action", {"id": <number if exists>, "salutation": "", "firstName": "", "lastName": "", "name": "", "title": "", "department": "", "email": "", "phone": "", "mobile": "", "partnerId": "", "dependents": ["partnerId"], "validationError": ""}}

Return compact single-line JSON. If more input needed, set ResponseType to "Information". The "dependents" property is used to indicate which property in the JSON is an ID and is required to map. In this case, it is only the partnerId. Hence, DONOT update the dependents value. Send the dependents property''s value as-is ("dependents": ["partnerId"] -> do not replace partnerId). Also, include "id" only if it is present.',
        '',
        NOW(),
        'Contact',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        '',
        'Extracts contact information from raw data or conversation summaries and formats it into structured JSON for contact creation or updates.',
        true,
        'Contact Management',
        false,
        60
    );

    -- Insert bulk_contact_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'bulk_contact_action',
        'You are processing contact data from Google Sheets for UNOPS. Convert each row into the exact JSON structure below. Only include non-empty fields. Required: lastName, email, title, partnerId.

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
{"id": <number if exists>, "salutation": "", "firstName": "", "lastName": "", "name": "", "title": "", "department": "", "email": "", "phone": "", "mobile": "", "partnerId": "", "dependents": ["partnerId"], "validationError": ""}

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

Return compact single-line JSON. If more input needed, set ResponseType to "Information". The "dependents" property is used to indicate which property in the JSON is an ID and is required to map. In this case, it is only the partnerId. Hence, DONOT update the dependents value. Send the dependents property''s value as-is ("dependents": ["partnerId"] -> do not replace partnerId). Also, include "id" only if it is present.',
        '',
        NOW(),
        'Contact',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        ' ',
        'Processes bulk contact data from arrays or objects, converting them into structured JSON format with automatic name parsing and partner linking.',
        true,
        'Data Import',
        false,
        60
    );

    -- Insert partner_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'partner_action',
        'Extract partner information from the provided data and return ONLY a valid JSON object. Do not include any conversation, greetings, explanations, or markdown formatting. Return ONLY the raw JSON.

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

Return compact single-line JSON without line breaks or unnecessary whitespace.',
        '',
        NOW(),
        'Partner',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        '',
        'Extracts partner information from raw data or conversation summaries and formats it into structured JSON for partner creation or updates with validation of acceptable values.',
        true,
        'Partner Management',
        false,
        60
    );

    -- Insert partner_priorities prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'partner_priorities',
        'You are a partnerships assistant at the United Nations Office for Project Services (UNOPS) that analyzes partner priorities and identifies key focus areas in international development, funding opportunities, and potential entry points for UNOPS.

Use external sources such as Google Search to identify current priorities, funding commitments, and strategic opportunities.

Output Format:
Provide the output in well-formed Markdown using the following structure:

**Focus Areas:**
For each focus area, use this structure:
**[Focus Area Name]**
**Focus:** [Explanation of the partner''s focus area and their approach]
**Budget/Expenditure Commitments:** [Overview of expenditure or commitments potentially available to UNOPS]  
**Key UNOPS entry points:** [Overview of alignment with UNOPS strategy and key entry points]

**Overarching Considerations:**
[Cross-cutting priorities and strategic considerations]

Do not include markdown code blocks or backticks in the response.',
        'The partner is: {partnerName}
Partner Information:
- Organization: {name}
- Status: {status}
- Partnership Level: {partnerGroup.name}
- Liaison Office: {liaisonOffice.name}
- Established: {partnership.establishedDate}
- Engagement Level: {engagement.engagementFrequency}
- Last Activity: {partnership.lastActivity}

Recent Engagement:
- Total Contacts: {summary.totalContacts} 
- Recent Interactions: {summary.recentInteractions} in last 30 days
- Last Interaction: {summary.lastInteractionDate}
- Key Contacts: {engagement.keyContactPoints}

User Information:
- Name: {userProfile.name}
- Position: {userProfile.position}
- Organization Unit: {userProfile.orgUnitName}
- Duty Station: {userProfile.dutyStation}
- Country Context: {userProfile.dutyStationCountry}',
        NOW(),
        'Partner',
        0,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        '[{ "category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "OFF" }, { "category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "OFF" }, { "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "OFF" }, {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "OFF" }]',
        '[{"googleSearch":{}}]',
        'GetBasicPartnerDetailsAsync',
        'Give an overview of partner priorities',
        true,
        'Partner Management',
        true,
        180
    );

    -- Insert bulk_partner_action prompt
    INSERT INTO public."AiPrompt" (
        "Type", "SystemInstructions", "UserPrompt", "CreatedAt", "Name", "Status", "ContentConfig", 
        "GenerationConfig", "Location", "Model", "Project", "SafetySettings", 
        "ToolsConfig", "DataRetrievalMethod", "Description", "AdminCanChange", 
        "Feature", "UseCache", "CacheInvalidationMinutes"
    ) VALUES (
        'bulk_partner_action',
        'You are processing partner data from Google Sheets for UNOPS. Convert each row into the exact JSON structure below. Only include non-empty fields to keep JSON compact.

**MANDATORY FIELDS:** name, partnerShortDescription, status (default: "Active")

**HEADER MAPPING:**
"ID"/"Partner ID" → id (number, only if present)
"Partner Name"/"Organization"/"Company" → name
"Short Name"/"Acronym"/"Abbreviation" → partnerShortDescription
"Long Description" → partnerLongDescription
"Status" → always default to "Draft"
"Partner Group" → partnerGroupId(number / text) - whatever you find should be added here
"Liaison Office" → liaisonOfficeId (number / text) - whatever you find should be added here
"Partner Focal Point" → partnerFocalPointUserId (number / text) - whatever you find should be added here
organizationHierarchyIds-> Array of Org units that you find 

**ESSENTIAL PARTNER JSON FORMAT:**
{"id": <number>, "name": "", "partnerShortDescription": "", "partnerLongDescription": "", "status": "Draft", "partnerGroupId": null, "liaisonOfficeId": null, "partnerFocalPointUserId": null, "organizationHierarchyIds": [], "dependents": ["partnerGroupId", "liaisonOfficeId", "partnerFocalPointUserId", "organizationHierarchyIds"], "validationError": ""}

**RULES:**
- Set validationError for missing mandatory fields
- Map text names to ID fields, include in dependents for resolution
- Omit null/empty fields to keep JSON compact
- Default status to "Draft"
- Default ID fields to null
- Only include "id" field in JSON output if ID column is present in source data
- Focus on essential fields only: name, partnerShortDescription, partnerLongDescription, status, and key ID references

**RESPONSE FORMAT:**
{"Message":"Partner data processed successfully.","Category":"Partner","ResponseType":"Action","records":[...]}

Return compact single-line JSON. If more input needed, set ResponseType to "Information". Include ID column in the response if and only if it is present, else ignore that key. Send the dependents field asis with the same values. It should always have "dependents": ["partnerGroupId", "liaisonOfficeId", "partnerFocalPointUserId", "organizationHierarchyIds"]',
        '',
        NOW(),
        'Partner',
        1,
        '{"role":"user","parts":[{"text":"{promptData}"}]}',
        '{"temperature":0.1,"top_p":0.2,"max_output_tokens":65535}',
        'europe-west4',
        'gemini-2.5-flash-lite',
        '{{PROJECT_ID}}',
        NULL,
        '[]',
        '',
        'Processes bulk partner data from arrays or objects, converting them into structured JSON format with validation of acceptable values and automatic field mapping.',
        true,
        'Data Import',
        false,
        60
    );

    RAISE NOTICE 'AI prompts inserted successfully: 18 records';
END $$;
