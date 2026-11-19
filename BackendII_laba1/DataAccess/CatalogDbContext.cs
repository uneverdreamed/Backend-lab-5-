using Microsoft.EntityFrameworkCore;
using Seismic.CatalogService.DataAccess.Entities;

namespace Seismic.CatalogService.DataAccess;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Earthquake> Earthquakes => Set<Earthquake>();

    public DbSet<SeismicStation> SeismicStations => Set<SeismicStation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SeismicStation>()
            .HasIndex(s => s.Code)
            .IsUnique();

        modelBuilder.Entity<Earthquake>()
            .HasIndex(e => e.ExternalId)
            .IsUnique();

        modelBuilder.Entity<Earthquake>()
            .HasOne(e => e.NearestStation)
            .WithMany(s => s.Earthquakes)
            .HasForeignKey(e => e.NearestStationId)
            .OnDelete(DeleteBehavior.SetNull);

        Seed(modelBuilder);
    }

    private static DateTime Utc(int y, int m, int d, int hh, int mm)
        => new(y, m, d, hh, mm, 0, DateTimeKind.Utc);

    private static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SeismicStation>().HasData(
            new SeismicStation { Id = 1, Code = "ANN", Name = "Анапа", Latitude = 44.881, Longitude = 37.314, Region = "Краснодарский край", Organization = "ФИЦ ЕГС РАН (ЦО, г. Обнинск)" },
            new SeismicStation { Id = 2, Code = "NVPR", Name = "Новополтавский", Latitude = 46.877, Longitude = 39.14, Region = "Краснодарский край", Organization = "ФИЦ ЕГС РАН (ЦО, г. Обнинск)" },
            new SeismicStation { Id = 3, Code = "ALID", Name = "Алаид", Latitude = 50.8756, Longitude = 155.552, Region = "Камчатский край", Organization = "ФИЦ ЕГС РАН (ЦО, г. Обнинск)" },
            new SeismicStation { Id = 4, Code = "KDTR", Name = "Ходутка", Latitude = 51.809, Longitude = 158.077, Region = "Камчатский край", Organization = "ФИЦ ЕГС РАН (Камчатский филлиал)" },
            new SeismicStation { Id = 5, Code = "TLKV", Name = "Толкаевка", Latitude = 52.554, Longitude = 53.280, Region = "Оренбургская область", Organization = "ФИЦ ЕГС РАН (ЦО, г. Обнинск)" },
            new SeismicStation { Id = 6, Code = "KIV", Name = "Кисловодск", Latitude = 43.955, Longitude = 42.686, Region = "Ставропольский край", Organization = "ФИЦ ЕГС РАН (ЦО, г. Обнинск)" },
            new SeismicStation { Id = 7, Code = "GAK", Name = "Горно-Алтайск", Latitude = 51.958, Longitude = 85.960, Region = "Республика Алтай", Organization = "ФИЦ ЕГС РАН (ЦО, г. Обнинск)" },
            new SeismicStation { Id = 8, Code = "PR1R", Name = "Романово", Latitude = 59.1846, Longitude = 56.7446, Region = "Пермский край", Organization = "ФИЦ ЕГС РАН (ЦО, г. Обнинск)" }
        );

        modelBuilder.Entity<Earthquake>().HasData(
            new Earthquake { Id = 1, ExternalId = "USGS-2025a", EpicenterLat = 52.229, EpicenterLon = 158.627, Depth = 45.0, Magnitude = 6.2, OccurredAt = Utc(2025, 1, 14, 22, 17), RegionName = "Камчатский край", Source = "USGS", NearestStationId = 4, DistanceToStationKm = 59.98 },
            new Earthquake { Id = 2, ExternalId = "EMSC-2025b", EpicenterLat = 50.526, EpicenterLon = 156.032, Depth = 58.0, Magnitude = 5.4, OccurredAt = Utc(2025, 2, 3, 9, 41), RegionName = "Камчатский край", Source = "EMSC", NearestStationId = 3, DistanceToStationKm = 51.52 },
            new Earthquake { Id = 3, ExternalId = "ATLAS-2025c", EpicenterLat = 51.509, EpicenterLon = 157.677, Depth = 35.0, Magnitude = 4.8, OccurredAt = Utc(2025, 2, 20, 14, 5), RegionName = "Камчатский край", Source = "GS RAS", NearestStationId = 4, DistanceToStationKm = 43.29 },
            new Earthquake { Id = 4, ExternalId = "USGS-2025d", EpicenterLat = 51.376, EpicenterLon = 155.202, Depth = 60.0, Magnitude = 6.7, OccurredAt = Utc(2025, 3, 11, 3, 28), RegionName = "Камчатский край", Source = "USGS", NearestStationId = 3, DistanceToStationKm = 60.77 },
            new Earthquake { Id = 5, ExternalId = "EMSC-2025e", EpicenterLat = 44.235, EpicenterLon = 43.016, Depth = 12.0, Magnitude = 5.1, OccurredAt = Utc(2025, 3, 29, 18, 52), RegionName = "Ставропольский край", Source = "EMSC", NearestStationId = 6, DistanceToStationKm = 40.79 },
            new Earthquake { Id = 6, ExternalId = "GS-2025f", EpicenterLat = 43.645, EpicenterLon = 42.396, Depth = 28.0, Magnitude = 4.3, OccurredAt = Utc(2025, 4, 15, 6, 14), RegionName = "Ставропольский край", Source = "GS RAS", NearestStationId = 6, DistanceToStationKm = 41.59 },
            new Earthquake { Id = 7, ExternalId = "GS-2025g", EpicenterLat = 45.231, EpicenterLon = 37.714, Depth = 15.0, Magnitude = 4.9, OccurredAt = Utc(2025, 4, 27, 23, 9), RegionName = "Краснодарский край", Source = "GS RAS", NearestStationId = 1, DistanceToStationKm = 50.02 },
            new Earthquake { Id = 8, ExternalId = "EMSC-2025h", EpicenterLat = 44.621, EpicenterLon = 37.614, Depth = 10.0, Magnitude = 3.8, OccurredAt = Utc(2025, 5, 6, 11, 33), RegionName = "Краснодарский край", Source = "EMSC", NearestStationId = 1, DistanceToStationKm = 37.38 },
            new Earthquake { Id = 9, ExternalId = "GS-2025i", EpicenterLat = 47.207, EpicenterLon = 38.77, Depth = 18.0, Magnitude = 5.6, OccurredAt = Utc(2025, 5, 22, 2, 47), RegionName = "Краснодарский край", Source = "GS RAS", NearestStationId = 2, DistanceToStationKm = 46.18 },
            new Earthquake { Id = 10, ExternalId = "GS-2025j", EpicenterLat = 52.358, EpicenterLon = 86.4, Depth = 9.0, Magnitude = 4.5, OccurredAt = Utc(2025, 6, 9, 16, 20), RegionName = "Республика Алтай", Source = "GS RAS", NearestStationId = 7, DistanceToStationKm = 53.66 },
            new Earthquake { Id = 11, ExternalId = "EMSC-2025k", EpicenterLat = 51.668, EpicenterLon = 85.62, Depth = 12.0, Magnitude = 3.9, OccurredAt = Utc(2025, 6, 25, 20, 5), RegionName = "Республика Алтай", Source = "EMSC", NearestStationId = 7, DistanceToStationKm = 39.83 },
            new Earthquake { Id = 12, ExternalId = "USGS-2025l", EpicenterLat = 52.854, EpicenterLon = 53.64, Depth = 8.0, Magnitude = 3.6, OccurredAt = Utc(2025, 7, 8, 4, 55), RegionName = "Оренбургская область", Source = "USGS", NearestStationId = 5, DistanceToStationKm = 41.24 },
            new Earthquake { Id = 13, ExternalId = "EMSC-2025m", EpicenterLat = 52.284, EpicenterLon = 52.97, Depth = 11.0, Magnitude = 3.9, OccurredAt = Utc(2025, 7, 21, 13, 12), RegionName = "Оренбургская область", Source = "EMSC", NearestStationId = 5, DistanceToStationKm = 36.65 },
            new Earthquake { Id = 14, ExternalId = "USGS-2025n", EpicenterLat = 59.525, EpicenterLon = 57.165, Depth = 22.0, Magnitude = 3.5, OccurredAt = Utc(2025, 8, 3, 7, 38), RegionName = "Пермский край", Source = "USGS", NearestStationId = 8, DistanceToStationKm = 44.73 },
            new Earthquake { Id = 15, ExternalId = "GS-2025o", EpicenterLat = 51.426, EpicenterLon = 156.052, Depth = 50.0, Magnitude = 7.1, OccurredAt = Utc(2025, 8, 19, 19, 3), RegionName = "Камчатский край", Source = "GS RAS", NearestStationId = 3, DistanceToStationKm = 70.44 },
            new Earthquake { Id = 16, ExternalId = "EMSC-2025p", EpicenterLat = 46.577, EpicenterLon = 39.49, Depth = 40.0, Magnitude = 4.6, OccurredAt = Utc(2025, 9, 2, 10, 26), RegionName = "Краснодарский край", Source = "EMSC", NearestStationId = 2, DistanceToStationKm = 42.71 }
        );
    }
}