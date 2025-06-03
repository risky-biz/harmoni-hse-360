using MediatR;
using Microsoft.Extensions.Logging;

namespace HarmoniHSE360.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid().ToString();

        _logger.LogInformation("Handling {RequestName} [{RequestGuid}]", requestName, requestGuid);

        TResponse response;

        try
        {
            response = await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request {RequestName} [{RequestGuid}] failed", requestName, requestGuid);
            throw;
        }

        _logger.LogInformation("Request {RequestName} [{RequestGuid}] completed", requestName, requestGuid);

        return response;
    }
}