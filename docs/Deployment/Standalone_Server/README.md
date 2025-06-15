# Harmoni360 Standalone Server Deployment Guide

## Overview

This guide provides comprehensive instructions for deploying Harmoni360 on a standalone physical server using Docker Compose. This deployment option is ideal for organizations that prefer on-premises hosting or need complete control over their infrastructure.

## Documentation Index

### Core Documentation
- **[README.md](./README.md)** - Main deployment guide (this file)
- **[Server_Hardware_Specifications.md](./Server_Hardware_Specifications.md)** - Hardware requirements and cost analysis
- **[Deployment_Summary.md](./Deployment_Summary.md)** - Complete implementation overview

### Operational Guides
- **[Troubleshooting_Guide.md](./Troubleshooting_Guide.md)** - Common issues and solutions
- **[Maintenance_Procedures.md](./Maintenance_Procedures.md)** - Automated and manual maintenance
- **[Migration_Strategy.md](./Migration_Strategy.md)** - Migration between deployment types

### Quick Reference
- **[docker-compose.prod.yml](../../../docker-compose.prod.yml)** - Production Docker Compose configuration
- **[.env.prod.example](../../../.env.prod.example)** - Environment configuration template
- **[deploy-standalone.sh](../../../scripts/deploy-standalone.sh)** - Automated deployment script

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Server Setup](#server-setup)
3. [Installation](#installation)
4. [Configuration](#configuration)
5. [Deployment](#deployment)
6. [SSL Certificate Setup](#ssl-certificate-setup)
7. [Monitoring](#monitoring)
8. [Backup and Recovery](#backup-and-recovery)
9. [Maintenance](#maintenance)
10. [Troubleshooting](#troubleshooting)

## Prerequisites

### Hardware Requirements
- **Minimum**: 8 cores, 32GB RAM, 3TB storage
- **Recommended**: 16 cores, 64GB RAM, 6TB storage
- See [Server Hardware Specifications](./Server_Hardware_Specifications.md) for detailed requirements

### Software Requirements
- Ubuntu Server 22.04 LTS (recommended) or CentOS Stream 9
- Docker CE 24.0+
- Docker Compose v2.20+
- Git
- OpenSSL
- Curl/wget

### Network Requirements
- Static IP address
- Domain name (for SSL certificate)
- Firewall access to ports 80, 443, and 22 (SSH)
- Minimum 100 Mbps internet connection

## Server Setup

### 1. Operating System Installation

Install Ubuntu Server 22.04 LTS with the following configuration:
- Use entire disk with LVM
- Install OpenSSH server
- Create a non-root user with sudo privileges

### 2. Initial System Configuration

```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Install required packages
sudo apt install -y curl wget git unzip htop iotop nethogs ufw fail2ban

# Configure timezone
sudo timedatectl set-timezone UTC

# Configure firewall
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw --force enable
```

### 3. Docker Installation

```bash
# Install Docker CE
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add user to docker group
sudo usermod -aG docker $USER

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Verify installation
docker --version
docker-compose --version

# Log out and back in for group changes to take effect
```

### 4. System Optimization

```bash
# Optimize kernel parameters for high-performance applications
sudo tee -a /etc/sysctl.conf << EOF
# Network optimizations
net.core.rmem_max = 134217728
net.core.wmem_max = 134217728
net.ipv4.tcp_rmem = 4096 87380 134217728
net.ipv4.tcp_wmem = 4096 65536 134217728
net.core.netdev_max_backlog = 5000
net.ipv4.tcp_congestion_control = bbr

# File system optimizations
fs.file-max = 2097152
vm.swappiness = 10
vm.dirty_ratio = 15
vm.dirty_background_ratio = 5

# Docker optimizations
net.bridge.bridge-nf-call-iptables = 1
net.bridge.bridge-nf-call-ip6tables = 1
net.ipv4.ip_forward = 1
EOF

# Apply changes
sudo sysctl -p

# Increase file descriptor limits
sudo tee -a /etc/security/limits.conf << EOF
* soft nofile 65536
* hard nofile 65536
* soft nproc 32768
* hard nproc 32768
EOF
```

## Quick Start

### Automated Deployment

For a quick automated deployment, use the provided deployment script:

```bash
# Clone the repository
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360

# Make the deployment script executable
chmod +x scripts/deploy-standalone.sh

# Run the automated deployment
./scripts/deploy-standalone.sh
```

The script will:
- Check all prerequisites
- Create necessary directories
- Configure environment
- Generate SSL certificates
- Deploy all services
- Run database migrations
- Set up monitoring and backups

## Manual Installation

### 1. Clone Repository

```bash
# Clone the Harmoni360 repository
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360

# Switch to the appropriate branch (if needed)
git checkout main
```

### 2. Create Directory Structure

```bash
# Create data directories
sudo mkdir -p /opt/harmoni360/{data,logs,backups}
sudo mkdir -p /opt/harmoni360/data/{postgres,redis,uploads,seq,prometheus,grafana}
sudo mkdir -p /opt/harmoni360/logs/{app,nginx}
sudo mkdir -p /opt/harmoni360/backups/{postgres,uploads}

# Set ownership
sudo chown -R $USER:$USER /opt/harmoni360

# Set permissions
chmod -R 755 /opt/harmoni360
chmod -R 700 /opt/harmoni360/backups
```

### 3. SSL Certificate Setup

```bash
# Create SSL directory
sudo mkdir -p /opt/harmoni360/ssl

# Option 1: Self-signed certificate (for testing)
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout /opt/harmoni360/ssl/key.pem \
  -out /opt/harmoni360/ssl/cert.pem \
  -subj "/C=US/ST=State/L=City/O=Organization/CN=your-domain.com"

# Option 2: Let's Encrypt certificate (recommended for production)
# Install certbot
sudo apt install -y certbot

# Obtain certificate (replace your-domain.com with your actual domain)
sudo certbot certonly --standalone -d your-domain.com -d www.your-domain.com

# Copy certificates to SSL directory
sudo cp /etc/letsencrypt/live/your-domain.com/fullchain.pem /opt/harmoni360/ssl/cert.pem
sudo cp /etc/letsencrypt/live/your-domain.com/privkey.pem /opt/harmoni360/ssl/key.pem

# Set permissions
sudo chown -R $USER:$USER /opt/harmoni360/ssl
chmod 600 /opt/harmoni360/ssl/key.pem
chmod 644 /opt/harmoni360/ssl/cert.pem
```

### DNS Configuration

See [DNS_Configuration.md](./DNS_Configuration.md) for details on setting up DNS
records. You will need to create an **A record** pointing your domain to the
server's public IP and optionally a **CNAME** for `www`. DNS changes can take
several hours to propagate, so verify with `dig` or `nslookup` before
continuing. Additional guidance is available in the
[Migration Strategy](./Migration_Strategy.md#step-4-dns-and-ssl-configuration).

## Configuration

### 1. Environment Configuration

```bash
# Copy environment template
cp .env.prod.example .env.prod

# Edit configuration file
nano .env.prod
```

**Required Configuration Values:**

```bash
# Database
POSTGRES_PASSWORD=your_secure_postgres_password

# Redis
REDIS_PASSWORD=your_secure_redis_password

# JWT
JWT_KEY=your_32_character_jwt_secret_key

# Seq Logging
SEQ_ADMIN_PASSWORD_HASH=your_seq_password_hash

# Grafana
GRAFANA_ADMIN_PASSWORD=your_grafana_password

# Domain
DOMAIN_NAME=your-domain.com
SSL_EMAIL=admin@your-domain.com
```

### 2. Generate Secure Passwords

```bash
# Generate PostgreSQL password
openssl rand -base64 32

# Generate Redis password
openssl rand -base64 32

# Generate JWT key
openssl rand -base64 32

# Generate Seq password hash
echo 'your-seq-password' | docker run --rm -i datalust/seq config hash
```

### 3. Update SSL Configuration

```bash
# Update nginx configuration with your domain
sed -i 's/your-domain.com/yourdomain.com/g' config/nginx/nginx.conf

# Update SSL certificate paths if needed
nano config/nginx/nginx.conf
```

## Deployment

### 1. Build and Start Services

```bash
# Load environment variables
source .env.prod

# Build application image
docker-compose -f docker-compose.prod.yml build

# Start all services
docker-compose -f docker-compose.prod.yml up -d

# Check service status
docker-compose -f docker-compose.prod.yml ps
```

### 2. Initialize Database

```bash
# Wait for database to be ready
sleep 60

# Run database migrations
docker-compose -f docker-compose.prod.yml exec app dotnet ef database update

# Verify database connection
docker-compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod -c "\dt"
```

### 3. Verify Deployment

```bash
# Check application health
curl -k https://your-domain.com/health

# Check all services are running
docker-compose -f docker-compose.prod.yml logs --tail=50

# Test database connectivity
docker-compose -f docker-compose.prod.yml exec app dotnet ef database update --dry-run
```

## Post-Deployment Configuration

### 1. Configure Monitoring

Access Grafana at `https://your-domain.com:3000`:
- Username: admin
- Password: (from GRAFANA_ADMIN_PASSWORD)

Import the provided dashboards from `config/grafana/dashboards/`.

### 2. Configure Logging

Access Seq at `http://your-server-ip:5341`:
- Use the admin password hash configured in SEQ_ADMIN_PASSWORD_HASH

### 3. Set Up Automated Backups

```bash
# Create backup script
sudo tee /opt/harmoni360/scripts/backup.sh << 'EOF'
#!/bin/bash
# Harmoni360 Backup Script

BACKUP_DIR="/opt/harmoni360/backups"
DATE=$(date +%Y%m%d_%H%M%S)

# Database backup
docker-compose -f /home/ubuntu/harmoni-hse-360/docker-compose.prod.yml exec -T postgres pg_dump -U harmoni360 Harmoni360_Prod > "$BACKUP_DIR/postgres/db_backup_$DATE.sql"

# Uploads backup
tar -czf "$BACKUP_DIR/uploads/uploads_backup_$DATE.tar.gz" -C /opt/harmoni360/data uploads/

# Cleanup old backups (keep 30 days)
find "$BACKUP_DIR" -name "*.sql" -mtime +30 -delete
find "$BACKUP_DIR" -name "*.tar.gz" -mtime +30 -delete

echo "Backup completed: $DATE"
EOF

# Make script executable
sudo chmod +x /opt/harmoni360/scripts/backup.sh

# Add to crontab (daily at 2 AM)
(crontab -l 2>/dev/null; echo "0 2 * * * /opt/harmoni360/scripts/backup.sh") | crontab -
```

## Monitoring and Maintenance

### 1. Health Monitoring

```bash
# Check service health
docker-compose -f docker-compose.prod.yml ps

# View logs
docker-compose -f docker-compose.prod.yml logs -f app

# Monitor resource usage
docker stats

# Check disk usage
df -h
du -sh /opt/harmoni360/*
```

### 2. Performance Monitoring

Access monitoring dashboards:
- **Grafana**: `https://your-domain.com:3000`
- **Prometheus**: `http://your-server-ip:9090`
- **Seq Logs**: `http://your-server-ip:5341`

### 3. Regular Maintenance Tasks

```bash
# Update application (monthly)
git pull origin main
docker-compose -f docker-compose.prod.yml build
docker-compose -f docker-compose.prod.yml up -d

# Clean up Docker resources (weekly)
docker system prune -f
docker volume prune -f

# Update SSL certificates (Let's Encrypt auto-renewal)
sudo certbot renew --quiet

# Database maintenance (weekly)
docker-compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod -c "VACUUM ANALYZE;"
```

## Security Hardening

### 1. System Security

```bash
# Configure fail2ban
sudo tee /etc/fail2ban/jail.local << EOF
[DEFAULT]
bantime = 3600
findtime = 600
maxretry = 3

[sshd]
enabled = true
port = ssh
filter = sshd
logpath = /var/log/auth.log
maxretry = 3
EOF

sudo systemctl enable fail2ban
sudo systemctl start fail2ban

# Disable root login
sudo sed -i 's/PermitRootLogin yes/PermitRootLogin no/' /etc/ssh/sshd_config
sudo systemctl restart ssh
```

### 2. Docker Security

```bash
# Enable Docker content trust
export DOCKER_CONTENT_TRUST=1

# Run Docker daemon with security options
sudo tee /etc/docker/daemon.json << EOF
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "100m",
    "max-file": "5"
  },
  "live-restore": true,
  "userland-proxy": false,
  "no-new-privileges": true
}
EOF

sudo systemctl restart docker
```

## Troubleshooting

See [Troubleshooting Guide](./Troubleshooting_Guide.md) for common issues and solutions.

## Next Steps

After successful deployment:
1. Configure monitoring alerts in Grafana
2. Set up automated backups to cloud storage
3. Implement log rotation and archival
4. Plan for scaling and disaster recovery

## Support

For additional support:
1. Check the [troubleshooting guide](./Troubleshooting_Guide.md)
2. Review application logs in Seq
3. Monitor system metrics in Grafana
4. Consult the [maintenance procedures](./Maintenance_Procedures.md)
