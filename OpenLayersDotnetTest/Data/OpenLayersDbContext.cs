using Microsoft.EntityFrameworkCore;

namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Entity Framework Core database context for the geospatial demo application.
/// </summary>
public sealed class OpenLayersDbContext(DbContextOptions<OpenLayersDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the spatial features stored in PostGIS.
    /// </summary>
    public DbSet<MapFeatureEntity> MapFeatures => Set<MapFeatureEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var entity = modelBuilder.Entity<MapFeatureEntity>();

        entity.ToTable("map_features", "public");
        entity.HasKey(feature => feature.Id);

        entity.Property(feature => feature.Id)
            .HasColumnName("id");

        entity.Property(feature => feature.Slug)
            .HasColumnName("slug");

        entity.Property(feature => feature.Name)
            .HasColumnName("name");

        entity.Property(feature => feature.Category)
            .HasColumnName("category");

        entity.Property(feature => feature.Description)
            .HasColumnName("description");

        entity.Property(feature => feature.SortOrder)
            .HasColumnName("sort_order");

        entity.Property(feature => feature.MetadataJson)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        entity.Property(feature => feature.Geometry)
            .HasColumnName("geom")
            .HasColumnType("geometry(Geometry,4326)");

        entity.Property(feature => feature.CreatedAt)
            .HasColumnName("created_at");

        entity.Property(feature => feature.UpdatedAt)
            .HasColumnName("updated_at");
    }
}