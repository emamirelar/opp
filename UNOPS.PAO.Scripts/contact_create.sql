DELETE FROM public."AiPrompt"
WHERE "Type" = 'contact_create';

INSERT INTO public."AiPrompt" ("Type", "Prompt", "CreatedAt", "Name", "Status", "GenerationConfig", "ContentConfig", "Project", "Location", "Model") VALUES
('contact_create', 'I am sending you some contact data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON.

Raw Data:
{promptData}

JSON format:
{"salutation": "", "firstName": "", "middleName": "", "lastName": "", "suffix": "", "title": "", "pronouns": "", "birthDate": "", "email": "", "phone": "", "mobile": "", "otherPhone": "", "fax": "", "partner": "", "department": "", "description": "", "status": "", "contactNumber": "", "assistant": "", "assistantPhone": "", "assistantEmail": "", "mailingStreet": "", "mailingStreet2": "", "mailingCity": "", "mailingStateProvince": "", "mailingPostalCode": "", "mailingCountry": "" }

Somethings to consider about the JSON format above are:
partner is the organization where the contact works', NOW(), 'General', 1, '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }', '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', 'unops-partneropportunity', 'europe-west3', 'gemini-1.5-flash-001');