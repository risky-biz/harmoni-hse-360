# Getting Started with Fly.io Deployment

## Overview

This guide walks you through the initial setup required to deploy HarmoniHSE360 to Fly.io, from creating your account to configuring your first deployment.

## 🎯 Prerequisites

### Required Accounts
- **Fly.io Account**: Free account with payment method for production deployments
- **GitHub Account**: For source code and CI/CD automation
- **Domain Name** (Optional): For custom domain setup

### Local Development Environment
- **Operating System**: Windows 10+, macOS 10.15+, or Linux
- **RAM**: Minimum 8GB (16GB recommended)
- **Storage**: 20GB free space
- **Internet**: Stable broadband connection

## 📝 Step 1: Fly.io Account Setup

### 1.1 Create Fly.io Account

1. Visit [fly.io/app/sign-up](https://fly.io/app/sign-up)
2. Sign up using GitHub, Google, or email
3. Verify your email address
4. Complete the onboarding process

### 1.2 Add Payment Method

**Important**: Even for free tier usage, Fly.io requires a payment method for verification.

1. Go to [fly.io/dashboard/billing](https://fly.io/dashboard/billing)
2. Click "Add Payment Method"
3. Enter your credit card information
4. Verify the payment method

### 1.3 Understand Fly.io Pricing

```
Free Tier Allowances (per month):
- 3 shared-cpu-1x instances (256MB RAM each)
- 3GB persistent volume storage
- 160GB outbound data transfer
- 3GB PostgreSQL database storage

Paid Resources:
- Additional instances: $1.94/month per 256MB
- PostgreSQL: $1.94/month per 1GB
- Redis: $1.94/month per 256MB
- Volumes: $0.15/month per GB
- Bandwidth: $0.02/GB over free tier
```

## 🛠️ Step 2: Install Fly CLI

### 2.1 Installation by Platform

#### Windows (PowerShell)
```powershell
# Using PowerShell
iwr https://fly.io/install.ps1 -useb | iex

# Using Chocolatey (if installed)
choco install flyctl

# Using Scoop (if installed)
scoop install flyctl
```

#### macOS
```bash
# Using Homebrew (recommended)
brew install flyctl

# Using curl
curl -L https://fly.io/install.sh | sh
```

#### Linux
```bash
# Using curl
curl -L https://fly.io/install.sh | sh

# Add to PATH (add to ~/.bashrc or ~/.zshrc)
export PATH="$HOME/.fly/bin:$PATH"
```

### 2.2 Verify Installation

```bash
# Check version
flyctl version

# Should output something like:
# flyctl v0.1.xxx linux/amd64 Commit: xxxxx BuildDate: 2025-01-xx
```

### 2.3 Authenticate with Fly.io

```bash
# Login to Fly.io
flyctl auth login

# This will open a browser window for authentication
# Follow the prompts to complete login

# Verify authentication
flyctl auth whoami
```

## 📦 Step 3: Prepare Your Repository

### 3.1 Clone the Repository

```bash
# Clone the HarmoniHSE360 repository
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360

# Switch to main branch
git checkout main

# Verify repository structure
ls -la
```

### 3.2 Review Configuration Files

#### Fly.io Configuration Template
```bash
# Copy the example configuration
cp fly.toml.example fly.toml

# Review the configuration
cat fly.toml
```

#### Docker Configuration
```bash
# Review the Fly.io optimized Dockerfile
cat Dockerfile.flyio
```

### 3.3 Understand Project Structure

```
harmoni-hse-360/
├── src/
│   ├── Harmoni360.Web/              # Main web application
│   │   ├── ClientApp/               # React frontend
│   │   ├── Controllers/             # API controllers
│   │   ├── appsettings.json         # Development settings
│   │   └── appsettings.Production.json # Production settings
│   ├── Harmoni360.Application/      # Business logic
│   ├── Harmoni360.Domain/           # Domain entities
│   └── Harmoni360.Infrastructure/   # Data access
├── docs/Deployment/Flyio/           # This documentation
├── scripts/                         # Deployment scripts
├── Dockerfile.flyio                 # Fly.io optimized Docker image
├── fly.toml.example                 # Fly.io configuration template
├── docker-compose.yml               # Local development
└── .github/workflows/               # CI/CD pipelines
```

## 🔧 Step 4: Initial Configuration

### 4.1 Configure Fly.io Application

```bash
# Create a new Fly.io application
flyctl apps create harmoni360-app

# Or use a custom name
flyctl apps create your-custom-app-name
```

### 4.2 Set Up PostgreSQL Database

```bash
# Create PostgreSQL database
flyctl postgres create --name harmoni360-db --region sjc

# Note the connection details provided
# Example output:
# Username: postgres
# Password: [generated-password]
# Hostname: harmoni360-db.internal
# Proxy port: 5432
# Postgres port: 5433
```

### 4.3 Set Up Redis Cache

```bash
# Create Redis instance
flyctl redis create --name harmoni360-cache --region sjc

# Note the connection details
# Example output:
# Redis URL: redis://default:[password]@harmoni360-cache.upstash.io:6379
```

### 4.4 Configure Application Secrets

```bash
# Set database connection string
flyctl secrets set ConnectionStrings__DefaultConnection="Host=harmoni360-db.internal;Port=5432;Database=harmoni360;Username=postgres;Password=[your-db-password]"

# Set Redis connection string
flyctl secrets set ConnectionStrings__Redis="[your-redis-url]"

# Generate and set JWT key (32+ characters)
flyctl secrets set Jwt__Key="$(openssl rand -base64 32)"

# Set environment
flyctl secrets set ASPNETCORE_ENVIRONMENT="Production"
```

## 🚀 Step 5: First Deployment

### 5.1 Deploy Application

```bash
# Deploy the application
flyctl deploy

# Monitor deployment progress
flyctl logs -f
```

### 5.2 Run Database Migrations

```bash
# Connect to the application and run migrations
flyctl ssh console -C "cd /app && dotnet ef database update"
```

### 5.3 Verify Deployment

```bash
# Check application status
flyctl status

# Test health endpoint
curl https://your-app-name.fly.dev/health

# Open application in browser
flyctl open
```

## 🔍 Step 6: Verification and Testing

### 6.1 Health Checks

```bash
# Check application health
curl -f https://your-app-name.fly.dev/health

# Expected response:
# {"status":"Healthy","totalDuration":"00:00:00.0123456"}
```

### 6.2 Database Connectivity

```bash
# Connect to database
flyctl postgres connect -a harmoni360-db

# Run test query
\dt
```

### 6.3 Application Functionality

1. **Open Application**: Visit `https://your-app-name.fly.dev`
2. **Test Login**: Use demo credentials (if demo mode enabled)
3. **Check API**: Visit `https://your-app-name.fly.dev/swagger`
4. **Monitor Logs**: Run `flyctl logs -f` in another terminal

## 📊 Step 7: Monitoring Setup

### 7.1 Enable Metrics

```bash
# View application metrics
flyctl metrics -a your-app-name

# View PostgreSQL metrics
flyctl metrics -a harmoni360-db
```

### 7.2 Set Up Log Monitoring

```bash
# Stream live logs
flyctl logs -f

# View historical logs
flyctl logs --since 1h
```

### 7.3 Configure Alerts (Optional)

1. Visit [Fly.io Dashboard](https://fly.io/dashboard)
2. Navigate to your application
3. Go to "Monitoring" tab
4. Set up alerts for:
   - High CPU usage (>80%)
   - High memory usage (>85%)
   - Application errors
   - Database connection issues

## 🔐 Step 8: Security Configuration

### 8.1 Review Security Settings

```bash
# Check current secrets (names only)
flyctl secrets list

# Verify SSL certificate
curl -I https://your-app-name.fly.dev
```

### 8.2 Configure Custom Domain (Optional)

```bash
# Add custom domain
flyctl certs create your-domain.com

# Add DNS records as instructed by Fly.io
# A record: your-domain.com -> [fly-ip]
# AAAA record: your-domain.com -> [fly-ipv6]
```

### 8.3 Security Best Practices

- ✅ Use strong, unique passwords for all services
- ✅ Enable two-factor authentication on Fly.io account
- ✅ Regularly rotate JWT keys and database passwords
- ✅ Monitor access logs and unusual activity
- ✅ Keep application dependencies updated

## 🎉 Next Steps

Congratulations! You've successfully set up your Fly.io deployment environment. Here's what to do next:

1. **Set Up CI/CD**: Follow [CI/CD Integration Guide](./CI_CD_Integration.md)
2. **Configure Environments**: Set up staging environment using [Environment Management](./Environment_Management.md)
3. **Enable Monitoring**: Implement comprehensive monitoring with [Monitoring and Logging](./Monitoring_and_Logging.md)
4. **Plan Scaling**: Review [Scaling and Performance](./Scaling_and_Performance.md) guide
5. **Backup Strategy**: Implement [Backup and Recovery](./Backup_and_Recovery.md) procedures

## 🆘 Troubleshooting

### Common Issues

**Issue**: `flyctl: command not found`
**Solution**: Ensure Fly CLI is installed and added to PATH

**Issue**: Authentication failed
**Solution**: Run `flyctl auth login` and complete browser authentication

**Issue**: Deployment fails with "insufficient resources"
**Solution**: Add payment method to Fly.io account

**Issue**: Database connection timeout
**Solution**: Verify database is running with `flyctl status -a harmoni360-db`

For more detailed troubleshooting, see [Troubleshooting Guide](./Troubleshooting.md).

---

*Next: [Manual Deployment Guide](./Manual_Deployment.md)*
