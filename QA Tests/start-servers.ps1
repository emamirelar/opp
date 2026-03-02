<#
.SYNOPSIS
    Starts the backend API, frontend dev server, and refreshes the gcloud
    IAM token for Cloud SQL proxy authentication.

.DESCRIPTION
    This script orchestrates the full stack needed for Playwright real-API tests:
      1. Refreshes the gcloud access token (for Cloud SQL proxy IAM auth)
      2. Verifies Cloud SQL proxy is running on port 5432
      3. Starts the .NET backend on http://localhost:5159 (if not already running)
      4. Starts the Angular frontend on http://localhost:4200 (if not already running)
      5. Waits for both services to become healthy

    Prerequisites:
      - Cloud SQL proxy must be running (port 5432)
      - gcloud CLI must be installed and authenticated
      - .NET SDK and Node.js must be installed

.EXAMPLE
    cd "QA Tests"
    .\start-servers.ps1

    # Then in another terminal:
    npx playwright test --project=real-api
#>

$ErrorActionPreference = "Continue"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$ServerProject = Join-Path $RepoRoot "UNOPS.PAO.Server"
$ClientApp = Join-Path $RepoRoot "UNOPS.PAO.ClientApp"

$BackendUrl = "http://localhost:5159"
$FrontendUrl = "http://localhost:4200"
$ProxyPort = 5432

function Write-Status($icon, $message) {
    Write-Host "  $icon $message"
}

function Test-Port($port) {
    try {
        $tcp = New-Object System.Net.Sockets.TcpClient
        $tcp.Connect("127.0.0.1", $port)
        $tcp.Close()
        return $true
    } catch {
        return $false
    }
}

function Test-HttpReady($url, $label, $maxAttempts = 20, $delaySeconds = 3) {
    for ($i = 1; $i -le $maxAttempts; $i++) {
        try {
            $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
            Write-Status "✓" "$label is ready ($url → HTTP $($response.StatusCode))"
            return $true
        } catch {
            if ($i -lt $maxAttempts) {
                Write-Host "    Waiting for $label... (attempt $i/$maxAttempts)" -ForegroundColor DarkGray
                Start-Sleep -Seconds $delaySeconds
            }
        }
    }
    Write-Status "✗" "$label did not become ready after $maxAttempts attempts"
    return $false
}

# ======================================================================
Write-Host ""
Write-Host "=== UNOPS Opportunity+ Server Startup ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Refresh gcloud token
Write-Host "[1/5] Refreshing gcloud IAM token..." -ForegroundColor Yellow
try {
    $token = gcloud auth print-access-token 2>$null
    if ($token -and $token.Length -gt 50) {
        $tokenFile = Join-Path $env:TEMP "gcloud_token.txt"
        $token | Set-Content -Path $tokenFile -NoNewline
        Write-Status "✓" "Token refreshed ($($token.Length) chars)"
    } else {
        Write-Status "⚠" "Token too short or empty — tests may fail"
    }
} catch {
    Write-Status "⚠" "gcloud not available — token not refreshed"
}

# Step 2: Verify Cloud SQL proxy
Write-Host "[2/5] Checking Cloud SQL proxy (port $ProxyPort)..." -ForegroundColor Yellow
if (Test-Port $ProxyPort) {
    Write-Status "✓" "Cloud SQL proxy is listening on port $ProxyPort"
} else {
    Write-Status "✗" "Cloud SQL proxy is NOT running on port $ProxyPort"
    Write-Host "    Start it with: cloud_sql_proxy --private-ip <instance>" -ForegroundColor Red
    Write-Host "    Cannot proceed without the proxy." -ForegroundColor Red
    exit 1
}

# Step 3: Start backend via TestApiServer (bypasses Secret Manager)
Write-Host "[3/5] Checking backend API ($BackendUrl)..." -ForegroundColor Yellow
if (Test-Port 5159) {
    Write-Status "✓" "Backend already running on port 5159"
} else {
    $testApiServerExe = Join-Path $PSScriptRoot "TestApiServer\bin\Debug\net9.0\TestApiServer.exe"
    if (-not (Test-Path $testApiServerExe)) {
        Write-Status "→" "Building TestApiServer..."
        dotnet build (Join-Path $PSScriptRoot "TestApiServer\TestApiServer.csproj") --verbosity quiet 2>&1 | Out-Null
    }
    Write-Status "→" "Starting TestApiServer (WebApplicationFactory reverse proxy)..."
    $backendJob = Start-Process -FilePath $testApiServerExe `
        -WorkingDirectory $RepoRoot `
        -PassThru `
        -WindowStyle Normal
    Write-Status "→" "Backend starting (PID: $($backendJob.Id))..."
}

# Step 4: Start frontend
Write-Host "[4/5] Checking frontend ($FrontendUrl)..." -ForegroundColor Yellow
if (Test-Port 4200) {
    Write-Status "✓" "Frontend already running on port 4200"
} else {
    Write-Status "→" "Starting frontend (ng serve --port 4200)..."
    $frontendJob = Start-Process -FilePath "cmd" `
        -ArgumentList "/c", "cd /d `"$ClientApp`" && npx ng serve --port 4200" `
        -PassThru `
        -WindowStyle Normal
    Write-Status "→" "Frontend starting (PID: $($frontendJob.Id))..."
}

# Step 5: Wait for services
Write-Host "[5/5] Waiting for services to become ready..." -ForegroundColor Yellow

$backendReady = Test-HttpReady $BackendUrl "Backend API" 40 3
$frontendReady = Test-HttpReady $FrontendUrl "Frontend" 40 3

Write-Host ""
Write-Host "=== Startup Summary ===" -ForegroundColor Cyan
Write-Status $(if (Test-Port $ProxyPort) { "✓" } else { "✗" }) "Cloud SQL proxy (port $ProxyPort)"
Write-Status $(if ($backendReady) { "✓" } else { "✗" }) "Backend API ($BackendUrl)"
Write-Status $(if ($frontendReady) { "✓" } else { "✗" }) "Frontend ($FrontendUrl)"

if ($backendReady -and $frontendReady) {
    Write-Host ""
    Write-Host "  All services ready! Run tests with:" -ForegroundColor Green
    Write-Host "    cd `"QA Tests`"" -ForegroundColor White
    Write-Host "    npx playwright test --project=real-api" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "  Some services failed to start." -ForegroundColor Red
    if (-not $backendReady) {
        Write-Host "  Backend: try starting from Visual Studio or check Secret Manager permissions." -ForegroundColor Red
    }
    if (-not $frontendReady) {
        Write-Host "  Frontend: try running 'cd UNOPS.PAO.ClientApp && ng serve --port 4200' manually." -ForegroundColor Red
    }
    exit 1
}
