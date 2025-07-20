using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Harmoni360.ElsaStudio.Services;

/// <summary>
/// Bridge service to render Elsa Studio components in React containers
/// </summary>
public class ElsaComponentBridge
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IServiceProvider _serviceProvider;
    private static IElsaAuthenticationService? _authService;

    public ElsaComponentBridge(IJSRuntime jsRuntime, IServiceProvider serviceProvider)
    {
        _jsRuntime = jsRuntime;
        _serviceProvider = serviceProvider;
        _authService = serviceProvider.GetService<IElsaAuthenticationService>();
    }

    /// <summary>
    /// Initialize the component bridge and make it available to JavaScript
    /// </summary>
    [JSInvokable]
    public static void Initialize()
    {
        Console.WriteLine("Elsa Component Bridge initialized");
    }

    /// <summary>
    /// Render workflow designer component in specified container
    /// </summary>
    [JSInvokable("renderWorkflowDesigner")]
    public static async Task RenderWorkflowDesigner(string containerId, string? workflowDefinitionId = null)
    {
        try
        {
            var parameters = new Dictionary<string, object?>();
            if (!string.IsNullOrEmpty(workflowDefinitionId))
            {
                parameters["WorkflowDefinitionId"] = workflowDefinitionId;
            }

            await InvokeComponentRender("WorkflowDesigner", containerId, parameters);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error rendering workflow designer: {ex.Message}");
        }
    }

    /// <summary>
    /// Render workflow instances component in specified container
    /// </summary>
    [JSInvokable("renderWorkflowInstances")]
    public static async Task RenderWorkflowInstances(string containerId)
    {
        try
        {
            await InvokeComponentRender("WorkflowInstances", containerId, new Dictionary<string, object?>());
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error rendering workflow instances: {ex.Message}");
        }
    }

    /// <summary>
    /// Render workflow dashboard component in specified container
    /// </summary>
    [JSInvokable("renderWorkflowDashboard")]
    public static async Task RenderWorkflowDashboard(string containerId)
    {
        try
        {
            await InvokeComponentRender("WorkflowDashboard", containerId, new Dictionary<string, object?>());
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error rendering workflow dashboard: {ex.Message}");
        }
    }

    /// <summary>
    /// Set authentication token for Blazor components
    /// </summary>
    [JSInvokable("setAuthToken")]
    public static async Task SetAuthToken(string token)
    {
        try
        {
            // Store auth token in Blazor app state for API calls
            if (_authService != null)
            {
                _authService.SetAuthToken(token);
                Console.WriteLine($"Auth token set in authentication service: {token[..Math.Min(token.Length, 20)]}...");
            }
            else
            {
                Console.Error.WriteLine("Authentication service not available");
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error setting auth token: {ex.Message}");
        }
    }

    /// <summary>
    /// Clean up component in specified container
    /// </summary>
    [JSInvokable("disposeComponent")]
    public static async Task DisposeComponent(string containerId)
    {
        try
        {
            // Clean up any resources or event handlers
            await Task.CompletedTask; // Placeholder for actual cleanup
            Console.WriteLine($"Component disposed: {containerId}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error disposing component: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method to invoke component rendering
    /// </summary>
    private static async Task InvokeComponentRender(string componentName, string containerId, Dictionary<string, object?> parameters)
    {
        // This would be implemented to actually render the component
        // For now, we'll create a placeholder that can be expanded
        var parametersJson = JsonSerializer.Serialize(parameters);
        Console.WriteLine($"Rendering {componentName} in {containerId} with parameters: {parametersJson}");
        
        // TODO: Implement actual component rendering logic
        // This might involve creating a new Blazor component host or using
        // the existing one to render specific components in target containers
        await Task.CompletedTask;
    }
}