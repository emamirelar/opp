#!/bin/bash
# Ensures the UNOPS.Workflow submodule is initialized before building Integration Tests.
# Run this before dotnet build if you see "Workflow does not exist in namespace UNOPS" errors.
# Usage: ./scripts/ensure-workflow-submodule.sh (from repo root or QA Tests/Integration Tests)

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
cd "$REPO_ROOT"

if [ ! -f "UNOPS.Workflow/UNOPS.Workflow.Business/UNOPS.Workflow.Business.csproj" ]; then
  echo "Initializing UNOPS.Workflow submodule (required for Integration Tests)..."
  git submodule update --init --recursive UNOPS.Workflow
  echo "Submodule initialized successfully."
else
  echo "UNOPS.Workflow submodule already present."
fi
