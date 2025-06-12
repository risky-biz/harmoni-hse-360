# HarmoniHSE360 Docker Standalone Deployment Guide

## Overview

This comprehensive guide covers deploying HarmoniHSE360 on a standalone server using Docker and Docker Compose. This deployment option provides complete control over your infrastructure while leveraging containerization for consistency, scalability, and ease of management.

## 📚 Documentation Index

### Getting Started
- **[README.md](./README.md)** - Main deployment guide (this file)
- **[Prerequisites.md](./Prerequisites.md)** - System requirements and dependencies
- **[Installation.md](./Installation.md)** - Step-by-step installation process

### Configuration
- **[Environment_Configuration.md](./Environment_Configuration.md)** - Environment variables and secrets
- **[SSL_Certificate_Setup.md](./SSL_Certificate_Setup.md)** - HTTPS and certificate management
- **[Database_Configuration.md](./Database_Configuration.md)** - PostgreSQL setup and optimization

### Operations
- **[Monitoring_and_Logging.md](./Monitoring_and_Logging.md)** - Observability stack setup
- **[Backup_and_Recovery.md](./Backup_and_Recovery.md)** - Data protection strategies
- **[Security_Hardening.md](./Security_Hardening.md)** - Security best practices
- **[Performance_Optimization.md](./Performance_Optimization.md)** - Performance tuning
- **[Maintenance_Procedures.md](./Maintenance_Procedures.md)** - Ongoing maintenance
- **[Troubleshooting.md](./Troubleshooting.md)** - Common issues and solutions

## 🏗️ Architecture Overview

### Technology Stack
- **Application**: .NET 8 ASP.NET Core + React 18 TypeScript
- **Database**: PostgreSQL 15 with TimescaleDB extension
- **Cache**: Redis 7.2 with persistence
- **Reverse Proxy**: Nginx with SSL termination
- **Monitoring**: Prometheus + Grafana + Seq
- **Container Runtime**: Docker + Docker Compose
- **Operating System**: Ubuntu 22.04 LTS (recommended)

### System Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Internet      │    │   Firewall      │    │   Load Balancer │
│   (Users)       │───►│   (UFW/iptables)│───►│   (Nginx)       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                       │
                                                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Monitoring    │    │  HarmoniHSE360  │    │   Database      │
│ (Grafana/Seq)   │◄──►│   Application   │◄──►│  (PostgreSQL)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │                       │
                                ▼                       ▼
                       ┌─────────────────┐    ┌─────────────────┐
                       │   Redis Cache   │    │   File Storage  │
                       │                 │    │   (Volumes)     │
                       └─────────────────┘    └─────────────────┘
```

### Container Architecture

```
Docker Host (Ubuntu 22.04 LTS)
├── nginx-proxy (Port 80, 443)
│   ├── SSL Termination
│   ├── Load Balancing
│   └── Static File Serving
├── harmoni360-app (Port 8080)
│   ├── .NET 8 Backend API
│   ├── React Frontend (SPA)
│   └── SignalR Real-time
├── postgres (Port 5432)
│   ├── PostgreSQL 15
│   ├── TimescaleDB Extension
│   └── Automated Backups
├── redis (Port 6379)
│   ├── Session Storage
│   ├── Caching Layer
│   └── Pub/Sub Messaging
├── prometheus (Port 9090)
│   ├── Metrics Collection
│   └── Alerting Rules
├── grafana (Port 3000)
│   ├── Dashboards
│   └── Visualization
└── seq (Port 5341)
    ├── Structured Logging
    └── Log Analysis
```

## 💰 Cost Analysis

### Hardware Requirements

| Component | Minimum | Recommended | Enterprise |
|-----------|---------|-------------|------------|
| **CPU** | 8 cores | 16 cores | 32 cores |
| **RAM** | 32GB | 64GB | 128GB |
| **Storage** | 3TB SSD | 6TB SSD | 12TB SSD |
| **Network** | 100 Mbps | 1 Gbps | 10 Gbps |

### Estimated Costs (USD)

| Deployment Size | Hardware Cost | Monthly Operating | Annual Total |
|-----------------|---------------|-------------------|--------------|
| **Small (50 users)** | $8,000 | $500 | $14,000 |
| **Medium (200 users)** | $15,000 | $800 | $24,600 |
| **Large (500+ users)** | $30,000 | $1,200 | $44,400 |

*Costs include hardware, power, cooling, internet, and maintenance*

## 🎯 Deployment Scenarios

### Scenario 1: Development/Testing Environment
**Use Case**: Development team, testing, staging
- **Resources**: 8 cores, 32GB RAM, 1TB SSD
- **Features**: Demo mode, sample data, development tools
- **Users**: 10-20 concurrent users
- **Cost**: ~$5,000 initial + $300/month

### Scenario 2: Small Production Environment
**Use Case**: Small organization, pilot implementation
- **Resources**: 16 cores, 64GB RAM, 3TB SSD
- **Features**: Full production, monitoring, backups
- **Users**: 50-100 concurrent users
- **Cost**: ~$15,000 initial + $800/month

### Scenario 3: Enterprise Production Environment
**Use Case**: Large organization, mission-critical deployment
- **Resources**: 32+ cores, 128GB+ RAM, 6TB+ SSD
- **Features**: High availability, advanced monitoring, disaster recovery
- **Users**: 200+ concurrent users
- **Cost**: ~$30,000+ initial + $1,200+/month

## 🚀 Quick Start

### Option 1: Automated Deployment Script

```bash
# Clone repository
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360

# Run automated deployment
chmod +x scripts/deploy-standalone.sh
sudo ./scripts/deploy-standalone.sh

# Follow prompts for configuration
```

### Option 2: Docker Compose Quick Start

```bash
# Clone and configure
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360

# Copy and configure environment
cp .env.prod.example .env.prod
nano .env.prod  # Edit configuration

# Deploy services
docker-compose -f docker-compose.prod.yml up -d

# Initialize database
docker-compose -f docker-compose.prod.yml exec app dotnet ef database update
```

### Option 3: Step-by-Step Manual Process

Follow the detailed [Installation Guide](./Installation.md) for complete control.

## 📋 Prerequisites Checklist

### Hardware Requirements
- [ ] Server meets minimum specifications (8 cores, 32GB RAM, 3TB SSD)
- [ ] Reliable internet connection (100+ Mbps)
- [ ] Uninterruptible Power Supply (UPS) for production
- [ ] Adequate cooling and ventilation

### Software Requirements
- [ ] Ubuntu Server 22.04 LTS installed
- [ ] Static IP address configured
- [ ] Domain name registered (for SSL)
- [ ] SSH access configured
- [ ] Firewall rules planned

### Network Requirements
- [ ] Ports 80, 443 accessible from internet
- [ ] Port 22 accessible for SSH (restricted IPs recommended)
- [ ] Internal network connectivity for services
- [ ] DNS records configured

### Security Requirements
- [ ] SSL certificate plan (Let's Encrypt or commercial)
- [ ] Backup storage solution identified
- [ ] Monitoring and alerting strategy defined
- [ ] Incident response procedures documented

## 🔧 Core Components

### 1. Application Container (harmoni360-app)
- **Base Image**: mcr.microsoft.com/dotnet/aspnet:8.0-alpine
- **Purpose**: Hosts .NET 8 backend API and React frontend
- **Resources**: 2-4 CPU cores, 4-8GB RAM
- **Ports**: 8080 (internal)
- **Volumes**: `/app/uploads` for file storage

### 2. Database Container (postgres)
- **Base Image**: postgres:15-alpine
- **Purpose**: Primary data storage with TimescaleDB
- **Resources**: 2-4 CPU cores, 8-16GB RAM
- **Ports**: 5432 (internal)
- **Volumes**: `/var/lib/postgresql/data`, `/backups`

### 3. Cache Container (redis)
- **Base Image**: redis:7-alpine
- **Purpose**: Session storage and application caching
- **Resources**: 1-2 CPU cores, 2-4GB RAM
- **Ports**: 6379 (internal)
- **Volumes**: `/data` for persistence

### 4. Reverse Proxy (nginx)
- **Base Image**: nginx:alpine
- **Purpose**: SSL termination, load balancing, static files
- **Resources**: 1-2 CPU cores, 1-2GB RAM
- **Ports**: 80, 443 (external)
- **Volumes**: SSL certificates, static content

### 5. Monitoring Stack
- **Prometheus**: Metrics collection and alerting
- **Grafana**: Dashboards and visualization
- **Seq**: Structured logging and analysis

## 📊 Performance Characteristics

### Expected Performance

| Metric | Small Deployment | Medium Deployment | Large Deployment |
|--------|------------------|-------------------|------------------|
| **Concurrent Users** | 50 | 200 | 500+ |
| **Response Time** | <500ms | <300ms | <200ms |
| **Throughput** | 100 req/s | 500 req/s | 1000+ req/s |
| **Database Connections** | 50 | 200 | 500 |
| **Memory Usage** | 16GB | 32GB | 64GB+ |
| **Storage Growth** | 10GB/month | 50GB/month | 200GB/month |

### Scaling Considerations
- **Horizontal Scaling**: Multiple application instances behind load balancer
- **Database Scaling**: Read replicas, connection pooling, query optimization
- **Cache Scaling**: Redis clustering, cache partitioning
- **Storage Scaling**: Network-attached storage, distributed file systems

## 🔐 Security Features

### Built-in Security
- **Container Isolation**: Each service runs in isolated containers
- **Network Segmentation**: Internal Docker networks for service communication
- **SSL/TLS Encryption**: HTTPS for all external communication
- **Secret Management**: Environment variables and Docker secrets
- **Access Control**: Role-based authentication and authorization

### Security Hardening
- **Firewall Configuration**: UFW with restrictive rules
- **Fail2Ban**: Intrusion prevention system
- **Regular Updates**: Automated security updates
- **Audit Logging**: Comprehensive audit trails
- **Backup Encryption**: Encrypted backup storage

## 📈 Monitoring and Observability

### Metrics Collection
- **Application Metrics**: Performance, errors, business metrics
- **Infrastructure Metrics**: CPU, memory, disk, network
- **Database Metrics**: Connections, queries, performance
- **Container Metrics**: Resource usage, health status

### Logging Strategy
- **Structured Logging**: JSON format with correlation IDs
- **Centralized Collection**: Seq for log aggregation
- **Log Retention**: Configurable retention policies
- **Alert Integration**: Automated alerting on errors

### Dashboards
- **System Overview**: High-level health and performance
- **Application Performance**: Response times, error rates
- **Infrastructure Health**: Resource utilization
- **Business Metrics**: User activity, feature usage

## 🎯 Next Steps

Choose your deployment path:

1. **Quick Start**: Use automated deployment script for rapid setup
2. **Detailed Setup**: Follow step-by-step [Installation Guide](./Installation.md)
3. **Custom Configuration**: Review [Environment Configuration](./Environment_Configuration.md)
4. **Security Setup**: Implement [Security Hardening](./Security_Hardening.md)
5. **Monitoring**: Configure [Monitoring and Logging](./Monitoring_and_Logging.md)

## 📞 Support and Resources

### Documentation
- [Installation Guide](./Installation.md) - Detailed setup instructions
- [Configuration Guide](./Environment_Configuration.md) - Environment setup
- [Security Guide](./Security_Hardening.md) - Security best practices
- [Troubleshooting Guide](./Troubleshooting.md) - Common issues

### External Resources
- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Nginx Documentation](https://nginx.org/en/docs/)

### Community Support
- GitHub Issues for bug reports and feature requests
- Docker Community for containerization questions
- PostgreSQL Community for database optimization
- Ubuntu Community for system administration

---

*This documentation is part of the HarmoniHSE360 deployment guide series. For other deployment options, see the main [Deployment README](../../README.md).*
