# Docker Standalone Installation Guide

## Overview

This guide provides step-by-step instructions for installing HarmoniHSE360 on a standalone server using Docker and Docker Compose. This approach provides production-ready containerized deployment with comprehensive monitoring, security, and backup capabilities.

## 📋 Prerequisites Verification

Before starting the installation, verify that your system meets all requirements:

### System Requirements Checklist
- [ ] Ubuntu Server 22.04 LTS installed
- [ ] Minimum 8 CPU cores, 32GB RAM, 3TB SSD storage
- [ ] Static IP address configured
- [ ] Domain name registered and DNS configured
- [ ] Internet connectivity for package downloads
- [ ] SSH access with sudo privileges

### Network Requirements Checklist
- [ ] Ports 80, 443 accessible from internet
- [ ] Port 22 accessible for SSH (restricted IPs recommended)
- [ ] Firewall rules planned and documented
- [ ] SSL certificate strategy defined

## 🚀 Installation Process

### Phase 1: System Preparation (30-45 minutes)

#### 1.1 Update System Packages
```bash
# Update package lists and upgrade system
sudo apt update && sudo apt upgrade -y

# Install essential packages
sudo apt install -y curl wget git unzip htop iotop nethogs ufw fail2ban

# Configure timezone
sudo timedatectl set-timezone UTC

# Verify system information
hostnamectl
df -h
free -h
```

#### 1.2 Configure Firewall
```bash
# Configure UFW firewall
sudo ufw default deny incoming
sudo ufw default allow outgoing

# Allow SSH (adjust port if needed)
sudo ufw allow ssh

# Allow HTTP and HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Enable firewall
sudo ufw --force enable

# Verify firewall status
sudo ufw status verbose
```

#### 1.3 System Optimization
```bash
# Optimize kernel parameters for high-performance applications
sudo tee -a /etc/sysctl.conf << 'EOF'
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
sudo tee -a /etc/security/limits.conf << 'EOF'
* soft nofile 65536
* hard nofile 65536
* soft nproc 32768
* hard nproc 32768
EOF
```

### Phase 2: Docker Installation (15-30 minutes)

#### 2.1 Install Docker CE
```bash
# Remove old Docker versions
sudo apt remove -y docker docker-engine docker.io containerd runc

# Install Docker dependencies
sudo apt install -y apt-transport-https ca-certificates curl gnupg lsb-release

# Add Docker's official GPG key
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

# Add Docker repository
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Update package index
sudo apt update

# Install Docker CE
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Add user to docker group
sudo usermod -aG docker $USER

# Enable Docker service
sudo systemctl enable docker
sudo systemctl start docker

# Verify Docker installation
docker --version
docker compose version
```

#### 2.2 Configure Docker Daemon
```bash
# Create Docker daemon configuration
sudo mkdir -p /etc/docker

sudo tee /etc/docker/daemon.json << 'EOF'
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "100m",
    "max-file": "5"
  },
  "live-restore": true,
  "userland-proxy": false,
  "no-new-privileges": true,
  "storage-driver": "overlay2",
  "storage-opts": [
    "overlay2.override_kernel_check=true"
  ]
}
EOF

# Restart Docker service
sudo systemctl restart docker

# Verify Docker is running
sudo systemctl status docker
```

#### 2.3 Test Docker Installation
```bash
# Test Docker with hello-world
docker run hello-world

# Test Docker Compose
docker compose version

# Log out and back in for group changes to take effect
exit
# SSH back in
```

### Phase 3: Application Setup (30-45 minutes)

#### 3.1 Clone Repository
```bash
# Clone the HarmoniHSE360 repository
git clone https://github.com/risky-biz/harmoni-hse-360.git
cd harmoni-hse-360

# Switch to main branch
git checkout main

# Verify repository structure
ls -la
```

#### 3.2 Create Directory Structure
```bash
# Create data directories
sudo mkdir -p /opt/harmoni360/{data,logs,backups,ssl}
sudo mkdir -p /opt/harmoni360/data/{postgres,redis,uploads,seq,prometheus,grafana}
sudo mkdir -p /opt/harmoni360/logs/{app,nginx}
sudo mkdir -p /opt/harmoni360/backups/{postgres,uploads}

# Set ownership
sudo chown -R $USER:$USER /opt/harmoni360

# Set permissions
chmod -R 755 /opt/harmoni360
chmod -R 700 /opt/harmoni360/backups
chmod -R 700 /opt/harmoni360/ssl
```

#### 3.3 Configure Environment
```bash
# Copy environment template
cp .env.prod.example .env.prod

# Generate secure passwords
DB_PASSWORD=$(openssl rand -base64 32)
REDIS_PASSWORD=$(openssl rand -base64 32)
JWT_KEY=$(openssl rand -base64 32)
SEQ_PASSWORD=$(openssl rand -base64 16)
GRAFANA_PASSWORD=$(openssl rand -base64 16)

# Update environment file
sed -i "s/POSTGRES_PASSWORD=.*/POSTGRES_PASSWORD=$DB_PASSWORD/" .env.prod
sed -i "s/REDIS_PASSWORD=.*/REDIS_PASSWORD=$REDIS_PASSWORD/" .env.prod
sed -i "s/JWT_KEY=.*/JWT_KEY=$JWT_KEY/" .env.prod
sed -i "s/SEQ_ADMIN_PASSWORD=.*/SEQ_ADMIN_PASSWORD=$SEQ_PASSWORD/" .env.prod
sed -i "s/GRAFANA_ADMIN_PASSWORD=.*/GRAFANA_ADMIN_PASSWORD=$GRAFANA_PASSWORD/" .env.prod

# Update domain name (replace with your actual domain)
read -p "Enter your domain name (e.g., harmoni360.yourdomain.com): " DOMAIN_NAME
sed -i "s/DOMAIN_NAME=.*/DOMAIN_NAME=$DOMAIN_NAME/" .env.prod

# Display generated passwords for safekeeping
echo "Generated passwords (save these securely):"
echo "Database Password: $DB_PASSWORD"
echo "Redis Password: $REDIS_PASSWORD"
echo "JWT Key: $JWT_KEY"
echo "Seq Password: $SEQ_PASSWORD"
echo "Grafana Password: $GRAFANA_PASSWORD"
```

### Phase 4: SSL Certificate Setup (15-30 minutes)

#### 4.1 Option A: Let's Encrypt Certificate (Recommended for Production)
```bash
# Install Certbot
sudo apt install -y certbot

# Stop any services using ports 80/443
sudo systemctl stop nginx 2>/dev/null || true

# Obtain certificate (replace with your domain)
sudo certbot certonly --standalone -d $DOMAIN_NAME -d www.$DOMAIN_NAME --email admin@$DOMAIN_NAME --agree-tos --non-interactive

# Copy certificates to application directory
sudo cp /etc/letsencrypt/live/$DOMAIN_NAME/fullchain.pem /opt/harmoni360/ssl/cert.pem
sudo cp /etc/letsencrypt/live/$DOMAIN_NAME/privkey.pem /opt/harmoni360/ssl/key.pem

# Set proper permissions
sudo chown $USER:$USER /opt/harmoni360/ssl/*.pem
chmod 644 /opt/harmoni360/ssl/cert.pem
chmod 600 /opt/harmoni360/ssl/key.pem
```

#### 4.2 Option B: Self-Signed Certificate (For Testing)
```bash
# Generate self-signed certificate
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout /opt/harmoni360/ssl/key.pem \
  -out /opt/harmoni360/ssl/cert.pem \
  -subj "/C=US/ST=State/L=City/O=Organization/CN=$DOMAIN_NAME"

# Set proper permissions
sudo chown $USER:$USER /opt/harmoni360/ssl/*.pem
chmod 644 /opt/harmoni360/ssl/cert.pem
chmod 600 /opt/harmoni360/ssl/key.pem
```

### Phase 5: Application Deployment (20-30 minutes)

#### 5.1 Build and Start Services
```bash
# Load environment variables
source .env.prod

# Build application image
docker compose -f docker-compose.prod.yml build

# Start all services
docker compose -f docker-compose.prod.yml up -d

# Check service status
docker compose -f docker-compose.prod.yml ps
```

#### 5.2 Initialize Database
```bash
# Wait for database to be ready
echo "Waiting for database to start..."
sleep 60

# Check database connectivity
docker compose -f docker-compose.prod.yml exec postgres pg_isready -U harmoni360

# Run database migrations
docker compose -f docker-compose.prod.yml exec app dotnet ef database update

# Verify database tables
docker compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod -c "\dt"
```

#### 5.3 Verify Deployment
```bash
# Check all services are running
docker compose -f docker-compose.prod.yml ps

# Check application logs
docker compose -f docker-compose.prod.yml logs app --tail=50

# Test application health
curl -k https://$DOMAIN_NAME/health

# Test database connectivity
docker compose -f docker-compose.prod.yml exec app dotnet ef database update --dry-run
```

### Phase 6: Post-Deployment Configuration (15-30 minutes)

#### 6.1 Configure Monitoring Access
```bash
# Get service URLs
echo "Application URL: https://$DOMAIN_NAME"
echo "Grafana URL: https://$DOMAIN_NAME:3000"
echo "Prometheus URL: http://$(hostname -I | awk '{print $1}'):9090"
echo "Seq URL: http://$(hostname -I | awk '{print $1}'):5341"

# Display login credentials
echo ""
echo "Login Credentials:"
echo "Grafana - Username: admin, Password: $GRAFANA_PASSWORD"
echo "Seq - Password: $SEQ_PASSWORD"
```

#### 6.2 Set Up Automated Backups
```bash
# Create backup script
sudo tee /opt/harmoni360/scripts/backup.sh << 'EOF'
#!/bin/bash
# HarmoniHSE360 Backup Script

BACKUP_DIR="/opt/harmoni360/backups"
DATE=$(date +%Y%m%d_%H%M%S)
COMPOSE_FILE="/home/$USER/harmoni-hse-360/docker-compose.prod.yml"

# Database backup
docker compose -f "$COMPOSE_FILE" exec -T postgres pg_dump -U harmoni360 Harmoni360_Prod > "$BACKUP_DIR/postgres/db_backup_$DATE.sql"

# Uploads backup
tar -czf "$BACKUP_DIR/uploads/uploads_backup_$DATE.tar.gz" -C /opt/harmoni360/data uploads/

# Cleanup old backups (keep 30 days)
find "$BACKUP_DIR" -name "*.sql" -mtime +30 -delete
find "$BACKUP_DIR" -name "*.tar.gz" -mtime +30 -delete

echo "Backup completed: $DATE"
EOF

# Make script executable
sudo chmod +x /opt/harmoni360/scripts/backup.sh

# Create scripts directory
sudo mkdir -p /opt/harmoni360/scripts

# Add to crontab (daily at 2 AM)
(crontab -l 2>/dev/null; echo "0 2 * * * /opt/harmoni360/scripts/backup.sh") | crontab -
```

#### 6.3 Configure Log Rotation
```bash
# Configure log rotation for Docker containers
sudo tee /etc/logrotate.d/docker-harmoni360 << 'EOF'
/opt/harmoni360/logs/*.log {
    daily
    missingok
    rotate 52
    compress
    delaycompress
    notifempty
    create 644 root root
}
EOF
```

## ✅ Installation Verification

### 1. Service Health Checks
```bash
# Check all containers are running
docker compose -f docker-compose.prod.yml ps

# Check application health endpoint
curl -f https://$DOMAIN_NAME/health

# Check database connectivity
docker compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod -c "SELECT version();"

# Check Redis connectivity
docker compose -f docker-compose.prod.yml exec redis redis-cli ping
```

### 2. Application Functionality Tests
```bash
# Test API endpoints
curl -f https://$DOMAIN_NAME/api/health
curl -f https://$DOMAIN_NAME/swagger

# Test static file serving
curl -f https://$DOMAIN_NAME/favicon.ico

# Test WebSocket connectivity (SignalR)
# This requires a WebSocket client - can be tested through the web interface
```

### 3. Monitoring Verification
```bash
# Check Prometheus targets
curl -f http://localhost:9090/api/v1/targets

# Check Grafana health
curl -f http://localhost:3000/api/health

# Check Seq health
curl -f http://localhost:5341/api
```

## 🔧 Post-Installation Tasks

### 1. Security Hardening
- Review and implement [Security Hardening Guide](./Security_Hardening.md)
- Configure fail2ban for additional protection
- Set up intrusion detection system
- Review and restrict SSH access

### 2. Performance Optimization
- Follow [Performance Optimization Guide](./Performance_Optimization.md)
- Configure database performance tuning
- Optimize Nginx configuration
- Set up CDN for static assets (optional)

### 3. Monitoring Setup
- Import Grafana dashboards from `config/grafana/dashboards/`
- Configure alerting rules in Prometheus
- Set up notification channels (email, Slack, etc.)
- Configure log retention policies

### 4. Backup Verification
- Test backup script execution
- Verify backup file integrity
- Test restore procedures
- Configure off-site backup storage

## 🆘 Troubleshooting Installation Issues

### Common Issues and Solutions

#### Docker Installation Issues
```bash
# If Docker installation fails
sudo apt remove docker-ce docker-ce-cli containerd.io
sudo apt autoremove
# Re-run Docker installation steps
```

#### Database Connection Issues
```bash
# Check database logs
docker compose -f docker-compose.prod.yml logs postgres

# Reset database if needed
docker compose -f docker-compose.prod.yml down
docker volume rm harmoni-hse-360_postgres_data
docker compose -f docker-compose.prod.yml up -d postgres
```

#### SSL Certificate Issues
```bash
# Check certificate validity
openssl x509 -in /opt/harmoni360/ssl/cert.pem -text -noout

# Regenerate self-signed certificate if needed
sudo rm /opt/harmoni360/ssl/*.pem
# Re-run SSL certificate setup
```

#### Application Startup Issues
```bash
# Check application logs
docker compose -f docker-compose.prod.yml logs app

# Restart application container
docker compose -f docker-compose.prod.yml restart app
```

## 🎯 Next Steps

After successful installation:

1. **Security Configuration**: Implement [Security Hardening](./Security_Hardening.md)
2. **Performance Tuning**: Follow [Performance Optimization](./Performance_Optimization.md)
3. **Monitoring Setup**: Configure [Monitoring and Logging](./Monitoring_and_Logging.md)
4. **Backup Strategy**: Implement [Backup and Recovery](./Backup_and_Recovery.md)
5. **Maintenance Planning**: Review [Maintenance Procedures](./Maintenance_Procedures.md)

## 📞 Support

For installation issues:
1. Check the [Troubleshooting Guide](./Troubleshooting.md)
2. Review Docker and application logs
3. Verify system requirements and prerequisites
4. Consult the community documentation

---

*Previous: [Prerequisites](./Prerequisites.md) | Next: [Environment Configuration](./Environment_Configuration.md)*
