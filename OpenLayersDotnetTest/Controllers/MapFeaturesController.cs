using Microsoft.AspNetCore.Mvc;
using OpenLayersDotnetTest.Contracts;
using OpenLayersDotnetTest.Data;

namespace OpenLayersDotnetTest.Controllers;

/// <summary>
/// Exposes GeoJSON feature data backed by PostGIS.
/// </summary>
[ApiController]
[Route("api/features")]
public sealed class MapFeaturesController(
    IMapFeatureQueryService queryService,
    IMapFeatureCommandService commandService) : ControllerBase
{
    private readonly IMapFeatureQueryService _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
    private readonly IMapFeatureCommandService _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

    /// <summary>
    /// Gets a single map feature as a GeoJSON document.
    /// </summary>
    /// <param name="id">The feature identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The GeoJSON feature payload.</returns>
    [HttpGet("{id:guid}")]
    [Produces("application/geo+json")]
    [ProducesResponseType<GeoJsonFeature>(StatusCodes.Status200OK, "application/geo+json")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GeoJsonFeature>> GetFeatureById(Guid id, CancellationToken cancellationToken)
    {
        var feature = await _queryService.GetFeatureByIdAsync(id, cancellationToken).ConfigureAwait(false);

        return feature is null
            ? NotFound()
            : CreateGeoJsonResult(feature, StatusCodes.Status200OK);
    }

    /// <summary>
    /// Gets all visible map features as a GeoJSON document.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The GeoJSON feature collection payload.</returns>
    [HttpGet]
    [Produces("application/geo+json")]
    [ProducesResponseType<GeoJsonFeatureCollection>(StatusCodes.Status200OK, "application/geo+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GeoJsonFeatureCollection>> GetFeatures(CancellationToken cancellationToken)
    {
        var featureCollection = await _queryService.GetFeatureCollectionAsync(cancellationToken).ConfigureAwait(false);

        return CreateGeoJsonResult(featureCollection, StatusCodes.Status200OK);
    }

    /// <summary>
    /// Creates a new map feature.
    /// </summary>
    /// <param name="request">The requested feature payload.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The created GeoJSON feature payload.</returns>
    [HttpPost]
    [Produces("application/geo+json")]
    [ProducesResponseType<GeoJsonFeature>(StatusCodes.Status201Created, "application/geo+json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GeoJsonFeature>> CreateFeature(
        [FromBody] MapFeatureUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var feature = await _commandService.CreateFeatureAsync(request, cancellationToken).ConfigureAwait(false);

            return CreateCreatedGeoJsonResult(feature);
        }
        catch (DuplicateFeatureSlugException exception)
        {
            return Conflict(CreateProblemDetails(StatusCodes.Status409Conflict, "Slug conflict", exception.Message));
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [exception.ParamName ?? "request"] = [exception.Message]
            }));
        }
    }

    /// <summary>
    /// Updates an existing map feature.
    /// </summary>
    /// <param name="id">The feature identifier.</param>
    /// <param name="request">The requested feature payload.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The updated GeoJSON feature payload.</returns>
    [HttpPut("{id:guid}")]
    [Produces("application/geo+json")]
    [ProducesResponseType<GeoJsonFeature>(StatusCodes.Status200OK, "application/geo+json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GeoJsonFeature>> UpdateFeature(
        Guid id,
        [FromBody] MapFeatureUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var feature = await _commandService.UpdateFeatureAsync(id, request, cancellationToken).ConfigureAwait(false);

            return feature is null
                ? NotFound()
                : CreateGeoJsonResult(feature, StatusCodes.Status200OK);
        }
        catch (DuplicateFeatureSlugException exception)
        {
            return Conflict(CreateProblemDetails(StatusCodes.Status409Conflict, "Slug conflict", exception.Message));
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [exception.ParamName ?? "request"] = [exception.Message]
            }));
        }
    }

    /// <summary>
    /// Deletes an existing map feature.
    /// </summary>
    /// <param name="id">The feature identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>No content when the feature is deleted.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFeature(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _commandService.DeleteFeatureAsync(id, cancellationToken).ConfigureAwait(false);

        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Gets a compact summary of feature counts grouped by category.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>A summary of available map features.</returns>
    [HttpGet("summary")]
    [ProducesResponseType<MapFeatureSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MapFeatureSummary>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _queryService.GetSummaryAsync(cancellationToken).ConfigureAwait(false);

        return Ok(summary);
    }

    private ActionResult<T> CreateGeoJsonResult<T>(T payload, int statusCode)
    {
        return new ObjectResult(payload)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/geo+json" }
        };
    }

    private ActionResult<GeoJsonFeature> CreateCreatedGeoJsonResult(GeoJsonFeature payload)
    {
        return new CreatedAtActionResult(nameof(GetFeatureById), ControllerContext.ActionDescriptor.ControllerName, new { id = payload.Id }, payload)
        {
            ContentTypes = { "application/geo+json" }
        };
    }

    private ProblemDetails CreateProblemDetails(int statusCode, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
    }
}