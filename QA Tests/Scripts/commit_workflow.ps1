# Commit the workflow changes
git add ".github/workflows/qa-tests.yml"
git commit -m "fix(ci): Add conditional checks to handle test projects gracefully

- Add checks to detect if test projects exist before running tests
- Skip tests gracefully with informative messages if projects not found
- Automatically enable tests once projects are merged
- Update test summary to handle both scenarios (tests run vs skipped)
- Resolves CI failures when test projects don't exist on target branch

This is a self-adapting solution that:
- Before merge: Skips tests, no errors, clear communication
- After merge: Runs all tests automatically, no changes needed
- Permanent fix: No maintenance required"
Write-Host "Changes committed successfully!"
