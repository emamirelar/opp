@echo off
setlocal enabledelayedexpansion

:: Configuration
set METADATA_PATH=..\UNOPS.PAO.AIService\api-metadata.json
set OUTPUT_PATH=tools.json

echo ============================================================================
echo 🚀 TOOLS.JSON GENERATOR - Using Existing API Metadata
echo ============================================================================
echo.

echo 📋 Configuration:
echo    Metadata Path: %METADATA_PATH%
echo    Output:        %OUTPUT_PATH%
echo.

:: Validate input file exists
if not exist "%METADATA_PATH%" (
    echo ❌ Error: API metadata file not found: %METADATA_PATH%
    echo    Please ensure the api-metadata.json file exists in the specified path
    pause
    exit /b 1
)

echo ============================================================================
echo 🤖 STEP 1: Generating tools.json with LLM from existing metadata...
echo ============================================================================

:: Change to script directory
cd /d "%~dp0"

:: Run Python LLM generator with existing metadata
python llm_generator.py --input "%METADATA_PATH%" --output "%OUTPUT_PATH%"

if %ERRORLEVEL% neq 0 (
    echo ❌ Error: Step 1 failed: LLM generation error
    pause
    exit /b 1
)

echo ✅ Step 1 completed successfully
echo.

echo ============================================================================
echo 🎉 SUCCESS! tools.json has been generated successfully!
echo ============================================================================
echo    📄 Output file: %OUTPUT_PATH%
echo    🕒 Generated at: %DATE% %TIME%

if exist "%OUTPUT_PATH%" (
    for %%A in ("%OUTPUT_PATH%") do set FILE_SIZE=%%~zA
    echo    📊 File size: !FILE_SIZE! bytes
) else (
    echo ❌ Warning: Output file was not created
    pause
    exit /b 1
)

echo.
echo ✅ Generation complete! Your API documentation is ready for the AI agent.
echo.
echo 📁 Generated files:
echo    • tools/tools.json (combined)
echo    • tools/endpoints/*-tools.json (individual entities)

pause
