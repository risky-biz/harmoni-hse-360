using HarmoniHSE360.Application;
using HarmoniHSE360.Infrastructure;
using HarmoniHSE360.Web.Middleware;
using HarmoniHSE360.Web.Hubs;
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
using HarmoniHSE360.Infrastructure.Persistence;
using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Infrastructure.Services;

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
builder.Services.AddHealthChecks();

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

app.UseHttpsRedirection();
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
app.UseAuthentication();
app.UseAuthorization();

// Custom middleware - moved after static files to avoid interfering
// app.UseMiddleware<ErrorHandlingMiddleware>();
// app.UseMiddleware<PerformanceMiddleware>();

// Map endpoints
app.MapControllers();

// SignalR hubs
app.MapHub<IncidentHub>("/hubs/incidents");
app.MapHub<NotificationHub>("/hubs/notifications");

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
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

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

        // Only seed if in development and tables exist
        if (app.Environment.IsDevelopment())
        {
            var canConnect = await dbContext.Database.CanConnectAsync();
            if (canConnect)
            {
                var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
                logger.LogInformation("Seeding database with demo data...");
                await seeder.SeedAsync();
                logger.LogInformation("Database seeding completed");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database");

        // Don't crash the app if migration fails - let the user run migrations manually
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

await app.RunAsync();