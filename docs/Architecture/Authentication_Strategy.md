# HarmoniHSE360 Authentication Strategy

## Overview
HarmoniHSE360 implements a flexible authentication system that supports multiple authentication methods to accommodate different deployment scenarios and organizational requirements.

## Authentication Options

### 1. **Local Authentication** (Always Available)
- ASP.NET Core Identity with secure password hashing
- Email/Username based login
- Password policies (complexity, expiration, history)
- Account lockout protection
- Email verification for new accounts

### 2. **Active Directory Integration** (Optional)
- Windows Authentication for on-premises AD
- Azure Active Directory via OAuth2/OpenID Connect
- Automatic user provisioning from AD groups
- Role synchronization with AD security groups

### 3. **External Identity Providers** (Optional)
- Google Workspace (for schools using Google)
- Microsoft 365 (for schools using Office)
- Custom SAML 2.0 providers
- OpenID Connect providers

### 4. **Multi-Factor Authentication**
- TOTP (Time-based One-Time Password) - Google Authenticator, Microsoft Authenticator
- SMS verification (with Twilio/local SMS gateway)
- Email verification codes
- Backup codes for account recovery

## Implementation Architecture

```
┌─────────────────────────────────────────────────────┐
│                  HarmoniHSE360                      │
├─────────────────────────────────────────────────────┤
│              Authentication Layer                    │
│  ┌────────────────────────────────────────────┐    │
│  │          ASP.NET Core Identity             │    │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐  │    │
│  │  │  Local   │ │    AD    │ │ External │  │    │
│  │  │  Users   │ │  Users   │ │  Users   │  │    │
│  │  └──────────┘ └──────────┘ └──────────┘  │    │
│  └────────────────────────────────────────────┘    │
│                                                     │
│  ┌────────────────────────────────────────────┐    │
│  │         Authentication Services            │    │
│  │  • Token Generation (JWT)                  │    │
│  │  • Session Management                      │    │
│  │  • Permission Resolution                   │    │
│  │  • Audit Logging                          │    │
│  └────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────┘
```

## Configuration Examples

### Development Environment (Local Auth Only)
```json
{
  "Authentication": {
    "EnableLocalAuth": true,
    "EnableActiveDirectory": false,
    "EnableExternalProviders": false,
    "RequireMfa": false
  }
}
```

### Enterprise Deployment (AD + Local Fallback)
```json
{
  "Authentication": {
    "EnableLocalAuth": true,
    "EnableActiveDirectory": true,
    "ActiveDirectory": {
      "Domain": "school.local",
      "AutoCreateUsers": true,
      "DefaultRoles": ["Employee"]
    },
    "RequireMfa": true,
    "MfaProviders": ["TOTP", "SMS"]
  }
}
```

### Cloud Deployment (External Providers)
```json
{
  "Authentication": {
    "EnableLocalAuth": true,
    "EnableExternalProviders": true,
    "ExternalProviders": {
      "Google": {
        "ClientId": "xxx",
        "ClientSecret": "yyy",
        "AllowedDomains": ["school.edu"]
      },
      "AzureAd": {
        "TenantId": "xxx",
        "ClientId": "yyy"
      }
    }
  }
}
```

## User Provisioning Flow

1. **First-Time Login**:
   - Check if user exists in local database
   - If not, create user based on authentication source
   - Assign default roles based on configuration
   - Send welcome email with additional setup steps

2. **Role Assignment**:
   - Local users: Manual role assignment by admin
   - AD users: Automatic based on AD groups
   - External users: Based on email domain or claims

3. **Profile Completion**:
   - Required fields: Full name, department, phone
   - Optional: Photo, emergency contact, preferences

## Security Considerations

1. **Password Policies** (for local auth):
   - Minimum 12 characters
   - Mix of uppercase, lowercase, numbers, symbols
   - Password history (last 5 passwords)
   - Expiration every 90 days (configurable)

2. **Session Management**:
   - JWT tokens with 8-hour expiration
   - Refresh tokens with 30-day expiration
   - Sliding expiration for active users
   - Force logout on security events

3. **Account Protection**:
   - Lockout after 5 failed attempts
   - Progressive delays between attempts
   - CAPTCHA after 3 failed attempts
   - Email alerts for suspicious activity

## Migration Strategy

For organizations moving from manual processes:

1. **Phase 1**: Enable local authentication only
2. **Phase 2**: Import existing user data
3. **Phase 3**: Enable AD integration (if applicable)
4. **Phase 4**: Enforce MFA for sensitive roles
5. **Phase 5**: Disable legacy access methods

## Benefits of This Approach

1. **Flexibility**: Works in any environment
2. **Scalability**: From small schools to large enterprises
3. **Security**: Industry-standard practices
4. **User Experience**: Single sign-on where possible
5. **Compliance**: Meets regulatory requirements
6. **Future-Proof**: Easy to add new providers