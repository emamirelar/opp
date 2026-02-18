<#
.SYNOPSIS
    Validates ALL test suites in the project for ratio compliance.

.DESCRIPTION
    This script scans all test suite directories and validates each against
    the comprehensive-test-strategy.mdc requirements:
    - Individual ratio checks: N>=3P, E>=3P, F>=3P, I>=3P (each must pass)
    - Minimum test counts per category
    - Fixed minimums for Security (50) and Concurrency (25)
    - Minimum total: 462
    
    Designed for CI/CD integration to catch compliance issues before merge.

.PARAMETER Module
    Optional. Specific module to validate (e.g., "Forms_Module", "DRiVE_Module").
    If not specified, validates all modules.

.PARAMETER FailOnWarning
    If specified, exit with error code 1 if any suite fails validation.

.PARAMETER OutputFormat
    Output format: "Console" (default), "Markdown", or "JSON"

.EXAMPLE
    .\Validate-AllTestSuites.ps1
    
.EXAMPLE
    .\Validate-AllTestSuites.ps1 -Module "Forms_Module" -FailOnWarning

.EXAMPLE
    .\Validate-AllTestSuites.ps1 -OutputFormat "Markdown" > compliance-report.md

.NOTES
    Based on comprehensive-test-strategy.mdc requirements.
    Run after test generation to ensure compliance.
#>

param(
    [string]$Module = "",
    [switch]$FailOnWarning,
    [ValidateSet("Console", "Markdown", "JSON")]
    [string]$OutputFormat = "Console"
)

$ErrorActionPreference = "Continue"

# Get tests root directory
$TestsRoot = Split-Path -Parent $PSScriptRoot

# Find all test suite directories (those containing *Tests.cs files)
$SearchPath = if ($Module) { Join-Path $TestsRoot $Module } else { $TestsRoot }

# Exclude certain directories
$ExcludeDirs = @("TestBase", "TestTemplates", "Scripts", "Documentation", "playwright-helpers", "bin", "obj")

$AllSuites = Get-ChildItem -Path $SearchPath -Directory -Recurse | Where-Object {
    $dir = $_
    $hasTestFiles = Get-ChildItem -Path $dir.FullName -Filter "*Tests.cs" -File -ErrorAction SilentlyContinue | Select-Object -First 1
    $notExcluded = $ExcludeDirs | ForEach-Object { $dir.FullName -notlike "*\$_*" } | Where-Object { $_ -eq $false } | Measure-Object | Select-Object -ExpandProperty Count
    $hasTestFiles -and ($notExcluded -eq 0)
} | Sort-Object FullName

$Results = @()
$PassCount = 0
$FailCount = 0
$WarnCount = 0

foreach ($Suite in $AllSuites) {
    $RelativePath = $Suite.FullName.Replace($TestsRoot, "").TrimStart("\")
    
    # Count tests per category
    $TestFiles = Get-ChildItem -Path $Suite.FullName -Filter "*.cs" -File | Where-Object { $_.Name -match "Tests\.cs$" }
    
    $Categories = @{
        "Positive" = 0
        "Negative" = 0
        "Boundary" = 0
        "Security" = 0
        "Concurrency" = 0
        "Unit" = 0
        "Functional" = 0
        "Integration" = 0
        "Performance" = 0
        "Load" = 0
    }
    
    foreach ($File in $TestFiles) {
        $Content = Get-Content $File.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $Content) { continue }
        
        $FactCount = ([regex]::Matches($Content, '\[Fact\]')).Count
        $TheoryCount = ([regex]::Matches($Content, '\[Theory\]')).Count
        $TestCount = $FactCount + $TheoryCount
        
        $FileName = $File.Name -replace '\.cs$', ''
        
        switch -Regex ($FileName) {
            "^Positive" { $Categories["Positive"] += $TestCount }
            "^Negative" { $Categories["Negative"] += $TestCount }
            "^Boundary|^Edge" { $Categories["Boundary"] += $TestCount }
            "^Security" { $Categories["Security"] += $TestCount }
            "^Concurrency" { $Categories["Concurrency"] += $TestCount }
            "^Unit" { $Categories["Unit"] += $TestCount }
            "^Functional" { $Categories["Functional"] += $TestCount }
            "^Integration" { $Categories["Integration"] += $TestCount }
            "^Performance" { $Categories["Performance"] += $TestCount }
            "^Load" { $Categories["Load"] += $TestCount }
        }
    }
    
    $P = $Categories["Positive"]
    $N = $Categories["Negative"]
    $E = $Categories["Boundary"]
    $S = $Categories["Security"]
    $C = $Categories["Concurrency"]
    $U = $Categories["Unit"]
    $F = $Categories["Functional"]
    $I = $Categories["Integration"]
    $Perf = $Categories["Performance"]
    $L = $Categories["Load"]
    
    # Calculate requirements
    $NegReq = [Math]::Max(50, [Math]::Ceiling(3 * $P))
    $EdgeReq = [Math]::Max(50, [Math]::Ceiling(3 * $P))
    $FuncReq = [Math]::Max(50, [Math]::Ceiling(3 * $P))
    $IntReq = [Math]::Max(50, [Math]::Ceiling(3 * $P))
    $SecReq = 50
    $ConReq = 25
    $UnitReq = 21
    $PerfReq = 16
    $LoadReq = 10
    $RatioReq = 3 * $P
    $MinTotalReq = 462
    
    # Check compliance (all 10 categories + individual ratio checks + total)
    $TotalTests = $P + $N + $E + $S + $C + $U + $F + $I + $Perf + $L
    $Checks = @{
        "Positive" = ($P -ge 30)
        "Negative" = ($N -ge $NegReq)
        "Boundary" = ($E -ge $EdgeReq)
        "Functional" = ($F -ge $FuncReq)
        "Integration" = ($I -ge $IntReq)
        "Security" = ($S -ge $SecReq)
        "Concurrency" = ($C -ge $ConReq)
        "Unit" = ($U -ge $UnitReq)
        "Performance" = ($Perf -ge $PerfReq)
        "Load" = ($L -ge $LoadReq)
        "N>=3P" = ($N -ge $RatioReq)
        "E>=3P" = ($E -ge $RatioReq)
        "F>=3P" = ($F -ge $RatioReq)
        "I>=3P" = ($I -ge $RatioReq)
        "Total" = ($TotalTests -ge $MinTotalReq)
    }
    
    $AllPass = ($Checks.Values | Where-Object { $_ -eq $false } | Measure-Object).Count -eq 0
    $Status = if ($AllPass) { "PASS" } elseif ($P -eq 0) { "SKIP" } else { "FAIL" }
    
    if ($Status -eq "PASS") { $PassCount++ }
    elseif ($Status -eq "FAIL") { $FailCount++ }
    else { $WarnCount++ }
    
    $Results += [PSCustomObject]@{
        Suite = $RelativePath
        Positive = $P
        Negative = $N
        Boundary = $E
        Security = $S
        Concurrency = $C
        Unit = $U
        Functional = $F
        Integration = $I
        Performance = $Perf
        Load = $L
        Total = $TotalTests
        RatioReq = $RatioReq
        MinTotalReq = $MinTotalReq
        Status = $Status
        Issues = ($Checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object { $_.Key }) -join ", "
    }
}

# Output results
switch ($OutputFormat) {
    "Console" {
        Write-Host "`n" + "=" * 80 -ForegroundColor Cyan
        Write-Host "  TEST SUITE COMPLIANCE REPORT" -ForegroundColor Cyan
        Write-Host "=" * 80 -ForegroundColor Cyan
        Write-Host "Based on comprehensive-test-strategy.mdc`n" -ForegroundColor Gray
        
        foreach ($R in $Results) {
            $Color = switch ($R.Status) { "PASS" { "Green" } "FAIL" { "Red" } default { "Yellow" } }
            Write-Host ("[$($R.Status)] $($R.Suite)") -ForegroundColor $Color
            if ($R.Status -eq "FAIL") {
                Write-Host ("       Issues: $($R.Issues)") -ForegroundColor Gray
                Write-Host ("       Core: P=$($R.Positive) N=$($R.Negative) B=$($R.Boundary) S=$($R.Security) C=$($R.Concurrency)") -ForegroundColor Gray
                Write-Host ("       Add'l: U=$($R.Unit) F=$($R.Functional) I=$($R.Integration) Perf=$($R.Performance) L=$($R.Load)") -ForegroundColor Gray
                Write-Host ("       Ratio: N/E/F/I each >= $($R.RatioReq), Total: $($R.Total)/$($R.MinTotalReq)") -ForegroundColor Gray
            }
        }
        
        Write-Host "`n" + "-" * 80
        Write-Host "SUMMARY: $PassCount PASS | $FailCount FAIL | $WarnCount SKIP" -ForegroundColor $(if ($FailCount -gt 0) { "Red" } else { "Green" })
        Write-Host "-" * 80 + "`n"
    }
    
    "Markdown" {
        Write-Output "# Test Suite Compliance Report"
        Write-Output ""
        Write-Output "## Legend"
        Write-Output "- **P**=Positive, **N**=Negative, **B**=Boundary, **S**=Security, **C**=Concurrency"
        Write-Output "- **U**=Unit, **F**=Functional, **I**=Integration, **Pf**=Performance, **L**=Load"
        Write-Output ""
        Write-Output "| Suite | Status | P | N | B | S | C | U | F | I | Pf | L | Issues |"
        Write-Output "|-------|--------|---|---|---|---|---|---|---|---|----|----|--------|"
        foreach ($R in $Results) {
            $StatusIcon = switch ($R.Status) { "PASS" { "✅" } "FAIL" { "❌" } default { "⚠️" } }
            Write-Output "| $($R.Suite) | $StatusIcon | $($R.Positive) | $($R.Negative) | $($R.Boundary) | $($R.Security) | $($R.Concurrency) | $($R.Unit) | $($R.Functional) | $($R.Integration) | $($R.Performance) | $($R.Load) | $($R.Issues) |"
        }
        Write-Output ""
        Write-Output "**Summary:** $PassCount PASS | $FailCount FAIL | $WarnCount SKIP"
        Write-Output ""
        Write-Output "## Minimum Requirements"
        Write-Output "| Category | Minimum |"
        Write-Output "|----------|---------|"
        Write-Output "| Positive | ≥30 |"
        Write-Output "| Negative | ≥50 AND ≥3×P |"
        Write-Output "| Boundary | ≥50 AND ≥3×P |"
        Write-Output "| Functional | ≥50 AND ≥3×P |"
        Write-Output "| Integration | ≥50 AND ≥3×P |"
        Write-Output "| Security | ≥50 |"
        Write-Output "| Concurrency | ≥25 |"
        Write-Output "| Unit | ≥21 |"
        Write-Output "| Performance | ≥16 |"
        Write-Output "| Load | ≥10 |"
        Write-Output "| Total | ≥462 |"
    }
    
    "JSON" {
        $Results | ConvertTo-Json -Depth 3
    }
}

# Exit with appropriate code
if ($FailOnWarning -and $FailCount -gt 0) {
    exit 1
}
exit 0
