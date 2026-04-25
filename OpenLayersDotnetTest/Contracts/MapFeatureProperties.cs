using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenLayersDotnetTest.Contracts;

/// <summary>
/// Represents the application-specific properties attached to a GeoJSON feature.
/// </summary>
public sealed class MapFeatureProperties
{
    /// <summary>
    /// Gets the stable slug identifier.
    /// </summary>
    [Description("Stable slug identifier for the feature.")]
    [JsonPropertyName("slug")]
    public required string Slug { get; init; }

    /// <summary>
    /// Gets the human-readable feature name.
    /// </summary>
    [Description("Human-readable name shown in the UI.")]
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the logical category of the feature.
    /// </summary>
    [Description("Logical feature category such as landmark, route, or area.")]
    [JsonPropertyName("category")]
    public required string Category { get; init; }

    /// <summary>
    /// Gets the descriptive text shown for the feature.
    /// </summary>
    [Description("Feature description used by API clients and the map UI.")]
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets arbitrary metadata associated with the feature.
    /// </summary>
    [Description("Free-form metadata bag containing additional feature attributes.")]
    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, JsonElement> Metadata { get; init; } = new Dictionary<string, JsonElement>();

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    [Description("UTC timestamp when the feature record was created.")]
    [JsonPropertyName("createdAt")]
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the last update timestamp.
    /// </summary>
    [Description("UTC timestamp when the feature record was last updated.")]
    [JsonPropertyName("updatedAt")]
    public required DateTimeOffset UpdatedAt { get; init; }
}