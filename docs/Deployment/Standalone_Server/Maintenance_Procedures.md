# Harmoni360 Standalone Server Maintenance Procedures

## Overview

This document outlines regular maintenance procedures to ensure optimal performance, security, and reliability of your Harmoni360 standalone server deployment.

## Maintenance Schedule

### Daily Tasks (Automated)
- Database backups
- Log rotation
- Health checks
- Security monitoring

### Weekly Tasks
- System updates
- Performance monitoring review
- Backup verification
- Database maintenance

### Monthly Tasks
- Application updates
- Security patches
- Capacity planning review
- Disaster recovery testing

### Quarterly Tasks
- Full system backup
- Security audit
- Performance optimization
- Documentation updates

## Daily Maintenance

### 1. Automated Health Checks

Create a health check script:

```bash
#!/bin/bash
# /opt/harmoni360/scripts/health-check.sh

LOG_FILE="/opt/harmoni360/logs/health-check.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Starting health check..." >> $LOG_FILE

# Check service status
SERVICES=("postgres" "redis" "app" "nginx" "seq" "prometheus" "grafana")
for service in "${SERVICES[@]}"; do
    if docker-compose -f /home/ubuntu/harmoni-hse-360/docker-compose.prod.yml ps $service | grep -q "Up"; then
        echo "[$DATE] ✓ $service is running" >> $LOG_FILE
    else
        echo "[$DATE] ✗ $service is not running" >> $LOG_FILE
        # Send alert (implement your notification method)
        echo "ALERT: $service is down on $(hostname)" | mail -s "Harmoni360 Service Alert" admin@your-domain.com
    fi
done

# Check application health endpoint
if curl -f -s https://your-domain.com/health > /dev/null; then
    echo "[$DATE] ✓ Application health check passed" >> $LOG_FILE
else
    echo "[$DATE] ✗ Application health check failed" >> $LOG_FILE
    echo "ALERT: Application health check failed on $(hostname)" | mail -s "Harmoni360 Health Alert" admin@your-domain.com
fi

# Check disk space
DISK_USAGE=$(df /opt/harmoni360 | awk 'NR==2 {print $5}' | sed 's/%//')
if [ $DISK_USAGE -gt 80 ]; then
    echo "[$DATE] ⚠ Disk usage is ${DISK_USAGE}%" >> $LOG_FILE
    echo "WARNING: Disk usage is ${DISK_USAGE}% on $(hostname)" | mail -s "Harmoni360 Disk Space Warning" admin@your-domain.com
else
    echo "[$DATE] ✓ Disk usage is ${DISK_USAGE}%" >> $LOG_FILE
fi

echo "[$DATE] Health check completed" >> $LOG_FILE
```

Add to crontab:
```bash
# Run every 15 minutes
*/15 * * * * /opt/harmoni360/scripts/health-check.sh
```

### 2. Automated Backups

Database backup script (already created in main guide):
```bash
# /opt/harmoni360/scripts/backup.sh
# Runs daily at 2 AM via cron
```

## Weekly Maintenance

### 1. System Updates

```bash
#!/bin/bash
# /opt/harmoni360/scripts/weekly-maintenance.sh

LOG_FILE="/opt/harmoni360/logs/maintenance.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Starting weekly maintenance..." >> $LOG_FILE

# Update system packages
sudo apt update && sudo apt upgrade -y >> $LOG_FILE 2>&1

# Update Docker images
cd /home/ubuntu/harmoni-hse-360
docker-compose -f docker-compose.prod.yml pull >> $LOG_FILE 2>&1

# Clean up Docker resources
docker system prune -f >> $LOG_FILE 2>&1
docker volume prune -f >> $LOG_FILE 2>&1

# Database maintenance
docker-compose -f docker-compose.prod.yml exec -T postgres psql -U harmoni360 -d Harmoni360_Prod -c "VACUUM ANALYZE;" >> $LOG_FILE 2>&1

# Check SSL certificate expiration
CERT_EXPIRY=$(openssl x509 -in /opt/harmoni360/ssl/cert.pem -noout -enddate | cut -d= -f2)
CERT_EXPIRY_EPOCH=$(date -d "$CERT_EXPIRY" +%s)
CURRENT_EPOCH=$(date +%s)
DAYS_UNTIL_EXPIRY=$(( ($CERT_EXPIRY_EPOCH - $CURRENT_EPOCH) / 86400 ))

if [ $DAYS_UNTIL_EXPIRY -lt 30 ]; then
    echo "[$DATE] ⚠ SSL certificate expires in $DAYS_UNTIL_EXPIRY days" >> $LOG_FILE
    echo "WARNING: SSL certificate expires in $DAYS_UNTIL_EXPIRY days on $(hostname)" | mail -s "Harmoni360 SSL Certificate Warning" admin@your-domain.com
else
    echo "[$DATE] ✓ SSL certificate valid for $DAYS_UNTIL_EXPIRY days" >> $LOG_FILE
fi

echo "[$DATE] Weekly maintenance completed" >> $LOG_FILE
```

Schedule weekly maintenance:
```bash
# Run every Sunday at 3 AM
0 3 * * 0 /opt/harmoni360/scripts/weekly-maintenance.sh
```

### 2. Performance Review

```bash
#!/bin/bash
# /opt/harmoni360/scripts/performance-review.sh

LOG_FILE="/opt/harmoni360/logs/performance-review.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Performance Review Report" >> $LOG_FILE
echo "=================================" >> $LOG_FILE

# System resources
echo "System Load:" >> $LOG_FILE
uptime >> $LOG_FILE

echo -e "\nMemory Usage:" >> $LOG_FILE
free -h >> $LOG_FILE

echo -e "\nDisk Usage:" >> $LOG_FILE
df -h /opt/harmoni360 >> $LOG_FILE

echo -e "\nDocker Container Stats:" >> $LOG_FILE
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}" >> $LOG_FILE

# Database performance
echo -e "\nDatabase Connection Count:" >> $LOG_FILE
docker-compose -f /home/ubuntu/harmoni-hse-360/docker-compose.prod.yml exec -T postgres psql -U harmoni360 -d Harmoni360_Prod -c "SELECT count(*) as active_connections FROM pg_stat_activity WHERE state = 'active';" >> $LOG_FILE

echo -e "\nTop 5 Slowest Queries:" >> $LOG_FILE
docker-compose -f /home/ubuntu/harmoni-hse-360/docker-compose.prod.yml exec -T postgres psql -U harmoni360 -d Harmoni360_Prod -c "SELECT query, mean_time, calls FROM pg_stat_statements ORDER BY mean_time DESC LIMIT 5;" >> $LOG_FILE

# Application logs summary
echo -e "\nApplication Error Summary (last 7 days):" >> $LOG_FILE
docker-compose -f /home/ubuntu/harmoni-hse-360/docker-compose.prod.yml logs --since 168h app 2>&1 | grep -i error | wc -l >> $LOG_FILE

echo "[$DATE] Performance review completed" >> $LOG_FILE
```

### 3. Backup Verification

```bash
#!/bin/bash
# /opt/harmoni360/scripts/verify-backups.sh

LOG_FILE="/opt/harmoni360/logs/backup-verification.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Starting backup verification..." >> $LOG_FILE

# Check if recent backups exist
LATEST_DB_BACKUP=$(ls -t /opt/harmoni360/backups/postgres/db_backup_*.sql 2>/dev/null | head -1)
LATEST_UPLOADS_BACKUP=$(ls -t /opt/harmoni360/backups/uploads/uploads_backup_*.tar.gz 2>/dev/null | head -1)

if [ -n "$LATEST_DB_BACKUP" ]; then
    BACKUP_AGE=$(( ($(date +%s) - $(stat -c %Y "$LATEST_DB_BACKUP")) / 86400 ))
    if [ $BACKUP_AGE -le 1 ]; then
        echo "[$DATE] ✓ Recent database backup found: $LATEST_DB_BACKUP" >> $LOG_FILE
        
        # Test backup integrity
        if docker-compose -f /home/ubuntu/harmoni-hse-360/docker-compose.prod.yml exec -T postgres pg_restore --list "$LATEST_DB_BACKUP" > /dev/null 2>&1; then
            echo "[$DATE] ✓ Database backup integrity check passed" >> $LOG_FILE
        else
            echo "[$DATE] ✗ Database backup integrity check failed" >> $LOG_FILE
        fi
    else
        echo "[$DATE] ⚠ Latest database backup is $BACKUP_AGE days old" >> $LOG_FILE
    fi
else
    echo "[$DATE] ✗ No database backup found" >> $LOG_FILE
fi

if [ -n "$LATEST_UPLOADS_BACKUP" ]; then
    BACKUP_AGE=$(( ($(date +%s) - $(stat -c %Y "$LATEST_UPLOADS_BACKUP")) / 86400 ))
    if [ $BACKUP_AGE -le 1 ]; then
        echo "[$DATE] ✓ Recent uploads backup found: $LATEST_UPLOADS_BACKUP" >> $LOG_FILE
        
        # Test backup integrity
        if tar -tzf "$LATEST_UPLOADS_BACKUP" > /dev/null 2>&1; then
            echo "[$DATE] ✓ Uploads backup integrity check passed" >> $LOG_FILE
        else
            echo "[$DATE] ✗ Uploads backup integrity check failed" >> $LOG_FILE
        fi
    else
        echo "[$DATE] ⚠ Latest uploads backup is $BACKUP_AGE days old" >> $LOG_FILE
    fi
else
    echo "[$DATE] ✗ No uploads backup found" >> $LOG_FILE
fi

echo "[$DATE] Backup verification completed" >> $LOG_FILE
```

## Monthly Maintenance

### 1. Application Updates

```bash
#!/bin/bash
# /opt/harmoni360/scripts/monthly-update.sh

LOG_FILE="/opt/harmoni360/logs/monthly-update.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Starting monthly application update..." >> $LOG_FILE

cd /home/ubuntu/harmoni-hse-360

# Create backup before update
echo "[$DATE] Creating pre-update backup..." >> $LOG_FILE
/opt/harmoni360/scripts/backup.sh

# Pull latest code
echo "[$DATE] Pulling latest code..." >> $LOG_FILE
git fetch origin >> $LOG_FILE 2>&1
git pull origin main >> $LOG_FILE 2>&1

# Build new images
echo "[$DATE] Building new application images..." >> $LOG_FILE
docker-compose -f docker-compose.prod.yml build >> $LOG_FILE 2>&1

# Update services with zero downtime
echo "[$DATE] Updating services..." >> $LOG_FILE
docker-compose -f docker-compose.prod.yml up -d >> $LOG_FILE 2>&1

# Run database migrations
echo "[$DATE] Running database migrations..." >> $LOG_FILE
docker-compose -f docker-compose.prod.yml exec -T app dotnet ef database update >> $LOG_FILE 2>&1

# Verify deployment
sleep 30
if curl -f -s https://your-domain.com/health > /dev/null; then
    echo "[$DATE] ✓ Application update successful" >> $LOG_FILE
else
    echo "[$DATE] ✗ Application update failed - rolling back" >> $LOG_FILE
    # Implement rollback procedure here
fi

echo "[$DATE] Monthly update completed" >> $LOG_FILE
```

### 2. Security Updates

```bash
#!/bin/bash
# /opt/harmoni360/scripts/security-update.sh

LOG_FILE="/opt/harmoni360/logs/security-update.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Starting security updates..." >> $LOG_FILE

# Update system security packages
sudo apt update >> $LOG_FILE 2>&1
sudo apt upgrade -y >> $LOG_FILE 2>&1

# Update Docker base images
cd /home/ubuntu/harmoni-hse-360
docker-compose -f docker-compose.prod.yml pull >> $LOG_FILE 2>&1

# Scan for vulnerabilities (if tools are installed)
if command -v trivy &> /dev/null; then
    echo "[$DATE] Running vulnerability scan..." >> $LOG_FILE
    trivy image harmoni360:prod-latest >> $LOG_FILE 2>&1
fi

# Update SSL certificates if needed
sudo certbot renew >> $LOG_FILE 2>&1

# Check for security updates in fail2ban rules
sudo fail2ban-client status >> $LOG_FILE 2>&1

echo "[$DATE] Security updates completed" >> $LOG_FILE
```

### 3. Capacity Planning

```bash
#!/bin/bash
# /opt/harmoni360/scripts/capacity-planning.sh

LOG_FILE="/opt/harmoni360/logs/capacity-planning.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Capacity Planning Report" >> $LOG_FILE
echo "===============================" >> $LOG_FILE

# Database size growth
echo "Database Size:" >> $LOG_FILE
docker-compose -f /home/ubuntu/harmoni-hse-360/docker-compose.prod.yml exec -T postgres psql -U harmoni360 -d Harmoni360_Prod -c "SELECT pg_size_pretty(pg_database_size('Harmoni360_Prod'));" >> $LOG_FILE

# Upload storage usage
echo -e "\nUpload Storage Usage:" >> $LOG_FILE
du -sh /opt/harmoni360/data/uploads/ >> $LOG_FILE

# Log storage usage
echo -e "\nLog Storage Usage:" >> $LOG_FILE
du -sh /opt/harmoni360/logs/ >> $LOG_FILE

# Backup storage usage
echo -e "\nBackup Storage Usage:" >> $LOG_FILE
du -sh /opt/harmoni360/backups/ >> $LOG_FILE

# System resource trends (requires historical data)
echo -e "\nSystem Resource Summary:" >> $LOG_FILE
echo "CPU Load Average (1min): $(uptime | awk -F'load average:' '{print $2}' | awk '{print $1}')" >> $LOG_FILE
echo "Memory Usage: $(free | awk 'NR==2{printf "%.2f%%", $3*100/$2}')" >> $LOG_FILE
echo "Disk Usage: $(df /opt/harmoni360 | awk 'NR==2{print $5}')" >> $LOG_FILE

echo "[$DATE] Capacity planning report completed" >> $LOG_FILE
```

## Quarterly Maintenance

### 1. Full System Backup

```bash
#!/bin/bash
# /opt/harmoni360/scripts/quarterly-backup.sh

BACKUP_DATE=$(date +%Y%m%d)
BACKUP_DIR="/opt/harmoni360/backups/quarterly"
LOG_FILE="/opt/harmoni360/logs/quarterly-backup.log"

mkdir -p $BACKUP_DIR

echo "[$DATE] Starting quarterly full backup..." >> $LOG_FILE

# Stop services for consistent backup
docker-compose -f /home/ubuntu/harmoni-hse-360/docker-compose.prod.yml stop

# Create full system backup
tar -czf "$BACKUP_DIR/harmoni360-full-backup-$BACKUP_DATE.tar.gz" \
    /opt/harmoni360/ \
    /home/ubuntu/harmoni-hse-360/ \
    --exclude=/opt/harmoni360/logs \
    --exclude=/opt/harmoni360/backups

# Restart services
docker-compose -f /home/ubuntu/harmoni-hse-360/docker-compose.prod.yml up -d

echo "[$DATE] Quarterly backup completed: $BACKUP_DIR/harmoni360-full-backup-$BACKUP_DATE.tar.gz" >> $LOG_FILE
```

### 2. Disaster Recovery Testing

```bash
#!/bin/bash
# /opt/harmoni360/scripts/dr-test.sh

LOG_FILE="/opt/harmoni360/logs/dr-test.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Starting disaster recovery test..." >> $LOG_FILE

# Test backup restoration (in test environment)
# This should be done on a separate test server
echo "[$DATE] Testing backup restoration procedures..." >> $LOG_FILE

# Test database recovery
echo "[$DATE] Testing database recovery..." >> $LOG_FILE

# Test application recovery
echo "[$DATE] Testing application recovery..." >> $LOG_FILE

# Document results
echo "[$DATE] Disaster recovery test completed" >> $LOG_FILE
```

## Monitoring and Alerting

### Set Up Email Alerts

```bash
# Install mail utilities
sudo apt install -y mailutils

# Configure postfix for sending emails
sudo dpkg-reconfigure postfix
```

### Create Alert Scripts

```bash
#!/bin/bash
# /opt/harmoni360/scripts/send-alert.sh

ALERT_TYPE=$1
MESSAGE=$2
EMAIL="admin@your-domain.com"

case $ALERT_TYPE in
    "critical")
        SUBJECT="CRITICAL: Harmoni360 Alert"
        ;;
    "warning")
        SUBJECT="WARNING: Harmoni360 Alert"
        ;;
    "info")
        SUBJECT="INFO: Harmoni360 Notification"
        ;;
    *)
        SUBJECT="Harmoni360 Alert"
        ;;
esac

echo "$MESSAGE" | mail -s "$SUBJECT" "$EMAIL"
```

## Log Management

### Log Rotation Configuration

```bash
# /etc/logrotate.d/harmoni360
/opt/harmoni360/logs/*/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 644 ubuntu ubuntu
    postrotate
        docker-compose -f /home/ubuntu/harmoni-hse-360/docker-compose.prod.yml restart nginx
    endscript
}
```

## Documentation Updates

### Quarterly Documentation Review

1. Update server specifications based on actual usage
2. Review and update troubleshooting procedures
3. Update backup and recovery procedures
4. Review security procedures and update as needed
5. Update monitoring thresholds based on performance data

## Maintenance Checklist

### Daily
- [ ] Automated health checks running
- [ ] Automated backups completed
- [ ] No critical alerts

### Weekly
- [ ] System updates applied
- [ ] Performance review completed
- [ ] Backup verification passed
- [ ] SSL certificate validity checked

### Monthly
- [ ] Application updates applied
- [ ] Security patches installed
- [ ] Capacity planning review completed
- [ ] Documentation updated

### Quarterly
- [ ] Full system backup completed
- [ ] Disaster recovery test performed
- [ ] Security audit completed
- [ ] Performance optimization review
