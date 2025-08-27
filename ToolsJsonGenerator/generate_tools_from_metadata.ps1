param(
    [Parameter(Mandatory=$false)]
    [string]$MetadataPath = "../UNOPS.PAO.AIService/api-metadata.json",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "tools.json"
)

Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "TOOLS.JSON GENERATOR - Using Existing API Metadata" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Green
Write-Host ""

    Write-Host "Configuration:" -ForegroundColor Cyan
    Write-Host "   Metadata Path: $MetadataPath" -ForegroundColor White
    Write-Host "   Output:        $OutputPath" -ForegroundColor White
Write-Host ""

# Validate input file exists
if (-not (Test-Path $MetadataPath)) {
    Write-Host "❌ Error: API metadata file not found: $MetadataPath" -ForegroundColor Red
    Write-Host "   Please ensure the api-metadata.json file exists in the specified path" -ForegroundColor Yellow
    exit 1
}

try {
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "STEP 1: Generating tools.json with LLM from existing metadata..." -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    
    # Change to script directory
    Push-Location $PSScriptRoot
    
    # Run Python LLM generator with existing metadata
    python llm_generator.py --input $MetadataPath --output $OutputPath
    
    if ($LASTEXITCODE -ne 0) {
        throw "Step 1 failed: LLM generation error"
    }
    
    Write-Host "✅ Step 1 completed successfully" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "SUCCESS! tools.json has been generated successfully!" -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "   Output file: $OutputPath" -ForegroundColor White
    Write-Host "   Generated at: $(Get-Date)" -ForegroundColor White
    
    if (Test-Path $OutputPath) {
        $fileSize = (Get-Item $OutputPath).Length
        Write-Host "   File size: $fileSize bytes" -ForegroundColor White
    } else {
        Write-Host "❌ Warning: Output file was not created" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    Write-Host "Generation complete! Your API documentation is ready for the AI agent." -ForegroundColor Green
    Write-Host ""
    Write-Host "Generated files:" -ForegroundColor Cyan
    Write-Host "   • tools/tools.json (combined)" -ForegroundColor White
    Write-Host "   • tools/endpoints/*-tools.json (individual entities)" -ForegroundColor White
    
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Restore original location
    Pop-Location
}
