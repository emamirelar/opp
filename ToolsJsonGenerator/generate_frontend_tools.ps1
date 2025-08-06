param(
    [Parameter(Mandatory=$true)]
    [string]$AngularProject,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "../UNOPS.PAO.AIService/config"
)

Write-Host "============================================================================" -ForegroundColor Green
Write-Host "🎨 FRONTEND UI TOOLS GENERATOR - Angular Component Documentation Generator" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Green
Write-Host ""

Write-Host "📋 Configuration:" -ForegroundColor Cyan
Write-Host "   Angular Project: $AngularProject" -ForegroundColor White
Write-Host "   Output Directory: $OutputDir/tools/ui/" -ForegroundColor White
Write-Host ""

# Validate Angular project exists
if (-not (Test-Path $AngularProject)) {
    Write-Host "❌ Error: Angular project not found: $AngularProject" -ForegroundColor Red
    exit 1
}

# Validate Angular project structure
if (-not (Test-Path (Join-Path $AngularProject "src\app"))) {
    Write-Host "❌ Error: Invalid Angular project structure. Missing src\app directory." -ForegroundColor Red
    exit 1
}

try {
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "🔍 STEP 1: Extracting Angular component metadata..." -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    
    # Change to script directory
    Push-Location $PSScriptRoot
    
    # Check if frontend extractor exists
    if (-not (Test-Path "frontend_extractor.py")) {
        throw "frontend_extractor.py not found. Make sure it's in the ToolsJsonGenerator directory."
    }
    
    Write-Host "✅ Step 1 completed - Frontend extractor ready" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "🤖 STEP 2: Generating UI guidance with Vertex AI..." -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    
    # Run frontend UI generator
    python generate_frontend_tools.py --angular-project $AngularProject --output-dir $OutputDir
    
    if ($LASTEXITCODE -ne 0) {
        throw "Step 2 failed: Frontend UI generation error"
    }
    
    Write-Host "✅ Step 2 completed successfully" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "🎉 SUCCESS! Frontend UI guidance has been generated successfully!" -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "   📁 Angular Project: $AngularProject" -ForegroundColor White
    Write-Host "   📄 Output Directory: $OutputDir\tools" -ForegroundColor White
    Write-Host "   🕒 Generated at: $(Get-Date)" -ForegroundColor White
    Write-Host ""
    
    # Check output files
    $uiFiles = Get-ChildItem -Path (Join-Path $OutputDir "tools") -Filter "*-ui.json" -ErrorAction SilentlyContinue
    
    if ($uiFiles) {
        Write-Host "📊 Generated UI guidance files:" -ForegroundColor White
        foreach ($file in $uiFiles) {
            $fileSize = (Get-Item $file.FullName).Length
            Write-Host "   - $($file.Name) ($fileSize bytes)" -ForegroundColor White
        }
    } else {
        Write-Host "⚠️ Warning: No UI guidance files were generated" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host ""
    Write-Host "✅ Frontend UI guidance ready for AI assistant!" -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "💡 NEXT STEPS:" -ForegroundColor Cyan
    Write-Host "   1. Your AI assistant can now provide contextual help for Angular pages" -ForegroundColor White
    Write-Host "   2. Add more @uiEntity documentation to components for richer guidance" -ForegroundColor White
    Write-Host "   3. Regenerate UI guidance when you add new components or features" -ForegroundColor White
    Write-Host ""
    Write-Host "📖 To use with your AI service:" -ForegroundColor Cyan
    Write-Host "   - UI guidance files are in: $OutputDir\tools\*-ui.json" -ForegroundColor White
    Write-Host "   - Your AI can now help users with page-specific guidance" -ForegroundColor White
    Write-Host "   - Each entity has both backend tools and frontend UI guidance" -ForegroundColor White
    Write-Host "============================================================================" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
} 