# Database Reseeding Guide

This guide provides step-by-step instructions for reseeding the database with demo data, particularly useful when incidents have been deleted and you need to restore sample data for testing.

## Overview

The Harmoni360 application includes an automated data seeding system that populates the database with:
- **Roles and Permissions** (Admin, HSEManager, Employee)
- **Demo Users** with different access levels
- **Sample Incidents** (6 incidents with various severities and statuses)
- **Corrective Actions** associated with incidents

## Prerequisites

- PostgreSQL database running and accessible
- .NET 8.0 SDK installed
- Development environment configured

## Method 1: Environment Variable Override (Recommended)

This is the fastest method that works without modifying configuration files.

### Steps:

1. **Stop the Backend** (if running):
   ```bash
   # Kill any existing backend processes
   pkill -f "dotnet.*Harmoni360" || lsof -ti:5000 | xargs kill -9
   ```

2. **Navigate to Backend Directory**:
   ```bash
   cd /path/to/harmoni-hse-360/src/Harmoni360.Web
   ```

3. **Start Backend with Reseeding Enabled**:
   ```bash
   ASPNETCORE_ENVIRONMENT=Development DataSeeding__ReSeedIncidents=true dotnet run
   ```

4. **Monitor the Logs**:
   Look for these log messages indicating successful reseeding:
   ```
   [INFO] ReSeedIncidents configuration value: True
   [INFO] Starting incident seeding...
   [INFO] Clearing existing incidents for re-seeding...
   [INFO] Existing incidents cleared
   [INFO] Seeded 6 incidents
   [INFO] Database seeding completed successfully
   ```

5. **Reset Configuration** (Optional):
   After seeding is complete, you can restart without the environment variable:
   ```bash
   # Stop with Ctrl+C, then restart normally
   dotnet run
   ```

## Method 2: Configuration File Modification

### Steps:

1. **Edit Configuration File**:
   ```bash
   # Edit appsettings.Development.json
   vi src/Harmoni360.Web/appsettings.Development.json
   ```

2. **Update the DataSeeding Section**:
   ```json
   {
     "DataSeeding": {
       "SeedIncidents": true,
       "ReSeedIncidents": true  // Change from false to true
     }
   }
   ```

3. **Restart the Backend**:
   ```bash
   cd src/Harmoni360.Web
   dotnet run
   ```

4. **Revert Configuration** (Important):
   After seeding completes, change `ReSeedIncidents` back to `false` to prevent accidental data loss:
   ```json
   {
     "DataSeeding": {
       "SeedIncidents": true,
       "ReSeedIncidents": false  // Reset to false
     }
   }
   ```

## Method 3: Background Process with Logging

For detailed monitoring of the seeding process:

1. **Start Backend in Background with Logging**:
   ```bash
   cd src/Harmoni360.Web
   ASPNETCORE_ENVIRONMENT=Development DataSeeding__ReSeedIncidents=true dotnet run > /tmp/backend.log 2>&1 &
   ```

2. **Monitor Seeding Progress**:
   ```bash
   # Watch for seeding logs
   tail -f /tmp/backend.log | grep -E "(ReSeedIncidents|incident seeding|Seeded.*incidents)"
   ```

3. **Check Completion**:
   ```bash
   # Verify backend is running
   curl http://localhost:5000/health
   ```

## Seeded Data Details

### Demo Users
- **Admin**: `admin@bsj.sch.id` / `Admin123!`
- **HSE Manager**: `hse.manager@bsj.sch.id` / `HSE123!`
- **Employee 1**: `john.doe@bsj.sch.id` / `Employee123!`
- **Employee 2**: `jane.smith@bsj.sch.id` / `Employee123!`

### Sample Incidents
1. **Fire alarm system malfunction** (Serious - Awaiting Action)
2. **Student minor burn injury** (Moderate - Under Investigation)
3. **Slip and fall incident** (Minor - Resolved)
4. **Playground equipment failure** (Critical - Awaiting Action)
5. **Food poisoning symptoms** (Serious - Closed)
6. **Student ankle sprain** (Minor - Resolved)

## Troubleshooting

### Common Issues

**1. Configuration Not Taking Effect**
- **Problem**: `ReSeedIncidents configuration value: False` in logs
- **Solution**: Use environment variable method instead of config file modification

**2. Port Already in Use**
- **Problem**: `Address already in use` error
- **Solution**: Kill existing processes:
  ```bash
  lsof -ti:5000 | xargs kill -9
  sleep 2
  ```

**3. Database Connection Issues**
- **Problem**: Cannot connect to PostgreSQL
- **Solution**: Verify database is running:
  ```bash
  # Check PostgreSQL status
  sudo systemctl status postgresql
  # Or check if port 5432 is open
  netstat -ln | grep 5432
  ```

**4. Seeding Logs Not Appearing**
- **Problem**: No seeding messages in logs
- **Solution**: Ensure you're in Development environment:
  ```bash
  echo $ASPNETCORE_ENVIRONMENT  # Should show "Development"
  ```

### Log Messages to Look For

**Successful Seeding:**
```
[INFO] ReSeedIncidents configuration value: True
[INFO] Incidents exist in database: True
[INFO] Starting incident seeding...
[INFO] Clearing existing incidents for re-seeding...
[INFO] Existing incidents cleared
[INFO] Seeded 6 incidents
[INFO] Database seeding completed successfully
```

**Skipped Seeding:**
```
[INFO] ReSeedIncidents configuration value: False
[INFO] Incidents already exist and ReSeedIncidents is false, skipping incident seeding
```

## Post-Seeding Verification

1. **Check API Endpoint**:
   ```bash
   curl -H "Content-Type: application/json" http://localhost:5000/api/incident
   ```

2. **Login to Frontend**:
   - Go to `http://localhost:5173`
   - Login with any demo user credentials
   - Navigate to Incident List to verify 6 incidents are present

3. **Test Functionality**:
   - Try creating a new incident
   - Test the View, Edit, and Delete buttons
   - Verify real-time updates work correctly

## Security Notes

- **Never enable `ReSeedIncidents` in production environments**
- **Always reset the configuration after seeding**
- **The demo users have simple passwords - change them in production**
- **Seeding will completely replace existing incident data**

## Related Files

- **Seeder Implementation**: `src/Harmoni360.Infrastructure/Services/DataSeeder.cs`
- **Configuration**: `src/Harmoni360.Web/appsettings.Development.json`
- **Startup Logic**: `src/Harmoni360.Web/Program.cs` (lines 168-215)

---

**Last Updated**: December 2024  
**Applies to**: Harmoni360 v1.0