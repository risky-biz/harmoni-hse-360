# HarmoniHSE360 Development Tracking Document

## Project Overview

**Project Name:** HarmoniHSE360  
**Client:** British School Jakarta (BSJ)  
**Project Type:** Enterprise HSE (Health, Safety, and Environment) Management System  
**Duration:** 12 months (4 phases)  
**Start Date:** TBD  

## Executive Summary

HarmoniHSE360 is a comprehensive cloud-based HSE management system designed to replace manual processes at British School Jakarta with a unified digital platform. The system will support all HSE activities across the campus while maintaining compliance with Indonesian regulations and international school standards.

### Key Objectives
- ✅ Reduce incident reporting time by 50%
- ✅ Increase proactive hazard identification by 30%
- ✅ Achieve 90% user adoption across all departments
- ✅ Maintain 95%+ regulatory compliance
- ✅ Support bilingual operations (English/Bahasa Indonesia)

## Technical Architecture

### Architecture Pattern: Modular Monolith with Clean Architecture

We will implement a **Modular Monolith** architecture pattern combined with **Clean Architecture** principles. This approach provides:
- Clear module boundaries with high cohesion and low coupling
- Independent development and testing of modules
- Future option to extract modules into microservices if needed
- Simplified deployment and operations compared to microservices
- Better performance with in-process communication between modules

### Technology Stack
- **Backend:** .NET 8 with Clean Architecture ✅ **IMPLEMENTED**
- **Frontend:** React 18 + TypeScript with CoreUI React ✅ **IMPLEMENTED** *(Changed from Blazor Server)*
- **Mobile:** Planned for .NET MAUI Blazor Hybrid (iOS/Android)
- **Database:** PostgreSQL ✅ **IMPLEMENTED** *(TimescaleDB extension not yet configured)*
- **Build Tool:** Vite for frontend ✅ **IMPLEMENTED**
- **Real-time:** SignalR ✅ **IMPLEMENTED**
- **State Management:** Redux Toolkit + React Query ✅ **IMPLEMENTED**
- **Forms:** React Hook Form + Yup validation ✅ **IMPLEMENTED**
- **Container:** Docker with multi-stage builds ✅ **CONFIGURED**
- **Orchestration:** Kubernetes on Biznet Gio (Indonesian cloud)
- **CI/CD:** GitHub Actions
- **IDE:** JetBrains Rider

### Key Technical Decisions
- **Architecture:** Clean Architecture (simplified from Modular Monolith) ✅ **IMPLEMENTED**
- **Module Communication:** In-process messaging with MediatR ✅ **IMPLEMENTED**
- **Frontend Framework:** React + TypeScript (changed from Blazor) ✅ **IMPLEMENTED**
- **UI Library:** CoreUI React (changed from Ant Design Blazor) ✅ **IMPLEMENTED**
- **API Documentation:** OpenAPI with Swagger ✅ **IMPLEMENTED**
- **Authentication:** JWT-based (simplified from SAML 2.0) ✅ **IMPLEMENTED**
- **Database ORM:** Entity Framework Core ✅ **IMPLEMENTED**
- **Logging:** Serilog with structured logging ✅ **IMPLEMENTED**
- **Real-time Communication:** SignalR ✅ **IMPLEMENTED**
- **Module Boundaries:** Vertical slices by business capability
- **Real-time Visualization:** ApexCharts for React (planned)
- **PDF Generation:** QuestPDF (planned)
- **Message Queue:** Redis with AMQP support (planned)
- **Monitoring:** Time-series data with continuous aggregates (planned)

### Performance Requirements
- 99.9% uptime availability
- <3 second page load (95th percentile)
- <2 second mobile response time
- Support 500 concurrent users
- Handle 10,000 transactions per hour
- Process 1,000 photos per day
- Stream 100 IoT sensor updates per second

## Development Phases and Epics

### Phase 1: Foundation (Months 1-3)
**Status:** 🚧 In Progress (Started June 2025)

#### Epic 10: User Management and Access Control System
- [x] JWT-based authentication system
- [x] Role-Based Access Control (RBAC) with User/Role entities
- [x] Basic user profile management
- [x] Token refresh and validation endpoints
- [x] User lifecycle management (activate/deactivate)
- [x] Audit logging infrastructure (IAuditableEntity)
- [ ] Single Sign-On with Active Directory
- [ ] Multi-factor authentication
- [ ] Delegation capabilities

#### Epic 11: Multi-Language Support and Localization System
- [ ] English and Bahasa Indonesia support
- [ ] Translation memory system
- [ ] Content synchronization
- [ ] Cultural adaptation features
- [ ] Right-to-left language support (future)
- [ ] In-context translation tools

#### Epic 12: Integration Hub and API Gateway
- [x] RESTful API development (AuthController implemented)
- [x] OpenAPI documentation with Swagger
- [x] Basic API structure with controllers
- [x] CORS configuration
- [x] Health checks endpoint
- [ ] Enterprise Service Bus
- [ ] System integrations (HR, Finance, LMS, SIS, BMS)
- [ ] Event streaming platform
- [ ] Developer portal with documentation
- [ ] API versioning and security

### Phase 2: Core HSE Functions (Months 4-6)
**Status:** 🚧 Early Development (Started June 2025)

#### Epic 1: Incident Management System
**Actual Progress:** ~10% complete ⚠️ **CRITICAL GAPS IDENTIFIED**

**✅ Foundation Complete:**
- [x] Core incident domain model with business logic
- [x] Incident severity and status tracking  
- [x] GeoLocation support for incidents
- [x] Incident attachments system
- [x] Involved persons tracking
- [x] Corrective Action (CAPA) workflow foundation
- [x] Domain events for incident lifecycle
- [x] Basic incident repository pattern
- [x] SignalR hubs for real-time notifications

**❌ Critical Missing Components:**
- [ ] **IncidentController and API endpoints** *(No backend API)*
- [ ] **Incident reporting web form** *(Only placeholder "Coming Soon")*
- [ ] **File upload functionality** *(No implementation)*
- [ ] **Navigation/routing fixes** *(Hash routing issues)*
- [ ] Multi-channel incident reporting
- [ ] Automated notifications and escalation
- [ ] Investigation management tools
- [ ] Regulatory reporting (Indonesian compliance)
- [ ] Mobile and offline support

**🚨 Blocker Issues:**
- Users cannot report incidents (no working form/API)
- Navigation redirects to placeholder pages
- Frontend-backend integration incomplete

#### Epic 2: Hazard Reporting and Risk Assessment System
- [ ] Photo-first hazard reporting
- [ ] Risk assessment tools (JSA, HIRA)
- [ ] Dynamic risk register
- [ ] Campus risk visualization (heat maps)
- [ ] QR code location scanning
- [ ] Gamification features

#### Epic 4: Document Management System for HSE
- [ ] Version control with check-in/check-out
- [ ] Approval workflows
- [ ] Multi-language document synchronization
- [ ] Distribution tracking and acknowledgment
- [ ] Full-text search with OCR
- [ ] Mobile offline access

#### Epic 9: Mobile Application Platform (Basic Features)
- [ ] Native iOS application
- [ ] Native Android application
- [ ] Offline data storage
- [ ] Basic incident reporting
- [ ] Document viewing
- [ ] Push notifications

### Phase 3: Advanced Features (Months 7-9)
**Status:** ⏳ Not Started

#### Epic 3: Compliance and Audit Management System
- [ ] Regulatory intelligence engine
- [ ] Multi-standard framework (COBIS, BSO, CIS)
- [ ] Mobile audit execution
- [ ] Finding and non-conformance management
- [ ] Compliance dashboard
- [ ] Automated regulatory reporting

#### Epic 5: Permit-to-Work System
- [ ] Digital permit templates
- [ ] Multi-stage approval workflows
- [ ] Conflict detection engine
- [ ] School calendar integration
- [ ] Contractor management
- [ ] QR code verification

#### Epic 6: Training and Certification Management System
- [ ] Role-based competency matrices
- [ ] Automated training assignment
- [ ] Multiple delivery methods support
- [ ] Digital certificate generation
- [ ] Training effectiveness tracking
- [ ] External certification management

#### Epic 8: Analytics and HSE Intelligence Platform
- [ ] Real-time executive dashboards
- [ ] Predictive analytics with ML
- [ ] Advanced root cause analysis
- [ ] Automated report generation
- [ ] Internal and external benchmarking
- [ ] API for custom analytics

### Phase 4: Specialized Systems (Months 10-12)
**Status:** ⏳ Not Started

#### Epic 7: Environmental Monitoring and Measurement System
- [ ] Air quality monitoring (indoor/outdoor)
- [ ] Noise level management
- [ ] Water quality testing
- [ ] Energy and waste management
- [ ] IoT sensor integration
- [ ] Sustainability education features

#### Epic 9: Mobile Application Platform (Advanced Features)
- [ ] Biometric authentication
- [ ] Advanced offline capabilities
- [ ] Voice input support
- [ ] Camera integration with annotation
- [ ] Location-based services
- [ ] Performance optimization

## Integration Requirements

### Internal Systems
- **HR System:** Employee data, organization structure (Bi-directional, Real-time)
- **Financial System:** Training costs, incident costs (Outbound, Daily batch)
- **Learning Management System:** Training enrollment and completion (Bi-directional, Real-time)
- **Student Information System:** Student data, emergency contacts (Inbound, Real-time)
- **Building Management System:** Environmental data, access logs (Inbound, Streaming)

### External Integrations
- WhatsApp Business API
- SMS Gateway
- Email Server (SMTP)
- Push Notification Services
- Indonesian Government Reporting Portals

## Compliance Requirements

### Indonesian Regulations
- PP No. 50 Tahun 2012 (SMK3 Implementation)
- UU No. 18/2008 (Waste Management)
- UU No. 24/2009 (Language Requirements)
- 2x24 hour incident reporting requirement
- P2K3 committee support
- Bilingual documentation (Bahasa Indonesia/English)

### International Standards
- COBIS (Council of British International Schools)
- BSO (British Schools Overseas)
- CIS (Council of International Schools)
- GDPR compliance for EU citizens
- ISO standards for document control

## User Categories and Roles

1. **System Administrators** - Full system configuration
2. **HSE Managers** - Comprehensive HSE functionality
3. **Department Heads** - Department-specific features
4. **Employees/Teachers** - Standard HSE participation
5. **Contractors** - Limited access for relevant activities
6. **Students** - Restricted access for hazard reporting
7. **Parents** - View-only for incident notifications

## Branding Guidelines

### Visual Identity
- **Brand Name:** HarmoniHSE360
- **Logo:** Circular badge with teal-to-green gradient containing white shield with checkmark
- **Primary Colors:** 
  - Teal (#008B8B range)
  - Green gradient
  - Bright cyan for "360"
- **Typography:** Modern sans-serif, clean and professional

## Success Metrics

### System Performance
- [ ] Page load time <3 seconds
- [ ] Mobile app response <2 seconds
- [ ] 99.9% uptime availability
- [ ] <1% transaction failure rate

### Business Outcomes
- [ ] 50% reduction in incident reporting time
- [ ] 30% increase in hazard identification
- [ ] 95% regulatory compliance rate
- [ ] 90% user adoption rate
- [ ] 70% reduction in paper-based processes

### User Satisfaction
- [ ] 4.5/5 user satisfaction score
- [ ] <5 minutes average task completion
- [ ] 80% mobile app usage
- [ ] <2 hours training per user required

## Development Environment Setup

### Local Development Stack
```yaml
version: '3.8'
services:
  timescaledb:
    image: timescale/timescaledb-ha:pg17
    environment:
      - POSTGRES_PASSWORD=dev_password
      - TS_TUNE_MEMORY=4GB
    ports:
      - "5432:5432"
    volumes:
      - ./sql/init:/docker-entrypoint-initdb.d/
```

### Current Project Structure - Clean Architecture (Implemented)
```
HarmoniHSE360/
├── src/
│   ├── HarmoniHSE360.Domain/                    ✅ Core business entities and logic
│   │   ├── Common/                              # Base classes and interfaces
│   │   ├── Entities/                           # User, Incident, Role, etc.
│   │   ├── Events/                             # Domain events
│   │   ├── Interfaces/                         # Repository interfaces
│   │   └── ValueObjects/                       # GeoLocation, etc.
│   │
│   ├── HarmoniHSE360.Application/              ✅ Use cases and application logic
│   │   ├── Common/                             # Shared application concerns
│   │   │   ├── Behaviors/                      # MediatR behaviors
│   │   │   └── Interfaces/                     # Application interfaces
│   │   └── Features/                           # Feature-based organization
│   │       ├── Authentication/                 # Login commands/queries
│   │       └── Incidents/                      # Incident management
│   │
│   ├── HarmoniHSE360.Infrastructure/           ✅ Data access and external services
│   │   ├── Persistence/                        # EF Core DbContext & repos
│   │   ├── Services/                          # JWT, Password hashing, etc.
│   │   └── Migrations/                        # Database migrations
│   │
│   └── HarmoniHSE360.Web/                     ✅ Main web application
│       ├── Controllers/                        # API controllers
│       ├── Hubs/                              # SignalR hubs
│       ├── Middleware/                         # Custom middleware
│       └── ClientApp/                         # React frontend
│           ├── src/
│           │   ├── components/                # Reusable UI components
│           │   ├── features/                  # Feature-specific code
│           │   ├── layouts/                   # Page layouts
│           │   ├── pages/                     # Route pages
│           │   ├── store/                     # Redux store
│           │   └── styles/                    # SCSS styling
│           └── package.json                   # Frontend dependencies
```

### Planned Evolution to Modular Monolith
*Future structure when scaling requirements demand it*

### Current Implementation Status (June 2025)

#### ✅ **Completed Infrastructure**
- **Clean Architecture Foundation:** Domain, Application, Infrastructure, Web layers
- **Database:** PostgreSQL with Entity Framework Core, initial migration completed
- **Authentication:** JWT-based auth with login/logout/refresh endpoints  
- **API:** RESTful endpoints with OpenAPI/Swagger documentation
- **Frontend:** React + TypeScript SPA with CoreUI components
- **Real-time:** SignalR hubs for incidents and notifications
- **Logging:** Serilog with structured logging
- **Containerization:** Docker configuration ready

#### ✅ **Core Domain Models**
- **User Management:** User, Role entities with RBAC
- **Incident Management:** Complete incident lifecycle with attachments, involved persons, corrective actions
- **Domain Events:** Event-driven architecture foundation
- **Audit Trail:** IAuditableEntity for change tracking

#### 🚧 **In Development**
- **Frontend UI:** Basic layouts and routing implemented, pages under development
- **Incident API:** Domain models ready, controller and endpoints pending
- **Data Seeding:** Basic user seeding implemented

#### ⏳ **Next Priorities (URGENT)**
1. **🔥 Critical Blockers** *(Must fix before any feature work)*:
   - Create IncidentController with basic CRUD endpoints
   - Build functional incident reporting form (replace placeholder)
   - Fix navigation routing issues (remove hash routing)
   - Implement file upload for incident attachments

2. **📋 Form Implementation Requirements** *(Per FRQ-INC-001)*:
   - Dynamic incident categorization (student/staff/property/environmental)
   - Location capture (GPS + manual campus building/room selection)
   - Photo/video evidence upload (min 5 photos, 2-min video support)
   - Auto-save functionality every 30 seconds
   - Form validation and error handling

3. **🔗 Integration Tasks**:
   - Connect React frontend to .NET API endpoints
   - Implement proper API error handling
   - Set up automated testing framework

### Module Communication Patterns (Current)
- **In-Process Events:** MediatR for synchronous communication ✅ **IMPLEMENTED**
- **Domain Events:** Event sourcing pattern for business logic ✅ **IMPLEMENTED**
- **Repository Pattern:** Interface-based data access ✅ **IMPLEMENTED**
- **CQRS Pattern:** Command/Query separation ✅ **IMPLEMENTED**

## Risk Mitigation Strategies

### Technical Risks
- Progressive rollout strategy
- Maintain fallback systems
- Comprehensive testing coverage
- Scalability planning

### Organizational Risks
- Executive sponsorship secured
- Change management program
- Comprehensive training plan
- Early wins celebration

### Compliance Risks
- Regular regulatory reviews
- Automated compliance checking
- Complete audit trails
- Legal counsel engagement

## Deployment Strategy

### Kubernetes on Biznet Gio
- Single region deployment (Jakarta)
- Multi-AZ for high availability
- Data residency compliance (PP 71/2019)
- Cost optimization with spot instances
- Expected cost: $500-800/month

### Container Strategy
- Multi-stage Docker builds
- Alpine Linux base images
- Non-root user execution
- Horizontal pod autoscaling
- Redis backplane for SignalR

## Next Steps

1. **Immediate Actions (Phase 1 Continuation):**
   - [x] ~~Finalize project timeline and start date~~
   - [x] ~~Set up development environment~~
   - [x] ~~Create GitHub repository structure~~
   - [x] ~~Initialize .NET 8 solution with Clean Architecture~~
   - [x] ~~Set up basic CI/CD pipeline with GitHub Actions~~
   - [ ] Complete incident management API endpoints
   - [ ] Implement incident UI forms and list views
   - [ ] Add file upload functionality
   - [ ] Set up automated testing framework

2. **Phase 1 Continuation (July 2025):**
   - [ ] Complete Epic 10: User Management System (60% complete)
   - [ ] Start Epic 11: Localization Framework
   - [ ] Complete Epic 12: Integration Hub basic features (40% complete)
   - [ ] Finalize incident management module (Epic 1 from Phase 2)

3. **Documentation and Quality:**
   - [x] ~~Set up API documentation with Swagger~~
   - [x] ~~Initialize user documentation repository~~
   - [ ] Create Architecture Decision Records (ADRs)
   - [ ] Establish comprehensive testing strategy
   - [ ] Set up performance monitoring
   - [ ] Create deployment documentation

4. **Technical Debt and Improvements:**
   - [ ] Optimize frontend bundle size
   - [ ] Implement proper error boundaries
   - [ ] Add comprehensive input validation
   - [ ] Set up database backup strategy
   - [ ] Configure TimescaleDB for time-series data

---

**Document Version:** 2.0  
**Last Updated:** June 3, 2025  
**Status:** Active Development - Phase 1 in Progress  
**Next Review:** Phase 1 Completion (July 2025)

### Recent Changes (v2.0)
- Updated technology stack to reflect React frontend (changed from Blazor)
- Added current implementation status with completed features
- Updated Phase 1 and Phase 2 epic progress
- Documented architecture decisions and technical stack changes
- Added immediate next steps and priorities