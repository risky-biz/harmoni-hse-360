# HarmoniHSE360 Local Development Environment

## Overview

This comprehensive guide creates a complete local simulation of the Docker Standalone production deployment on Windows 11, providing hands-on experience with the full HarmoniHSE360 stack before implementing the production deployment.

## 📚 Documentation Index

### Setup Guides
- **[README.md](./README.md)** - Main local development guide (this file)
- **[Windows_Setup.md](./Windows_Setup.md)** - Windows 11 and Docker Desktop configuration
- **[Ngrok_Configuration.md](./Ngrok_Configuration.md)** - Public access and SSL simulation
- **[Environment_Configuration.md](./Environment_Configuration.md)** - Local environment adaptation
- **[Testing_Procedures.md](./Testing_Procedures.md)** - Verification and testing guide
- **[Troubleshooting.md](./Troubleshooting.md)** - Common issues and solutions

### Configuration Files
- **[docker-compose.local.yml](../../../docker-compose.local.yml)** - Local Docker Compose configuration
- **[.env.local.example](../../../.env.local.example)** - Environment variables template
- **[local-dev/config/](../../../local-dev/config/)** - Service configuration files

## 🎯 Objectives

### Primary Goals
- **Production Parity**: Mirror the production Docker Standalone deployment exactly
- **Public Access**: Simulate real-world domain and SSL access using ngrok
- **Learning Environment**: Hands-on experience with deployment and operations
- **Risk Mitigation**: Validate deployment procedures before production investment

### Expected Outcomes
- Complete HarmoniHSE360 stack running locally on Windows 11
- Public HTTPS access via ngrok tunneling
- Full monitoring and logging stack operational
- Understanding of production deployment procedures
- Confidence in the production investment decision

## 💻 System Requirements

### Windows 11 Requirements
- **OS**: Windows 11 Pro/Enterprise (Hyper-V support required)
- **RAM**: 32GB minimum (64GB recommended for full stack)
- **CPU**: 8 cores minimum (16 cores recommended)
- **Storage**: 500GB free space (SSD recommended)
- **Network**: Stable broadband connection for ngrok tunneling

### Resource Allocation Planning
```
Total System Resources: 32GB RAM, 8 CPU cores
├── Windows 11 Host: 8GB RAM, 2 CPU cores
├── Docker Desktop: 20GB RAM, 6 CPU cores
│   ├── HarmoniHSE360 App: 4GB RAM, 2 CPU cores
│   ├── PostgreSQL: 4GB RAM, 1 CPU core
│   ├── Redis: 2GB RAM, 1 CPU core
│   ├── Nginx: 1GB RAM, 0.5 CPU cores
│   ├── Prometheus: 3GB RAM, 1 CPU core
│   ├── Grafana: 2GB RAM, 0.5 CPU cores
│   ├── Seq: 2GB RAM, 0.5 CPU cores
│   └── System Overhead: 2GB RAM, 0.5 CPU cores
└── Development Tools: 4GB RAM
```

## 🚀 Quick Start

### Prerequisites Check
```powershell
# Verify Windows version
winver

# Check available RAM
Get-CimInstance -ClassName Win32_PhysicalMemory | Measure-Object -Property Capacity -Sum

# Check CPU cores
Get-CimInstance -ClassName Win32_Processor | Select-Object NumberOfCores, NumberOfLogicalProcessors
```

### 1. Setup Windows Environment
Follow the [Windows Setup Guide](./Windows_Setup.md) to:
- Enable Hyper-V and WSL2
- Install Docker Desktop
- Configure resource allocation
- Install development tools

### 2. Configure Ngrok
Follow the [Ngrok Configuration Guide](./Ngrok_Configuration.md) to:
- Install and configure ngrok
- Set up tunneling for public access
- Configure custom domains (optional)

### 3. Deploy Local Environment
```powershell
# Clone repository
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360

# Copy environment configuration
copy .env.local.example .env.local

# Edit environment variables
notepad .env.local

# Start the complete stack
docker-compose -f docker-compose.local.yml up -d

# Verify deployment
docker-compose -f docker-compose.local.yml ps
```

### 4. Start Ngrok Tunneling
```powershell
# Start ngrok for main application
ngrok start harmoni360-app

# Start additional tunnels (paid plan)
ngrok start grafana prometheus seq
```

## 📊 Service Architecture

### Local Services Overview
| Service | Local Port | Ngrok Tunnel | Purpose |
|---------|------------|--------------|---------|
| **HarmoniHSE360 App** | 8080 | harmoni360-app | Main application |
| **Nginx Proxy** | 80, 443 | harmoni360-app | Reverse proxy & SSL |
| **PostgreSQL** | 5432 | - | Database |
| **Redis** | 6379 | - | Cache |
| **Grafana** | 3000 | grafana | Monitoring dashboards |
| **Prometheus** | 9090 | prometheus | Metrics collection |
| **Seq** | 5341 | seq | Structured logging |

### Network Architecture
```
Internet (via ngrok)
         │
         ▼
┌─────────────────┐
│   Ngrok Tunnel  │
│   (HTTPS)       │
└─────────────────┘
         │
         ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Nginx Proxy   │    │  HarmoniHSE360  │    │   PostgreSQL    │
│   (Port 443)    │───►│   Application   │◄──►│   (Port 5432)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │   Redis Cache   │
                       │   (Port 6379)   │
                       └─────────────────┘
```

## 🔧 Configuration Management

### Environment Variables
The local environment uses `.env.local` for configuration:
- Database credentials and connection strings
- Redis configuration
- JWT secrets
- Monitoring service passwords
- Ngrok configuration

### Service Configuration
Configuration files are organized in `local-dev/config/`:
- `nginx/` - Reverse proxy configuration
- `prometheus/` - Metrics collection setup
- `grafana/` - Dashboard provisioning
- `seq/` - Logging configuration

## 📈 Monitoring Stack

### Grafana Dashboards
Access at: `http://localhost:3000` or via ngrok tunnel
- **System Overview**: Resource utilization and health
- **Application Performance**: Response times and errors
- **Database Metrics**: Connection pools and query performance
- **Business Metrics**: User activity and feature usage

### Prometheus Metrics
Access at: `http://localhost:9090` or via ngrok tunnel
- Application metrics from .NET
- System metrics from node-exporter
- Container metrics from Docker
- Custom business metrics

### Seq Logging
Access at: `http://localhost:5341` or via ngrok tunnel
- Structured application logs
- Error tracking and analysis
- Performance monitoring
- Audit trail logging

## 🔐 Security Considerations

### Local Development Security
- **Self-signed certificates** for local HTTPS
- **Development secrets** (not production values)
- **Firewall considerations** for ngrok tunneling
- **Access control** for monitoring services

### Ngrok Security
- **Authentication tokens** for tunnel access
- **IP whitelisting** (paid plans)
- **Password protection** for sensitive services
- **Tunnel monitoring** and access logs

## 🧪 Testing Procedures

### Automated Testing
```powershell
# Run health checks
.\local-dev\scripts\health-check.ps1

# Test all services
.\local-dev\scripts\test-services.ps1

# Performance testing
.\local-dev\scripts\load-test.ps1
```

### Manual Testing
1. **Application Access**: Test via ngrok HTTPS tunnel
2. **Database Connectivity**: Verify PostgreSQL connection
3. **Cache Performance**: Test Redis functionality
4. **Monitoring**: Check all dashboards and metrics
5. **Logging**: Verify log aggregation in Seq

## 🔄 Development Workflow

### Daily Development
1. **Start Environment**: `docker-compose -f docker-compose.local.yml up -d`
2. **Start Ngrok**: `ngrok start harmoni360-app`
3. **Development**: Code changes with hot reload
4. **Testing**: Use ngrok URL for external testing
5. **Monitoring**: Check Grafana for performance metrics
6. **Cleanup**: `docker-compose -f docker-compose.local.yml down`

### Environment Updates
```powershell
# Update application image
docker-compose -f docker-compose.local.yml build app
docker-compose -f docker-compose.local.yml up -d app

# Update configuration
docker-compose -f docker-compose.local.yml restart nginx

# Reset database (development only)
docker-compose -f docker-compose.local.yml down -v
docker-compose -f docker-compose.local.yml up -d
```

## 📋 Maintenance Tasks

### Regular Maintenance
- **Weekly**: Update Docker images
- **Monthly**: Clean up Docker volumes and images
- **Quarterly**: Review and update configurations

### Backup Procedures
```powershell
# Backup database
.\local-dev\scripts\backup-database.ps1

# Backup configuration
.\local-dev\scripts\backup-config.ps1

# Restore from backup
.\local-dev\scripts\restore-backup.ps1
```

## 🎯 Production Migration

### Key Differences
- **Hardware**: Local simulation vs. dedicated server
- **SSL Certificates**: Self-signed vs. Let's Encrypt/Commercial
- **Resource Allocation**: Development vs. production sizing
- **Backup Strategy**: Local vs. enterprise backup solutions
- **Monitoring**: Development vs. production alerting

### Migration Checklist
- [ ] Hardware procurement and setup
- [ ] Production SSL certificate acquisition
- [ ] Environment variable adaptation
- [ ] Backup strategy implementation
- [ ] Monitoring and alerting configuration
- [ ] Security hardening procedures
- [ ] Performance optimization
- [ ] Disaster recovery planning

## 📞 Support Resources

### Documentation
- [Windows Setup Guide](./Windows_Setup.md)
- [Ngrok Configuration](./Ngrok_Configuration.md)
- [Environment Configuration](./Environment_Configuration.md)
- [Testing Procedures](./Testing_Procedures.md)
- [Troubleshooting Guide](./Troubleshooting.md)

### External Resources
- [Docker Desktop Documentation](https://docs.docker.com/desktop/windows/)
- [Ngrok Documentation](https://ngrok.com/docs)
- [Windows 11 Hyper-V Guide](https://docs.microsoft.com/en-us/virtualization/hyper-v-on-windows/)

### Community Support
- Docker Community Forums
- Ngrok Community Support
- HarmoniHSE360 GitHub Issues

---

*This local development environment provides a complete simulation of the production Docker Standalone deployment, enabling hands-on experience and validation before the production investment.*
