# Environment Management for Fly.io Deployment

## Overview

This guide covers comprehensive environment management for HarmoniHSE360 deployments on Fly.io, including secrets management, configuration strategies, environment-specific settings, and best practices for maintaining secure and scalable deployments.

## 🏗️ Environment Architecture

### Environment Strategy

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Development   │    │     Staging     │    │   Production    │
│   (Local)       │───►│   (fly.dev)     │───►│   (fly.dev)     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Docker Compose  │    │ harmoni360-stg  │    │ harmoni360-app  │
│ Local Database  │    │ Shared Resources│    │ Dedicated Res.  │
│ Demo Data       │    │ Test Data       │    │ Production Data │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Environment Characteristics

| Environment | Purpose | Data | Resources | Monitoring |
|-------------|---------|------|-----------|------------|
| **Development** | Local development | Demo/Test | Minimal | Basic |
| **Staging** | Testing, demos | Sample/Test | Shared | Standard |
| **Production** | Live application | Real | Dedicated | Comprehensive |

## 🔐 Secrets Management

### 1. Fly.io Secrets Overview

Fly.io provides secure secret management through encrypted environment variables that are injected into your application at runtime.

```bash
# List current secrets (names only, values hidden)
flyctl secrets list -a harmoni360-app

# Set a secret
flyctl secrets set SECRET_NAME="secret_value" -a harmoni360-app

# Remove a secret
flyctl secrets unset SECRET_NAME -a harmoni360-app

# Import secrets from file
flyctl secrets import -a harmoni360-app < secrets.env
```

### 2. Required Secrets Configuration

#### Database Configuration
```bash
# PostgreSQL connection string
flyctl secrets set ConnectionStrings__DefaultConnection="Host=harmoni360-db.internal;Port=5432;Database=harmoni360;Username=postgres;Password=YOUR_DB_PASSWORD;Pooling=true;MinPoolSize=10;MaxPoolSize=100" -a harmoni360-app

# Redis connection string
flyctl secrets set ConnectionStrings__Redis="redis://default:YOUR_REDIS_PASSWORD@harmoni360-cache.upstash.io:6379" -a harmoni360-app
```

#### Authentication Configuration
```bash
# JWT signing key (must be 32+ characters)
flyctl secrets set Jwt__Key="$(openssl rand -base64 32)" -a harmoni360-app

# JWT issuer and audience
flyctl secrets set Jwt__Issuer="Harmoni360" -a harmoni360-app
flyctl secrets set Jwt__Audience="Harmoni360Users" -a harmoni360-app
```

#### Application Configuration
```bash
# Environment setting
flyctl secrets set ASPNETCORE_ENVIRONMENT="Production" -a harmoni360-app

# Application URLs
flyctl secrets set ASPNETCORE_URLS="http://+:8080" -a harmoni360-app
```

#### Optional Integrations
```bash
# Email service (if using external SMTP)
flyctl secrets set Email__SmtpHost="smtp.gmail.com" -a harmoni360-app
flyctl secrets set Email__SmtpPort="587" -a harmoni360-app
flyctl secrets set Email__Username="your-email@gmail.com" -a harmoni360-app
flyctl secrets set Email__Password="your-app-password" -a harmoni360-app

# External API keys
flyctl secrets set ExternalApi__ApiKey="your-api-key" -a harmoni360-app

# Logging service (Seq)
flyctl secrets set Seq__ApiKey="your-seq-api-key" -a harmoni360-app
```

### 3. Environment-Specific Secret Management

#### Staging Environment Secrets
```bash
# Database (staging)
flyctl secrets set ConnectionStrings__DefaultConnection="Host=harmoni360-staging-db.internal;Port=5432;Database=harmoni360_staging;Username=postgres;Password=STAGING_DB_PASSWORD" -a harmoni360-staging

# Redis (staging)
flyctl secrets set ConnectionStrings__Redis="redis://default:STAGING_REDIS_PASSWORD@harmoni360-staging-cache.upstash.io:6379" -a harmoni360-staging

# JWT (staging - different key)
flyctl secrets set Jwt__Key="$(openssl rand -base64 32)" -a harmoni360-staging

# Environment
flyctl secrets set ASPNETCORE_ENVIRONMENT="Staging" -a harmoni360-staging
```

#### Production Environment Secrets
```bash
# Database (production)
flyctl secrets set ConnectionStrings__DefaultConnection="Host=harmoni360-db.internal;Port=5432;Database=Harmoni360_Prod;Username=harmoni360;Password=PRODUCTION_DB_PASSWORD" -a harmoni360-app

# The Fly Postgres cluster must be created with the same database name and user.

# Redis (production)
flyctl secrets set ConnectionStrings__Redis="redis://default:PRODUCTION_REDIS_PASSWORD@harmoni360-cache.upstash.io:6379" -a harmoni360-app

# JWT (production - strong key)
flyctl secrets set Jwt__Key="$(openssl rand -base64 32)" -a harmoni360-app

# Environment
flyctl secrets set ASPNETCORE_ENVIRONMENT="Production" -a harmoni360-app
```

## ⚙️ Configuration Management

### 1. Fly.io Configuration Files

#### Production Configuration (`fly.toml`)
```toml
app = "harmoni360-app"
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

  [[services.ports]]
    handlers = ["http"]
    port = 80
    force_https = true

  [[services.ports]]
    handlers = ["tls", "http"]
    port = 443

  [services.concurrency]
    type = "connections"
    hard_limit = 50
    soft_limit = 40

[[services.http_checks]]
  interval = "10s"
  grace_period = "5s"
  method = "GET"
  path = "/health"
  protocol = "http"
  timeout = "2s"
  tls_skip_verify = false

[mounts]
  source = "harmoni360_uploads"
  destination = "/app/uploads"

[[vm]]
  cpu_kind = "shared"
  cpus = 1
  memory_mb = 1024
```

#### Staging Configuration (`fly.staging.toml`)
```toml
app = "harmoni360-staging"
primary_region = "sjc"

[build]
  dockerfile = "Dockerfile.flyio"

[env]
  ASPNETCORE_ENVIRONMENT = "Staging"
  ASPNETCORE_URLS = "http://+:8080"
  ASPNETCORE_FORWARDEDHEADERS_ENABLED = "true"

[[services]]
  internal_port = 8080
  protocol = "tcp"
  auto_stop_machines = true
  auto_start_machines = true
  min_machines_running = 0

  [[services.ports]]
    handlers = ["http"]
    port = 80
    force_https = true

  [[services.ports]]
    handlers = ["tls", "http"]
    port = 443

  [services.concurrency]
    type = "connections"
    hard_limit = 25
    soft_limit = 20

[[services.http_checks]]
  interval = "30s"
  grace_period = "10s"
  method = "GET"
  path = "/health"
  protocol = "http"
  timeout = "5s"

[mounts]
  source = "harmoni360_staging_uploads"
  destination = "/app/uploads"

[[vm]]
  cpu_kind = "shared"
  cpus = 1
  memory_mb = 512
```

### 2. Application Configuration Files

#### Production Settings (`appsettings.Production.json`)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Application": {
    "DemoMode": false,
    "Environment": "Production",
    "DemoSettings": {
      "ShowDemoBanner": false,
      "AllowDataModification": true,
      "AllowUserCreation": true,
      "AllowDataDeletion": false,
      "ShowSampleDataLabels": false
    }
  },
  "DataSeeding": {
    "ForceReseed": false,
    "Categories": {
      "Essential": true,
      "SampleData": false,
      "UserAccounts": false
    }
  },
  "Features": {
    "EnableSwagger": false,
    "EnableDetailedErrors": false,
    "EnableSensitiveDataLogging": false
  }
}
```

#### Staging Settings (`appsettings.Staging.json`)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "Application": {
    "DemoMode": true,
    "Environment": "Staging",
    "DemoSettings": {
      "ShowDemoBanner": true,
      "AllowDataModification": true,
      "AllowUserCreation": true,
      "AllowDataDeletion": true,
      "ShowSampleDataLabels": true,
      "BannerMessage": "🧪 Staging Environment - Test with sample data"
    }
  },
  "DataSeeding": {
    "ForceReseed": false,
    "Categories": {
      "Essential": true,
      "SampleData": true,
      "UserAccounts": true
    }
  },
  "Features": {
    "EnableSwagger": true,
    "EnableDetailedErrors": true,
    "EnableSensitiveDataLogging": false
  }
}
```

## 🔄 Environment Promotion Strategy

### 1. Configuration Promotion Workflow

```
Development → Staging → Production
     │            │           │
     ▼            ▼           ▼
Local Config → Test Config → Prod Config
Demo Data   → Sample Data → Real Data
Debug Mode  → Test Mode   → Prod Mode
```

### 2. Automated Environment Setup

#### Environment Setup Script
```bash
#!/bin/bash
# setup-environment.sh

ENVIRONMENT=$1
APP_NAME=$2

if [ -z "$ENVIRONMENT" ] || [ -z "$APP_NAME" ]; then
    echo "Usage: $0 <environment> <app-name>"
    echo "Example: $0 staging harmoni360-staging"
    exit 1
fi

echo "Setting up $ENVIRONMENT environment for $APP_NAME..."

# Generate secure passwords
DB_PASSWORD=$(openssl rand -base64 32)
REDIS_PASSWORD=$(openssl rand -base64 32)
JWT_KEY=$(openssl rand -base64 32)

# Set environment-specific secrets
flyctl secrets set ASPNETCORE_ENVIRONMENT="$ENVIRONMENT" -a "$APP_NAME"
flyctl secrets set Jwt__Key="$JWT_KEY" -a "$APP_NAME"

# Set database connection
flyctl secrets set ConnectionStrings__DefaultConnection="Host=${APP_NAME}-db.internal;Port=5432;Database=harmoni360_${ENVIRONMENT,,};Username=postgres;Password=$DB_PASSWORD" -a "$APP_NAME"

# Set Redis connection
flyctl secrets set ConnectionStrings__Redis="redis://default:$REDIS_PASSWORD@${APP_NAME}-cache.upstash.io:6379" -a "$APP_NAME"

echo "Environment $ENVIRONMENT configured successfully!"
echo "Database password: $DB_PASSWORD"
echo "Redis password: $REDIS_PASSWORD"
echo "JWT key: $JWT_KEY"
```

### 3. Configuration Validation

#### Pre-deployment Validation Script
```bash
#!/bin/bash
# validate-environment.sh

APP_NAME=$1

if [ -z "$APP_NAME" ]; then
    echo "Usage: $0 <app-name>"
    exit 1
fi

echo "Validating environment for $APP_NAME..."

# Check required secrets
REQUIRED_SECRETS=(
    "ConnectionStrings__DefaultConnection"
    "ConnectionStrings__Redis"
    "Jwt__Key"
    "ASPNETCORE_ENVIRONMENT"
)

for secret in "${REQUIRED_SECRETS[@]}"; do
    if flyctl secrets list -a "$APP_NAME" | grep -q "$secret"; then
        echo "✅ $secret is configured"
    else
        echo "❌ $secret is missing"
        exit 1
    fi
done

# Test database connectivity
echo "Testing database connectivity..."
flyctl ssh console -a "$APP_NAME" -C "cd /app && dotnet ef database update --dry-run"

# Test application health
echo "Testing application health..."
APP_URL=$(flyctl info -a "$APP_NAME" | grep "Hostname" | awk '{print $2}')
curl -f "https://$APP_URL/health" || exit 1

echo "✅ Environment validation completed successfully!"
```

## 📊 Environment Monitoring

### 1. Environment-Specific Monitoring

#### Production Monitoring
```bash
# Monitor production application
flyctl metrics -a harmoni360-app

# Check production logs
flyctl logs -a harmoni360-app --since 1h

# Monitor database performance
flyctl metrics -a harmoni360-db
```

#### Staging Monitoring
```bash
# Monitor staging application
flyctl metrics -a harmoni360-staging

# Check staging logs
flyctl logs -a harmoni360-staging --since 1h
```

### 2. Automated Health Checks

#### Health Check Configuration
```yaml
# .github/workflows/health-check.yml
name: Environment Health Check

on:
  schedule:
    - cron: '*/15 * * * *'  # Every 15 minutes
  workflow_dispatch:

jobs:
  health-check:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        environment: [staging, production]
        include:
          - environment: staging
            app_name: harmoni360-staging
            url: https://harmoni360-staging.fly.dev
          - environment: production
            app_name: harmoni360-app
            url: https://harmoni360-app.fly.dev

    steps:
      - name: Check Application Health
        run: |
          response=$(curl -s -o /dev/null -w "%{http_code}" ${{ matrix.url }}/health)
          if [ $response -eq 200 ]; then
            echo "✅ ${{ matrix.environment }} is healthy"
          else
            echo "❌ ${{ matrix.environment }} health check failed (HTTP $response)"
            exit 1
          fi

      - name: Check Database Connectivity
        run: |
          flyctl ssh console -a ${{ matrix.app_name }} -C "cd /app && dotnet ef database update --dry-run"
        env:
          FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}
```

## 🔧 Environment Troubleshooting

### 1. Common Configuration Issues

#### Secret Management Issues
```bash
# Check if secrets are properly set
flyctl secrets list -a harmoni360-app

# Verify secret values (be careful with sensitive data)
flyctl ssh console -a harmoni360-app -C "env | grep -E '(ConnectionStrings|Jwt)'"

# Reset secrets if corrupted
flyctl secrets unset SECRET_NAME -a harmoni360-app
flyctl secrets set SECRET_NAME="new_value" -a harmoni360-app
```

#### Database Connection Issues
```bash
# Test database connectivity
flyctl ssh console -a harmoni360-app -C "cd /app && dotnet ef database update --dry-run"

# Check database status
flyctl status -a harmoni360-db

# Connect to database directly
flyctl postgres connect -a harmoni360-db
```

### 2. Environment Synchronization

#### Sync Staging to Production
```bash
#!/bin/bash
# sync-environments.sh

echo "Syncing staging configuration to production..."

# Get staging secrets (excluding environment-specific ones)
STAGING_SECRETS=$(flyctl secrets list -a harmoni360-staging --json | jq -r '.[] | select(.name | test("^(Jwt__|Email__|ExternalApi__)")) | .name')

for secret in $STAGING_SECRETS; do
    echo "Syncing $secret..."
    # Note: This is a simplified example - in practice, you'd need to handle this more carefully
    # flyctl secrets set "$secret"="$(get_staging_secret_value)" -a harmoni360-app
done

echo "Environment sync completed!"
```

## 🎯 Best Practices

### 1. Secret Management Best Practices
- ✅ Use strong, unique passwords for each environment
- ✅ Rotate secrets regularly (quarterly for production)
- ✅ Never commit secrets to version control
- ✅ Use different secrets for each environment
- ✅ Monitor secret access and usage

### 2. Configuration Management Best Practices
- ✅ Use environment-specific configuration files
- ✅ Validate configuration before deployment
- ✅ Document all configuration changes
- ✅ Use infrastructure as code when possible
- ✅ Implement configuration drift detection

### 3. Environment Promotion Best Practices
- ✅ Test thoroughly in staging before production
- ✅ Use automated deployment pipelines
- ✅ Implement rollback procedures
- ✅ Monitor applications after deployment
- ✅ Document environment differences

## 📞 Support and Resources

### Fly.io Resources
- [Fly.io Secrets Documentation](https://fly.io/docs/reference/secrets/)
- [Fly.io Configuration Reference](https://fly.io/docs/reference/configuration/)
- [Fly.io Environment Variables](https://fly.io/docs/reference/runtime-environment/)

### Internal Resources
- [Getting Started Guide](./Getting_Started.md)
- [CI/CD Integration](./CI_CD_Integration.md)
- [Troubleshooting Guide](./Troubleshooting.md)

---

*Previous: [CI/CD Integration](./CI_CD_Integration.md) | Next: [Monitoring and Logging](./Monitoring_and_Logging.md)*
