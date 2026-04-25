# OpenLayers .NET PostGIS Demo

This project is a simple Blazor Server integration/test application. Sample spatial data stored in PostgreSQL + PostGIS is served as GeoJSON through the .NET API layer and displayed in the browser with OpenLayers.

## Technologies

- .NET 10 / Blazor Server
- PostgreSQL + PostGIS
- Npgsql + EF Core
- GeoJSON
- OpenLayers
- Docker Compose

## Run With Docker

```bash
docker compose up --build
```

The default development workflow runs with `dotnet watch` inside the container. That means after `docker compose up`, Razor, C#, CSS, and JS changes under `OpenLayersDotnetTest` are automatically watched and rebuilt inside the application container.

The `bin` and `obj` folders are stored in named volumes inside the container. This prevents bind-mount-related permission issues and root-owned build artifacts on the Linux host from interfering with the development workflow.

Docker Compose's own watch feature is also configured. If you want, you can additionally use this command:

```bash
docker compose up --build --watch
```

Application: <http://localhost:5121> (if you change `APP_HOST_PORT`, use that same port here)

Scalar API documentation: <http://localhost:5121/scalar>

GeoJSON endpoint: <http://localhost:5121/api/features>

Health check: <http://localhost:5121/health>

To follow the logs:

```bash
docker compose logs -f app
```

To stop the services:

```bash
docker compose down
```

To remove the database volume and rerun the init scripts from scratch:

```bash
docker compose down -v --rmi all
docker compose up --build
```

## Project Structure

- `docker-compose.yml`: Manages the application and PostGIS services.
- `OpenLayersDotnetTest/Dockerfile`: Produces a `dotnet watch` development image and a publish image for deployment.
- `db/init`: PostGIS extension, table, index, and sample data scripts.
- `OpenLayersDotnetTest/Data`: EF Core-based query/command services, GeoJSON mappers, and the data access layer.
- `OpenLayersDotnetTest/wwwroot/js/map.js`: OpenLayers map integration.
- `OpenLayersDotnetTest/Components/Pages/Home.razor`: Single-screen map interface.

## Database Notes

When the PostGIS container starts for the first time, the scripts under `db/init` run in order. The `map_features` table uses the `geometry(Geometry, 4326)` type, has a GiST index for geometry, and a GIN index for the `metadata` field. The sample dataset includes point, line, and polygon geometries.

Docker Compose is intended for development; the default database password is only for local testing.
