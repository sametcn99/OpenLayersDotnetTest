using System.ComponentModel;
using System.Text.Json.Serialization;

namespace OpenLayersDotnetTest.Contracts;

/// <summary>
/// Represents a GeoJSON feature collection response.
/// </summary>
public sealed class GeoJsonFeatureCollection
{
    /// <summary>
    /// Gets the GeoJSON object type.
    /// </summary>
    [Description("GeoJSON top-level object type. Always FeatureCollection.")]
    [JsonPropertyName("type")]
    public string Type { get; init; } = "FeatureCollection";

    /// <summary>
    /// Gets the features contained in the collection.
    /// </summary>
    [Description("GeoJSON features returned by the API.")]
    [JsonPropertyName("features")]
    public required IReadOnlyList<GeoJsonFeature> Features { get; init; }
}