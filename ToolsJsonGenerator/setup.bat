@echo off
echo ============================================================================
echo 🚀 TOOLS.JSON GENERATOR SETUP
echo ============================================================================
echo.

echo 📋 This script will help you set up the Python environment for LLM generation
echo.

REM Check if Python is installed
echo 🐍 Checking Python installation...
python --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Python is not installed or not in PATH
    echo    Please install Python 3.8+ from https://python.org
    echo    Make sure to add Python to PATH during installation
    pause
    exit /b 1
)

python --version
echo ✅ Python is available
echo.

REM Install dependencies
echo 📦 Installing Python dependencies...
pip install -r requirements.txt

if errorlevel 1 (
    echo ❌ Failed to install dependencies
    echo    Try running: pip install --upgrade pip
    echo    Then run this script again
    pause
    exit /b 1
)

echo ✅ Dependencies installed successfully
echo.

echo 🔑 Google Cloud Setup:
echo    You need to set up Google Cloud authentication and environment variables:
echo    
echo    Option 1 - Set environment variables for current session:
echo       set GOOGLE_CLOUD_PROJECT=your-project-id
echo       set GOOGLE_CLOUD_LOCATION=us-central1
echo    
echo    Option 2 - Set permanently (Windows):
echo       setx GOOGLE_CLOUD_PROJECT "your-project-id"
echo       setx GOOGLE_CLOUD_LOCATION "us-central1"
echo    
echo    Option 3 - Authenticate with gcloud CLI:
echo       gcloud auth application-default login
echo    
echo    Make sure Vertex AI API is enabled in your Google Cloud project!
echo.

echo ============================================================================
echo ✅ Setup Complete!
echo ============================================================================
echo.
echo 🎯 Next steps:
echo    1. Set up Google Cloud authentication (see options above)
echo    2. Enable Vertex AI API in your Google Cloud project
echo    3. Build your project - tools.json will be generated automatically
echo    4. Or run manually: python llm_generator.py --input api-metadata.json --output tools.json --project your-project-id
echo.
pause 