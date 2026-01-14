# Cloud SQL Connection Guide for UNOPS Opportunity+ Development

**Purpose**: This guide helps developers connect to Google Cloud SQL PostgreSQL databases for local development using IAM authentication.

**Target Audience**: UNOPS Opportunity+ developers who need database access for local development.

---

## Prerequisites

Before you begin, ensure you have:

1. ✅ **Google Cloud SDK (gcloud)** installed - [Download here](https://cloud.google.com/sdk/docs/install)
2. ✅ **pgAdmin 4** (recommended) or another PostgreSQL client - [Download here](https://www.pgadmin.org/download/)
3. ✅ **Access permissions** to the GCP project `unops-opportunityplus-dev`
4. ✅ **IAM database user** created for your UNOPS email (e.g., `yourname@unops.org`)

**⚠️ Important**: Contact your GCP administrator if you don't have the required permissions.

---

## Quick Start Guide

### Step 1: Authenticate with Google Cloud

Open PowerShell/Command Prompt and run:

```powershell
# Login to your UNOPS Google account
gcloud auth login

# Set up Application Default Credentials (for .NET/Python apps)
gcloud auth application-default login
```

### Step 2: Start the Database Connection

**Option A: Automated Setup (Recommended)**

Run the all-in-one batch file:
```batch
Scripts\connect-cloud-sql-full.bat
```

This will:
- ✅ Start SSH tunnel in a separate window (keep it open!)
- ✅ Generate IAM access token
- ✅ Copy token to clipboard
- ✅ Display connection details

**Option B: Manual Setup**

1. **Start SSH Tunnel** (keep this window open):
   ```batch
   Scripts\connect-cloud-sql-tunnel.bat
   ```

2. **Get Access Token** (use as password):
   ```batch
   Scripts\get-db-access-token.bat
   ```

---

## Connecting with pgAdmin 4

### Initial Setup (First Time Only)

1. **Open pgAdmin 4**

2. **Right-click "Servers"** in the left panel → **"Register"** → **"Server..."**

3. **Configure Connection Settings**:

   **📋 General Tab:**
   - **Name**: `UNOPS Cloud SQL - Dev` (or any name you prefer)

   **📋 Connection Tab:**
   | Setting | Value | Notes |
   |---------|-------|-------|
   | **Host name/address** | `localhost` or `127.0.0.1` | Via SSH tunnel |
   | **Port** | `6364` | Local tunnel port |
   | **Maintenance database** | `unops-opportunityplus-dev-db-[yourname]` | Replace `[yourname]` with your database name |
   | **Username** | `yourname@unops.org` | Your full UNOPS email |
   | **Password** | *(paste access token)* | Get from `get-db-access-token.bat` |
   | **Save password?** | ❌ **NO - Do not check this** | Token expires in 1 hour |

   **📋 SSL Tab:**
   - **SSL mode**: `Prefer` (or `Disable`)

4. **Click "Save"**

### Connecting After Initial Setup

Once you've registered the server, follow these steps each time:

1. **Start SSH Tunnel** (if not already running):
   ```batch
   Scripts\connect-cloud-sql-tunnel.bat
   ```
   ⚠️ **Keep this terminal window open!**

2. **Get Fresh Access Token**:
   ```batch
   Scripts\get-db-access-token.bat
   ```
   Token is automatically copied to clipboard.

3. **In pgAdmin**:
   - Click on your registered server (e.g., "UNOPS Cloud SQL - Dev")
   - **Paste the token** when prompted for password
   - Click "OK"

**🔑 Key Point**: Access tokens expire in **~1 hour**. When connection fails, just run `get-db-access-token.bat` again and reconnect.

---

## Available Databases

Each developer typically has their own database following this pattern:

| Database Name | Description | Access |
|--------------|-------------|--------|
| `unops-opportunityplus-dev-db-[yourname]` | Your personal dev database | Full access |
| `unops-opportunityplus-dev-db-001` | Shared development database | May have limited access |
| `postgres` | Default PostgreSQL database | Read-only |

**Replace `[yourname]` with your actual name** (e.g., `unops-opportunityplus-dev-db-anushas`)

---

## .NET Application Configuration

### Update `appsettings.json`

**Location**: `UNOPS.PAO.Server/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DbSchema": "public",
    "DbContext": "Host=127.0.0.1;Port=6364;Database=unops-opportunityplus-dev-db-[yourname];Username=yourname@unops.org;",
    "UseIamAuthentication": true
  }
}
```

**Important**:
- ✅ Replace `[yourname]` with your actual database name
- ✅ Replace `yourname@unops.org` with your UNOPS email
- ✅ Keep `UseIamAuthentication: true`
- ✅ **Do NOT include a password** - it's handled automatically

### Prerequisites for .NET IAM Auth

1. ✅ Run `gcloud auth application-default login` once
2. ✅ Start SSH tunnel before running the application
3. ✅ The application automatically refreshes tokens every 55 minutes

---

## Python AI Service Configuration

### Update `AIService/config/local.json`

**Location**: `AIService/config/local.json`

```json
{
  "google_cloud": {
    "project": "unops-opportunityplus-dev",
    "location": "europe-west4"
  },
  "database": {
    "url": "postgresql+asyncpg://yourname%40unops.org@localhost:6364/unops-opportunityplus-dev-db-[yourname]"
  }
}
```

**Note**: `%40` is the URL-encoded `@` symbol. Replace `yourname` with your actual name.

### Running AI Service

**Start the service** (automatically gets fresh token):
```batch
Scripts\run-ai-service-uvicorn.bat
```

**Alternative with ADK Web Interface**:
```batch
Scripts\run-ai-service-adk.bat
```

**Access**:
- API: http://localhost:8000
- Swagger Docs: http://localhost:8000/docs
- ADK Web UI: http://localhost:8080 (if using adk.bat)

---

## Database Setup and Data Population

### Running Database Migrations

Once your database is configured, run Entity Framework migrations:

```bash
# Navigate to server directory
cd UNOPS.PAO.Server

# Run migrations
dotnet ef database update
```

**Important Notes:**
- Migrations create all required tables and schema
- The `events` and `sessions` tables are created by Google ADK on first AI Service use
- If migrations fail with permission errors, verify your IAM user has schema modification permissions

### Populating External Data (EDS)

After migrations complete, populate your database with external data:

#### Configure EDS (First Time Only)

**Update `ExternalDataService/appsettings.Local.json`**:

```json
{
    "ConnectionStrings": {
        "DbSchema": "public"
    },
    
    "BigQuery": {
      "ProjectId": "unops-opportunityplus-dev",
      "UseDefaultCredentials": true,
      "CredentialsPath": ""
    },
    
    "ExternalDataService": {
      "ConfigurationPath": "../ExternalDataService/config",
      "Enabled": true,
      "CheckIntervalMinutes": 1,
      "AutoCreateTables": true,
      "TestMode": false,
      "LogLevel": "Information",
      "CommandTimeoutSeconds": 300
    }
}
```

**Important Configuration Notes:**
- ✅ **Do NOT add database connection strings** - the batch file provides them with IAM token
- ✅ Ensure `BigQuery.ProjectId` is set to `unops-opportunityplus-dev`
- ✅ Keep `UseDefaultCredentials: true` for BigQuery
- ✅ Set `TestMode: false` for production-like behavior
- ✅ The batch file automatically injects connection strings with your database name and IAM authentication

#### Option A: Using Batch File (Recommended)

```batch
Scripts\run-external-data-service.bat
```

**The batch file automatically:**
1. ✅ Initializes Git submodules
2. ✅ Gets fresh IAM access token
3. ✅ Injects token into connection string
4. ✅ Runs the service with Local environment
5. ✅ Populates data from external sources

#### Option B: Manual Commands

```bash
# From project root
git submodule update --init --recursive
cd UNOPS.PAO.ExternalDataService
dotnet run --environment Local
```

**Prerequisites:**
- ✅ Cloud SQL tunnel running (`connect-cloud-sql-tunnel.bat`)
- ✅ Database migrations completed (`dotnet ef database update`)
- ✅ **EDS configuration file updated** (see "Configure EDS" section above)
- ✅ Network access to external data sources (BigQuery)
- ✅ Google Cloud authentication configured (`gcloud auth application-default login`)

**What EDS Populates:**
- 📊 Reference data (countries, regions, SDGs, etc.)
- 🏢 Organization master data
- 👥 User profiles and permissions
- 📋 Lookup tables and configurations
- 🔄 Synchronization with external systems

**Expected Behavior:**
- Service logs data fetching progress to console
- May take several minutes depending on data volume
- Exits automatically when complete
- Check console output for any errors or warnings

**Troubleshooting EDS:**

| Issue | Solution |
|-------|----------|
| Connection timeout | Verify tunnel is running on port 6364 |
| Permission denied | Check IAM user has INSERT/UPDATE permissions |
| Submodule errors | Run `git submodule update --init --recursive --force` |
| Service not found | Verify `UNOPS.PAO.ExternalDataService` directory exists |
| Build errors | Run `dotnet restore` and `dotnet build` first |
| Config errors | **Update `ExternalDataService/appsettings.Local.json`** with your database name and email |
| BigQuery errors | Verify `BigQuery.ProjectId` is `unops-opportunityplus-dev` in config |
| Password missing error | Don't add password to config - batch file injects IAM token automatically |

---

## Complete Workflow Example

Here's a typical development session:

### Morning Setup (Start of Day)

1. **Start SSH Tunnel** (leave window open all day):
   ```batch
   Scripts\connect-cloud-sql-tunnel.bat
   ```

2. **First Time Only - Run Migrations**:
   ```bash
   cd UNOPS.PAO.Server
   dotnet ef database update
   ```

3. **First Time Only - Configure & Populate Data**:
   
   **⚠️ IMPORTANT**: Before running EDS, update `ExternalDataService/appsettings.Local.json`:
   - Ensure `BigQuery.ProjectId` is `unops-opportunityplus-dev`
   - **Do NOT add database connection strings** - the batch file handles them
   - See "Database Setup and Data Population" section for full config example
   
   Then run:
   ```batch
   Scripts\run-external-data-service.bat
   ```
   
   The batch file will automatically:
   - Use your current Google Cloud credentials
   - Set your database name based on your email (e.g., `unops-opportunityplus-dev-db-anushas`)
   - Inject IAM authentication token

4. **Get Access Token** (for pgAdmin):
   ```batch
   Scripts\get-db-access-token.bat
   ```

5. **Connect pgAdmin**:
   - Click your server
   - Paste token when prompted
   - Start working with database

6. **Run .NET Application**:
   - SSH tunnel already running ✅
   - Application gets tokens automatically ✅
   - Just hit F5 in Visual Studio

7. **Run AI Service** (if needed):
   ```batch
   Scripts\run-ai-service-uvicorn.bat
   ```

### Throughout the Day

**When token expires** (~1 hour later):
- pgAdmin loses connection
- Run `Scripts\get-db-access-token.bat` again
- Reconnect pgAdmin with new token
- .NET app continues working (auto-refreshes)
- AI service continues working (auto-refreshes)

### End of Day

- Close pgAdmin
- Stop AI service (Ctrl+C)
- Stop .NET app
- **Close SSH tunnel terminal** (or leave running if you prefer)

---

## Troubleshooting

### ❌ "Connection refused" or "Connection timed out"

**Cause**: SSH tunnel not running or disconnected

**Fix**:
1. Check if tunnel terminal is still open
2. Restart tunnel:
   ```batch
   Scripts\connect-cloud-sql-tunnel.bat
   ```

---

### ❌ "FATAL: Cloud SQL IAM user authentication failed"

**Cause**: Access token expired (happens every ~1 hour)

**Fix**:
1. Get new token:
   ```batch
   Scripts\get-db-access-token.bat
   ```
2. Reconnect pgAdmin with new token

---

### ❌ "FATAL: password authentication failed"

**Possible Causes**:
- Wrong token
- Wrong username format
- Incomplete token (didn't copy fully)

**Fix**:
1. Ensure username is your **full email**: `yourname@unops.org`
2. Ensure token is **complete** (very long, ~200+ characters)
3. Generate fresh token:
   ```batch
   Scripts\get-db-access-token.bat
   ```
4. Copy **entire token** (should be automatically copied to clipboard)

---

### ❌ "permission denied for schema public"

**Cause**: Your IAM user lacks permissions on that database

**Fix**:
1. Verify you're connecting to YOUR database: `unops-opportunityplus-dev-db-[yourname]`
2. If issue persists, contact GCP administrator to grant permissions:
   ```sql
   GRANT ALL PRIVILEGES ON DATABASE "unops-opportunityplus-dev-db-yourname" TO "yourname@unops.org";
   GRANT ALL PRIVILEGES ON SCHEMA public TO "yourname@unops.org";
   ```

---

### ❌ "database does not exist"

**Cause**: Database name is incorrect

**Fix**:
1. Ask your GCP administrator for your database name
2. It usually follows pattern: `unops-opportunityplus-dev-db-[yourname]`
3. Update connection strings in:
   - `appsettings.json` (for .NET)
   - `AIService/config/local.json` (for Python)

---

### ❌ SSH tunnel asks for passphrase repeatedly

**Cause**: SSH key has a passphrase set

**Fix**:
- Enter the passphrase when prompted
- Or use `ssh-agent` to cache credentials
- Or regenerate SSH key without passphrase

---

### ❌ .NET Application: "Connection pool exhausted"

**Cause**: Too many connections or connections not being closed

**Fix**:
1. Restart SSH tunnel
2. Restart application
3. Check for connection leaks in code

---

## Connection Details Reference

| Setting | Value | Notes |
|---------|-------|-------|
| **GCP Project** | `unops-opportunityplus-dev` | Dev environment |
| **Cloud SQL Instance** | `unops-opportunityplus-dev-postgres-001` | PostgreSQL 15 |
| **Region** | `europe-west4` | Belgium |
| **Private IP** | `10.129.0.16` | Internal only |
| **Local Tunnel Port** | `6364` | Via SSH tunnel |
| **Bastion VM** | `unopsgc567901-sql-proxy` | IAP-enabled |
| **Bastion Zone** | `europe-west4-b` | VM location |
| **Auth Method** | IAM Authentication | OAuth2 tokens |
| **Token Expiry** | ~1 hour | Regenerate as needed |

---

## Security Best Practices

1. ✅ **Never commit access tokens** to git
2. ✅ **Never save passwords** in pgAdmin (tokens expire anyway)
3. ✅ **Close SSH tunnel** when not in use
4. ✅ **Use IAM authentication** (more secure than static passwords)
5. ✅ **Keep gcloud SDK updated**: `gcloud components update`
6. ✅ **Log out of unused accounts**: `gcloud auth revoke`

---

## Quick Reference Commands

```powershell
# ====================================
# Authentication
# ====================================
gcloud auth login                           # Login to Google Cloud
gcloud auth application-default login       # Set up ADC for apps
gcloud auth list                           # Check current account
gcloud config set account yourname@unops.org # Switch account

# ====================================
# Database Connection
# ====================================
Scripts\connect-cloud-sql-tunnel.bat        # Start SSH tunnel
Scripts\get-db-access-token.bat            # Get token for pgAdmin
Scripts\connect-cloud-sql-full.bat         # All-in-one setup

# ====================================
# Database Setup (First Time Only)
# ====================================
cd UNOPS.PAO.Server
dotnet ef database update                   # Run migrations
cd ..
Scripts\run-external-data-service.bat       # Populate external data

# ====================================
# AI Service
# ====================================
Scripts\run-ai-service-uvicorn.bat         # Run with Uvicorn
Scripts\run-ai-service-adk.bat             # Run with ADK Web UI

# ====================================
# Troubleshooting
# ====================================
gcloud auth print-access-token             # Manual token generation
netstat -ano | findstr ":6364"             # Check if port is in use
gcloud compute ssh unopsgc567901-sql-proxy --tunnel-through-iap --project=unops-opportunityplus-dev --zone=europe-west4-b --ssh-flag="-L 6364:10.129.0.16:5432"  # Manual tunnel
```

---

## Batch Files Overview

| Script | Purpose | When to Use |
|--------|---------|-------------|
| `connect-cloud-sql-full.bat` | 🚀 **All-in-one setup** - Starts tunnel + gets token | **Best for daily startup** |
| `connect-cloud-sql-tunnel.bat` | Start SSH tunnel only | When tunnel disconnects |
| `get-db-access-token.bat` | Get fresh access token | When token expires (~1 hour) |
| `run-ai-service-uvicorn.bat` | Start AI service with Uvicorn | For AI feature development |
| `run-ai-service-adk.bat` | Start AI service with ADK Web UI | For AI agent testing |

---

## Need Help?

- **GCP Console**: https://console.cloud.google.com/sql/instances?project=unops-opportunityplus-dev
- **Cloud SQL IAM Docs**: https://cloud.google.com/sql/docs/postgres/iam-authentication
- **Permission Issues**: Contact your GCP project administrator
- **Technical Issues**: Contact the development team lead

---

## Summary for New Developers

**To start working with the database:**

1. ✅ Install gcloud SDK and pgAdmin 4
2. ✅ Run `gcloud auth login` and `gcloud auth application-default login`
3. ✅ Get your database name from admin (format: `unops-opportunityplus-dev-db-yourname`)
4. ✅ Run `Scripts\connect-cloud-sql-full.bat` (starts tunnel + gets token)
5. ✅ In pgAdmin: Register server with connection details (see "Connecting with pgAdmin 4" section)
6. ✅ Update `appsettings.json` with your database name
7. ✅ Start developing! 🎉

**Remember**: Access tokens expire every hour - just run `get-db-access-token.bat` again when needed.

---

*Last Updated: January 2026*
