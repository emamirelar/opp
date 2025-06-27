# API Worker Agent Module

A modular API worker agent that uses a LoopAgent to iterate through detected entities and make appropriate API calls based on `tools.json` configuration.

## 📁 Module Structure

```
api_worker/
├── __init__.py          # Module initialization and exports
├── agent.py             # LoopAgent and LlmAgent definitions
├── callback.py          # Callback functions for entity processing
├── prompt.py            # Dynamic prompt building for API operations
├── utilities.py         # API utility functions and tools
└── README.md           # This documentation
```

## 🔧 Components

### `agent.py`
- **Purpose**: Defines the main API worker agents using LoopAgent architecture
- **Structure**:
  - `api_caller_agent` (LlmAgent): Processes individual entities
  - `api_worker_agent` (LoopAgent): Manages the iteration through entities
- **Key Features**:
  - Uses Google ADK `LoopAgent` for iteration control
  - Integrates tools for API calls and loop termination
  - Dynamic prompts for each entity processing step

### `callback.py`
- **Purpose**: Contains callback functions for entity processing logic
- **Functions**:
  - `prepare_api_worker_before_model()`: Loads entity detection results and API config
  - `get_current_entity_for_processing()`: Gets current entity to process
  - `advance_to_next_entity()`: Moves to next entity in queue
  - `find_appropriate_endpoint()`: Maps entity/intent to API endpoints
  - `record_api_result()`: Records API operation results
  - `get_processing_summary()`: Provides final processing summary

### `prompt.py`
- **Purpose**: Dynamic prompt building for API operations
- **Functions**:
  - `build_api_worker_prompt()`: Constructs entity-specific prompts
  - `build_completion_prompt()`: Creates completion prompt
  - `dynamic_api_worker_instruction()`: Main instruction callback
  - `get_api_worker_examples()`: Provides test examples

### `utilities.py`
- **Purpose**: Core API utilities and tool functions
- **Key Functions**:
  - `invoke_api_tool()`: Makes HTTP API calls with full logging
  - `exit_loop_on_success()`: Terminates loop processing
  - `extract_iap_headers_from_context()`: Handles authentication
  - `construct_api_url()`: Builds complete URLs with path parameters
  - `separate_path_and_query_params()`: Separates parameter types

## 🚀 How It Works

### **Processing Flow:**

1. **Initialization** (via `prepare_api_worker_before_model`)
   - Loads entity detection results from previous agent
   - Builds entity-to-endpoints mapping from tools.json
   - Sets up processing state and queue

2. **Loop Processing** (via `LoopAgent`)
   - Iterates through each detected entity
   - For each entity:
     - Finds appropriate API endpoint
     - Builds URL and parameters
     - Makes API call via `invoke_api_tool`
     - Records results

3. **Completion** (via `exit_loop_on_success`)
   - Provides processing summary
   - Terminates loop when all entities processed

### **Entity-to-Endpoint Mapping:**

```python
# Intent mapping to HTTP methods and endpoint patterns
{
    "search": {"methods": ["GET"], "patterns": ["Get", "List", "Search"]},
    "create": {"methods": ["POST"], "patterns": ["Create", "Add", "New"]},
    "update": {"methods": ["PUT", "PATCH"], "patterns": ["Update", "Modify"]},
    "delete": {"methods": ["DELETE"], "patterns": ["Delete", "Remove"]},
    "similarity_search": {"methods": ["GET"], "patterns": ["Similarity"]}
}
```

## 🔧 API Call Features

### **HTTP Methods Supported:**
- `GET` - Search and retrieve operations
- `POST` - Create operations  
- `PUT` - Update operations
- `DELETE` - Delete operations

### **Authentication:**
- Automatic IAP header extraction from ADK context
- Development mode support with simulated headers
- Environment variable configuration (`IS_DEVELOPMENT`, `DEV_EMAIL`)

### **Error Handling:**
- Connection error detection and diagnostics
- SSL certificate handling (disabled for localhost)
- Timeout management (200 seconds)
- Comprehensive error logging and troubleshooting

### **Parameter Handling:**
- Path parameter substitution (`/api/partner/{id}`)
- Query parameter encoding for GET/DELETE
- JSON body encoding for POST/PUT
- Automatic parameter separation

## 📊 Example Operations

### **Search Partners:**
```json
Input Entity: {
    "entity": "Partner",
    "intent": "search", 
    "extracted_params": {"status": "active"}
}
→ API Call: GET /api/partner?status=active
```

### **Create Contact:**
```json
Input Entity: {
    "entity": "Contact",
    "intent": "create",
    "extracted_params": {
        "firstName": "John",
        "lastName": "Doe", 
        "email": "john@example.com"
    }
}
→ API Call: POST /api/contact (with JSON body)
```

### **Update Partner:**
```json
Input Entity: {
    "entity": "Partner",
    "intent": "update",
    "extracted_params": {"id": 123, "name": "New Name"}
}
→ API Call: PUT /api/partner (with path param substitution)
```

## 🔄 Configuration

### **Environment Variables:**
- `API_BASE_URL`: Base URL for API calls (default: https://localhost:44426)
- `IS_DEVELOPMENT`: Enable development mode (`TRUE`/`FALSE`)
- `DEV_EMAIL`: Development email for IAP simulation

### **Dynamic Loading:**
- Entity configurations loaded from `config/tools.json`
- Endpoint mappings built automatically
- No manual configuration required

## 🧪 Testing

### **Example Test Scenarios:**
- Single entity processing
- Multiple entity processing
- Error handling (connection failures, authentication issues)
- Different HTTP methods and parameter types
- Loop termination conditions

### **Debugging Features:**
- Comprehensive logging for each API call
- Pre-flight connectivity checks
- Parameter analysis and URL construction
- Response parsing and error details
- Processing progress tracking

## 🔗 Integration

### **Input Requirements:**
- Expects `entity_intent_detection` array from entity detection agent
- Each entity should have: `entity`, `intent`, `extracted_params`, `confidence`

### **Output Provided:**
- `api_call_result`: Individual API call results
- `api_worker_state`: Complete processing state and summary
- Processing logs and status updates

### **Loop Control:**
- Automatic iteration through entity queue
- `exit_loop_on_success()` tool for proper termination
- Max iterations limit for safety (10 iterations)

The API worker agent seamlessly integrates with the entity detection agent to provide end-to-end processing of user requests through intelligent API orchestration! 