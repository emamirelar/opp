param(
    [Parameter(Mandatory=$true)]
    [string]$Environment
)

$envFile = "environments/$Environment.env"

if (-not (Test-Path $envFile)) {
    Write-Host "❌ Error: Environment file not found: $envFile" -ForegroundColor Red
    Write-Host ""
    Write-Host "Available environments:" -ForegroundColor Yellow
    Get-ChildItem "environments/*.env" | ForEach-Object {
        $name = $_.BaseName
        Write-Host "  $name" -ForegroundColor White
    }
    exit 1
}

Write-Host "============================================================================" -ForegroundColor Green
Write-Host "🌍 SETTING ENVIRONMENT: $Environment" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Green
Write-Host ""

Write-Host "📂 Loading configuration from: $envFile" -ForegroundColor Cyan
Write-Host ""

# Read and set environment variables from file
Get-Content $envFile | ForEach-Object {
    if ($_ -match "^([^=]+)=(.*)$") {
        $name = $matches[1].Trim()
        $value = $matches[2].Trim()
        
        [Environment]::SetEnvironmentVariable($name, $value, "Process")
        Write-Host "✅ Set $name=$value" -ForegroundColor Green
    }
}

$project = $env:GOOGLE_CLOUD_PROJECT
$location = $env:GOOGLE_CLOUD_LOCATION

Write-Host ""
Write-Host "============================================================================" -ForegroundColor Green
Write-Host "✅ Environment $Environment configured successfully!" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "🎯 Current configuration:" -ForegroundColor Cyan
Write-Host "   Project: $project" -ForegroundColor White
Write-Host "   Location: $location" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Now you can build your project:" -ForegroundColor Yellow
Write-Host "   dotnet build UNOPS.PAO.Presentation" -ForegroundColor White
Write-Host ""
Write-Host "💡 Or build with explicit configuration:" -ForegroundColor Yellow
Write-Host "   dotnet build UNOPS.PAO.Presentation -p:GoogleCloudProject=$project -p:GoogleCloudLocation=$location" -ForegroundColor White
Write-Host ""
Write-Host "============================================================================" -ForegroundColor Green 