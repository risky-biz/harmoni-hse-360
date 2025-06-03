# HarmoniHSE360

![HarmoniHSE360 Logo](Documents/Assets/Images/Harmoni_HSE_360_Logo.png)

## Enterprise Health, Safety, and Environment Management System

HarmoniHSE360 is a comprehensive cloud-based HSE management system designed for British School Jakarta. Built using a Modular Monolith architecture with Clean Architecture principles, it provides a unified platform for managing all health, safety, and environmental activities across the campus.

## 🏗️ Architecture

- **Modular Monolith**: Clear module boundaries with high cohesion and low coupling
- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and API layers
- **Technology Stack**:
  - Backend: .NET 8
  - Frontend: Blazor Server with Ant Design Blazor
  - Database: PostgreSQL with TimescaleDB
  - Cache: Redis
  - Container: Docker
  - Orchestration: Kubernetes

## 📋 Features (Planned)

### Phase 1: Foundation (Months 1-3)
- User Management and Access Control
- Multi-Language Support (English/Bahasa Indonesia)
- Integration Hub and API Gateway

### Phase 2: Core HSE Functions (Months 4-6)
- Incident Management System
- Hazard Reporting and Risk Assessment
- Document Management System
- Mobile Application (Basic)

### Phase 3: Advanced Features (Months 7-9)
- Compliance and Audit Management
- Permit-to-Work System
- Training and Certification Management
- Analytics and Intelligence Platform

### Phase 4: Specialized Systems (Months 10-12)
- Environmental Monitoring
- Advanced Mobile Features
- System Optimization

## 🚀 Getting Started

### Prerequisites
- Docker Desktop (recommended) OR
- .NET 8 SDK + Node.js 20+ (for local development)

### Quick Start with Docker (Recommended)

#### Development Environment
```bash
# Clone the repository
git clone https://github.com/your-org/harmoni-hse-360.git
cd harmoni-hse-360

# Option 1: Simple setup (recommended)
# Start only database services
docker compose -f docker-compose.dev-simple.yml up -d

# Then run backend and frontend locally - see Docker Guide

# Option 2: Full Docker development
docker compose -f docker-compose.dev.yml up

# Access the application
# Frontend: http://localhost:5173
# API: http://localhost:5000
# pgAdmin: http://localhost:5050
```

#### Production Environment
```bash
# Copy and configure environment
cp .env.example .env
# Edit .env with your values

# Start production environment
docker compose up -d

# Access at http://localhost:8080
```

See [Docker Guide](docs/Guides/Docker_Guide.md) for detailed instructions.

### Local Development Setup (Alternative)

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/harmoni-hse-360.git
   cd harmoni-hse-360
   ```

2. **Start database with Docker**
   ```bash
   docker compose up -d postgres redis
   ```

3. **Backend Setup**
   ```bash
   cd src/HarmoniHSE360.Web
   dotnet restore
   dotnet run
   ```

4. **Frontend Setup** (new terminal)
   ```bash
   cd src/HarmoniHSE360.Web/ClientApp
   npm install
   npm run dev
   ```

6. **Run the Blazor Server app**
   ```bash
   cd src/Web/HarmoniHSE360.BlazorServer
   dotnet run
   ```

### Docker Development

Run the entire stack with Docker Compose:
```bash
docker-compose up -d
```

Access the applications:
- **API**: http://localhost:5000
- **Blazor Web**: http://localhost:5001  
- **API Documentation**: http://localhost:5000/swagger
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379
- **PgAdmin**: http://localhost:5050

### 🔑 Authentication

The system comes with pre-seeded users for immediate testing:

**Quick Access**: All users use password `HarmoniHSE360!`

- **Admin**: `admin@harmonihse360.com`
- **HSE Manager**: `hse.manager@harmonihse360.com`  
- **Engineer**: `engineer@harmonihse360.com`

👉 **See [Seeded Users Guide](./docs/Guides/Seeded_Users.md) for complete user list**

## 📁 Project Structure

```
HarmoniHSE360/
├── src/
│   ├── BuildingBlocks/          # Shared kernel
│   ├── Modules/                 # Business modules
│   ├── API/                     # API host
│   ├── Web/                     # Blazor Server UI
│   └── Mobile/                  # Mobile apps
├── tests/                       # Test projects
├── docs/                        # Documentation
└── scripts/                     # Utility scripts
```

## 🧪 Testing

Run all tests:
```bash
dotnet test
```

Run with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 Documentation

### 🔐 Authentication & Users
- **[Seeded Users Guide](docs/Guides/Seeded_Users.md)** - Login credentials for testing
- **[Authentication Guide](docs/Guides/Authentication_Guide.md)** - Complete authentication documentation

### 📋 Business & Technical
- [Business Requirements](docs/Markdown/Business_Analyst_Harmoni_HSE_360_REQUIREMENTS.md)
- [Technical Architecture](docs/Markdown/Solution_Architect_Comprehensive_Development_Practices_for_Harmoni_HSE_360_Application.md)
- [Project Epics](docs/Markdown/Project_Manager_Harmoni_HSE_360_EPICS.md)
- [Development Tracking](docs/Markdown/HarmoniHSE360_Development_Tracking.md)

### 🛠️ Development Guides
- [Database Access Guide](docs/Guides/Database_Access_Guide.md)
- [Verification Guide](docs/Guides/Verification_Guide.md)

## 📜 License

This project is proprietary software for British School Jakarta.

## 👥 Team

- Business Analyst
- Solution Architect
- Project Manager
- Head of Design
- Senior Full Stack Engineers

---

Built with ❤️ for British School Jakarta