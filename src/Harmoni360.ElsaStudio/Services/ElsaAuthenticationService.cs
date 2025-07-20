using System.Net.Http.Headers;

namespace Harmoni360.ElsaStudio.Services;

public interface IElsaAuthenticationService
{
    string? AuthToken { get; set; }
    void SetAuthToken(string token);
    void ConfigureHttpClient(HttpClient httpClient);
}

public class ElsaAuthenticationService : IElsaAuthenticationService
{
    private string? _authToken;

    public string? AuthToken 
    { 
        get => _authToken; 
        set => _authToken = value; 
    }

    public void SetAuthToken(string token)
    {
        _authToken = token;
        Console.WriteLine($"Auth token updated: {token[..Math.Min(token.Length, 20)]}...");
    }

    public void ConfigureHttpClient(HttpClient httpClient)
    {
        if (!string.IsNullOrEmpty(_authToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _authToken);
        }
    }
}