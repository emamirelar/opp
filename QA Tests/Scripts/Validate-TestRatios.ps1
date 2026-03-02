<#
.SYNOPSIS
    Validates test suite compliance with the 3:1 ratio strategy.

.DESCRIPTION
    This script analyzes a test suite directory and validates:
    - Individual ratio checks: N>=3P, E>=3P, F>=3P, I>=3P (each must pass)
    - Minimum test counts per category
    - Fixed minimums for Security (50) and Concurrency (25)
    - Minimum total: 462

.PARAMETER Path
    Path to the test suite directory (relative to UNOPS.Pdj.Tests or absolute)

.PARAMETER Detailed
    Show detailed breakdown of tests per file

.EXAMPLE
    .\Validate-TestRatios.ps1 -Path "Forms_Module\ESOURCE2-931_CAPA"
    
.EXAMPLE
    .\Validate-TestRatios.ps1 -Path "Forms_Module\ESOURCE2-931_CAPA" -Detailed

.NOTES
    Based on comprehensive-test-strategy.mdc requirements:
    - Individual ratio checks: Negative>=3xPositive, Edge>=3xPositive, Functional>=3xPositive, Integration>=3xPositive
    - Negative: >= 50 AND >= 3 x Positive
    - Edge: >= 50 AND >= 3 x Positive
    - Functional: >= 50 AND >= 3 x Positive
    - Integration: >= 50 AND >= 3 x Positive
    - Security: >= 50 (FIXED)
    - Concurrency: >= 25 (FIXED)
    - Total: >= 462
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Path,
    
    [switch]$Detailed
)

# Resolve path
$TestsRoot = Split-Path -Parent $PSScriptRoot
$FullPath = if ([System.IO.Path]::IsPathRooted($Path)) { 
    $Path 
} else { 
    Join-Path $TestsRoot $Path 
}

if (-not (Test-Path $FullPath)) {
    Write-Host "ERROR: Path not found: $FullPath" -ForegroundColor Red
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  TEST RATIO VALIDATION REPORT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Path: $FullPath`n" -ForegroundColor Gray

# Count [Fact] and [Theory] attributes in each test file
$TestFiles = Get-ChildItem -Path $FullPath -Filter "*.cs" -Recurse | Where-Object { $_.Name -match "Tests\.cs$" }

$Categories = @{
    "Positive" = 0
    "Negative" = 0
    "Edge" = 0
    "Boundary" = 0
    "Security" = 0
    "Concurrency" = 0
    "Unit" = 0
    "Integration" = 0
    "Performance" = 0
    "Functional" = 0
    "Load" = 0
    "Stress" = 0
    "Limits" = 0
    "EndToEnd" = 0
    "AcceptanceCriteria" = 0
    "Other" = 0
}

$FileBreakdown = @()

foreach ($File in $TestFiles) {
    $Content = Get-Content $File.FullName -Raw
    $FactCount = ([regex]::Matches($Content, '\[Fact\]')).Count
    $TheoryCount = ([regex]::Matches($Content, '\[Theory\]')).Count
    $TestCount = $FactCount + $TheoryCount
    
    $FileName = $File.Name -replace '\.cs$', ''
    
    # Categorize based on filename
    $Category = switch -Regex ($FileName) {
        "^Positive" { "Positive" }
        "^Negative" { "Negative" }
        "^Edge" { "Edge" }
        "^Boundary" { "Boundary" }  # Treat as Edge
        "^Security" { "Security" }
        "^Concurrency" { "Concurrency" }
        "^Unit" { "Unit" }
        "^Integration" { "Integration" }
        "^Performance" { "Performance" }
        "^Functional" { "Functional" }
        "^Load" { "Load" }
        "^Stress" { "Stress" }
        "^Limits" { "Limits" }
        "^EndToEnd" { "EndToEnd" }
        "^AcceptanceCriteria" { "AcceptanceCriteria" }
        default { "Other" }
    }
    
    $Categories[$Category] += $TestCount
    
    $FileBreakdown += [PSCustomObject]@{
        File = $FileName
        Category = $Category
        Tests = $TestCount
    }
}

# Combine Boundary into Edge for ratio calculation
$EdgeTotal = $Categories["Edge"] + $Categories["Boundary"]

# Display file breakdown if detailed
if ($Detailed) {
    Write-Host "FILE BREAKDOWN:" -ForegroundColor Yellow
    Write-Host "-" * 50
    $FileBreakdown | Sort-Object Category, File | Format-Table -AutoSize
}

# Display category counts
Write-Host "CATEGORY COUNTS:" -ForegroundColor Yellow
Write-Host "-" * 50

$Positive = $Categories["Positive"]
$Negative = $Categories["Negative"]
$Security = $Categories["Security"]
$Concurrency = $Categories["Concurrency"]

Write-Host ("Positive Tests:      {0,4}" -f $Positive)
Write-Host ("Negative Tests:      {0,4}" -f $Negative)
Write-Host ("Edge/Boundary Tests: {0,4}" -f $EdgeTotal)
Write-Host ("Security Tests:      {0,4}" -f $Security)
Write-Host ("Concurrency Tests:   {0,4}" -f $Concurrency)

$Unit = $Categories["Unit"]
$Functional = $Categories["Functional"]
$Integration = $Categories["Integration"]
$Performance = $Categories["Performance"]
$Load = $Categories["Load"]

Write-Host ""
Write-Host "MANDATORY ADDITIONAL:" -ForegroundColor Yellow
Write-Host ("Unit Tests:          {0,4}" -f $Unit)
Write-Host ("Functional Tests:    {0,4}" -f $Functional)
Write-Host ("Integration Tests:   {0,4}" -f $Integration)
Write-Host ("Performance Tests:   {0,4}" -f $Performance)
Write-Host ("Load Tests:          {0,4}" -f $Load)

$OtherTotal = $Categories["Stress"] + $Categories["Limits"] + $Categories["EndToEnd"] + 
              $Categories["AcceptanceCriteria"] + $Categories["Other"]
Write-Host ("Other Tests:         {0,4}" -f $OtherTotal) -ForegroundColor Gray

$TotalTests = $Positive + $Negative + $EdgeTotal + $Security + $Concurrency + $Unit + $Functional + $Integration + $Performance + $Load + $OtherTotal
Write-Host ("-" * 30)
Write-Host ("TOTAL:               {0,4}" -f $TotalTests) -ForegroundColor Cyan

# Calculate requirements
Write-Host "`nREQUIREMENTS CHECK:" -ForegroundColor Yellow
Write-Host "-" * 50

$NegativeReq = [Math]::Max(50, [Math]::Ceiling(3 * $Positive))
$EdgeReq = [Math]::Max(50, [Math]::Ceiling(3 * $Positive))
$SecurityReq = 50
$ConcurrencyReq = 25
$RatioReq = 3 * $Positive

$AllPassed = $true

# Check Negative
$NegativePass = $Negative -ge $NegativeReq
$NegativeStatus = if ($NegativePass) { "[PASS]" } else { "[FAIL]" }
$NegativeColor = if ($NegativePass) { "Green" } else { "Red" }
Write-Host ("Negative:    {0,4} >= {1,4} (max(50, 3x{2}))  {3}" -f $Negative, $NegativeReq, $Positive, $NegativeStatus) -ForegroundColor $NegativeColor
$AllPassed = $AllPassed -and $NegativePass

# Check Edge
$EdgePass = $EdgeTotal -ge $EdgeReq
$EdgeStatus = if ($EdgePass) { "[PASS]" } else { "[FAIL]" }
$EdgeColor = if ($EdgePass) { "Green" } else { "Red" }
Write-Host ("Edge:        {0,4} >= {1,4} (max(50, 3x{2}))  {3}" -f $EdgeTotal, $EdgeReq, $Positive, $EdgeStatus) -ForegroundColor $EdgeColor
$AllPassed = $AllPassed -and $EdgePass

# Check Security (FIXED)
$SecurityPass = $Security -ge $SecurityReq
$SecurityStatus = if ($SecurityPass) { "[PASS]" } else { "[FAIL]" }
$SecurityColor = if ($SecurityPass) { "Green" } else { "Red" }
Write-Host ("Security:    {0,4} >= {1,4} (FIXED minimum)       {2}" -f $Security, $SecurityReq, $SecurityStatus) -ForegroundColor $SecurityColor
$AllPassed = $AllPassed -and $SecurityPass

# Check Concurrency (FIXED)
$ConcurrencyPass = $Concurrency -ge $ConcurrencyReq
$ConcurrencyStatus = if ($ConcurrencyPass) { "[PASS]" } else { "[FAIL]" }
$ConcurrencyColor = if ($ConcurrencyPass) { "Green" } else { "Red" }
Write-Host ("Concurrency: {0,4} >= {1,4} (FIXED minimum)       {2}" -f $Concurrency, $ConcurrencyReq, $ConcurrencyStatus) -ForegroundColor $ConcurrencyColor
$AllPassed = $AllPassed -and $ConcurrencyPass

# Check Mandatory Additional Tests
Write-Host "`nMANDATORY ADDITIONAL TESTS:" -ForegroundColor Yellow
Write-Host "-" * 50

# Fixed minimums for mandatory additional categories
$UnitReq = 21        # Validation(5) + Formatting(3) + Calculations(5) + Status(5) + Collections(3)
$FunctionalReq = [Math]::Max(50, [Math]::Ceiling(3 * $Positive))
$IntegrationReq = [Math]::Max(50, [Math]::Ceiling(3 * $Positive))
$PerformanceReq = 16 # SingleOps(2) + BulkOps(3) + Search(5) + Concurrent(3) + Memory(3)
$LoadReq = 10        # SustainedLoad(3) + SpikeTesting(2) + StressTesting(2) + Scalability(3)

$UnitPass = $Unit -ge $UnitReq
$UnitStatus = if ($UnitPass) { "[PASS]" } else { "[FAIL]" }
$UnitColor = if ($UnitPass) { "Green" } else { "Red" }
Write-Host ("Unit:        {0,4} >= {1,4} (FIXED minimum)         {2}" -f $Unit, $UnitReq, $UnitStatus) -ForegroundColor $UnitColor
$AllPassed = $AllPassed -and $UnitPass

$FunctionalPass = $Functional -ge $FunctionalReq
$FunctionalStatus = if ($FunctionalPass) { "[PASS]" } else { "[FAIL]" }
$FunctionalColor = if ($FunctionalPass) { "Green" } else { "Red" }
Write-Host ("Functional:  {0,4} >= {1,4} (max(50, 3x{2}))  {3}" -f $Functional, $FunctionalReq, $Positive, $FunctionalStatus) -ForegroundColor $FunctionalColor
$AllPassed = $AllPassed -and $FunctionalPass

$IntegrationPass = $Integration -ge $IntegrationReq
$IntegrationStatus = if ($IntegrationPass) { "[PASS]" } else { "[FAIL]" }
$IntegrationColor = if ($IntegrationPass) { "Green" } else { "Red" }
Write-Host ("Integration: {0,4} >= {1,4} (max(50, 3x{2}))  {3}" -f $Integration, $IntegrationReq, $Positive, $IntegrationStatus) -ForegroundColor $IntegrationColor
$AllPassed = $AllPassed -and $IntegrationPass

$PerformancePass = $Performance -ge $PerformanceReq
$PerformanceStatus = if ($PerformancePass) { "[PASS]" } else { "[FAIL]" }
$PerformanceColor = if ($PerformancePass) { "Green" } else { "Red" }
Write-Host ("Performance: {0,4} >= {1,4} (FIXED minimum)         {2}" -f $Performance, $PerformanceReq, $PerformanceStatus) -ForegroundColor $PerformanceColor
$AllPassed = $AllPassed -and $PerformancePass

$LoadPass = $Load -ge $LoadReq
$LoadStatus = if ($LoadPass) { "[PASS]" } else { "[FAIL]" }
$LoadColor = if ($LoadPass) { "Green" } else { "Red" }
Write-Host ("Load:        {0,4} >= {1,4} (FIXED minimum)         {2}" -f $Load, $LoadReq, $LoadStatus) -ForegroundColor $LoadColor
$AllPassed = $AllPassed -and $LoadPass

# Individual Ratio Checks (N>=3P, E>=3P, F>=3P, I>=3P)
Write-Host "`nRATIO COMPLIANCE (each must pass):" -ForegroundColor Yellow
Write-Host "-" * 50

$RatioReq = 3 * $Positive
$NegRatioPass = $Negative -ge $RatioReq
$EdgeRatioPass = $EdgeTotal -ge $RatioReq
$FuncRatioPass = $Functional -ge $RatioReq
$IntRatioPass = $Integration -ge $RatioReq

Write-Host ("N>=3P: {0,4} >= {1,4}  {2}" -f $Negative, $RatioReq, $(if ($NegRatioPass) { "[PASS]" } else { "[FAIL]" })) -ForegroundColor $(if ($NegRatioPass) { "Green" } else { "Red" })
Write-Host ("E>=3P: {0,4} >= {1,4}  {2}" -f $EdgeTotal, $RatioReq, $(if ($EdgeRatioPass) { "[PASS]" } else { "[FAIL]" })) -ForegroundColor $(if ($EdgeRatioPass) { "Green" } else { "Red" })
Write-Host ("F>=3P: {0,4} >= {1,4}  {2}" -f $Functional, $RatioReq, $(if ($FuncRatioPass) { "[PASS]" } else { "[FAIL]" })) -ForegroundColor $(if ($FuncRatioPass) { "Green" } else { "Red" })
Write-Host ("I>=3P: {0,4} >= {1,4}  {2}" -f $Integration, $RatioReq, $(if ($IntRatioPass) { "[PASS]" } else { "[FAIL]" })) -ForegroundColor $(if ($IntRatioPass) { "Green" } else { "Red" })

$RatioPass = $NegRatioPass -and $EdgeRatioPass -and $FuncRatioPass -and $IntRatioPass
$AllPassed = $AllPassed -and $RatioPass

# Minimum total check (462)
$MinTotalReq = 462
$TotalPass = $TotalTests -ge $MinTotalReq
Write-Host "`nTotal:  {0,4} >= {1,4} (minimum)  {2}" -f $TotalTests, $MinTotalReq, $(if ($TotalPass) { "[PASS]" } else { "[FAIL]" })) -ForegroundColor $(if ($TotalPass) { "Green" } else { "Red" })
$AllPassed = $AllPassed -and $TotalPass

# Final Result
Write-Host "`n========================================" -ForegroundColor Cyan
if ($AllPassed) {
    Write-Host "  VALIDATION RESULT: PASSED" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Cyan
    exit 0
} else {
    Write-Host "  VALIDATION RESULT: FAILED" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "`nAction Required: Add more tests to meet requirements" -ForegroundColor Yellow
    Write-Host "See: .cursor\rules\comprehensive-test-strategy.mdc`n" -ForegroundColor Gray
    exit 1
}
