# HarmoniHSE360 Local Development Environment

## 🎯 Quick Start

This directory contains everything needed to run a complete local simulation of the HarmoniHSE360 Docker Standalone production deployment on Windows 11.

### Prerequisites
- Windows 11 Pro/Enterprise with Hyper-V
- 32GB+ RAM, 8+ CPU cores, 500GB+ free space
- Docker Desktop installed and configured
- Ngrok account and authtoken

### One-Command Setup
```powershell
# Run the automated setup script
.\scripts\setup-local-environment.ps1 -GenerateSecrets -StartServices -SetupNgrok -NgrokAuthToken "your_token_here"
```

### Manual Setup
```powershell
# 1. Copy environment configuration
Copy-Item ..\.env.local.example ..\.env.local

# 2. Edit configuration
notepad ..\.env.local

# 3. Start services
docker-compose -f ..\docker-compose.local.yml up -d

# 4. Start ngrok tunnels
.\scripts\start-ngrok.ps1 -Action start -All -Background -ShowUrls
```

## 📁 Directory Structure

```
local-dev/
├── README.md                   # This file
├── config/                     # Service configurations
│   ├── nginx/                  # Reverse proxy configuration
│   │   ├── nginx.local.conf    # Main nginx config
│   │   └── conf.d/             # Additional nginx configs
│   ├── prometheus/             # Monitoring configuration
│   │   ├── prometheus.local.yml # Prometheus config
│   │   └── rules/              # Alerting rules
│   ├── grafana/                # Dashboard configuration
│   │   ├── provisioning/       # Auto-provisioning configs
│   │   └── dashboards/         # Dashboard definitions
│   ├── redis/                  # Cache configuration
│   │   └── redis.conf          # Redis config file
│   └── postgres/               # Database configuration
│       └── init-scripts/       # Database initialization
├── scripts/                    # Automation scripts
│   ├── setup-local-environment.ps1  # Main setup script
│   ├── start-ngrok.ps1         # Ngrok tunnel management
│   ├── health-check.ps1        # Service health verification
│   ├── backup-database.ps1     # Database backup
│   └── generate-secrets.ps1    # Secret generation
├── ssl/                        # SSL certificates
│   ├── cert.pem               # SSL certificate
│   ├── key.pem                # SSL private key
│   └── openssl.conf           # OpenSSL configuration
├── logs/                       # Application logs
│   ├── nginx/                 # Nginx access/error logs
│   └── app/                   # Application logs
└── backups/                    # Local backups
    ├── postgres/              # Database backups
    └── uploads/               # File upload backups
```

## 🚀 Services Overview

### Core Application Stack
| Service | Port | Purpose | Health Check |
|---------|------|---------|--------------|
| **HarmoniHSE360 App** | 8080 | Main application | `/health` |
| **Nginx Proxy** | 80, 443 | Reverse proxy & SSL | `/health` |
| **PostgreSQL** | 5432 | Database | `pg_isready` |
| **Redis** | 6379 | Cache & sessions | `PING` |

### Monitoring Stack
| Service | Port | Purpose | Access |
|---------|------|---------|--------|
| **Grafana** | 3000 | Dashboards | `admin:password` |
| **Prometheus** | 9090 | Metrics collection | No auth |
| **Seq** | 5341 | Structured logging | `admin:password` |
| **Node Exporter** | 9100 | System metrics | No auth |

### Ngrok Tunnels
| Tunnel | Local Port | Purpose |
|--------|------------|---------|
| `harmoni360-app` | 443 | Main application access |
| `grafana` | 3000 | Monitoring dashboard |
| `prometheus` | 9090 | Metrics interface |
| `seq` | 5341 | Log analysis |

## 🔧 Configuration Files

### Environment Configuration
- **Source**: `../.env.local.example`
- **Target**: `../.env.local`
- **Purpose**: All environment variables and secrets

### Service Configurations
- **Nginx**: `config/nginx/nginx.local.conf`
- **Prometheus**: `config/prometheus/prometheus.local.yml`
- **Grafana**: `config/grafana/provisioning/`
- **Docker Compose**: `../docker-compose.local.yml`

## 📊 Monitoring and Observability

### Grafana Dashboards
Access: `http://localhost:3000` or via ngrok tunnel

**Pre-configured Dashboards:**
- **System Overview**: CPU, memory, disk, network
- **Application Performance**: Response times, error rates
- **Database Metrics**: Connections, queries, performance
- **Business Metrics**: User activity, feature usage

### Prometheus Metrics
Access: `http://localhost:9090` or via ngrok tunnel

**Metric Categories:**
- Application metrics (.NET runtime, HTTP requests)
- System metrics (CPU, memory, disk, network)
- Database metrics (connections, queries)
- Custom business metrics

### Seq Logging
Access: `http://localhost:5341` or via ngrok tunnel

**Log Categories:**
- Application logs (structured JSON)
- Error tracking and analysis
- Performance monitoring
- Audit trail logging

## 🔐 Security Configuration

### SSL Certificates
- **Self-signed certificates** for local HTTPS
- **Ngrok provides** trusted certificates for public access
- **Certificate location**: `ssl/cert.pem` and `ssl/key.pem`

### Authentication
- **Grafana**: `admin` / `[generated-password]`
- **Seq**: `admin` / `[generated-password]`
- **Application**: Demo users available in development mode

### Network Security
- **Internal networks** for service communication
- **Firewall rules** via Docker networks
- **Rate limiting** configured in Nginx

## 🧪 Development Workflow

### Daily Development
```powershell
# Start environment
docker-compose -f ..\docker-compose.local.yml up -d

# Start ngrok tunnels
.\scripts\start-ngrok.ps1 -Action start -All -Background

# Check service status
.\scripts\health-check.ps1

# View logs
docker-compose -f ..\docker-compose.local.yml logs -f app

# Stop environment
docker-compose -f ..\docker-compose.local.yml down
```

### Code Changes
```powershell
# Rebuild application after code changes
docker-compose -f ..\docker-compose.local.yml build app
docker-compose -f ..\docker-compose.local.yml up -d app

# Or restart specific service
docker-compose -f ..\docker-compose.local.yml restart app
```

### Database Management
```powershell
# Run database migrations
docker-compose -f ..\docker-compose.local.yml exec app dotnet ef database update

# Backup database
.\scripts\backup-database.ps1

# Connect to database
docker-compose -f ..\docker-compose.local.yml exec postgres psql -U harmoni360 -d Harmoni360_Local
```

## 🔧 Maintenance Tasks

### Regular Maintenance
```powershell
# Update Docker images
docker-compose -f ..\docker-compose.local.yml pull
docker-compose -f ..\docker-compose.local.yml build --no-cache

# Clean up Docker resources
docker system prune -f
docker volume prune -f

# Rotate logs
docker-compose -f ..\docker-compose.local.yml restart nginx
```

### Backup Procedures
```powershell
# Backup database
.\scripts\backup-database.ps1

# Backup configuration
.\scripts\backup-config.ps1

# Backup uploads
tar -czf backups\uploads\uploads_$(Get-Date -Format 'yyyyMMdd_HHmmss').tar.gz uploads\
```

## 🆘 Troubleshooting

### Common Issues

#### Services Won't Start
```powershell
# Check Docker Desktop status
docker version

# Check service logs
docker-compose -f ..\docker-compose.local.yml logs [service-name]

# Restart Docker Desktop
Restart-Service -Name "com.docker.service"
```

#### Database Connection Issues
```powershell
# Check database status
docker-compose -f ..\docker-compose.local.yml exec postgres pg_isready -U harmoni360

# Reset database
docker-compose -f ..\docker-compose.local.yml down -v
docker-compose -f ..\docker-compose.local.yml up -d postgres
```

#### Ngrok Tunnel Issues
```powershell
# Check ngrok status
.\scripts\start-ngrok.ps1 -Action status

# Restart tunnels
.\scripts\start-ngrok.ps1 -Action restart -All

# Check ngrok web interface
Start-Process "http://localhost:4040"
```

### Performance Issues
```powershell
# Check resource usage
docker stats

# Check system resources
Get-Process | Sort-Object CPU -Descending | Select-Object -First 10

# Adjust Docker resource limits
# Docker Desktop → Settings → Resources → Advanced
```

## 📋 Useful Commands

### Docker Management
```powershell
# View all services
docker-compose -f ..\docker-compose.local.yml ps

# View service logs
docker-compose -f ..\docker-compose.local.yml logs -f [service]

# Execute commands in containers
docker-compose -f ..\docker-compose.local.yml exec app bash
docker-compose -f ..\docker-compose.local.yml exec postgres psql -U harmoni360

# Scale services
docker-compose -f ..\docker-compose.local.yml up -d --scale app=2
```

### Monitoring Commands
```powershell
# Check application health
Invoke-WebRequest -Uri "https://localhost/health"

# Get tunnel URLs
.\scripts\start-ngrok.ps1 -Action urls

# Monitor resource usage
docker stats --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}"
```

## 🎯 Production Migration

### Key Differences from Production
- **Hardware**: Local simulation vs. dedicated server
- **SSL**: Self-signed vs. commercial certificates
- **Scaling**: Single instance vs. load balanced
- **Backup**: Local vs. enterprise backup solutions
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

## 📞 Support

### Documentation
- [Main Local Development Guide](../docs/Deployment/Local_Development/README.md)
- [Windows Setup Guide](../docs/Deployment/Local_Development/Windows_Setup.md)
- [Ngrok Configuration](../docs/Deployment/Local_Development/Ngrok_Configuration.md)
- [Environment Configuration](../docs/Deployment/Local_Development/Environment_Configuration.md)

### Quick Help
```powershell
# Get help for setup script
Get-Help .\scripts\setup-local-environment.ps1 -Full

# Get help for ngrok script
Get-Help .\scripts\start-ngrok.ps1 -Full

# Check service health
.\scripts\health-check.ps1 -Verbose
```

---

*This local development environment provides a complete simulation of the production Docker Standalone deployment, enabling hands-on experience and validation before the production investment.*
