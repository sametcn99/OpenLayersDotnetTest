using System.ComponentModel.DataAnnotations;

namespace OpenLayersDotnetTest.Configuration;

/// <summary>
/// Provides strongly typed database connection settings.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// The configuration section name for database settings.
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// Gets the PostgreSQL connection string used by Npgsql.
    /// </summary>
    [Required]
    public string ConnectionString { get; init; } = string.Empty;
}
