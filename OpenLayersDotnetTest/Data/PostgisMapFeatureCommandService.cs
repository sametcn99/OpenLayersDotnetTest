using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using OpenLayersDotnetTest.Contracts;

namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Executes write operations for map features using EF Core and PostGIS.
/// </summary>
public sealed class PostgisMapFeatureCommandService(
    OpenLayersDbContext dbContext,
    IMapFeatureSlugUniquenessChecker slugUniquenessChecker,
    ILogger<PostgisMapFeatureCommandService> logger) : IMapFeatureCommandService
{
    private readonly OpenLayersDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IMapFeatureSlugUniquenessChecker _slugUniquenessChecker = slugUniquenessChecker ?? throw new ArgumentNullException(nameof(slugUniquenessChecker));
    private readonly ILogger<PostgisMapFeatureCommandService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<GeoJsonFeature> CreateFeatureAsync(MapFeatureUpsertRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await _slugUniquenessChecker.EnsureSlugIsUniqueAsync(request.Slug, null, cancellationToken).ConfigureAwait(false);

            var now = DateTimeOffset.UtcNow;
            var feature = new MapFeatureEntity
            {
                Id = Guid.NewGuid(),
                Slug = request.Slug,
                Name = request.Name,
                Category = request.Category,
                Description = request.Description,
                CreatedAt = now,
                UpdatedAt = now,
                Geometry = new Point(0, 0)
                {
                    SRID = 4326
                }
            };

            MapFeatureMapper.Apply(feature, request);

            _dbContext.MapFeatures.Add(feature);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return MapFeatureMapper.ToGeoJsonFeature(feature);
        }
        catch (DuplicateFeatureSlugException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not ArgumentException)
        {
            _logger.LogError(exception, "EF Core create feature command failed.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<GeoJsonFeature?> UpdateFeatureAsync(Guid id, MapFeatureUpsertRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var feature = await _dbContext.MapFeatures
                .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
                .ConfigureAwait(false);

            if (feature is null)
            {
                return null;
            }

            await _slugUniquenessChecker.EnsureSlugIsUniqueAsync(request.Slug, id, cancellationToken).ConfigureAwait(false);

            MapFeatureMapper.Apply(feature, request);
            feature.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return MapFeatureMapper.ToGeoJsonFeature(feature);
        }
        catch (DuplicateFeatureSlugException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not ArgumentException)
        {
            _logger.LogError(exception, "EF Core update feature command failed for feature {FeatureId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteFeatureAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var feature = await _dbContext.MapFeatures
                .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
                .ConfigureAwait(false);

            if (feature is null)
            {
                return false;
            }

            _dbContext.MapFeatures.Remove(feature);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "EF Core delete feature command failed for feature {FeatureId}.", id);
            throw;
        }
    }
}