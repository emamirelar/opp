# UNOPS Opportunity+ AI Agent - Advanced Multi-Agent System

A sophisticated **multi-agent AI system** built with Google ADK (Agent Development Kit) that provides intelligent entity detection, API integration, Google Drive operations, web search capabilities, and comprehensive workflow automation for UNOPS operations.

## 🚀 **Core Capabilities**

### 🧠 **Multi-Agent Architecture**
- **Sequential Agent Processing**: Coordinated workflow through specialized sub-agents
- **Entity Detection**: Advanced NLP-based entity recognition from natural language queries
- **API Worker**: Automated API calls with intelligent endpoint mapping
- **Response Formatting**: User-friendly response generation with context awareness
- **Contextual Agents**: User profile and screen context management

### 🔍 **Search & Knowledge Management**
- **Google Drive Integration**: Search, read, and analyze documents in Google Drive
- **Knowledge Base Search**: Semantic search through organizational knowledge base
- **Web Search**: Google search integration for external information and current events
- **Similarity Search**: AI-powered semantic search using vector embeddings
- **Content Search**: Deep content analysis across files and documents

### 🌐 **API Integration & Data Management**
- **Dynamic API Endpoint Mapping**: Automatic endpoint detection based on entity types
- **15+ Supported Entities**: Partner, Contact, Interaction, Document, AI Prompt, and more
- **CRUD Operations**: Complete Create, Read, Update, Delete functionality
- **Permission Management**: Role-based access control and user permissions
- **Cache Management**: Intelligent caching with TTL and entity-aware invalidation

### 📊 **Advanced Features**
- **Streaming Responses**: Real-time response streaming for better user experience
- **Session Management**: Persistent conversation state and context
- **File Analysis**: AI-powered document analysis and content extraction
- **Accessibility Support**: Screen reader compatibility and accessibility features
- **Multi-Environment Support**: Development, test, and production configurations

## 🏗️ **System Architecture**

### 🎯 **Agent Hierarchy**
```
🎯 Root Agent (Sequential Processing)
├── 🧠 Contextual Agent
│   ├── 👤 User Detail Agent      # User profile management
│   └── 🖥️ Screen Context Agent   # UI context awareness
└── 🤖 User Request Agent (Main AI Assistant)
    ├── 🔍 Direct Response Tools
    │   ├── search_google_drive_knowledge    # Knowledge base search
    │   ├── search_google_drive             # File search & management
    │   ├── read_google_drive_file          # Document reading
    │   ├── search_google_drive_content     # Content-based search
    │   ├── get_cache_stats                 # Cache monitoring
    │   ├── clear_cache                     # Cache management
    │   └── refresh_cache                   # Cache refresh
    ├── 🌐 Search Agent                     # Google web search
    └── 🔄 Workflow Agent (Sequential Processing)
        ├── 🎯 Entity Detection Agent
        │   ├── Detects 15+ entity types
        │   ├── Determines user intent (CRUD operations)
        │   ├── Extracts parameters and context
        │   └── Maps entities to API endpoints
        ├── 🔧 API Worker Agent (Loop Agent)
        │   └── API Caller Agent
        │       ├── Dynamic endpoint mapping
        │       ├── HTTP request execution
        │       ├── Authentication handling
        │       ├── Response processing
        │       └── Error handling & retries
        └── 📄 Response Formatter Agent
            ├── User-friendly response formatting
            ├── Error message handling
            ├── Follow-up suggestions
            └── Context-aware responses
```

### 🔧 **System Components**
```
┌─────────────────────────────────────────────────────────────────┐
│                    🌐 FastAPI Application                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │   Web Interface │  │   REST API      │  │   Chat Endpoint │   │
│  │   /dev-ui       │  │   /docs         │  │   /chat         │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                                   │
┌─────────────────────────────────────────────────────────────────┐
│                    🧠 Google ADK Integration                     │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │   Agent Runner  │  │   Session Mgmt  │  │   Tool Registry │   │
│  │   Orchestration │  │   State Mgmt    │  │   Dynamic Load  │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                                   │
┌─────────────────────────────────────────────────────────────────┐
│                    ⚙️ Configuration System                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │ framework_config│  │   tools.json    │  │   .env file     │   │
│  │ Environment-    │  │   1,572 lines   │  │   Runtime       │   │
│  │ specific JSON   │  │   Entity Config │  │   Variables     │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                                   │
┌─────────────────────────────────────────────────────────────────┐
│                    🔧 Core Services                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │   Cache System  │  │   Config Mgr    │  │   Google Drive  │   │
│  │   Entity-aware  │  │   Singleton     │  │   Tool          │   │
│  │   TTL-based     │  │   Load-once     │  │   Integration   │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                                   │
┌─────────────────────────────────────────────────────────────────┐
│                    💾 Data & Integration Layer                   │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │   PostgreSQL    │  │   Google Cloud  │  │   External APIs │   │
│  │   Session Store │  │   Vertex AI     │  │   UNOPS Backend │   │
│  │   User Data     │  │   Secret Mgr    │  │   15+ Entities  │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## 📋 **Supported Entities & Operations**

### 🏢 **Core Business Entities**
| Entity | Description | Operations | Key Features |
|--------|-------------|------------|--------------|
| **Partner** | Organizations, companies, vendors | CRUD, Search, Similarity | Hierarchical grouping, permissions |
| **Contact** | Individual contacts | CRUD, Search, Similarity | Profile management, relationships |
| **Interaction** | Communications, meetings | CRUD, Search, History | Timeline tracking, summaries |
| **Document** | File management | CRUD, Upload, Analysis | Type categorization, metadata |
| **AiPrompt** | AI prompt templates | CRUD, Management | Template system, versioning |

### 🔧 **System Entities**
| Entity | Description | Operations | Key Features |
|--------|-------------|------------|--------------|
| **GoogleDrive** | Drive integration | Search, Read, Upload | Content analysis, file management |
| **Gemini** | AI services | Chat, Analysis, Embeddings | Session management, file analysis |
| **Configuration** | System settings | View, Update | Entity configuration, permissions |
| **UserManagement** | User administration | CRUD, Permissions | Role-based access control |
| **Notification** | System notifications | CRUD, Management | Alert system, user preferences |

### 🌐 **Additional Entities**
- **OrganizationHierarchy**: Organizational structure management
- **Link**: URL and link management
- **Profile**: User profile management
- **PartnerTree**: Partner relationship hierarchy
- **UserData**: User-specific data management

## 🎯 **Key Features & Capabilities**

### 🔍 **Intelligent Search Capabilities**
```
📚 Knowledge Base Search
├── Search Google Drive documents
├── Semantic content analysis
├── Document summarization
└── Context-aware responses

🌐 Web Search Integration
├── Google search for current events
├── External company information
├── Latest news and developments
└── Authoritative source verification

🔎 Similarity Search
├── AI-powered semantic matching
├── Vector embeddings
├── Cross-entity relationship discovery
└── Intelligent recommendations
```

### 🤖 **AI-Powered Operations**
```
🧠 Entity Detection
├── 15+ entity types recognized
├── Intent classification (CRUD)
├── Parameter extraction
└── Context understanding

📊 Data Processing
├── Automated API calls
├── Response formatting
├── Error handling
├── Permission checking
└── Cache management

💬 Conversational AI
├── Natural language processing
├── Context-aware responses
├── Follow-up suggestions
└── Multi-turn conversations
```

### 🔧 **Advanced System Features**
```
⚡ Performance Optimization
├── Intelligent caching system
├── Entity-aware cache invalidation
├── Streaming responses
├── Parallel processing
└── Connection pooling

🔒 Security & Access Control
├── Role-based permissions
├── IAP integration
├── CORS configuration
├── Secure authentication
└── Audit logging

🌍 Multi-Environment Support
├── Development configuration
├── Test environment
├── Production settings
└── Environment-specific overrides
```

## 📁 **Project Structure**

```
UNOPS.PAO.AIService/
├── 🏠 Root Files
│   ├── main.py                         # FastAPI application with ADK
│   ├── framework_config.py             # Configuration loader
│   ├── agent.py                        # Root agent definition
│   ├── requirements.txt                # Dependencies
│   ├── .env                           # Environment variables
│   └── README.md                      # This documentation
├── 📁 config/                         # Configuration files
│   ├── framework_config_dev.json      # Development configuration
│   ├── framework_config_test.json     # Test configuration
│   ├── framework_config_prod.json     # Production configuration
│   ├── tools.json                     # Entity & API definitions (1,572 lines)
│   └── knowledge_base.txt             # Knowledge base content
├── 🤖 ai_assistant/                   # Core AI system
│   ├── agent.py                       # Main agent with all capabilities
│   ├── config_manager.py              # Configuration management
│   ├── cache.py                       # Entity-aware caching
│   ├── search_agent.py                # Google search integration
│   ├── contextual_agents/             # Context management
│   │   ├── user_detail_agent.py       # User profile management
│   │   ├── screen_context_agent.py    # UI context awareness
│   │   └── contextual_agent.py        # Context coordination
│   ├── tools/                         # Tool implementations
│   │   ├── google_drive_tool.py       # Google Drive integration
│   │   └── __init__.py                # Tool registry
│   └── workflow_agent/                # Workflow orchestration
│       ├── agent.py                   # Sequential agent coordinator
│       ├── entity_detection/          # Entity detection sub-agent
│       │   ├── agent.py               # Entity recognition
│       │   └── prompts.py             # Detection prompts
│       ├── api_worker/                # API worker sub-agent
│       │   ├── agent.py               # API orchestration
│       │   ├── callback.py            # Dynamic tool injection
│       │   └── utilities.py           # API utilities
│       └── response_formatter/        # Response formatting sub-agent
│           ├── agent.py               # Response formatting
│           └── prompts.py             # Formatting prompts
├── 🧪 tests/                          # Test files
│   ├── test_entity_detection.py       # Entity detection tests
│   ├── test_enhanced_workflow.py      # Workflow tests
│   ├── test_google_drive.py          # Google Drive tests
│   └── test_fixes.py                 # System integration tests
└── 📜 scripts/                        # Utility scripts
    ├── update_packages.bat            # Dependency updates
    └── run_app.bat                    # Application runner
```

## 🚀 **Quick Start**

### 1. **Environment Setup**
```bash
# Clone repository
git clone <repository-url>
cd UNOPS.PAO.AIService

# Create virtual environment
python -m venv venv
venv\Scripts\activate  # Windows
# source venv/bin/activate  # Linux/Mac

# Install dependencies
pip install -r requirements.txt
```

### 2. **Configuration**
```bash
# Create .env file
echo "CURRENT_ENV=dev" > .env

# Configuration files are already set up:
# - config/framework_config_dev.json (development)
# - config/framework_config_test.json (testing)
# - config/framework_config_prod.json (production)
```

### 3. **Run Application**
```bash
# Option 1: Direct execution
python main.py

# Option 2: Using batch file (Windows)
run_app.bat

# Option 3: With custom port
set PORT=8080 && python main.py
```

### 4. **Access Interfaces**
- **Web Interface**: http://localhost:8000/dev-ui
- **API Documentation**: http://localhost:8000/docs
- **Chat Endpoint**: http://localhost:8000/chat

## 💬 **Usage Examples**

### 🔍 **Search & Discovery**
```
"Find all partners containing UNICEF"
"Search for contacts in Bangladesh"
"Show me documents related to climate change"
"Find interactions from last month"
"Search Google Drive for partnership agreements"
```

### 🔧 **Data Operations**
```
"Create a new partner named Red Cross"
"Update contact John Smith's email address"
"Delete partner ID 123"
"Show me details for contact Madeline"
"Upload a document for partner UNICEF"
```

### 🧠 **AI-Powered Queries**
```
"What's the latest news about UNOPS?"
"Analyze this document for key insights"
"Find partners similar to humanitarian organizations"
"Generate a summary for partner 456"
"Search for opportunities in renewable energy"
```

### 📊 **System Management**
```
"Show cache statistics"
"Clear the partner cache"
"Refresh all cached data"
"Check my permissions for partner 123"
"Display system configuration"
```

## 🔧 **Configuration System**

### 🌍 **Environment-Based Configuration**
```bash
# Set environment in .env file
CURRENT_ENV=dev    # Loads framework_config_dev.json
CURRENT_ENV=test   # Loads framework_config_test.json
CURRENT_ENV=prod   # Loads framework_config_prod.json
```

### ⚙️ **Key Configuration Sections**
```json
{
  "branding": {
    "application_name": "Opportunity+ AI Agent",
    "project_name": "UNOPS Opportunity+",
    "organization": "UNOPS"
  },
  "server": {
    "host": "0.0.0.0",
    "port": 8000,
    "api_base_url": "https://your-api-backend.com"
  },
  "google_cloud": {
    "project": "unops-pao-ai-service",
    "location": "us-central1"
  },
  "runtime": {
    "gemini_model": "gemini-2.5-flash",
    "api_timeout": 30,
    "max_retries": 3
  }
}
```

## 🧪 **Testing**

### 🔍 **Test Suites**
```bash
# Entity detection tests
python test_entity_detection.py

# Workflow processing tests
python test_enhanced_workflow.py

# Google Drive integration tests
python test_google_drive.py

# System integration tests
python test_fixes.py
```

### ✅ **Test Coverage**
- ✅ Entity detection and intent recognition
- ✅ API endpoint mapping and execution
- ✅ Google Drive search and file operations
- ✅ Cache management and invalidation
- ✅ Configuration loading and validation
- ✅ Multi-agent workflow coordination
- ✅ Response formatting and error handling

## 🔒 **Security & Authentication**

### 🛡️ **Security Features**
- **IAP Integration**: Google Identity-Aware Proxy support
- **Role-Based Access Control**: Granular permissions per entity
- **CORS Configuration**: Cross-origin resource sharing controls
- **Secret Management**: Google Cloud Secret Manager integration
- **Secure Sessions**: Encrypted session storage

### 🔑 **Authentication Flow**
1. **User Authentication**: IAP or custom authentication
2. **Session Creation**: Secure session establishment
3. **Permission Checking**: Role-based access validation
4. **API Authorization**: Endpoint-specific permission checks
5. **Audit Logging**: Security event tracking

## 📊 **Monitoring & Analytics**

### 📈 **Built-in Monitoring**
- **Cache Performance**: Hit/miss rates, memory usage
- **Agent Metrics**: Response times, success rates
- **API Analytics**: Endpoint usage, error rates
- **User Activity**: Session tracking, usage patterns

### 📊 **Monitoring Endpoints**
```bash
GET /framework/info          # System information
GET /framework/config        # Configuration status
GET /framework/tools         # Available tools
GET /cache/stats            # Cache statistics
GET /google-drive/files     # Google Drive file listing
```

## 🚀 **Advanced Features**

### 🔄 **Streaming & Real-time**
- **Streaming Responses**: Real-time response generation
- **Session Persistence**: Conversation state management
- **Context Awareness**: Multi-turn conversation support
- **Background Processing**: Asynchronous task execution

### 🧠 **AI & Machine Learning**
- **Vector Embeddings**: Semantic search capabilities
- **Similarity Matching**: AI-powered relationship discovery
- **Content Analysis**: Document understanding and summarization
- **Intent Recognition**: Natural language understanding

### 🔧 **System Administration**
- **Cache Management**: Intelligent caching with TTL
- **Configuration Hot-reload**: Runtime configuration updates
- **Health Monitoring**: System health checks
- **Performance Optimization**: Resource usage optimization

## 🤝 **API Integration**

### 🌐 **Supported API Operations**
- **RESTful APIs**: Full CRUD operations
- **Authentication**: Bearer token and API key support
- **Error Handling**: Comprehensive error management
- **Retry Logic**: Automatic retry with exponential backoff
- **Rate Limiting**: Request throttling and queuing

### 📡 **External Integrations**
- **Google Drive API**: File operations and content search
- **Google Search API**: Web search capabilities
- **Vertex AI**: Advanced AI model integration
- **PostgreSQL**: Database operations and session storage

## 📞 **Support & Troubleshooting**

### 🔧 **Common Issues**
```bash
# Virtual environment not activated
venv\Scripts\activate

# Dependencies not installed
pip install -r requirements.txt

# Port already in use
set PORT=8001 && python main.py

# Configuration issues
python -c "from framework_config import validate_config; print(validate_config())"
```

### 🐛 **Debug Mode**
```bash
# Enable debug logging
python main.py --debug

# Check configuration
python -c "from ai_assistant.config_manager import config_manager; print(config_manager.framework_config)"

# Test Google Drive integration
python test_google_drive.py
```

## 📄 **License & Contributing**

This project is part of UNOPS internal systems and follows UNOPS software development guidelines.

---

**🎯 Quick Summary:**
- **Multi-Agent System**: 15+ entities, intelligent workflow processing
- **Search Capabilities**: Google Drive, web search, semantic similarity
- **AI Integration**: Gemini AI, vector embeddings, natural language processing
- **Enterprise Ready**: Role-based access, caching, monitoring, multi-environment support
- **Easy Deployment**: Single command startup, configuration-driven, Docker ready

**🚀 Get Started:** `python main.py` → Access http://localhost:8000/dev-ui → Start chatting! 