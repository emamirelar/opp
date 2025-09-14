param(
    [Parameter(Mandatory=$true)]
    [string]$UIToolsDir,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "../UNOPS.PAO.ClientApp/src/app/common/tours"
)

Write-Host "============================================================================" -ForegroundColor Magenta
Write-Host "🎪 DRIVERJS TOUR GENERATOR - Create Interactive Tours from UI Metadata" -ForegroundColor Magenta
Write-Host "============================================================================" -ForegroundColor Magenta
Write-Host ""

Write-Host "📋 Configuration:" -ForegroundColor Cyan
Write-Host "   UI Tools Directory: $UIToolsDir" -ForegroundColor White
Write-Host "   Output Directory: $OutputDir" -ForegroundColor White
Write-Host ""

# Validate UI tools directory exists
if (-not (Test-Path $UIToolsDir)) {
    Write-Host "❌ Error: UI tools directory not found: $UIToolsDir" -ForegroundColor Red
    exit 1
}

Write-Host "============================================================================" -ForegroundColor Green
Write-Host "🎪 Generating DriverJS tours from UI metadata..." -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Green

try {
    # Run the tour generator
    $process = Start-Process -FilePath "python" -ArgumentList "tour_generator.py --ui-tools-dir `"$UIToolsDir`" --output-dir `"$OutputDir`"" -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -ne 0) {
        Write-Host ""
        Write-Host "❌ Tour generation failed with error code $($process.ExitCode)" -ForegroundColor Red
        exit $process.ExitCode
    }
    
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "✅ SUCCESS: DriverJS tours generated!" -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host "📁 Tours saved to: $OutputDir" -ForegroundColor White
    Write-Host "🎪 Import these tour files in your Angular components to enable guided tours" -ForegroundColor White
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Install driver.js in your Angular project: npm install driver.js" -ForegroundColor White
    Write-Host "2. Import tour configurations in your components" -ForegroundColor White
    Write-Host "3. Initialize tours with DriverJS" -ForegroundColor White
    Write-Host "============================================================================" -ForegroundColor Green
    
} catch {
    Write-Host ""
    Write-Host "❌ Error running tour generator: $_" -ForegroundColor Red
    exit 1
} 