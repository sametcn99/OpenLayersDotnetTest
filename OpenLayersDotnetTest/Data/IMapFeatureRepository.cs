using OpenLayersDotnetTest.Contracts;

namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Reads map features from the spatial data store.
/// </summary>
public interface IMapFeatureRepository
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
    /// Creates a new map feature.
    /// </summary>
    /// <param name="request">The requested feature payload.</param>
    /// <param name="cancellationToken">A token used to cancel the database request.</param>
    /// <returns>The created GeoJSON feature.</returns>
    Task<GeoJsonFeature> CreateFeatureAsync(MapFeatureUpsertRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing map feature.
    /// </summary>
    /// <param name="id">The feature identifier.</param>
    /// <param name="request">The requested feature payload.</param>
    /// <param name="cancellationToken">A token used to cancel the database request.</param>
    /// <returns>The updated GeoJSON feature if found; otherwise <see langword="null" />.</returns>
    Task<GeoJsonFeature?> UpdateFeatureAsync(Guid id, MapFeatureUpsertRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a map feature.
    /// </summary>
    /// <param name="id">The feature identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the database request.</param>
    /// <returns><see langword="true" /> when the feature was deleted; otherwise <see langword="false" />.</returns>
    Task<bool> DeleteFeatureAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a small category summary for the available map features.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the database request.</param>
    /// <returns>A summary of feature counts grouped by category.</returns>
    Task<MapFeatureSummary> GetSummaryAsync(CancellationToken cancellationToken);
}
