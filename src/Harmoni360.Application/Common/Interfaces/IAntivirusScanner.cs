using System.IO;
namespace Harmoni360.Application.Common.Interfaces;

public interface IAntivirusScanner
{
    Task ScanAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
