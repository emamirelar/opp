@echo off
setlocal enabledelayedexpansion

echo ============================================================================
echo 🎪 DRIVERJS TOUR GENERATOR - Create Interactive Tours from UI Metadata
echo ============================================================================
echo.

REM Check if UI tools directory is provided
if "%~1"=="" (
    echo ❌ Error: UI tools directory is required
    echo Usage: generate_tours.bat ^<ui_tools_dir^> ^[angular_output_dir^]
    echo Example: generate_tours.bat "../UNOPS.PAO.AIService/config/tools/ui" "../UNOPS.PAO.ClientApp/src/app/common/tours"
    exit /b 1
)

set "UI_TOOLS_DIR=%~1"
set "OUTPUT_DIR=%~2"
if "%OUTPUT_DIR%"=="" set "OUTPUT_DIR=../UNOPS.PAO.ClientApp/src/app/common/tours"

echo 📋 Configuration:
echo    UI Tools Directory: %UI_TOOLS_DIR%
echo    Output Directory: %OUTPUT_DIR%
echo.

REM Validate UI tools directory exists
if not exist "%UI_TOOLS_DIR%" (
    echo ❌ Error: UI tools directory not found: %UI_TOOLS_DIR%
    exit /b 1
)

echo ============================================================================
echo 🎪 Generating DriverJS tours from UI metadata...
echo ============================================================================

cd /d "%~dp0"
python tour_generator.py --ui-tools-dir "%UI_TOOLS_DIR%" --output-dir "%OUTPUT_DIR%"

if !ERRORLEVEL! neq 0 (
    echo.
    echo ❌ Tour generation failed with error code !ERRORLEVEL!
    exit /b !ERRORLEVEL!
)

echo.
echo ============================================================================
echo ✅ SUCCESS: DriverJS tours generated!
echo ============================================================================
echo 📁 Tours saved to: %OUTPUT_DIR%
echo 🎪 Import these tour files in your Angular components to enable guided tours
echo.
echo Next steps:
echo 1. Install driver.js in your Angular project: npm install driver.js
echo 2. Import tour configurations in your components
echo 3. Initialize tours with DriverJS
echo ============================================================================ 