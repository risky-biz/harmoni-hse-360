# HarmoniHSE360 Fly.io Cloud Deployment Guide

## Overview

This comprehensive guide covers deploying HarmoniHSE360 to Fly.io, a modern cloud platform that provides global edge deployment with automatic scaling and zero-downtime deployments. This deployment option is ideal for demo environments, staging, and small to medium production deployments.

## 📚 Documentation Index

### Getting Started
- **[README.md](./README.md)** - Main deployment guide (this file)
- **[Getting_Started.md](./Getting_Started.md)** - Initial setup and account configuration
- **[Prerequisites.md](./Prerequisites.md)** - System requirements and dependencies

### Deployment Process
- **[Manual_Deployment.md](./Manual_Deployment.md)** - Step-by-step manual deployment
- **[CI_CD_Integration.md](./CI_CD_Integration.md)** - GitHub Actions automation
- **[Environment_Management.md](./Environment_Management.md)** - Secrets and configuration

### Operations
- **[Monitoring_and_Logging.md](./Monitoring_and_Logging.md)** - Observability setup
- **[Scaling_and_Performance.md](./Scaling_and_Performance.md)** - Performance optimization
- **[Backup_and_Recovery.md](./Backup_and_Recovery.md)** - Data protection strategies
- **[Troubleshooting.md](./Troubleshooting.md)** - Common issues and solutions

## 🚀 Quick Start

### Option 1: Automated CI/CD Deployment (Recommended)

```bash
# 1. Fork the repository and configure secrets
# 2. Push to develop branch → deploys to staging
# 3. Push to main branch → deploys to production (with approval)

# Manual trigger via GitHub CLI
gh workflow run deploy.yml --ref main -f environment=production
```

### Option 2: One-Command Manual Deployment

```bash
# Clone repository
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360

# Run deployment script
chmod +x scripts/deploy-flyio.sh
./scripts/deploy-flyio.sh
```

### Option 3: Step-by-Step Manual Process

Follow the [Manual Deployment Guide](./Manual_Deployment.md) for complete control.

## 🏗️ Architecture Overview

### Technology Stack
- **Backend**: .NET 8 ASP.NET Core Web API
- **Frontend**: React 18 + TypeScript SPA
- **Database**: PostgreSQL 15+ (Fly.io managed)
- **Cache**: Redis 7.2 (Fly.io managed)
- **Real-time**: SignalR for live notifications
- **Container**: Docker with Alpine Linux (optimized for Fly.io)
- **CDN**: Fly.io global edge network

### Deployment Environments

| Environment | URL | Branch | Auto-Deploy | Resources | Purpose |
|-------------|-----|--------|-------------|-----------|---------|
| **Staging** | `harmoni360-staging.fly.dev` | `develop` | ✅ | 1 CPU, 512MB | Testing, demos |
| **Production** | `harmoni360-app.fly.dev` | `main` | ✅ (with approval) | 1 CPU, 1GB | Live application |

### System Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Fly.io CDN    │    │  GitHub Actions │    │   Monitoring    │
│ (Global Edge)   │    │    (CI/CD)      │    │   & Logging     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ HarmoniHSE360   │    │  PostgreSQL DB  │    │   Redis Cache   │
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

## 💰 Cost Analysis

### Fly.io Pricing (as of 2025)

| Resource | Free Tier | Paid Tier | Staging Cost | Production Cost |
|----------|-----------|-----------|--------------|-----------------|
| **App Instance** | 3 shared-cpu-1x | $1.94/month per 256MB | ~$3.88/month | ~$7.76/month |
| **PostgreSQL** | 3GB storage | $1.94/month per 1GB | ~$5.82/month | ~$11.64/month |
| **Redis** | 256MB | $1.94/month per 256MB | ~$1.94/month | ~$3.88/month |
| **Volumes** | 3GB | $0.15/month per GB | ~$1.50/month | ~$3.00/month |
| **Bandwidth** | 100GB | $0.02/GB | Included | ~$2.00/month |

**Total Monthly Cost:**
- **Staging**: ~$13.14/month
- **Production**: ~$28.28/month

## 🔧 Prerequisites

### Required Accounts and Tools
- [Fly.io account](https://fly.io/app/sign-up) with payment method
- [GitHub account](https://github.com) for CI/CD
- [Fly CLI](https://fly.io/docs/hands-on/install-flyctl/) installed locally
- [Docker](https://docs.docker.com/get-docker/) (20.10+) for local testing
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) for development
- [Node.js](https://nodejs.org/) (20.x LTS) for frontend development

### System Requirements
- **Local Development**: 8GB RAM, 20GB free disk space
- **Internet**: Stable connection for deployment and monitoring

## 🎯 Deployment Scenarios

### Scenario 1: Demo/Staging Environment
**Use Case**: Product demonstrations, user acceptance testing, development staging
- **Resources**: Minimal (shared-cpu-1x, 512MB RAM)
- **Database**: Small PostgreSQL instance (3GB)
- **Features**: Demo mode enabled, sample data included
- **Cost**: ~$13/month

### Scenario 2: Small Production Environment
**Use Case**: Small teams (10-50 users), pilot implementations
- **Resources**: Dedicated CPU, 1GB RAM
- **Database**: Standard PostgreSQL (10GB)
- **Features**: Full production features, monitoring enabled
- **Cost**: ~$28/month

### Scenario 3: Medium Production Environment
**Use Case**: Medium organizations (50-200 users), full deployment
- **Resources**: Multiple instances, 2GB RAM each
- **Database**: High-performance PostgreSQL (50GB)
- **Features**: High availability, advanced monitoring, backups
- **Cost**: ~$75/month

## 📋 Pre-Deployment Checklist

### Account Setup
- [ ] Fly.io account created and verified
- [ ] Payment method added to Fly.io account
- [ ] GitHub repository forked or cloned
- [ ] Fly CLI installed and authenticated

### Configuration
- [ ] Domain name configured (optional)
- [ ] SSL certificate requirements determined
- [ ] Environment variables planned
- [ ] Database backup strategy defined

### Security
- [ ] JWT secret key generated (32+ characters)
- [ ] Database passwords generated
- [ ] GitHub secrets configured
- [ ] Access control policies defined

## 🔗 Quick Links

### Essential URLs
- **Fly.io Dashboard**: https://fly.io/dashboard
- **Fly.io Documentation**: https://fly.io/docs/
- **GitHub Actions**: https://github.com/risky-biz/harmoni-hse-360/actions
- **Application Health**: https://harmoni360-app.fly.dev/health

### Configuration Files
- [`fly.toml.example`](../../../fly.toml.example) - Fly.io app configuration
- [`Dockerfile.flyio`](../../../Dockerfile.flyio) - Optimized Docker image
- [`.github/workflows/deploy.yml`](../../../.github/workflows/deploy.yml) - CI/CD pipeline
- [`appsettings.Production.json`](../../../src/Harmoni360.Web/appsettings.Production.json) - App configuration

## 📞 Support and Resources

### Getting Help
- **Fly.io Community**: https://community.fly.io/
- **Fly.io Documentation**: https://fly.io/docs/
- **GitHub Issues**: Create issue in repository
- **Internal Documentation**: [Troubleshooting Guide](./Troubleshooting.md)

### Next Steps
1. Review [Getting Started Guide](./Getting_Started.md)
2. Follow [Manual Deployment](./Manual_Deployment.md) or set up [CI/CD Integration](./CI_CD_Integration.md)
3. Configure [Environment Management](./Environment_Management.md)
4. Set up [Monitoring and Logging](./Monitoring_and_Logging.md)

---

*This documentation is part of the HarmoniHSE360 deployment guide series. For other deployment options, see the main [Deployment README](../README.md).*
