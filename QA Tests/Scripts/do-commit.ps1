$ErrorActionPreference = "Stop"

# Configure git to not use pager
$env:GIT_PAGER = ""

Write-Host "Adding workflow file..."
git add ".github/workflows/qa-tests.yml"

Write-Host "Committing changes..."
git commit -m "fix(ci): Add conditional checks to handle test projects gracefully" -m "- Add checks to detect if test projects exist before running tests" -m "- Skip tests gracefully with informative messages if projects not found" -m "- Automatically enable tests once projects are merged" -m "- Update test summary to handle both scenarios (tests run vs skipped)" -m "- Resolves CI failures when test projects don't exist on target branch" -m "" -m "This is a self-adapting solution that:" -m "- Before merge: Skips tests, no errors, clear communication" -m "- After merge: Runs all tests automatically, no changes needed" -m "- Permanent fix: No maintenance required"

Write-Host "Pushing to remote..."
git push origin QA-Tests

Write-Host ""
Write-Host "✅ Successfully committed and pushed workflow fix!" -ForegroundColor Green
