# PR #671 - Database Connection Guide

**Important**: Understanding your database setup before running verification tests.

---

## 🔍 **Your Current Configuration**

Based on `UNOPS.PAO.Server/appsettings.json`:

```json
"ConnectionStrings": {
  "DbContext": "Host=127.0.0.1;Port=6364;Database=unops-opportunityplus-dev-db-anushas;Username=anushas@unops.org;",
  "UseIamAuthentication": true
}
```

---

## 🌐 **Database Type: REMOTE (Google Cloud SQL via Proxy)**

### **What This Means:**

**NOT a local database!** Your setup is connecting to:

- **Remote Database**: Google Cloud PostgreSQL instance
- **Connection Method**: Cloud SQL Proxy tunnel
- **Host**: 127.0.0.1 (localhost) is just the proxy endpoint
- **Actual Port**: 6364 (Cloud SQL Proxy listens here)
- **Real Location**: Google Cloud `unops-opportunityplus-dev`
- **Authentication**: IAM-based (Google Cloud credentials)
- **User**: anushas@unops.org

### **Architecture:**
```
Your Machine
    ↓
Cloud SQL Proxy (localhost:6364)
    ↓
[Internet/Google Cloud Network]
    ↓
PostgreSQL Database in Google Cloud
(unops-opportunityplus-dev project)
```

---

## ⚠️ **CRITICAL: Prerequisites for Database Access**

Before running PR #671 verification, you MUST have:

### **1. Cloud SQL Proxy Running**
```powershell
# Check if proxy is running
Get-Process | Where-Object { $_.ProcessName -like "*cloud*sql*proxy*" }

# Or check port 6364
Test-NetConnection -ComputerName 127.0.0.1 -Port 6364
```

**If not running**, you need to start it:
```bash
# Typical command (update with your specific instance)
cloud-sql-proxy unops-opportunityplus-dev:europe-west4:your-instance-name
```

### **2. Google Cloud Authentication**
```bash
# Verify you're authenticated
gcloud auth list

# If not authenticated:
gcloud auth login
gcloud auth application-default login
```

### **3. IAM Permissions**
Your Google account (anushas@unops.org) needs:
- ✅ Cloud SQL Client role
- ✅ Read/Write permissions on the database
- ✅ Access to the `unops-opportunityplus-dev` project

---

## 🚦 **Decision: Which Environment to Test?**

You have **3 options** for running PR #671 verification:

### **Option A: Test on DEV Environment (RECOMMENDED)**

**What**: Use the existing remote DEV database (current configuration)

**Pros:**
- ✅ Already configured in your appsettings.json
- ✅ Real data to test with
- ✅ Migration already applied (if deployed to DEV)
- ✅ Matches production-like environment

**Cons:**
- ⚠️ Requires Cloud SQL Proxy running
- ⚠️ Requires Google Cloud authentication
- ⚠️ Shared environment (other developers may be using it)

**Best For**: 
- Verifying that DEV environment is working correctly
- Testing with real/representative data
- Confirming migration was applied to DEV

**How to Proceed:**
1. Ensure Cloud SQL Proxy is running
2. Verify you can connect (see "Quick Connection Test" below)
3. Run the verification scripts

---

### **Option B: Set Up Local PostgreSQL (IF NEEDED)**

**What**: Install PostgreSQL locally for isolated testing

**Pros:**
- ✅ Full control over database
- ✅ No dependency on Cloud SQL Proxy
- ✅ Can test without affecting others
- ✅ Faster queries (no network latency)

**Cons:**
- ⚠️ Requires PostgreSQL installation
- ⚠️ Need to apply all migrations manually
- ⚠️ Won't have real data (need to seed test data)
- ⚠️ Doesn't test actual DEV environment

**Setup Steps:**
```powershell
# 1. Install PostgreSQL
# Download from: https://www.postgresql.org/download/windows/

# 2. Create database
psql -U postgres
CREATE DATABASE opportunityplus_local;

# 3. Update appsettings to point to local DB
# Create appsettings.Local.json with:
{
  "ConnectionStrings": {
    "DbContext": "Host=localhost;Port=5432;Database=opportunityplus_local;Username=postgres;Password=yourpassword;",
    "UseIamAuthentication": false
  }
}

# 4. Apply migrations
dotnet ef database update --project UNOPS.PAO.UNOPSDataAccess --startup-project UNOPS.PAO.Server
```

---

### **Option C: Skip Database Tests, Run Application Tests Only**

**What**: Verify application functionality without database verification

**Best For**:
- When database access is complex/blocked
- Quick smoke test of application behavior
- Verifying UI/UX changes only

**Limitation**: 
- Cannot verify migration was applied
- Cannot check for NULL/empty Stage values
- Less comprehensive verification

---

## 🔧 **Quick Connection Test**

Before running PR #671 verification, test your database connection:

### **Test 1: Check Proxy (if using Cloud SQL)**
```powershell
# Check if proxy process is running
Get-Process | Where-Object { $_.ProcessName -like "*cloud*sql*proxy*" } | Select-Object ProcessName, Id, StartTime

# Check if port 6364 is listening
Test-NetConnection -ComputerName 127.0.0.1 -Port 6364
```

**Expected**: Should show proxy process and successful connection to port 6364

---

### **Test 2: Database Connection Test**
```powershell
# Quick connection test using psql (if installed)
psql "host=127.0.0.1 port=6364 dbname=unops-opportunityplus-dev-db-anushas user=anushas@unops.org"

# If that works, run quick query:
\dt public."Opportunities"
```

**Expected**: Should connect and show Opportunities table

---

### **Test 3: EF Core Connection Test**
```bash
# From project root
cd UNOPS.PAO.Server

# List migrations (tests connection)
dotnet ef migrations list --project ../UNOPS.PAO.UNOPSDataAccess --startup-project .
```

**Expected**: Should list migrations including `20260122185435_SetDefaultStageForOpportunity`

---

## ✅ **Recommended Approach for PR #671 Verification**

Based on typical development workflow:

### **If Cloud SQL Proxy is Running** (Most Likely)

1. **Verify Connection:**
   ```powershell
   # Test connection to DEV database
   psql "host=127.0.0.1 port=6364 dbname=unops-opportunityplus-dev-db-anushas user=anushas@unops.org"
   ```

2. **Run Database Verification:**
   - Use the DEV environment connection
   - Run `verify-pr-671.sql` to check migration status
   - This verifies DEV environment is ready

3. **Run Application Smoke Test:**
   - Start application: `dotnet run` from UNOPS.PAO.Server
   - Application will connect to DEV database
   - Follow `PR_671_QUICK_TEST_GUIDE.md`

---

### **If Cloud SQL Proxy is NOT Running**

**Option 1**: Start the proxy
```bash
# You'll need the exact command for your setup
# Format: cloud-sql-proxy INSTANCE_CONNECTION_NAME
cloud-sql-proxy unops-opportunityplus-dev:europe-west4:instance-name
```

**Option 2**: Skip database verification
- Go straight to application testing
- Application will either work (if proxy starts automatically) or show connection errors
- Document any connection issues

---

## 📋 **Updated Verification Checklist**

### **Before Starting PR #671 Verification:**

- [ ] **Understand your database type**
  - [ ] Using Cloud SQL Proxy to DEV environment (most likely)
  - [ ] Using local PostgreSQL
  - [ ] Not sure (check steps below)

- [ ] **Test database connection**
  - [ ] Cloud SQL Proxy running (if applicable)
  - [ ] Can connect using psql or similar tool
  - [ ] EF migrations list command works

- [ ] **Choose verification approach**
  - [ ] Full verification (database + application)
  - [ ] Application only (skip database queries)

---

## 🎯 **Quick Decision Tree**

```
Can you run: psql "host=127.0.0.1 port=6364 dbname=unops-opportunityplus-dev-db-anushas user=anushas@unops.org"
    ↓
   YES → Great! Use DEV environment verification
        → Run verify-pr-671.sql
        → Run application smoke test
    ↓
   NO → Is Cloud SQL Proxy installed?
        ↓
       YES → Start Cloud SQL Proxy
            → Retry connection
            → If works: continue verification
        ↓
       NO → Contact team lead/DevOps for:
            • Cloud SQL Proxy setup instructions
            • OR alternative local database setup
            • OR access to a test environment
```

---

## 🆘 **Troubleshooting**

### **Issue: "Connection refused on port 6364"**
**Solution**: Cloud SQL Proxy is not running
```bash
# Start proxy (get exact command from your team)
cloud-sql-proxy [your-instance-connection-name]
```

### **Issue: "Authentication failed"**
**Solution**: IAM authentication issue
```bash
# Re-authenticate with gcloud
gcloud auth login
gcloud auth application-default login
```

### **Issue: "Database does not exist"**
**Solution**: Connected to wrong instance or database name changed
- Check appsettings.json for correct database name
- Verify with team which database to use

### **Issue: "Permission denied"**
**Solution**: Your Google account lacks necessary IAM roles
- Contact DevOps/Admin to grant Cloud SQL Client role
- Verify project access

---

## 📞 **Need Help?**

If you're stuck with database connection:

1. **Quick Option**: Skip database verification, run application tests only
   - Start application with `dotnet run`
   - If it starts successfully → database connection is working
   - Follow `PR_671_QUICK_TEST_GUIDE.md` for application tests

2. **Get Help**: Contact your team with this info:
   - Your Google account email
   - Error message when trying to connect
   - Screenshot of Cloud SQL Proxy status (if applicable)

---

## 🎯 **Summary for PR #671**

**Your Database Setup:**
- 🌐 **Type**: Remote Google Cloud SQL (NOT local)
- 🔌 **Access**: Via Cloud SQL Proxy on localhost:6364
- 🔐 **Auth**: IAM-based with Google credentials
- 📊 **Database**: unops-opportunityplus-dev-db-anushas

**To Verify PR #671:**
1. ✅ Test connection to database (psql or EF migrations list)
2. ✅ Run database verification SQL (if connection works)
3. ✅ Run application smoke test
4. ✅ Document results

**If Connection Fails:**
- Try application smoke test only (skip database queries)
- Application startup will reveal if DB connection is working
- Contact team if persistent issues

---

**Next Steps:** 
Return to `PR_671_START_HERE.md` with this understanding of your database setup!
