# Fly.io Deployment Troubleshooting Guide

## Overview

This comprehensive troubleshooting guide covers common issues, diagnostic procedures, and solutions for HarmoniHSE360 deployments on Fly.io. Use this guide to quickly identify and resolve deployment, runtime, and performance issues.

## 🔍 Quick Diagnostic Commands

### Essential Fly.io Commands
```bash
# Check application status
flyctl status -a harmoni360-app

# View recent logs
flyctl logs -a harmoni360-app --since 1h

# Stream live logs
flyctl logs -a harmoni360-app -f

# Check application configuration
flyctl config show -a harmoni360-app

# List secrets (names only)
flyctl secrets list -a harmoni360-app

# Check database status
flyctl status -a harmoni360-db

# Connect to application console
flyctl ssh console -a harmoni360-app

# Check application metrics
flyctl metrics -a harmoni360-app
```

### Health Check Commands
```bash
# Test application health endpoint
curl -f https://harmoni360-app.fly.dev/health

# Test API endpoints
curl -f https://harmoni360-app.fly.dev/api/health

# Check SSL certificate
curl -I https://harmoni360-app.fly.dev

# Test database connectivity
flyctl ssh console -a harmoni360-app -C "cd /app && dotnet ef database update --dry-run"
```

## 🚨 Common Issues and Solutions

### 1. Deployment Failures

#### Issue: Build Failures
**Symptoms:**
- Deployment fails during Docker build
- "No space left on device" errors
- Package installation failures

**Diagnostic Commands:**
```bash
# Check build logs
flyctl logs -a harmoni360-app | grep -i error

# Check disk usage during build
flyctl ssh console -a harmoni360-app -C "df -h"
```

**Solutions:**
```bash
# Clear Docker build cache
flyctl deploy --no-cache -a harmoni360-app

# Use smaller base image
# Edit Dockerfile.flyio to use alpine variants

# Optimize Dockerfile layers
# Combine RUN commands to reduce layers
```

#### Issue: Database Migration Failures
**Symptoms:**
- Application starts but database errors occur
- "Cannot connect to database" errors
- Migration timeout errors

**Diagnostic Commands:**
```bash
# Check database status
flyctl status -a harmoni360-db

# Test database connectivity
flyctl postgres connect -a harmoni360-db

# Check migration logs
flyctl ssh console -a harmoni360-app -C "cd /app && dotnet ef database update --verbose"
```

**Solutions:**
```bash
# Manually run migrations
flyctl ssh console -a harmoni360-app -C "cd /app && dotnet ef database update"

# Reset database if needed (CAUTION: Data loss)
flyctl postgres create --name harmoni360-db-new --region sjc
# Update connection string and redeploy

# Check database connection string
flyctl secrets list -a harmoni360-app | grep ConnectionStrings
```

### 2. Runtime Issues

#### Issue: Application Won't Start
**Symptoms:**
- Application shows as "stopped" or "crashed"
- Health checks failing
- 502/503 errors from load balancer

**Diagnostic Commands:**
```bash
# Check application status
flyctl status -a harmoni360-app

# View startup logs
flyctl logs -a harmoni360-app --since 10m

# Check process status
flyctl ssh console -a harmoni360-app -C "ps aux"
```

**Solutions:**
```bash
# Restart application
flyctl restart -a harmoni360-app

# Check environment variables
flyctl ssh console -a harmoni360-app -C "env | grep ASPNETCORE"

# Verify secrets are set
flyctl secrets list -a harmoni360-app

# Scale up if resource constrained
flyctl scale memory 1024 -a harmoni360-app
```

#### Issue: Database Connection Errors
**Symptoms:**
- "Connection timeout" errors
- "Authentication failed" errors
- Intermittent database connectivity

**Diagnostic Commands:**
```bash
# Test database from application
flyctl ssh console -a harmoni360-app -C "cd /app && dotnet ef database update --dry-run"

# Check database logs
flyctl logs -a harmoni360-db

# Test direct database connection
flyctl postgres connect -a harmoni360-db
```

**Solutions:**
```bash
# Verify connection string format
# Should be: Host=harmoni360-db.internal;Port=5432;Database=Harmoni360_Prod;Username=harmoni360;Password=xxx

# Ensure your Fly Postgres cluster was created with this database name and user.

# Update connection string
flyctl secrets set ConnectionStrings__DefaultConnection="Host=harmoni360-db.internal;Port=5432;Database=Harmoni360_Prod;Username=harmoni360;Password=YOUR_PASSWORD" -a harmoni360-app

# Restart application after secret update
flyctl restart -a harmoni360-app

# Check database resource usage
flyctl metrics -a harmoni360-db
```

### 3. Performance Issues

#### Issue: Slow Response Times
**Symptoms:**
- High response times (>3 seconds)
- Timeouts on API calls
- Poor user experience

**Diagnostic Commands:**
```bash
# Check application metrics
flyctl metrics -a harmoni360-app

# Monitor resource usage
flyctl ssh console -a harmoni360-app -C "top"

# Check database performance
flyctl metrics -a harmoni360-db

# Test response times
curl -w "@curl-format.txt" -o /dev/null -s https://harmoni360-app.fly.dev/health
```

**Solutions:**
```bash
# Scale up CPU/memory
flyctl scale memory 2048 -a harmoni360-app
flyctl scale vm shared-cpu-2x -a harmoni360-app

# Add more instances
flyctl scale count 2 -a harmoni360-app

# Optimize database
flyctl postgres connect -a harmoni360-db
# Run: VACUUM ANALYZE;

# Enable Redis caching
flyctl redis create --name harmoni360-cache --region sjc
# Update Redis connection string
```

#### Issue: High Memory Usage
**Symptoms:**
- Out of memory errors
- Application restarts frequently
- Slow garbage collection

**Diagnostic Commands:**
```bash
# Check memory usage
flyctl ssh console -a harmoni360-app -C "free -h"

# Monitor .NET memory
flyctl ssh console -a harmoni360-app -C "cd /app && dotnet-counters monitor --process-id \$(pgrep dotnet)"

# Check for memory leaks
flyctl logs -a harmoni360-app | grep -i "memory\|gc\|heap"
```

**Solutions:**
```bash
# Increase memory allocation
flyctl scale memory 2048 -a harmoni360-app

# Optimize .NET garbage collection
# Add to fly.toml:
# [env]
# DOTNET_gcServer = "1"
# DOTNET_gcConcurrent = "1"

# Review application code for memory leaks
# Check for unclosed database connections
# Review caching strategies
```

### 4. SSL/TLS Issues

#### Issue: SSL Certificate Problems
**Symptoms:**
- "Certificate not trusted" warnings
- SSL handshake failures
- Mixed content warnings

**Diagnostic Commands:**
```bash
# Check certificate status
curl -I https://harmoni360-app.fly.dev

# Test SSL configuration
openssl s_client -connect harmoni360-app.fly.dev:443

# Check certificate details
flyctl certs list -a harmoni360-app
```

**Solutions:**
```bash
# Add custom domain certificate
flyctl certs create your-domain.com -a harmoni360-app

# Check DNS configuration
dig your-domain.com
dig AAAA your-domain.com

# Force HTTPS in application
# Update fly.toml:
# [[services.ports]]
# handlers = ["http"]
# port = 80
# force_https = true
```

### 5. CI/CD Pipeline Issues

#### Issue: GitHub Actions Deployment Failures
**Symptoms:**
- Deployment workflow fails
- Authentication errors
- Build timeouts

**Diagnostic Commands:**
```bash
# Check workflow status
gh run list --workflow=deploy.yml

# View workflow logs
gh run view [run-id] --log

# Test Fly.io token
flyctl auth whoami
```

**Solutions:**
```bash
# Regenerate Fly.io API token
flyctl auth token

# Update GitHub secret
gh secret set FLY_API_TOKEN --body "your-new-token"

# Check workflow permissions
# Ensure FLY_API_TOKEN has deploy permissions

# Retry failed deployment
gh workflow run deploy.yml --ref main
```

## 🔧 Advanced Troubleshooting

### Database Performance Optimization
```bash
# Connect to database
flyctl postgres connect -a harmoni360-db

# Check database size
SELECT pg_size_pretty(pg_database_size('harmoni360'));

# Check table sizes
SELECT schemaname,tablename,pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as size
FROM pg_tables
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

# Check slow queries
SELECT query, mean_time, calls
FROM pg_stat_statements
ORDER BY mean_time DESC
LIMIT 10;

# Optimize database
VACUUM ANALYZE;
REINDEX DATABASE harmoni360;
```

### Application Performance Monitoring
```bash
# Monitor .NET performance counters
flyctl ssh console -a harmoni360-app -C "cd /app && dotnet-counters monitor --process-id \$(pgrep dotnet) --counters System.Runtime,Microsoft.AspNetCore.Hosting"

# Check garbage collection
flyctl ssh console -a harmoni360-app -C "cd /app && dotnet-gcdump collect --process-id \$(pgrep dotnet)"

# Monitor HTTP requests
flyctl logs -a harmoni360-app | grep -E "(GET|POST|PUT|DELETE)"
```

### Network Connectivity Testing
```bash
# Test internal connectivity
flyctl ssh console -a harmoni360-app -C "ping harmoni360-db.internal"

# Test external connectivity
flyctl ssh console -a harmoni360-app -C "curl -I https://api.github.com"

# Check DNS resolution
flyctl ssh console -a harmoni360-app -C "nslookup harmoni360-db.internal"
```

## 📊 Monitoring and Alerting

### Setting Up Monitoring
```bash
# Enable application metrics
# Add to appsettings.json:
{
  "Metrics": {
    "Enabled": true,
    "Endpoint": "/metrics"
  }
}

# Monitor with external services
# Configure Uptime Robot, Pingdom, or similar
curl -f https://harmoni360-app.fly.dev/health
```

### Log Analysis
```bash
# Search for specific errors
flyctl logs -a harmoni360-app | grep -i "exception\|error\|fail"

# Monitor response times
flyctl logs -a harmoni360-app | grep "RequestFinished" | tail -20

# Check authentication issues
flyctl logs -a harmoni360-app | grep -i "auth\|login\|token"
```

## 🆘 Emergency Procedures

### Application Down
```bash
# 1. Check application status
flyctl status -a harmoni360-app

# 2. Check recent logs for errors
flyctl logs -a harmoni360-app --since 30m | grep -i error

# 3. Restart application
flyctl restart -a harmoni360-app

# 4. If restart fails, scale down and up
flyctl scale count 0 -a harmoni360-app
flyctl scale count 1 -a harmoni360-app

# 5. If still failing, rollback to previous version
flyctl releases -a harmoni360-app
flyctl releases rollback [version] -a harmoni360-app
```

### Database Issues
```bash
# 1. Check database status
flyctl status -a harmoni360-db

# 2. Test connectivity
flyctl postgres connect -a harmoni360-db

# 3. Check database logs
flyctl logs -a harmoni360-db

# 4. If unresponsive, restart database
flyctl restart -a harmoni360-db

# 5. If data corruption suspected, restore from backup
flyctl postgres backup list -a harmoni360-db
flyctl postgres backup restore [backup-id] -a harmoni360-db
```

### Complete System Recovery
```bash
# 1. Create new application
flyctl apps create harmoni360-app-recovery

# 2. Restore database from backup
flyctl postgres backup restore [backup-id] -a harmoni360-db-recovery

# 3. Update DNS to point to recovery app
# Update A/AAAA records

# 4. Deploy application to recovery environment
flyctl deploy -a harmoni360-app-recovery

# 5. Verify functionality
curl -f https://harmoni360-app-recovery.fly.dev/health
```

## 📞 Getting Help

### Fly.io Support Channels
- **Community Forum**: https://community.fly.io/
- **Documentation**: https://fly.io/docs/
- **Status Page**: https://status.fly.io/
- **Support Email**: support@fly.io (for paid plans)

### Internal Resources
- [Getting Started Guide](./Getting_Started.md)
- [Environment Management](./Environment_Management.md)
- [CI/CD Integration](./CI_CD_Integration.md)
- [Main Deployment README](../README.md)

### Emergency Contacts
- **Technical Lead**: [Contact information]
- **DevOps Team**: [Contact information]
- **On-call Engineer**: [Contact information]

---

*This troubleshooting guide is part of the comprehensive HarmoniHSE360 Fly.io deployment documentation.*
