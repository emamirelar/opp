# Opportunity+ AI Agent - Deployment Guide

## Project Structure

Your project is now organized for both local development and production deployment:

```
agentic-ai/
├── opportunity_ai_agent/     # Your agent folder
│   ├── __init__.py
│   ├── agent.py             # Main agent definition
│   ├── tools/               # Agent tools
│   ├── sub_agents/          # Sub-agents
│   └── utils/               # Utilities
├── main.py                  # FastAPI application (PRODUCTION)
├── main_cli.py              # CLI version (DEVELOPMENT)
├── requirements.txt         # Python dependencies
├── Dockerfile               # Container build instructions (recommended format)
├── docker-compose.yml       # Local development with database
├── .env                     # Environment variables
├── utils.py                 # Utility functions
└── test_rest_api.py         # API testing script
```

## Running Locally

### Option 1: FastAPI Web Server (Recommended)
```bash
# Start the FastAPI server with web UI
python main.py

# Server will start on http://localhost:8000
# Includes web UI, REST API, and database persistence
```

### Option 2: CLI Interface (Development)
```bash
# Run the CLI version for interactive testing
python main_cli.py

# Interactive command-line interface
# Uses same database as web version
```

### Option 3: Docker Compose (Full Stack)
```bash
# Start both app and database in containers
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

## Environment Variables

Create a `.env` file with:
```env
# Database Configuration
DATABASE_URL=postgresql://postgres:0Y%2FX3YNxHLL0fL4T@localhost:5433/anusha

# Google Cloud Configuration (if using Vertex AI)
GOOGLE_GENAI_USE_VERTEXAI=TRUE
GOOGLE_CLOUD_PROJECT=unops-partneropportunity
GOOGLE_CLOUD_LOCATION=europe-west4

# Optional: Port override
PORT=8000
```

## API Usage

### 1. Create Session with Initial State
```bash
curl -X POST "http://localhost:8000/apps/opportunity_ai_agent/users/user/sessions" \
  -H "Content-Type: application/json" \
  -d '{
    "state": {
      "user_name": "Anusha Swaminathan",
      "user_email": "anushas@unops.org",
      "user_role": "Partnership Manager",
      "user_department": "UNOPS"
    }
  }'
```

### 2. Chat with Agent (Server-Sent Events)
```bash
curl -X POST "http://localhost:8000/apps/opportunity_ai_agent/users/user/sessions/{session_id}/run_sse" \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Hello! What can you help me with?"
  }'
```

### 3. Get Session Information
```bash
curl -X GET "http://localhost:8000/apps/opportunity_ai_agent/users/user/sessions/{session_id}"
```

## Docker Deployment

### Recommended Dockerfile (Already Configured)
The project uses the recommended ADK Dockerfile format:
```dockerfile
FROM python:3.13-slim
WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

RUN adduser --disabled-password --gecos "" myuser && \
    chown -R myuser:myuser /app

COPY . .

USER myuser

ENV PATH="/home/myuser/.local/bin:$PATH"

CMD ["sh", "-c", "uvicorn main:app --host 0.0.0.0 --port $PORT"]
```

### Build and Run
```bash
# Build the image
docker build -t opportunity-ai-agent .

# Run with environment variables
docker run -p 8000:8000 \
  -e PORT=8000 \
  -e DATABASE_URL="postgresql://postgres:password@host:5432/database" \
  -e GOOGLE_CLOUD_PROJECT="your-project" \
  -e GOOGLE_CLOUD_LOCATION="your-location" \
  opportunity-ai-agent
```

### Using Docker Compose (Recommended for Local Development)
```bash
# Start all services (app + database)
docker-compose up -d

# View logs
docker-compose logs -f app

# Stop all services
docker-compose down

# Rebuild and restart
docker-compose up --build -d
```

The docker-compose.yml includes:
- FastAPI application
- PostgreSQL database
- Proper networking between services
- Volume persistence for database
- Environment variable configuration

## Cloud Deployment

### Google Cloud Run
```bash
# Build and push to Google Container Registry
gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/opportunity-ai-agent

# Deploy to Cloud Run
gcloud run deploy opportunity-ai-agent \
  --image gcr.io/YOUR_PROJECT_ID/opportunity-ai-agent \
  --platform managed \
  --region YOUR_REGION \
  --allow-unauthenticated \
  --set-env-vars="PORT=8080,DATABASE_URL=YOUR_DATABASE_URL,GOOGLE_CLOUD_PROJECT=YOUR_PROJECT_ID"
```

### Other Cloud Providers
The Docker image can be deployed to:
- AWS ECS/Fargate
- Azure Container Instances
- Kubernetes clusters
- Any Docker-compatible platform

## Testing

### Test the API
```bash
python test_rest_api.py
```

### Health Check
```bash
curl http://localhost:8000/health
```

### API Information
```bash
curl http://localhost:8000/api/info
```

## Monitoring and Logs

### Health Check Endpoint
- `GET /health` - Returns service status and database connectivity

### Logging
- Application logs are written to stdout
- Configure log level with `LOG_LEVEL` environment variable

### Docker Logs
```bash
# View application logs
docker-compose logs -f app

# View database logs
docker-compose logs -f db
```

## Security Considerations

### Production Deployment
1. **CORS**: Update `ALLOWED_ORIGINS` in `main.py` to restrict cross-origin requests
2. **Environment Variables**: Use secure secret management (not .env files)
3. **Database**: Use SSL connections and proper authentication
4. **HTTPS**: Always use HTTPS in production
5. **Authentication**: Add authentication middleware if needed

### Example Secure Configuration
```python
ALLOWED_ORIGINS = [
    "https://your-frontend-domain.com",
    "https://your-app-domain.com"
]
```

## Troubleshooting

### Common Issues
1. **Database Connection**: Check DATABASE_URL format and network connectivity
2. **Agent Loading**: Ensure `opportunity_ai_agent/agent.py` has `root_agent` defined
3. **Dependencies**: Run `pip install -r requirements.txt`
4. **Port Conflicts**: Change PORT environment variable if 8000 is in use

### Docker Issues
```bash
# Check if containers are running
docker-compose ps

# View container logs
docker-compose logs app

# Restart specific service
docker-compose restart app

# Rebuild from scratch
docker-compose down
docker-compose up --build
```

### Debug Mode
Set environment variable for development:
```env
DEBUG=true
```

### Logs
Check application logs for detailed error information:
```bash
python main.py 2>&1 | tee app.log
``` 