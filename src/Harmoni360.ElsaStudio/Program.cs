using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Elsa.Studio.Core.BlazorWasm.Extensions;
using Elsa.Studio.Shell.Extensions;
using Elsa.Studio.Login.BlazorWasm.Extensions;
using Elsa.Studio.Workflows.Extensions;
using Elsa.Studio.Dashboard.Extensions;
using Elsa.Studio.Workflows.Designer.Extensions;
using Elsa.Studio.Contracts;
using Elsa.Studio.Extensions;
using Elsa.Studio.Models;
using Elsa.Studio.Shell;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Harmoni360.ElsaStudio.JsonContext;
using Refit;

// Build the host.
var builder = WebAssemblyHostBuilder.CreateDefault(args);
var configuration = builder.Configuration;

// Register root components.
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.RootComponents.RegisterCustomElsaStudioElements();

// Configure JSON serialization to use only source generation
// This avoids the NullabilityInfoContext_NotSupported error in WebAssembly
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    // Use only the source-generated context - no fallback to reflection
    TypeInfoResolver = ElsaStudioJsonContext.Default
};

// Configure all HTTP clients to use the custom JSON options
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.ConfigureHttpClient(client =>
    {
        client.DefaultRequestHeaders.Add("User-Agent", "Harmoni360-ElsaStudio/1.0");
        client.Timeout = TimeSpan.FromSeconds(30);
    });
});

// Configure JSON serializer options globally
builder.Services.AddSingleton(jsonOptions);
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.TypeInfoResolver = ElsaStudioJsonContext.Default;
});


// Register shell services and modules.
var backendApiConfig = new BackendApiConfig
{
    ConfigureBackendOptions = options =>
    {
        // Get the base URL from configuration
        configuration.GetSection("Backend").Bind(options);
        
        // If the URL is relative, make it absolute based on the current location
        var urlString = options.Url?.ToString() ?? "";
        if (!string.IsNullOrEmpty(urlString) && !urlString.StartsWith("http"))
        {
            var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
            var absoluteUrl = new Uri(baseUri, urlString);
            options.Url = absoluteUrl;
        }
    }
};

builder.Services.AddCore();
builder.Services.AddShell(opt =>
{
    opt.DisableAuthorization = true;
});
builder.Services.AddRemoteBackend(backendApiConfig);
builder.Services.AddLoginModule();
builder.Services.AddDashboardModule();
builder.Services.AddWorkflowsModule();

// Build the application.
var app = builder.Build();

// Run startup tasks
var startupTaskRunner = app.Services.GetRequiredService<IStartupTaskRunner>();
await startupTaskRunner.RunStartupTasksAsync();

// Run the application.
await app.RunAsync();