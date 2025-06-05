# HarmoniHSE360 Demo Preparation Guide

## Overview

This guide provides step-by-step instructions for preparing your HarmoniHSE360 application for client demonstrations after successful deployment to Fly.io.

## Pre-Demo Checklist

### 1. Application Health Verification

**Check Application Status:**
```bash
# Verify application is running
fly status -a harmonihse360-app

# Check health endpoint
curl https://harmonihse360-app.fly.dev/health

# Monitor logs for errors
fly logs -a harmonihse360-app | grep -i error
```

**Expected Results:**
- ✅ Application status: "running"
- ✅ Health check: HTTP 200 response
- ✅ No critical errors in logs

### 2. Database Connectivity Test

```bash
# Connect to application console
fly ssh console -a harmonihse360-app

# Test database connection
cd /app
dotnet ef database update --dry-run
```

**Expected Results:**
- ✅ Database connection successful
- ✅ All migrations applied
- ✅ No pending migrations

### 3. Redis Functionality Test

```bash
# Test Redis connection from application
fly ssh console -a harmonihse360-app

# Test Redis connectivity (if redis-cli available)
redis-cli -u $ConnectionStrings__Redis ping
```

**Expected Results:**
- ✅ Redis responds with "PONG"
- ✅ No connection timeouts

## Demo Data Setup

### 1. Create Demo User Accounts

**Access Application Console:**
```bash
fly ssh console -a harmonihse360-app
cd /app
```

**Create Admin User:**
```bash
# Run user creation command (adjust based on your seeding implementation)
dotnet run --seed-users
```

**Demo User Accounts to Create:**

| Role | Email | Password | Purpose |
|------|-------|----------|---------|
| Admin | admin@harmonihse360.com | Admin123! | Full system access |
| HSE Manager | manager@harmonihse360.com | Manager123! | Management features |
| HSE Officer | officer@harmonihse360.com | Officer123! | Field operations |
| Employee | employee@harmonihse360.com | Employee123! | Basic user access |

### 2. Sample Incident Data

**Create Sample Incidents:**
```sql
-- Connect to database
fly postgres connect -a harmonihse360-db

-- Insert sample incidents
INSERT INTO Incidents (Title, Description, Severity, Status, ReportedBy, CreatedAt) VALUES
('Slip and Fall in Warehouse', 'Employee slipped on wet floor in warehouse area', 'Medium', 'Open', 'employee@harmonihse360.com', NOW()),
('Chemical Spill in Lab', 'Minor chemical spill in laboratory requiring cleanup', 'High', 'InProgress', 'officer@harmonihse360.com', NOW() - INTERVAL '2 days'),
('Equipment Malfunction', 'Safety equipment malfunction detected during inspection', 'Low', 'Resolved', 'manager@harmonihse360.com', NOW() - INTERVAL '1 week');
```

### 3. Sample Company Data

**Create Demo Organizations:**
```sql
-- Insert sample companies/departments
INSERT INTO Organizations (Name, Description, CreatedAt) VALUES
('Manufacturing Department', 'Main production facility', NOW()),
('Quality Assurance', 'Quality control and testing', NOW()),
('Maintenance Team', 'Equipment maintenance and repair', NOW());
```

### 4. File Upload Test Data

**Prepare Sample Files:**
```bash
# Create sample files for upload testing
fly ssh console -a harmonihse360-app
cd /app/uploads

# Create test directories
mkdir -p incidents/photos
mkdir -p reports/documents

# Verify permissions
ls -la
```

## Demo Scenarios

### Scenario 1: Incident Reporting Workflow

**Preparation:**
1. Ensure all user accounts are created
2. Have sample incident ready to create
3. Prepare sample photos for upload

**Demo Flow:**
1. **Login as Employee** → Report new incident
2. **Login as HSE Officer** → Review and investigate
3. **Login as Manager** → Approve resolution
4. **Show real-time notifications** (SignalR)

### Scenario 2: Dashboard and Analytics

**Preparation:**
1. Ensure sample incidents with various statuses exist
2. Create incidents across different time periods
3. Verify charts and metrics display correctly

**Demo Flow:**
1. **Login as Manager** → View dashboard
2. **Show incident trends** → Charts and graphs
3. **Filter by date range** → Demonstrate filtering
4. **Export reports** → PDF generation

### Scenario 3: Mobile Responsiveness

**Preparation:**
1. Test application on mobile devices
2. Verify touch interactions work
3. Check responsive design elements

**Demo Flow:**
1. **Open on mobile device** → Show responsive design
2. **Report incident on mobile** → Touch-friendly interface
3. **Upload photos** → Mobile camera integration

## Performance Optimization for Demo

### 1. Warm-up Application

```bash
# Pre-warm the application
curl https://harmonihse360-app.fly.dev/
curl https://harmonihse360-app.fly.dev/api/health
curl https://harmonihse360-app.fly.dev/incidents
```

### 2. Scale for Demo

```bash
# Scale up for demo if needed
fly scale count 2 -a harmonihse360-app
fly scale memory 1024 -a harmonihse360-app
```

### 3. Monitor During Demo

```bash
# Monitor logs during demo
fly logs -f -a harmonihse360-app

# Monitor metrics
fly metrics -a harmonihse360-app
```

## Demo Environment URLs

### Primary URLs
- **Application:** https://harmonihse360-app.fly.dev
- **Health Check:** https://harmonihse360-app.fly.dev/health
- **API Documentation:** https://harmonihse360-app.fly.dev/swagger

### Custom Domain (if configured)
- **Production URL:** https://harmonihse360.yourdomain.com

## Demo Script Template

### Introduction (2 minutes)
```
"Welcome to HarmoniHSE360, a comprehensive Health, Safety, and Environment management system built with modern technology stack including .NET 8, React 18, and real-time capabilities."
```

### Key Features Demonstration (15 minutes)

**1. User Authentication & Authorization (2 min)**
- Multi-role access control
- Secure JWT-based authentication
- Role-based feature access

**2. Incident Management (5 min)**
- Create new incident report
- Upload photos and documents
- Real-time status updates
- Workflow management

**3. Dashboard & Analytics (3 min)**
- Real-time metrics
- Interactive charts
- Trend analysis
- Export capabilities

**4. Real-time Features (2 min)**
- Live notifications
- SignalR integration
- Multi-user collaboration

**5. Mobile Responsiveness (3 min)**
- Responsive design
- Touch-friendly interface
- Mobile-optimized workflows

### Technical Architecture (5 minutes)
```
"The application is built using Clean Architecture principles with:
- .NET 8 backend with Entity Framework Core
- React 18 frontend with TypeScript
- PostgreSQL database with TimescaleDB for time-series data
- Redis for caching and real-time features
- Docker containerization
- Deployed on Fly.io for global performance"
```

## Post-Demo Cleanup

### 1. Reset Demo Data (Optional)

```bash
# Clear demo incidents
fly postgres connect -a harmonihse360-db
DELETE FROM Incidents WHERE ReportedBy LIKE '%@harmonihse360.com';

# Reset user accounts
UPDATE Users SET LastLoginAt = NULL WHERE Email LIKE '%@harmonihse360.com';
```

### 2. Scale Down Resources

```bash
# Scale back to minimal resources
fly scale count 1 -a harmonihse360-app
fly scale memory 512 -a harmonihse360-app
```

### 3. Backup Demo State

```bash
# Create backup of demo state
fly postgres backup create -a harmonihse360-db
```

## Troubleshooting During Demo

### Common Issues

**Application Slow to Load:**
```bash
# Quick restart
fly restart -a harmonihse360-app
```

**Database Connection Issues:**
```bash
# Check database status
fly status -a harmonihse360-db
```

**Real-time Features Not Working:**
```bash
# Check Redis status
fly ext redis status harmonihse360-redis
```

### Emergency Contacts

- **Technical Support:** [Your contact information]
- **Fly.io Status:** https://status.fly.io/
- **Backup Demo Environment:** [If available]

## Demo Success Metrics

### Technical Metrics
- [ ] Application load time < 3 seconds
- [ ] All features functional
- [ ] No error messages displayed
- [ ] Real-time features working
- [ ] Mobile responsiveness verified

### Business Metrics
- [ ] All demo scenarios completed
- [ ] Client questions answered
- [ ] Technical architecture explained
- [ ] Next steps discussed
- [ ] Follow-up scheduled

---

*Last Updated: January 2025*
*Version: 1.0*
