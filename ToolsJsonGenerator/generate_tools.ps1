param(
    [Parameter(Mandatory=$true)]
    [string]$DllPath,
    
    [Parameter(Mandatory=$true)]
    [string]$XmlPath,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "tools.json"
)

Write-Host "============================================================================" -ForegroundColor Green
Write-Host "🚀 TOOLS.JSON GENERATOR - Automated API Documentation Generator" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Green
Write-Host ""

$TempFile = "temp_endpoints_$(Get-Random).json"

Write-Host "📋 Configuration:" -ForegroundColor Cyan
Write-Host "   DLL Path: $DllPath" -ForegroundColor White
Write-Host "   XML Path: $XmlPath" -ForegroundColor White  
Write-Host "   Output:   $OutputPath" -ForegroundColor White
Write-Host "   Temp:     $TempFile" -ForegroundColor White
Write-Host ""

# Validate input files exist
if (-not (Test-Path $DllPath)) {
    Write-Host "❌ Error: Assembly file not found: $DllPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $XmlPath)) {
    Write-Host "❌ Error: XML documentation file not found: $XmlPath" -ForegroundColor Red
    exit 1
}

try {
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "🔍 STEP 1: Extracting endpoints with .NET reflection..." -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    
    # Change to script directory
    Push-Location $PSScriptRoot
    
    # Run .NET reflection extractor
    dotnet run --project ReflectionExtractor -- --dll $DllPath --xml $XmlPath --output $TempFile
    
    if ($LASTEXITCODE -ne 0) {
        throw "Step 1 failed: .NET reflection extraction error"
    }
    
    if (-not (Test-Path $TempFile)) {
        throw "Step 1 failed: Temporary file was not created"
    }
    
    Write-Host "✅ Step 1 completed successfully" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "🤖 STEP 2: Generating tools.json with LLM..." -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    
    # Run Python LLM generator
    python llm_generator.py --input $TempFile --output $OutputPath
    
    if ($LASTEXITCODE -ne 0) {
        throw "Step 2 failed: LLM generation error"
    }
    
    Write-Host "✅ Step 2 completed successfully" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "🧹 STEP 3: Cleaning up temporary files..." -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    
    if (Test-Path $TempFile) {
        Remove-Item $TempFile
        Write-Host "✅ Removed temporary file: $TempFile" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Temporary file not found (may have been cleaned up already)" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "🎉 SUCCESS! tools.json has been generated successfully!" -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "   📄 Output file: $OutputPath" -ForegroundColor White
    Write-Host "   🕒 Generated at: $(Get-Date)" -ForegroundColor White
    
    if (Test-Path $OutputPath) {
        $fileSize = (Get-Item $OutputPath).Length
        Write-Host "   📊 File size: $fileSize bytes" -ForegroundColor White
    } else {
        Write-Host "❌ Warning: Output file was not created" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    Write-Host "✅ Generation complete! Your API documentation is ready for the AI agent." -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    
    # Cleanup on error
    if (Test-Path $TempFile) {
        Remove-Item $TempFile -ErrorAction SilentlyContinue
    }
    
    exit 1
} finally {
    Pop-Location
} 