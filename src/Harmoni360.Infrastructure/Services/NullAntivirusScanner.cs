using System.IO;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Infrastructure.Services;

public class NullAntivirusScanner : IAntivirusScanner
{
    public Task ScanAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        // No-op scanner used in development environments
        return Task.CompletedTask;
    }
}
