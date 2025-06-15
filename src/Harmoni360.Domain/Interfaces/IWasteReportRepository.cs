using Harmoni360.Domain.Entities.Waste;

namespace Harmoni360.Domain.Interfaces;

public interface IWasteReportRepository : IRepository<WasteReport>
{
    Task<WasteReport?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
