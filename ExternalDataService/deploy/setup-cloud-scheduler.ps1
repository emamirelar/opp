# Setup Cloud Scheduler Jobs for UNOPS Opportunity+ (PAO) - External Data Service
# PowerShell version for Windows users

param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectId,
    
    [Parameter(Mandatory=$true)]
    [string]$Environment,
    
    [Parameter(Mandatory=$true)]
    [string]$Region,
    
    [Parameter(Mandatory=$false)]
    [string]$OAuthClientId,
    
    [Parameter(Mandatory=$false)]
    [string]$ConfigFilePath,
    
    [Parameter(Mandatory=$false)]
    [string]$TimeZone,
    
    [Parameter(Mandatory=$false)]
    [string]$SchedulerRegion
)

# Function to print colored output
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-ErrorMsg {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-WarningMsg {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

# Map environment name to appsettings file name
function Get-AppSettingsEnvName {
    param([string]$env)
    switch ($env) {
        "dev" { return "Development" }
        "test" { return "Test" }
        "qa" { return "QA" }
        "prod" { return "Production" }
        default { return $env }
    }
}

# Try to auto-discover OAuth Client ID from config file
if (-not $OAuthClientId) {
    # If config file path not provided, try default location
    if (-not $ConfigFilePath) {
        $appSettingsEnv = Get-AppSettingsEnvName -env $Environment
        $ConfigFilePath = "./ExternalDataService/appsettings.$appSettingsEnv.json"
    }
    
    if (Test-Path $ConfigFilePath) {
        Write-Info "Reading OAuth Client ID from config file: $ConfigFilePath"
        try {
            $config = Get-Content $ConfigFilePath -Raw | ConvertFrom-Json
            $OAuthClientId = $config.CloudScheduler.OAuthClientId
            
            if (-not $OAuthClientId) {
                Write-ErrorMsg "Could not find CloudScheduler.OAuthClientId in config file"
                Write-ErrorMsg "Please provide OAuth Client ID as parameter or add it to the config file"
                exit 1
            }
            Write-Info "Found OAuth Client ID: $OAuthClientId"
            
            # Also read TimeZone and Region if not provided
            if (-not $TimeZone -and $config.CloudScheduler.TimeZone) {
                $TimeZone = $config.CloudScheduler.TimeZone
            }
            if (-not $SchedulerRegion -and $config.CloudScheduler.Region) {
                $SchedulerRegion = $config.CloudScheduler.Region
            }
        }
        catch {
            Write-ErrorMsg "Failed to parse config file: $_"
            exit 1
        }
    }
    else {
        Write-ErrorMsg "Config file not found: $ConfigFilePath"
        Write-ErrorMsg "Please provide OAuth Client ID as parameter or specify config file path"
        exit 1
    }
}

# Set default timezone if not provided
if (-not $TimeZone) {
    $TimeZone = "Europe/Copenhagen"
}

# Auto-construct Service URL based on environment
# Pattern (TO BE PROVIDED BY INFRA - using placeholder pattern):
#   prod:        https://eds.opportunityplus.unops.org
#   qa:          https://eds.opportunityplus-qa.unops.org
#   dev/test:    https://eds.opportunityplus-{env}.unops.org
if ($Environment -eq "prod") {
    $ServiceUrl = "https://eds.opportunityplus.unops.org"
} elseif ($Environment -eq "qa") {
    $ServiceUrl = "https://eds.opportunityplus-qa.unops.org"
} else {
    $ServiceUrl = "https://eds.opportunityplus-$Environment.unops.org"
}

# Map Cloud Run region to nearest Scheduler region if not specified
# Note: Cloud Scheduler doesn't support all regions (e.g., europe-west4)
if (-not $SchedulerRegion) {
    $SchedulerRegion = switch ($Region) {
        "europe-west4" { "europe-west3" }
        "europe-west2" { "europe-west2" }
        "europe-west1" { "europe-west1" }
        "us-central1"  { "us-central1" }
        "us-east1"     { "us-east1" }
        default { $Region }
    }
}

Write-Info "Setting up Cloud Scheduler for UNOPS Opportunity+ (PAO)"
Write-Info "Project ID: $ProjectId"
Write-Info "Environment: $Environment"
Write-Info "Service URL (Load Balancer): $ServiceUrl"
Write-Info "OAuth Client ID: $OAuthClientId"
Write-Info "Cloud Run Region: $Region"
Write-Info "Scheduler Region: $SchedulerRegion"
Write-Info "Time Zone: $TimeZone"

# Set the project
gcloud config set project $ProjectId

# Enable required APIs
Write-Info "Enabling Cloud Scheduler API..."
gcloud services enable cloudscheduler.googleapis.com --project=$ProjectId

# Create service account for Cloud Scheduler (if it doesn't exist)
$SchedulerSA = "cloud-scheduler-invoker@$ProjectId.iam.gserviceaccount.com"

Write-Info "Checking if service account exists..."
$saExists = gcloud iam service-accounts describe $SchedulerSA --project=$ProjectId 2>$null
if (-not $saExists) {
    Write-Info "Creating service account for Cloud Scheduler..."
    gcloud iam service-accounts create cloud-scheduler-invoker --display-name="Cloud Scheduler Invoker for PAO External Data Service" --project=$ProjectId
    
    Write-Info "Granting Cloud Run Invoker role..."
    $ServiceName = "external-data-service-$Environment"
    gcloud run services add-iam-policy-binding $ServiceName --member="serviceAccount:$SchedulerSA" --role="roles/run.invoker" --region=$Region --project=$ProjectId
} else {
    Write-Info "Service account already exists"
}

# Function to create a scheduler job
function New-SchedulerJob {
    param(
        [string]$JobName,
        [string]$Description,
        [string]$Schedule,
        [string]$Endpoint,
        [int]$Timeout,
        [int]$MaxRetry,
        [string]$HttpMethod = "POST"
    )
    
    Write-Info "Processing scheduler job: $JobName"
    
    $jobExists = $false
    $needsUpdate = $false
    
    $existingJob = gcloud scheduler jobs describe $JobName --location=$SchedulerRegion --project=$ProjectId 2>$null
    if ($existingJob) {
        $jobExists = $true
        Write-Info "Job $JobName already exists, checking if update is needed..."
        
        $currentConfig = gcloud scheduler jobs describe $JobName --location=$SchedulerRegion --project=$ProjectId --format="value(schedule,httpTarget.uri,httpTarget.httpMethod,attemptDeadline,retryConfig.retryCount)" 2>$null
        
        $expectedUri = "$ServiceUrl$Endpoint"
        $expectedConfig = "$Schedule $expectedUri $HttpMethod ${Timeout}s $MaxRetry"
        
        if ($currentConfig -ne $expectedConfig) {
            $needsUpdate = $true
            Write-WarningMsg "Job configuration has changed, update required"
        } else {
            Write-Info "Job $JobName is up to date, skipping"
            return
        }
    }
    
    if ($jobExists -and $needsUpdate) {
        Write-Info "Updating existing job: $JobName"
        gcloud scheduler jobs delete $JobName --location=$SchedulerRegion --project=$ProjectId --quiet
        Write-Info "Deleted existing job for update"
    } elseif (-not $jobExists) {
        Write-Info "Creating new job: $JobName"
    }
    
    if (-not $jobExists -or $needsUpdate) {
        $fullUri = "$ServiceUrl$Endpoint"
        gcloud scheduler jobs create http $JobName --location=$SchedulerRegion --schedule="$Schedule" --time-zone="$TimeZone" --uri="$fullUri" --http-method=$HttpMethod --oidc-service-account-email=$SchedulerSA --oidc-token-audience="$OAuthClientId" --attempt-deadline="${Timeout}s" --max-retry-attempts=$MaxRetry --max-backoff="3600s" --description="$Description" --headers="Content-Type=application/json" --project=$ProjectId
        
        if ($jobExists) {
            Write-Info "Updated job: $JobName"
        } else {
            Write-Info "Created job: $JobName"
        }
    }
}

Write-Host ""
Write-Info "========================================="
Write-Info "Creating PAO External Data Scheduler Jobs"
Write-Info "========================================="

if ($env:SCHEDULER_COMMANDS) {
    Write-WarningMsg "SCHEDULER_COMMANDS environment variable detected, but PowerShell script uses fallback definitions."
    Write-WarningMsg "For YAML-driven scheduler job creation, use the shell script version in Jenkins pipeline."
}

Write-Info "Using hardcoded scheduler job definitions (PowerShell fallback mode)..."

# 1. Users Daily Sync
New-SchedulerJob -JobName "pao-external-data-users-daily" -Description "Daily synchronization of AspNetUsers from UNOPS Resource data" -Schedule "0 2 * * *" -Endpoint "/api/sync/execute/users" -Timeout 1800 -MaxRetry 3 -HttpMethod "POST"

# 2. Currencies Daily Sync
New-SchedulerJob -JobName "pao-external-data-currencies-daily" -Description "Daily synchronization of currency reference data" -Schedule "30 2 * * *" -Endpoint "/api/sync/execute/currencies" -Timeout 1800 -MaxRetry 2 -HttpMethod "POST"

# 3. Countries Daily Sync
New-SchedulerJob -JobName "pao-external-data-countries-daily" -Description "Daily synchronization of country and geographic reference data" -Schedule "45 2 * * *" -Endpoint "/api/sync/execute/countries" -Timeout 1800 -MaxRetry 2 -HttpMethod "POST"

# 4. User Profiles Daily Sync
New-SchedulerJob -JobName "pao-external-data-user-profiles-daily" -Description "Daily synchronization of user profile data" -Schedule "0 3 * * *" -Endpoint "/api/sync/execute/user-profiles" -Timeout 1800 -MaxRetry 3 -HttpMethod "POST"

# 5. User Roles Daily Sync
New-SchedulerJob -JobName "pao-external-data-user-roles-daily" -Description "Daily synchronization of AspNetUserRoles data" -Schedule "15 3 * * *" -Endpoint "/api/sync/execute/user-roles" -Timeout 1800 -MaxRetry 3 -HttpMethod "POST"

# 6. Engagement Partners Daily Sync
New-SchedulerJob -JobName "pao-external-data-engagement-partners-daily" -Description "Daily synchronization of engagement partners data" -Schedule "30 3 * * *" -Endpoint "/api/sync/execute/engagement-partners" -Timeout 1800 -MaxRetry 3 -HttpMethod "POST"

# 7. Engagements Daily Sync
New-SchedulerJob -JobName "pao-external-data-engagements-daily" -Description "Daily synchronization of engagements data" -Schedule "45 3 * * *" -Endpoint "/api/sync/execute/engagements" -Timeout 1800 -MaxRetry 3 -HttpMethod "POST"

# 8. Exchange Rates Daily Sync
New-SchedulerJob -JobName "pao-external-data-exchange-rates-daily" -Description "Daily synchronization of exchange rates data" -Schedule "0 4 * * *" -Endpoint "/api/sync/execute/exchange-rates" -Timeout 1800 -MaxRetry 2 -HttpMethod "POST"

# 9. Organization Hierarchies Daily Sync
New-SchedulerJob -JobName "pao-external-data-org-hierarchies-daily" -Description "Daily synchronization of organization hierarchies data" -Schedule "15 4 * * *" -Endpoint "/api/sync/execute/organization-hierarchies" -Timeout 1800 -MaxRetry 2 -HttpMethod "POST"

# 10. Entity User Roles DoA Daily Sync (after organization-hierarchies)
New-SchedulerJob -JobName "pao-external-data-entity-user-roles-doa-daily" -Description "Daily synchronization of Delegation of Authority (DoA) roles for Engagement Acceptance" -Schedule "20 4 * * *" -Endpoint "/api/sync/execute/entity-user-roles-doa" -Timeout 1800 -MaxRetry 3 -HttpMethod "POST"

# 11. Entity User Roles Management Daily Sync (after organization-hierarchies)
New-SchedulerJob -JobName "pao-external-data-entity-user-roles-mgmt-daily" -Description "Daily synchronization of Organization Management roles (Directors, Deputies)" -Schedule "25 4 * * *" -Endpoint "/api/sync/execute/entity-user-roles-mgmt" -Timeout 1800 -MaxRetry 3 -HttpMethod "POST"

# 12. Partner Agreements Daily Sync
New-SchedulerJob -JobName "pao-external-data-partner-agreements-daily" -Description "Daily synchronization of partner agreements data" -Schedule "30 4 * * *" -Endpoint "/api/sync/execute/partner-agreements" -Timeout 1800 -MaxRetry 2 -HttpMethod "POST"

# 13. Weekly Full Sync
New-SchedulerJob -JobName "pao-external-data-weekly-full-sync" -Description "Weekly comprehensive synchronization of all PAO external data" -Schedule "0 1 * * 1" -Endpoint "/api/sync/execute-all" -Timeout 1800 -MaxRetry 2 -HttpMethod "POST"

# 14. Health Check
New-SchedulerJob -JobName "pao-external-data-health-check" -Description "Regular health check of the PAO external data service" -Schedule "*/15 * * * *" -Endpoint "/health" -Timeout 30 -MaxRetry 1 -HttpMethod "GET"

# 15. Configuration Status Check
New-SchedulerJob -JobName "pao-external-data-config-status" -Description "Periodic status check of all PAO external data sync configurations" -Schedule "0 */4 * * *" -Endpoint "/api/sync/status" -Timeout 180 -MaxRetry 1 -HttpMethod "GET"

Write-Host ""
Write-Info "========================================="
Write-Info "PAO Scheduler Jobs Created Successfully!"
Write-Info "========================================="

Write-Info "Current Cloud Scheduler jobs for PAO External Data Service:"
gcloud scheduler jobs list --location=$SchedulerRegion --project=$ProjectId --filter="name:pao-external-data"

Write-Host ""
Write-Info "Management Commands:"
Write-Info "  Manual trigger: gcloud scheduler jobs run JOB_NAME --location=$SchedulerRegion --project=$ProjectId"
Write-Info "  Pause job: gcloud scheduler jobs pause JOB_NAME --location=$SchedulerRegion --project=$ProjectId"
Write-Info "  Resume job: gcloud scheduler jobs resume JOB_NAME --location=$SchedulerRegion --project=$ProjectId"

Write-Host ""
Write-Info "Service URLs:"
Write-Info "  External Data Service: $ServiceUrl"
Write-Info "  Admin Interface: $ServiceUrl/admin"
Write-Info "  Health Check: $ServiceUrl/health"
Write-Info "  API Documentation: $ServiceUrl/swagger"

Write-Info "Setup complete!"
