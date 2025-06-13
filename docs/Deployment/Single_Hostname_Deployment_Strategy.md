# Single Hostname Deployment Strategy

## 🎯 Overview

This document outlines the deployment strategy for Harmoni360 using a single Fly.io hostname (`harmoni-hse-360.fly.dev`) while maintaining the ability to test staging changes before production deployment.

## 🏗️ Architecture Decision

### **Single App, Environment-Based Deployment**

Instead of maintaining separate Fly.io apps for staging and production, we use:
- **Single Fly.io App**: `harmoni-hse-360`
- **Single Hostname**: `harmoni-hse-360.fly.dev`
- **Environment Control**: Via `ASPNETCORE_ENVIRONMENT` secret/variable
- **Deployment Separation**: Through GitHub Actions workflow branches

### **Why This Approach?**

1. **Hostname Constraint**: Only one hostname available
2. **Cost Efficiency**: Single app reduces resource costs
3. **Simplified Management**: One app to monitor and maintain
4. **Environment Flexibility**: Easy switching between staging and production
5. **Testing Capability**: Still allows pre-production testing

## 🔄 Deployment Workflow

### **Staging Deployment (develop branch)**
```mermaid
graph LR
    A[Push to develop] --> B[Build & Test]
    B --> C[Deploy with Staging Env]
    C --> D[Set ASPNETCORE_ENVIRONMENT=Staging]
    D --> E[Run Migrations]
    E --> F[Health Check]
    F --> G[Notify Success/Failure]
```

### **Production Deployment (main branch)**
```mermaid
graph LR
    A[Push to main] --> B[Build & Test]
    B --> C[Deploy with Production Env]
    C --> D[Set ASPNETCORE_ENVIRONMENT=Production]
    D --> E[Run Migrations]
    E --> F[Health Check]
    F --> G[Notify Success/Failure]
```

## 🚀 Deployment Commands

### **Staging Deployment**
```bash
# Automatic (via GitHub Actions on develop branch push)
git push origin develop

# Manual staging deployment
flyctl deploy --config fly.toml --env ASPNETCORE_ENVIRONMENT=Staging
flyctl secrets set ASPNETCORE_ENVIRONMENT="Staging" -a harmoni-hse-360

# Verify staging deployment
curl https://harmoni-hse-360.fly.dev/health
flyctl logs -a harmoni-hse-360 | grep "Environment: Staging"
```

### **Production Deployment**
```bash
# Automatic (via GitHub Actions on main branch push)
git push origin main

# Manual production deployment
flyctl deploy --config fly.toml --env ASPNETCORE_ENVIRONMENT=Production
flyctl secrets set ASPNETCORE_ENVIRONMENT="Production" -a harmoni-hse-360

# Verify production deployment
curl https://harmoni-hse-360.fly.dev/health
flyctl logs -a harmoni-hse-360 | grep "Environment: Production"
```

## 🔧 Configuration Management

### **Single fly.toml Configuration**
```toml
app = "harmoni-hse-360"
primary_region = "sjc"

[build]
  dockerfile = "Dockerfile.flyio"

[env]
  # Environment controlled via secrets during deployment
  ASPNETCORE_URLS = "http://+:8080"
  ASPNETCORE_FORWARDEDHEADERS_ENABLED = "true"

[[services]]
  internal_port = 8080
  protocol = "tcp"
  auto_stop_machines = false
  auto_start_machines = true
  min_machines_running = 1

[vm]
  cpu_kind = "shared"
  cpus = 1
  memory_mb = 1024
```

### **Environment Detection in Application**
The application automatically detects the environment via:
```csharp
// In Program.cs
var environment = app.Environment.EnvironmentName;
logger.LogInformation("Running in {Environment} environment", environment);
```

## 🧪 Testing Strategy

### **Pre-Production Testing Process**

1. **Feature Development**
   ```bash
   # Work on feature branch
   git checkout -b feature/new-feature
   # ... make changes ...
   git push origin feature/new-feature
   ```

2. **Staging Testing**
   ```bash
   # Merge to develop for staging deployment
   git checkout develop
   git merge feature/new-feature
   git push origin develop
   # Automatic staging deployment triggers
   ```

3. **Staging Verification**
   ```bash
   # Test staging environment
   curl https://harmoni-hse-360.fly.dev/health
   # Manual testing on https://harmoni-hse-360.fly.dev
   ```

4. **Production Deployment**
   ```bash
   # Merge to main for production deployment
   git checkout main
   git merge develop
   git push origin main
   # Automatic production deployment triggers
   ```

### **Environment Verification Commands**
```bash
# Check current environment
flyctl ssh console -a harmoni-hse-360 -C "echo \$ASPNETCORE_ENVIRONMENT"

# Check application logs for environment
flyctl logs -a harmoni-hse-360 | grep "Environment:"

# Check application configuration
flyctl ssh console -a harmoni-hse-360 -C "cd /app && dotnet --info"
```

## 🔍 Monitoring and Verification

### **Environment Status Check**
```bash
# Application status
flyctl status -a harmoni-hse-360

# Current environment
flyctl secrets list -a harmoni-hse-360 | grep ASPNETCORE_ENVIRONMENT

# Health check
curl https://harmoni-hse-360.fly.dev/health

# Application logs
flyctl logs -f -a harmoni-hse-360
```

### **Deployment Verification Checklist**
- [ ] Application starts successfully
- [ ] Correct environment detected in logs
- [ ] Health check returns HTTP 200
- [ ] Database migrations applied
- [ ] No critical errors in logs
- [ ] Application accessible via https://harmoni-hse-360.fly.dev

## ⚠️ Important Considerations

### **Deployment Timing**
- **Staging and Production share the same URL**
- **Only one environment can be active at a time**
- **Plan deployments to minimize disruption**
- **Use maintenance windows for major changes**

### **Data Management**
- **Database is shared between environments**
- **Use environment-specific configuration for external services**
- **Consider data seeding strategies for staging**

### **Rollback Strategy**
```bash
# Quick rollback to previous deployment
flyctl releases list -a harmoni-hse-360
flyctl releases rollback <release-id> -a harmoni-hse-360

# Emergency rollback
flyctl deploy --config fly.toml --env ASPNETCORE_ENVIRONMENT=Production
```

## 🔄 Migration from Dual-App Setup

### **Cleanup Steps**
1. **Remove staging-specific configurations**
   - ✅ Removed `fly.staging.toml`
   - ✅ Updated GitHub Actions workflow
   - ✅ Updated documentation

2. **Update validation scripts**
   - ✅ Modified to check single app configuration
   - ✅ Updated health check URLs

3. **Update team processes**
   - ✅ New deployment workflow documented
   - ✅ Environment verification procedures

## 📋 Best Practices

### **Development Workflow**
1. **Always test in staging first** (develop branch)
2. **Verify staging deployment** before promoting to production
3. **Use feature branches** for development
4. **Monitor logs** during and after deployments
5. **Plan deployments** during low-traffic periods

### **Environment Management**
1. **Use environment-specific secrets** for sensitive configuration
2. **Monitor resource usage** and adjust VM configuration as needed
3. **Implement proper logging** to distinguish between environments
4. **Use health checks** to verify deployment success

## 📞 Support and Troubleshooting

### **Common Issues**
- **Wrong environment deployed**: Check `ASPNETCORE_ENVIRONMENT` secret
- **Health check failures**: Verify application startup logs
- **Database issues**: Check migration status and connection strings

### **Support Commands**
```bash
# Debug environment
flyctl ssh console -a harmoni-hse-360

# Check secrets
flyctl secrets list -a harmoni-hse-360

# Monitor real-time logs
flyctl logs -f -a harmoni-hse-360

# Application shell access
flyctl ssh console -a harmoni-hse-360 -C "bash"
```

This single-hostname strategy maintains testing capabilities while working within the constraint of one available hostname, providing a robust and cost-effective deployment solution.
