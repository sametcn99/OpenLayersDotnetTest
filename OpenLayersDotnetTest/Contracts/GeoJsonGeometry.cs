using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenLayersDotnetTest.Contracts;

/// <summary>
/// Represents a GeoJSON geometry.
/// </summary>
public sealed class GeoJsonGeometry
{
    /// <summary>
    /// Gets the geometry type.
    /// </summary>
    [Description("GeoJSON geometry type such as Point, LineString, or Polygon.")]
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Gets the geometry coordinates.
    /// </summary>
    [Description("Geometry coordinates encoded according to the GeoJSON specification for the given geometry type.")]
    [JsonPropertyName("coordinates")]
    public required JsonElement Coordinates { get; init; }
}