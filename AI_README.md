# AI Integration - ReadMe

## Abstract
This document outlines the integration of AI features within the project, detailing the architecture, setup, and configuration necessary for utilizing AI-powered functionalities. The AI component is designed to process meeting minutes, summarize content, and generate context-aware responses through the Gemini API via Vertex AI. Additionally, recent enhancements include the introduction of an AI Assistant (Chatbot) that facilitates user interactions for creating and retrieving various entities.

## Overview
This document provides details on the AI integration in the project, including the architecture, setup, and configuration required to use the AI-powered features. The AI component is responsible for processing meeting minutes, summarizing content, and generating context-aware responses using Gemini API via Vertex AI.

## Architecture
The AI-powered summary tool is designed to dynamically generate context-aware summaries based on the current screen in the web application. The architecture consists of the following components:

### Frontend (Angular 19)
- Sends requests to the backend, specifying the screen context.

### Backend (.NET Core API)
- Receives requests from the frontend.
- Determines the relevant database tables and retrieves necessary data.
- Constructs an appropriate prompt for the AI model.
- Calls the Gemini API via Vertex AI.
- Processes and returns the AI-generated response to the frontend.

### Database (PostgreSQL)
- Stores AI-related data, including prompts mapped to different screens.
- Ensures that relevant data is fetched dynamically based on the screen context.

### AI Model (Gemini API via Vertex AI)
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
We have four tables: `AiPrompt`, `AiScreenMapping`, `AiChatHistory`, and `AiChatSession`.

#### AiPrompt Table
```
CREATE TABLE AiPrompt (
    Type TEXT PRIMARY KEY,
    PromptTemplate TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    Name TEXT NOT NULL,
    Status INTEGER NOT NULL
);
```
- The `Type` field should match the type value sent in the API request.

#### AiScreenMapping Table
```
CREATE TABLE AiScreenMapping (
    Type TEXT NOT NULL,
    TableName TEXT NOT NULL,
    ComparisonKey TEXT NOT NULL,
    RelatedEntity TEXT,
    RelatedEntityKey TEXT,
    QueryConditions TEXT,
    Order INT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    Name TEXT NOT NULL,
    Status INTEGER NOT NULL
);
```
- The `Type` field should match the type value sent in the API request.
- The table defines how different entities are related for data retrieval.
- `QueryConditions` is intended for future use, allowing additional conditions to be applied dynamically.
- `Order` determines the order in which the join should be made when multiple tables are involved.

#### AiChatHistory Table
```
CREATE TABLE AiChatHistory (
    Id SERIAL PRIMARY KEY,
    Sender TEXT NOT NULL,
    Message TEXT NOT NULL,
    RawMessage TEXT,
    TimeStamp TIMESTAMP DEFAULT NOW(),
    Type TEXT NOT NULL,
    EntityType TEXT,
    RequestType TEXT,
    SessionId UUID REFERENCES AiChatSession(Id)
);
```
- Stores chat history for AI Assistant interactions.
- `RawMessage` captures only the user's input before processing.
- `SessionId` links messages to their respective sessions.

#### AiChatSession Table
```
CREATE TABLE AiChatSession (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    StartTime TIMESTAMP DEFAULT NOW(),
    EndTime TIMESTAMP,
    UserId INT NOT NULL,
    Status TEXT DEFAULT 'Active'
);
```
- Tracks the lifecycle of a chat session.
- `Status` determines if the session is still active.

##### Example Entries for Contacts Summary
```
INSERT INTO AiScreenMapping (Type, TableName, ComparisonKey, RelatedEntity, RelatedEntityKey, QueryConditions, Order, CreatedAt, Name, Status) VALUES 
('contacts_summary', 'Contacts', 'Id', 'Interactions', 'ContactId', NULL, 1, NOW(), 'Contacts', 1),
('contacts_summary', 'Contacts', 'PartnerId', 'Partners', 'Id', NULL, 2, NOW(), 'Contacts', 1),
('partner_interactions_summary', 'Partners', 'Id', 'Contacts', 'PartnerId', NULL, 1, NOW(), 'Partners', 1),
('partner_interactions_summary', 'Contacts', 'Id', 'Interactions', 'ContactId', NULL, 2, NOW(), 'Partners', 1);

INSERT INTO AiPrompt (Type, PromptTemplate, CreatedAt, Name, Status) VALUES
('contacts_summary', "Sample Prompt", NOW(), 'Contacts', 1);
```

### 3. API Endpoints
#### Process AI Data
**POST** `/api/process-data`

##### Request Body:
```
{
   "id": 10,
   "type": "contacts_summary"
}
```
##### Response:
- Returns the processed summary from Gemini based on the requested entity.

#### AI Assistant Chatbot
**POST** `/api/ai-assistant/chat`

- Enables users to interactively create and retrieve entities such as contacts, partners, partner levels, and interactions.

#### Image & Audio Upload
**POST** `/api/scan-data`

- Accepts `form-data` with an optional file and a `type` parameter.
- Image data is extracted using **Google Cloud Vision (OCR)**.
- Audio data is transcribed using **Google Cloud Speech**.

### 4. Entity Intent Detection
The prompt type for entity detection is `entity_intent_detection`. It will respond in the following JSON format:
```
{
  "Entity": "<entity>",
  "Intent": "<action/information>",
  "Summary": "<summary of the complete conversation>",
  "Message": "<response to the user>",
  "Forward": "<yes/no>",
  "Type": "<next prompt type derived from entity and intent>"
}
```

- The `Summary` field from the response is sent to the next prompt type, where the `promptType` is the `Type` field in the JSON response.

## Conclusion
This AI integration provides automated and contextual summaries within the application by leveraging **Gemini API via Vertex AI**. The setup ensures flexibility and adaptability to various screen contexts by dynamically fetching relevant data and constructing AI prompts accordingly. The **new AI Assistant** enhances user interaction, while **image and audio processing capabilities** extend the application's functionality. 

For further details or troubleshooting, refer to the main project documentation.

