using System.Text.Json;
using NetTopologySuite.Geometries;
using OpenLayersDotnetTest.Contracts;

namespace OpenLayersDotnetTest.Data;

/// <summary>
/// Converts map feature entities to and from API contracts.
/// </summary>
public static class MapFeatureMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Maps a persisted entity to a GeoJSON feature response.
    /// </summary>
    /// <param name="feature">The source entity.</param>
    /// <returns>The GeoJSON feature response.</returns>
    public static GeoJsonFeature ToGeoJsonFeature(MapFeatureEntity feature)
    {
        ArgumentNullException.ThrowIfNull(feature);

        return new GeoJsonFeature
        {
            Id = feature.Id,
            Geometry = ToGeoJsonGeometry(feature.Geometry),
            Properties = new MapFeatureProperties
            {
                Slug = feature.Slug,
                Name = feature.Name,
                Category = feature.Category,
                Description = feature.Description,
                Metadata = ParseMetadata(feature.MetadataJson),
                CreatedAt = feature.CreatedAt,
                UpdatedAt = feature.UpdatedAt
            }
        };
    }

    /// <summary>
    /// Applies the API request values to a feature entity.
    /// </summary>
    /// <param name="feature">The target entity.</param>
    /// <param name="request">The source request.</param>
    public static void Apply(MapFeatureEntity feature, MapFeatureUpsertRequest request)
    {
        ArgumentNullException.ThrowIfNull(feature);
        ArgumentNullException.ThrowIfNull(request);

        feature.Slug = NormalizeRequiredText(request.Slug, nameof(request.Slug));
        feature.Name = NormalizeRequiredText(request.Name, nameof(request.Name));
        feature.Category = NormalizeRequiredText(request.Category, nameof(request.Category));
        feature.Description = NormalizeRequiredText(request.Description, nameof(request.Description));
        feature.SortOrder = request.SortOrder;
        feature.MetadataJson = SerializeMetadata(request.Metadata);
        feature.Geometry = ToGeometry(request.Geometry);
    }

    private static string NormalizeRequiredText(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{propertyName} is required.", propertyName);
        }

        return value.Trim();
    }

    private static IReadOnlyDictionary<string, JsonElement> ParseMetadata(string metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return new Dictionary<string, JsonElement>();
        }

        using var document = JsonDocument.Parse(metadataJson);

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return new Dictionary<string, JsonElement>();
        }

        return document.RootElement
            .EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.OrdinalIgnoreCase);
    }

    private static string SerializeMetadata(IDictionary<string, JsonElement>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return "{}";
        }

        return JsonSerializer.Serialize(metadata, SerializerOptions);
    }

    private static GeoJsonGeometry ToGeoJsonGeometry(Geometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return geometry switch
        {
            Point point => CreateGeometry("Point", new[] { point.X, point.Y }),
            MultiPoint multiPoint => CreateGeometry(
                "MultiPoint",
                multiPoint.Geometries.Cast<Point>().Select(point => new[] { point.X, point.Y }).ToArray()),
            LineString lineString => CreateGeometry(
                "LineString",
                lineString.Coordinates.Select(coordinate => new[] { coordinate.X, coordinate.Y }).ToArray()),
            MultiLineString multiLineString => CreateGeometry(
                "MultiLineString",
                multiLineString.Geometries
                    .Cast<LineString>()
                    .Select(line => line.Coordinates.Select(coordinate => new[] { coordinate.X, coordinate.Y }).ToArray())
                    .ToArray()),
            Polygon polygon => CreateGeometry("Polygon", CreatePolygonCoordinates(polygon)),
            MultiPolygon multiPolygon => CreateGeometry(
                "MultiPolygon",
                multiPolygon.Geometries.Cast<Polygon>().Select(CreatePolygonCoordinates).ToArray()),
            _ => throw new NotSupportedException($"Geometry type '{geometry.GeometryType}' is not supported.")
        };
    }

    private static Geometry ToGeometry(GeoJsonGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var type = NormalizeRequiredText(geometry.Type, nameof(geometry.Type));

        return type switch
        {
            "Point" => CreatePoint(geometry.Coordinates),
            "MultiPoint" => CreateMultiPoint(geometry.Coordinates),
            "LineString" => CreateLineString(geometry.Coordinates),
            "MultiLineString" => CreateMultiLineString(geometry.Coordinates),
            "Polygon" => CreatePolygon(geometry.Coordinates),
            "MultiPolygon" => CreateMultiPolygon(geometry.Coordinates),
            _ => throw new ArgumentException($"Unsupported GeoJSON geometry type '{type}'.", nameof(geometry))
        };
    }

    private static Point CreatePoint(JsonElement coordinates)
    {
        var values = ReadPosition(coordinates);
        var point = new Point(values[0], values[1])
        {
            SRID = 4326
        };

        return point;
    }

    private static MultiPoint CreateMultiPoint(JsonElement coordinates)
    {
        var points = coordinates.EnumerateArray().Select(ReadPosition).Select(values => new Point(values[0], values[1]) { SRID = 4326 }).ToArray();

        return new MultiPoint(points)
        {
            SRID = 4326
        };
    }

    private static LineString CreateLineString(JsonElement coordinates)
    {
        var lineString = new LineString(ReadCoordinateArray(coordinates))
        {
            SRID = 4326
        };

        return lineString;
    }

    private static MultiLineString CreateMultiLineString(JsonElement coordinates)
    {
        var lines = coordinates
            .EnumerateArray()
            .Select(line => new LineString(ReadCoordinateArray(line)) { SRID = 4326 })
            .ToArray();

        return new MultiLineString(lines)
        {
            SRID = 4326
        };
    }

    private static Polygon CreatePolygon(JsonElement coordinates)
    {
        var rings = coordinates.EnumerateArray().Select(ReadCoordinateArray).ToArray();

        if (rings.Length == 0)
        {
            throw new ArgumentException("Polygon geometry must contain at least one linear ring.", nameof(coordinates));
        }

        var shell = new LinearRing(rings[0])
        {
            SRID = 4326
        };

        var holes = rings.Skip(1).Select(ring => new LinearRing(ring) { SRID = 4326 }).ToArray();

        return new Polygon(shell, holes)
        {
            SRID = 4326
        };
    }

    private static MultiPolygon CreateMultiPolygon(JsonElement coordinates)
    {
        var polygons = coordinates.EnumerateArray().Select(CreatePolygon).ToArray();

        return new MultiPolygon(polygons)
        {
            SRID = 4326
        };
    }

    private static Coordinate[] ReadCoordinateArray(JsonElement coordinates)
    {
        return coordinates.EnumerateArray().Select(ReadPosition).Select(values => new Coordinate(values[0], values[1])).ToArray();
    }

    private static double[] ReadPosition(JsonElement position)
    {
        if (position.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException("GeoJSON coordinate position must be an array.", nameof(position));
        }

        var values = position.EnumerateArray().Select(ReadNumber).ToArray();

        if (values.Length < 2)
        {
            throw new ArgumentException("GeoJSON coordinate position must contain longitude and latitude values.", nameof(position));
        }

        return values;
    }

    private static double ReadNumber(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new ArgumentException("GeoJSON coordinates must be numeric values.", nameof(value));
        }

        return value.GetDouble();
    }

    private static double[][][] CreatePolygonCoordinates(Polygon polygon)
    {
        var rings = new List<double[][]>
        {
            polygon.ExteriorRing.Coordinates.Select(coordinate => new[] { coordinate.X, coordinate.Y }).ToArray()
        };

        rings.AddRange(polygon.Holes.Select(hole => hole.Coordinates.Select(coordinate => new[] { coordinate.X, coordinate.Y }).ToArray()));

        return rings.ToArray();
    }

    private static GeoJsonGeometry CreateGeometry(string type, object coordinates)
    {
        return new GeoJsonGeometry
        {
            Type = type,
            Coordinates = JsonSerializer.SerializeToElement(coordinates, SerializerOptions)
        };
    }
}