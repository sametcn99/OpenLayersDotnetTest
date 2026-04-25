using System.ComponentModel;
using System.Text.Json.Serialization;

namespace OpenLayersDotnetTest.Contracts;

/// <summary>
/// Represents a GeoJSON feature.
/// </summary>
public sealed class GeoJsonFeature
{
    /// <summary>
    /// Gets the unique identifier of the feature.
    /// </summary>
    [Description("Unique feature identifier.")]
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the GeoJSON object type.
    /// </summary>
    [Description("GeoJSON object type. Always Feature.")]
    [JsonPropertyName("type")]
    public string Type { get; init; } = "Feature";

    /// <summary>
    /// Gets the GeoJSON geometry definition.
    /// </summary>
    [Description("GeoJSON geometry payload for the feature.")]
    [JsonPropertyName("geometry")]
    public required GeoJsonGeometry Geometry { get; init; }

    /// <summary>
    /// Gets the custom properties associated with the feature.
    /// </summary>
    [Description("Application-specific properties attached to the feature.")]
    [JsonPropertyName("properties")]
    public required MapFeatureProperties Properties { get; init; }
}