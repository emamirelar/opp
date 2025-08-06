@echo off
setlocal enabledelayedexpansion

echo ============================================================================
echo 🎨 FRONTEND UI TOOLS GENERATOR - Angular Component Documentation Generator
echo ============================================================================
echo.

REM Check if Angular project path is provided
if "%~1"=="" (
    echo ❌ Error: Angular project path is required
    echo Usage: generate_frontend_tools.bat ^<angular_project_path^> ^[output_dir^]
    echo Example: generate_frontend_tools.bat "../UNOPS.PAO.ClientApp" "../UNOPS.PAO.AIService/config"
    exit /b 1
)

set "ANGULAR_PROJECT=%~1"
set "OUTPUT_DIR=%~2"
if "%OUTPUT_DIR%"=="" set "OUTPUT_DIR=../UNOPS.PAO.AIService/config"

echo 📋 Configuration:
echo    Angular Project: %ANGULAR_PROJECT%
echo    Output Directory: %OUTPUT_DIR%/tools/ui/
echo.

REM Validate Angular project exists
if not exist "%ANGULAR_PROJECT%" (
    echo ❌ Error: Angular project not found: %ANGULAR_PROJECT%
    exit /b 1
)

REM Validate Angular project structure
if not exist "%ANGULAR_PROJECT%\src\app" (
    echo ❌ Error: Invalid Angular project structure. Missing src\app directory.
    exit /b 1
)

echo ============================================================================
echo 🔍 STEP 1: Extracting Angular component metadata...
echo ============================================================================

cd /d "%~dp0"

REM Check if frontend extractor exists
if not exist "frontend_extractor.py" (
    echo ❌ Error: frontend_extractor.py not found. Make sure it's in the ToolsJsonGenerator directory.
    exit /b 1
)

echo ✅ Step 1 completed - Frontend extractor ready

echo.
echo ============================================================================
echo 🤖 STEP 2: Generating UI guidance with Vertex AI...
echo ============================================================================

python generate_frontend_tools.py --angular-project "%ANGULAR_PROJECT%" --output-dir "%OUTPUT_DIR%"

if !ERRORLEVEL! neq 0 (
    echo ❌ Step 2 failed: Frontend UI generation error
    exit /b 1
)

echo ✅ Step 2 completed successfully

echo.
echo ============================================================================
echo 🎉 SUCCESS! Frontend UI guidance has been generated successfully!
echo ============================================================================
echo    📁 Angular Project: %ANGULAR_PROJECT%
echo    📄 Output Directory: %OUTPUT_DIR%\tools
echo    🕒 Generated at: %date% %time%
echo.

REM Check output files
if exist "%OUTPUT_DIR%\tools\*-ui.json" (
    echo 📊 Generated UI guidance files:
    for %%f in ("%OUTPUT_DIR%\tools\*-ui.json") do (
        for %%A in ("%%f") do echo    - %%~nxA (%%~zA bytes)
    )
) else (
    echo ⚠️ Warning: No UI guidance files were generated
    exit /b 1
)

echo.
echo ✅ Frontend UI guidance ready for AI assistant!
echo ============================================================================

echo.
echo 💡 NEXT STEPS:
echo    1. Your AI assistant can now provide contextual help for Angular pages
echo    2. Add more @uiEntity documentation to components for richer guidance
echo    3. Regenerate UI guidance when you add new components or features
echo.
echo 📖 To use with your AI service:
echo    - UI guidance files are in: %OUTPUT_DIR%\tools\*-ui.json
echo    - Your AI can now help users with page-specific guidance
echo    - Each entity has both backend tools and frontend UI guidance
echo ============================================================================ 