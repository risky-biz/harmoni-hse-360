# Harmoni360 Authentication Guide

## Overview

Harmoni360 uses JWT-based authentication with role-based access control (RBAC). The system comes pre-seeded with default users for testing and initial setup.

## Quick Start

### Default Password
All seeded users use the default password: **`Harmoni360!`**

### Authentication Endpoints

- **Login**: `POST /api/auth/login`
- **Register**: `POST /api/auth/register`
- **Current User**: `GET /api/auth/me` (requires authentication)

## Seeded Users

The system comes with pre-configured users representing different roles and access levels:

### 🔑 System Administrator
- **Email**: `admin@harmoni360.com`
- **Password**: `Harmoni360!`
- **Role**: System Administrator
- **Access**: Full system access, all modules and configurations
- **Department**: IT
- **Position**: System Administrator

### 👤 HSE Manager
- **Email**: `hse.manager@harmoni360.com`
- **Password**: `Harmoni360!`
- **Role**: HSE Manager
- **Access**: HSE module management, reporting, compliance oversight
- **Department**: Health, Safety, Security, & Environment
- **Position**: HSE Manager
- **Certifications**: NEBOSH General Certificate, IOSH Managing Safely, ISO 14001 Lead Auditor

### 🏢 Department Heads

#### Engineering Department Head
- **Email**: `dept.head.engineering@harmoni360.com`
- **Password**: `Harmoni360!`
- **Role**: Department Head
- **Access**: Engineering department oversight, safety management
- **Department**: Engineering
- **Position**: Department Head
- **Certifications**: Professional Engineer (PE), Certified Safety Professional (CSP)

#### Operations Department Head
- **Email**: `dept.head.operations@harmoni360.com`
- **Password**: `Harmoni360!`
- **Role**: Department Head
- **Access**: Operations department oversight, facility operations
- **Department**: Operations
- **Position**: Department Head
- **Certifications**: Operations Management Certificate, OSHA 30-Hour General Industry

### 👷 Employees

#### Safety Officer
- **Email**: `safety.officer@harmoni360.com`
- **Password**: `Harmoni360!`
- **Role**: Employee
- **Access**: Incident reporting, safety protocols, compliance monitoring
- **Department**: Health, Safety, Security, & Environment
- **Position**: Safety Officer
- **Certifications**: OSHA 30-Hour Construction, First Aid/CPR Certified, Incident Investigation

#### Senior Engineer
- **Email**: `engineer@harmoni360.com`
- **Password**: `Harmoni360!`
- **Role**: Employee
- **Access**: Engineering processes, risk assessment, safety protocols
- **Department**: Engineering
- **Position**: Senior Engineer
- **Certifications**: Process Safety Management, Hazard Analysis (HAZOP)

#### Senior Technician
- **Email**: `technician@harmoni360.com`
- **Password**: `Harmoni360!`
- **Role**: Employee
- **Access**: Equipment maintenance, safety procedures, operational tasks
- **Department**: Operations
- **Position**: Senior Technician
- **Certifications**: OSHA 10-Hour General Industry, Confined Space Entry, Lockout/Tagout

### 🏗️ External Users

#### Contractor
- **Email**: `contractor@externalcompany.com`
- **Password**: `Harmoni360!`
- **Role**: Contractor
- **Access**: Limited access for contractor-specific tasks and safety compliance
- **Department**: External Contractor
- **Position**: Electrical Contractor
- **Certifications**: Licensed Electrician, OSHA 10-Hour Construction, Arc Flash Safety

#### Student
- **Email**: `student@university.edu`
- **Password**: `Harmoni360!`
- **Role**: Student
- **Access**: Educational access, research data, learning materials
- **Department**: Student Services
- **Position**: Graduate Student

#### Parent/Guardian
- **Email**: `parent@email.com`
- **Password**: `Harmoni360!`
- **Role**: Parent
- **Access**: Safety notifications, incident reports affecting students
- **Department**: External
- **Position**: Parent/Guardian

## Authentication Flow

### 1. Login Request
```json
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@harmoni360.com",
  "password": "Harmoni360!"
}
```

### 2. Successful Response
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "expiresAt": "2024-01-01T13:00:00Z",
  "requiresTwoFactor": false
}
```

### 3. Using the Token
Include the access token in the Authorization header:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## JWT Token Claims

The JWT token includes the following claims:

- **sub** (NameIdentifier): User ID
- **email**: User email address
- **name**: Full name (First + Last)
- **given_name**: First name
- **family_name**: Last name
- **role**: User roles (multiple values possible)
- **permission**: User permissions (multiple values possible)
- **department**: User department
- **position**: User position
- **authentication_source**: Local/External

## Role-Based Permissions

### System Administrator
- Full access to all modules
- User management and role assignment
- System configuration and settings
- All CRUD operations

### HSE Manager
- HSE module management
- Incident reporting and investigation
- Compliance monitoring and reporting
- Risk assessment oversight
- Training management

### Department Head
- Department-specific oversight
- Employee safety management
- Incident reporting for their department
- Training assignment and tracking

### Employee
- Basic incident reporting
- Personal safety records
- Training completion
- Equipment checkout/maintenance logging

### Contractor
- Limited incident reporting
- Contractor-specific safety protocols
- Equipment usage logging
- Compliance documentation

### Student
- Educational content access
- Research data (read-only)
- Safety training materials
- Limited incident reporting

### Parent
- Child-related safety notifications
- Incident report access (child-related)
- Safety communication portal

## Security Features

### Password Security
- **Algorithm**: PBKDF2 with SHA256
- **Iterations**: 100,000
- **Salt**: 32-byte random salt per password
- **Hash Size**: 32 bytes

### JWT Security
- **Algorithm**: HS256 (HMAC with SHA-256)
- **Token Lifetime**: 1 hour (configurable)
- **Refresh Token**: 7 days (configurable)
- **Issuer Validation**: Enabled
- **Audience Validation**: Enabled

## Development Setup

### Using Docker Compose
1. Run `docker-compose up -d`
2. Migrations and seeding happen automatically
3. API available at `http://localhost:5000`
4. Blazor UI available at `http://localhost:5001`

### Local Development
1. Ensure PostgreSQL is running
2. Update connection string in `appsettings.Development.json`
3. Run `dotnet run` from the API project
4. Migrations and seeding happen automatically

## Testing Authentication

### Using Swagger UI
1. Navigate to `http://localhost:5000/swagger`
2. Use the `/api/auth/login` endpoint
3. Copy the returned access token
4. Click "Authorize" button in Swagger UI
5. Enter: `Bearer {your-access-token}`

### Using Blazor UI
1. Navigate to `http://localhost:5001`
2. Click "Login" 
3. Use any seeded user credentials
4. Explore role-based features

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKeyHereWhichShouldBeAtLeast32CharactersLongForProduction",
    "Issuer": "Harmoni360",
    "Audience": "Harmoni360Users",
    "ExpirationInHours": 1
  }
}
```

### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=timescaledb;Database=harmoni360_dev;Username=harmoni360;Password=Harmoni360!2024;Port=5432"
  }
}
```

## Troubleshooting

### Common Issues

1. **"Invalid email or password"**
   - Verify you're using the correct email from the seeded users list
   - Ensure password is exactly: `Harmoni360!`
   - Check that seeding completed successfully in API logs

2. **"User account is deactivated"**
   - All seeded users are active by default
   - Check if user was manually deactivated

3. **Token validation errors**
   - Verify JWT settings match between issuer and validator
   - Check token hasn't expired (1-hour default)
   - Ensure Bearer prefix is included in Authorization header

4. **Database connection issues**
   - Verify PostgreSQL is running
   - Check connection string matches your database setup
   - Ensure database user has proper permissions

### Logs to Check
- API startup logs for migration and seeding status
- Authentication logs for login attempts
- Database logs for connection issues

## Production Considerations

⚠️ **Important**: Before deploying to production:

1. **Change Default Passwords**: All seeded users must have their passwords changed
2. **Update JWT Secret**: Use a strong, unique secret key
3. **Enable HTTPS**: All authentication must use HTTPS
4. **Configure CORS**: Restrict origins to your domains
5. **Review Permissions**: Ensure role permissions match your security requirements
6. **Enable Audit Logging**: Track all authentication events
7. **Set Up Monitoring**: Monitor failed login attempts

## Support

For authentication-related issues:
1. Check this documentation first
2. Review API logs for error details
3. Verify database connectivity and migrations
4. Test with known working credentials (seeded users)
5. Check JWT token claims and expiration