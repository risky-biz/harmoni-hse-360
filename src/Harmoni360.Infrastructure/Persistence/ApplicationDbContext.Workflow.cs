using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Infrastructure.Persistence;

public partial class ApplicationDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Elsa 3.4.2 with EntityFrameworkCore automatically configures tables
        // No manual configuration required - tables are created through migrations
    }
}