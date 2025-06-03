# 🚀 HarmoniHSE360 - Getting Started Guide

Complete guide for running and testing the HarmoniHSE360 application with authentication functionality.

## 🎯 Overview

HarmoniHSE360 is a **Modular Monolith** application built with:
- **Backend**: .NET 8.0 with Clean Architecture
- **Frontend**: React 18.2 + TypeScript with CoreUI components
- **Authentication**: JWT tokens with role-based access
- **Database**: PostgreSQL with Entity Framework Core
- **Architecture**: Single deployable unit with integrated React SPA

## 📋 Prerequisites

### For Local Development
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 20+** - [Download here](https://nodejs.org/)
- **PostgreSQL 15+** - [Download here](https://postgresql.org/download/) (or use Docker)

### For Docker Deployment
- **Docker Desktop** - [Download here](https://docker.com/products/docker-desktop)
- **Docker Compose** (included with Docker Desktop)

---

## 🛠️ Option 1: Local Development Setup

### Step 1: Database Setup

**Option A: Local PostgreSQL**
```bash
# Create database (assumes PostgreSQL is installed)
createdb HarmoniHSE360
```

**Option B: PostgreSQL via Docker**
```bash
# Run PostgreSQL in Docker
docker run --name harmonihse360-db \
  -e POSTGRES_DB=HarmoniHSE360 \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres123 \
  -p 5432:5432 -d postgres:15-alpine
```

### Step 2: Configure Application Settings

Create or update `src/HarmoniHSE360.Web/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=HarmoniHSE360;Username=postgres;Password=postgres123"
  },
  "Jwt": {
    "Key": "YourSuperSecretJwtKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "HarmoniHSE360",
    "Audience": "HarmoniHSE360Users",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Step 3: Create and Run Database Migrations

Since this is the first run, you need to create the initial migration:

**Option A: Using provided scripts**
```bash
# Windows PowerShell
./scripts/create-migration.ps1

# Linux/Mac/WSL
chmod +x ./scripts/create-migration.sh
./scripts/create-migration.sh
```

**Option B: Manual commands**
```bash
# Navigate to Web project
cd src/HarmoniHSE360.Web

# Create initial migration
dotnet ef migrations add InitialCreate -p ../HarmoniHSE360.Infrastructure -s . -c ApplicationDbContext

# Apply migration and seed data
dotnet ef database update -p ../HarmoniHSE360.Infrastructure -s . -c ApplicationDbContext
```

The database will be automatically seeded with demo users when running in Development environment.

### Step 4: Install Frontend Dependencies

```bash
# Navigate to React app
cd ClientApp

# Install npm packages
npm install
```

### Step 5: Start Development Servers

**Terminal 1 - React Dev Server:**
```bash
cd src/HarmoniHSE360.Web/ClientApp
npm run dev
```
*This starts Vite dev server on http://localhost:5173*

**Terminal 2 - .NET API:**
```bash
cd src/HarmoniHSE360.Web
dotnet run
```
*This starts the API server on http://localhost:5000*

### Step 6: Access the Application

- **Frontend (React)**: http://localhost:5173
- **Backend API**: http://localhost:5000
- **Swagger Documentation**: http://localhost:5000/swagger

---

## 🐳 Option 2: Docker Deployment

### Step 1: Environment Setup (Optional)

Create a `.env` file from the example:
```bash
cp .env.example .env
# Edit .env file with your specific paths (especially APPDATA on Windows)
```

### Step 2: Start All Services

```bash
# For development with hot reload
docker-compose up -d

# For production-like deployment
docker-compose -f docker-compose.yml up -d
```

### Step 3: Create and Run Database Migrations

Since this is the first run, you need to create the initial migration:

```bash
# Wait for services to be ready (about 30 seconds)

# Create the initial migration
docker-compose exec app dotnet ef migrations add InitialCreate -p ../HarmoniHSE360.Infrastructure -s . -c ApplicationDbContext

# Apply migration and seed data
docker-compose exec app dotnet ef database update -p ../HarmoniHSE360.Infrastructure -s . -c ApplicationDbContext
```

The database will be automatically seeded with demo users when the application starts in Development environment.

### Step 4: Access the Application

- **Full Application**: http://localhost:8080
- **Database**: localhost:5432 (accessible from host)
- **Swagger Documentation**: http://localhost:8080/swagger

### Step 5: Verify Services

```bash
# Check all services are running
docker-compose ps

# View application logs
docker-compose logs -f app

# View database logs
docker-compose logs postgres
```

### Development with Docker

The `docker-compose.override.yml` provides:
- **Hot reload**: Changes to C# code automatically restart the app
- **Volume mounts**: Edit code on your host, see changes in container
- **Development tools**: Entity Framework CLI available in container
- **Debugging support**: Attach debugger to running container

---

## 🔐 Testing Authentication

### Demo User Credentials

The application includes pre-seeded demo users:

| Role | Email | Password | Access Level |
|------|-------|----------|--------------|
| **Admin** | admin@bsj.sch.id | Admin123! | Full system access |
| **HSE Manager** | hse.manager@bsj.sch.id | HSE123! | HSE management features |
| **Employee** | john.doe@bsj.sch.id | Employee123! | Basic employee access |
| **Employee** | jane.smith@bsj.sch.id | Employee123! | Basic employee access |

### Authentication Test Scenarios

#### 1. Basic Login Flow
1. Navigate to the application URL
2. You'll be automatically redirected to `/login`
3. The login page displays demo user credentials with auto-fill buttons
4. Click any "Use Credentials" button or manually enter credentials
5. Upon successful login, you'll be redirected to `/dashboard`
6. JWT token is automatically stored in localStorage

#### 2. Protected Routes Test
1. Try accessing `/dashboard` without being logged in
2. Should redirect to `/login` page
3. After logging in, you should be able to access `/dashboard`
4. Try logging out - should redirect back to `/login`

#### 3. Token Validation Test
1. Login successfully
2. Open browser DevTools → Application/Storage → Local Storage
3. Look for `authToken` - this contains your JWT
4. The token expires after 60 minutes
5. After expiration, accessing protected routes should redirect to login

#### 4. API Integration Test
1. Login to get a valid token
2. Open `/swagger` in a new tab
3. Use the "Authorize" button with format: `Bearer your-jwt-token`
4. Test protected API endpoints

### CoreUI Features Showcase

The application demonstrates CoreUI React components with Harmoni branding:

- **Login Form**: Custom styled with CoreUI form components
- **Navigation**: CoreUI sidebar with Harmoni color scheme
- **Dashboard**: CoreUI cards and layout components
- **Demo User Display**: CoreUI accordion for credentials showcase
- **Responsive Design**: Mobile-friendly CoreUI grid system

---

## 🛠️ Troubleshooting

### Common Issues and Solutions

#### Database Connection Issues

**Problem**: Cannot connect to PostgreSQL
```bash
# Check if PostgreSQL is running
docker-compose ps postgres
# OR for local installation
pg_isready -U postgres
```

**Solution**: 
- For Docker: `docker-compose restart postgres`
- For local: Check PostgreSQL service is running
- Verify connection string in appsettings.json

#### Frontend Build Errors

**Problem**: npm install fails or build errors
```bash
# Clear cache and reinstall
cd src/HarmoniHSE360.Web/ClientApp
rm -rf node_modules package-lock.json
npm install
```

**Problem**: Vite dev server won't start
```bash
# Check if port 5173 is available
netstat -an | grep 5173
# OR try different port
npm run dev -- --port 3000
```

#### CORS Issues

**Problem**: API requests blocked by CORS policy

**Solution**: 
- Ensure Vite dev server is running on port 5173
- Check `Program.cs` has correct CORS configuration
- For local development, API should proxy to `http://localhost:5173`

#### JWT Authentication Issues

**Problem**: Token validation fails
- Verify JWT secret key is at least 32 characters
- Check token hasn't expired (60-minute default)
- Ensure Bearer prefix is included: `Bearer your-token`

**Problem**: Login fails with valid credentials
- Check database seeding completed successfully
- Verify password exactly matches (case-sensitive)
- Check API logs for detailed error messages

#### Docker Issues

**Problem**: Services won't start
```bash
# Check service logs
docker-compose logs app
docker-compose logs postgres

# Restart all services
docker-compose down
docker-compose up -d
```

**Problem**: Port conflicts
- Ensure ports 8080 and 5432 are not in use
- Modify docker-compose.yml to use different ports if needed

### Performance Issues

**Slow initial load**: 
- Docker builds can take 5-10 minutes initially
- Subsequent starts are much faster due to layer caching

**Memory usage**: 
- Ensure Docker Desktop has at least 4GB RAM allocated
- React dev server uses hot module replacement for faster development

---

## 🔍 Development Tools

### Useful Commands

```bash
# Check application logs
docker-compose logs -f app

# Access database directly
docker-compose exec postgres psql -U postgres -d HarmoniHSE360

# Check Redis cache
docker-compose exec redis redis-cli ping

# Restart specific service
docker-compose restart app

# View service resource usage
docker stats
```

### Browser DevTools Tips

- **Network Tab**: Monitor API calls and response times
- **Console Tab**: Check for JavaScript errors
- **Application Tab**: Inspect localStorage for JWT tokens
- **Redux DevTools**: Install extension to monitor state changes

### API Testing

- **Swagger UI**: Available at `/swagger` endpoint
- **Postman**: Import API collection for comprehensive testing
- **cURL Examples**: Use browser DevTools to copy as cURL

---

## 📊 Architecture Overview

### Project Structure
```
src/
├── HarmoniHSE360.Domain/          # Domain entities and business logic
├── HarmoniHSE360.Application/     # Use cases and interfaces
├── HarmoniHSE360.Infrastructure/  # Data access and external services
└── HarmoniHSE360.Web/            # API controllers and React SPA
    ├── Controllers/              # REST API endpoints
    ├── ClientApp/               # React application
    │   ├── src/components/      # React components
    │   ├── src/features/        # Redux slices and API
    │   ├── src/pages/          # Route components
    │   └── src/layouts/        # Layout components
    └── Program.cs              # Application entry point
```

### Key Technologies
- **Clean Architecture**: Separation of concerns with clear dependencies
- **CQRS with MediatR**: Command/Query separation for business logic
- **Entity Framework Core**: Database ORM with migrations
- **JWT Authentication**: Stateless authentication with role-based access
- **React + Redux Toolkit**: Modern frontend with state management
- **CoreUI React**: Professional UI component library
- **Vite**: Fast development server with hot module replacement

---

## 🎯 Next Steps

After successfully running and testing authentication:

1. **Explore the Codebase**:
   - Review Domain entities in `src/HarmoniHSE360.Domain/`
   - Check authentication implementation in `Application/Features/Authentication/`
   - Examine React components in `ClientApp/src/`

2. **Extend Functionality**:
   - Add new modules following the existing patterns
   - Implement role-based authorization for specific features
   - Add new React pages and components

3. **Production Preparation**:
   - Change demo user passwords
   - Configure production JWT secrets
   - Set up monitoring and logging
   - Configure HTTPS and security headers

4. **Database Management**:
   - Add new Entity Framework migrations as needed
   - Implement data seeding for production environment
   - Consider database backup strategies

---

## 📞 Support

For issues or questions:

1. **Check this guide first** - Most common issues are covered above
2. **Review application logs** - Use `docker-compose logs app` for detailed errors
3. **Verify prerequisites** - Ensure all required software is installed and running
4. **Test with demo users** - Use provided seeded credentials to isolate issues
5. **Check database connectivity** - Verify PostgreSQL is accessible and migrations completed

The application is production-ready for authentication and basic navigation. Additional HSE-specific modules can be built following the established patterns and architecture.