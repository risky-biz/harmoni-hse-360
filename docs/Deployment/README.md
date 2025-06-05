# HarmoniHSE360 Deployment Documentation

This directory contains comprehensive deployment documentation for the HarmoniHSE360 application, focusing on cloud deployment strategies and production-ready configurations.

## 📋 Documentation Overview

### Core Deployment Guides

| Document | Description | Audience |
|----------|-------------|----------|
| [Fly.io Deployment Guide](./Fly_io_Deployment_Guide.md) | Complete step-by-step manual deployment to Fly.io | DevOps, Developers |
| [GitHub Actions CI/CD Guide](./GitHub_Actions_CI_CD_Guide.md) | **Automated CI/CD pipeline implementation with workflow fixes** | **DevOps, Developers** |
| [Comprehensive Troubleshooting Guide](./Troubleshooting_Guide.md) | **All deployment and CI/CD issues with monitoring procedures** | **All Users** |
| [Fly.io Token Documentation Summary](./Flyio_Token_Documentation_Summary.md) | Complete overview of all token-related documentation | All Users |
| [Quick Reference](./Quick_Reference.md) | Essential commands and troubleshooting | All Users |
| [Demo Preparation Guide](./Demo_Preparation_Guide.md) | Client demo setup and scenarios | Sales, Management |
| [Deployment Checklist](./Deployment_Checklist.md) | Comprehensive verification checklist | DevOps, QA |

## 🚀 Quick Start

### Prerequisites
- Fly.io account with payment method
- Docker Desktop installed
- .NET 8 SDK installed
- Node.js 20+ installed

### Automated Deployment (Manual)
```bash
# Linux/macOS
chmod +x scripts/deploy-flyio.sh
./scripts/deploy-flyio.sh

# Windows PowerShell
.\scripts\deploy-flyio.ps1
```

### CI/CD Automated Deployment
```bash
# Set up GitHub Actions CI/CD pipeline
# 1. Configure repository secrets
# 2. Push to develop branch (deploys to staging)
# 3. Push to main branch (deploys to production)

# Manual trigger
gh workflow run deploy.yml --ref main
```

### Manual Deployment
Follow the [Fly.io Deployment Guide](./Fly_io_Deployment_Guide.md) for detailed instructions.

## 🏗️ Architecture Overview

### Technology Stack
- **Backend:** .NET 8 ASP.NET Core
- **Frontend:** React 18 + TypeScript
- **Database:** PostgreSQL 15+
- **Cache:** Redis 7.2
- **Real-time:** SignalR
- **Containerization:** Docker with Alpine Linux

### Deployment Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Fly.io App    │    │  PostgreSQL DB  │    │   Redis Cache   │
│  (Container)    │◄──►│   (Fly.io)      │    │   (Upstash)     │
│                 │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │
         ▼
┌─────────────────┐
│ Persistent Vol  │
│   (Uploads)     │
└─────────────────┘
```

## 📊 Deployment Options Comparison

| Platform | Cost | Complexity | Features | Recommendation |
|----------|------|------------|----------|----------------|
| **Fly.io** | Free tier available | Medium | Full Docker support, Global CDN | ⭐⭐⭐⭐⭐ **Recommended** |
| Render.com | Free with limitations | Low | Easy setup, 30-day DB limit | ⭐⭐⭐⭐ Good alternative |
| Railway.app | $5/month minimum | Low | Excellent DX, no free tier | ⭐⭐⭐ Paid option |
| Azure Container Apps | ~$25/month | High | Enterprise features | ⭐⭐⭐ Enterprise choice |

## 🔧 Configuration Files

### Essential Files
- `Dockerfile.flyio` - Optimized Docker configuration for Fly.io
- `fly.toml.example` - Fly.io application configuration template
- `appsettings.Production.json` - Production environment settings
- `scripts/deploy-flyio.sh` - Automated deployment script (Linux/macOS)
- `scripts/deploy-flyio.ps1` - Automated deployment script (Windows)

### Environment Variables
| Variable | Description | Required |
|----------|-------------|----------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | ✅ |
| `ConnectionStrings__Redis` | Redis connection string | ✅ |
| `Jwt__Key` | JWT signing key (32+ characters) | ✅ |
| `ASPNETCORE_ENVIRONMENT` | Environment name (Production) | ✅ |

## 📈 Monitoring and Maintenance

### Health Monitoring
- **Health Endpoint:** `/health`
- **Metrics:** Available via Fly.io dashboard
- **Logs:** Real-time via `fly logs -f`

### Backup Strategy
- **Database:** Automated daily backups via Fly.io
- **Application:** Version control + container registry
- **Uploads:** Persistent volume snapshots

### Scaling Guidelines
| Metric | Threshold | Action |
|--------|-----------|--------|
| CPU Usage | >80% | Scale up CPU or add instances |
| Memory Usage | >85% | Increase memory allocation |
| Response Time | >3s | Add instances or optimize |
| Error Rate | >1% | Investigate and fix issues |

## 🎯 Demo Environment

### Demo URLs
- **Application:** https://harmonihse360-app.fly.dev
- **Health Check:** https://harmonihse360-app.fly.dev/health
- **API Docs:** https://harmonihse360-app.fly.dev/swagger

### Demo Accounts
| Role | Email | Password | Purpose |
|------|-------|----------|---------|
| Admin | admin@harmonihse360.com | Admin123! | Full access |
| Manager | manager@harmonihse360.com | Manager123! | Management |
| Officer | officer@harmonihse360.com | Officer123! | Operations |
| Employee | employee@harmonihse360.com | Employee123! | Basic user |

## 🔍 Troubleshooting Quick Reference

### Common Commands
```bash
# Check application status
fly status -a harmonihse360-app

# View logs
fly logs -a harmonihse360-app

# Restart application
fly restart -a harmonihse360-app

# Access console
fly ssh console -a harmonihse360-app

# Check database
fly postgres connect -a harmonihse360-db

# Monitor metrics
fly metrics -a harmonihse360-app
```

### Emergency Procedures
1. **Application Down:** Check logs, restart if needed
2. **Database Issues:** Verify connection, check backups
3. **Performance Problems:** Scale resources, check metrics
4. **SSL Issues:** Recreate certificates, verify DNS

## 📚 Additional Resources

### External Documentation
- [Fly.io Documentation](https://fly.io/docs/)
- [.NET 8 Deployment Guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)

### Internal Documentation
- [Getting Started Guide](../Guides/Getting_Started_Guide.md)
- [Docker Guide](../Guides/Docker_Guide.md)
- [Authentication Guide](../Guides/Authentication_Guide.md)

## 🤝 Support

### Getting Help
- **Technical Issues:** Create issue in repository
- **Deployment Problems:** Check troubleshooting guide
- **Fly.io Issues:** Visit [Fly.io Community](https://community.fly.io/)

### Contributing
- Follow deployment best practices
- Update documentation when making changes
- Test deployment scripts before committing
- Document any new configuration requirements

---

## 📝 Document Status

| Document | Status | Last Updated | Reviewer |
|----------|--------|--------------|----------|
| Fly.io Deployment Guide | ✅ Complete | Jan 2025 | Tech Lead |
| GitHub Actions CI/CD Guide | ✅ Complete | Jan 2025 | DevOps |
| Comprehensive Troubleshooting Guide | ✅ Complete | Jan 2025 | DevOps |
| Fly.io Token Documentation Summary | ✅ Complete | Jan 2025 | Tech Lead |
| Demo Preparation Guide | ✅ Complete | Jan 2025 | Sales Team |

### Consolidation Summary

**Files Removed (Consolidated):**
- `CI_CD_Troubleshooting_Guide.md` → Merged into `Troubleshooting_Guide.md`
- `CI_CD_Monitoring_Guide.md` → Merged into `Troubleshooting_Guide.md`
- `Workflow_Fixes_Summary.md` → Merged into `GitHub_Actions_CI_CD_Guide.md`
- `CI_CD_Legacy_Workflow_Fixes.md` → Merged into `GitHub_Actions_CI_CD_Guide.md`

**Result:** Reduced from 12 files to 8 files while maintaining all essential information.

---

*For questions or improvements to this documentation, please create an issue or submit a pull request.*
