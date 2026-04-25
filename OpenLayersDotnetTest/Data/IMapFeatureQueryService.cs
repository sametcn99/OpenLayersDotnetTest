using OpenLayersDotnetTest.Contracts;

namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Executes read operations for map features.
/// </summary>
public interface IMapFeatureQueryService
{
    /// <summary>
    /// Gets a single map feature by its identifier.
    /// </summary>
    /// <param name="id">The feature identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the database request.</param>
    /// <returns>The GeoJSON feature if found; otherwise <see langword="null" />.</returns>
    Task<GeoJsonFeature?> GetFeatureByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all visible map features as a GeoJSON FeatureCollection document.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the database request.</param>
    /// <returns>A GeoJSON FeatureCollection response model.</returns>
    Task<GeoJsonFeatureCollection> GetFeatureCollectionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a small category summary for the available map features.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the database request.</param>
    /// <returns>A summary of feature counts grouped by category.</returns>
    Task<MapFeatureSummary> GetSummaryAsync(CancellationToken cancellationToken);
}