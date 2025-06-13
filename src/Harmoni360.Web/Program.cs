using Harmoni360.Application;
using Harmoni360.Infrastructure;
using Harmoni360.Web.Middleware;
using Harmoni360.Web.Hubs;
using Harmoni360.Web.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Serilog;
using System.Text;
using Harmoni360.Infrastructure.Persistence;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Infrastructure.Services;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Log startup information for debugging
Console.WriteLine($"Starting Harmoni360 application...");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"ASPNETCORE_URLS: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");
Console.WriteLine($"Content Root: {builder.Environment.ContentRootPath}");

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

// Add services
builder.Services.AddControllers();

// Configure file upload size limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

// Configure form options for file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Harmoni360 API",
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

// Add module-based authorization system
builder.Services.AddModuleBasedAuthorization();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add memory caching for incident list
builder.Services.AddMemoryCache();

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Add SignalR notification services
builder.Services.AddScoped<ISecurityNotificationHub, SecurityNotificationHub>();

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

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"));

// Add Rate Limiting for API protection
builder.Services.AddRateLimiter(options =>
{
    // General API rate limit policy
    options.AddPolicy("ApiDefault", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Health API rate limit policy
    options.AddPolicy("HealthApi", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Stricter rate limit for health analytics endpoints
    options.AddPolicy("HealthAnalytics", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Emergency alerts have higher limits
    options.AddPolicy("HealthEmergency", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 50,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Health dashboard gets special treatment for real-time updates
    options.AddPolicy("HealthDashboard", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6 // 10-second segments
            }));

    // Default global policy
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 1000,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.RejectionStatusCode = 429;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            await context.HttpContext.Response.WriteAsync(
                $"Rate limit exceeded. Retry after {retryAfter.TotalSeconds} seconds.", token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsync("Rate limit exceeded.", token);
        }
    };
});

// Build app
var app = builder.Build();

// Set uploads path for file storage service
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? app.Environment.ContentRootPath, "uploads");
app.Configuration["FileStorage:UploadsPath"] = uploadsPath;

// Configure pipeline
// app.UseResponseCompression(); // Temporarily disabled to fix chunked encoding issue

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

// Only use HTTPS redirection in development - Fly.io handles HTTPS termination
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseSerilogRequestLogging();

// Add CORS for development
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCors");
}

// Serve static files for React app
app.UseStaticFiles();

// Create uploads directory if it doesn't exist (use the same path we set in configuration)
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    app.Logger.LogInformation("Created uploads directory at: {UploadsPath}", uploadsPath);
}

// Serve uploaded files (with authentication requirement)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Add cache control for uploaded files
        ctx.Context.Response.Headers["Cache-Control"] = "private, max-age=3600";
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseSpaStaticFiles();
}

app.UseRouting();

// Add custom middleware early in pipeline, but after routing for path detection
app.Use(async (context, next) =>
{
    // Only apply custom middleware to API and Hub routes
    if (context.Request.Path.StartsWithSegments("/api") || 
        context.Request.Path.StartsWithSegments("/hubs"))
    {
        var errorMiddleware = new ErrorHandlingMiddleware(async (ctx) => await next(ctx), 
            context.RequestServices.GetRequiredService<ILogger<ErrorHandlingMiddleware>>());
        await errorMiddleware.InvokeAsync(context);
    }
    else
    {
        await next(context);
    }
});

app.Use(async (context, next) =>
{
    // Only apply performance middleware to API routes (not static files)
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        var perfMiddleware = new PerformanceMiddleware(async (ctx) => await next(ctx), 
            context.RequestServices.GetRequiredService<ILogger<PerformanceMiddleware>>());
        await perfMiddleware.InvokeAsync(context);
    }
    else
    {
        await next(context);
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Map endpoints
app.MapControllers();

// SignalR hubs
app.MapHub<IncidentHub>("/hubs/incidents");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<HealthHub>("/hubs/health");
app.MapHub<SecurityHub>("/hubs/security");

// Health checks
app.MapHealthChecks("/health");

// Configure SPA
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";

    if (app.Environment.IsDevelopment())
    {
        // Only use SPA proxy if the frontend is not running separately
        // For separate frontend development, this will be handled by Vite proxy
        var useViteProxy = app.Configuration.GetValue<bool>("UseSpaProxy", false);

        if (useViteProxy)
        {
            spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
        }
    }
});

// Seed database if needed
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Checking database migrations...");

        // Check if database exists and create/migrate if needed
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed");
        }
        else
        {
            logger.LogInformation("Database is up to date");
        }

        // Seed database if tables exist using configuration
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            logger.LogInformation("Seeding database using configuration...");
            await seeder.SeedAsync();
            logger.LogInformation("Database seeding completed");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database");

        // Don't crash the app if migration fails - let the user run migrations manually
        // In production, we'll run migrations separately via flyctl ssh console
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
        else
        {
            logger.LogWarning("Database migration failed in production - continuing startup. Run migrations manually via: flyctl ssh console -C 'cd /app && dotnet ef database update'");
        }
    }
}

// Log final startup information
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Harmoni360 application starting...");
startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
startupLogger.LogInformation("URLs: {Urls}", Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

await app.RunAsync();