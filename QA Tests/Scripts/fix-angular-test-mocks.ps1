#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automatically adds missing service mocks to Angular test files
.DESCRIPTION
    This script fixes Angular test failures by adding mock providers for:
    - TranslateService (200+ tests)
    - DialogService (80+ tests)
    - MarkdownService (20+ tests)
.NOTES
    Author: UNOPS Opportunity+ System Development Team
    Date: January 23, 2026
#>

param(
    [switch]$DryRun = $false
)

$scriptDir = $PSScriptRoot
$clientAppDir = Join-Path $scriptDir "UNOPS.PAO.ClientApp"
$specFiles = Get-ChildItem -Path $clientAppDir -Filter "*.spec.ts" -Recurse

$statsTotal = 0
$statsUpdated = 0
$statsSkipped = 0
$statsErrors = 0

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  Angular Test Mock Updater" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Mode: $(if ($DryRun) { 'DRY RUN (no changes)' } else { 'LIVE (will modify files)' })" -ForegroundColor $(if ($DryRun) { 'Yellow' } else { 'Green' })
Write-Host "Found $($specFiles.Count) test files" -ForegroundColor White
Write-Host ""

foreach ($file in $specFiles) {
    $statsTotal++
    $relativePath = $file.FullName.Replace("$clientAppDir\", "")
    
    try {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction Stop
        $originalContent = $content
        $modified = $false
        
        # Check if file imports TranslateModule but doesn't import test utilities
        $needsTranslateService = $content -match 'TranslateModule' -and 
                                  $content -notmatch 'createMockTranslateService'
        
        # Check if file uses DialogService but doesn't mock it
        $needsDialogService = $content -match 'DialogService' -and 
                              $content -notmatch 'createMockDialogService' -and
                              $content -match 'No provider for DialogService'
        
        # Check if file uses MarkdownService but doesn't mock it
        $needsMarkdownService = $content -match 'MarkdownService' -and 
                                $content -notmatch 'createMockMarkdownService'
        
        if ($needsTranslateService -or $needsDialogService -or $needsMarkdownService) {
            Write-Host "[PROCESSING] $relativePath" -ForegroundColor Yellow
            
            # Add import statement for test utilities if not present
            if ($content -notmatch "from '@shared/testing/test-utilities'") {
                $importPattern = "import \{([^\}]+)\} from '@ngx-translate/core';"
                if ($content -match $importPattern) {
                    $replacement = "$($matches[0])`nimport { createMockTranslateService, createMockDialogService, createMockMarkdownService } from '@shared/testing/test-utilities';"
                    $content = $content -replace [regex]::Escape($matches[0]), $replacement
                    $modified = $true
                    Write-Host "  ✓ Added test utilities import" -ForegroundColor Green
                }
            }
            
            # Replace existing TranslateService mock if needed
            if ($needsTranslateService) {
                # Pattern: mockTranslateService = jasmine.createSpyObj('TranslateService', [...]); 
                $pattern = "mockTranslateService\s*=\s*jasmine\.createSpyObj\('TranslateService',\s*\[([^\]]+)\]\);"
                if ($content -match $pattern) {
                    $replacement = "mockTranslateService = createMockTranslateService() as any;"
                    $content = $content -replace $pattern, $replacement
                    $modified = $true
                    Write-Host "  ✓ Replaced TranslateService mock creation" -ForegroundColor Green
                }
                
                # Remove individual method setup like mockTranslateService.instant.and.returnValue(...)
                $content = $content -replace "mockTranslateService\.instant\.and\.returnValue\([^\)]+\);?\s*\n", ""
                $content = $content -replace "mockTranslateService\.get\.and\.returnValue\([^\)]+\);?\s*\n", ""
            }
            
            # Add DialogService mock if needed
            if ($needsDialogService -and $content -match "DialogService") {
                # Check if providers array exists
                if ($content -match "providers:\s*\[") {
                    # Add to existing providers
                    $pattern = "(providers:\s*\[)"
                    $replacement = "`$1`n        { provide: DialogService, useValue: createMockDialogService() },"
                    $content = $content -replace $pattern, $replacement
                    $modified = $true
                    Write-Host "  ✓ Added DialogService provider" -ForegroundColor Green
                }
            }
            
            # Add MarkdownService mock if needed
            if ($needsMarkdownService -and $content -match "MarkdownService") {
                # Check if providers array exists
                if ($content -match "providers:\s*\[") {
                    # Add to existing providers
                    $pattern = "(providers:\s*\[)"
                    $replacement = "`$1`n        { provide: MarkdownService, useValue: createMockMarkdownService() },"
                    $content = $content -replace $pattern, $replacement
                    $modified = $true
                    Write-Host "  ✓ Added MarkdownService provider" -ForegroundColor Green
                }
            }
        }
        
        if ($modified) {
            if (-not $DryRun) {
                Set-Content -Path $file.FullName -Value $content -NoNewline -ErrorAction Stop
                Write-Host "[UPDATED] $relativePath" -ForegroundColor Green
            } else {
                Write-Host "[DRY RUN] Would update: $relativePath" -ForegroundColor Cyan
            }
            $statsUpdated++
        } else {
            $statsSkipped++
        }
        
    } catch {
        Write-Host "[ERROR] $relativePath - $($_.Exception.Message)" -ForegroundColor Red
        $statsErrors++
    }
}

Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  Summary" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Total files scanned:  $statsTotal" -ForegroundColor White
Write-Host "Files updated:        $statsUpdated" -ForegroundColor Green
Write-Host "Files skipped:        $statsSkipped" -ForegroundColor Yellow
Write-Host "Errors:               $statsErrors" -ForegroundColor $(if ($statsErrors -gt 0) { 'Red' } else { 'Green' })
Write-Host ""

if ($DryRun) {
    Write-Host "This was a DRY RUN. No files were modified." -ForegroundColor Yellow
    Write-Host "Run without -DryRun to apply changes." -ForegroundColor Yellow
} else {
    Write-Host "Updates complete!" -ForegroundColor Green
}
