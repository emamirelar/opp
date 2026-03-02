# =====================================================
# Create Test Data via API for Playwright Tests
# =====================================================
# This script creates test records using the actual API
# to ensure all foreign keys and validations are handled
# =====================================================

$baseUrl = "http://localhost:5159"
$userEmail = "test@playwright.local"

Write-Host "Creating test data via API..." -ForegroundColor Cyan
Write-Host "Backend URL: $baseUrl" -ForegroundColor Gray
Write-Host "Test User: $userEmail" -ForegroundColor Gray
Write-Host ""

# Set authentication cookies
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$cookie1 = New-Object System.Net.Cookie
$cookie1.Name = "dev-user-email"
$cookie1.Value = $userEmail
$cookie1.Domain = "localhost"
$session.Cookies.Add("http://localhost", $cookie1)

$cookie2 = New-Object System.Net.Cookie
$cookie2.Name = "DevIAPAuth"
$cookie2.Value = $userEmail
$cookie2.Domain = "localhost"
$session.Cookies.Add("http://localhost", $cookie2)

try {
    # =====================================================
    # 1. Create Test Partner
    # =====================================================
    Write-Host "Creating test partner..." -ForegroundColor Yellow
    
    $partnerData = @{
        name = "Test Partner Organization"
        partnerShortDescription = "Test partner for E2E testing"
        partnerLongDescription = "This is a test partner organization created for Playwright E2E automated testing with real backend integration."
        canCreateNewOpportunities = $true
        status = 1  # Active
    } | ConvertTo-Json -Depth 10

    $partnerResponse = Invoke-RestMethod -Uri "$baseUrl/api/partner" `
        -Method Post `
        -Body $partnerData `
        -ContentType "application/json" `
        -WebSession $session
    
    $partnerId = $partnerResponse.id
    Write-Host "✅ Partner created with ID: $partnerId" -ForegroundColor Green
    
    # =====================================================
    # 2. Create Test Contact
    # =====================================================
    Write-Host "Creating test contact..." -ForegroundColor Yellow
    
    $contactData = @{
        firstName = "John"
        lastName = "Doe"
        email = "john.doe@playwright.test"
        title = "Test Manager"
        partnerId = $partnerId
        status = 1  # Active
    } | ConvertTo-Json -Depth 10

    $contactResponse = Invoke-RestMethod -Uri "$baseUrl/api/contact" `
        -Method Post `
        -Body $contactData `
        -ContentType "application/json" `
        -WebSession $session
    
    $contactId = $contactResponse.id
    Write-Host "✅ Contact created with ID: $contactId" -ForegroundColor Green
    
    # =====================================================
    # 3. Create Test Interaction
    # =====================================================
    Write-Host "Creating test interaction..." -ForegroundColor Yellow
    
    $interactionData = @{
        subject = "Test Meeting"
        type = 1  # Meeting
        date = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
        description = "This is a test interaction for Playwright E2E tests"
        contactId = $contactId
        partnerIds = @($partnerId)
        status = 1  # Active
    } | ConvertTo-Json -Depth 10

    $interactionResponse = Invoke-RestMethod -Uri "$baseUrl/api/interaction" `
        -Method Post `
        -Body $interactionData `
        -ContentType "application/json" `
        -WebSession $session
    
    $interactionId = $interactionResponse.id
    Write-Host "✅ Interaction created with ID: $interactionId" -ForegroundColor Green
    
    # =====================================================
    # 4. Create Test Opportunity
    # =====================================================
    Write-Host "Creating test opportunity..." -ForegroundColor Yellow
    
    $opportunityData = @{
        name = "Test Opportunity Project"
        description = "This is a test opportunity for Playwright E2E tests with real backend integration. It tests the opportunity detail page functionality."
        initiativeBudgetUSD = 100000.00
        stage = "Draft"
        status = 1  # Active
    } | ConvertTo-Json -Depth 10

    $opportunityResponse = Invoke-RestMethod -Uri "$baseUrl/api/opportunity" `
        -Method Post `
        -Body $opportunityData `
        -ContentType "application/json" `
        -WebSession $session
    
    $opportunityId = $opportunityResponse.id
    Write-Host "✅ Opportunity created with ID: $opportunityId" -ForegroundColor Green
    
    # =====================================================
    # Summary
    # =====================================================
    Write-Host ""
    Write-Host "✅ Test data creation complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Created Records:" -ForegroundColor Cyan
    Write-Host "  - Partner ID: $partnerId" -ForegroundColor White
    Write-Host "  - Contact ID: $contactId" -ForegroundColor White
    Write-Host "  - Interaction ID: $interactionId" -ForegroundColor White
    Write-Host "  - Opportunity ID: $opportunityId" -ForegroundColor White
    Write-Host ""
    Write-Host "You can now run Playwright tests:" -ForegroundColor Cyan
    Write-Host "  npx playwright test partner-item-basic.spec.ts --project=chromium" -ForegroundColor Gray
    
} catch {
    Write-Host "❌ Error creating test data:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure the backend is running at: $baseUrl" -ForegroundColor Yellow
    exit 1
}
