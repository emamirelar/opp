# Ensures the UNOPS.Workflow submodule is initialized before building Integration Tests.
# Run this before dotnet build if you see "Workflow does not exist in namespace UNOPS" errors.
# Usage: .\scripts\ensure-workflow-submodule.ps1 (from repo root or QA Tests/Integration Tests)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "..\..\..")
Set-Location $RepoRoot

$WorkflowCsproj = "UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj"
if (-not (Test-Path $WorkflowCsproj)) {
    Write-Host "Initializing UNOPS.Workflow submodule (required for Integration Tests)..."
    git submodule update --init --recursive UNOPS.Workflow
    Write-Host "Submodule initialized successfully."
} else {
    Write-Host "UNOPS.Workflow submodule already present."
}
