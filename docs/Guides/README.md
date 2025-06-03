# HarmoniHSE360 Guides

This folder contains all operational guides for the HarmoniHSE360 system.

## 📚 Available Guides

### 🔐 Authentication & Security
#### 1. [Seeded Users Guide](./Seeded_Users.md)
Quick reference for testing and development credentials:
- Default login credentials for all user roles
- Password information
- Quick access examples

#### 2. [Authentication Guide](./Authentication_Guide.md)
Complete authentication system documentation:
- JWT implementation details
- API endpoints and usage
- Security features and configuration
- Production deployment checklist

### 🛠️ System Operations
#### 3. [Getting Started Guide](./Getting_Started_Guide.md)
Complete guide for running and testing the HarmoniHSE360 application:
- Local development setup
- Docker deployment options
- Authentication testing scenarios
- Troubleshooting common issues

#### 4. [Database Access Guide](./Database_Access_Guide.md)
Comprehensive guide for database access:
- pgAdmin setup and configuration
- Connection credentials and methods
- Useful SQL queries
- Troubleshooting database issues

## 🚀 Quick Links

### Application URLs (Local Development)
- **React Frontend**: http://localhost:5173
- **API Backend**: http://localhost:5000
- **Database**: localhost:5432

### Application URLs (Docker)
- **Full Application**: http://localhost:8080
- **Database**: localhost:5432

### Quick Commands
```bash
# Docker deployment
docker-compose up -d

# Local development (React)
cd src/HarmoniHSE360.Web/ClientApp && npm run dev

# Local development (.NET)
cd src/HarmoniHSE360.Web && dotnet run

# Check service status
docker-compose ps

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

## 📝 Documentation Structure

```
docs/
├── Guides/                         # Operational guides (you are here)
│   ├── Getting_Started_Guide.md    # Complete setup and testing guide
│   ├── Authentication_Guide.md     # Complete authentication documentation
│   ├── Seeded_Users.md             # Quick user credentials reference
│   ├── Database_Access_Guide.md    # Database access and management
│   └── README.md                   # This file
├── Architecture/                   # Technical architecture documents
│   ├── Authentication_Strategy.md
│   └── Authentication_Implementation_Summary.md
└── Markdown/                       # Project requirements and specifications
    ├── Business_Analyst_Harmoni_HSE_360_REQUIREMENTS.md
    ├── Project_Manager_Harmoni_HSE_360_EPICS.md
    ├── Solution_Architect_Comprehensive_Development_Practices_V2.md
    └── HarmoniHSE360_Development_Tracking.md
```