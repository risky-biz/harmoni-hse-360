# HarmoniHSE360 Comprehensive Troubleshooting Guide

This guide provides solutions for all deployment and CI/CD issues encountered with the HarmoniHSE360 application.

## Table of Contents

1. [Manual Deployment Issues](#manual-deployment-issues)
2. [CI/CD Pipeline Issues](#cicd-pipeline-issues)
3. [Monitoring and Maintenance](#monitoring-and-maintenance)
4. [Emergency Procedures](#emergency-procedures)
5. [Getting Help](#getting-help)

---

## Manual Deployment Issues

### 1. Application Won't Start

**Symptoms:**
- App shows as "crashed" in `fly status`
- Health checks failing
- Application not responding

**Diagnosis:**
```bash
# Check application logs
fly logs -a harmonihse360-app

# Check application status
fly status -a harmonihse360-app

# Check machine status
fly machine list -a harmonihse360-app
```

**Common Causes & Solutions:**

#### Missing Environment Variables
```bash
# Check current secrets
fly secrets list -a harmonihse360-app

# Set missing secrets
fly secrets set ConnectionStrings__DefaultConnection="your-db-connection" -a harmonihse360-app
fly secrets set ConnectionStrings__Redis="your-redis-connection" -a harmonihse360-app
fly secrets set Jwt__Key="your-jwt-key" -a harmonihse360-app
```

#### Database Connection Issues
```bash
# Test database connectivity
fly postgres connect -a harmonihse360-db

# Check database status
fly status -a harmonihse360-db

# Verify connection string format
# Should be: postgres://username:password@hostname:5432/database
```

#### Port Configuration Issues
Ensure your `fly.toml` has correct port configuration:
```toml
[[services]]
  internal_port = 8080  # Must match ASPNETCORE_URLS
  protocol = "tcp"
```

### 2. Database Migration Failures

**Symptoms:**
- Application starts but database operations fail
- "Table doesn't exist" errors
- Migration timeout errors

**Solutions:**

#### Manual Migration
```bash
# Connect to application
fly ssh console -a harmonihse360-app

# Run migrations manually
cd /app
dotnet ef database update --verbose
```

#### Reset Database (Development Only)
```bash
# Drop and recreate database
fly postgres connect -a harmonihse360-db
DROP DATABASE harmonihse360;
CREATE DATABASE harmonihse360;

# Run migrations again
fly ssh console -a harmonihse360-app -C "cd /app && dotnet ef database update"
```

### 3. Redis Connection Issues

**Symptoms:**
- SignalR not working
- Cache operations failing
- Redis connection timeouts

**Diagnosis:**
```bash
# Check Redis status
fly ext redis status harmonihse360-redis

# Test Redis connection
fly ssh console -a harmonihse360-app
redis-cli -u $ConnectionStrings__Redis ping
```

**Solutions:**

#### Verify Redis Configuration
```bash
# Check Redis connection string format
# Should be: redis://username:password@hostname:6379

# Update Redis connection if needed
fly secrets set ConnectionStrings__Redis="correct-redis-url" -a harmonihse360-app
```

### 4. Volume Mount Issues

**Symptoms:**
- File upload failures
- Permission denied errors
- Volume not accessible

**Diagnosis:**
```bash
# Check volume status
fly volumes list -a harmonihse360-app

# Check volume mounts
fly ssh console -a harmonihse360-app
ls -la /app/uploads
```

**Solutions:**

#### Fix Volume Permissions
```bash
fly ssh console -a harmonihse360-app
sudo chown -R appuser:appgroup /app/uploads
sudo chmod 755 /app/uploads
```

#### Recreate Volume
```bash
# Destroy old volume (WARNING: Data loss)
fly volumes destroy vol_xyz -a harmonihse360-app

# Create new volume
fly volumes create harmonihse360_uploads --region sjc --size 1 -a harmonihse360-app

# Redeploy application
fly deploy -a harmonihse360-app
```

### 5. SSL Certificate Issues

**Symptoms:**
- HTTPS not working
- Certificate warnings
- Domain not accessible

**Diagnosis:**
```bash
# Check certificate status
fly certs list -a harmonihse360-app

# Check certificate details
fly certs show yourdomain.com -a harmonihse360-app
```

**Solutions:**

#### Recreate Certificate
```bash
# Remove old certificate
fly certs remove yourdomain.com -a harmonihse360-app

# Create new certificate
fly certs create yourdomain.com -a harmonihse360-app

# Verify DNS configuration
nslookup yourdomain.com
```

### 6. Performance Issues

**Symptoms:**
- Slow response times
- High memory usage
- CPU throttling

**Diagnosis:**
```bash
# Check resource usage
fly metrics -a harmonihse360-app

# Check machine specifications
fly machine list -a harmonihse360-app

# Monitor real-time performance
fly logs -f -a harmonihse360-app
```

**Solutions:**

#### Scale Resources
```bash
# Increase memory
fly scale memory 1024 -a harmonihse360-app

# Add more instances
fly scale count 2 -a harmonihse360-app

# Use performance CPU
fly machine update --vm-cpu-kind performance -a harmonihse360-app
```

### 7. Build Failures

**Symptoms:**
- Docker build fails
- Deployment stuck at build stage
- Out of memory during build

**Solutions:**

#### Optimize Dockerfile
```dockerfile
# Use multi-stage builds efficiently
# Clear npm cache

RUN npm ci --legacy-peer-deps && npm cache clean --force

# Optimize .NET build
RUN dotnet publish -c Release --no-restore -o /app/publish
```

#### Local Build Test
```bash
# Test build locally
docker build -f Dockerfile.flyio -t harmonihse360:test .

# Test run locally
docker run -p 8080:8080 harmonihse360:test
```

## Monitoring and Debugging

### Real-time Monitoring
```bash
# Follow logs in real-time
fly logs -f -a harmonihse360-app

# Monitor specific machine
fly logs -f -a harmonihse360-app -i machine_id

# Filter logs by level
fly logs -f -a harmonihse360-app | grep ERROR
```

### Health Check Debugging
```bash
# Test health endpoint manually
curl https://harmonihse360-app.fly.dev/health

# Check health check configuration in fly.toml
[[services.http_checks]]
  interval = "10s"
  grace_period = "5s"
  method = "GET"
  path = "/health"
  protocol = "http"
  timeout = "2s"
```

### Database Debugging
```bash
# Connect to database
fly postgres connect -a harmonihse360-db

# Check database size
SELECT pg_size_pretty(pg_database_size('harmonihse360'));

# Check active connections
SELECT count(*) FROM pg_stat_activity;

# Check slow queries
SELECT query, mean_exec_time, calls 
FROM pg_stat_statements 
ORDER BY mean_exec_time DESC 
LIMIT 10;
```

## Recovery Procedures

### Application Recovery
```bash
# Restart application
fly restart -a harmonihse360-app

# Rollback to previous version
fly releases -a harmonihse360-app
fly rollback -a harmonihse360-app --version X

# Force redeploy
fly deploy --force -a harmonihse360-app
```

### Database Recovery
```bash
# List available backups
fly postgres backup list -a harmonihse360-db

# Restore from backup
fly postgres backup restore backup_id -a harmonihse360-db
```

### Complete Environment Reset
```bash
# WARNING: This will destroy all data
# Destroy application
fly apps destroy harmonihse360-app

# Destroy database
fly postgres destroy harmonihse360-db

# Destroy Redis
fly ext redis destroy harmonihse360-redis

# Redeploy from scratch
./scripts/deploy-flyio.sh
```

## Getting Help

### Fly.io Support Channels
- **Community Forum:** https://community.fly.io/
- **Documentation:** https://fly.io/docs/
- **Status Page:** https://status.fly.io/

### HarmoniHSE360 Support
- **Repository Issues:** https://github.com/risky-biz/harmoni-hse-360/issues
- **Documentation:** docs/README.md

### Useful Commands Reference
```bash
# Application management
fly status -a harmonihse360-app
fly logs -a harmonihse360-app
fly ssh console -a harmonihse360-app
fly restart -a harmonihse360-app

# Database management
fly postgres connect -a harmonihse360-db
fly postgres backup create -a harmonihse360-db
fly postgres backup list -a harmonihse360-db

# Secrets management
fly secrets list -a harmonihse360-app
fly secrets set KEY=value -a harmonihse360-app
fly secrets unset KEY -a harmonihse360-app

# Scaling
fly scale count 2 -a harmonihse360-app
fly scale memory 1024 -a harmonihse360-app
fly scale show -a harmonihse360-app
```

---

## CI/CD Pipeline Issues

### Build and Test Failures

#### .NET Build Failures

**Symptoms:**
- Compilation errors in GitHub Actions
- Missing dependencies
- Version conflicts

**Common Solutions:**

```bash
# Check .NET version compatibility
dotnet --version
dotnet --list-sdks

# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose logging
dotnet restore --verbosity detailed
```

**Workflow Debugging:**
```yaml
- name: Debug .NET build
  run: |
    echo "Current directory: $(pwd)"
    echo "Available .NET SDKs:"
    dotnet --list-sdks
    echo "Project files:"
    find . -name "*.csproj" -o -name "*.sln"
    dotnet restore --verbosity detailed
```

#### React/TypeScript Build Failures

**Symptoms:**
- npm install failures
- TypeScript compilation errors
- Missing environment variables

**Common Solutions:**

```bash
# Clear npm cache
npm cache clean --force

# Install with exact versions
npm ci --legacy-peer-deps --prefer-offline

# Check Node.js version
node --version
npm --version
```

#### Test Failures

**Symptoms:**
- Unit tests failing in CI but passing locally
- Database connection issues
- Timeout errors

**Solutions:**

```yaml
# Add test debugging
- name: Run tests with debugging
  run: |
    dotnet test --logger "console;verbosity=detailed" \
      --collect:"XPlat Code Coverage" \
      -- TestRunParameters.Parameter\(name=\"ConnectionString\",value=\"${{ env.TEST_CONNECTION_STRING }}\"\)
  env:
    ASPNETCORE_ENVIRONMENT: Testing
    ConnectionStrings__DefaultConnection: "Host=localhost;Port=5432;Database=harmonihse360_test;Username=postgres;Password=postgres"
```

### Security Scan Issues

#### Trivy Scanner Failures

**Symptoms:**
- Trivy scan timeouts
- False positive vulnerabilities
- SARIF upload failures

**Solutions:**

```yaml
# Configure Trivy with custom settings
- name: Run Trivy with custom config
  uses: aquasecurity/trivy-action@master
  with:
    scan-type: 'fs'
    scan-ref: '.'
    format: 'sarif'
    output: 'trivy-results.sarif'
    severity: 'CRITICAL,HIGH'
    timeout: '10m'
    ignore-unfixed: true
```

#### npm Audit Issues

**Symptoms:**
- High severity vulnerabilities in dependencies
- Audit failures blocking deployment

**Solutions:**

```bash
# Create audit exceptions file
echo '{"advisories": ["GHSA-xxxx-xxxx-xxxx"]}' > .nsprc

# Use audit-ci with allowlist
npm install -g audit-ci
audit-ci --config audit-ci.json
```

### Docker Build Problems

#### Build Context Issues

**Symptoms:**
- Files not found during Docker build
- Large build context
- Build timeouts

**Solutions:**

```dockerfile
# Optimize Dockerfile.flyio
# Use .dockerignore to reduce context size
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy only necessary files first
COPY ["src/HarmoniHSE360.Web/HarmoniHSE360.Web.csproj", "HarmoniHSE360.Web/"]
RUN dotnet restore "HarmoniHSE360.Web/HarmoniHSE360.Web.csproj"

# Then copy everything else
COPY src/ .
```

**Create `.dockerignore`:**
```
.git
.github
docs/
TestResults/
**/node_modules
**/coverage
**/.env
**/bin
**/obj
```

### Deployment Failures

#### Fly.io Deployment Issues

**Symptoms:**
- Deployment timeouts
- Health check failures
- Resource allocation errors

**Debugging Steps:**

```bash
# Check Fly.io app status
fly status -a harmonihse360-app

# View deployment logs
fly logs -a harmonihse360-app

# Check machine status
fly machine list -a harmonihse360-app

# Manual deployment test
fly deploy --config fly.toml --verbose
```

**Common Solutions:**

```yaml
# Add deployment retry logic
- name: Deploy with retry
  uses: nick-invision/retry@v2
  with:
    timeout_minutes: 10
    max_attempts: 3
    command: flyctl deploy --config fly.toml
  env:
    FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}
```

### Secret Management Problems

#### Token Rotation

**Automated token rotation:**

```bash
#!/bin/bash
# rotate-tokens.sh

# Generate new Fly.io token
NEW_TOKEN=$(fly tokens create deploy -x 999999h --name "github-actions-$(date +%Y%m%d)")

# Update GitHub secret
gh secret set FLY_API_TOKEN --body "$NEW_TOKEN"

# Revoke old token (manual step)
echo "Remember to revoke old tokens in Fly.io dashboard"
```

#### Secret Validation

```yaml
# Validate secrets in workflow
- name: Validate secrets
  run: |
    if [ -z "${{ secrets.FLY_API_TOKEN }}" ]; then
      echo "Error: FLY_API_TOKEN secret is not set"
      exit 1
    fi

    # Test token validity
    flyctl auth whoami
  env:
    FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}
```

---

## Monitoring and Maintenance

### Pipeline Health Monitoring

#### Key Metrics to Monitor

| Metric | Target | Alert Threshold | Action Required |
|--------|--------|-----------------|-----------------|
| Build Success Rate | >95% | <90% | Investigate failures |
| Average Build Time | <15 min | >20 min | Optimize pipeline |
| Deployment Success Rate | >98% | <95% | Review deployment process |
| Test Coverage | >80% | <75% | Improve test coverage |
| Security Scan Pass Rate | 100% | <100% | Address vulnerabilities |

#### Health Check Commands

```bash
# View workflow runs
gh run list --workflow=deploy.yml --limit=50 --json status,conclusion,createdAt,updatedAt

# Check pipeline health
gh run list --workflow=deploy.yml --limit=20 --json status,conclusion | \
  jq -r '.[] | "\(.status): \(.conclusion)"' | sort | uniq -c

# Check security alerts
gh api repos/risky-biz/harmoni-hse-360/code-scanning/alerts | \
  jq -r '.[] | "\(.rule.security_severity_level): \(.rule.description)"' | \
  sort | uniq -c
```

### Automated Maintenance Tasks

#### Weekly Tasks (Every Monday)
- [ ] Review pipeline success rates
- [ ] Check security scan results
- [ ] Review dependency updates
- [ ] Validate backup procedures

#### Monthly Tasks (First Monday)
- [ ] Rotate API tokens
- [ ] Review and update security policies
- [ ] Audit user access and permissions
- [ ] Update documentation

#### Quarterly Tasks
- [ ] Comprehensive security audit
- [ ] Disaster recovery testing
- [ ] Pipeline architecture review
- [ ] Cost optimization analysis

---

## Emergency Procedures

### Rollback Deployment

```bash
# Quick rollback using Fly.io
fly releases -a harmonihse360-app
fly rollback -a harmonihse360-app --version <previous-version>
```

### Disable Workflow

```bash
# Disable workflow temporarily
gh workflow disable deploy.yml

# Re-enable when ready
gh workflow enable deploy.yml
```

### Manual Deployment Bypass

```bash
# Manual deployment bypass
fly deploy --config fly.toml --image ghcr.io/risky-biz/harmoni-hse-360:latest
```

### Complete Environment Reset

```bash
# WARNING: This will destroy all data
# Destroy application
fly apps destroy harmonihse360-app

# Destroy database
fly postgres destroy harmonihse360-db

# Destroy Redis
fly ext redis destroy harmonihse360-redis

# Redeploy from scratch
./scripts/deploy-flyio.sh
```

---

*Last Updated: January 2025*
*Version: 2.0 - Consolidated Guide*
