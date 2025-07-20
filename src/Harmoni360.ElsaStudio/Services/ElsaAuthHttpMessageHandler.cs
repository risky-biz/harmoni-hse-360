using System.Net.Http.Headers;

namespace Harmoni360.ElsaStudio.Services;

public class ElsaAuthHttpMessageHandler : DelegatingHandler
{
    private readonly IElsaAuthenticationService _authService;

    public ElsaAuthHttpMessageHandler(IElsaAuthenticationService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Add auth token to request
        if (!string.IsNullOrEmpty(_authService.AuthToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.AuthToken);
            Console.WriteLine($"Added auth token to request: {request.RequestUri}");
        }

        // Add CORS headers
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine($"Unauthorized response from Elsa API: {request.RequestUri}");
            }

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling Elsa API: {request.RequestUri} - {ex.Message}");
            throw;
        }
    }
}