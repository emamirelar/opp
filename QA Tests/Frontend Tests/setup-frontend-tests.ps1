# UNOPS Opportunity+ Frontend Test Setup Script
# This script copies frontend spec files to the appropriate Angular component/service folders
#
# Usage: .\setup-frontend-tests.ps1
# Run from: QA Tests/Frontend Tests/ folder OR project root

param(
    [switch]$DryRun = $false,
    [switch]$Force = $false,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"

# Determine project root
$scriptDir = $PSScriptRoot
if (-not $scriptDir) {
    $scriptDir = Get-Location
}

# Navigate to project root
$projectRoot = (Get-Item $scriptDir).Parent.Parent.FullName
if ($scriptDir -match "opportunityplus$") {
    $projectRoot = $scriptDir
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Frontend Test Setup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Project Root: $projectRoot" -ForegroundColor Gray
Write-Host "Dry Run: $DryRun" -ForegroundColor Gray
Write-Host ""

# Define source and destination mappings
$testMappings = @(
    @{
        Source = "QA Tests\Frontend Tests\components\base-entity-view.component.spec.ts"
        Destination = "UNOPS.PAO.ClientApp\src\app\shared\components\base-entity-view"
        ComponentName = "BaseEntityViewComponent"
    },
    @{
        Source = "QA Tests\Frontend Tests\components\related-info-panel.component.spec.ts"
        Destination = "UNOPS.PAO.ClientApp\src\app\shared\components\related-info-panel"
        ComponentName = "RelatedInfoPanelComponent"
    },
    @{
        Source = "QA Tests\Frontend Tests\components\enhanced-entity-layout.component.spec.ts"
        Destination = "UNOPS.PAO.ClientApp\src\app\shared\components\enhanced-entity-layout"
        ComponentName = "EnhancedEntityLayoutComponent"
    },
    @{
        Source = "QA Tests\Frontend Tests\components\partner-view-enhanced.component.spec.ts"
        Destination = "UNOPS.PAO.ClientApp\src\app\features\partnerships\partners\components\partner\view"
        ComponentName = "PartnerViewEnhanced"
    },
    @{
        Source = "QA Tests\Frontend Tests\components\contact-view-enhanced.component.spec.ts"
        Destination = "UNOPS.PAO.ClientApp\src\app\features\partnerships\contacts\components\contact\view"
        ComponentName = "ContactViewEnhanced"
    },
    @{
        Source = "QA Tests\Frontend Tests\services\panel-layout.service.spec.ts"
        Destination = "UNOPS.PAO.ClientApp\src\app\shared\services"
        ComponentName = "PanelLayoutService"
    }
)

$copiedCount = 0
$skippedCount = 0
$createdFolders = 0
$errors = @()

foreach ($mapping in $testMappings) {
    $sourcePath = Join-Path $projectRoot $mapping.Source
    $destDir = Join-Path $projectRoot $mapping.Destination
    $destFile = Join-Path $destDir (Split-Path $mapping.Source -Leaf)
    
    Write-Host "Processing: $($mapping.ComponentName)" -ForegroundColor White
    
    # Check if source file exists
    if (-not (Test-Path $sourcePath)) {
        Write-Host "  [SKIP] Source file not found: $sourcePath" -ForegroundColor Yellow
        $skippedCount++
        continue
    }
    
    # Create destination folder if it doesn't exist
    if (-not (Test-Path $destDir)) {
        if ($DryRun) {
            Write-Host "  [DRY RUN] Would create folder: $destDir" -ForegroundColor Magenta
        } else {
            try {
                New-Item -ItemType Directory -Path $destDir -Force | Out-Null
                Write-Host "  [CREATED] Folder: $($mapping.Destination)" -ForegroundColor Green
                $createdFolders++
            } catch {
                Write-Host "  [ERROR] Could not create folder: $destDir" -ForegroundColor Red
                $errors += "Failed to create: $destDir - $_"
                continue
            }
        }
    }
    
    # Check if destination file already exists
    if ((Test-Path $destFile) -and -not $Force) {
        Write-Host "  [SKIP] File already exists (use -Force to overwrite)" -ForegroundColor Yellow
        $skippedCount++
        continue
    }
    
    # Copy the file
    if ($DryRun) {
        Write-Host "  [DRY RUN] Would copy to: $($mapping.Destination)" -ForegroundColor Magenta
        $copiedCount++
    } else {
        try {
            Copy-Item -Path $sourcePath -Destination $destFile -Force
            Write-Host "  [COPIED] -> $($mapping.Destination)" -ForegroundColor Green
            $copiedCount++
        } catch {
            Write-Host "  [ERROR] Copy failed: $_" -ForegroundColor Red
            $errors += "Failed to copy $($mapping.ComponentName): $_"
        }
    }
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Copied:         $copiedCount files" -ForegroundColor Green
Write-Host "  Skipped:        $skippedCount files" -ForegroundColor Yellow
Write-Host "  Folders Created: $createdFolders" -ForegroundColor Blue

if ($errors.Count -gt 0) {
    Write-Host "  Errors:         $($errors.Count)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Errors:" -ForegroundColor Red
    foreach ($err in $errors) {
        Write-Host "  - $err" -ForegroundColor Red
    }
}

Write-Host ""

if ($DryRun) {
    Write-Host "This was a DRY RUN. No files were actually copied." -ForegroundColor Magenta
    Write-Host "Run without -DryRun to copy files." -ForegroundColor Magenta
    Write-Host ""
}

# Next steps
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Navigate to Angular app: cd UNOPS.PAO.ClientApp" -ForegroundColor White
Write-Host "  2. Install dependencies:    npm install" -ForegroundColor White
Write-Host "  3. Run tests:               ng test" -ForegroundColor White
Write-Host ""

