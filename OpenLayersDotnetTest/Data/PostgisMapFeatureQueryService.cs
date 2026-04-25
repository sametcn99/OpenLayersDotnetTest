using Microsoft.EntityFrameworkCore;
using OpenLayersDotnetTest.Contracts;

namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Executes read operations for map features using EF Core and PostGIS.
/// </summary>
public sealed class PostgisMapFeatureQueryService(
    OpenLayersDbContext dbContext,
    ILogger<PostgisMapFeatureQueryService> logger) : IMapFeatureQueryService
{
    private readonly OpenLayersDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ILogger<PostgisMapFeatureQueryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<GeoJsonFeature?> GetFeatureByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var feature = await _dbContext.MapFeatures
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
                .ConfigureAwait(false);

            return feature is null ? null : MapFeatureMapper.ToGeoJsonFeature(feature);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "EF Core single feature query failed for feature {FeatureId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<GeoJsonFeatureCollection> GetFeatureCollectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var features = await _dbContext.MapFeatures
                .AsNoTracking()
                .OrderBy(feature => feature.SortOrder)
                .ThenBy(feature => feature.Name)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new GeoJsonFeatureCollection
            {
                Features = features.Select(MapFeatureMapper.ToGeoJsonFeature).ToArray()
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "EF Core GeoJSON query failed.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<MapFeatureSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _dbContext.MapFeatures
                .AsNoTracking()
                .GroupBy(feature => feature.Category)
                .OrderBy(group => group.Key)
                .Select(group => new
                {
                    Category = group.Key,
                    Count = group.Count()
                })
                .ToDictionaryAsync(item => item.Category, item => item.Count, cancellationToken)
                .ConfigureAwait(false);

            return new MapFeatureSummary
            {
                TotalCount = categories.Values.Sum(),
                Categories = categories
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "EF Core feature summary query failed.");
            throw;
        }
    }
}