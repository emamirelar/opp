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
    RelatedEntity TEXT,
    RelatedEntityKey TEXT,
    QueryConditions TEXT,
    Order INT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    Name TEXT NOT NULL,
    Status INTEGER NOT NULL
);
```
- The `Type` field should match the `type` value sent in the API request.
- The table defines how different entities are related for data retrieval.
- **`QueryConditions`** is a field intended for future use, allowing additional conditions to be applied dynamically.
- **`Order`** determines the order in which the join should be made when multiple tables are involved.

##### **Example Entries for Contacts Summary**
```sql
INSERT INTO AiScreenMapping (Type, TableName, ComparisonKey, RelatedEntity, RelatedEntityKey, QueryConditions, Order, CreatedAt, Name, Status) VALUES 
('contacts_summary', 'Contacts', 'Id', 'Interactions', 'ContactId', NULL, 1, NOW(), 'Contacts', 1),
('contacts_summary', 'Contacts', 'PartnerId', 'Partners', 'Id', NULL, 2, NOW(), 'Contacts', 1),
('partner_interactions_summary', 'Partners', 'Id', 'Contacts', 'PartnerId', NULL, 1, NOW(), 'Partners', 1),
('partner_interactions_summary', 'Contacts', 'Id', 'Interactions', 'ContactId', NULL, 2, NOW(), 'Partners', 1);

```
```sql
INSERT INTO AiPrompt (Type, PromptTemplate, CreatedAt, Name, Status) VALUES
('contacts_summary', "Sample Prompt", NOW(), 'Contacts', 1);
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
   "type": "contacts_summary"
}
```
##### Response:
- Returns the processed summary from Gemini based on the requested entity.

### 4. Dynamic Data Retrieval
The backend dynamically fetches data from various tables based on the `type` sent in the API request. The mapping between `type` and related tables is pre-configured, ensuring only relevant data is included in the AI prompt.

### 5. AI Integration in Code
- `GeminiController.cs` handles the API request and first fetches the `AiPrompt` data to check if a prompt template is available.
- If a prompt template is found, it queries the `AiScreenMapping` table to determine which tables to fetch data from.
- The core logic for data retrieval based on screen mapping is implemented in `UNOPSGeminiManager.cs`.
- The main method `GetDataBasedOnScreenMapping` uses reflection to dynamically retrieve data from tables. It also dynamically constructs SQL queries based on the entries in the AiScreenMapping table. This allows the backend to intelligently determine which tables and columns should be queried for a given screen type (i.e., the type parameter sent in the request payload).
    - **`Dynamic SQL Creation`**: The SQL queries are generated programmatically based on the mapping configuration stored in `AiScreenMapping`, ensuring that only relevant data is retrieved for each screen context. This dynamic query generation ensures flexibility in handling different types of requests while maintaining a structured and secure approach to database querying.
    - **`Order Field`**: The `Order` field ensures that the joins are executed in the correct sequence when multiple related tables are involved. This guarantees that the join operations respect the logical order of the database relationships, preserving data integrity and consistency.
    - **`Handling Missing Related Entities`**: If the `RelatedEntity` and `RelatedEntityKey` are not specified in the `AiScreenMapping` table, the code will default to performing a regular `SELECT` query with a `WHERE` clause to retrieve the relevant data. This fallback mechanism ensures that even when complex relationships are not defined, the system can still fetch the necessary data and construct the AI prompt accordingly.
- The method leverages reflection to ensure that the tables and columns being queried exist in the database before executing any SQL commands. This helps prevent issues like querying non-existent tables or columns, which could lead to runtime errors or security vulnerabilities.
- Custom conditions and logic can be added in these methods based on the `type` parameter sent in the request payload.
- The `QueryConditions` field in `AiScreenMapping` is currently **not being used** but is intended for future enhancements where additional query conditions may be applied dynamically.
- Calling Gemini directly from .NET was not possible as **Vertex AI** does not have built-in support in the AIPlatform package. Instead, **HTTPClient** was used to call Gemini via a direct URL.

### 6. Prompt Engineering
To refine AI responses, use **Vertex AI Studio** under the GCP project unops-partneropportunity. This allows for interactive testing, fine-tuning, and validation of prompts before deploying them.

### 7. Automatic Table Creation
If you build and run the application via **Visual Studio**, the migration scripts will automatically create these tables. You only need to insert the relevant data as shown above.

## Conclusion
This AI integration provides automated and contextual summaries within the application by leveraging Gemini API via Vertex AI. The setup ensures flexibility and adaptability to various screen contexts by dynamically fetching relevant data and constructing AI prompts accordingly.

For further details or troubleshooting, refer to the main project documentation.

