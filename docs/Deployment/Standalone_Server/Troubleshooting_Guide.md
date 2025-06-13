# Harmoni360 Standalone Server Troubleshooting Guide

## Common Issues and Solutions

### 1. Container Startup Issues

#### Problem: Services fail to start
```bash
# Check service status
docker-compose -f docker-compose.prod.yml ps

# Check logs for specific service
docker-compose -f docker-compose.prod.yml logs postgres
docker-compose -f docker-compose.prod.yml logs app
docker-compose -f docker-compose.prod.yml logs nginx
```

**Common Causes:**
- Insufficient disk space
- Port conflicts
- Missing environment variables
- Permission issues

**Solutions:**
```bash
# Check disk space
df -h

# Check port usage
sudo netstat -tulpn | grep :80
sudo netstat -tulpn | grep :443

# Verify environment file
cat .env.prod | grep -v "^#" | grep -v "^$"

# Fix permissions
sudo chown -R $USER:$USER /opt/harmoni360
chmod -R 755 /opt/harmoni360/data
```

#### Problem: Database connection failures
```bash
# Check PostgreSQL logs
docker-compose -f docker-compose.prod.yml logs postgres

# Test database connectivity
docker-compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod -c "SELECT 1;"
```

**Solutions:**
```bash
# Restart PostgreSQL service
docker-compose -f docker-compose.prod.yml restart postgres

# Check database configuration
docker-compose -f docker-compose.prod.yml exec postgres cat /etc/postgresql/postgresql.conf

# Verify connection string in application
docker-compose -f docker-compose.prod.yml exec app printenv | grep ConnectionStrings
```

### 2. Performance Issues

#### Problem: High CPU usage
```bash
# Monitor container resource usage
docker stats

# Check system load
htop
top
```

**Solutions:**
```bash
# Scale application containers (if needed)
docker-compose -f docker-compose.prod.yml up -d --scale app=2

# Optimize PostgreSQL configuration
# Edit config/postgres/postgresql.conf
# Adjust shared_buffers, work_mem, effective_cache_size

# Check for slow queries
docker-compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod -c "SELECT query, mean_time, calls FROM pg_stat_statements ORDER BY mean_time DESC LIMIT 10;"
```

#### Problem: High memory usage
```bash
# Check memory usage by container
docker stats --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}"

# Check system memory
free -h
```

**Solutions:**
```bash
# Adjust container memory limits in docker-compose.prod.yml
# Restart services with new limits
docker-compose -f docker-compose.prod.yml up -d

# Clear Redis cache if needed
docker-compose -f docker-compose.prod.yml exec redis redis-cli -a $REDIS_PASSWORD FLUSHDB
```

### 3. SSL/TLS Issues

#### Problem: SSL certificate errors
```bash
# Check certificate validity
openssl x509 -in /opt/harmoni360/ssl/cert.pem -text -noout

# Test SSL connection
openssl s_client -connect your-domain.com:443 -servername your-domain.com
```

**Solutions:**
```bash
# Renew Let's Encrypt certificate
sudo certbot renew

# Copy renewed certificates
sudo cp /etc/letsencrypt/live/your-domain.com/fullchain.pem /opt/harmoni360/ssl/cert.pem
sudo cp /etc/letsencrypt/live/your-domain.com/privkey.pem /opt/harmoni360/ssl/key.pem

# Restart nginx
docker-compose -f docker-compose.prod.yml restart nginx
```

#### Problem: Mixed content warnings
**Solution:** Ensure all resources are served over HTTPS and update any hardcoded HTTP URLs in the application.

### 4. Database Issues

#### Problem: Database migration failures
```bash
# Check migration status
docker-compose -f docker-compose.prod.yml exec app dotnet ef migrations list

# Run migrations manually
docker-compose -f docker-compose.prod.yml exec app dotnet ef database update
```

**Solutions:**
```bash
# Reset migrations (CAUTION: Data loss)
docker-compose -f docker-compose.prod.yml exec app dotnet ef database drop --force
docker-compose -f docker-compose.prod.yml exec app dotnet ef database update

# Backup before migration
docker-compose -f docker-compose.prod.yml exec postgres pg_dump -U harmoni360 Harmoni360_Prod > backup_before_migration.sql
```

#### Problem: Database performance issues
```bash
# Check database statistics
docker-compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod -c "SELECT schemaname,tablename,attname,n_distinct,correlation FROM pg_stats WHERE tablename = 'your_table';"

# Analyze query performance
docker-compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod -c "EXPLAIN ANALYZE SELECT * FROM your_table WHERE condition;"
```

**Solutions:**
```bash
# Run database maintenance
docker-compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod -c "VACUUM ANALYZE;"

# Update table statistics
docker-compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod -c "ANALYZE;"

# Reindex if needed
docker-compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod -c "REINDEX DATABASE Harmoni360_Prod;"
```

### 5. Network and Connectivity Issues

#### Problem: Cannot access application
```bash
# Check if services are listening
sudo netstat -tulpn | grep :80
sudo netstat -tulpn | grep :443

# Check firewall rules
sudo ufw status

# Test internal connectivity
docker-compose -f docker-compose.prod.yml exec nginx curl -I http://app:8080/health
```

**Solutions:**
```bash
# Open required ports
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Check nginx configuration
docker-compose -f docker-compose.prod.yml exec nginx nginx -t

# Restart nginx
docker-compose -f docker-compose.prod.yml restart nginx
```

#### Problem: SignalR connection issues
```bash
# Check WebSocket connectivity
# Use browser developer tools to check WebSocket connections

# Check nginx WebSocket configuration
docker-compose -f docker-compose.prod.yml exec nginx cat /etc/nginx/nginx.conf | grep -A 10 "location /hubs/"
```

**Solutions:**
```bash
# Ensure WebSocket headers are properly set in nginx configuration
# Restart nginx after configuration changes
docker-compose -f docker-compose.prod.yml restart nginx
```

### 6. File Upload Issues

#### Problem: File upload failures
```bash
# Check upload directory permissions
ls -la /opt/harmoni360/data/uploads/

# Check nginx file size limits
docker-compose -f docker-compose.prod.yml exec nginx cat /etc/nginx/nginx.conf | grep client_max_body_size
```

**Solutions:**
```bash
# Fix upload directory permissions
sudo chown -R 1000:1000 /opt/harmoni360/data/uploads/
chmod -R 755 /opt/harmoni360/data/uploads/

# Increase file size limits in nginx.conf
# client_max_body_size 100m;

# Restart nginx
docker-compose -f docker-compose.prod.yml restart nginx
```

### 7. Monitoring and Logging Issues

#### Problem: Logs not appearing in Seq
```bash
# Check Seq service status
docker-compose -f docker-compose.prod.yml logs seq

# Test Seq connectivity from application
docker-compose -f docker-compose.prod.yml exec app curl -I http://seq:5341/health
```

**Solutions:**
```bash
# Restart Seq service
docker-compose -f docker-compose.prod.yml restart seq

# Check Seq configuration in application
docker-compose -f docker-compose.prod.yml exec app printenv | grep Seq
```

#### Problem: Grafana dashboards not loading
```bash
# Check Prometheus connectivity
docker-compose -f docker-compose.prod.yml exec grafana curl -I http://prometheus:9090/api/v1/status/config
```

**Solutions:**
```bash
# Restart monitoring services
docker-compose -f docker-compose.prod.yml restart prometheus grafana

# Check Prometheus configuration
docker-compose -f docker-compose.prod.yml exec prometheus cat /etc/prometheus/prometheus.yml
```

### 8. Backup and Recovery Issues

#### Problem: Backup script failures
```bash
# Check backup script logs
tail -f /var/log/cron.log | grep backup

# Test backup script manually
/opt/harmoni360/scripts/backup.sh
```

**Solutions:**
```bash
# Fix backup script permissions
chmod +x /opt/harmoni360/scripts/backup.sh

# Check backup directory permissions
ls -la /opt/harmoni360/backups/

# Test database backup manually
docker-compose -f docker-compose.prod.yml exec postgres pg_dump -U harmoni360 Harmoni360_Prod > test_backup.sql
```

### 9. System Resource Issues

#### Problem: Disk space running low
```bash
# Check disk usage
df -h
du -sh /opt/harmoni360/*

# Check Docker disk usage
docker system df
```

**Solutions:**
```bash
# Clean up old Docker images and containers
docker system prune -a -f

# Clean up old log files
find /opt/harmoni360/logs -name "*.log" -mtime +30 -delete

# Clean up old backups
find /opt/harmoni360/backups -name "*.sql" -mtime +30 -delete
find /opt/harmoni360/backups -name "*.tar.gz" -mtime +30 -delete

# Rotate logs
docker-compose -f docker-compose.prod.yml restart
```

### 10. Emergency Recovery Procedures

#### Complete System Recovery
```bash
# 1. Stop all services
docker-compose -f docker-compose.prod.yml down

# 2. Backup current state
sudo tar -czf /tmp/harmoni360_emergency_backup.tar.gz /opt/harmoni360/

# 3. Restore from backup
# (Restore database and uploads from your backup location)

# 4. Restart services
docker-compose -f docker-compose.prod.yml up -d

# 5. Verify functionality
curl -k https://your-domain.com/health
```

#### Database Recovery
```bash
# 1. Stop application
docker-compose -f docker-compose.prod.yml stop app

# 2. Restore database from backup
docker-compose -f docker-compose.prod.yml exec postgres psql -U harmoni360 -d Harmoni360_Prod < backup_file.sql

# 3. Restart application
docker-compose -f docker-compose.prod.yml start app
```

## Getting Help

### Log Collection for Support
```bash
# Collect all relevant logs
mkdir -p /tmp/harmoni360-logs
docker-compose -f docker-compose.prod.yml logs > /tmp/harmoni360-logs/docker-compose.log
cp /var/log/syslog /tmp/harmoni360-logs/
cp /opt/harmoni360/logs/nginx/error.log /tmp/harmoni360-logs/
tar -czf harmoni360-support-logs.tar.gz /tmp/harmoni360-logs/
```

### System Information Collection
```bash
# Collect system information
uname -a > system-info.txt
docker version >> system-info.txt
docker-compose version >> system-info.txt
df -h >> system-info.txt
free -h >> system-info.txt
docker ps >> system-info.txt
```

### Contact Information
- Review the main [deployment documentation](./README.md)
- Check the [maintenance procedures](./Maintenance_Procedures.md)
- Consult the [server specifications](./Server_Hardware_Specifications.md)
