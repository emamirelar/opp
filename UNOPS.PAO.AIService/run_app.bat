@echo off
echo =====================================
echo UNOPS AI Service - Application Runner
echo =====================================

REM Check if virtual environment exists
if not exist "venv\Scripts\activate.bat" (
    echo Virtual environment not found!
    echo Please run 'update_packages.bat' first to set up the environment.
    pause
    exit /b 1
)

REM Activate virtual environment
echo Activating virtual environment...
call venv\Scripts\activate.bat

REM Check if activation was successful
if "%VIRTUAL_ENV%"=="" (
    echo Failed to activate virtual environment.
    echo Please run 'update_packages.bat' to fix the environment.
    pause
    exit /b 1
)

echo Virtual environment activated: %VIRTUAL_ENV%

REM Check if main.py exists
if not exist "main.py" (
    echo main.py not found in current directory.
    echo Please ensure you're running this from the project root directory.
    pause
    exit /b 1
)

REM Display startup information
echo.
echo Starting UNOPS AI Service...
echo - Server will start on: http://localhost:8000
echo - Web UI will be available at: http://localhost:8000/dev-ui
echo - API Documentation: http://localhost:8000/docs
echo.
echo The web interface will open automatically in your browser.
echo To stop the server, press Ctrl+C in this window.
echo.

REM Start the application in background and open browser
echo Starting FastAPI server...
start "UNOPS AI Service" python main.py

REM Wait a moment for server to start up
timeout /t 3 /nobreak >nul

REM Open the web interface in default browser
echo Opening web interface...
start http://localhost:8000/dev-ui?ai_assistant

REM Wait for the server process to finish
echo.
echo =====================================
echo Server is running...
echo =====================================
echo.
echo Web interface opened in your browser: http://localhost:8000/dev-ui?ai_assistant
echo.
echo To stop the server:
echo 1. Close this window, OR
echo 2. Press Ctrl+C in the server window
echo.
echo Server logs will appear in the "UNOPS AI Service" window.
echo.
pause 