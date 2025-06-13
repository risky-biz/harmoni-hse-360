# Fly.io Deployment Failure Analysis and Resolution

## 🚨 Problem Summary

The Fly.io deployment for `harmoni-hse-360` was failing with the following critical issues:

1. **Application not listening on expected address (0.0.0.0:8080)**
2. **Health check timeouts**
3. **Network connectivity issues preventing VM status checks**
4. **Only `/.fly/hallpass` process found on port 22 (SSH service)**

## 🔍 Root Cause Analysis

### Primary Issues Identified:

1. **Missing Production Configuration File**
   - GitHub Actions workflow referenced `--config fly.toml` for production deployment
   - Only `fly.toml.example` and `fly.staging.toml` existed in repository
   - No actual `fly.toml` file for production app `harmoni-hse-360`

2. **Docker Image Version Mismatch**
   - `Dockerfile.flyio` was using .NET 9.0 images
   - Project targets .NET 8.0 (`<TargetFramework>net8.0</TargetFramework>`)
   - Version mismatch prevented application from starting properly

3. **Application Configuration Issues**
   - HTTPS redirection enabled in production (conflicts with Fly.io proxy)
   - App name inconsistency between fly.toml and GitHub Actions workflow
   - Potential database migration blocking application startup

4. **Health Check Configuration Mismatch**
   - GitHub Actions health check URL used wrong app name
   - Expected: `harmoni-hse-360.fly.dev`
   - Actual in workflow: `harmoni360-app.fly.dev`

## ✅ Implemented Fixes

### 1. Created Production fly.toml Configuration

**File:** `fly.toml`
```toml
app = "harmoni-hse-360"
primary_region = "sjc"

[build]
  dockerfile = "Dockerfile.flyio"

[env]
  ASPNETCORE_ENVIRONMENT = "Production"
  ASPNETCORE_URLS = "http://+:8080"
  ASPNETCORE_FORWARDEDHEADERS_ENABLED = "true"

[[services]]
  internal_port = 8080
  protocol = "tcp"
  auto_stop_machines = false
  auto_start_machines = true
  min_machines_running = 1

[[services.http_checks]]
  interval = "10s"
  grace_period = "10s"
  method = "GET"
  path = "/health"
  protocol = "http"
  timeout = "5s"
  tls_skip_verify = false

[vm]
  cpu_kind = "shared"
  cpus = 1
  memory_mb = 1024
```

### 2. Fixed Docker Image Version Mismatch

**File:** `Dockerfile.flyio`
- Changed FROM `mcr.microsoft.com/dotnet/sdk:9.0-alpine` → `mcr.microsoft.com/dotnet/sdk:8.0-alpine`
- Changed FROM `mcr.microsoft.com/dotnet/aspnet:9.0-alpine` → `mcr.microsoft.com/dotnet/aspnet:8.0-alpine`

### 3. Updated Application Configuration

**File:** `src/Harmoni360.Web/Program.cs`

**Changes Made:**
- **Conditional HTTPS Redirection:** Only enabled in development environment
- **Enhanced Logging:** Added startup debugging information
- **Resilient Database Migration:** Improved error handling for production
- **Basic Health Check:** Added self-check that doesn't depend on database

### 4. Fixed GitHub Actions Workflow

**File:** `.github/workflows/deploy.yml`
- Updated health check URL from `harmoni360-app.fly.dev` → `harmoni-hse-360.fly.dev`

## 🚀 Deployment Steps

### Step 1: Verify Configuration Files
```bash
# Ensure fly.toml exists with correct app name
cat fly.toml | grep "app ="
# Should show: app = "harmoni-hse-360"
```

### Step 2: Deploy Application
```bash
# Deploy using the corrected configuration
flyctl deploy --config fly.toml

# Monitor deployment logs
flyctl logs -f -a harmoni-hse-360
```

### Step 3: Verify Application Startup
```bash
# Check application status
flyctl status -a harmoni-hse-360

# Test health endpoint
curl https://harmoni-hse-360.fly.dev/health

# Check if app is listening on correct port
flyctl ssh console -a harmoni-hse-360 -C "netstat -tlnp | grep :8080"
```

### Step 4: Run Database Migrations (if needed)
```bash
# Connect to application and run migrations
flyctl ssh console -a harmoni-hse-360 -C "cd /app && dotnet ef database update"
```

## 🔧 Troubleshooting Commands

### Check Application Logs
```bash
# Real-time logs
flyctl logs -f -a harmoni-hse-360

# Filter for errors
flyctl logs -a harmoni-hse-360 | grep -i error

# Check startup logs
flyctl logs -a harmoni-hse-360 | grep "Starting Harmoni360"
```

### Verify Network Configuration
```bash
# Check machine status
flyctl machine list -a harmoni-hse-360

# Check service configuration
flyctl services list -a harmoni-hse-360

# Test internal connectivity
flyctl ssh console -a harmoni-hse-360 -C "curl -v http://localhost:8080/health"
```

### Debug Application Startup
```bash
# Connect to application console
flyctl ssh console -a harmoni-hse-360

# Check running processes
ps aux | grep dotnet

# Check port bindings
netstat -tlnp | grep :8080

# Check environment variables
env | grep ASPNETCORE
```

## 📋 Post-Deployment Verification Checklist

- [ ] Application status shows "running"
- [ ] Health endpoint returns HTTP 200
- [ ] Application logs show successful startup
- [ ] Database connection established (if configured)
- [ ] No critical errors in logs
- [ ] Application accessible via public URL

## 🔮 Prevention Measures

1. **Configuration Management**
   - Always maintain production `fly.toml` in repository
   - Use consistent app naming across all configuration files
   - Validate configuration files before deployment

2. **Docker Image Management**
   - Ensure Dockerfile versions match project target framework
   - Use specific version tags instead of latest
   - Test Docker builds locally before deployment

3. **Application Configuration**
   - Use environment-specific configurations
   - Implement graceful error handling for external dependencies
   - Add comprehensive health checks

4. **CI/CD Pipeline**
   - Validate configuration consistency in pipeline
   - Add pre-deployment health checks
   - Implement rollback procedures

## 📞 Support Resources

- **Fly.io Documentation:** https://fly.io/docs/
- **Fly.io Community:** https://community.fly.io/
- **Project Repository:** https://github.com/risky-biz/harmoni-hse-360
- **Internal Documentation:** [Troubleshooting Guide](./Troubleshooting.md)
