# Harmoni360 Standalone Server Hardware Specifications

## Executive Summary

This document provides comprehensive hardware specifications for deploying Harmoni360 on a standalone physical server to support 100 concurrent users. The specifications are based on the application stack (.NET 8 backend, React frontend, PostgreSQL, Redis, SignalR) and real-world performance requirements.

## Application Stack Analysis

### Technology Stack
- **Backend**: .NET 8 Web API with SignalR
- **Frontend**: React with TypeScript (SPA)
- **Database**: PostgreSQL 15
- **Cache**: Redis 7
- **Reverse Proxy**: Nginx
- **Logging**: Seq + Serilog
- **Monitoring**: Prometheus + Grafana
- **Container Runtime**: Docker + Docker Compose

### Performance Characteristics
- **Real-time Features**: SignalR for incident notifications, health monitoring
- **File Uploads**: Support for 100MB file uploads (incident reports, PPE documentation)
- **Database Operations**: Heavy read/write operations for incident management
- **Caching**: Redis for session management and performance optimization

## Server Specifications

### Minimum Viable Configuration (100 Concurrent Users)

#### Hardware Requirements
- **CPU**: 8 cores / 16 threads (Intel Xeon E-2288G or AMD EPYC 7302P equivalent)
- **RAM**: 32 GB DDR4 ECC
- **Storage**: 
  - **System**: 500 GB NVMe SSD (OS + Applications)
  - **Database**: 1 TB NVMe SSD (PostgreSQL data)
  - **Uploads**: 2 TB SATA SSD (File storage)
- **Network**: Gigabit Ethernet (1 Gbps)
- **Power**: Redundant PSU (750W minimum)

#### Estimated Cost: $3,500 - $4,500

### Optimal Configuration (100+ Concurrent Users with Growth)

#### Hardware Requirements
- **CPU**: 16 cores / 32 threads (Intel Xeon Silver 4314 or AMD EPYC 7443P)
- **RAM**: 64 GB DDR4 ECC
- **Storage**:
  - **System**: 1 TB NVMe SSD (OS + Applications)
  - **Database**: 2 TB NVMe SSD RAID 1 (PostgreSQL data with redundancy)
  - **Uploads**: 4 TB SATA SSD RAID 1 (File storage with redundancy)
  - **Backup**: 8 TB HDD (Local backups)
- **Network**: Dual Gigabit Ethernet (bonded for redundancy)
- **Power**: Redundant PSU (1000W)

#### Estimated Cost: $6,500 - $8,500

## Performance Calculations

### Concurrent User Analysis

#### Per User Resource Consumption
- **Memory**: ~256 MB per active user (including SignalR connections)
- **CPU**: ~0.1 CPU cores per active user during peak operations
- **Database Connections**: ~2-3 connections per active user
- **Network**: ~50 KB/s per user average, 500 KB/s peak

#### 100 Concurrent Users Requirements
- **Total Memory**: 25.6 GB (application) + 8 GB (OS/services) = ~34 GB
- **Total CPU**: 10 cores peak usage
- **Database Connections**: 200-300 concurrent connections
- **Network Bandwidth**: 5 MB/s average, 50 MB/s peak

### Storage Requirements

#### Database Growth Estimation
- **Initial Size**: 1 GB
- **Monthly Growth**: 2-5 GB (depending on incident volume)
- **3-Year Projection**: 100-200 GB
- **Recommended**: 1 TB minimum for 5+ years

#### File Upload Storage
- **Average File Size**: 5 MB per incident report
- **Monthly Uploads**: 1,000 files = 5 GB
- **3-Year Projection**: 180 GB
- **Recommended**: 2 TB minimum with growth buffer

## Operating System Requirements

### Recommended OS
- **Primary**: Ubuntu Server 22.04 LTS
- **Alternative**: CentOS Stream 9 or RHEL 9
- **Container Runtime**: Docker CE 24.0+ with Docker Compose v2

### System Configuration
- **Kernel**: Linux 5.15+ with container optimizations
- **File System**: ext4 or XFS for data volumes
- **Swap**: 8 GB (25% of RAM for minimum config)
- **Open Files Limit**: 65536
- **Network Buffers**: Optimized for high concurrent connections

## Network Requirements

### Bandwidth Specifications
- **Minimum**: 100 Mbps dedicated bandwidth
- **Recommended**: 500 Mbps dedicated bandwidth
- **Peak Usage**: Up to 50 MB/s during file uploads

### Network Configuration
- **Static IP**: Required for SSL certificate and DNS
- **Firewall**: Hardware firewall recommended
- **Load Balancing**: Not required for single server setup
- **CDN**: Optional for static asset delivery

## Scaling Considerations

### Vertical Scaling (Single Server)
- **CPU**: Can scale to 32+ cores for 200+ users
- **RAM**: Can scale to 128 GB for 300+ users
- **Storage**: Add additional NVMe drives as needed

### Horizontal Scaling Triggers
- **CPU Usage**: Consistently >80% during business hours
- **Memory Usage**: >90% utilization
- **Database Connections**: Approaching PostgreSQL limits (100 per GB RAM)
- **Response Time**: >2 seconds for API calls

### Migration to Multi-Server Architecture
When single server reaches limits:
1. **Database Server**: Separate PostgreSQL to dedicated server
2. **Application Servers**: Multiple app servers with load balancer
3. **File Storage**: Network-attached storage (NAS) or object storage
4. **Cache Layer**: Redis cluster for high availability

## Cost Analysis

### Initial Hardware Investment
| Configuration | Hardware Cost | 3-Year TCO* |
|---------------|---------------|-------------|
| Minimum Viable | $4,000 | $8,000 |
| Optimal | $7,500 | $12,000 |

*TCO includes hardware, power, cooling, maintenance

### Operational Costs (Annual)
- **Power Consumption**: $500-800/year (24/7 operation)
- **Internet/Hosting**: $1,200-2,400/year (colocation)
- **Maintenance**: $500-1,000/year
- **Backup Storage**: $200-500/year (cloud backup)

### Comparison with Cloud Hosting
- **Fly.io Equivalent**: ~$200-400/month = $2,400-4,800/year
- **AWS/Azure Equivalent**: ~$300-600/month = $3,600-7,200/year
- **Break-even Point**: 12-18 months for standalone server

## Monitoring and Alerting Thresholds

### Critical Alerts
- **CPU Usage**: >90% for 5+ minutes
- **Memory Usage**: >95% for 2+ minutes
- **Disk Space**: <10% free space
- **Database Connections**: >80% of max connections

### Warning Alerts
- **CPU Usage**: >75% for 15+ minutes
- **Memory Usage**: >85% for 10+ minutes
- **Disk Space**: <20% free space
- **Response Time**: >1 second average

## Backup and Disaster Recovery

### Backup Requirements
- **Database**: Daily full backup + continuous WAL archiving
- **File Uploads**: Daily incremental backup
- **Configuration**: Weekly backup of Docker configs
- **Retention**: 30 days local + 1 year cloud storage

### Recovery Time Objectives
- **RTO (Recovery Time)**: 4 hours maximum
- **RPO (Recovery Point)**: 1 hour maximum data loss
- **Backup Verification**: Weekly restore testing

## Security Considerations

### Physical Security
- **Server Location**: Secure data center or locked server room
- **Access Control**: Biometric or key card access
- **Environmental**: UPS, fire suppression, climate control

### Network Security
- **Firewall**: Hardware firewall with DPI
- **VPN**: Site-to-site VPN for remote management
- **Monitoring**: Network intrusion detection system

## Conclusion

The recommended optimal configuration provides excellent performance for 100 concurrent users with room for growth. The initial investment of $7,500 pays for itself within 18 months compared to cloud hosting, while providing better performance and data control.

For organizations starting with budget constraints, the minimum viable configuration can be upgraded incrementally as usage grows.
