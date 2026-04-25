using Microsoft.AspNetCore.Mvc;
using OpenLayersDotnetTest.Contracts;
using OpenLayersDotnetTest.Data;

namespace OpenLayersDotnetTest.Controllers;

/// <summary>
/// Provides lightweight service health diagnostics.
/// </summary>
[ApiController]
[Route("health")]
public sealed class HealthController(OpenLayersDbContext dbContext) : ControllerBase
{
    private readonly OpenLayersDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    /// <summary>
    /// Gets the application and database health status.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The current health snapshot.</returns>
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken cancellationToken)
    {
        var isConnected = await _dbContext.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new HealthResponse
        {
            Status = isConnected ? "Healthy" : "Unhealthy",
            Database = isConnected ? "Connected" : "Unavailable"
        });
    }
}