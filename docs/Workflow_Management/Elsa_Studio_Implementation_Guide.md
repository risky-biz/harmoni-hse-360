# Elsa Studio Implementation Guide
## Step-by-Step Integration Instructions for Harmoni360 HSSE

## Prerequisites

Before starting the implementation, ensure you have:
- ✅ Harmoni360 HSSE application running locally
- ✅ .NET 8 SDK installed
- ✅ Node.js and npm for frontend development
- ✅ PostgreSQL database accessible
- ✅ Understanding of the existing authentication system

## Phase 1: Package Installation and Basic Setup

### Step 1.1: Install Elsa Studio NuGet Packages

Add the following packages to `src/Harmoni360.Web/Harmoni360.Web.csproj`:

```xml
<!-- Add these package references -->
<PackageReference Include="Elsa.Studio.Host.Server" Version="3.0.0" />
<PackageReference Include="Elsa.Studio.Dashboard" Version="3.0.0" />
<PackageReference Include="Elsa.Studio.Workflows" Version="3.0.0" />
<PackageReference Include="Elsa.Studio.Localization" Version="3.0.0" />
```

### Step 1.2: Update Program.cs Configuration

Modify `src/Harmoni360.Web/Program.cs` to add Elsa Studio services:

```csharp
// Add after existing Elsa Core configuration (around line 140)
// Configure Elsa Studio
builder.Services.AddElsaStudio(studio =>
{
    studio.AddDashboardModule();
    studio.AddWorkflowsModule();
    studio.AddLocalizationModule();
    
    // Configure to use existing authentication
    studio.UseAuthentication(auth =>
    {
        auth.UseJwtBearer(); // Use existing JWT configuration
    });
    
    // Configure backend connection
    studio.AddRemoteBackend(client =>
    {
        client.BaseAddress = new Uri("https://localhost:5000/elsa/api/");
        client.AuthenticationHandler = typeof(ElsaStudioAuthenticationHandler);
    });
});
```

### Step 1.3: Create Authentication Handler

Create `src/Harmoni360.Web/Services/ElsaStudioAuthenticationHandler.cs`:

```csharp
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Harmoni360.Web.Services;

public class ElsaStudioAuthenticationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ElsaStudioAuthenticationHandler> _logger;

    public ElsaStudioAuthenticationHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<ElsaStudioAuthenticationHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            // Get the JWT token from the current request
            var token = await httpContext.GetTokenAsync("access_token");
            
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Added JWT token to Elsa Studio request");
            }
            else
            {
                _logger.LogWarning("No JWT token found for Elsa Studio request");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

### Step 1.4: Register Authentication Handler

Add to `Program.cs` service registration:

```csharp
// Register the authentication handler
builder.Services.AddScoped<ElsaStudioAuthenticationHandler>();
builder.Services.AddHttpContextAccessor(); // If not already registered
```

## Phase 2: Routing Configuration

### Step 2.1: Configure Elsa Studio Routing

Update the routing configuration in `Program.cs` (around line 500):

```csharp
// Configure Elsa Studio routing BEFORE the SPA configuration
app.MapWhen(context => context.Request.Path.StartsWithSegments("/elsa-studio"), 
    elsaStudioApp =>
    {
        elsaStudioApp.UseRouting();
        elsaStudioApp.UseAuthentication();
        elsaStudioApp.UseAuthorization();
        
        // Add authorization requirement for workflow management
        elsaStudioApp.Use(async (context, next) =>
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Check if user has WorkflowManagement module access
                var hasAccess = context.User.HasClaim("permission", "WorkflowManagement.Read") ||
                               context.User.IsInRole("SuperAdmin") ||
                               context.User.IsInRole("Developer") ||
                               context.User.IsInRole("WorkflowManager");
                
                if (!hasAccess)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Access denied. Workflow management permissions required.");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Authentication required.");
                return;
            }
            
            await next();
        });
        
        // Map Elsa Studio endpoints
        elsaStudioApp.MapElsaStudio();
    });

// Update existing SPA configuration to exclude /elsa-studio paths
app.MapWhen(context => !context.Request.Path.StartsWithSegments("/elsa/api") &&
                      !context.Request.Path.StartsWithSegments("/elsa-studio") &&
                      !context.Request.Path.StartsWithSegments("/_framework") &&
                      !context.Request.Path.StartsWithSegments("/v1") &&
                      !context.Request.Path.StartsWithSegments("/api") &&
                      !context.Request.Path.StartsWithSegments("/hubs") &&
                      !context.Request.Path.StartsWithSegments("/health"), 
    appBuilder =>
    {
        // Existing SPA configuration...
    });
```

## Phase 3: Frontend Integration

### Step 3.1: Update React Routing

Modify `src/Harmoni360.Web/ClientApp/src/App.tsx` to handle Elsa Studio routing:

```typescript
// Add to the protected routes section (around line 1125)
{/* Workflow Management - Elsa Studio Integration */}
<Route 
  path="/workflows" 
  element={
    <PrivateRoute 
      module={ModuleType.WorkflowManagement} 
      permission={PermissionType.Read}
    >
      <WorkflowManagement />
    </PrivateRoute>
  } 
/>
<Route 
  path="/workflows/*" 
  element={
    <PrivateRoute 
      module={ModuleType.WorkflowManagement} 
      permission={PermissionType.Read}
    >
      <WorkflowManagement />
    </PrivateRoute>
  } 
/>
```

### Step 3.2: Create Enhanced Workflow Management Component

Update `src/Harmoni360.Web/ClientApp/src/pages/workflows/WorkflowManagement.tsx`:

```typescript
import React, { useEffect, useState } from 'react';
import { useAuth } from '../../hooks/useAuth';
import { usePermissions } from '../../hooks/usePermissions';
import { ModuleType, PermissionType } from '../../types/auth';
import { CCard, CCardBody, CCardHeader, CSpinner, CAlert } from '@coreui/react';

const WorkflowManagement: React.FC = () => {
  const { token, user } = useAuth();
  const permissions = usePermissions();
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Check permissions
  const canRead = permissions.hasPermission(ModuleType.WorkflowManagement, PermissionType.Read);
  const canCreate = permissions.hasPermission(ModuleType.WorkflowManagement, PermissionType.Create);
  const canUpdate = permissions.hasPermission(ModuleType.WorkflowManagement, PermissionType.Update);
  const canDelete = permissions.hasPermission(ModuleType.WorkflowManagement, PermissionType.Delete);

  useEffect(() => {
    if (!canRead) {
      setError('You do not have permission to access workflow management.');
      setIsLoading(false);
      return;
    }

    // Set up authentication for the iframe
    const setupElsaStudioAuth = async () => {
      try {
        // The iframe will automatically inherit the authentication context
        // since it's served from the same domain
        setIsLoading(false);
      } catch (err) {
        setError('Failed to initialize Elsa Studio authentication.');
        setIsLoading(false);
      }
    };

    setupElsaStudioAuth();
  }, [canRead, token]);

  if (!canRead) {
    return (
      <CCard>
        <CCardHeader>
          <h4>Access Denied</h4>
        </CCardHeader>
        <CCardBody>
          <CAlert color="warning">
            You do not have permission to access workflow management. 
            Please contact your administrator to request access.
          </CAlert>
        </CCardBody>
      </CCard>
    );
  }

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner color="primary" />
        <span className="ms-2">Loading Elsa Studio...</span>
      </div>
    );
  }

  if (error) {
    return (
      <CCard>
        <CCardHeader>
          <h4>Workflow Management</h4>
        </CCardHeader>
        <CCardBody>
          <CAlert color="danger">{error}</CAlert>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <div className="workflow-management-page" style={{ height: 'calc(100vh - 120px)' }}>
      <div className="mb-3">
        <h2>Workflow Management</h2>
        <p className="text-muted">
          Design, manage, and monitor workflows for your HSSE processes.
        </p>
      </div>
      
      <div style={{ height: 'calc(100% - 80px)', border: '1px solid #dee2e6', borderRadius: '0.375rem' }}>
        <iframe
          src="/elsa-studio/"
          style={{
            width: '100%',
            height: '100%',
            border: 'none',
            borderRadius: '0.375rem'
          }}
          title="Elsa Studio - Workflow Designer"
          allow="fullscreen"
        />
      </div>
    </div>
  );
};

export default WorkflowManagement;
```

## Phase 4: Navigation Integration

### Step 4.1: Add Workflow Management to Navigation

Update `src/Harmoni360.Web/ClientApp/src/utils/navigationUtils.ts`:

```typescript
// Add to the navigation configuration (around line 1000)
{
  component: 'CNavTitle',
  name: 'Workflow Management',
  module: ModuleType.WorkflowManagement,
  submodules: [
    {
      component: 'CNavGroup',
      name: 'Workflows',
      to: '#workflows',
      icon: null,
      items: [
        {
          component: 'CNavItem',
          name: 'Workflow Designer',
          to: '/workflows',
          icon: null,
          module: ModuleType.WorkflowManagement,
          permission: PermissionType.Read,
        },
        {
          component: 'CNavItem',
          name: 'Workflow Instances',
          to: '/workflows/instances',
          icon: null,
          module: ModuleType.WorkflowManagement,
          permission: PermissionType.Read,
        },
        {
          component: 'CNavItem',
          name: 'Workflow Dashboard',
          to: '/workflows/dashboard',
          icon: null,
          module: ModuleType.WorkflowManagement,
          permission: PermissionType.Read,
        },
      ],
    },
  ],
},
```

## Phase 5: Testing and Validation

### Step 5.1: Development Testing

1. **Start the application**:
   ```bash
   cd src/Harmoni360.Web
   dotnet run
   ```

2. **Verify Elsa Studio access**:
   - Navigate to `https://localhost:5000/elsa-studio/`
   - Ensure authentication is working
   - Verify workflow designer loads correctly

3. **Test React integration**:
   - Navigate to `/workflows` in the React app
   - Verify iframe loads Elsa Studio
   - Test navigation between different workflow sections

### Step 5.2: Permission Testing

Test with different user roles:
- **SuperAdmin**: Should have full access
- **WorkflowManager**: Should have create/edit access
- **Regular users**: Should have read-only access or no access

## Phase 6: Production Considerations

### Step 6.1: Security Hardening

1. **Content Security Policy**: Update CSP headers to allow iframe embedding
2. **HTTPS Configuration**: Ensure all Elsa Studio resources are served over HTTPS
3. **Authentication Timeout**: Configure appropriate session timeouts

### Step 6.2: Performance Optimization

1. **Static Asset Caching**: Configure appropriate cache headers for Elsa Studio assets
2. **Compression**: Enable response compression for Elsa Studio content
3. **Resource Loading**: Optimize initial load time for Elsa Studio components

## Code Examples and Templates

### Example: Complete Program.cs Configuration

```csharp
// Complete Elsa Studio configuration in Program.cs
var builder = WebApplication.CreateBuilder(args);

// ... existing service registrations ...

// Configure Elsa Studio (add after existing Elsa Core configuration)
builder.Services.AddElsaStudio(studio =>
{
    studio.AddDashboardModule();
    studio.AddWorkflowsModule();
    studio.AddLocalizationModule();

    // Use existing JWT authentication
    studio.UseAuthentication(auth =>
    {
        auth.UseJwtBearer();
    });

    // Configure backend connection
    studio.AddRemoteBackend(client =>
    {
        client.BaseAddress = new Uri("https://localhost:5000/elsa/api/");
        client.AuthenticationHandler = typeof(ElsaStudioAuthenticationHandler);
    });
});

// Register authentication handler
builder.Services.AddScoped<ElsaStudioAuthenticationHandler>();

var app = builder.Build();

// ... existing middleware configuration ...

// Configure Elsa Studio routing (add before SPA configuration)
app.MapWhen(context => context.Request.Path.StartsWithSegments("/elsa-studio"),
    elsaStudioApp =>
    {
        elsaStudioApp.UseRouting();
        elsaStudioApp.UseAuthentication();
        elsaStudioApp.UseAuthorization();

        // Authorization middleware for workflow management
        elsaStudioApp.Use(async (context, next) =>
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var hasAccess = context.User.HasClaim("permission", "WorkflowManagement.Read") ||
                               context.User.IsInRole("SuperAdmin") ||
                               context.User.IsInRole("Developer") ||
                               context.User.IsInRole("WorkflowManager");

                if (!hasAccess)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Access denied. Workflow management permissions required.");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Authentication required.");
                return;
            }

            await next();
        });

        elsaStudioApp.MapElsaStudio();
    });

// Update SPA configuration to exclude Elsa Studio paths
app.MapWhen(context => !context.Request.Path.StartsWithSegments("/elsa/api") &&
                      !context.Request.Path.StartsWithSegments("/elsa-studio") &&
                      !context.Request.Path.StartsWithSegments("/_framework") &&
                      !context.Request.Path.StartsWithSegments("/v1") &&
                      !context.Request.Path.StartsWithSegments("/api") &&
                      !context.Request.Path.StartsWithSegments("/hubs") &&
                      !context.Request.Path.StartsWithSegments("/health"),
    appBuilder =>
    {
        appBuilder.UseSpa(spa =>
        {
            spa.Options.SourcePath = "ClientApp";
            // ... existing SPA configuration
        });
    });

app.Run();
```

## Troubleshooting

### Common Issues

1. **Authentication Failures**:
   - Verify JWT token is being passed correctly
   - Check token expiration and refresh logic
   - Ensure user has required permissions

2. **Routing Issues**:
   - Verify path precedence in routing configuration
   - Check for conflicts with existing routes
   - Ensure proper fallback handling

3. **Performance Issues**:
   - Monitor Blazor SignalR connections
   - Check for memory leaks in long-running sessions
   - Optimize database queries for workflow data

## Next Steps

After successful implementation:

1. **User Training**: Provide training materials for workflow designers
2. **Documentation**: Create user guides for workflow management
3. **Monitoring**: Set up monitoring for workflow execution and performance
4. **Backup Strategy**: Implement backup procedures for workflow definitions

This implementation guide provides a complete roadmap for integrating Elsa Studio into the Harmoni360 HSSE application while maintaining security, performance, and usability standards.
