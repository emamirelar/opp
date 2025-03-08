# AI Integration - ReadMe

## Overview
This document provides details on the AI integration in the project, including the architecture, setup, and configuration required to use the AI-powered features. The AI component is responsible for processing meeting minutes, summarizing content, and generating context-aware responses using Gemini API via Vertex AI.

## Architecture
The AI-powered summary tool is designed to dynamically generate context-aware summaries based on the current screen in the web application. The architecture consists of the following components:

1. **Frontend (Angular 19)**: Sends requests to the backend, specifying the screen context.
2. **Backend (.NET Core API)**:
   - Receives requests from the frontend.
   - Determines the relevant database tables and retrieves necessary data.
   - Constructs an appropriate prompt for the AI model.
   - Calls the Gemini API via Vertex AI.
   - Processes and returns the AI-generated response to the frontend.
3. **Database (PostgreSQL)**:
   - Stores AI-related data, including prompts mapped to different screens.
   - Holds records of past interactions and summaries.
   - Ensures that relevant data is fetched dynamically based on the screen context.
4. **AI Model (Gemini API via Vertex AI)**:
   - Receives structured prompts from the backend.
   - Generates summaries based on provided data.
   - Returns structured responses for further processing and display.

## Setup & Configuration
### 1. Authentication
Before making API calls, ensure you are authenticated with Google Cloud by running the following command:
```
gcloud auth application-default login --impersonate-service-account=pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com
```

### 2. Database Setup
We have two tables: `AiPrompt` and `AiScreenMapping`.

#### **AiPrompt Table**
```sql
CREATE TABLE AiPrompt (
    Type TEXT PRIMARY KEY,
    PromptTemplate TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    Name TEXT NOT NULL,
    Status INTEGER NOT NULL
);
```
- The `Type` field should match the `type` value sent in the API request.

#### **AiScreenMapping Table**
```sql
CREATE TABLE AiScreenMapping (
    Type TEXT NOT NULL,
    TableName TEXT NOT NULL,
    ComparisonKey TEXT NOT NULL,
    RelatedEntity TEXT NOT NULL,
    RelatedEntityKey TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    Name TEXT NOT NULL,
    Status INTEGER NOT NULL
);
```
- The `Type` field should match the `type` value sent in the API request.
- The table defines how different entities are related for data retrieval.

##### **Example Entries for Contacts Summary**
```sql
INSERT INTO AiScreenMapping (Type, TableName, ComparisonKey, RelatedEntity, RelatedEntityKey, CreatedAt, Name, Status) VALUES 
('contacts_summary', 'Contacts', 'Id', 'Interactions', 'ContactId', NOW(), 'Contacts', 1),
('contacts_summary', 'Contacts', 'PartnerId', 'Partners', 'Id', NOW(), 'Contacts', 1);
```
```sql
INSERT INTO AiPrompt (Type, PromptTemplate, CreatedAt, Name, Status) VALUES
('contacts_summary', '"""I\'m providing a JSON object containing contact information, partner information and interaction history. I need you to generate a summary in Markdown format, using the following template:

## Contact Summary

**Name:** [Contact Name]  
**Email:** [Contact Email]

**Key Interactions:**

*   **[Date of Interaction] - [Type of Interaction]:** [A brief summary of the interaction. 1-2 sentences.]

**Key Information about Partners**
* Partner Name

**Overall Summary:**

[A 6-15 sentence paragraph providing an overview of the contact based on the interactions and partners, highlighting key themes, and sentiment.]

Instructions:

Contact Information: Extract the contact\'s name and email from the """"Contacts"""" object in the JSON and populate the Name and Email fields in the template.  
Key Interactions: For each interaction in the """"Interactions"""" array:  
Extract the Date and use it for [Date of Interaction]. Format the date as YYYY-MM-DD.  
Determine the [Type of Interaction] based on the Type field. Use the following mapping:  
3: """"Note""""  
Decode the Base64 encoded Data field.  
Create a brief 1-2 sentence summary of the interaction using the decoded Data and populate the [A brief summary of the interaction. 1-2 sentences.] field.  
Overall Summary: Based on all the interactions, create a 6-15 sentence paragraph providing an overall summary of the contact. Include key themes, sentiment (if discernible from the interaction data), and any potential needs or concerns that emerge from the interactions.  
Markdown Formatting: Ensure the entire summary is correctly formatted in Markdown.  
Focus: The primary focus of the summary should be to understand the general topics discussed and the tone of any interactions.

JSON Data:

{jsonData}

Please provide the generated Markdown summary based on these instructions."""', NOW(), 'Contacts', 1);
```

### 3. API Endpoint
#### **Process AI Data**
```
POST /api/process-data
```
##### Request Body:
```json
{
   "id": 10,
   "type": "contacts-summary"
}
```
##### Response:
- Returns the processed summary from Gemini based on the requested entity.

### 4. Dynamic Data Retrieval
The backend dynamically fetches data from various tables based on the `type` sent in the API request. The mapping between `type` and related tables is pre-configured, ensuring only relevant data is included in the AI prompt.

### 5. AI Prompt Construction
The backend constructs a structured prompt using the stored templates and dynamically retrieved data. Example:
```markdown
## Contact Summary

**Name:** [Contact Name]  
**Email:** [Contact Email]

**Key Interactions:**

*   **[Date of Interaction] - [Type of Interaction]:** [A brief summary of the interaction. 1-2 sentences.]

**Key Information about Partners**
* Partner Name

**Overall Summary:**

[A 6-15 sentence paragraph providing an overview of the contact based on the interactions and partners, highlighting key themes, and sentiment.]
```

## Error Handling
- **Missing `type` or `id` in Request**: Returns a 400 Bad Request error.
- **Invalid API Key or Authentication Issues**: Logs error and returns 500 Internal Server Error.
- **No Data Found for the Given ID**: Returns an appropriate message in the response.

## Future Enhancements
- **Fine-tuning AI Prompts** for better contextual understanding.
- **Caching AI Responses** to improve efficiency and reduce API calls.
- **User Feedback Mechanism** to refine AI-generated summaries over time.

## Conclusion
This AI integration provides automated and contextual summaries within the application by leveraging Gemini API via Vertex AI. The setup ensures flexibility and adaptability to various screen contexts by dynamically fetching relevant data and constructing AI prompts accordingly.

