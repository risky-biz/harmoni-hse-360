# Harmoni360 Deployment Quick Reference

## 🚀 One-Command Deployment

### Automated Deployment
```bash
# Linux/macOS
./scripts/deploy-flyio.sh

# Windows PowerShell
.\scripts\deploy-flyio.ps1
```

## 📋 Essential Commands

### Initial Setup
```bash
# Install Fly CLI
curl -L https://fly.io/install.sh | sh

# Authenticate
fly auth login

# Initialize project
fly launch --no-deploy --name harmoni360-app
```

### Database & Redis
```bash
# Create PostgreSQL
fly postgres create --name harmoni360-db --region sjc

# Create Redis
fly ext redis create --name harmoni360-redis

# Connect to database
fly postgres connect -a harmoni360-db
```

### Application Management
```bash
# Deploy application
fly deploy -a harmoni360-app

# Check status
fly status -a harmoni360-app

# View logs
fly logs -a harmoni360-app

# Restart app
fly restart -a harmoni360-app
```

### Secrets Management
```bash
# Set database connection
fly secrets set ConnectionStrings__DefaultConnection="postgres://..." -a harmoni360-app

# Set Redis connection
fly secrets set ConnectionStrings__Redis="redis://..." -a harmoni360-app

# Set JWT key
fly secrets set Jwt__Key="your-secret-key" -a harmoni360-app

# List secrets
fly secrets list -a harmoni360-app
```

### Scaling & Performance
```bash
# Scale instances
fly scale count 2 -a harmoni360-app

# Scale memory
fly scale memory 1024 -a harmoni360-app

# View metrics
fly metrics -a harmoni360-app
```

### Volumes & Storage
```bash
# Create volume
fly volumes create harmoni360_uploads --region sjc --size 1 -a harmoni360-app

# List volumes
fly volumes list -a harmoni360-app
```

### SSL & Domains
```bash
# Add custom domain
fly certs create yourdomain.com -a harmoni360-app

# List certificates
fly certs list -a harmoni360-app
```

## 🔧 Troubleshooting Commands

### Diagnostics
```bash
# SSH into container
fly ssh console -a harmoni360-app

# Run database migrations
fly ssh console -a harmoni360-app -C "cd /app && dotnet ef database update"

# Test health endpoint
curl https://harmoni360-app.fly.dev/health

# Check machine status
fly machine list -a harmoni360-app
```

### Recovery
```bash
# Rollback deployment
fly releases -a harmoni360-app
fly rollback -a harmoni360-app --version X

# Force redeploy
fly deploy --force -a harmoni360-app

# Restart specific machine
fly machine restart MACHINE_ID -a harmoni360-app
```

## 📊 Monitoring URLs

| Service | URL | Purpose |
|---------|-----|---------|
| Application | https://harmoni360-app.fly.dev | Main application |
| Health Check | https://harmoni360-app.fly.dev/health | Health status |
| API Docs | https://harmoni360-app.fly.dev/swagger | API documentation |
| Fly Dashboard | https://fly.io/apps/harmoni360-app | Fly.io management |

## 🔐 Demo Accounts

| Role | Email | Password | Access Level |
|------|-------|----------|--------------|
| Admin | admin@harmoni360.com | Admin123! | Full system access |
| Manager | manager@harmoni360.com | Manager123! | Management features |
| Officer | officer@harmoni360.com | Officer123! | Field operations |
| Employee | employee@harmoni360.com | Employee123! | Basic user access |

## 📁 Configuration Files

| File | Purpose | Location |
|------|---------|----------|
| `fly.toml` | Fly.io app configuration | Project root |
| `Dockerfile.flyio` | Optimized Docker build | Project root |
| `appsettings.Production.json` | Production settings | src/Harmoni360.Web/ |

## 🚨 Emergency Procedures

### Application Down
```bash
# 1. Check status
fly status -a harmoni360-app

# 2. Check logs
fly logs -a harmoni360-app

# 3. Restart if needed
fly restart -a harmoni360-app
```

### Database Issues
```bash
# 1. Check database status
fly status -a harmoni360-db

# 2. Test connection
fly postgres connect -a harmoni360-db

# 3. Check backups
fly postgres backup list -a harmoni360-db
```

### Performance Issues
```bash
# 1. Check metrics
fly metrics -a harmoni360-app

# 2. Scale up temporarily
fly scale count 2 -a harmoni360-app
fly scale memory 1024 -a harmoni360-app

# 3. Monitor improvement
fly logs -f -a harmoni360-app
```

## 📞 Support Contacts

| Issue Type | Contact | URL |
|------------|---------|-----|
| Fly.io Platform | Community Forum | https://community.fly.io/ |
| Fly.io Status | Status Page | https://status.fly.io/ |
| Application Issues | GitHub Issues | https://github.com/risky-biz/harmoni-hse-360/issues |

## 🔄 Regular Maintenance

### Daily
- [ ] Check application health
- [ ] Monitor error logs
- [ ] Verify backup completion

### Weekly
- [ ] Review performance metrics
- [ ] Check resource usage
- [ ] Update dependencies if needed

### Monthly
- [ ] Security audit
- [ ] Cost optimization review
- [ ] Documentation updates

## 📚 Documentation Links

- **[Full Deployment Guide](./Fly_io_Deployment_Guide.md)** - Complete step-by-step instructions
- **[Troubleshooting Guide](./Troubleshooting_Guide.md)** - Common issues and solutions
- **[Demo Preparation](./Demo_Preparation_Guide.md)** - Client demo setup
- **[Deployment Checklist](./Deployment_Checklist.md)** - Verification checklist

---

*Keep this reference handy for quick deployment operations and troubleshooting.*
