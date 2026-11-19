using Microsoft.EntityFrameworkCore;
using Seismic.ReportsService.DataAccess.Entities;

namespace Seismic.ReportsService.DataAccess;

public class ReportsDbContext : DbContext
{
    public ReportsDbContext(DbContextOptions<ReportsDbContext> options) : base(options)
    {
    }

    public DbSet<Report> Reports => Set<Report>();
}