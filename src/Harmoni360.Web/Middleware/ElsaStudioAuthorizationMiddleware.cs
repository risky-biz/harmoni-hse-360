using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Harmoni360.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Harmoni360.Web.Middleware;

public class ElsaStudioAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ElsaStudioAuthorizationMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public ElsaStudioAuthorizationMiddleware(RequestDelegate next, ILogger<ElsaStudioAuthorizationMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the request is for Elsa Studio
        if (context.Request.Path.StartsWithSegments("/elsa-studio"))
        {
            // Skip authentication for Blazor framework files and static assets
            if (context.Request.Path.StartsWithSegments("/elsa-studio/_framework") ||
                context.Request.Path.StartsWithSegments("/elsa-studio/_content") ||
                context.Request.Path.Value?.Contains(".js") == true ||
                context.Request.Path.Value?.Contains(".css") == true ||
                context.Request.Path.Value?.Contains(".dat") == true ||
                context.Request.Path.Value?.Contains(".wasm") == true ||
                context.Request.Path.Value?.Contains(".dll") == true ||
                context.Request.Path.Value?.Contains(".pdb") == true ||
                context.Request.Path.Value?.Contains(".json") == true ||
                context.Request.Path.Value?.Contains(".ico") == true ||
                context.Request.Path.Value?.Contains(".png") == true ||
                context.Request.Path.Value?.Contains(".svg") == true ||
                context.Request.Path.Value?.Contains("favicon") == true ||
                context.Request.Path.Value?.EndsWith("/favicon.ico") == true ||
                context.Request.Path.Value?.EndsWith("/favicon.png") == true)
            {
                await _next(context);
                return;
            }

            // Extract JWT token from various sources
            string? token = null;
            
            // 1. Check Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }
            
            // 2. Check query parameter (for initial redirect)
            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Query["token"].FirstOrDefault();
            }
            
            // 3. Check cookies
            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Cookies["harmoni360_token"];
            }

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No JWT token found for Elsa Studio access attempt from {IP}", 
                    context.Connection.RemoteIpAddress);
                
                await WriteUnauthorizedResponse(context);
                return;
            }

            // Validate JWT token
            var (isValid, claims) = await ValidateJwtToken(token);
            if (!isValid || claims == null)
            {
                _logger.LogWarning("Invalid JWT token for Elsa Studio access attempt from {IP}", 
                    context.Connection.RemoteIpAddress);
                
                await WriteUnauthorizedResponse(context);
                return;
            }

            // Get user's roles from JWT claims
            var roles = claims.Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToArray();

            var userName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Unknown";

            // Check if user has SuperAdmin or Developer role
            var hasRequiredRole = roles.Contains(RoleType.SuperAdmin.ToString()) || 
                                 roles.Contains(RoleType.Developer.ToString());

            if (!hasRequiredRole)
            {
                _logger.LogWarning("Unauthorized access attempt to Elsa Studio by user {User} with roles {Roles}", 
                    userName, string.Join(", ", roles));
                
                // Return 403 Forbidden with a proper HTML response
                context.Response.StatusCode = 403;
                context.Response.ContentType = "text/html";
                
                var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Access Denied - Elsa Studio</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background-color: #f5f5f5;
        }
        .container {
            text-align: center;
            padding: 2rem;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            max-width: 500px;
        }
        h1 {
            color: #d32f2f;
            margin-bottom: 1rem;
        }
        p {
            color: #666;
            margin-bottom: 2rem;
        }
        .button {
            display: inline-block;
            padding: 0.75rem 1.5rem;
            background-color: #1976d2;
            color: white;
            text-decoration: none;
            border-radius: 4px;
            transition: background-color 0.3s;
        }
        .button:hover {
            background-color: #1565c0;
        }
        .icon {
            font-size: 4rem;
            color: #ffa726;
            margin-bottom: 1rem;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='icon'><i class='fa-solid fa-triangle-exclamation'></i></div>
        <h1>Elsa Studio Access Restricted</h1>
        <p>Access to Elsa Studio is restricted to System Administrators and Developers only.</p>
        <p>Your current role does not have permission to access this resource.</p>
        <a href='/dashboard' class='button'>Go to Dashboard</a>
    </div>
</body>
</html>";
                
                await context.Response.WriteAsync(html);
                return;
            }

            _logger.LogInformation("Authorized Elsa Studio access by user {User} with roles {Roles}", 
                userName, string.Join(", ", roles));
        }

        // User is authorized or not accessing Elsa Studio
        await _next(context);
    }

    private Task<(bool isValid, IEnumerable<Claim>? claims)> ValidateJwtToken(string token)
    {
        try
        {
            var jwtSecretKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtSecretKey))
            {
                _logger.LogError("JWT SecretKey not configured");
                return Task.FromResult<(bool isValid, IEnumerable<Claim>? claims)>((false, null));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtSecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return Task.FromResult((true, (IEnumerable<Claim>?)principal.Claims));
        }
        catch (Exception ex)
        {
            _logger.LogDebug("JWT token validation failed: {Error}", ex.Message);
            return Task.FromResult<(bool isValid, IEnumerable<Claim>? claims)>((false, null));
        }
    }

    private async Task WriteUnauthorizedResponse(HttpContext context)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "text/html";
        
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Authentication Required - Elsa Studio</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background-color: #f5f5f5;
        }
        .container {
            text-align: center;
            padding: 2rem;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            max-width: 500px;
        }
        h1 {
            color: #d32f2f;
            margin-bottom: 1rem;
        }
        p {
            color: #666;
            margin-bottom: 2rem;
        }
        .button {
            display: inline-block;
            padding: 0.75rem 1.5rem;
            background-color: #1976d2;
            color: white;
            text-decoration: none;
            border-radius: 4px;
            transition: background-color 0.3s;
        }
        .button:hover {
            background-color: #1565c0;
        }
        .icon {
            font-size: 4rem;
            color: #ffa726;
            margin-bottom: 1rem;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='icon'><i class=""fa-solid fa-lock""></i></div>
        <h1>Authentication Required</h1>
        <p>You need to be logged in to access Elsa Studio.</p>
        <p>Please log in to the main application first.</p>
        <a href='/login' class='button'>Go to Login</a>
    </div>
    <script>
        // Auto-redirect to login after 3 seconds
        setTimeout(() => {
            window.location.href = '/login';
        }, 3000);
    </script>
</body>
</html>";
        
        await context.Response.WriteAsync(html);
    }
}

// Extension method to register the middleware
public static class ElsaStudioAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseElsaStudioAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ElsaStudioAuthorizationMiddleware>();
    }
}