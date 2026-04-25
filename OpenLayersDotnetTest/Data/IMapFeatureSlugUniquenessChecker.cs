namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Validates that feature slugs remain unique within the data store.
/// </summary>
public interface IMapFeatureSlugUniquenessChecker
{
    /// <summary>
    /// Ensures the provided slug is unique.
    /// </summary>
    /// <param name="slug">The slug to validate.</param>
    /// <param name="excludedFeatureId">An optional feature identifier to exclude from the uniqueness check.</param>
    /// <param name="cancellationToken">A token used to cancel the database request.</param>
    /// <returns>A task that completes when validation succeeds.</returns>
    Task EnsureSlugIsUniqueAsync(string slug, Guid? excludedFeatureId, CancellationToken cancellationToken);
}