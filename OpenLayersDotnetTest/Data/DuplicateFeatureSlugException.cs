namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Represents a duplicate feature slug conflict.
/// </summary>
public sealed class DuplicateFeatureSlugException(string slug)
    : InvalidOperationException($"A feature with slug '{slug}' already exists.")
{
    /// <summary>
    /// Gets the conflicting slug value.
    /// </summary>
    public string Slug { get; } = slug;
}