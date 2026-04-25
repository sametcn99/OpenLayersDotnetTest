using NetTopologySuite.Geometries;

namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Entity Framework model for a persisted spatial feature.
/// </summary>
public sealed class MapFeatureEntity
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the stable slug.
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the ordering value.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the raw metadata JSON.
    /// </summary>
    public string MetadataJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the PostGIS geometry.
    /// </summary>
    public required Geometry Geometry { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}