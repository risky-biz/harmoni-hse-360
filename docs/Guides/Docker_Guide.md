# Docker Setup Guide for HarmoniHSE360

This guide explains how to run HarmoniHSE360 using Docker for both development and production environments.

## Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose v2.0+
- At least 4GB of available RAM
- 10GB of free disk space

## Quick Start

### Development Environment

#### Option 1: Simple Development Setup (Recommended)

1. **Start database services only**
   ```bash
   docker compose -f docker-compose.dev-simple.yml up -d
   ```

2. **Run backend locally**
   ```bash
   cd src/HarmoniHSE360.Web
   dotnet run
   ```

3. **Run frontend locally** (new terminal)
   ```bash
   cd src/HarmoniHSE360.Web/ClientApp
   npm install
   npm run dev
   ```

4. **Access the application**
   - Frontend: http://localhost:5173
   - Backend API: http://localhost:5000
   - pgAdmin: http://localhost:5050
     - Email: `dev@harmonihse360.com`
     - Password: `DevPassword123!`

#### Option 2: Full Docker Development (Advanced)

1. **Run the full development environment**
   ```bash
   docker compose -f docker-compose.dev.yml up
   ```

2. **Access the application**
   - Frontend: http://localhost:5173
   - Backend API: http://localhost:5000
   - pgAdmin: http://localhost:5050

**Note**: The full Docker development setup requires building a custom development image which may take longer.

### Default Login Credentials
- Username: `admin@harmonihse360.com`
- Password: `Admin123!`
- See `docs/Guides/Seeded_Users.md` for more test users

### Production Environment

1. **Create environment file**
   ```bash
   cp .env.example .env
   # Edit .env with your production values
   ```

2. **Build and run production**
   ```bash
   docker compose up -d
   ```

3. **Access the application**
   - Application: http://localhost:8080 (or configured port)
   - Nginx (if enabled): http://localhost:80

## Architecture Overview

### Development Setup
- **Frontend**: Vite dev server with hot reload (port 5173)
- **Backend**: .NET with hot reload (port 5000)
- **Database**: PostgreSQL (port 5432)
- **Cache**: Redis (port 6379)
- **Admin**: pgAdmin (port 5050)

### Production Setup
- **App**: Combined frontend and backend (port 8080)
- **Proxy**: Nginx reverse proxy (ports 80/443)
- **Database**: PostgreSQL (internal network)
- **Cache**: Redis with password (internal network)

## Common Docker Commands

### Development

```bash
# Start services
docker compose -f docker-compose.dev.yml up

# Start in background
docker compose -f docker-compose.dev.yml up -d

# View logs
docker compose -f docker-compose.dev.yml logs -f app

# Stop services
docker compose -f docker-compose.dev.yml down

# Rebuild after code changes
docker compose -f docker-compose.dev.yml up --build

# Run database migrations
docker compose -f docker-compose.dev.yml exec app dotnet ef database update
```

### Production

```bash
# Start services
docker compose up -d

# View logs
docker compose logs -f app

# Stop services
docker compose down

# Update and restart
docker compose pull
docker compose up -d

# Backup database
docker compose exec postgres pg_dump -U postgres HarmoniHSE360 > backup.sql
```

## Environment Variables

Key environment variables (see `.env.example` for full list):

- `APP_PORT`: Application port (default: 8080)
- `POSTGRES_PASSWORD`: Database password
- `JWT_KEY`: JWT signing key (min 32 characters)
- `REDIS_PASSWORD`: Redis password
- `CORS_ORIGINS`: Allowed CORS origins

## Troubleshooting

### Port Conflicts
If you get port binding errors:
```bash
# Check what's using the port
lsof -i :5173  # Mac/Linux
netstat -ano | findstr :5173  # Windows

# Change port in docker-compose file or .env
```

### Database Connection Issues
```bash
# Check if database is healthy
docker compose ps
docker compose logs postgres

# Connect to database manually
docker compose exec postgres psql -U postgres -d HarmoniHSE360
```

### Permission Issues
```bash
# Fix upload directory permissions
docker compose exec app chown -R appuser:appgroup /app/uploads
```

### Memory Issues
Increase Docker Desktop memory allocation:
- Docker Desktop → Settings → Resources → Memory: 4GB minimum

## Data Persistence

### Volumes
- `postgres_data`: Database files
- `app_uploads`: User uploaded files
- `app_logs`: Application logs
- `pgadmin_data`: pgAdmin configuration

### Backup and Restore

**Backup:**
```bash
# Database
docker compose exec postgres pg_dump -U postgres HarmoniHSE360 > backup_$(date +%Y%m%d).sql

# Uploads
docker run --rm -v harmonihse360_app_uploads:/data -v $(pwd):/backup alpine tar czf /backup/uploads_$(date +%Y%m%d).tar.gz -C /data .
```

**Restore:**
```bash
# Database
docker compose exec -T postgres psql -U postgres HarmoniHSE360 < backup_20240101.sql

# Uploads
docker run --rm -v harmonihse360_app_uploads:/data -v $(pwd):/backup alpine tar xzf /backup/uploads_20240101.tar.gz -C /data
```

## Security Considerations

1. **Change default passwords** in production
2. **Use environment files** (.env) and never commit them
3. **Enable HTTPS** in production (see nginx.conf)
4. **Regular updates**: Keep Docker images updated
5. **Network isolation**: Production database should not expose ports

## Performance Tuning

### Docker Compose Settings
```yaml
deploy:
  resources:
    limits:
      cpus: '2'
      memory: 2G
    reservations:
      cpus: '1'
      memory: 1G
```

### PostgreSQL Tuning
Add to postgres service:
```yaml
command: 
  - "postgres"
  - "-c"
  - "shared_buffers=256MB"
  - "-c"
  - "max_connections=200"
```

## Monitoring

### View real-time metrics
```bash
# CPU and memory usage
docker stats

# Disk usage
docker system df
```

### Application logs
```bash
# All logs
docker compose logs

# Specific service
docker compose logs -f app

# Last 100 lines
docker compose logs --tail=100 app
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET Docker Documentation](https://docs.microsoft.com/en-us/dotnet/core/docker/introduction)