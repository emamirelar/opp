@echo off
setlocal enabledelayedexpansion

if "%~1"=="" (
    echo ❌ Error: Environment name is required
    echo Usage: set-environment.bat ^<environment^>
    echo.
    echo Available environments:
    echo   dev        - Development environment
    echo   staging    - Staging environment  
    echo   production - Production environment
    echo.
    echo Example: set-environment.bat dev
    exit /b 1
)

set "ENVIRONMENT=%~1"
set "ENV_FILE=environments\%ENVIRONMENT%.env"

if not exist "%ENV_FILE%" (
    echo ❌ Error: Environment file not found: %ENV_FILE%
    echo.
    echo Available environments:
    for %%f in (environments\*.env) do (
        set "filename=%%~nf"
        echo   !filename!
    )
    exit /b 1
)

echo ============================================================================
echo 🌍 SETTING ENVIRONMENT: %ENVIRONMENT%
echo ============================================================================
echo.

echo 📂 Loading configuration from: %ENV_FILE%
echo.

REM Read and set environment variables from file
for /f "usebackq tokens=1,2 delims==" %%a in ("%ENV_FILE%") do (
    if not "%%a"=="" if not "%%b"=="" (
        set "%%a=%%b"
        echo ✅ Set %%a=%%b
    )
)

echo.
echo ============================================================================
echo ✅ Environment %ENVIRONMENT% configured successfully!
echo ============================================================================
echo.
echo 🎯 Current configuration:
echo    Project: !GOOGLE_CLOUD_PROJECT!
echo    Location: !GOOGLE_CLOUD_LOCATION!
echo.
echo 🚀 Now you can build your project:
echo    dotnet build UNOPS.PAO.Presentation
echo.
echo 💡 Or build with explicit configuration:
echo    dotnet build UNOPS.PAO.Presentation -p:GoogleCloudProject=!GOOGLE_CLOUD_PROJECT! -p:GoogleCloudLocation=!GOOGLE_CLOUD_LOCATION!
echo.
echo ============================================================================ 