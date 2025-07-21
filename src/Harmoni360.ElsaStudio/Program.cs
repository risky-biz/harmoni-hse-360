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

// Build the host.
var builder = WebAssemblyHostBuilder.CreateDefault(args);
var configuration = builder.Configuration;

// Register root components.
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.RootComponents.RegisterCustomElsaStudioElements();

// Note: Configuring JSON options to avoid NullabilityInfoContext issues in WebAssembly
// This will be handled by Elsa's internal configuration

// Configure HTTP client defaults
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.ConfigureHttpClient(client =>
    {
        client.DefaultRequestHeaders.Add("User-Agent", "Harmoni360-ElsaStudio/1.0");
    });
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

// Run each startup task.
var startupTaskRunner = app.Services.GetRequiredService<IStartupTaskRunner>();
await startupTaskRunner.RunStartupTasksAsync();

// Run the application.
await app.RunAsync();