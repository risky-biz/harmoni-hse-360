# System Setup for Manual Deployment

## Overview

This guide covers the complete system setup and configuration required for manually deploying HarmoniHSE360 on Ubuntu Server 22.04 LTS without Docker containerization. This includes operating system configuration, security hardening, and system optimization.

## 🎯 System Preparation Objectives

### Primary Goals
- Configure a secure, optimized Ubuntu Server environment
- Implement security best practices from the start
- Optimize system performance for web applications
- Establish monitoring and logging foundations
- Prepare for application-specific requirements

### Expected Outcomes
- Hardened Ubuntu Server 22.04 LTS installation
- Optimized kernel parameters for web applications
- Configured firewall with appropriate rules
- Established user accounts and permissions
- Monitoring and logging infrastructure ready

## 📋 Prerequisites Verification

### Hardware Requirements Check
```bash
# Check CPU information
lscpu | grep -E "(CPU|Thread|Core)"

# Check memory
free -h

# Check storage
df -h
lsblk

# Check network interfaces
ip addr show
```

### Minimum Requirements Verification
- **CPU**: 8 cores minimum (16 recommended)
- **RAM**: 32GB minimum (64GB recommended)
- **Storage**: 3TB SSD minimum (6TB recommended)
- **Network**: 100 Mbps minimum (1 Gbps recommended)

## 🚀 Phase 1: Initial System Configuration (30-45 minutes)

### 1.1 System Update and Package Management
```bash
# Update package lists
sudo apt update

# Upgrade all packages
sudo apt upgrade -y

# Install essential packages
sudo apt install -y \
    curl \
    wget \
    git \
    unzip \
    htop \
    iotop \
    nethogs \
    tree \
    vim \
    nano \
    software-properties-common \
    apt-transport-https \
    ca-certificates \
    gnupg \
    lsb-release \
    build-essential \
    ufw \
    fail2ban \
    logrotate \
    rsync \
    cron \
    ntp

# Clean up package cache
sudo apt autoremove -y
sudo apt autoclean
```

### 1.2 System Information and Verification
```bash
# Display system information
echo "=== System Information ==="
hostnamectl
echo ""

echo "=== CPU Information ==="
lscpu | head -20
echo ""

echo "=== Memory Information ==="
free -h
echo ""

echo "=== Storage Information ==="
df -h
echo ""

echo "=== Network Information ==="
ip addr show | grep -E "(inet|ether)"
```

### 1.3 Hostname and Network Configuration
```bash
# Set hostname (replace with your desired hostname)
read -p "Enter hostname for this server: " NEW_HOSTNAME
sudo hostnamectl set-hostname "$NEW_HOSTNAME"

# Update /etc/hosts
sudo tee -a /etc/hosts << EOF
127.0.0.1 $NEW_HOSTNAME
EOF

# Configure timezone
sudo timedatectl set-timezone UTC

# Verify configuration
hostnamectl
timedatectl
```

## 🔐 Phase 2: Security Configuration (45-60 minutes)

### 2.1 User Account Management
```bash
# Create application user
sudo useradd -m -s /bin/bash harmoni360
sudo usermod -aG sudo harmoni360

# Set password for application user
sudo passwd harmoni360

# Create application directories
sudo mkdir -p /opt/harmoni360/{app,data,logs,backups,ssl}
sudo chown -R harmoni360:harmoni360 /opt/harmoni360
sudo chmod -R 755 /opt/harmoni360
sudo chmod -R 700 /opt/harmoni360/ssl
sudo chmod -R 700 /opt/harmoni360/backups
```

### 2.2 SSH Security Configuration
```bash
# Backup original SSH configuration
sudo cp /etc/ssh/sshd_config /etc/ssh/sshd_config.backup

# Configure SSH security settings
sudo tee /etc/ssh/sshd_config.d/99-harmoni360-security.conf << 'EOF'
# HarmoniHSE360 SSH Security Configuration

# Disable root login
PermitRootLogin no

# Use SSH protocol version 2 only
Protocol 2

# Change default port (optional - update firewall rules accordingly)
# Port 2222

# Authentication settings
PubkeyAuthentication yes
PasswordAuthentication yes
PermitEmptyPasswords no
ChallengeResponseAuthentication no
UsePAM yes

# Connection settings
ClientAliveInterval 300
ClientAliveCountMax 2
MaxAuthTries 3
MaxSessions 10

# Restrict users (uncomment and modify as needed)
# AllowUsers harmoni360 your-admin-user

# Disable X11 forwarding
X11Forwarding no

# Disable agent forwarding
AllowAgentForwarding no

# Disable TCP forwarding
AllowTcpForwarding no

# Use strong ciphers
Ciphers chacha20-poly1305@openssh.com,aes256-gcm@openssh.com,aes128-gcm@openssh.com,aes256-ctr,aes192-ctr,aes128-ctr
MACs hmac-sha2-256-etm@openssh.com,hmac-sha2-512-etm@openssh.com,hmac-sha2-256,hmac-sha2-512
KexAlgorithms curve25519-sha256@libssh.org,diffie-hellman-group16-sha512,diffie-hellman-group18-sha512
EOF

# Test SSH configuration
sudo sshd -t

# Restart SSH service
sudo systemctl restart ssh
```

### 2.3 Firewall Configuration
```bash
# Reset UFW to defaults
sudo ufw --force reset

# Set default policies
sudo ufw default deny incoming
sudo ufw default allow outgoing

# Allow SSH (adjust port if changed above)
sudo ufw allow ssh
# If you changed SSH port: sudo ufw allow 2222/tcp

# Allow HTTP and HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Allow specific application ports (internal access only)
sudo ufw allow from 127.0.0.1 to any port 5000  # Application
sudo ufw allow from 127.0.0.1 to any port 5432  # PostgreSQL
sudo ufw allow from 127.0.0.1 to any port 6379  # Redis

# Optional: Allow monitoring ports from specific networks
# sudo ufw allow from 192.168.1.0/24 to any port 9090  # Prometheus
# sudo ufw allow from 192.168.1.0/24 to any port 3000  # Grafana
# sudo ufw allow from 192.168.1.0/24 to any port 5341  # Seq

# Enable firewall
sudo ufw --force enable

# Display firewall status
sudo ufw status verbose
```

### 2.4 Fail2Ban Configuration
```bash
# Create Fail2Ban configuration for SSH
sudo tee /etc/fail2ban/jail.d/harmoni360-ssh.conf << 'EOF'
[sshd]
enabled = true
port = ssh
filter = sshd
logpath = /var/log/auth.log
maxretry = 3
bantime = 3600
findtime = 600
EOF

# Create Fail2Ban configuration for Nginx (for later)
sudo tee /etc/fail2ban/jail.d/harmoni360-nginx.conf << 'EOF'
[nginx-http-auth]
enabled = true
filter = nginx-http-auth
logpath = /var/log/nginx/error.log
maxretry = 3
bantime = 3600

[nginx-limit-req]
enabled = true
filter = nginx-limit-req
logpath = /var/log/nginx/error.log
maxretry = 10
bantime = 600
EOF

# Enable and start Fail2Ban
sudo systemctl enable fail2ban
sudo systemctl start fail2ban

# Check Fail2Ban status
sudo fail2ban-client status
```

## ⚡ Phase 3: System Optimization (30-45 minutes)

### 3.1 Kernel Parameter Optimization
```bash
# Backup original sysctl configuration
sudo cp /etc/sysctl.conf /etc/sysctl.conf.backup

# Create optimized sysctl configuration
sudo tee /etc/sysctl.d/99-harmoni360-optimization.conf << 'EOF'
# HarmoniHSE360 System Optimization

# Network optimizations
net.core.rmem_max = 134217728
net.core.wmem_max = 134217728
net.ipv4.tcp_rmem = 4096 87380 134217728
net.ipv4.tcp_wmem = 4096 65536 134217728
net.core.netdev_max_backlog = 5000
net.ipv4.tcp_congestion_control = bbr
net.ipv4.tcp_slow_start_after_idle = 0
net.ipv4.tcp_tw_reuse = 1
net.core.somaxconn = 65535
net.ipv4.ip_local_port_range = 1024 65535

# File system optimizations
fs.file-max = 2097152
fs.nr_open = 1048576
vm.swappiness = 10
vm.dirty_ratio = 15
vm.dirty_background_ratio = 5
vm.vfs_cache_pressure = 50

# Security optimizations
net.ipv4.conf.default.rp_filter = 1
net.ipv4.conf.all.rp_filter = 1
net.ipv4.conf.all.accept_redirects = 0
net.ipv6.conf.all.accept_redirects = 0
net.ipv4.conf.all.send_redirects = 0
net.ipv4.conf.all.accept_source_route = 0
net.ipv6.conf.all.accept_source_route = 0
net.ipv4.conf.all.log_martians = 1
net.ipv4.icmp_echo_ignore_broadcasts = 1
net.ipv4.icmp_ignore_bogus_error_responses = 1
net.ipv4.tcp_syncookies = 1

# Memory optimizations
kernel.shmmax = 68719476736
kernel.shmall = 4294967296
EOF

# Apply sysctl changes
sudo sysctl -p /etc/sysctl.d/99-harmoni360-optimization.conf

# Verify changes
sudo sysctl net.core.rmem_max
sudo sysctl vm.swappiness
```

### 3.2 File Descriptor Limits
```bash
# Backup original limits configuration
sudo cp /etc/security/limits.conf /etc/security/limits.conf.backup

# Configure file descriptor limits
sudo tee -a /etc/security/limits.conf << 'EOF'

# HarmoniHSE360 File Descriptor Limits
* soft nofile 65536
* hard nofile 65536
* soft nproc 32768
* hard nproc 32768
harmoni360 soft nofile 65536
harmoni360 hard nofile 65536
harmoni360 soft nproc 32768
harmoni360 hard nproc 32768
EOF

# Configure systemd limits
sudo mkdir -p /etc/systemd/system.conf.d
sudo tee /etc/systemd/system.conf.d/limits.conf << 'EOF'
[Manager]
DefaultLimitNOFILE=65536
DefaultLimitNPROC=32768
EOF

# Configure PAM limits
echo "session required pam_limits.so" | sudo tee -a /etc/pam.d/common-session
```

### 3.3 Storage Optimization
```bash
# Check current mount options
mount | grep "ext4\|xfs"

# Create optimized fstab backup
sudo cp /etc/fstab /etc/fstab.backup

# Add noatime option to reduce disk I/O (if not already present)
# Note: This should be done carefully and may require a reboot
echo "# Consider adding 'noatime' option to your root filesystem for better performance"
echo "# Example: UUID=xxx / ext4 defaults,noatime 0 1"

# Configure log rotation for application logs
sudo tee /etc/logrotate.d/harmoni360 << 'EOF'
/opt/harmoni360/logs/*.log {
    daily
    missingok
    rotate 52
    compress
    delaycompress
    notifempty
    create 644 harmoni360 harmoni360
    postrotate
        systemctl reload harmoni360 2>/dev/null || true
    endscript
}
EOF
```

## 📊 Phase 4: Monitoring and Logging Setup (20-30 minutes)

### 4.1 System Monitoring Configuration
```bash
# Install monitoring tools
sudo apt install -y sysstat iftop iotop htop

# Enable sysstat data collection
sudo systemctl enable sysstat
sudo systemctl start sysstat

# Configure sysstat collection interval
sudo sed -i 's/ENABLED="false"/ENABLED="true"/' /etc/default/sysstat
sudo sed -i 's/5-55\/10/\*\/2/' /etc/cron.d/sysstat
```

### 4.2 Log Management Configuration
```bash
# Configure rsyslog for application logging
sudo tee /etc/rsyslog.d/99-harmoni360.conf << 'EOF'
# HarmoniHSE360 Application Logging

# Create separate log files for application
:programname, isequal, "harmoni360" /opt/harmoni360/logs/application.log
:programname, isequal, "harmoni360" stop

# Log all authentication events
auth,authpriv.* /opt/harmoni360/logs/auth.log

# Log all mail events
mail.* /opt/harmoni360/logs/mail.log
EOF

# Create log directories
sudo mkdir -p /opt/harmoni360/logs
sudo chown harmoni360:harmoni360 /opt/harmoni360/logs
sudo chmod 755 /opt/harmoni360/logs

# Restart rsyslog
sudo systemctl restart rsyslog
```

### 4.3 Cron and Scheduled Tasks
```bash
# Create maintenance script directory
sudo mkdir -p /opt/harmoni360/scripts
sudo chown harmoni360:harmoni360 /opt/harmoni360/scripts

# Create system maintenance script
sudo tee /opt/harmoni360/scripts/system-maintenance.sh << 'EOF'
#!/bin/bash
# HarmoniHSE360 System Maintenance Script

LOG_FILE="/opt/harmoni360/logs/maintenance.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Starting system maintenance" >> "$LOG_FILE"

# Update package lists
apt update >> "$LOG_FILE" 2>&1

# Clean package cache
apt autoremove -y >> "$LOG_FILE" 2>&1
apt autoclean >> "$LOG_FILE" 2>&1

# Rotate logs manually if needed
logrotate -f /etc/logrotate.d/harmoni360 >> "$LOG_FILE" 2>&1

# Check disk usage
df -h >> "$LOG_FILE" 2>&1

# Check memory usage
free -h >> "$LOG_FILE" 2>&1

echo "[$DATE] System maintenance completed" >> "$LOG_FILE"
EOF

# Make script executable
sudo chmod +x /opt/harmoni360/scripts/system-maintenance.sh

# Add to crontab for harmoni360 user
sudo -u harmoni360 crontab -l 2>/dev/null | { cat; echo "0 2 * * 0 /opt/harmoni360/scripts/system-maintenance.sh"; } | sudo -u harmoni360 crontab -
```

## ✅ Phase 5: Verification and Testing (15-20 minutes)

### 5.1 System Configuration Verification
```bash
# Check system limits
ulimit -n
ulimit -u

# Check sysctl parameters
sysctl net.core.rmem_max
sysctl vm.swappiness
sysctl fs.file-max

# Check firewall status
sudo ufw status verbose

# Check fail2ban status
sudo fail2ban-client status

# Check SSH configuration
sudo sshd -t
```

### 5.2 Security Verification
```bash
# Check for listening services
sudo netstat -tulpn | grep LISTEN

# Check user accounts
cat /etc/passwd | grep -E "(harmoni360|root)"

# Check sudo access
sudo -u harmoni360 sudo -l

# Check file permissions
ls -la /opt/harmoni360/
```

### 5.3 Performance Baseline
```bash
# CPU information
lscpu | grep -E "(CPU|Thread|Core|MHz)"

# Memory information
free -h

# Disk performance test (basic)
sudo hdparm -Tt /dev/sda

# Network interface information
ip link show
```

## 📋 System Setup Checklist

### Security Configuration
- [ ] SSH hardened with key-based authentication
- [ ] Firewall configured with minimal required ports
- [ ] Fail2Ban configured for intrusion prevention
- [ ] User accounts created with appropriate permissions
- [ ] System updates applied

### Performance Optimization
- [ ] Kernel parameters optimized for web applications
- [ ] File descriptor limits increased
- [ ] Network parameters tuned
- [ ] Storage optimized with appropriate mount options
- [ ] Log rotation configured

### Monitoring and Logging
- [ ] System monitoring tools installed
- [ ] Log management configured
- [ ] Maintenance scripts created and scheduled
- [ ] Baseline performance metrics recorded

### Application Readiness
- [ ] Application user and directories created
- [ ] Required ports opened in firewall
- [ ] SSL certificate directory prepared
- [ ] Backup directories created with proper permissions

## 🔧 Post-Setup Tasks

### Immediate Tasks
1. **Test SSH Access**: Verify SSH access with new configuration
2. **Verify Firewall**: Test that only required ports are accessible
3. **Check Logs**: Verify that logging is working correctly
4. **Performance Test**: Run basic performance tests

### Before Application Installation
1. **Create Backup**: Take a system snapshot or backup
2. **Document Configuration**: Record all custom configurations
3. **Test Connectivity**: Verify internet connectivity and DNS resolution
4. **Prepare SSL**: Obtain SSL certificates for your domain

## 🆘 Troubleshooting Common Issues

### SSH Access Issues
```bash
# Check SSH service status
sudo systemctl status ssh

# Check SSH configuration
sudo sshd -t

# View SSH logs
sudo journalctl -u ssh -f
```

### Firewall Issues
```bash
# Check firewall status
sudo ufw status verbose

# Temporarily disable firewall (for testing only)
sudo ufw disable

# Re-enable firewall
sudo ufw enable
```

### Performance Issues
```bash
# Check system load
uptime
top

# Check memory usage
free -h

# Check disk I/O
iotop

# Check network usage
nethogs
```

## 🎯 Next Steps

After completing system setup:

1. **Dependencies Installation**: Follow [Dependencies Installation Guide](./Dependencies_Installation.md)
2. **Database Setup**: Configure PostgreSQL using [Database Setup Guide](./Database_Setup.md)
3. **Application Installation**: Deploy HarmoniHSE360 using [Application Installation Guide](./Application_Installation.md)
4. **Service Configuration**: Set up system services using [Service Configuration Guide](./Service_Configuration.md)

## 📞 Support Resources

### System Administration
- [Ubuntu Server Guide](https://ubuntu.com/server/docs)
- [UFW Documentation](https://help.ubuntu.com/community/UFW)
- [Fail2Ban Documentation](https://www.fail2ban.org/wiki/index.php/Main_Page)

### Performance Tuning
- [Linux Performance Tuning](https://www.kernel.org/doc/Documentation/sysctl/)
- [Network Tuning Guide](https://fasterdata.es.net/host-tuning/linux/)

### Security Hardening
- [CIS Ubuntu Benchmarks](https://www.cisecurity.org/benchmark/ubuntu_linux)
- [Ubuntu Security Guide](https://ubuntu.com/security)

---

*Previous: [README](./README.md) | Next: [Dependencies Installation](./Dependencies_Installation.md)*
