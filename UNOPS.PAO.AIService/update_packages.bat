@echo off
echo =====================================
echo UNOPS AI Service - Update Packages
echo =====================================

REM Check if virtual environment exists
if not exist "venv\Scripts\activate.bat" (
    echo Virtual environment not found. Creating virtual environment...
    python -m venv venv
    if errorlevel 1 (
        echo Failed to create virtual environment. Please ensure Python is installed.
        pause
        exit /b 1
    )
    echo Virtual environment created successfully!
)

REM Activate virtual environment
echo Activating virtual environment...
call venv\Scripts\activate.bat

REM Check if activation was successful
if "%VIRTUAL_ENV%"=="" (
    echo Failed to activate virtual environment.
    pause
    exit /b 1
)

echo Virtual environment activated: %VIRTUAL_ENV%

REM Upgrade pip to latest version
echo Upgrading pip to latest version...
python -m pip install --upgrade pip

REM Install/update requirements
echo Installing/updating packages from requirements.txt...
pip install -r requirements.txt

if errorlevel 1 (
    echo Failed to install packages. Please check requirements.txt and try again.
    pause
    exit /b 1
)

echo.
echo =====================================
echo Package update completed successfully!
echo =====================================
echo.
echo Your virtual environment is ready to use.
echo You can now run 'run_app.bat' to start the application.
echo.
pause 