# Fly.io Single-App Environment Configuration

## Overview

This document describes the single-app Fly.io configuration for the Harmoni360 application, using one hostname (`harmoni-hse-360.fly.dev`) with environment-based deployment strategy for both staging and production.

## 🔧 Applied Fixes Summary

### Common Fixes Applied to Both Environments:

1. **Docker Configuration**
   - ✅ Updated to .NET 8.0 images (matching project target framework)
   - ✅ Proper port exposure and environment variables

2. **Application Configuration**
   - ✅ Added `ASPNETCORE_FORWARDEDHEADERS_ENABLED = "true"`
   - ✅ Maintained `ASPNETCORE_URLS = "http://+:8080"`
   - ✅ Conditional HTTPS redirection in Program.cs

3. **Health Check Configuration**
   - ✅ Improved timeout and grace period settings
   - ✅ Consistent health check endpoint (`/health`)

4. **Service Configuration**
   - ✅ Auto-scaling machine management
   - ✅ Restart policies for stability

## 📊 Configuration Comparison

### Single App Configuration
| Setting | Value | Notes |
|---------|-------|-------|
| **App Name** | `harmoni-hse-360` | Single app for both environments |
| **URL** | `harmoni-hse-360.fly.dev` | Single hostname constraint |
| **Region** | `sjc` | San Jose region |
| **Environment Control** | Via `ASPNETCORE_ENVIRONMENT` secret | Staging/Production switching |

### Environment Variables
| Variable | Staging | Production |
|----------|---------|------------|
| **ASPNETCORE_ENVIRONMENT** | `Staging` | `Production` |
| **ASPNETCORE_URLS** | `http://+:8080` | `http://+:8080` |
| **ASPNETCORE_FORWARDEDHEADERS_ENABLED** | `true` | `true` |

### Resource Allocation
| Resource | Staging | Production | Rationale |
|----------|---------|------------|-----------|
| **CPU** | 1 shared | 1 shared | Cost optimization |
| **Memory** | 512MB | 1024MB | Production needs more memory |
| **Min Machines** | 0 | 1 | Staging can scale to zero |
| **Auto Stop** | true | false | Staging saves costs when idle |

### Health Check Configuration
| Setting | Staging | Production | Rationale |
|---------|---------|------------|-----------|
| **Interval** | 15s | 10s | Staging less frequent checks |
| **Grace Period** | 10s | 10s | Consistent startup time |
| **Timeout** | 5s | 5s | Consistent response time |
| **Path** | `/health` | `/health` | Same endpoint |

### Concurrency Limits
| Setting | Staging | Production | Rationale |
|---------|---------|------------|-----------|
| **Hard Limit** | 25 | 50 | Production handles more load |
| **Soft Limit** | 20 | 40 | Proportional to hard limit |

### Storage Configuration
| Setting | Staging | Production |
|---------|---------|------------|
| **Volume Name** | `harmoni360_staging_uploads` | `harmoni360_uploads` |
| **Mount Path** | `/app/uploads` | `/app/uploads` |

### Restart Policies
| Setting | Staging | Production | Rationale |
|---------|---------|------------|-----------|
| **Policy** | `on-failure` | `always` | Production needs higher availability |

## 🚀 Deployment Pipeline Comparison

### GitHub Actions Workflow

#### Staging Deployment
```yaml
# Triggered by: develop branch, manual dispatch
- Deploy: flyctl deploy --config fly.staging.toml
- Migrations: flyctl ssh console --config fly.staging.toml
- Health Check: https://harmoni360-staging.fly.dev/health
- Notifications: Slack alerts for success/failure
```

#### Production Deployment
```yaml
# Triggered by: main branch, manual dispatch
- Deploy: flyctl deploy --config fly.toml
- Migrations: flyctl ssh console --config fly.toml
- Health Check: https://harmoni-hse-360.fly.dev/health
- Notifications: Slack alerts for success/failure
```

## 🔍 Environment-Specific Differences

### Intentional Differences (Maintained)

1. **Resource Allocation**
   - Staging: 512MB RAM (cost optimization)
   - Production: 1024MB RAM (performance optimization)

2. **Auto-Scaling Behavior**
   - Staging: Can scale to zero machines (cost savings)
   - Production: Minimum 1 machine always running (availability)

3. **Health Check Frequency**
   - Staging: 15-second intervals (reduced monitoring overhead)
   - Production: 10-second intervals (faster issue detection)

4. **Restart Policies**
   - Staging: Restart only on failure
   - Production: Always restart for maximum uptime

### Consistent Configurations (Applied to Both)

1. **Docker Images**: Both use .NET 8.0
2. **Port Configuration**: Both use 8080 internally
3. **Health Check Endpoint**: Both use `/health`
4. **Environment Headers**: Both enable forwarded headers
5. **Application Startup**: Both use improved resilient startup logic

## 📋 Validation Checklist

### Pre-Deployment Validation
- [ ] Staging fly.staging.toml has correct app name
- [ ] Production fly.toml has correct app name
- [ ] Both configurations use .NET 8.0 Docker images
- [ ] Health check URLs match app names in GitHub Actions
- [ ] Environment-specific resource limits are appropriate
- [ ] Auto-scaling settings match environment requirements

### Post-Deployment Validation
- [ ] Staging health check: `curl https://harmoni360-staging.fly.dev/health`
- [ ] Production health check: `curl https://harmoni-hse-360.fly.dev/health`
- [ ] Both applications start without "not listening" errors
- [ ] Database migrations run successfully in both environments
- [ ] Slack notifications work for both environments

## 🛠️ Troubleshooting Commands

### Staging Environment
```bash
# Status check
flyctl status -a harmoni360-staging

# Logs
flyctl logs -f -a harmoni360-staging

# Health check
curl https://harmoni360-staging.fly.dev/health

# SSH access
flyctl ssh console -a harmoni360-staging
```

### Production Environment
```bash
# Status check
flyctl status -a harmoni-hse-360

# Logs
flyctl logs -f -a harmoni-hse-360

# Health check
curl https://harmoni-hse-360.fly.dev/health

# SSH access
flyctl ssh console -a harmoni-hse-360
```

## 📈 Monitoring and Maintenance

### Environment-Specific Monitoring

1. **Staging**
   - Monitor for auto-scaling behavior
   - Validate cost optimization (machines scaling to zero)
   - Test deployment pipeline changes

2. **Production**
   - Monitor uptime and availability
   - Track performance metrics
   - Ensure minimum machine count maintained

### Shared Monitoring
- Health check response times
- Application startup logs
- Database migration success
- Resource utilization trends

## 🔄 Future Updates

When making configuration changes:

1. **Apply architectural fixes to both environments**
2. **Maintain environment-specific optimizations**
3. **Update this comparison document**
4. **Test changes in staging before production**
5. **Validate with updated validation scripts**

## 📞 Support Resources

- **Staging App**: https://fly.io/apps/harmoni360-staging
- **Production App**: https://fly.io/apps/harmoni-hse-360
- **Fly.io Documentation**: https://fly.io/docs/
- **Internal Troubleshooting**: [Flyio_Deployment_Fix_Analysis.md](./Flyio_Deployment_Fix_Analysis.md)
