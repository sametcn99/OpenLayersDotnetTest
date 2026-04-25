using System.ComponentModel;

namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Represents a compact feature summary for API clients.
/// </summary>
public sealed class MapFeatureSummary
{
    /// <summary>
    /// Gets the total number of map features.
    /// </summary>
    [Description("Total number of features currently exposed by the API.")]
    public required int TotalCount { get; init; }

    /// <summary>
    /// Gets feature counts keyed by category.
    /// </summary>
    [Description("Per-category feature counts keyed by feature category name.")]
    public required IReadOnlyDictionary<string, int> Categories { get; init; }
}
