@echo off
setlocal enabledelayedexpansion

echo ============================================================================
echo 🚀 TOOLS.JSON GENERATOR - Automated API Documentation Generator
echo ============================================================================
echo.

REM Check if required parameters are provided
if "%~1"=="" (
    echo ❌ Error: DLL path is required
    echo Usage: generate_tools.bat ^<dll_path^> ^<xml_path^> ^[output_path^]
    echo Example: generate_tools.bat "bin\Release\net9.0\UNOPS.PAO.Presentation.dll" "bin\Release\net9.0\UNOPS.PAO.Presentation.xml" "tools.json"
    exit /b 1
)

if "%~2"=="" (
    echo ❌ Error: XML documentation path is required
    echo Usage: generate_tools.bat ^<dll_path^> ^<xml_path^> ^[output_path^]
    exit /b 1
)

set "DLL_PATH=%~1"
set "XML_PATH=%~2"
set "OUTPUT_PATH=%~3"
if "%OUTPUT_PATH%"=="" set "OUTPUT_PATH=tools.json"

set "TEMP_FILE=temp_endpoints_%RANDOM%.json"

echo 📋 Configuration:
echo    DLL Path: %DLL_PATH%
echo    XML Path: %XML_PATH%
echo    Output:   %OUTPUT_PATH%
echo    Temp:     %TEMP_FILE%
echo.

REM Validate input files exist
if not exist "%DLL_PATH%" (
    echo ❌ Error: Assembly file not found: %DLL_PATH%
    exit /b 1
)

if not exist "%XML_PATH%" (
    echo ❌ Error: XML documentation file not found: %XML_PATH%
    exit /b 1
)

echo ============================================================================
echo 🔍 STEP 1: Extracting endpoints with .NET reflection...
echo ============================================================================

cd /d "%~dp0"
dotnet run --project ReflectionExtractor -- --dll "%DLL_PATH%" --xml "%XML_PATH%" --output "%TEMP_FILE%"

if !ERRORLEVEL! neq 0 (
    echo ❌ Step 1 failed: .NET reflection extraction error
    if exist "%TEMP_FILE%" del "%TEMP_FILE%"
    exit /b 1
)

if not exist "%TEMP_FILE%" (
    echo ❌ Step 1 failed: Temporary file was not created
    exit /b 1
)

echo ✅ Step 1 completed successfully

echo.
echo ============================================================================
echo 🤖 STEP 2: Generating tools.json with LLM...
echo ============================================================================

python llm_generator.py --input "%TEMP_FILE%" --output "%OUTPUT_PATH%"

if !ERRORLEVEL! neq 0 (
    echo ❌ Step 2 failed: LLM generation error
    if exist "%TEMP_FILE%" del "%TEMP_FILE%"
    exit /b 1
)

echo ✅ Step 2 completed successfully

echo.
echo ============================================================================
echo 🧹 STEP 3: Cleaning up temporary files...
echo ============================================================================

if exist "%TEMP_FILE%" (
    del "%TEMP_FILE%"
    echo ✅ Removed temporary file: %TEMP_FILE%
) else (
    echo ⚠️  Temporary file not found (may have been cleaned up already)
)

echo.
echo ============================================================================
echo 🎉 SUCCESS! tools.json has been generated successfully!
echo ============================================================================
echo    📄 Output file: %OUTPUT_PATH%
echo    🕒 Generated at: %date% %time%
echo.

if exist "%OUTPUT_PATH%" (
    for %%A in ("%OUTPUT_PATH%") do echo    📊 File size: %%~zA bytes
) else (
    echo ❌ Warning: Output file was not created
    exit /b 1
)

echo.
echo ✅ Generation complete! Your API documentation is ready for the AI agent.
echo ============================================================================ 