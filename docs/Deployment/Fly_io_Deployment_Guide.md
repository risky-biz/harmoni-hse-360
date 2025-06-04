# HarmoniHSE360 Fly.io Deployment Guide

## Table of Contents

1. [Prerequisites and Setup](#prerequisites-and-setup)
2. [Application Preparation](#application-preparation)
3. [Infrastructure Deployment](#infrastructure-deployment)
4. [Application Deployment](#application-deployment)
5. [Post-Deployment Configuration](#post-deployment-configuration)
6. [Troubleshooting and Maintenance](#troubleshooting-and-maintenance)
7. [Demo Preparation](#demo-preparation)

---

## Prerequisites and Setup

### 1.1 Fly.io Account Creation

1. **Create Fly.io Account**
   - Visit [https://fly.io/app/sign-up](https://fly.io/app/sign-up)
   - Sign up using GitHub, Google, or email
   - Verify your email address

2. **Add Payment Method** (Required even for free tier)
   - Go to [Billing Settings](https://fly.io/dashboard/billing)
   - Add a credit card (you won't be charged within free tier limits)

### 1.2 Install Fly CLI

**Windows (PowerShell):**
```powershell
iwr https://fly.io/install.ps1 -useb | iex
```

**macOS/Linux:**
```bash
curl -L https://fly.io/install.sh | sh
```

**Verify Installation:**
```bash
fly version
```

### 1.3 Authenticate with Fly.io

```bash
fly auth login
```

### 1.4 Required Tools

Ensure you have the following installed:
- **Docker Desktop** (latest version)
- **.NET 8 SDK**
- **Node.js 20+**
- **Git**

### 1.5 Local Development Environment

1. **Clone and Setup Project:**
   ```bash
   git clone https://github.com/risky-biz/harmoni-hse-360.git
   cd harmoni-hse-360
   ```

2. **Verify Local Build:**
   ```bash
   docker build -t harmonihse360:local .
   ```

---

## Application Preparation

### 2.1 Docker Configuration Optimization

Create an optimized Dockerfile for Fly.io deployment:

**Create `Dockerfile.flyio`:**
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Install Node.js for React build
RUN apk add --no-cache nodejs npm

# Copy csproj files and restore
COPY ["src/HarmoniHSE360.Web/HarmoniHSE360.Web.csproj", "HarmoniHSE360.Web/"]
COPY ["src/HarmoniHSE360.Application/HarmoniHSE360.Application.csproj", "HarmoniHSE360.Application/"]
COPY ["src/HarmoniHSE360.Domain/HarmoniHSE360.Domain.csproj", "HarmoniHSE360.Domain/"]
COPY ["src/HarmoniHSE360.Infrastructure/HarmoniHSE360.Infrastructure.csproj", "HarmoniHSE360.Infrastructure/"]
RUN dotnet restore "HarmoniHSE360.Web/HarmoniHSE360.Web.csproj"

# Copy everything else
COPY src/ .

# Build React app
WORKDIR /src/HarmoniHSE360.Web/ClientApp
RUN npm ci --only=production
RUN npm run build

# Build .NET app
WORKDIR /src/HarmoniHSE360.Web
RUN dotnet publish "HarmoniHSE360.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Install cultures for globalization
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Create non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# Copy published files
COPY --from=build /app/publish .

# Create uploads directory and set permissions
RUN mkdir -p uploads && chown -R appuser:appgroup uploads

# Set user
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "HarmoniHSE360.Web.dll"]
```

### 2.2 Environment Variables Configuration

**Create `appsettings.Production.json`:**
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
  "ConnectionStrings": {
    "DefaultConnection": "",
    "Redis": ""
  },
  "Jwt": {
    "Key": "",
    "Issuer": "HarmoniHSE360",
    "Audience": "HarmoniHSE360Users",
    "ExpirationMinutes": "60",
    "RefreshTokenExpirationDays": "7"
  }
}
```

### 2.3 Database Migration Preparation

**Create migration script `scripts/migrate.sh`:**
```bash
#!/bin/bash
echo "Running database migrations..."
dotnet ef database update --project src/HarmoniHSE360.Infrastructure --startup-project src/HarmoniHSE360.Web
echo "Database migrations completed."
```

---

## Infrastructure Deployment

### 3.1 PostgreSQL Database Setup

1. **Create PostgreSQL Cluster:**
   ```bash
   fly postgres create --name harmonihse360-db --region sjc
   ```

2. **Note the Connection Details:**
   - Save the connection string provided
   - Note the database name, username, and password

3. **Verify Database Connection:**
   ```bash
   fly postgres connect -a harmonihse360-db
   ```

### 3.2 Redis Setup via Upstash

1. **Create Redis Instance:**
   ```bash
   fly ext redis create --name harmonihse360-redis
   ```

2. **Note Redis Connection String:**
   - Save the Redis URL provided
   - This will be used in environment variables

### 3.3 Network Configuration

The database and Redis will be automatically configured on Fly.io's private network. No additional network setup is required.

---

## Application Deployment

### 4.1 Initialize Fly Application

1. **Initialize Fly App:**
   ```bash
   fly launch --no-deploy --name harmonihse360-app --region sjc
   ```

2. **Configure fly.toml:**
   ```toml
   app = "harmonihse360-app"
   primary_region = "sjc"

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

     [services.concurrency]
       type = "connections"
       hard_limit = 25
       soft_limit = 20

   [[services.http_checks]]
     interval = "10s"
     grace_period = "5s"
     method = "GET"
     path = "/health"
     protocol = "http"
     timeout = "2s"
     tls_skip_verify = false

   [mounts]
     source = "harmonihse360_uploads"
     destination = "/app/uploads"
   ```

### 4.2 Create Persistent Volume

```bash
fly volumes create harmonihse360_uploads --region sjc --size 1
```

### 4.3 Configure Secrets

1. **Set Database Connection:**
   ```bash
   fly secrets set ConnectionStrings__DefaultConnection="postgres://username:password@hostname:5432/database"
   ```

2. **Set Redis Connection:**
   ```bash
   fly secrets set ConnectionStrings__Redis="redis://username:password@hostname:6379"
   ```

3. **Set JWT Key:**
   ```bash
   fly secrets set Jwt__Key="YourSuperSecretProductionJwtKeyThatMustBeAtLeast32CharactersLong!"
   ```

### 4.4 Deploy Application

1. **Build and Deploy:**
   ```bash
   fly deploy
   ```

2. **Monitor Deployment:**
   ```bash
   fly logs
   ```

### 4.5 Custom Domain Setup

1. **Add Custom Domain:**
   ```bash
   fly certs create harmonihse360.yourdomain.com
   ```

2. **Configure DNS:**
   - Add CNAME record pointing to your Fly.io app
   - SSL certificate will be automatically provisioned

---

## Post-Deployment Configuration

### 5.1 Database Migration

1. **Run Migrations:**
   ```bash
   fly ssh console
   cd /app
   dotnet ef database update
   ```

### 5.2 Health Checks Setup

The health check is already configured in `fly.toml`. Verify it's working:

```bash
fly status
```

### 5.3 Performance Optimization

1. **Scale Application (if needed):**
   ```bash
   fly scale count 2
   ```

2. **Monitor Resource Usage:**
   ```bash
   fly metrics
   ```

---

## Troubleshooting and Maintenance

### 6.1 Common Issues

**Issue: Application won't start**
```bash
# Check logs
fly logs

# Check app status
fly status

# Restart app
fly restart
```

**Issue: Database connection failed**
```bash
# Verify database is running
fly status -a harmonihse360-db

# Check connection string
fly secrets list
```

### 6.2 Monitoring and Logging

1. **View Real-time Logs:**
   ```bash
   fly logs -f
   ```

2. **Access Application Metrics:**
   ```bash
   fly dashboard
   ```

### 6.3 Backup Procedures

1. **Database Backup:**
   ```bash
   fly postgres backup create -a harmonihse360-db
   ```

2. **List Backups:**
   ```bash
   fly postgres backup list -a harmonihse360-db
   ```

---

## Demo Preparation

### 7.1 Sample Data Loading

1. **Access Application Console:**
   ```bash
   fly ssh console -a harmonihse360-app
   ```

2. **Run Data Seeding:**
   ```bash
   cd /app
   dotnet run --seed-data
   ```

### 7.2 Demo User Accounts

Create the following demo accounts:

- **Admin User:** admin@harmonihse360.com / Admin123!
- **Manager User:** manager@harmonihse360.com / Manager123!
- **Employee User:** employee@harmonihse360.com / Employee123!

### 7.3 Testing Checklist

- [ ] Application loads successfully
- [ ] User authentication works
- [ ] Database operations function
- [ ] Real-time features (SignalR) work
- [ ] File upload functionality
- [ ] All major features accessible
- [ ] Performance is acceptable
- [ ] SSL certificate is valid

### 7.4 Demo Environment URLs

- **Application:** https://harmonihse360-app.fly.dev
- **Custom Domain:** https://harmonihse360.yourdomain.com
- **Health Check:** https://harmonihse360-app.fly.dev/health

---

## Maintenance Commands

**Update Application:**
```bash
fly deploy
```

**Scale Resources:**
```bash
fly scale memory 1024  # Scale to 1GB RAM
fly scale count 2      # Scale to 2 instances
```

**Monitor Performance:**
```bash
fly metrics
fly status
```

**Access Database:**
```bash
fly postgres connect -a harmonihse360-db
```

---

## Related Documentation

### Deployment Documentation Suite
- **[Troubleshooting Guide](./Troubleshooting_Guide.md)** - Common issues and solutions
- **[Demo Preparation Guide](./Demo_Preparation_Guide.md)** - Client demo setup
- **[Deployment Checklist](./Deployment_Checklist.md)** - Comprehensive verification checklist
- **[Deployment README](./README.md)** - Overview and quick reference

### Automated Deployment
- **Linux/macOS:** `./scripts/deploy-flyio.sh`
- **Windows:** `.\scripts\deploy-flyio.ps1`

## Support and Resources

### External Resources
- **Fly.io Documentation:** https://fly.io/docs/
- **Fly.io Community:** https://community.fly.io/
- **Fly.io Status:** https://status.fly.io/

### Internal Resources
- **HarmoniHSE360 Repository:** https://github.com/risky-biz/harmoni-hse-360
- **Getting Started Guide:** [../Guides/Getting_Started_Guide.md](../Guides/Getting_Started_Guide.md)
- **Docker Guide:** [../Guides/Docker_Guide.md](../Guides/Docker_Guide.md)

---

*Last Updated: January 2025*
*Version: 1.0*
*Part of HarmoniHSE360 Deployment Documentation Suite*
