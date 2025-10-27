DELETE FROM public."AiPrompt"
WHERE "Type" = 'partner_create';

INSERT INTO public."AiPrompt" ("Type", "Prompt", "CreatedAt", "Name", "Status", "GenerationConfig", "ContentConfig", "Project", "Location", "Model") VALUES
('partner_create', 'I am sending you partner data in raw format. Determine where each data point fits in the JSON format provided below and return the formatted JSON.

Raw Data:
{promptData}

JSON format:
{ "name": "", "status": "", "newEngagement": "", "phone": "", "website": "", "shortName": "", "internalReportingLevel": "", "externalReportingLevel": "", "pooledFund": "", "ddRequired": "", "ddeacDone": "", "eacReference": "", "globalKeyAccount": "", "unSecretariatEntity": "", "levyPotentiallyApplies": "", "reasonForLevyNotApplying": "", "levyTreatment": "", "scope": "", "address1Street": "", "address1Street2": "", "address1City": "", "address1StateProvince": "", "address1PostalCode": "", "address1Country": "", "address2Street": "", "address2Street2": "", "address2City": "", "address2StateProvince": "", "address2PostalCode": "", "address2Country": "" }

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
Accpetable values for "scope" are: "Global", "Regional", "Local"', NOW(), 'General', 1, '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }', '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', 'unops-partneropportunity', 'europe-west3', 'gemini-1.5-flash-001');