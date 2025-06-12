# Harmoni360 Deployment Documentation

This directory contains comprehensive deployment documentation for the Harmoni360 application, focusing on cloud deployment strategies and production-ready configurations.

## 📋 Documentation Overview

### Deployment Options

HarmoniHSE360 supports three distinct deployment scenarios to meet different organizational needs:

#### 1. 🌐 Cloud Deployment (Fly.io)
**Best for**: Demo environments, staging, small to medium production deployments
- **Location**: [Flyio/](./Flyio/)
- **Features**: Global CDN, automatic scaling, zero-downtime deployments
- **Cost**: Free tier available, pay-as-you-scale
- **Complexity**: Medium

#### 2. 🐳 Standalone Server with Docker
**Best for**: On-premises production, enterprise environments, full control
- **Location**: [Standalone_Server/Docker/](./Standalone_Server/Docker/)
- **Features**: Complete containerization, production-ready orchestration
- **Cost**: Hardware + maintenance costs
- **Complexity**: Medium-High

#### 3. ⚙️ Standalone Server without Docker
**Best for**: Traditional IT environments, legacy infrastructure, maximum control
- **Location**: [Standalone_Server/Manual/](./Standalone_Server/Manual/)
- **Features**: Direct installation, system service management
- **Cost**: Hardware + maintenance costs
- **Complexity**: High

### Core Documentation

| Document | Description | Audience |
|----------|-------------|----------|
| **[Infrastructure Overview](./Infrastructure_Overview.md)** | **Complete system architecture and deployment strategy** | **DevOps Engineers, Architects** |
| **[Environment Configuration](./Environment_Configuration.md)** | **Environment variables and secrets management** | **DevOps Engineers, Developers** |
| **[Security Best Practices](./Security_Best_Practices.md)** | **Security considerations and compliance guidelines** | **Security Engineers, DevOps** |
| [Troubleshooting Guide](./Troubleshooting_Guide.md) | Common issues and solutions | All Users |
| [Quick Reference](./Quick_Reference.md) | Essential commands and shortcuts | All Users |
| [Deployment Checklist](./Deployment_Checklist.md) | Pre-deployment verification checklist | DevOps, QA |

## 🚀 Quick Start Options

### 1. 🌐 Cloud Deployment (Fly.io) - Recommended for Demos/Staging
```bash
# Automated CI/CD deployment
# 1. Configure FLY_API_TOKEN secret in GitHub repository
# 2. Push to develop branch → deploys to staging automatically
# 3. Push to main branch → deploys to production with approval

# Manual deployment
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360
chmod +x scripts/deploy-flyio.sh
./scripts/deploy-flyio.sh
```

### 2. 🐳 Docker Standalone - Recommended for Production
```bash
# Automated Docker deployment
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360
chmod +x scripts/deploy-standalone.sh
sudo ./scripts/deploy-standalone.sh

# Manual Docker deployment
cp .env.prod.example .env.prod
# Edit .env.prod with your configuration
docker-compose -f docker-compose.prod.yml up -d
```

### 3. ⚙️ Manual Standalone - Maximum Control
```bash
# Traditional deployment without containers
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360
chmod +x scripts/install-manual.sh
sudo ./scripts/install-manual.sh
```

### Prerequisites
- [Fly.io CLI](https://fly.io/docs/hands-on/install-flyctl/) with authenticated account
- [Docker](https://docs.docker.com/get-docker/) (20.10+)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (20.x LTS)
- Payment method configured in Fly.io account

## 🏗️ Infrastructure Architecture

### Technology Stack
- **Backend:** .NET 8 ASP.NET Core Web API
- **Frontend:** React 18 + TypeScript SPA
- **Database:** PostgreSQL 15+ with Entity Framework Core
- **Cache:** Redis 7.2 for session and data caching
- **Real-time:** SignalR for live notifications
- **Containerization:** Docker with Alpine Linux (multi-stage builds)
- **Deployment:** Fly.io with global edge deployment
- **CI/CD:** GitHub Actions with automated testing and security scanning

### Deployment Environments

| Deployment Type | Environment | URL/Location | Branch | Auto-Deploy | Resources |
|-----------------|-------------|--------------|--------|-------------|-----------|
| **Fly.io Cloud** | Staging | `harmoni360-staging.fly.dev` | `develop` | ✅ | 1 CPU, 512MB |
| **Fly.io Cloud** | Production | `harmoni360-app.fly.dev` | `main` | ✅ (with approval) | 1 CPU, 1GB |
| **Docker Standalone** | Production | `your-domain.com` | `main` | Optional | 8+ CPU, 32GB+ RAM |
| **Manual Standalone** | Production | `your-domain.com` | `main` | Manual | 8+ CPU, 32GB+ RAM |

### System Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Fly.io CDN    │    │  GitHub Actions │    │   Monitoring    │
│ (Global Edge)   │    │    (CI/CD)      │    │   & Logging     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Harmoni360   │    │  PostgreSQL DB  │    │   Redis Cache   │
│   Container     │◄──►│   (Fly.io)      │    │   (Fly.io)      │
│ (.NET + React)  │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │
         ▼
┌─────────────────┐
│ Persistent Vol  │
│ (File Uploads)  │
└─────────────────┘
```

## 📊 Deployment Options Comparison

| Deployment Option | Cost | Complexity | Scalability | Control | Best For |
|-------------------|------|------------|-------------|---------|----------|
| **🌐 Fly.io Cloud** | $13-28/month | Medium | Auto-scaling | Medium | Demos, small-medium prod |
| **🐳 Docker Standalone** | $15K+ initial | Medium-High | Manual scaling | High | Enterprise, on-premises |
| **⚙️ Manual Standalone** | $12K+ initial | High | Manual scaling | Maximum | Legacy environments, compliance |

### Detailed Comparison

#### Fly.io Cloud Deployment
- **Pros**: Quick setup, global CDN, automatic scaling, managed services
- **Cons**: Vendor lock-in, limited customization, ongoing costs
- **Ideal for**: Demos, staging, small to medium production deployments

#### Docker Standalone Deployment
- **Pros**: Full control, containerized consistency, easier scaling, comprehensive monitoring
- **Cons**: Requires Docker expertise, more complex setup, hardware costs
- **Ideal for**: Production environments, enterprise deployments, on-premises requirements

#### Manual Standalone Deployment
- **Pros**: Maximum control, no container dependencies, traditional IT practices
- **Cons**: Complex setup, manual dependency management, harder to scale
- **Ideal for**: Legacy environments, strict compliance requirements, maximum customization

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
- **Application:** https://harmoni360-app.fly.dev
- **Health Check:** https://harmoni360-app.fly.dev/health
- **API Docs:** https://harmoni360-app.fly.dev/swagger

### Demo Accounts
| Role | Email | Password | Purpose |
|------|-------|----------|---------|
| Admin | admin@harmoni360.com | Admin123! | Full access |
| Manager | manager@harmoni360.com | Manager123! | Management |
| Officer | officer@harmoni360.com | Officer123! | Operations |
| Employee | employee@harmoni360.com | Employee123! | Basic user |

## 🔍 Troubleshooting Quick Reference

### Common Commands
```bash
# Check application status
fly status -a harmoni360-app

# View logs
fly logs -a harmoni360-app

# Restart application
fly restart -a harmoni360-app

# Access console
fly ssh console -a harmoni360-app

# Check database
fly postgres connect -a harmoni360-db

# Monitor metrics
fly metrics -a harmoni360-app
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
