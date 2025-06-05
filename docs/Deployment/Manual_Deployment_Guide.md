# Manual Deployment Guide for HarmoniHSE360

## 📋 Overview

This guide provides step-by-step instructions for manually deploying the HarmoniHSE360 application to Fly.io using Docker containers. This process is essential for understanding the deployment workflow and for emergency deployments.

## 🎯 Prerequisites

### Required Tools and Accounts

| Tool | Version | Purpose | Installation |
|------|---------|---------|--------------|
| **Fly.io CLI** | Latest | Deployment platform | `curl -L https://fly.io/install.sh \| sh` |
| **Docker** | 20.10+ | Container runtime | [Docker Desktop](https://docs.docker.com/get-docker/) |
| **.NET SDK** | 8.0.x | Application build | [Download .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **Node.js** | 20.x LTS | Frontend build | [Download Node.js](https://nodejs.org/) |
| **Git** | Latest | Source control | [Download Git](https://git-scm.com/) |

### Account Setup

1. **Fly.io Account**
   ```bash
   # Create account and authenticate
   fly auth signup
   fly auth login
   
   # Verify authentication
   fly auth whoami
   ```

2. **Payment Method**
   - Add payment method to Fly.io account
   - Required for PostgreSQL and Redis services
   - Free tier available for basic usage

## 🚀 Step-by-Step Deployment Process

### Step 1: Environment Preparation

#### 1.1 Clone Repository
```bash
# Clone the repository
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360

# Checkout appropriate branch
git checkout main  # For production
# git checkout develop  # For staging
```

#### 1.2 Verify Prerequisites
```bash
# Check tool versions
fly version
docker --version
dotnet --version
node --version
npm --version

# Verify Docker is running
docker ps
```

### Step 2: Database Setup

#### 2.1 Create PostgreSQL Database
```bash
# Create PostgreSQL cluster
fly postgres create --name harmonihse360-db --region sjc

# Note the connection details provided
# Example output:
# Username: postgres
# Password: [generated-password]
# Hostname: harmonihse360-db.internal
# Proxy port: 5432
# Postgres port: 5433
```

#### 2.2 Create Database Connection
```bash
# Connect to database to verify
fly postgres connect -a harmonihse360-db

# Create application database (if needed)
CREATE DATABASE harmonihse360_production;
\q
```

### Step 3: Redis Cache Setup

#### 3.1 Create Redis Instance
```bash
# Create Redis instance via Upstash (Fly.io extension)
fly ext redis create --name harmonihse360-redis

# Alternative: Create Redis app
fly redis create --name harmonihse360-redis --region sjc
```

#### 3.2 Get Redis Connection String
```bash
# Get Redis connection details
fly redis status harmonihse360-redis

# Note the connection string format:
# redis://default:[password]@[hostname]:6379
```

### Step 4: Application Configuration

#### 4.1 Create Fly.io Configuration
```bash
# Copy example configuration
cp fly.toml.example fly.toml

# Edit configuration for your environment
nano fly.toml  # or use your preferred editor
```

#### 4.2 Configure Application Settings
```toml
# fly.toml configuration
app = "harmonihse360-app"  # Change to your app name
primary_region = "sjc"     # Choose your region

[build]
  dockerfile = "Dockerfile.flyio"

[env]
  ASPNETCORE_ENVIRONMENT = "Production"
  ASPNETCORE_URLS = "http://+:8080"

[[services]]
  internal_port = 8080
  protocol = "tcp"

  [[services.ports]]
    handlers = ["http"]
    port = 80
    force_https = true

  [[services.ports]]
    handlers = ["tls", "http"]
    port = 443
```

### Step 5: Secrets Configuration

#### 5.1 Set Database Connection
```bash
# Set PostgreSQL connection string
fly secrets set ConnectionStrings__DefaultConnection="Host=harmonihse360-db.internal;Port=5432;Database=harmonihse360_production;Username=postgres;Password=[your-db-password]"
```

#### 5.2 Set Redis Connection
```bash
# Set Redis connection string
fly secrets set ConnectionStrings__Redis="redis://default:[redis-password]@[redis-hostname]:6379"
```

#### 5.3 Set JWT Configuration
```bash
# Generate secure JWT key (32+ characters)
JWT_KEY="HarmoniHSE360-Production-JWT-$(date +%s)-$(openssl rand -hex 16)"

# Set JWT key
fly secrets set Jwt__Key="$JWT_KEY"
```

#### 5.4 Verify Secrets
```bash
# List configured secrets (values hidden)
fly secrets list
```

### Step 6: Volume Creation

#### 6.1 Create Persistent Volume
```bash
# Create volume for file uploads
fly volumes create harmonihse360_uploads --region sjc --size 1

# Verify volume creation
fly volumes list
```

### Step 7: Application Deployment

#### 7.1 Initialize Fly Application
```bash
# Initialize Fly app (if not already done)
fly launch --no-deploy --name harmonihse360-app --region sjc

# This creates the app without deploying
```

#### 7.2 Deploy Application
```bash
# Deploy the application
fly deploy

# Monitor deployment progress
fly logs -f
```

#### 7.3 Verify Deployment
```bash
# Check application status
fly status

# Test health endpoint
curl https://harmonihse360-app.fly.dev/health

# Check application logs
fly logs --app harmonihse360-app
```

### Step 8: Database Migration

#### 8.1 Run Entity Framework Migrations
```bash
# SSH into the application container
fly ssh console

# Navigate to application directory
cd /app

# Run database migrations
dotnet ef database update

# Exit container
exit
```

#### 8.2 Verify Database Schema
```bash
# Connect to database
fly postgres connect -a harmonihse360-db

# Check tables
\dt

# Verify data
SELECT COUNT(*) FROM "__EFMigrationsHistory";
\q
```

## 🔧 Post-Deployment Configuration

### Custom Domain Setup (Optional)

#### 1. Add Custom Domain
```bash
# Add your custom domain
fly certs create yourdomain.com

# Add www subdomain
fly certs create www.yourdomain.com

# Check certificate status
fly certs list
```

#### 2. DNS Configuration
```bash
# Get Fly.io IP addresses
fly ips list

# Configure DNS records:
# A record: yourdomain.com -> [IPv4 from above]
# AAAA record: yourdomain.com -> [IPv6 from above]
# CNAME record: www.yourdomain.com -> yourdomain.com
```

### SSL Certificate Verification
```bash
# Check certificate status
fly certs show yourdomain.com

# Test HTTPS
curl -I https://yourdomain.com/health
```

## 🔍 Verification and Testing

### Application Health Checks

#### 1. Basic Health Check
```bash
# Test health endpoint
curl https://harmonihse360-app.fly.dev/health

# Expected response: HTTP 200 OK
```

#### 2. Database Connectivity
```bash
# SSH into container and test database
fly ssh console
cd /app
dotnet ef database update --dry-run
exit
```

#### 3. Redis Connectivity
```bash
# Check Redis connection from application logs
fly logs | grep -i redis
```

### Performance Testing

#### 1. Load Testing
```bash
# Simple load test with curl
for i in {1..10}; do
  curl -w "%{time_total}\n" -o /dev/null -s https://harmonihse360-app.fly.dev/health
done
```

#### 2. Resource Monitoring
```bash
# Monitor resource usage
fly status
fly metrics

# Check detailed metrics
fly dashboard
```

## 🚨 Troubleshooting Common Issues

### Deployment Failures

#### 1. Build Errors
```bash
# Check build logs
fly logs --app harmonihse360-app

# Common issues:
# - Node.js version mismatch
# - Missing dependencies
# - Docker build context issues
```

#### 2. Runtime Errors
```bash
# Check application logs
fly logs -f

# Common issues:
# - Database connection failures
# - Missing environment variables
# - Port binding issues
```

### Database Issues

#### 1. Connection Problems
```bash
# Test database connectivity
fly postgres connect -a harmonihse360-db

# Check connection string format
fly secrets list | grep ConnectionStrings
```

#### 2. Migration Failures
```bash
# SSH into container
fly ssh console

# Check migration status
cd /app
dotnet ef migrations list

# Apply specific migration
dotnet ef database update [MigrationName]
```

### Performance Issues

#### 1. Slow Response Times
```bash
# Check resource usage
fly status
fly metrics

# Scale up if needed
fly scale memory 1024  # Increase to 1GB RAM
fly scale count 2      # Add second instance
```

#### 2. High CPU Usage
```bash
# Monitor CPU usage
fly metrics

# Check application logs for errors
fly logs | grep -i error
```

## 🔄 Rollback Procedures

### Emergency Rollback

#### 1. Quick Rollback
```bash
# Get previous release
fly releases

# Rollback to previous version
fly releases rollback [version-number]
```

#### 2. Database Rollback
```bash
# SSH into container
fly ssh console
cd /app

# Rollback to specific migration
dotnet ef database update [PreviousMigrationName]
```

## 📊 Monitoring and Maintenance

### Regular Maintenance Tasks

#### 1. Log Monitoring
```bash
# Check for errors
fly logs | grep -i error

# Monitor performance
fly logs | grep -i "request took"
```

#### 2. Resource Monitoring
```bash
# Check resource usage
fly status
fly metrics

# Monitor costs
fly dashboard  # View billing information
```

#### 3. Security Updates
```bash
# Update base images
fly deploy  # Rebuilds with latest base images

# Check for vulnerabilities
docker scan harmonihse360:latest
```

---

**Document Version:** 1.0.0  
**Last Updated:** December 2024  
**Next Review:** March 2025
