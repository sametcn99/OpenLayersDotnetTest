using System.ComponentModel;

namespace OpenLayersDotnetTest.Contracts;

/// <summary>
/// Represents the response payload for the health endpoint.
/// </summary>
public sealed class HealthResponse
{
    /// <summary>
    /// Gets the overall application health status.
    /// </summary>
    [Description("Overall application health status.")]
    public required string Status { get; init; }

    /// <summary>
    /// Gets the database connectivity status.
    /// </summary>
    [Description("Database connectivity state as seen by the API process.")]
    public required string Database { get; init; }
}