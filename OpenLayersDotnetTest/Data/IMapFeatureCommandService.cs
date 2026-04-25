using OpenLayersDotnetTest.Contracts;

namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Executes write operations for map features.
/// </summary>
public interface IMapFeatureCommandService
{
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
}