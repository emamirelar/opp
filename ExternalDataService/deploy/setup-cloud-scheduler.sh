#!/bin/bash
# Setup Cloud Scheduler Jobs for UNOPS Opportunity+ (PAO) - External Data Service
# This script creates Cloud Scheduler jobs to automatically trigger PAO-specific sync configurations

set -e

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Check if required parameters are provided
if [ $# -lt 3 ]; then
    echo "Usage: $0 <project-id> <environment> <region> [oauth-client-id] [config-file-path]"
    echo ""
    echo "Parameters:"
    echo "  project-id       GCP Project ID (e.g., unops-opportunityplus-development)"
    echo "  environment      Environment name: dev, test, qa, or prod"
    echo "  region           Cloud Run region (e.g., europe-west4)"
    echo "  oauth-client-id  (Optional) IAP OAuth Client ID - auto-read from config if not provided"
    echo "  config-file-path (Optional) Path to appsettings.{Env}.json file"
    echo ""
    echo "Example (with OAuth Client ID):"
    echo "  $0 unops-opportunityplus-development dev europe-west4 123456789-xxx.apps.googleusercontent.com"
    echo ""
    echo "Example (auto-read from config file):"
    echo "  $0 unops-opportunityplus-development dev europe-west4 \"\" ./ExternalDataService/appsettings.Development.json"
    echo ""
    echo "URL Pattern (TO BE PROVIDED BY INFRA):"
    echo "  prod:        https://eds.opportunityplus.unops.org"
    echo "  qa:          https://eds.opportunityplus.qa.unops.org"
    echo "  dev/test:    https://eds.opportunityplus.{env}.unops.org"
    exit 1
fi

PROJECT_ID=$1
ENVIRONMENT=$2
REGION=$3
OAUTH_CLIENT_ID=${4:-""}
CONFIG_FILE_PATH=${5:-""}

# Map environment name to appsettings file name
# Handles both short (dev, test, qa, prod) and full (development, test, qa, production) names
get_appsettings_env_name() {
    local env=$1
    case $env in
        "dev"|"development") echo "Development" ;;
        "test") echo "Test" ;;
        "qa") echo "QA" ;;
        "prod"|"production") echo "Production" ;;
        *) echo "$env" ;;
    esac
}

# Try to auto-discover OAuth Client ID from config file
if [ -z "$OAUTH_CLIENT_ID" ]; then
    # If config file path not provided, try default location
    if [ -z "$CONFIG_FILE_PATH" ]; then
        APPSETTINGS_ENV=$(get_appsettings_env_name "$ENVIRONMENT")
        CONFIG_FILE_PATH="./ExternalDataService/appsettings.${APPSETTINGS_ENV}.json"
    fi
    
    if [ -f "$CONFIG_FILE_PATH" ]; then
        print_info "Reading OAuth Client ID from config file: $CONFIG_FILE_PATH"
        # Use grep and sed to extract OAuthClientId (works without jq)
        OAUTH_CLIENT_ID=$(grep -o '"OAuthClientId"[[:space:]]*:[[:space:]]*"[^"]*"' "$CONFIG_FILE_PATH" | sed 's/.*: *"\([^"]*\)".*/\1/' | head -1)
        
        if [ -z "$OAUTH_CLIENT_ID" ]; then
            print_error "Could not find CloudScheduler.OAuthClientId in config file"
            print_error "Please provide OAuth Client ID as parameter or add it to the config file"
            exit 1
        fi
        print_info "Found OAuth Client ID: $OAUTH_CLIENT_ID"
    else
        print_error "Config file not found: $CONFIG_FILE_PATH"
        print_error "Please provide OAuth Client ID as parameter or specify config file path"
        exit 1
    fi
fi

# Read additional settings from config file if available
TIMEZONE="Europe/Copenhagen"
SCHEDULER_REGION=""

if [ -n "$CONFIG_FILE_PATH" ] && [ -f "$CONFIG_FILE_PATH" ]; then
    # Try to read TimeZone from config
    CONFIG_TIMEZONE=$(grep -o '"TimeZone"[[:space:]]*:[[:space:]]*"[^"]*"' "$CONFIG_FILE_PATH" | sed 's/.*: *"\([^"]*\)".*/\1/' | head -1)
    if [ -n "$CONFIG_TIMEZONE" ]; then
        TIMEZONE="$CONFIG_TIMEZONE"
    fi
    
    # Try to read Region from config (scheduler region)
    CONFIG_REGION=$(grep -o '"Region"[[:space:]]*:[[:space:]]*"[^"]*"' "$CONFIG_FILE_PATH" | sed 's/.*: *"\([^"]*\)".*/\1/' | head -1)
    if [ -n "$CONFIG_REGION" ]; then
        SCHEDULER_REGION="$CONFIG_REGION"
    fi
fi

# Auto-construct Service URL based on environment
# Pattern (TO BE PROVIDED BY INFRA - using placeholder pattern):
#   prod:        https://eds.opportunityplus.unops.org
#   qa:          https://eds.opportunityplus.qa.unops.org
#   dev/test:    https://eds.opportunityplus.{env}.unops.org
if [ "$ENVIRONMENT" = "prod" ] || [ "$ENVIRONMENT" = "production" ]; then
    SERVICE_URL="https://eds.opportunityplus.unops.org"
elif [ "$ENVIRONMENT" = "qa" ]; then
    SERVICE_URL="https://eds.opportunityplus.qa.unops.org"
else
    SERVICE_URL="https://eds.opportunityplus.${ENVIRONMENT}.unops.org"
fi

# Map Cloud Run region to nearest Scheduler region if not specified
# Note: Cloud Scheduler doesn't support all regions (e.g., europe-west4)
if [ -z "$SCHEDULER_REGION" ]; then
    case $REGION in
        "europe-west4") SCHEDULER_REGION="europe-west3" ;;   # europe-west4 not supported by Cloud Scheduler
        "europe-west2") SCHEDULER_REGION="europe-west2" ;;
        "europe-west1") SCHEDULER_REGION="europe-west1" ;;
        "us-central1")  SCHEDULER_REGION="us-central1" ;;
        "us-east1")     SCHEDULER_REGION="us-east1" ;;
        *) SCHEDULER_REGION=$REGION ;;
    esac
fi

print_info "Setting up Cloud Scheduler for UNOPS Opportunity+ (PAO)"
print_info "Project ID: $PROJECT_ID"
print_info "Environment: $ENVIRONMENT"
print_info "Service URL (Load Balancer): $SERVICE_URL"
print_info "OAuth Client ID: $OAUTH_CLIENT_ID"
print_info "Cloud Run Region: $REGION"
print_info "Scheduler Region: $SCHEDULER_REGION"
print_info "Time Zone: $TIMEZONE"

# Set the project
gcloud config set project $PROJECT_ID

# Enable required APIs
print_info "Enabling Cloud Scheduler API..."
gcloud services enable cloudscheduler.googleapis.com --project=$PROJECT_ID

# Create service account for Cloud Scheduler (if it doesn't exist)
SCHEDULER_SA="cloud-scheduler-invoker@${PROJECT_ID}.iam.gserviceaccount.com"

print_info "Checking if service account exists..."
if ! gcloud iam service-accounts describe $SCHEDULER_SA --project=$PROJECT_ID 2>/dev/null; then
    print_info "Creating service account for Cloud Scheduler..."
    gcloud iam service-accounts create cloud-scheduler-invoker \
        --display-name="Cloud Scheduler Invoker for PAO External Data Service" \
        --project=$PROJECT_ID
    
    print_info "Granting Cloud Run Invoker role..."
    # Extract service name from URL - use the environment-specific service name
    # For PAO, the service name follows the pattern: external-data-service-{environment}
    SERVICE_NAME="external-data-service-${ENVIRONMENT}"
    gcloud run services add-iam-policy-binding $SERVICE_NAME \
        --member="serviceAccount:${SCHEDULER_SA}" \
        --role="roles/run.invoker" \
        --region=$REGION \
        --project=$PROJECT_ID
else
    print_info "Service account already exists"
fi

# Function to create or update a scheduler job
create_scheduler_job() {
    local JOB_NAME=$1
    local DESCRIPTION=$2
    local SCHEDULE=$3
    local ENDPOINT=$4
    local TIMEOUT=$5
    local MAX_RETRY=$6
    local HTTP_METHOD=${7:-"POST"}
    
    print_info "Processing scheduler job: $JOB_NAME"
    
    # Check if job exists and get current configuration
    local JOB_EXISTS=false
    local NEEDS_UPDATE=false
    
    if gcloud scheduler jobs describe $JOB_NAME --location=$SCHEDULER_REGION --project=$PROJECT_ID 2>/dev/null >/dev/null; then
        JOB_EXISTS=true
        print_info "Job $JOB_NAME already exists, checking if update is needed..."
        
        # Get current job configuration
        local CURRENT_CONFIG=$(gcloud scheduler jobs describe $JOB_NAME --location=$SCHEDULER_REGION --project=$PROJECT_ID --format="value(schedule,httpTarget.uri,httpTarget.httpMethod,attemptDeadline,retryConfig.retryCount)" 2>/dev/null)
        
        # Build expected configuration
        local EXPECTED_URI="${SERVICE_URL}${ENDPOINT}"
        local EXPECTED_CONFIG="${SCHEDULE} ${EXPECTED_URI} ${HTTP_METHOD} ${TIMEOUT}s ${MAX_RETRY}"
        
        # Compare configurations (simplified comparison)
        if [[ "$CURRENT_CONFIG" != "$EXPECTED_CONFIG" ]]; then
            NEEDS_UPDATE=true
            print_warning "Job configuration has changed, update required"
        else
            print_info "✓ Job $JOB_NAME is up to date, skipping"
            return 0
        fi
    fi
    
    # Update existing job or create new one
    if [ "$JOB_EXISTS" = true ] && [ "$NEEDS_UPDATE" = true ]; then
        print_info "Updating existing job: $JOB_NAME"
        # For Cloud Scheduler, we need to delete and recreate to update configuration
        # This is a limitation of the gcloud CLI - no direct update command for all properties
        gcloud scheduler jobs delete $JOB_NAME \
            --location=$SCHEDULER_REGION \
            --project=$PROJECT_ID \
            --quiet
        print_info "Deleted existing job for update"
    elif [ "$JOB_EXISTS" = false ]; then
        print_info "Creating new job: $JOB_NAME"
    fi
    
    # Only create/update if needed (job doesn't exist or needs update)
    if [ "$JOB_EXISTS" = false ] || [ "$NEEDS_UPDATE" = true ]; then
        # Create the job with IAP-compatible OIDC configuration
        gcloud scheduler jobs create http $JOB_NAME \
            --location=$SCHEDULER_REGION \
            --schedule="$SCHEDULE" \
            --time-zone="$TIMEZONE" \
            --uri="${SERVICE_URL}${ENDPOINT}" \
            --http-method=$HTTP_METHOD \
            --oidc-service-account-email=$SCHEDULER_SA \
            --oidc-token-audience="$OAUTH_CLIENT_ID" \
            --attempt-deadline="${TIMEOUT}s" \
            --max-retry-attempts=$MAX_RETRY \
            --max-backoff="3600s" \
            --min-backoff="5s" \
            --headers="Content-Type=application/json" \
            --description="$DESCRIPTION" \
            --project=$PROJECT_ID
        
        if [ "$JOB_EXISTS" = true ]; then
            print_info "✓ Updated job: $JOB_NAME"
        else
            print_info "✓ Created job: $JOB_NAME"
        fi
    fi
}

# Create scheduler jobs for PAO-specific sync configurations
echo ""
print_info "========================================="
print_info "Creating PAO External Data Scheduler Jobs"
print_info "========================================="

# Check if scheduler commands were provided by Jenkins (YAML-driven approach)
if [ -n "$SCHEDULER_COMMANDS" ]; then
    print_info "Using scheduler jobs generated from YAML configuration..."
    
    # Execute the dynamically generated scheduler commands
    eval "$SCHEDULER_COMMANDS"
    
else
    print_warning "No SCHEDULER_COMMANDS provided, using fallback hardcoded job definitions..."
    
    # Fallback: Hardcoded job definitions (for manual execution or if YAML processing fails)
    # ORDER: Matches config/01-aspnetusers through config/12-exchange-rates
    
    # 01 - Users Daily Sync - Daily at 2:00 AM
    create_scheduler_job \
        "pao-external-data-users-daily" \
        "Daily synchronization of AspNetUsers from UNOPS Resource data" \
        "0 2 * * *" \
        "/api/sync/execute/users" \
        "1800" \
        "3" \
        "POST"

    # 02 - User Profiles Daily Sync - Daily at 2:15 AM
    create_scheduler_job \
        "pao-external-data-user-profiles-daily" \
        "Daily synchronization of user profile data" \
        "15 2 * * *" \
        "/api/sync/execute/user-profiles" \
        "1800" \
        "3" \
        "POST"

    # 03 - User Roles Daily Sync - Daily at 2:30 AM
    create_scheduler_job \
        "pao-external-data-user-roles-daily" \
        "Daily synchronization of AspNetUserRoles data" \
        "30 2 * * *" \
        "/api/sync/execute/user-roles" \
        "1800" \
        "3" \
        "POST"

    # 04 - Countries Daily Sync - Daily at 2:45 AM
    create_scheduler_job \
        "pao-external-data-countries-daily" \
        "Daily synchronization of country and geographic reference data" \
        "45 2 * * *" \
        "/api/sync/execute/countries" \
        "1800" \
        "2" \
        "POST"

    # 05 - Currencies Daily Sync - Daily at 3:00 AM
    create_scheduler_job \
        "pao-external-data-currencies-daily" \
        "Daily synchronization of currency reference data" \
        "0 3 * * *" \
        "/api/sync/execute/currencies" \
        "1800" \
        "2" \
        "POST"

    # 06 - Engagements Daily Sync - Daily at 3:15 AM (MUST run before engagement-partners)
    create_scheduler_job \
        "pao-external-data-engagements-daily" \
        "Daily synchronization of engagements data" \
        "15 3 * * *" \
        "/api/sync/execute/engagements" \
        "1800" \
        "3" \
        "POST"

    # 07 - Engagement Partners Daily Sync - Daily at 3:30 AM
    create_scheduler_job \
        "pao-external-data-engagement-partners-daily" \
        "Daily synchronization of engagement partners data" \
        "30 3 * * *" \
        "/api/sync/execute/engagement-partners" \
        "1800" \
        "3" \
        "POST"

    # 08 - Partner Agreements Daily Sync - Daily at 3:45 AM
    create_scheduler_job \
        "pao-external-data-partner-agreements-daily" \
        "Daily synchronization of partner agreements data" \
        "45 3 * * *" \
        "/api/sync/execute/partner-agreements" \
        "1800" \
        "2" \
        "POST"

    # 09 - Organization Hierarchies Daily Sync - Daily at 4:00 AM
    create_scheduler_job \
        "pao-external-data-org-hierarchies-daily" \
        "Daily synchronization of organization hierarchies data" \
        "0 4 * * *" \
        "/api/sync/execute/organization-hierarchies" \
        "1800" \
        "2" \
        "POST"

    # 10 - Entity User Roles DoA Daily Sync - Daily at 4:15 AM
    create_scheduler_job \
        "pao-external-data-entity-user-roles-doa-daily" \
        "Daily synchronization of Delegation of Authority (DoA) roles for Engagement Acceptance" \
        "15 4 * * *" \
        "/api/sync/execute/entity-user-roles-doa" \
        "1800" \
        "3" \
        "POST"

    # 11 - Entity User Roles Management Daily Sync - Daily at 4:25 AM
    create_scheduler_job \
        "pao-external-data-entity-user-roles-mgmt-daily" \
        "Daily synchronization of Organization Management roles (Directors, Deputies)" \
        "25 4 * * *" \
        "/api/sync/execute/entity-user-roles-mgmt" \
        "1800" \
        "3" \
        "POST"

    # 12 - Exchange Rates Daily Sync - Daily at 4:35 AM
    create_scheduler_job \
        "pao-external-data-exchange-rates-daily" \
        "Daily synchronization of exchange rates data" \
        "35 4 * * *" \
        "/api/sync/execute/exchange-rates" \
        "1800" \
        "2" \
        "POST"

    # 13. Weekly Full Sync - Monday at 1 AM
    create_scheduler_job \
        "pao-external-data-weekly-full-sync" \
        "Weekly comprehensive synchronization of all PAO external data" \
        "0 1 * * 1" \
        "/api/sync/execute-all" \
        "1800" \
        "2" \
        "POST"

    # 14. Health Check - Every 15 minutes
    create_scheduler_job \
        "pao-external-data-health-check" \
        "Regular health check of the PAO external data service" \
        "*/15 * * * *" \
        "/health" \
        "30" \
        "1" \
        "GET"

    # 15. Configuration Status Check - Every 4 hours
    create_scheduler_job \
        "pao-external-data-config-status" \
        "Periodic status check of all PAO external data sync configurations" \
        "0 */4 * * *" \
        "/api/sync/status" \
        "180" \
        "1" \
        "GET"
fi

echo ""
print_info "========================================="
print_info "PAO Scheduler Jobs Created Successfully!"
print_info "========================================="

# List all jobs
print_info "Current Cloud Scheduler jobs for PAO External Data Service:"
gcloud scheduler jobs list --location=$SCHEDULER_REGION --project=$PROJECT_ID --filter="name:pao-external-data"

echo ""
print_info "Management Commands:"
print_info "  Manual trigger: gcloud scheduler jobs run JOB_NAME --location=$SCHEDULER_REGION --project=$PROJECT_ID"
print_info "  View logs: gcloud logging read 'resource.type=\"cloud_scheduler_job\" AND resource.labels.job_id=\"JOB_NAME\"' --project=$PROJECT_ID --limit=10"
print_info "  Pause job: gcloud scheduler jobs pause JOB_NAME --location=$SCHEDULER_REGION --project=$PROJECT_ID"
print_info "  Resume job: gcloud scheduler jobs resume JOB_NAME --location=$SCHEDULER_REGION --project=$PROJECT_ID"

echo ""
print_info "Service URLs:"
print_info "  External Data Service: $SERVICE_URL"
print_info "  Admin Interface: $SERVICE_URL/admin"
print_info "  Health Check: $SERVICE_URL/health"
print_info "  API Documentation: $SERVICE_URL/swagger"

print_info "Setup complete!"
