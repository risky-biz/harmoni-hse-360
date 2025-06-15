# Environment Configuration Guide for Harmoni360

## 📋 Overview

This guide provides comprehensive documentation for configuring environments, managing secrets, and setting up environment-specific configurations for the Harmoni360 application across development, staging, and production environments.

## 🌍 Environment Architecture

### Environment Separation Strategy

```mermaid
graph TB
    subgraph "Development"
        DEV_APP[Local Application]
        DEV_DB[(Local PostgreSQL)]
        DEV_REDIS[(Local Redis)]
    end
    
    subgraph "Staging"
        STAGE_APP[Staging App<br/>harmoni360-staging.fly.dev]
        STAGE_DB[(Staging PostgreSQL<br/>Fly.io)]
        STAGE_REDIS[(Staging Redis<br/>Fly.io)]
    end
    
    subgraph "Production"
        PROD_APP[Production App<br/>harmoni360-app.fly.dev]
        PROD_DB[(Production PostgreSQL<br/>Fly.io)]
        PROD_REDIS[(Production Redis<br/>Fly.io)]
    end
    
    DEV_APP --> DEV_DB
    DEV_APP --> DEV_REDIS
    STAGE_APP --> STAGE_DB
    STAGE_APP --> STAGE_REDIS
    PROD_APP --> PROD_DB
    PROD_APP --> PROD_REDIS
```

### Environment Characteristics

| Environment | Purpose | Data Persistence | Auto-Deploy | Approval Required |
|-------------|---------|------------------|-------------|-------------------|
| **Development** | Local development and testing | Temporary | Manual | No |
| **Staging** | Pre-production testing | Persistent | Yes (develop branch) | No |
| **Production** | Live application | Persistent | Yes (main branch) | Yes |

## 🔧 Configuration Files Structure

### ASP.NET Core Configuration Hierarchy

```
Configuration Priority (highest to lowest):
1. Environment Variables
2. Fly.io Secrets
3. appsettings.{Environment}.json
4. appsettings.json
5. Default values
```

### Configuration Files Overview

| File | Environment | Purpose | Source Control |
|------|-------------|---------|----------------|
| `appsettings.json` | All | Base configuration | ✅ Committed |
| `appsettings.Development.json` | Development | Dev overrides | ✅ Committed |
| `appsettings.Production.json` | Production | Prod overrides | ✅ Committed |
| `.env` | Local | Local environment variables | ❌ Not committed |
| `fly.toml` | Production | Fly.io production config | ✅ Committed |
| `fly.staging.toml` | Staging | Fly.io staging config | ✅ Committed |

## 🔐 Secrets Management

### Development Environment

#### Local Environment Variables (`.env`)

<augment_code_snippet path=".env.example" mode="EXCERPT">
````bash
# Harmoni360 Environment Configuration
# Copy this file to .env and update with your values

# Application Settings
APP_PORT=8080

# Database Configuration
POSTGRES_DB=Harmoni360
POSTGRES_USER=postgres
POSTGRES_PASSWORD=YourStrongProductionPassword123!

# JWT Configuration
JWT_KEY=YourSuperSecretProductionJwtKeyThatMustBeAtLeast32CharactersLong!
JWT_ISSUER=Harmoni360
JWT_AUDIENCE=Harmoni360Users
````
</augment_code_snippet>

#### Development Configuration

<augment_code_snippet path="src/Harmoni360.Web/appsettings.Development.json" mode="EXCERPT">
````json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=Harmoni360_Dev;Username=postgres;Password=postgres123"
  },
  "Jwt": {
    "Key": "YourSuperSecretDevelopmentJwtKeyThatIsAtLeast32CharactersLong!",
    "ExpirationMinutes": 1440,
    "RefreshTokenExpirationDays": 30
  },
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
````
</augment_code_snippet>

### Staging Environment

#### Fly.io Staging Configuration

<augment_code_snippet path="fly.staging.toml" mode="EXCERPT">
````toml
# Fly.io configuration file for Harmoni360 Staging Environment

app = "harmoni360-staging"
primary_region = "sjc"

[env]
  ASPNETCORE_ENVIRONMENT = "Staging"
  ASPNETCORE_URLS = "http://+:8080"

[vm]
  cpu_kind = "shared"
  cpus = 1
  memory_mb = 512
````
</augment_code_snippet>

#### Staging Secrets Setup

```bash
# Database connection
fly secrets set ConnectionStrings__DefaultConnection="Host=harmoni360-staging-db.internal;Port=5432;Database=harmoni360_staging;Username=postgres;Password=[staging-db-password]" -a harmoni360-staging

# Redis connection
fly secrets set ConnectionStrings__Redis="redis://default:[staging-redis-password]@harmoni360-staging-redis.internal:6379" -a harmoni360-staging

# JWT configuration
fly secrets set Jwt__Key="Harmoni360-Staging-JWT-$(date +%s)-$(openssl rand -hex 16)" -a harmoni360-staging
```

### Production Environment

#### Production Configuration Template

<augment_code_snippet path="src/Harmoni360.Web/appsettings.Production.json" mode="EXCERPT">
````json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "",
    "Redis": ""
  },
  "Jwt": {
    "Key": "",
    "Issuer": "Harmoni360",
    "Audience": "Harmoni360Users",
    "ExpirationMinutes": "60",
    "RefreshTokenExpirationDays": "7"
  }
}
````
</augment_code_snippet>

#### Production Secrets Setup

```bash
# Database connection (replace with actual values)
fly secrets set ConnectionStrings__DefaultConnection="Host=harmoni360-db.internal;Port=5432;Database=Harmoni360_Prod;Username=harmoni360;Password=[production-db-password]" -a harmoni360-app

> Ensure the Fly Postgres cluster was created with this database name and user.

# Redis connection (replace with actual values)
fly secrets set ConnectionStrings__Redis="redis://default:[production-redis-password]@harmoni360-redis.internal:6379" -a harmoni360-app

# JWT configuration (generate secure key)
JWT_KEY="Harmoni360-Production-JWT-$(date +%s)-$(openssl rand -hex 32)"
fly secrets set Jwt__Key="$JWT_KEY" -a harmoni360-app
```

## 📊 Environment Variables Reference

### Required Environment Variables

| Variable | Development | Staging | Production | Description |
|----------|-------------|---------|------------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Development | Staging | Production | Runtime environment |
| `ASPNETCORE_URLS` | http://+:5000 | http://+:8080 | http://+:8080 | Binding URLs |
| `ConnectionStrings__DefaultConnection` | Local PostgreSQL | Staging DB | Production DB | Database connection |
| `ConnectionStrings__Redis` | Local Redis | Staging Redis | Production Redis | Cache connection |
| `Jwt__Key` | Dev key (long) | Staging key | Production key | JWT signing key |

### Optional Environment Variables

| Variable | Default | Description | Environment |
|----------|---------|-------------|-------------|
| `Jwt__Issuer` | Harmoni360 | JWT token issuer | All |
| `Jwt__Audience` | Harmoni360Users | JWT token audience | All |
| `Jwt__ExpirationMinutes` | 60 | Token expiration time | All |
| `Jwt__RefreshTokenExpirationDays` | 7 | Refresh token expiration | All |
| `Seq__ServerUrl` | - | Structured logging endpoint | Production |

### Application-Specific Variables

| Variable | Purpose | Example Value | Required |
|----------|---------|---------------|----------|
| `DataSeeding__SeedIncidents` | Enable incident data seeding | true/false | No |
| `DataSeeding__ReSeedIncidents` | Re-seed incidents on startup | false | No |
| `DataSeeding__SeedPPEData` | Enable PPE data seeding | true | No |
| `MAX_FILE_SIZE` | Maximum upload file size | 10485760 (10MB) | No |
| `ALLOWED_FILE_EXTENSIONS` | Allowed file types | .jpg,.png,.pdf | No |

## 🔒 Security Configuration

### JWT Configuration

#### Development Settings
```json
{
  "Jwt": {
    "Key": "YourSuperSecretDevelopmentJwtKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "Harmoni360",
    "Audience": "Harmoni360Users",
    "ExpirationMinutes": 1440,
    "RefreshTokenExpirationDays": 30
  }
}
```

#### Production Settings
```bash
# Generate secure production JWT key
JWT_KEY="Harmoni360-Production-JWT-$(date +%s)-$(openssl rand -hex 32)"

# Set via Fly.io secrets
fly secrets set Jwt__Key="$JWT_KEY" -a harmoni360-app
fly secrets set Jwt__ExpirationMinutes="60" -a harmoni360-app
fly secrets set Jwt__RefreshTokenExpirationDays="7" -a harmoni360-app
```

### Database Security

#### Connection String Security
```bash
# Secure connection string format
"Host=hostname;Port=5432;Database=dbname;Username=user;Password=password;SSL Mode=Require;Trust Server Certificate=true"

# Environment-specific examples:
# Development: SSL Mode=Disable (local only)
# Staging: SSL Mode=Require
# Production: SSL Mode=Require
```

#### Database User Permissions
```sql

-- Create application-specific database user
CREATE USER harmoni360 WITH PASSWORD 'secure_password';

-- Grant minimal required permissions
GRANT CONNECT ON DATABASE Harmoni360_Prod TO harmoni360;
GRANT USAGE ON SCHEMA public TO harmoni360;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO harmoni360;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO harmoni360;
```

## 🔄 Configuration Management Scripts

### Environment Setup Script

```bash
#!/bin/bash
# setup-environment.sh

ENVIRONMENT=${1:-development}

case $ENVIRONMENT in
  "development")
    echo "Setting up development environment..."
    cp .env.example .env
    echo "Edit .env file with your local configuration"
    ;;
  "staging")
    echo "Setting up staging environment..."
    fly secrets set ConnectionStrings__DefaultConnection="$STAGING_DB_CONNECTION" -a harmoni360-staging
    fly secrets set ConnectionStrings__Redis="$STAGING_REDIS_CONNECTION" -a harmoni360-staging
    ;;
  "production")
    echo "Setting up production environment..."
    fly secrets set ConnectionStrings__DefaultConnection="$PRODUCTION_DB_CONNECTION" -a harmoni360-app
    fly secrets set ConnectionStrings__Redis="$PRODUCTION_REDIS_CONNECTION" -a harmoni360-app
    ;;
  *)
    echo "Usage: $0 {development|staging|production}"
    exit 1
    ;;
esac
```

### Secrets Validation Script

```bash
#!/bin/bash
# validate-secrets.sh

APP_NAME=${1:-harmoni360-app}

echo "Validating secrets for $APP_NAME..."

# Check required secrets
REQUIRED_SECRETS=(
  "ConnectionStrings__DefaultConnection"
  "ConnectionStrings__Redis"
  "Jwt__Key"
)

for secret in "${REQUIRED_SECRETS[@]}"; do
  if fly secrets list -a "$APP_NAME" | grep -q "$secret"; then
    echo "✅ $secret - configured"
  else
    echo "❌ $secret - missing"
  fi
done

# Test database connection
echo "Testing database connection..."
fly ssh console -a "$APP_NAME" -C "cd /app && dotnet ef database update --dry-run" || echo "❌ Database connection failed"

echo "Validation complete."
```

## 🔍 Configuration Troubleshooting

### Common Configuration Issues

#### 1. Missing Environment Variables
```bash
# Check current environment variables
fly ssh console -a harmoni360-app -C "env | grep -E '(ConnectionStrings|Jwt)'"

# Verify secrets are set
fly secrets list -a harmoni360-app
```

#### 2. Database Connection Issues
```bash
# Test database connectivity
fly postgres connect -a harmoni360-db

# Check connection string format
fly secrets list -a harmoni360-app | grep ConnectionStrings
```

#### 3. JWT Configuration Problems
```bash
# Verify JWT key length (must be 32+ characters)
fly ssh console -a harmoni360-app -C "echo \$Jwt__Key | wc -c"

# Test JWT token generation
curl -X POST https://harmoni360-app.fly.dev/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password"}'
```

### Configuration Validation

#### Health Check Endpoint
```bash
# Check application health
curl https://harmoni360-app.fly.dev/health

# Expected response:
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "redis": "Healthy"
  }
}
```

#### Environment-Specific Testing
```bash
# Development
curl http://localhost:5000/health

# Staging
curl https://harmoni360-staging.fly.dev/health

# Production
curl https://harmoni360-app.fly.dev/health
```

## 📈 Configuration Best Practices

### Security Best Practices

1. **Never commit secrets to source control**
2. **Use environment-specific secret management**
3. **Rotate secrets regularly**
4. **Use strong, unique passwords**
5. **Enable SSL/TLS for all external connections**

### Configuration Management

1. **Use configuration hierarchy effectively**
2. **Document all environment variables**
3. **Validate configuration on startup**
4. **Use typed configuration classes**
5. **Implement configuration change detection**

### Deployment Considerations

1. **Test configuration changes in staging first**
2. **Use blue-green deployments for configuration updates**
3. **Monitor application health after configuration changes**
4. **Have rollback procedures for configuration issues**
5. **Document configuration dependencies**

---

**Document Version:** 1.0.0  
**Last Updated:** December 2024  
**Next Review:** March 2025
