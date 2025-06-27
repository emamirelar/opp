# UNOPS AI Service

An advanced agentic AI system built with Google ADK (Agent Development Kit) for UNOPS operations. This service provides intelligent entity detection, API integration, and workflow automation capabilities.

## 🚀 Features

- **Entity Detection**: Advanced NLP-based entity and intent detection from user queries
- **API Worker**: Automated API calls based on detected entities and intents
- **Workflow Orchestration**: Sequential agent processing with loop management
- **Dynamic Configuration**: Tools and endpoints loaded from `tools.json` configuration
- **Web Interface**: Built-in web UI for testing and interaction
- **Real-time Processing**: FastAPI-based server with streaming support

## 📋 Prerequisites

- Python 3.9 or higher
- Git
- Windows (for batch files) or similar environment

## 🛠️ Initial Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd UNOPSAiService
```

### 2. Create Virtual Environment
```bash
python -m venv venv
```

### 3. Activate Virtual Environment

**Windows:**
```bash
venv\Scripts\activate
```

**Linux/Mac:**
```bash
source venv/bin/activate
```

### 4. Install Dependencies
```bash
pip install -r requirements.txt
```

### 5. Environment Configuration

Create a `.env` file in the root directory (optional):
```env
# Server Configuration
PORT=8000
HOST=0.0.0.0
SERVE_WEB_INTERFACE=true

# Database Configuration  
DATABASE_URL=postgresql://postgres:password@localhost:5433/database_name

# Development Settings
API_BASE_URL=https://localhost:44426
IS_DEVELOPMENT=true
DEV_EMAIL=your.email@example.com
```

## 🚀 Quick Start

### Option 1: Using Batch Files (Windows)

**Update Dependencies:**
```bash
update_packages.bat
```

**Run the Application:**
```bash
run_app.bat
```

This will automatically:
- Activate the virtual environment
- Start the FastAPI server
- Open the web interface in your browser

### Option 2: Manual Commands

**Activate Virtual Environment:**
```bash
venv\Scripts\activate
```

**Run the Application:**
```bash
python main.py
```

**Access the Web Interface:**
Open your browser to: http://localhost:8000/dev-ui

## 🏗️ Architecture

### Agent Structure
```
workflow_agent (SequentialAgent)
├── entity_detection_agent (LlmAgent)
│   ├── Detects entities from user input
│   ├── Determines user intent
│   └── Extracts parameters
└── api_worker_agent (LoopAgent)
    └── api_caller_agent (LlmAgent)
        ├── Maps entities to API endpoints
        ├── Executes HTTP requests
        └── Formats responses
```

### Key Components

- **Entity Detection**: Processes user queries to identify entities (Contact, Partner, Interaction, etc.)
- **Intent Recognition**: Determines user intent (search, create, update, delete)
- **API Integration**: Automatically calls appropriate API endpoints based on detection results
- **Loop Management**: Handles multi-step workflows with proper exit conditions
- **Configuration Management**: Dynamic loading from `tools.json` configuration file

## 📁 Project Structure

```
UNOPSAiService/
├── ai_assistant/
│   ├── agent.py                    # Root agent definition
│   ├── config_manager.py           # Configuration management
│   └── workflow_agent/
│       ├── agent.py                # Main workflow agent
│       ├── entity_detection/       # Entity detection sub-agent
│       └── api_worker/             # API worker sub-agent
├── config/
│   ├── tools.json                  # API endpoints and entity configuration
│   └── knowledge_base.txt          # Knowledge base content
├── main.py                         # FastAPI application entry point
├── requirements.txt                # Python dependencies
├── update_packages.bat             # Dependency update script
├── run_app.bat                     # Application runner script
└── README.md                       # This file
```

## 🔧 Configuration

### tools.json
The `config/tools.json` file contains:
- Entity definitions and synonyms
- API endpoint configurations
- Parameter mappings
- Usage examples

### Environment Variables
- `PORT`: Server port (default: 8000)
- `HOST`: Server host (default: 0.0.0.0)
- `API_BASE_URL`: Backend API base URL
- `DATABASE_URL`: PostgreSQL connection string
- `IS_DEVELOPMENT`: Enable development mode
- `DEV_EMAIL`: Development user email

## 🧪 Testing

Run the test suites to verify functionality:

```bash
# Test entity detection
python test_entity_detection.py

# Test workflow processing
python test_enhanced_workflow.py

# Test intent detection fixes
python test_madeline_intent_fix.py

# Test loop exit behavior
python test_loop_exit_behavior.py
```

## 📚 API Endpoints

### Main Endpoints
- `GET /`: Root endpoint
- `POST /chat`: Chat with the AI agent
- `GET /dev-ui`: Web development interface
- `GET /docs`: Interactive API documentation

### Chat Request Format
```json
{
  "app_name": "opportunity_ai_agent",
  "user_id": "user123", 
  "session_id": "session456",
  "message": "Get details of contact Madeline",
  "streaming": false,
  "state": {}
}
```

## 💡 Usage Examples

### Basic Queries
- `"Find all contacts"`
- `"Get details of contact Madeline"`
- `"Create a new partner named UNICEF"`
- `"Update contact John Smith's email"`
- `"Show me all interactions from last week"`

### Entity Types Supported
- **Contact**: Individual contacts and their information
- **Partner**: Partner organizations and relationships
- **Interaction**: Communications and meetings
- **Document**: File and document management
- **AiPrompt**: AI prompt templates
- **Configuration**: System configuration management

## 🐛 Troubleshooting

### Common Issues

**1. Virtual Environment Not Activated**
```bash
# Ensure you see (venv) in your command prompt
venv\Scripts\activate
```

**2. Dependencies Not Installed**
```bash
pip install -r requirements.txt
```

**3. Port Already in Use**
```bash
# Change port in .env file or use environment variable
set PORT=8001
python main.py
```

**4. Database Connection Issues**
- Check PostgreSQL server is running
- Verify DATABASE_URL configuration
- Ensure database exists and is accessible

### Debug Mode
Run with debug logging:
```bash
python main.py --debug
```

## 🔒 Security Notes

- The application includes IAP header simulation for development
- Use proper authentication in production environments
- Configure CORS settings appropriately
- Secure database connections with proper credentials

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests to ensure functionality
5. Submit a pull request

## 📄 License

This project is part of UNOPS internal systems and follows UNOPS software development guidelines.

## 📞 Support

For technical support or questions:
- Check the troubleshooting section
- Review the test files for usage examples
- Consult the API documentation at `/docs`

---

**Quick Start Summary:**
1. Run `update_packages.bat` (first time setup)
2. Run `run_app.bat` (to start the application)
3. Access http://localhost:8000/dev-ui in your browser
4. Start chatting with the AI agent! 