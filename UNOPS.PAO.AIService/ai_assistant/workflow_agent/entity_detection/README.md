# Entity Detection Agent Module

A modular entity detection agent that dynamically loads entity and intent information from `tools.json` configuration.

## 📁 Module Structure

```
entity_detection/
├── __init__.py          # Module initialization and exports
├── agent.py             # Agent definition and configuration  
├── callback.py          # Callback functions for entity processing
├── prompt.py            # Dynamic prompt building logic
└── README.md           # This documentation
```

## 🔧 Components

### `agent.py`
- **Purpose**: Defines the main entity detection agent
- **Key Features**: 
  - Uses Google ADK `LlmAgent`
  - Integrates dynamic instruction callback
  - Configures before-model callback for entity loading

### `callback.py`
- **Purpose**: Contains callback functions for the agent
- **Functions**:
  - `extract_entity_intent_before_model()`: Loads entity data from tools.json
  - `dynamic_instruction_callback()`: Builds dynamic prompts
- **Key Features**:
  - Processes 13+ entity types from configuration
  - Maps HTTP methods to intents automatically
  - Extracts synonyms and endpoint patterns

### `prompt.py`
- **Purpose**: Dynamic prompt building and templates
- **Functions**:
  - `build_dynamic_prompt()`: Constructs detection prompts
  - `get_entity_detection_examples()`: Provides test examples
- **Key Features**:
  - Uses entity information loaded from tools.json
  - Supports multiple entity detection
  - Includes comprehensive examples and patterns

## 🚀 Usage

```python
from ai_assistant.workflow_agent.entity_detection import entity_detection_agent

# The agent is automatically configured and ready to use
# It will dynamically load entity information from tools.json
```

## 📊 Entity Detection Features

### **Supported Entities** (Dynamically Loaded)
- **Partner**: organizations, companies, vendors, suppliers
- **Contact**: people, individuals, representatives
- **Interaction**: meetings, communications, calls, emails  
- **Gemini**: AI chat, similarity search, AI assistant
- **Document**: files, attachments, uploads
- **Notification**: alerts, messages, notices
- **Configuration**: settings, entity management
- **AiPrompt**: AI templates, prompt configurations
- **UserManagement**: users, roles, permissions
- **OrganizationHierarchy**: org structure, hierarchy
- **Link**: relationships, connections, associations
- **Profile**: user profiles, personal information
- **PartnerTree**: partner hierarchy, tree structures
- **UserData**: user preferences, user settings

### **Intent Detection**
- `search/get/list`: Show, find, search, list, get, display
- `create/add`: Create, add, new, register, establish  
- `update/modify`: Update, modify, change, edit, alter
- `delete/remove`: Delete, remove, eliminate, drop
- `similarity_search`: Find similar, like, related to

### **Response Format**
Returns JSON array with detected entities:
```json
[
  {
    "entity": "Partner",
    "intent": "search", 
    "confidence": 0.95,
    "extracted_params": {"status": "active"},
    "reasoning": "Clear partner search request",
    "synonyms_matched": ["organization"],
    "endpoint_guidance": "Use for browsing partners"
  }
]
```

## 🔄 Configuration

The agent automatically loads configuration from:
- `config/tools.json` - Entity definitions, synonyms, endpoints
- Dynamic loading ensures always up-to-date entity information
- No manual configuration required

## 🧪 Testing

Example queries and expected responses are defined in `prompt.py`:
- Single entity detection
- Multiple entity detection  
- Knowledge/help requests
- Similarity search queries 