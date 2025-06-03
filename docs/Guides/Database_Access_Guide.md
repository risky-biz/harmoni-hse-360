# Database Access Guide

## 🚀 Quick Start

### Database Credentials
```
Host: timescaledb (from Docker containers)
Host: localhost (from your computer)
Port: 5432
Database: harmonihse360_dev
Username: harmonihse360
Password: HarmoniHSE360!2024
```

## 📊 pgAdmin Web Interface

### 1. Start pgAdmin (if not already running)
```bash
docker-compose up -d pgadmin
```

### 2. Access pgAdmin
Open your browser: http://localhost:5050

### 3. Login Credentials
- **Email**: `admin@harmonihse360.com`
- **Password**: `PgAdmin2024!`

### 4. Add Database Server in pgAdmin

1. Right-click "Servers" → "Register" → "Server"
2. **General tab**: 
   - Name: `HarmoniHSE360 Dev`
3. **Connection tab**:
   - **Host**: `timescaledb` (⚠️ NOT localhost - pgAdmin runs in Docker)
   - **Port**: `5432`
   - **Database**: `harmonihse360_dev`
   - **Username**: `harmonihse360`
   - **Password**: `HarmoniHSE360!2024`
   - **Save password**: ✓ (check this)
4. Click "Save"

## 🔗 Connection Methods

### From Docker Containers (Internal Network)
Use hostname `timescaledb`:
```
Host: timescaledb
Port: 5432
Database: harmonihse360_dev
Username: harmonihse360
Password: HarmoniHSE360!2024
```

### From Your Computer (External)
Use `localhost`:
```
Host: localhost
Port: 5432
Database: harmonihse360_dev
Username: harmonihse360
Password: HarmoniHSE360!2024
```

### Connection String for .NET Apps
```
Host=localhost;Port=5432;Database=harmonihse360_dev;Username=harmonihse360;Password=HarmoniHSE360!2024
```

## 🛠️ Alternative Database Tools

### Command Line (psql)
```bash
# From your computer
psql -h localhost -p 5432 -U harmonihse360 -d harmonihse360_dev

# From inside container
docker exec harmonihse360-timescaledb psql -U harmonihse360 -d harmonihse360_dev
```

### Desktop Applications
For TablePlus, DBeaver, DataGrip, etc.:
- Host: `localhost`
- Port: `5432`
- Database: `harmonihse360_dev`
- Username: `harmonihse360`
- Password: `HarmoniHSE360!2024`

## 🔍 Useful SQL Queries

### Check TimescaleDB Extension
```sql
SELECT default_version, installed_version 
FROM pg_available_extensions 
WHERE name = 'timescaledb';
```

### List All Tables
```sql
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;
```

### Check Database Size
```sql
SELECT pg_database.datname,
       pg_size_pretty(pg_database_size(pg_database.datname)) AS size
FROM pg_database
WHERE datname = 'harmonihse360_dev';
```

### List Hypertables (TimescaleDB)
```sql
SELECT hypertable_schema, hypertable_name, owner 
FROM timescaledb_information.hypertables;
```

### Create Module Schemas
```sql
CREATE SCHEMA IF NOT EXISTS user_management;
CREATE SCHEMA IF NOT EXISTS incident_management;
CREATE SCHEMA IF NOT EXISTS document_management;
CREATE SCHEMA IF NOT EXISTS hazard_reporting;
CREATE SCHEMA IF NOT EXISTS compliance_audit;
CREATE SCHEMA IF NOT EXISTS permit_to_work;
CREATE SCHEMA IF NOT EXISTS training;
CREATE SCHEMA IF NOT EXISTS environmental;
CREATE SCHEMA IF NOT EXISTS analytics;
```

## 🚨 Troubleshooting

### Connection Failed?

1. **Verify TimescaleDB is running:**
   ```bash
   docker-compose ps timescaledb
   ```

2. **Check the password in use:**
   The `docker-compose.override.yml` file sets the password. Make sure you're using: `HarmoniHSE360!2024`

3. **Test connection from command line:**
   ```bash
   docker exec harmonihse360-timescaledb psql -U harmonihse360 -d harmonihse360_dev -c "SELECT 1;"
   ```

4. **Check Docker network:**
   ```bash
   docker network inspect harmoni-hse-360_harmonihse360-network
   ```

### Reset Everything
If all else fails, reset the database:
```bash
docker-compose down
docker volume rm harmoni-hse-360_timescale_data
docker-compose up -d timescaledb
```

### Reset pgAdmin
```bash
docker-compose down pgadmin
docker volume rm harmoni-hse-360_pgadmin_data
docker-compose up -d pgadmin
```

## 📝 Development Tips

### 1. Use TimescaleDB for Time-Series Data
```sql
-- Create a hypertable for environmental readings
CREATE TABLE environmental_readings (
    time TIMESTAMPTZ NOT NULL,
    sensor_id INTEGER,
    temperature DOUBLE PRECISION,
    humidity DOUBLE PRECISION
);

SELECT create_hypertable('environmental_readings', 'time');
```

### 2. Enable UUID Extension
```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

### 3. Regular Backups
```bash
# Backup
docker exec harmonihse360-timescaledb pg_dump -U harmonihse360 harmonihse360_dev > backup_$(date +%Y%m%d).sql

# Restore
docker exec -i harmonihse360-timescaledb psql -U harmonihse360 harmonihse360_dev < backup_20240601.sql
```

## 🔐 Security Notes

1. **Change passwords before production deployment**
2. **Enable SSL/TLS for production connections**
3. **Use connection pooling for better performance**
4. **Implement row-level security for multi-tenant data**
5. **Regular security audits and updates**

## 📋 Quick Reference

| Service | URL/Port | Credentials |
|---------|----------|-------------|
| pgAdmin | http://localhost:5050 | admin@harmonihse360.com / PgAdmin2024! |
| PostgreSQL | localhost:5432 | harmonihse360 / HarmoniHSE360!2024 |
| Database | harmonihse360_dev | - |

---

**Remember**: When connecting from pgAdmin web interface, use `timescaledb` as the hostname, NOT `localhost`!