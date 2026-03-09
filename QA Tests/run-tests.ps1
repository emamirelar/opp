<#
.SYNOPSIS
    Runs QA test suite with pre-flight database connectivity check.

.DESCRIPTION
    Verifies the Cloud SQL Proxy is running before executing dotnet test.
    Prevents wasting time on a full test run when the proxy isn't started.

.PARAMETER Filter
    dotnet test filter expression. Default excludes unit tests.

.PARAMETER Configuration
    Build configuration. Default: Release.

.PARAMETER NoBuild
    Skip building before running tests.

.PARAMETER InMemory
    Force in-memory database mode (no proxy needed).

.EXAMPLE
    .\run-tests.ps1
    .\run-tests.ps1 -InMemory
    .\run-tests.ps1 -Filter "FullyQualifiedName~OpportunityWhySection"
    .\run-tests.ps1 -NoBuild
#>
param(
    [string]$Filter = "FullyQualifiedName!~.Unit.&FullyQualifiedName!~UnitTest",
    [string]$Configuration = "Release",
    [switch]$NoBuild,
    [switch]$InMemory
)

$ErrorActionPreference = "Stop"
$ProjectPath = Join-Path $PSScriptRoot "C# Tests\UNOPS.PAO.Business.Tests\UNOPS.PAO.Business.Tests.csproj"

# ── Read connection config to determine host/port ──
function Get-DbHostPort {
    $settingsPath = Join-Path $PSScriptRoot "C# Tests\UNOPS.PAO.Business.Tests\appsettings.Testing.json"
    $host_ = "127.0.0.1"
    $port_ = 5432

    if (Test-Path $settingsPath) {
        try {
            $json = Get-Content $settingsPath -Raw | ConvertFrom-Json
            $connStr = $json.ConnectionStrings.DbContext
            if (-not $connStr) { $connStr = $json.ConnectionStrings.DefaultConnection }
            if ($connStr) {
                if ($connStr -match "Host=([^;]+)") { $host_ = $Matches[1] }
                if ($connStr -match "Port=(\d+)")   { $port_ = [int]$Matches[1] }
            }
        } catch { }
    }

    # Environment variable override
    $envConn = $env:TEST_DB_CONNECTION_STRING
    if ($envConn) {
        if ($envConn -match "Host=([^;]+)") { $host_ = $Matches[1] }
        if ($envConn -match "Port=(\d+)")   { $port_ = [int]$Matches[1] }
    }

    return @{ Host = $host_; Port = $port_ }
}

# ── Pre-flight: verify Cloud SQL Proxy is running ──
function Test-ProxyConnectivity {
    param([string]$Host_, [int]$Port_)

    Write-Host ""
    Write-Host "  Pre-flight check: Cloud SQL Proxy at ${Host_}:${Port_}..." -NoNewline

    try {
        $tcp = New-Object System.Net.Sockets.TcpClient
        $result = $tcp.BeginConnect($Host_, $Port_, $null, $null)
        $success = $result.AsyncWaitHandle.WaitOne(5000)
        if ($success -and $tcp.Connected) {
            $tcp.Close()
            Write-Host " OK" -ForegroundColor Green
            return $true
        }
        $tcp.Close()
    } catch { }

    Write-Host " FAILED" -ForegroundColor Red
    Write-Host ""
    Write-Host "  ╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "  ║              DATABASE PROXY NOT RUNNING                     ║" -ForegroundColor Red
    Write-Host "  ╠══════════════════════════════════════════════════════════════╣" -ForegroundColor Red
    Write-Host "  ║                                                            ║" -ForegroundColor Red
    Write-Host "  ║  Cannot connect to ${Host_}:${Port_}                       " -ForegroundColor Red -NoNewline
    Write-Host "║" -ForegroundColor Red
    Write-Host "  ║                                                            ║" -ForegroundColor Red
    Write-Host "  ║  Start the Cloud SQL Proxy first:                          ║" -ForegroundColor Red
    Write-Host "  ║    cloud-sql-proxy --port $Port_ <instance>                " -ForegroundColor Yellow -NoNewline
    Write-Host "║" -ForegroundColor Red
    Write-Host "  ║                                                            ║" -ForegroundColor Red
    Write-Host "  ║  Or run with -InMemory flag (no proxy needed):             ║" -ForegroundColor Red
    Write-Host "  ║    .\run-tests.ps1 -InMemory                               ║" -ForegroundColor Yellow
    Write-Host "  ║                                                            ║" -ForegroundColor Red
    Write-Host "  ╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    return $false
}

# ── Main ──
Write-Host ""
Write-Host "  ========================================" -ForegroundColor Cyan
Write-Host "   UNOPS Opportunity+ Test Runner" -ForegroundColor Cyan
Write-Host "  ========================================" -ForegroundColor Cyan

if ($InMemory) {
    Write-Host "  Mode: SQLite In-Memory (no proxy needed)" -ForegroundColor Yellow
    $env:USE_INMEMORY_DB = "true"
} else {
    $dbInfo = Get-DbHostPort
    if (-not (Test-ProxyConnectivity -Host_ $dbInfo.Host -Port_ $dbInfo.Port)) {
        exit 1
    }
    Write-Host "  Mode: PostgreSQL via Cloud SQL Proxy" -ForegroundColor Green
}

Write-Host "  Filter: $Filter" -ForegroundColor Gray
Write-Host ""

# Build args
$args_ = @(
    "test", $ProjectPath,
    "--configuration", $Configuration,
    "--filter", $Filter,
    "--logger", "console;verbosity=detailed"
)
if ($NoBuild) { $args_ += "--no-build" }

# Run tests
& dotnet @args_
$exitCode = $LASTEXITCODE

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "  All tests passed." -ForegroundColor Green
} else {
    Write-Host "  Some tests failed (exit code: $exitCode)." -ForegroundColor Red
}

exit $exitCode
