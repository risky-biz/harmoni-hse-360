# Environment Configuration for Local Development

## Overview

This guide covers the complete configuration of environment variables, secrets, and service settings for the HarmoniHSE360 local development environment. The configuration is designed to mirror production settings while being optimized for local development and testing.

## 📁 Configuration File Structure

```
harmoni-hse-360/
├── .env.local.example          # Environment template
├── .env.local                  # Your local configuration (git-ignored)
├── local-dev/config/           # Service configurations
│   ├── nginx/                  # Nginx reverse proxy config
│   ├── prometheus/             # Monitoring configuration
│   ├── grafana/                # Dashboard configuration
│   ├── redis/                  # Cache configuration
│   └── postgres/               # Database configuration
└── docker-compose.local.yml    # Docker services definition
```

## 🔧 Environment Variables Configuration

### 1. Initial Setup

```powershell
# Copy the environment template
Copy-Item .env.local.example .env.local

# Edit the configuration file
notepad .env.local
```

### 2. Core Application Settings

```bash
# =============================================================================
# APPLICATION CONFIGURATION
# =============================================================================
ASPNETCORE_ENVIRONMENT=Development
APP_PORT=8080
APP_BASE_URL=https://localhost
BUILD_VERSION=latest
DEMO_MODE=true
FORCE_RESEED=false
```

**Configuration Details:**
- `ASPNETCORE_ENVIRONMENT`: Set to `Development` for local development
- `APP_PORT`: Internal application port (8080)
- `APP_BASE_URL`: Base URL for the application (used for redirects)
- `DEMO_MODE`: Enables demo features and sample data
- `FORCE_RESEED`: Forces database reseeding on startup

### 3. Database Configuration

```bash
# =============================================================================
# DATABASE CONFIGURATION
# =============================================================================
POSTGRES_DB=Harmoni360_Local
POSTGRES_USER=harmoni360
POSTGRES_PASSWORD=LocalDev_SecurePassword123!
POSTGRES_HOST=postgres
POSTGRES_PORT=5432

# PostgreSQL Performance Tuning (Local Development)
POSTGRES_SHARED_BUFFERS=256MB
POSTGRES_EFFECTIVE_CACHE_SIZE=1GB
POSTGRES_WORK_MEM=16MB
POSTGRES_MAINTENANCE_WORK_MEM=128MB
POSTGRES_MAX_CONNECTIONS=100
```

**Security Considerations:**
- Use strong passwords (32+ characters)
- Different passwords for each environment
- Never commit actual passwords to version control

**Performance Tuning:**
- `SHARED_BUFFERS`: 25% of available RAM for PostgreSQL
- `EFFECTIVE_CACHE_SIZE`: 75% of available RAM
- `WORK_MEM`: Per-operation memory limit
- `MAINTENANCE_WORK_MEM`: Memory for maintenance operations

### 4. Redis Configuration

```bash
# =============================================================================
# REDIS CONFIGURATION
# =============================================================================
REDIS_PASSWORD=LocalDev_RedisPassword456!
REDIS_PORT=6379
```

**Redis Features Enabled:**
- Password authentication
- Persistence (AOF + RDB)
- Memory optimization
- Connection pooling

### 5. JWT Authentication

```bash
# =============================================================================
# JWT CONFIGURATION
# =============================================================================
JWT_KEY=LocalDev_JwtSecretKeyThatMustBeAtLeast32CharactersLongForSecurity!
JWT_ISSUER=Harmoni360
JWT_AUDIENCE=Harmoni360Users
JWT_EXPIRATION_MINUTES=60
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=7
```

**JWT Key Generation:**
```powershell
# Generate a secure JWT key
$jwtKey = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString() + (New-Guid).ToString()))
Write-Host "JWT_KEY=$jwtKey"
```

### 6. Monitoring Stack Configuration

```bash
# =============================================================================
# MONITORING CONFIGURATION
# =============================================================================

# Prometheus Configuration
PROMETHEUS_PORT=9090

# Grafana Configuration
GRAFANA_PORT=3000
GRAFANA_ADMIN_USER=admin
GRAFANA_ADMIN_PASSWORD=LocalDev_GrafanaAdmin789!

# Seq Logging Configuration
SEQ_PORT=5341
SEQ_ADMIN_PASSWORD_HASH=$SHA256$V1$10000$Hashed_Password_Here
SEQ_API_KEY=

# Node Exporter Configuration
NODE_EXPORTER_PORT=9100
```

**Seq Password Hash Generation:**
```powershell
# Generate Seq password hash
$seqPassword = "YourSecurePassword"
$seqHash = echo $seqPassword | docker run --rm -i datalust/seq config hash
Write-Host "SEQ_ADMIN_PASSWORD_HASH=$seqHash"
```

### 7. Ngrok Configuration

```bash
# =============================================================================
# NGROK CONFIGURATION
# =============================================================================
NGROK_AUTHTOKEN=your_ngrok_authtoken_here
NGROK_REGION=us
NGROK_SUBDOMAIN_APP=harmoni360-dev
NGROK_SUBDOMAIN_GRAFANA=harmoni360-grafana
NGROK_SUBDOMAIN_PROMETHEUS=harmoni360-prometheus
NGROK_SUBDOMAIN_SEQ=harmoni360-seq
```

**Ngrok Setup:**
1. Sign up at [ngrok.com](https://ngrok.com)
2. Get your authtoken from [dashboard](https://dashboard.ngrok.com/get-started/your-authtoken)
3. Update `NGROK_AUTHTOKEN` in `.env.local`

## 🔐 Security Configuration

### 1. Development Security Settings

```bash
# =============================================================================
# SECURITY CONFIGURATION (DEVELOPMENT ONLY)
# =============================================================================
# WARNING: These are development-only settings. Never use in production!
ALLOW_INSECURE_HTTP=true
DISABLE_HTTPS_REDIRECTION=false
CORS_ALLOW_ANY_ORIGIN=true
TRUST_PROXY_HEADERS=true
```

### 2. SSL Certificate Configuration

```bash
# =============================================================================
# SSL CONFIGURATION
# =============================================================================
SSL_CERT_PATH=./local-dev/ssl/cert.pem
SSL_KEY_PATH=./local-dev/ssl/key.pem
SSL_DOMAIN=localhost
```

**Generate Self-Signed Certificate:**
```powershell
# Using OpenSSL (if available)
openssl req -x509 -nodes -days 365 -newkey rsa:2048 `
  -keyout local-dev/ssl/key.pem `
  -out local-dev/ssl/cert.pem `
  -subj "/C=US/ST=Dev/L=Local/O=HarmoniHSE360/CN=localhost"

# Using PowerShell
$cert = New-SelfSignedCertificate -DnsName "localhost", "*.ngrok.io" `
  -CertStoreLocation "cert:\LocalMachine\My" `
  -NotAfter (Get-Date).AddYears(1)
```

## 🎛️ Service-Specific Configuration

### 1. Nginx Configuration

Location: `local-dev/config/nginx/nginx.local.conf`

**Key Features:**
- SSL termination
- Reverse proxy to application
- Static file serving
- Rate limiting
- Security headers
- Ngrok compatibility

**Custom Configuration:**
```nginx
# Custom upstream for load balancing
upstream harmoni360_app {
    server app:8080;
    keepalive 32;
}

# Rate limiting zones
limit_req_zone $binary_remote_addr zone=api:10m rate=10r/s;
limit_req_zone $binary_remote_addr zone=login:10m rate=1r/s;
```

### 2. Prometheus Configuration

Location: `local-dev/config/prometheus/prometheus.local.yml`

**Scrape Targets:**
- HarmoniHSE360 application metrics
- Node exporter (system metrics)
- PostgreSQL metrics (if exporter available)
- Redis metrics (if exporter available)
- Nginx metrics (if exporter available)

**Custom Metrics:**
```yaml
scrape_configs:
  - job_name: 'harmoni360-app'
    static_configs:
      - targets: ['app:8080']
    scrape_interval: 15s
    metrics_path: /metrics
```

### 3. Grafana Configuration

Location: `local-dev/config/grafana/provisioning/`

**Auto-Provisioned:**
- Prometheus datasource
- Default dashboards
- Alert rules
- Notification channels

**Dashboard Categories:**
- Application Performance
- System Monitoring
- Business Metrics
- Infrastructure Health

## 🔄 Environment Management

### 1. Environment Switching

```powershell
# Switch to development mode
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Switch to staging simulation
$env:ASPNETCORE_ENVIRONMENT = "Staging"

# Restart services to apply changes
docker-compose -f docker-compose.local.yml restart app
```

### 2. Configuration Validation

```powershell
# Validate environment configuration
.\local-dev\scripts\validate-config.ps1

# Check service connectivity
.\local-dev\scripts\health-check.ps1

# Test database connection
docker-compose -f docker-compose.local.yml exec app dotnet ef database update --dry-run
```

### 3. Secret Management

```powershell
# Generate all secrets at once
.\local-dev\scripts\generate-secrets.ps1

# Rotate specific secrets
.\local-dev\scripts\rotate-secret.ps1 -SecretName "JWT_KEY"

# Backup configuration
.\local-dev\scripts\backup-config.ps1
```

## 🧪 Development Features

### 1. Feature Flags

```bash
# =============================================================================
# FEATURE FLAGS
# =============================================================================
FEATURE_ENABLE_REAL_TIME_NOTIFICATIONS=true
FEATURE_ENABLE_FILE_UPLOADS=true
FEATURE_ENABLE_AUDIT_LOGGING=true
FEATURE_ENABLE_PERFORMANCE_MONITORING=true
```

### 2. Development Tools

```bash
# =============================================================================
# DEVELOPMENT TOOLS
# =============================================================================
ENABLE_SWAGGER=true
ENABLE_DETAILED_ERRORS=true
ENABLE_SENSITIVE_DATA_LOGGING=false
ENABLE_HOT_RELOAD=true
```

### 3. Data Seeding

```bash
# =============================================================================
# DEVELOPMENT CONFIGURATION
# =============================================================================
SEED_SAMPLE_DATA=true
SEED_TEST_USERS=true
SEED_DEMO_INCIDENTS=true
```

## 📊 Performance Configuration

### 1. Resource Limits

```bash
# =============================================================================
# PERFORMANCE CONFIGURATION
# =============================================================================
MAX_REQUEST_SIZE=100MB
CONNECTION_TIMEOUT=30
COMMAND_TIMEOUT=30

# Cache configuration
CACHE_DEFAULT_EXPIRATION=3600
CACHE_SLIDING_EXPIRATION=1800
```

### 2. Docker Resource Allocation

```bash
# =============================================================================
# WINDOWS-SPECIFIC CONFIGURATION
# =============================================================================
DOCKER_MEMORY_LIMIT=20g
DOCKER_CPU_LIMIT=6
DOCKER_SWAP_LIMIT=4g
```

## 🔧 Troubleshooting Configuration

### 1. Debug Settings

```bash
# =============================================================================
# TROUBLESHOOTING
# =============================================================================
VERBOSE_LOGGING=false
DEBUG_MODE=false
TRACE_REQUESTS=false

# Health check configuration
HEALTH_CHECK_TIMEOUT=30
HEALTH_CHECK_INTERVAL=30
HEALTH_CHECK_RETRIES=5
```

### 2. Common Configuration Issues

#### Issue: Database Connection Failed
```bash
# Check database configuration
echo $POSTGRES_PASSWORD
docker-compose -f docker-compose.local.yml logs postgres

# Test connection
docker-compose -f docker-compose.local.yml exec postgres psql -U harmoni360 -d Harmoni360_Local
```

#### Issue: Redis Connection Failed
```bash
# Check Redis configuration
echo $REDIS_PASSWORD
docker-compose -f docker-compose.local.yml logs redis

# Test connection
docker-compose -f docker-compose.local.yml exec redis redis-cli -a $REDIS_PASSWORD ping
```

#### Issue: JWT Token Invalid
```bash
# Verify JWT configuration
echo $JWT_KEY | wc -c  # Should be 32+ characters

# Regenerate JWT key
$newJwtKey = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString() + (New-Guid).ToString()))
```

## 📋 Configuration Checklist

### Pre-Deployment Checklist
- [ ] `.env.local` file created and configured
- [ ] All passwords generated and secure (32+ characters)
- [ ] JWT key generated and configured
- [ ] Ngrok authtoken configured
- [ ] SSL certificates generated
- [ ] Database configuration validated
- [ ] Redis configuration validated
- [ ] Monitoring stack configured

### Security Checklist
- [ ] Strong passwords for all services
- [ ] JWT key is cryptographically secure
- [ ] SSL certificates properly configured
- [ ] Development-only security settings documented
- [ ] No production secrets in development environment
- [ ] `.env.local` added to `.gitignore`

### Performance Checklist
- [ ] Resource limits configured appropriately
- [ ] Database performance tuning applied
- [ ] Cache configuration optimized
- [ ] Docker resource allocation set
- [ ] Health check timeouts configured

## 🎯 Next Steps

After configuring the environment:

1. **Start Services**: `docker-compose -f docker-compose.local.yml up -d`
2. **Verify Configuration**: Run health checks and validation scripts
3. **Start Ngrok**: `.\local-dev\scripts\start-ngrok.ps1 -Action start -All -Background`
4. **Test Application**: Access via ngrok URL or https://localhost
5. **Monitor Services**: Check Grafana dashboards and Prometheus metrics

## 📞 Support Resources

### Configuration References
- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [PostgreSQL Configuration](https://www.postgresql.org/docs/current/runtime-config.html)
- [Redis Configuration](https://redis.io/topics/config)
- [Nginx Configuration](https://nginx.org/en/docs/)

### Troubleshooting
- [Testing Procedures](./Testing_Procedures.md)
- [Troubleshooting Guide](./Troubleshooting.md)
- [Windows Setup Guide](./Windows_Setup.md)

---

*Previous: [Ngrok Configuration](./Ngrok_Configuration.md) | Next: [Testing Procedures](./Testing_Procedures.md)*
