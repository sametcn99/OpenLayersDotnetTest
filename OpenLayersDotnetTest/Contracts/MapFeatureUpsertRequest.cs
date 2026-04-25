using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenLayersDotnetTest.Contracts;

/// <summary>
/// Represents the request payload used to create or update a map feature.
/// </summary>
public sealed class MapFeatureUpsertRequest
{
    /// <summary>
    /// Gets the stable slug identifier.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [Description("Stable slug identifier for the feature.")]
    [JsonPropertyName("slug")]
    public required string Slug { get; init; }

    /// <summary>
    /// Gets the human-readable feature name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [Description("Human-readable name shown in the UI.")]
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the logical feature category.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [Description("Logical feature category such as landmark, route, or area.")]
    [JsonPropertyName("category")]
    public required string Category { get; init; }

    /// <summary>
    /// Gets the descriptive text shown for the feature.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [Description("Feature description used by API clients and the map UI.")]
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets the display ordering value.
    /// </summary>
    [Range(0, int.MaxValue)]
    [Description("Display ordering for the feature.")]
    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; init; }

    /// <summary>
    /// Gets arbitrary metadata associated with the feature.
    /// </summary>
    [Description("Free-form metadata bag containing additional feature attributes.")]
    [JsonPropertyName("metadata")]
    public IDictionary<string, JsonElement> Metadata { get; init; } = new Dictionary<string, JsonElement>();

    /// <summary>
    /// Gets the GeoJSON geometry payload.
    /// </summary>
    [Required]
    [Description("GeoJSON geometry payload for the feature.")]
    [JsonPropertyName("geometry")]
    public required GeoJsonGeometry Geometry { get; init; }
}