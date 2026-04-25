using Microsoft.EntityFrameworkCore;

namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Validates feature slug uniqueness against PostGIS-backed storage.
/// </summary>
public sealed class MapFeatureSlugUniquenessChecker(OpenLayersDbContext dbContext) : IMapFeatureSlugUniquenessChecker
{
    private readonly OpenLayersDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    /// <inheritdoc />
    public async Task EnsureSlugIsUniqueAsync(string slug, Guid? excludedFeatureId, CancellationToken cancellationToken)
    {
        var normalizedSlug = slug.Trim();
        var slugExists = await _dbContext.MapFeatures
            .AsNoTracking()
            .AnyAsync(
                feature => feature.Slug == normalizedSlug && (!excludedFeatureId.HasValue || feature.Id != excludedFeatureId.Value),
                cancellationToken)
            .ConfigureAwait(false);

        if (slugExists)
        {
            throw new DuplicateFeatureSlugException(normalizedSlug);
        }
    }
}