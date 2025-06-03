# **Comprehensive Development Practices for HarmoniHSE360 Enterprise Application**
A complete guide for building a Health, Safety, and Environment application using .NET 8 Modular Monolith architecture with React and CoreUI frontend, focusing on single deployment, real-time updates, time-series data, and development workflow optimization.

## **Table of Contents**
1. [Architecture Overview - Modular Monolith](#architecture-overview)
2. [Project Structure and Setup](#project-structure)
3. [Backend API Development within Monolith](#backend-api)
4. [Integrated React Frontend with CoreUI](#frontend-integration)
5. [Real-time Features with SignalR](#realtime-features)
6. [Mobile Development Strategy](#mobile-development)
7. [PostgreSQL with TimescaleDB Optimization](#postgresql-timescaledb)
8. [Single Deployment to Kubernetes](#kubernetes-deployment)
9. [Development Workflow](#development-workflow)

## **Architecture Overview - Modular Monolith**

### **Why Modular Monolith?**
- **Simplified Deployment**: One application, one deployment pipeline
- **Reduced Operational Complexity**: No service discovery, API gateways, or inter-service communication
- **Easier Development**: All code in one repository, easier debugging
- **Performance**: No network latency between modules
- **Future-Ready**: Can evolve to microservices when needed

### **System Architecture**
```
┌─────────────────────────────────────────────────────────────┐
│             HarmoniHSE360 Modular Monolith                   │
├─────────────────────────────────────────────────────────────┤
│                    Presentation Layer                         │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │  React SPA  │  │   API        │  │    SignalR       │   │
│  │  (Static)   │  │  Controllers │  │    Hubs          │   │
│  └─────────────┘  └──────────────┘  └──────────────────┘   │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                          │
│  ┌────────────┐  ┌────────────┐  ┌─────────────────────┐   │
│  │ Incidents  │  │  Hazards   │  │    Compliance       │   │
│  │  Module    │  │   Module   │  │     Module          │   │
│  └────────────┘  └────────────┘  └─────────────────────┘   │
│  ┌────────────┐  ┌────────────┐  ┌─────────────────────┐   │
│  │ Training   │  │Environment │  │    Analytics        │   │
│  │  Module    │  │   Module   │  │     Module          │   │
│  └────────────┘  └────────────┘  └─────────────────────┘   │
├─────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                       │
│  ┌────────────┐  ┌────────────┐  ┌─────────────────────┐   │
│  │ PostgreSQL │  │   Redis    │  │   File Storage      │   │
│  │ TimescaleDB│  │   Cache    │  │   (Local/S3)        │   │
│  └────────────┘  └────────────┘  └─────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### **Technology Stack**
- **Backend**: .NET 8, ASP.NET Core, Entity Framework Core 8
- **Frontend**: React 18.2, CoreUI React 4.11, TypeScript 5.3
- **State Management**: Redux Toolkit with RTK Query
- **Real-time**: SignalR Core
- **Database**: PostgreSQL 16 with TimescaleDB 2.13
- **Caching**: Redis 7.2 (or In-Memory for simpler deployment)
- **Container**: Single Docker container
- **Deployment**: Kubernetes (single deployment unit)

## **Project Structure and Setup**

### **Monolith Project Structure**
```
HarmoniHSE360/
├── src/
│   ├── HarmoniHSE360.Domain/           # Domain entities and interfaces
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Interfaces/
│   │
│   ├── HarmoniHSE360.Application/      # Business logic
│   │   ├── Features/
│   │   │   ├── Incidents/
│   │   │   ├── Hazards/
│   │   │   └── [Other modules...]
│   │   └── Common/
│   │
│   ├── HarmoniHSE360.Infrastructure/   # Data access and external services
│   │   ├── Persistence/
│   │   ├── Services/
│   │   └── DependencyInjection.cs
│   │
│   └── HarmoniHSE360.Web/              # Web application host
│       ├── Controllers/                # API Controllers
│       ├── Hubs/                       # SignalR Hubs
│       ├── ClientApp/                  # React application
│       │   ├── public/
│       │   ├── src/
│       │   ├── package.json
│       │   └── vite.config.ts
│       ├── Middleware/
│       ├── Program.cs
│       └── HarmoniHSE360.Web.csproj
│
├── tests/
│   ├── HarmoniHSE360.UnitTests/
│   ├── HarmoniHSE360.IntegrationTests/
│   └── HarmoniHSE360.E2ETests/
│
├── docker/
│   └── Dockerfile
│
└── k8s/
    ├── deployment.yaml
    ├── service.yaml
    └── ingress.yaml
```

### **Initial Project Setup**

#### **1. Create the Solution**
```bash
# Create solution
dotnet new sln -n HarmoniHSE360

# Create projects
dotnet new classlib -n HarmoniHSE360.Domain
dotnet new classlib -n HarmoniHSE360.Application
dotnet new classlib -n HarmoniHSE360.Infrastructure
dotnet new webapi -n HarmoniHSE360.Web

# Add projects to solution
dotnet sln add src/HarmoniHSE360.Domain/HarmoniHSE360.Domain.csproj
dotnet sln add src/HarmoniHSE360.Application/HarmoniHSE360.Application.csproj
dotnet sln add src/HarmoniHSE360.Infrastructure/HarmoniHSE360.Infrastructure.csproj
dotnet sln add src/HarmoniHSE360.Web/HarmoniHSE360.Web.csproj

# Add project references
dotnet add src/HarmoniHSE360.Application reference src/HarmoniHSE360.Domain
dotnet add src/HarmoniHSE360.Infrastructure reference src/HarmoniHSE360.Application
dotnet add src/HarmoniHSE360.Web reference src/HarmoniHSE360.Infrastructure
```

#### **2. Setup React App within Web Project**
```bash
cd src/HarmoniHSE360.Web

# Create React app with Vite
npm create vite@latest ClientApp -- --template react-ts

cd ClientApp
npm install @coreui/react @coreui/icons @coreui/icons-react
npm install @reduxjs/toolkit react-redux react-router-dom
npm install axios @microsoft/signalr date-fns
npm install -D @types/react @types/react-dom @vitejs/plugin-react
```

#### **3. Configure Web Project for SPA Integration**

**HarmoniHSE360.Web.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <SpaRoot>ClientApp\</SpaRoot>
    <DefaultItemExcludes>$(DefaultItemExcludes);$(SpaRoot)node_modules\**</DefaultItemExcludes>
    <SpaProxyServerUrl>http://localhost:5173</SpaProxyServerUrl>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.SpaServices.Extensions" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="6.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\HarmoniHSE360.Infrastructure\HarmoniHSE360.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Don't publish the SPA source files, but do show them in the project files list -->
    <Content Remove="$(SpaRoot)**" />
    <None Remove="$(SpaRoot)**" />
    <None Include="$(SpaRoot)**" Exclude="$(SpaRoot)node_modules\**" />
  </ItemGroup>

  <Target Name="DebugEnsureNodeEnv" BeforeTargets="Build" Condition=" '$(Configuration)' == 'Debug' And !Exists('$(SpaRoot)node_modules') ">
    <Exec Command="node --version" ContinueOnError="true">
      <Output TaskParameter="ExitCode" PropertyName="ErrorCode" />
    </Exec>
    <Error Condition="'$(ErrorCode)' != '0'" Text="Node.js is required to build and run this project." />
    <Message Importance="high" Text="Restoring dependencies using 'npm'. This may take several minutes..." />
    <Exec WorkingDirectory="$(SpaRoot)" Command="npm install" />
  </Target>

  <Target Name="PublishRunWebpack" AfterTargets="ComputeFilesToPublish">
    <Exec WorkingDirectory="$(SpaRoot)" Command="npm install" />
    <Exec WorkingDirectory="$(SpaRoot)" Command="npm run build" />
    <ItemGroup>
      <DistFiles Include="$(SpaRoot)dist\**" />
      <ResolvedFileToPublish Include="@(DistFiles->'%(FullPath)')" Exclude="@(ResolvedFileToPublish)">
        <RelativePath>%(DistFiles.Identity)</RelativePath>
        <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
        <ExcludeFromSingleFile>true</ExcludeFromSingleFile>
      </ResolvedFileToPublish>
    </ItemGroup>
  </Target>
</Project>
```

## **Backend API Development within Monolith**

### **Program.cs Configuration**
```csharp
using HarmoniHSE360.Application;
using HarmoniHSE360.Infrastructure;
using HarmoniHSE360.Web.Hubs;
using HarmoniHSE360.Web.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HarmoniHSE360 API",
        Version = "v1",
        Description = "HSE Management System API"
    });
});

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
        
        // Support SignalR authentication
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HSEManager", policy => policy.RequireRole("HSEManager", "Admin"));
    options.AddPolicy("Employee", policy => policy.RequireRole("Employee", "HSEManager", "Admin"));
});

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add SPA static files
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/dist";
});

// Build app
var app = builder.Build();

// Configure pipeline
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

// Serve static files for React app
app.UseStaticFiles();
if (!app.Environment.IsDevelopment())
{
    app.UseSpaStaticFiles();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Custom middleware
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();

// Map endpoints
app.MapControllers();
app.MapHub<IncidentHub>("/hubs/incidents");
app.MapHub<MetricsHub>("/hubs/metrics");
app.MapHub<NotificationHub>("/hubs/notifications");

// Health checks
app.MapHealthChecks("/health");

// Configure SPA
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
    
    if (app.Environment.IsDevelopment())
    {
        // Use Vite dev server
        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    }
});

// Seed database if needed
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    
    if (app.Environment.IsDevelopment())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
        await seeder.SeedAsync();
    }
}

await app.RunAsync();
```

### **API Controller Example**
```csharp
// Controllers/IncidentsController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Employee")]
public class IncidentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<IncidentHub> _hubContext;
    
    public IncidentsController(IMediator mediator, IHubContext<IncidentHub> hubContext)
    {
        _mediator = mediator;
        _hubContext = hubContext;
    }
    
    [HttpGet]
    public async Task<ActionResult<PaginatedList<IncidentDto>>> GetIncidents(
        [FromQuery] GetIncidentsQuery query)
    {
        return await _mediator.Send(query);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<IncidentDetailDto>> GetIncident(int id)
    {
        var result = await _mediator.Send(new GetIncidentByIdQuery { Id = id });
        if (result == null)
            return NotFound();
        return result;
    }
    
    [HttpPost]
    public async Task<ActionResult<IncidentDto>> CreateIncident(
        [FromForm] CreateIncidentCommand command)
    {
        var result = await _mediator.Send(command);
        
        // Notify connected clients
        await _hubContext.Clients.All.SendAsync("IncidentCreated", result);
        
        return CreatedAtAction(nameof(GetIncident), new { id = result.Id }, result);
    }
}
```

### **Module Organization Example**
```csharp
// Application/Features/Incidents/IncidentModule.cs
namespace HarmoniHSE360.Application.Features.Incidents;

public static class IncidentModule
{
    public static IServiceCollection AddIncidentModule(this IServiceCollection services)
    {
        // Register module-specific services
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IIncidentNotificationService, IncidentNotificationService>();
        
        // Register validators
        services.AddValidatorsFromAssemblyContaining<CreateIncidentCommandValidator>();
        
        // Register AutoMapper profiles
        services.AddAutoMapper(typeof(IncidentMappingProfile));
        
        return services;
    }
}

// Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // Add modules
        services.AddIncidentModule();
        services.AddHazardModule();
        services.AddComplianceModule();
        services.AddTrainingModule();
        services.AddEnvironmentModule();
        services.AddAnalyticsModule();
        
        // Add common services
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        
        return services;
    }
}
```

## **Integrated React Frontend with CoreUI**

### **React App Configuration**

#### **vite.config.ts**
```typescript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';
import path from 'path';

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      manifest: {
        name: 'HarmoniHSE360',
        short_name: 'HSE360',
        theme_color: '#0097A7',
        background_color: '#ffffff',
        display: 'standalone',
        start_url: '/',
        icons: [
          {
            src: '/icon-192.png',
            sizes: '192x192',
            type: 'image/png'
          },
          {
            src: '/icon-512.png',
            sizes: '512x512',
            type: 'image/png'
          }
        ]
      }
    })
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    }
  },
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/hubs': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
        ws: true
      }
    }
  },
  build: {
    outDir: 'dist',
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: {
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          'redux-vendor': ['@reduxjs/toolkit', 'react-redux'],
          'coreui-vendor': ['@coreui/react', '@coreui/icons-react'],
        }
      }
    }
  }
});
```

#### **App.tsx - Main React Component**
```tsx
import React, { Suspense, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Provider } from 'react-redux';

// CoreUI styles
import '@coreui/coreui/dist/css/coreui.min.css';
import '@coreui/icons/css/all.min.css';

// Custom styles
import './styles/app.scss';

// Store
import { store } from './store';

// Layouts
import DefaultLayout from './layouts/DefaultLayout';
import AuthLayout from './layouts/AuthLayout';

// Guards
import PrivateRoute from './components/auth/PrivateRoute';

// Lazy load pages
const Dashboard = React.lazy(() => import('./pages/Dashboard'));
const IncidentList = React.lazy(() => import('./pages/incidents/IncidentList'));
const IncidentCreate = React.lazy(() => import('./pages/incidents/IncidentCreate'));
const Login = React.lazy(() => import('./pages/auth/Login'));

// Loading component
const Loading = () => (
  <div className="d-flex justify-content-center align-items-center min-vh-100">
    <CSpinner color="primary" />
  </div>
);

function App() {
  useEffect(() => {
    // Initialize service worker for offline support
    if ('serviceWorker' in navigator) {
      navigator.serviceWorker.register('/sw.js');
    }
  }, []);

  return (
    <Provider store={store}>
      <BrowserRouter>
        <Suspense fallback={<Loading />}>
          <Routes>
            {/* Auth Routes */}
            <Route element={<AuthLayout />}>
              <Route path="/login" element={<Login />} />
            </Route>
            
            {/* Protected Routes */}
            <Route element={<PrivateRoute><DefaultLayout /></PrivateRoute>}>
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/dashboard" element={<Dashboard />} />
              
              {/* Incident Management */}
              <Route path="/incidents">
                <Route index element={<IncidentList />} />
                <Route path="create" element={<IncidentCreate />} />
              </Route>
            </Route>
          </Routes>
        </Suspense>
      </BrowserRouter>
    </Provider>
  );
}

export default App;
```

### **API Service Configuration**
```typescript
// src/api/client.ts
import axios, { AxiosInstance } from 'axios';
import { store } from '@/store';
import { logout, refreshToken } from '@/features/auth/authSlice';

// Use relative URLs since we're in the same deployment
const API_BASE_URL = '/api';

export const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000,
});

// Request interceptor
apiClient.interceptors.request.use(
  (config) => {
    const state = store.getState();
    const token = state.auth.token;
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        await store.dispatch(refreshToken()).unwrap();
        const state = store.getState();
        originalRequest.headers.Authorization = `Bearer ${state.auth.token}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        store.dispatch(logout());
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);
```

### **SignalR Integration**
```typescript
// src/services/signalRService.ts
import * as signalR from '@microsoft/signalr';
import { store } from '@/store';

class SignalRService {
  private connection: signalR.HubConnection | null = null;

  constructor() {
    this.createConnection();
  }

  private createConnection() {
    const state = store.getState();
    const token = state.auth.token;

    // Use relative URL for same-origin deployment
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.setupEventHandlers();
  }

  private setupEventHandlers() {
    if (!this.connection) return;

    this.connection.on('IncidentCreated', (incident) => {
      // Handle real-time incident notifications
      console.log('New incident:', incident);
    });

    this.connection.on('MetricUpdate', (metric, value) => {
      // Handle real-time metric updates
      console.log('Metric update:', metric, value);
    });
  }

  async startConnection(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.start();
        console.log('SignalR Connected');
      } catch (err) {
        console.error('SignalR Connection Error:', err);
        setTimeout(() => this.startConnection(), 5000);
      }
    }
  }
}

export const signalRService = new SignalRService();
```

## **Real-time Features with SignalR**

### **SignalR Hub Implementation**
```csharp
// Hubs/IncidentHub.cs
[Authorize]
public class IncidentHub : Hub
{
    private readonly ILogger<IncidentHub> _logger;
    
    public IncidentHub(ILogger<IncidentHub> logger)
    {
        _logger = logger;
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var roles = Context.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();
        
        // Add user to role-based groups
        foreach (var role in roles)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role-{role}");
        }
        
        // Add to user-specific group
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        
        await base.OnConnectedAsync();
        _logger.LogInformation("User {UserId} connected to IncidentHub", userId);
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} disconnected from IncidentHub", userId);
        await base.OnDisconnectedAsync(exception);
    }
}

// Usage in Domain Event Handler
public class IncidentCreatedEventHandler : INotificationHandler<IncidentCreatedEvent>
{
    private readonly IHubContext<IncidentHub> _hubContext;
    
    public IncidentCreatedEventHandler(IHubContext<IncidentHub> hubContext)
    {
        _hubContext = hubContext;
    }
    
    public async Task Handle(IncidentCreatedEvent notification, CancellationToken cancellationToken)
    {
        var incidentDto = new IncidentNotificationDto
        {
            Id = notification.Incident.Id,
            Title = notification.Incident.Title,
            Severity = notification.Incident.Severity,
            Location = notification.Incident.Location,
            ReportedAt = notification.Incident.CreatedAt
        };
        
        // Notify all connected clients
        await _hubContext.Clients.All.SendAsync("IncidentCreated", incidentDto, cancellationToken);
        
        // Notify specific groups based on severity
        if (notification.Incident.Severity >= IncidentSeverity.Serious)
        {
            await _hubContext.Clients.Group("role-HSEManager")
                .SendAsync("SeriousIncidentAlert", incidentDto, cancellationToken);
        }
    }
}
```

## **Mobile Development Strategy**

### **Progressive Web App Configuration**
Since we're using a monolith, the PWA is served directly from the ASP.NET Core application:

```csharp
// Program.cs addition for PWA manifest
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Service Worker can't be cached
        if (ctx.File.Name == "sw.js")
        {
            ctx.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
            ctx.Context.Response.Headers.Add("Expires", "-1");
        }
    }
});

// Ensure manifest.json is served correctly
app.MapGet("/manifest.json", () => Results.File("ClientApp/dist/manifest.json", "application/manifest+json"));
```

### **Service Worker for Offline Support**
```javascript
// ClientApp/public/sw.js
const CACHE_NAME = 'hse360-v1';
const urlsToCache = [
  '/',
  '/index.html',
  '/static/css/main.css',
  '/static/js/main.js',
  '/manifest.json',
  '/icon-192.png',
  '/icon-512.png'
];

// Install event
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(urlsToCache))
  );
});

// Fetch event
self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request)
      .then(response => {
        // Cache hit - return response
        if (response) {
          return response;
        }

        // Clone the request
        const fetchRequest = event.request.clone();

        return fetch(fetchRequest).then(response => {
          // Check if valid response
          if (!response || response.status !== 200 || response.type !== 'basic') {
            return response;
          }

          // Clone the response
          const responseToCache = response.clone();

          caches.open(CACHE_NAME)
            .then(cache => {
              cache.put(event.request, responseToCache);
            });

          return response;
        });
      })
  );
});

// Background sync for offline incident reports
self.addEventListener('sync', event => {
  if (event.tag === 'sync-incidents') {
    event.waitUntil(syncOfflineIncidents());
  }
});

async function syncOfflineIncidents() {
  const db = await openDB('hse360-offline', 1);
  const tx = db.transaction('incidents', 'readonly');
  const incidents = await tx.store.getAll();
  
  for (const incident of incidents) {
    try {
      const response = await fetch('/api/incidents', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${incident.token}`
        },
        body: JSON.stringify(incident.data)
      });
      
      if (response.ok) {
        // Remove from offline store
        await db.delete('incidents', incident.id);
      }
    } catch (error) {
      console.error('Failed to sync incident:', error);
    }
  }
}
```

## **PostgreSQL with TimescaleDB Optimization**

### **Database Configuration in Monolith**
```csharp
// Infrastructure/Persistence/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<Hazard> Hazards => Set<Hazard>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();
    public DbSet<Training> Trainings => Set<Training>();
    public DbSet<EnvironmentalReading> EnvironmentalReadings => Set<EnvironmentalReading>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Configure TimescaleDB hypertable
        modelBuilder.HasPostgresExtension("timescaledb");
        
        modelBuilder.Entity<EnvironmentalReading>(entity =>
        {
            entity.HasNoKey(); // TimescaleDB handles this
            entity.ToTable("environmental_readings");
            
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.SensorId).HasColumnName("sensor_id");
            entity.Property(e => e.Value).HasColumnName("value");
            
            entity.HasIndex(e => new { e.SensorId, e.Time })
                .HasDatabaseName("idx_env_sensor_time");
        });
    }
}

// Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsqlOptions.UseNodaTime();
                });
        });
        
        // Add repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<ITimeSeriesRepository, TimeSeriesRepository>();
        
        // Add caching
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "HSE360";
        });
        
        // Add other infrastructure services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        
        return services;
    }
}
```

### **Time-Series Repository**
```csharp
// Infrastructure/Persistence/Repositories/TimeSeriesRepository.cs
public class TimeSeriesRepository : ITimeSeriesRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TimeSeriesRepository> _logger;
    
    public TimeSeriesRepository(ApplicationDbContext context, ILogger<TimeSeriesRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task BulkInsertReadingsAsync(IEnumerable<EnvironmentalReading> readings)
    {
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        using var writer = connection.BeginBinaryImport(
            "COPY environmental_readings (time, sensor_id, location_id, measurement_type, value, unit) FROM STDIN (FORMAT BINARY)");
        
        foreach (var reading in readings)
        {
            writer.StartRow();
            writer.Write(reading.Time, NpgsqlDbType.TimestampTz);
            writer.Write(reading.SensorId);
            writer.Write(reading.LocationId);
            writer.Write(reading.MeasurementType);
            writer.Write(reading.Value);
            writer.Write(reading.Unit);
        }
        
        await writer.CompleteAsync();
        _logger.LogInformation("Bulk inserted {Count} environmental readings", readings.Count());
    }
    
    public async Task<IEnumerable<EnvironmentalReading>> GetLatestReadingsAsync(
        int locationId,
        string measurementType,
        TimeSpan period)
    {
        var since = DateTime.UtcNow.Subtract(period);
        
        return await _context.EnvironmentalReadings
            .FromSqlRaw(@"
                SELECT * FROM environmental_readings
                WHERE location_id = {0}
                AND measurement_type = {1}
                AND time > {2}
                ORDER BY time DESC",
                locationId, measurementType, since)
            .ToListAsync();
    }
}
```

## **Single Deployment to Kubernetes**

### **Dockerfile for Monolith**
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install Node.js for React build
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy csproj files and restore
COPY ["src/HarmoniHSE360.Web/HarmoniHSE360.Web.csproj", "HarmoniHSE360.Web/"]
COPY ["src/HarmoniHSE360.Application/HarmoniHSE360.Application.csproj", "HarmoniHSE360.Application/"]
COPY ["src/HarmoniHSE360.Domain/HarmoniHSE360.Domain.csproj", "HarmoniHSE360.Domain/"]
COPY ["src/HarmoniHSE360.Infrastructure/HarmoniHSE360.Infrastructure.csproj", "HarmoniHSE360.Infrastructure/"]
RUN dotnet restore "HarmoniHSE360.Web/HarmoniHSE360.Web.csproj"

# Copy everything else
COPY src/ .

# Build React app
WORKDIR /src/HarmoniHSE360.Web/ClientApp
RUN npm ci
RUN npm run build

# Build .NET app
WORKDIR /src/HarmoniHSE360.Web
RUN dotnet build "HarmoniHSE360.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "HarmoniHSE360.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Install cultures for globalization
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Create non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# Copy published files
COPY --from=publish /app/publish .

# Set user
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "HarmoniHSE360.Web.dll"]
```

### **Kubernetes Deployment**
```yaml
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: hse360
  namespace: hse360
spec:
  replicas: 3
  selector:
    matchLabels:
      app: hse360
  template:
    metadata:
      labels:
        app: hse360
    spec:
      containers:
      - name: hse360
        image: registry.biznetgio.com/bsj/hse360:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://+:8080"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: hse360-secrets
              key: db-connection-string
        - name: ConnectionStrings__Redis
          value: "redis-service:6379"
        - name: Jwt__Key
          valueFrom:
            secretKeyRef:
              name: hse360-secrets
              key: jwt-key
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
          limits:
            memory: "1Gi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
        volumeMounts:
        - name: uploads
          mountPath: /app/uploads
      volumes:
      - name: uploads
        persistentVolumeClaim:
          claimName: hse360-uploads-pvc

---
# k8s/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: hse360-service
  namespace: hse360
spec:
  selector:
    app: hse360
  ports:
  - port: 80
    targetPort: 8080
  type: ClusterIP

---
# k8s/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: hse360-ingress
  namespace: hse360
  annotations:
    kubernetes.io/ingress.class: "nginx"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/proxy-body-size: "50m"
spec:
  tls:
  - hosts:
    - hse360.bsj.sch.id
    secretName: hse360-tls
  rules:
  - host: hse360.bsj.sch.id
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: hse360-service
            port:
              number: 80

---
# k8s/hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: hse360-hpa
  namespace: hse360
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: hse360
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### **Supporting Services**
```yaml
# k8s/redis.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: redis
  namespace: hse360
spec:
  replicas: 1
  selector:
    matchLabels:
      app: redis
  template:
    metadata:
      labels:
        app: redis
    spec:
      containers:
      - name: redis
        image: redis:7-alpine
        ports:
        - containerPort: 6379
        resources:
          requests:
            memory: "256Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "200m"

---
apiVersion: v1
kind: Service
metadata:
  name: redis-service
  namespace: hse360
spec:
  selector:
    app: redis
  ports:
  - port: 6379
    targetPort: 6379
```

## **Development Workflow**

### **Local Development Setup**
```yaml
# docker-compose.dev.yml
version: '3.8'

services:
  db:
    image: timescale/timescaledb-ha:pg16-latest
    environment:
      POSTGRES_DB: hse360_dev
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  app:
    build:
      context: .
      dockerfile: Dockerfile
      target: build
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__DefaultConnection: "Host=db;Database=hse360_dev;Username=postgres;Password=postgres"
      ConnectionStrings__Redis: "redis:6379"
    ports:
      - "5000:5000"
      - "5173:5173"
    volumes:
      - ./src:/src
      - ~/.aspnet/https:/https:ro
    depends_on:
      - db
      - redis
    command: dotnet watch run --project /src/HarmoniHSE360.Web/HarmoniHSE360.Web.csproj

volumes:
  postgres_data:
```

### **Development Scripts**

**package.json in root:**
```json
{
  "name": "harmonihse360",
  "version": "1.0.0",
  "scripts": {
    "dev": "docker-compose -f docker-compose.dev.yml up",
    "dev:api": "cd src/HarmoniHSE360.Web && dotnet watch run",
    "dev:ui": "cd src/HarmoniHSE360.Web/ClientApp && npm run dev",
    "build": "dotnet build",
    "test": "dotnet test",
    "test:ui": "cd src/HarmoniHSE360.Web/ClientApp && npm test",
    "docker:build": "docker build -t hse360:latest .",
    "docker:run": "docker run -p 8080:8080 hse360:latest",
    "ef:add": "dotnet ef migrations add",
    "ef:update": "dotnet ef database update"
  }
}
```

### **CI/CD Pipeline**
```yaml
# .github/workflows/ci-cd.yml
name: HSE360 CI/CD

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  REGISTRY: registry.biznetgio.com
  IMAGE_NAME: bsj/hse360

jobs:
  test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: timescale/timescaledb:latest-pg16
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: hse360_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '20'
    
    - name: Restore .NET dependencies
      run: dotnet restore
    
    - name: Build .NET
      run: dotnet build --no-restore --configuration Release
    
    - name: Test .NET
      run: dotnet test --no-build --configuration Release --logger "trx"
      env:
        ConnectionStrings__DefaultConnection: "Host=localhost;Database=hse360_test;Username=postgres;Password=postgres"
    
    - name: Install React dependencies
      run: npm ci
      working-directory: src/HarmoniHSE360.Web/ClientApp
    
    - name: Test React
      run: npm test -- --coverage --watchAll=false
      working-directory: src/HarmoniHSE360.Web/ClientApp
    
    - name: Build React
      run: npm run build
      working-directory: src/HarmoniHSE360.Web/ClientApp

  build-and-push:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Log in to Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ secrets.REGISTRY_USERNAME }}
        password: ${{ secrets.REGISTRY_PASSWORD }}
    
    - name: Build and push
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: |
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:latest
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}

  deploy:
    needs: build-and-push
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Deploy to Kubernetes
      run: |
        kubectl set image deployment/hse360 hse360=${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }} -n hse360
        kubectl rollout status deployment/hse360 -n hse360
```

## **Conclusion**

This Modular Monolith architecture provides:

### **Benefits:**
1. **Single Deployment Unit**: One Docker container, one Kubernetes deployment
2. **Simplified Operations**: No service discovery, API gateways, or complex networking
3. **Shared Resources**: Efficient resource usage with shared memory and CPU
4. **Easy Development**: All code in one place, simplified debugging
5. **Performance**: No network latency between modules
6. **Cost Effective**: Lower infrastructure costs

### **Maintains Modern Features:**
- React with CoreUI for rich UI
- Real-time updates with SignalR
- Progressive Web App capabilities
- Clean Architecture within the monolith
- Module separation for future microservices migration

### **Migration Path:**
When ready to scale, modules can be extracted to microservices:
1. Start with the module boundaries already defined
2. Extract high-traffic modules first (e.g., Analytics)
3. Use the existing API contracts
4. Gradually move to distributed architecture

This approach gives you the best of both worlds: modern technology stack with simplified deployment and operations, perfect for your current stage while maintaining flexibility for future growth.