using Harmoni360.Domain.Entities.Waste;
using Harmoni360.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Infrastructure.Persistence.Repositories;

public class WasteReportRepository : Repository<WasteReport>, IWasteReportRepository
{
    public WasteReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<WasteReport?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Reporter)
            .Include(w => w.Attachments)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }
}
