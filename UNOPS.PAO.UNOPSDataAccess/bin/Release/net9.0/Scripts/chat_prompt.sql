DELETE FROM public."AiPrompt"
WHERE "Type" = 'chat_prompt';

INSERT INTO public."AiPrompt" ("Type", "Prompt", "CreatedAt", "Name", "Status", "GenerationConfig", "ContentConfig", "Project", "Location", "Model") VALUES
('chat_prompt', 'I am sending you a chat prompt from a user.

Analyze the prompt to determine if the user is seeking information OR entered prompt to perform an action. 

The following are the type of actions that the user may want to perform:
1. Create a contact record
2. Create a partner record 
3. Create A contact interaction record
4. Create A partner tree record

If the user is seeking information, then respond with the following JSON: 
{ "request_type": "information", "entity": "" }

If the user wants to perform an action, then respond with the following JSON substituting {entity_name} with the determined entity name: 
{ "request_type": "action", "entity": "{entity_name}" }

The acceptable values for {entityname} are contact, partner, interaction and partnertree
The acceptable values for {operation} are create

Now, here is the prompt:
{promptData}', NOW(), 'General', 1, '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }', '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', 'unops-partneropportunity', 'europe-west3', 'gemini-1.5-flash-001');