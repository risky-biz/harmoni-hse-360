# Harmoni360 Migration Strategy Guide

## Overview

This document provides comprehensive procedures for migrating Harmoni360 between different deployment environments, specifically between Fly.io cloud deployment and standalone server deployment.

## Migration Scenarios

### 1. Fly.io to Standalone Server
- Moving from cloud to on-premises
- Better control over infrastructure
- Cost optimization for long-term deployment

### 2. Standalone Server to Fly.io
- Moving from on-premises to cloud
- Improved scalability and managed infrastructure
- Reduced operational overhead

### 3. Standalone Server to Standalone Server
- Hardware upgrade or replacement
- Data center migration
- Disaster recovery scenarios

## Pre-Migration Planning

### 1. Assessment and Planning

#### Current Environment Analysis
```bash
# Document current Fly.io deployment
fly status -a harmoni360-app
fly postgres list
fly ext redis list

# Export current configuration
fly config show -a harmoni360-app > current-fly-config.toml

# Document database size and structure
fly postgres connect -a harmoni360-db
\l+
\dt+
SELECT pg_size_pretty(pg_database_size('harmoni360'));
```

#### Capacity Planning
- Estimate data transfer time based on database size
- Plan for acceptable downtime window
- Identify peak usage times to avoid
- Prepare rollback procedures

### 2. Migration Timeline

#### Phase 1: Preparation (1-2 weeks)
- Set up target environment
- Test migration procedures
- Prepare monitoring and alerting

#### Phase 2: Data Migration (1-2 days)
- Database migration
- File upload migration
- Configuration migration

#### Phase 3: Cutover (4-8 hours)
- DNS changes
- Final data sync
- Application testing

#### Phase 4: Post-Migration (1 week)
- Monitoring and optimization
- Performance tuning
- Documentation updates

## Migration Procedures

### Fly.io to Standalone Server Migration

#### Step 1: Prepare Standalone Server

```bash
# Follow the complete standalone server setup guide
# Ensure all services are running and configured

# Test standalone server deployment with sample data
docker-compose -f docker-compose.prod.yml up -d
```

#### Step 2: Export Data from Fly.io

```bash
# Create database dump from Fly.io
fly postgres connect -a harmoni360-db
pg_dump -h localhost -U postgres -d harmoni360 --no-owner --no-privileges > flyio_database_export.sql

# Download the database dump
fly ssh sftp get /tmp/flyio_database_export.sql -a harmoni360-db

# Export file uploads (if using Fly.io volumes)
fly ssh console -a harmoni360-app
tar -czf /tmp/uploads_backup.tar.gz -C /app uploads/
exit

# Download uploads backup
fly ssh sftp get /tmp/uploads_backup.tar.gz -a harmoni360-app
```

#### Step 3: Import Data to Standalone Server

```bash
# Stop application to prevent data changes during migration
docker-compose -f docker-compose.prod.yml stop app

# Import database
docker-compose -f docker-compose.prod.yml exec -T postgres psql -U harmoni360 -d Harmoni360_Prod < flyio_database_export.sql

# Import file uploads
tar -xzf uploads_backup.tar.gz -C /opt/harmoni360/data/

# Set proper permissions
sudo chown -R 1000:1000 /opt/harmoni360/data/uploads/

# Start application
docker-compose -f docker-compose.prod.yml start app
```

#### Step 4: DNS and SSL Configuration

See [DNS Configuration](./DNS_Configuration.md) for detailed steps on creating A
and CNAME records before updating SSL certificates.

```bash
# Update DNS records to point to standalone server
# A record: your-domain.com -> your-server-ip
# CNAME record: www.your-domain.com -> your-domain.com

# Obtain SSL certificate for standalone server
sudo certbot certonly --standalone -d your-domain.com -d www.your-domain.com

# Copy certificates to Docker volume
sudo cp /etc/letsencrypt/live/your-domain.com/fullchain.pem /opt/harmoni360/ssl/cert.pem
sudo cp /etc/letsencrypt/live/your-domain.com/privkey.pem /opt/harmoni360/ssl/key.pem

# Restart nginx to load new certificates
docker-compose -f docker-compose.prod.yml restart nginx
```

#### Step 5: Verification and Testing

```bash
# Test application functionality
curl -k https://your-domain.com/health

# Test database connectivity
docker-compose -f docker-compose.prod.yml exec app dotnet ef database update --dry-run

# Test file uploads
# Use the application UI to upload a test file

# Monitor logs for errors
docker-compose -f docker-compose.prod.yml logs -f app
```

### Standalone Server to Fly.io Migration

#### Step 1: Prepare Fly.io Environment

```bash
# Initialize Fly.io application
fly launch --no-deploy --name harmoni360-app

# Create PostgreSQL database
fly postgres create --name harmoni360-db --region sjc

# Create Redis instance
fly ext redis create --name harmoni360-redis

# Configure fly.toml based on fly.toml.example
cp fly.toml.example fly.toml
# Edit fly.toml with your specific configuration
```

#### Step 2: Export Data from Standalone Server

```bash
# Create database dump
docker-compose -f docker-compose.prod.yml exec postgres pg_dump -U harmoni360 Harmoni360_Prod --no-owner --no-privileges > standalone_database_export.sql

# Create uploads backup
tar -czf standalone_uploads_backup.tar.gz -C /opt/harmoni360/data uploads/
```

#### Step 3: Import Data to Fly.io

```bash
# Upload database dump to Fly.io
fly ssh console -a harmoni360-db
# Copy the SQL file to the container and import
psql -U postgres -d harmoni360 < standalone_database_export.sql

# Create volume for uploads
fly volumes create harmoni360_uploads --region sjc --size 10

# Upload files to Fly.io volume
fly ssh console -a harmoni360-app
# Extract uploads backup to /app/uploads/
```

#### Step 4: Deploy and Configure

```bash
# Deploy application to Fly.io
fly deploy -a harmoni360-app

# Run database migrations
fly ssh console -a harmoni360-app -C "cd /app && dotnet ef database update"

# Configure environment variables
fly secrets set JWT_KEY="your-jwt-key" -a harmoni360-app
fly secrets set ConnectionStrings__DefaultConnection="your-db-connection" -a harmoni360-app
```

### Server-to-Server Migration

#### Step 1: Prepare Target Server

```bash
# Set up new server following the standalone server guide
# Ensure all prerequisites are met
# Configure Docker and Docker Compose
```

#### Step 2: Sync Data

```bash
# Create full backup on source server
/opt/harmoni360/scripts/backup.sh

# Transfer backup to target server
rsync -avz --progress /opt/harmoni360/backups/ user@target-server:/opt/harmoni360/backups/

# Transfer configuration files
rsync -avz --progress /home/ubuntu/harmoni-hse-360/ user@target-server:/home/ubuntu/harmoni-hse-360/
```

#### Step 3: Restore on Target Server

```bash
# On target server, restore database
docker-compose -f docker-compose.prod.yml exec -T postgres psql -U harmoni360 -d Harmoni360_Prod < /opt/harmoni360/backups/postgres/latest_backup.sql

# Restore uploads
tar -xzf /opt/harmoni360/backups/uploads/latest_backup.tar.gz -C /opt/harmoni360/data/

# Start services
docker-compose -f docker-compose.prod.yml up -d
```

## Zero-Downtime Migration Strategies

### Blue-Green Deployment

#### Setup
```bash
# Prepare identical environment (Green)
# Keep current environment running (Blue)
# Sync data to Green environment
# Test Green environment thoroughly
```

#### Cutover
```bash
# Update DNS to point to Green environment
# Monitor for issues
# Keep Blue environment as fallback
```

### Database Replication Migration

#### Setup Master-Slave Replication
```bash
# Configure source database as master
# Configure target database as slave
# Allow replication to sync
```

#### Cutover
```bash
# Stop writes to master
# Promote slave to master
# Update application configuration
# Redirect traffic to new environment
```

## Rollback Procedures

### Emergency Rollback

#### DNS Rollback
```bash
# Revert DNS changes to previous environment
# Update TTL to minimum value for faster propagation
# Monitor application health
```

#### Database Rollback
```bash
# Stop application on new environment
# Restore database from pre-migration backup
# Restart application on original environment
# Verify data integrity
```

### Partial Rollback

#### Application-Only Rollback
```bash
# Keep data on new environment
# Rollback application deployment
# Update configuration to use new database
```

## Post-Migration Tasks

### 1. Performance Optimization

```bash
# Monitor application performance
# Optimize database queries
# Adjust resource allocations
# Update monitoring thresholds
```

### 2. Security Review

```bash
# Update firewall rules
# Review SSL certificate configuration
# Update backup procedures
# Test disaster recovery procedures
```

### 3. Documentation Updates

```bash
# Update deployment documentation
# Update monitoring procedures
# Update backup and recovery procedures
# Update troubleshooting guides
```

## Migration Checklist

### Pre-Migration
- [ ] Target environment prepared and tested
- [ ] Migration procedures documented and tested
- [ ] Rollback procedures prepared
- [ ] Stakeholders notified of migration window
- [ ] Monitoring and alerting configured

### During Migration
- [ ] Source environment backed up
- [ ] Data exported successfully
- [ ] Data imported successfully
- [ ] Application functionality verified
- [ ] Performance benchmarks met

### Post-Migration
- [ ] DNS changes propagated
- [ ] SSL certificates valid
- [ ] Monitoring systems updated
- [ ] Backup procedures tested
- [ ] Documentation updated
- [ ] Stakeholders notified of completion

## Troubleshooting Migration Issues

### Common Issues

#### Database Connection Errors
```bash
# Check connection strings
# Verify database credentials
# Test network connectivity
# Check firewall rules
```

#### File Upload Issues
```bash
# Verify file permissions
# Check storage paths
# Test upload functionality
# Monitor disk space
```

#### Performance Degradation
```bash
# Compare resource utilization
# Check database query performance
# Monitor network latency
# Review application logs
```

### Recovery Procedures

#### Data Corruption
```bash
# Stop application immediately
# Restore from last known good backup
# Verify data integrity
# Investigate root cause
```

#### Service Unavailability
```bash
# Check service status
# Review error logs
# Restart affected services
# Escalate if necessary
```

## Support and Resources

### Migration Support
- Review [troubleshooting guide](./Troubleshooting_Guide.md)
- Consult [maintenance procedures](./Maintenance_Procedures.md)
- Check [deployment documentation](./README.md)

### Emergency Contacts
- Document emergency contact procedures
- Prepare escalation matrix
- Maintain communication channels during migration
