using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class AiPromptSeeder
    {
        public static async Task SeedAiPromptsAsync(UNOPSAppDbContext context)
        {
            if (await context.AiPrompts.AnyAsync())
            {
                return;
            }

            var aiPrompts = new List<AiPrompt>
            {
                new AiPrompt
                {
                    Type = "contact_interactions_summary",
                    Prompt = "I am providing a contact name. I need you to generate a summary in Markdown format, using the following template:\n\n## Contact Summary\n\n**Name:** [Contact Name]  \n**Email:** [Contact Email]\n**Title:** [Contact Title]\n**Relationship:** [Brief description of time UNOPS has engaged with the Contact and main things UNOPS has done with the contact]\n\n**Key Interactions:**\n\n*   **[Date of Interaction] - [Type of Interaction]:** [Brief description of interaction]\n*   **[Date of Interaction] - [Type of Interaction]:** [Brief description of interaction]\n\n**Partner Information:**\n\n*   **Organization:** [Partner Name]\n*   **Status:** [Partner Status]\n\n**Considerations**\n** [Summary of any issues identified with the Contact or the Partner] \n\n**Additional Notes:**\n\n*   [Check if there is a CV linked to the contact]\n*   [Any other relevant information about the contact or their interactions]\n\nPlease format the response as clean Markdown without code blocks or backticks. Please try to fill in with as much information as available in the contact's page. Please surface the Partner related to the contact, his/her title, email, and any other information available related to this contact in the system. If any information is missing, simply omit that section.\n\nData: {promptData}",
                    CreatedAt = DateTime.UtcNow,
                    Name = "Contact",
                    Status = (EntityStatus)1,
                    ContentConfig = "{\"role\":\"user\",\"parts\":[{\"text\":\"{promptData}\"}]}",
                    GenerationConfig = "{\"temperature\":0.1,\"top_p\":0.2,\"max_output_tokens\":65535}",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = "[]",
                    PromptFunction = "GetContactWithInteractionsAsync",
                    Description = "Generates a comprehensive summary of contact information including partner details and interaction history in a structured format.",
                    AdminCanChange = true
                },
                new AiPrompt
                {
                    Type = "bulk_partner_action",
                    Prompt = "You are an AI assistant processing partner data from Google Sheets for UNOPS. Convert each row into the exact JSON structure below. Only include non-empty fields to keep JSON compact.\n\n**MANDATORY FIELDS:** name, partnerShortDescription, status (default: \"Active\")\n\n**HEADER MAPPING:**\n\"ID\"/\"Partner ID\" → id (number, only if present)\n\"Partner Name\"/\"Organization\"/\"Company\" → name\n\"Short Name\"/\"Acronym\"/\"Abbreviation\" → partnerShortDescription\n\"Long Description\" → partnerLongDescription\n\"Status\" → status (\"Draft\", \"Active\", \"Closed\", \"Archived\")\n\"Partner Category\" → partnerCategoryId (number)\n\"Liaison Office\" → liaisonOfficeId (number)\n\"Partner Focal Point\" → partnerFocalPointUserId (number)\n\"ERP Dimension\" → erpDimValue (number)\n\n**ESSENTIAL PARTNER JSON FORMAT:**\n{\"name\": \"\", \"partnerShortDescription\": \"\", \"partnerLongDescription\": \"\", \"status\": \"Active\", \"partnerCategoryId\": null, \"liaisonOfficeId\": null, \"partnerFocalPointUserId\": null, \"erpDimValue\": null, \"dependents\": [\"partnerCategoryId\", \"liaisonOfficeId\", \"partnerFocalPointUserId\"], \"validationError\": \"\"}\n\n**RULES:**\n- Set validationError for missing mandatory fields\n- Map text names to ID fields, include in dependents for resolution\n- Omit null/empty fields to keep JSON compact\n- Default status to \"Active\"\n- Default ID fields to null\n- Only include \"id\" field in JSON output if ID column is present in source data\n- Focus on essential fields only: name, partnerShortDescription, partnerLongDescription, status, and key ID references\n\n**RESPONSE FORMAT:**\n{\"Message\":\"Partner data processed successfully.\",\"Category\":\"Partner\",\"ResponseType\":\"Action\",\"records\":[...]}\n\nReturn compact single-line JSON. If more input needed, set ResponseType to \"Information\".\n\nInput data: {promptData}",
                    CreatedAt = DateTime.UtcNow,
                    Name = "Partner",
                    Status = (EntityStatus)1,
                    ContentConfig = "{ \"role\": \"user\", \"parts\": [ { \"text\": \"{promptData}\" } ] }",
                    GenerationConfig = "{ \"temperature\": 0.1, \"top_p\": 0.2, \"max_output_tokens\": 65535 }",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = null,
                    PromptFunction = "",
                    Description = "Processes bulk partner data from arrays or objects, converting them into structured JSON format with validation of acceptable values and automatic field mapping.",
                    AdminCanChange = false
                },
                new AiPrompt
                {
                    Type = "partner_priorities",
                    Prompt = "I am providing a JSON object containing partner information and the fact that I work in Senegal. \nIdentify the name of the partner from that JSON data and using external sources such as google search, identify the key focus areas in international development, potentially available funding or commitments and potential entry points for UNOPS.\nIn additon provide an overview of crosscutting priorities as \"Overarching Considerations:\"\n\nPLease use the following structure\n\n**Focus Areas:**\nFor each of the focus areas, please use the following structure\n**[Focus Area]**\n**Focus: ** [Provide explanation of the partner's focus area and thier approach]\n**Budget/Expenditure Commitments: ** [Provide an overview of expenditure or commitments that are potentially available to UNOPS]\n**Key UNOPS entry points:** [Provide an overview of how this aligns with UNOPS strategy and priorities and key entry points ]\n(add 2 line breaks)\n\n\nJSON Data:\n{promptData}\n\nSTRICTLY do not use the word \"markdown\" when you convert the final result to Markdown. Please provide the generated Markdown summary based on these instructions. Add additional line space after each detail. If any detail that you are instructed to provide is unavailable, do not include that in the response. Do not assume any detail. Please do not include \"```markdown\\n\" in the response.",
                    CreatedAt = DateTime.UtcNow,
                    Name = "partner_priorities",
                    Status = (EntityStatus)0,
                    ContentConfig = "{\"role\":\"user\",\"parts\":[{\"text\":\"{promptData}\"}]}",
                    GenerationConfig = "{\"temperature\":1,\"top_p\":0.2,\"max_output_tokens\":65535}",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = "[{ \"category\": \"HARM_CATEGORY_HATE_SPEECH\", \"threshold\": \"OFF\" }, { \"category\": \"HARM_CATEGORY_DANGEROUS_CONTENT\", \"threshold\": \"OFF\" }, { \"category\": \"HARM_CATEGORY_SEXUALLY_EXPLICIT\", \"threshold\": \"OFF\" }, {\"category\": \"HARM_CATEGORY_HARASSMENT\", \"threshold\": \"OFF\" }]",
                    ToolsConfig = "[{\"googleSearch\":{}}]",
                    PromptFunction = "GetBasicPartnerDetailsAsync",
                    Description = "Give an overview of partner priorities",
                    AdminCanChange = true
                },
                new AiPrompt
                {
                    Type = "partner_news",
                    Prompt = "I am providing a JSON object containing partner information and that I work in [Org Unit].\nIdentify the name of the partner from that JSON data and find the latest development news articles on the partner. \nFor sources, please use Google News as well as development news sites such as Devex and donor tracker. \nStories are presented in order of newest to oldest. Try to avoid repeating the same stories from multiple sources.\nDo not include any introductory phrases to your answer, just the desired format below and do not include padding between new lines.\n\n**Desired Output Format:**\n\nHere are the most recent news stories for [Partner Name] (bold text, get this from the partners property from the JSON) (give 2 line breaks)\n### Global News\n (Text for is underlined Bold 18pt with an icon indicaing that the list can be expanded / collapsed. ) (line break)\nThis is followed by a list of the 5 most recent global news stories)\n\n### Regional / Local news \n(Text for header is underlined Bold 18pt with with an icon indicaing that the list can be expanded / collapsed. ) (line break)\n(This is followed by a list of the 5 most recent news stories releavant to my country. If no news stories are available from the last month my country, then please look news stories in my UN region)\n\nFor each news article please use the following layout.\n**News Headline** Text is  bright green (add one line break)\n**Description** Short summary of the news article (normal text in black), maximum 200 characters. Followed by **[hyperlink]** text contains the word \"Read Article\" with a hyperlink to open the specific news article in a separate browser page (insert line break)\nSource Name - DateOfNews: (in dd mmm YYYY format)\n\n(add 2 line breaks)\n\nAfter the top 5 stories in each section, please include\n**[Hyperlink]** (this text with the phrase \"See more stories\" with an embedded hyperlink to open a google news page searching for the partner with stories ordered from newest to oldest)\n\n(add 2 line breaks)\n\nJSON Data:\n{promptData}\n\nSTRICTLY do not use the word \"markdown\" when you convert the final result to Markdown. Please provide the generated Markdown summary based on these instructions. If any detail that you are instructed to provide is unavailable, do not include that in the response. Do not assume any detail. Please do not include \"```markdown\\n\" in the response.",
                    CreatedAt = DateTime.UtcNow,
                    Name = "Partner",
                    Status = (EntityStatus)1,
                    ContentConfig = "{\"role\":\"user\",\"parts\":[{\"text\":\"{promptData}\"}]}",
                    GenerationConfig = "{\"temperature\":0.1,\"top_p\":0.1,\"max_output_tokens\":65535}",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = "[{ \"category\": \"HARM_CATEGORY_HATE_SPEECH\", \"threshold\": \"OFF\" }, { \"category\": \"HARM_CATEGORY_DANGEROUS_CONTENT\", \"threshold\": \"OFF\" }, { \"category\": \"HARM_CATEGORY_SEXUALLY_EXPLICIT\", \"threshold\": \"OFF\" }, {\"category\": \"HARM_CATEGORY_HARASSMENT\", \"threshold\": \"OFF\" }]",
                    ToolsConfig = "[{\"googleSearch\":{}}]",
                    PromptFunction = "GetBasicPartnerDetailsAsync",
                    Description = "Searches for and summarizes the latest news articles about a partner organization, identifying current focus areas and trends from recent developments.",
                    AdminCanChange = true
                },
                new AiPrompt
                {
                    Type = "general_information",
                    Prompt = "Strictly return the response in JSON format as below - \n\n{Category: \"General\", ResponseType: \"INFORMATION\", Message: \"Add your response here\"}",
                    CreatedAt = DateTime.UtcNow,
                    Name = "AiAssistant",
                    Status = (EntityStatus)1,
                    ContentConfig = "{ \"role\": \"user\", \"parts\": [ { \"text\": \"{promptData}\" } ] }",
                    GenerationConfig = "{ \"temperature\": 0.1, \"top_p\": 0.2, \"max_output_tokens\": 65535 }",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = null,
                    PromptFunction = "",
                    Description = "Returns a general response in JSON format.",
                    AdminCanChange = true
                },
                new AiPrompt
                {
                    Type = "partnertree_action",
                    Prompt = "I am sending you partner tree (level) data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)\n\nJSON format:\n{ \"\"Message\"\": \"\"Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message\"\", \"\"Category\"\": \"\"PartnerTree\"\", ResponseType: \"\"Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION\"\", \"\"description\"\": \"\"\"\", \"\"code\"\": \"\"\"\", \"\"type\"\": \"\"\"\", \"\"parent\"\": \"\"\"\", \"\"name\"\": \"\"\"\" }\n\nSomethings to consider about the JSON format above are:\n\"\"code\"\" looks like an ID field but text which will be similar to \"\"ACADEMIC_TRAINING_RESEARC\"\". If you cannot find a data in such a format, autogenerate a code of the similar kind based on the name and description you extract.\n\"\"parent\"\" is also look-alike of code but the code of the parent. If you cannot find it in the data, leave it blank. If parent is left blank, consider \"\"type\"\" as Level_1 and mention it in the Message.\n\"\"type\"\" can be Level_1, Level_2, Level_3 or Level_4. Level_1 will always have parent as blank.\n\nSTRICTLY do not use the word \"markdown\" while converting the final response to the final JSON.\n\nBe very polite and kind and greet the user. Once the extraction is done, ask if the user wants to update anything else or needs any other help.\n\nThe prompt could be an extracted text from an audio or an image OR could be a summary of the conversation with the user. The summary could be talking about multiple entities. Only extract the details relevant to Partner level and the latest details. This is just a one time call to you, so your task is to just extract data from the provided information if possible. For example, there could have been multiple discussions about the partner levels. Pick the latest request. Use this to form the JSON. Whether the prompt is an extracted text or a summary will be highlighted before the message begins (for example: Summary: <summary> OR Extracted text: <extracted text>)\n\nPrompt: \n{promptData}",
                    CreatedAt = DateTime.UtcNow,
                    Name = "PartnerTree",
                    Status = (EntityStatus)1,
                    ContentConfig = "{ \"role\": \"user\", \"parts\": [ { \"text\": \"{promptData}\" } ] }",
                    GenerationConfig = "{ \"temperature\": 0.1, \"top_p\": 0.2, \"max_output_tokens\": 65535 }",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = null,
                    PromptFunction = "",
                    Description = "Extracts partner tree (level) information from raw data and formats it into structured JSON with auto-generated codes and hierarchical type determination.",
                    AdminCanChange = false
                },
                new AiPrompt
                {
                    Type = "contact_action",
                    Prompt = "You are an AI assistant processing contact data. Extract contact information from the provided data and return ONLY a valid JSON object. Do not include any conversation, greetings, explanations, or markdown formatting. Return ONLY the raw JSON.\n\nJSON format:\n{\"Message\": \"Data extracted successfully\", \"Category\": \"Contact\", \"ResponseType\": \"Action\", \"salutation\": \"\", \"firstName\": \"\", \"lastName\": \"\", \"title\": \"\", \"email\": \"\", \"phone\": \"\", \"mobile\": \"\", \"status\": \"Active\", \"partnerId\": \"\", \"department\": \"\", \"dependents\": [\"partnerId\"]}\n\n**RULES:**\n- Required fields: lastName, email, title, partnerId\n- Salutation: Auto-detect from Mr., Ms., Mrs., Dr., Prof., Sir, Madam\n- Partner: Set partnerId as string name, include \"partnerId\" in dependents for ID resolution\n- Omit null/empty fields from JSON to keep it compact\n- Always set status to \"Active\"\n- Set validationError for missing required fields\n- Only include \"id\" field in JSON output if ID is present in source data\n- Focus on essential fields only: name components, title, email, phone, partnerId, department\n\n**HEADER MAPPING:**\n\"ID\"/\"Contact ID\" → id (number, only if present)\n\"Full Name\"/\"Name\"/\"Contact Name\" → firstName + lastName\n\"Email\"/\"Email Address\"/\"E-mail\" → email\n\"Phone\"/\"Phone Number\"/\"Telephone\" → phone\n\"Mobile\"/\"Cell Phone\"/\"Mobile Number\" → mobile\n\"Company\"/\"Organization\"/\"Partner\"/\"Employer\" → partnerId\n\"Job Title\"/\"Position\"/\"Role\" → title\n\"Department\"/\"Division\"/\"Unit\" → department\n\nReturn compact single-line JSON without line breaks or unnecessary whitespace.\n\nInput data: {promptData}",
                    CreatedAt = DateTime.UtcNow,
                    Name = "Contact",
                    Status = (EntityStatus)1,
                    ContentConfig = "{\"role\":\"user\",\"parts\":[{\"text\":\"{promptData}\"}]}",
                    GenerationConfig = "{\"temperature\":0.1,\"top_p\":0.2,\"max_output_tokens\":65535}",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = "[]",
                    PromptFunction = "",
                    Description = "Extracts contact information from raw data or conversation summaries and formats it into structured JSON for contact creation or updates.",
                    AdminCanChange = false
                },
                new AiPrompt
                {
                    Type = "partner_action",
                    Prompt = "You are an AI assistant processing partner data. Extract partner information from the provided data and return ONLY a valid JSON object. Do not include any conversation, greetings, explanations, or markdown formatting. Return ONLY the raw JSON.\n\nJSON format:\n{\"Message\": \"Data extracted successfully\", \"Category\": \"Partner\", \"ResponseType\": \"Action\", \"name\": \"\", \"partnerShortDescription\": \"\", \"partnerLongDescription\": \"\", \"status\": \"Active\", \"partnerCategoryId\": null, \"liaisonOfficeId\": null, \"partnerFocalPointUserId\": null, \"erpDimValue\": null, \"dependents\": [\"partnerCategoryId\", \"liaisonOfficeId\", \"partnerFocalPointUserId\"]}\n\n**RULES:**\n- Required fields: name, partnerShortDescription, status\n- Status: Default to \"Active\"\n- Partner: Set ID fields as string names, include in dependents for ID resolution\n- Omit null/empty fields from JSON to keep it compact\n- Always set status to \"Active\"\n- Default ID fields to null\n- Only include \"id\" field in JSON output if ID is present in source data\n- Focus on essential fields only: name, partnerShortDescription, partnerLongDescription, status, and key ID references\n\n**HEADER MAPPING:**\n\"ID\"/\"Partner ID\" → id (number, only if present)\n\"Partner Name\"/\"Organization\"/\"Company\" → name\n\"Short Name\"/\"Acronym\"/\"Abbreviation\" → partnerShortDescription\n\"Long Description\" → partnerLongDescription\n\"Partner Category\" → partnerCategoryId\n\"Liaison Office\" → liaisonOfficeId\n\"Partner Focal Point\" → partnerFocalPointUserId\n\"ERP Dimension\" → erpDimValue\n\nReturn compact single-line JSON without line breaks or unnecessary whitespace.\n\nInput data: {promptData}",
                    CreatedAt = DateTime.UtcNow,
                    Name = "Partner",
                    Status = (EntityStatus)1,
                    ContentConfig = "{\"role\":\"user\",\"parts\":[{\"text\":\"{promptData}\"}]}",
                    GenerationConfig = "{\"temperature\":0.1,\"top_p\":0.2,\"max_output_tokens\":65535}",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = "[]",
                    PromptFunction = "",
                    Description = "Extracts partner information from raw data or conversation summaries and formats it into structured JSON for partner creation or updates with validation of acceptable values.",
                    AdminCanChange = false
                },
                new AiPrompt
                {
                    Type = "bulk_interaction_action",
                    Prompt = "You are an AI assistant that processes interaction data for bulk import. You will receive interaction data as an array of arrays (with optional header) or an array of objects, or text extracted from audio/image.\n\nConvert each item into the exact JSON structure shown below. Only include non-empty fields.\n\n**Required fields:** type, date, subject\n**Validation rules:**\n- Map contact names to contactIds (keep as text if name, number if ID)\n- Map partner names to partnerIds (keep as text if name, number if ID)\n- Map user names to userIds (keep as text if name, number if ID)\n- Format date as ISO 8601 timestamp (YYYY-MM-DDTHH:mm:ss.sssZ)\n- Default status to \"Active\"\n- Include dependents for all ID fields that are text names\n- Based on the context of the message, auto-detect the date.\n- Put one of the contactIds into contactId\n- Only include \"id\" field in JSON output if ID column is present in source data\n- Focus on essential fields only: type, date, subject, description, contactId, status, emailAddresse - Extract email addresses from the interaction content and populate emailAddresses as an array of strings\n\n**Interaction types:** \"Email\", \"Chat\", \"Phone\", \"VideoMeeting\", \"InPersonMeeting\", \"Other\"\n\n**HEADER MAPPING:**\n\"ID\"/\"Interaction ID\" → id (number, only if present)\n\"Type\" → type\n\"Date\" → date\n\"Subject\" → subject\n\"Description\" → description\n\"Contact\" → contactId\n\"Status\" → status\n\n**ESSENTIAL INTERACTION JSON FORMAT:**\n{\"type\": \"\", \"date\": \"\", \"subject\": \"\", \"description\": \"\", \"status\": \"Active\", \"contactId\": \"\", \"emailAddresses\": [], \"dependents\": [\"contactId\"], \"validationError\": \"\"}\n\n**Response format:** {\"Message\":\"Action completed successfully.\", \"Category\":\"Interaction\", \"ResponseType\":\"Action\", \"records\":[...]}\n\nReturn compact single-line JSON. If more input needed, set ResponseType to \"Information\".\n\nInput data: {promptData}",
                    CreatedAt = DateTime.UtcNow,
                    Name = "Interaction",
                    Status = (EntityStatus)1,
                    ContentConfig = "{ \"role\": \"user\", \"parts\": [ { \"text\": \"{promptData}\" } ] }",
                    GenerationConfig = "{ \"temperature\": 0.1, \"top_p\": 0.2, \"max_output_tokens\": 65535 }",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = null,
                    PromptFunction = "",
                    Description = "Processes bulk interaction data from arrays or objects, converting them into structured JSON format with automatic date parsing, field mapping, and validation of interaction types.",
                    AdminCanChange = false
                },
                new AiPrompt
                {
                    Type = "bulk_contact_action",
                    Prompt = "You are an AI assistant processing contact data from Google Sheets for UNOPS. Convert each row into the exact JSON structure below. Only include non-empty fields. Required: lastName, email, title, partnerId.\n\n**HEADER MAPPING:**\n\"ID\"/\"Contact ID\" → id (number, only if present)\n\"Full Name\"/\"Name\"/\"Contact Name\" → firstName + lastName + name (computed as full name)\n\"Email\"/\"Email Address\"/\"E-mail\" → email\n\"Phone\"/\"Phone Number\"/\"Telephone\" → phone\n\"Mobile\"/\"Cell Phone\"/\"Mobile Number\" → mobile\n\"Company\"/\"Organization\"/\"Partner\"/\"Employer\" → partnerId (string, add to dependents)\n\"Job Title\"/\"Position\"/\"Role\" → title\n\"Department\"/\"Division\"/\"Unit\" → department\n\n**SALUTATION DETECTION:**\nAuto-detect from: Mr., Ms., Mrs., Dr., Prof., Sir, Madam\n\n**ESSENTIAL CONTACT JSON FORMAT:**\n{\"salutation\": \"\", \"firstName\": \"\", \"lastName\": \"\", \"name\": \"\", \"title\": \"\", \"department\": \"\", \"email\": \"\", \"phone\": \"\", \"mobile\": \"\", \"status\": \"Active\", \"partnerId\": \"\", \"dependents\": [\"partnerId\"], \"validationError\": \"\"}\n\n**RULES:**\n- Set validationError for missing required fields (lastName, email, title, partnerId)\n- Validate email format\n- Set partnerId as string name, include \"partnerId\" in dependents for ID resolution\n- Omit null/empty fields from JSON to keep it compact\n- Always set status to \"Active\"\n- Compute name field as concatenation of salutation + firstName + lastName\n- Only include \"id\" field in JSON output if ID column is present in source data\n- Focus on essential fields only: name components, title, email, phone, partnerId, department\n\n**RESPONSE FORMAT:**\n{\"Message\":\"Contact data processed successfully.\",\"Category\":\"Contact\",\"ResponseType\":\"Action\",\"records\":[...]}\n\nReturn compact single-line JSON. If more input needed, set ResponseType to \"Information\".\n\nInput data: {promptData}",
                    CreatedAt = DateTime.UtcNow,
                    Name = "Contact",
                    Status = (EntityStatus)1,
                    ContentConfig = "{\"role\":\"user\",\"parts\":[{\"text\":\"{promptData}\"}]}",
                    GenerationConfig = "{\"temperature\":0.1,\"top_p\":0.2,\"max_output_tokens\":65535}",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = "[]",
                    PromptFunction = "",
                    Description = "Processes bulk contact data from arrays or objects, converting them into structured JSON format with automatic name parsing and partner linking.",
                    AdminCanChange = false
                },
                new AiPrompt
                {
                    Type = "partner_interactions_summary",
                    Prompt = "I am providing a partner name. For example, the user may ask the question \"Can you give me a summary of the latest interactions with [Partner]?\"\n\nI need you to generate a summary in Markdown format, using the following template:\n\n###Partner Summary\nUsing the partner name produce a short 3 sentence summary of the organisation based on internal information, do not use any other internal data points from the partner record. \n\n##Summary of key interactions\nProvide an introductory paragraph of interactions related to the partner in the last month. Highlight any key high-level interactions.\n\nFor example, using the following format, generate a summary of an interaction that looks like this: \n\nOn 19/09/2024, Beth Hayes from org unit (in bold) had a Type of interaction. It was discussed the need for a significant reduction in energy-efficient procedures, which could be achieved through enterprise-level investments. (If the interaction is related to a project please indicate the country and number, and if not say \"not related to a specific project\").**See more**  (text contains the word \"See more\" with a hyperlink to open the specific interaction record). (give line breaks after each interaction summary)\n\n##List of interactions\n\nThere have been several recent interactions between UNOPS and [Partner]\n[Date of interaction]: A high-level meeting between UNOPS' [Personnel name, Personnel title] and the World Bank's [Contact name, contact title] to discuss ongoing projects. \n[Date of interaction]: a meeting between the World Bank and UNOPS' project teams to discuss project [Engagement name, engagement code] and project process, where key milestones such as timely delivery of supplies were identified. \n[Date of interaction]: high-level meeting between the World Bank's [Contact name, contact title], and the UNOPS delegation at the [Event name]\n.**See more**  (text contains the word \"See more\" with a hyperlink to open the specific interaction record)\n\n##Considerations\n** [Summary of any issues identified with the Contact or the Partner] \n\n##Portfolio and Pipeline\n** [Surface of internal data regarding UNOPS and World Bank Portfolio from dashboard: EA, and delivery key project examples and main regions where we operate, key impact figures beneficiaries, etc).] As a source, please use: OuP, partnerships dashboards, GDrive.\n** [Surface of external data regarding World Bank Portfolio: total portfolio, portfolio per regions, and delivery key project examples and other key impact figures]\n\nInstructions:\n \n{promptData}\n\nSTRICTLY do not use the word \"markdown\" when you convert the final result to Markdown. Please provide the generated Markdown summary based on these instructions. Add additional line space after each detail. If any detail that you are instructed to provide is unavailable, do not include that in the response. The final response from you should give me a quick summary of the partner. Do not assume any detail. Please do not include \"```markdown\\n\" in the response.",
                    CreatedAt = DateTime.UtcNow,
                    Name = "Partner",
                    Status = (EntityStatus)1,
                    ContentConfig = "{\"role\":\"user\",\"parts\":[{\"text\":\"{promptData}\"}]}",
                    GenerationConfig = "{\"temperature\":0.7,\"top_p\":0.2,\"max_output_tokens\":65535}",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = "[{\"googleSearch\":{}}]",
                    PromptFunction = "GetPartnerWithContactsAndInteractionsAsync",
                    Description = "Creates detailed partner interaction summaries with contact details, interaction history, and overall partnership assessment in structured Markdown format.",
                    AdminCanChange = true
                },
                new AiPrompt
                {
                    Type = "interaction_action",
                    Prompt = "I am sending you interaction data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON. Strictly return a JSON even if you cannot find any data. The user could just be trying to have a normal conversation. Send the response in the Message property of the JSON (look at the given format below)\n\nJSON format:\n{ \"Message\": \"Response to the user. If you were able to extract the data successfully, reply as Action completed successfully or any equivalent message\", \"Category\": \"Interaction\", ResponseType: \"Action/Information (if you extracted the data successfully, send it as Action. If you are asking for more information, send it as INFORMATION\", \"type\": \"\", \"date\": \"\", \"subject\": \"\", \"description\": \"\", \"contactId\": \"\", \"status\": \"Active\", \"emailAddresses\": [], dependents: [\"contactId\"]  }\n\nSome things to consider about the JSON format above are:\n\"type\" is the Interaction type which could be \"Email\", \"Chat\", \"Phone\", \"VideoMeeting\", \"InPersonMeeting\"\n\"date\" Ensure the date is formatted as ISO 8601 timestamp\n\"subject\" Brief summary or title of the interaction\n\"description\" Detailed content of the interaction\n* Interaction can be linked to a contact. The JSON must have a property called contactId. Generally, the user will not know the ID of the Contact and hence will pass it as a Name. \nPut the name in the \"contactId\" property value and add contactId to the dependents property as an array. for example, dependents: [\"contactId\"]\n* Only include \"id\" field in JSON output if ID is present in source data\n* Focus on essential fields only: type, date, subject, description, contactId, status, emailAddresses\"emailAddresses\" should be an array of email addresses extracted from the interaction content. If no email addresses are found, use an empty array []\n\n**HEADER MAPPING:**\n\"ID\"/\"Interaction ID\" → id (number, only if present)\n\"Type\" → type\n\"Date\" → date\n\"Subject\" → subject\n\"Description\" → description\n\"Contact\" → contactId\n\nSTRICTLY do not use the word \"markdown\" while converting the final response to the final JSON.\n\nBe very polite and kind and greet the user. Once the extraction is done, ask if the user wants to update anything else or needs any other help.\n\nThe prompt could be an extracted text from an audio or an image OR could be a summary of the conversation with the user. The summary could be talking about multiple entities. Only extract the details relevant to Interactions and the latest details. For example, there could have been multiple discussions about Interactions. Pick the latest request. Use this to form the JSON. Whether the prompt is an extracted text or a summary will be highlighted before the message begins (for example: Summary: <summary> OR Extracted text: <extracted text>)\n\nPrompt: \n{promptData}",
                    CreatedAt = DateTime.UtcNow,
                    Name = "Interaction",
                    Status = (EntityStatus)1,
                    ContentConfig = "{\"role\":\"user\",\"parts\":[{\"text\":\"{promptData}\"}]}",
                    GenerationConfig = "{\"temperature\":0.1,\"top_p\":0.2,\"max_output_tokens\":65535}",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = "[]",
                    PromptFunction = "GetInteractionDetailsAsync",
                    Description = "Retrieves and summarizes interaction information in bullet points for easy understanding and reference.",
                    AdminCanChange = false
                },
                new AiPrompt
                {
                    Type = "domain_organization_lookup",
                    Prompt = "I am providing a JSON array containing email domains. For each domain, identify the most likely organization or company name that uses that domain.\n\n**Input Format:**\n{promptData}\n\n**Desired Output Format:**\nReturn a JSON array with the same order as input, where each element contains:\n{\n  \"domain\": \"[original domain]\",\n  \"organization\": \"[organization name]\"\n}\n\n**Instructions:**\n- For each domain, provide the most likely organization name\n- If you cannot determine a likely organization name, use \"Unknown\" \n- Do not include explanations or additional text\n- Return only the JSON array\n- Ensure the response is valid JSON format\n- Maintain the same order as the input domains\n\nExample input: [\"microsoft.com\", \"google.com\", \"unknowndomain123.com\"]\nExample output: [{\"domain\": \"microsoft.com\", \"organization\": \"Microsoft Corporation\"}, {\"domain\": \"google.com\", \"organization\": \"Google Inc.\"}, {\"domain\": \"unknowndomain123.com\", \"organization\": \"Unknown\"}]",
                    CreatedAt = DateTime.UtcNow,
                    Name = "Domain Organization Lookup",
                    Status = (EntityStatus)1,
                    ContentConfig = "{ \"role\": \"user\", \"parts\": [ { \"text\": \"{promptData}\" } ] }",
                    GenerationConfig = "{ \"temperature\": 0.1, \"top_p\": 0.2, \"max_output_tokens\": 2048 }",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = "[{ \"category\": \"HARM_CATEGORY_HATE_SPEECH\", \"threshold\": \"OFF\" }, { \"category\": \"HARM_CATEGORY_DANGEROUS_CONTENT\", \"threshold\": \"OFF\" }, { \"category\": \"HARM_CATEGORY_SEXUALLY_EXPLICIT\", \"threshold\": \"OFF\" }, {\"category\": \"HARM_CATEGORY_HARASSMENT\", \"threshold\": \"OFF\" }]",
                    ToolsConfig = null,
                    PromptFunction = "GetPartnerNamesFromGeminiAsync",
                    Description = "Batch lookup of organization names from email domains using Gemini AI",
                    AdminCanChange = false
                },
                new AiPrompt
                {
                    Type = "interaction_summary",
                    Prompt = "I am providing interaction data. Please provide a concise summary of the interaction in the following structure in a bullet point list. Please do not include a lead in sentence.\n\n\nDate of interation:\nKey people involved:\nKey points:\nFollow up needed:\n\nData: {promptData}",
                    CreatedAt = DateTime.UtcNow,
                    Name = "Interaction",
                    Status = (EntityStatus)1,
                    ContentConfig = "{\"role\":\"user\",\"parts\":[{\"text\":\"{promptData}\"}]}",
                    GenerationConfig = "{\"temperature\":0.7,\"top_p\":0.2,\"max_output_tokens\":65535}",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = null,
                    PromptFunction = "GetInteractionDetailsAsync",
                    Description = "Generates a comprehensive summary of interaction details including participants, content, context, and outcomes in a structured Markdown format.",
                    AdminCanChange = true
                },
                new AiPrompt
                {
                    Type = "user_role_import",
                    Prompt = "You are an AI assistant processing user role assignment data from Google Sheets for UNOPS. Convert each row into the exact JSON structure below. Only include non-empty fields to keep JSON compact.\n\n**MANDATORY FIELDS:** userId, roleIds\n\n**HEADER MAPPING:**\n\"User\"/\"User Name\"/\"Name\"/\"Full Name\" → userId (single user name)\n\"Email\"/\"User Email\"/\"Email Address\" → userId (single email)\n\"User ID\" → userId (single user ID)\n\"Role\"/\"Roles\"/\"Role Name\"/\"Role Names\" → roleIds (array of role names or IDs)\n\"Role ID\"/\"Role IDs\" → roleIds (array of role IDs)\n\n**USER ROLE ASSIGNMENT JSON FORMAT:**\n{\"userId\": \"\", \"roleIds\": [], \"dependents\": [\"userId\", \"roleIds\"], \"validationError\": \"\"}\n\n**RULES:**\n- userId: Single user name, email, or user ID (keep as text for names/emails, number for IDs)\n- roleIds: Array of role names or role IDs (keep as text for names, number for IDs)\n- Parse comma-separated role values into arrays (e.g., \"Role1, Role2\" → [\"Role1\", \"Role2\"])\n- Parse semicolon-separated role values into arrays (e.g., \"Admin; Manager; Viewer\" → [\"Admin\", \"Manager\", \"Viewer\"])\n- Always include both userId and roleIds in dependents for ID resolution\n- userId will be resolved against UserProfile table\n- roleIds will be resolved against AspNetRoles table\n- Set validationError for missing mandatory fields\n- Omit null/empty fields to keep JSON compact\n- Handle multiple roles per row (comma or semicolon separated)\n- Each row represents ONE user with multiple roles\n- NEVER include \"id\" or \"Id\" field in JSON output\n- If user field contains multiple users, create separate records for each user\n\n**PARSING EXAMPLES:**\n- User: \"John Doe\", Roles: \"Admin, Manager\" → {\"userId\": \"John Doe\", \"roleIds\": [\"Admin\", \"Manager\"]}\n- User: \"jane@unops.org\", Roles: \"Viewer; Editor\" → {\"userId\": \"jane@unops.org\", \"roleIds\": [\"Viewer\", \"Editor\"]}\n- User: \"123\", Roles: \"ADMIN_ROLE\" → {\"userId\": \"123\", \"roleIds\": [\"ADMIN_ROLE\"]}\n\n**RESPONSE FORMAT:**\n{\"Message\":\"User role data processed successfully.\",\"Category\":\"UserRole\",\"ResponseType\":\"Action\",\"records\":[...]}\n\nReturn compact single-line JSON. If more input needed, set ResponseType to \"Information\".\n\nInput data: {promptData}",
                    CreatedAt = DateTime.UtcNow,
                    Name = "UserRole",
                    Status = (EntityStatus)1,
                    ContentConfig = "{\"role\":\"user\",\"parts\":[{\"text\":\"{promptData}\"}]}",
                    GenerationConfig = "{\"temperature\":0.1,\"top_p\":0.2,\"max_output_tokens\":65535}",
                    Location = "europe-west4",
                    Model = "gemini-2.5-flash",
                    Project = "unops-partneropportunity",
                    SafetySettings = null,
                    ToolsConfig = null,
                    PromptFunction = "",
                    Description = "Processes bulk user role assignment data from arrays or objects, converting them into structured JSON format with automatic parsing of multiple users and roles per row.",
                    AdminCanChange = false
                }
            };

            await context.AiPrompts.AddRangeAsync(aiPrompts);
            await context.SaveChangesAsync();
        }
    }
}