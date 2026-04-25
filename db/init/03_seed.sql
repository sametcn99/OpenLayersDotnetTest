INSERT INTO public.map_features (
    slug,
    name,
    category,
    description,
    sort_order,
    metadata,
    geom
)
VALUES
    (
        'galata-tower',
        'Galata Tower',
        'landmark',
        'Historic stone tower north of the Golden Horn.',
        10,
        '{"style":"pin","district":"Beyoglu"}'::jsonb,
        ST_SetSRID(ST_MakePoint(28.974167, 41.025694), 4326)
    ),
    (
        'taksim-square',
        'Taksim Square',
        'landmark',
        'Major civic square and transit point in Istanbul.',
        20,
        '{"style":"pin","district":"Beyoglu"}'::jsonb,
        ST_SetSRID(ST_MakePoint(28.985000, 41.036900), 4326)
    ),
    (
        'bosphorus-demo-route',
        'Bosphorus Demo Route',
        'route',
        'Sample line feature following a ferry-like path on the Bosphorus.',
        30,
        '{"style":"route","mode":"ferry"}'::jsonb,
        ST_GeomFromText('LINESTRING(28.949700 41.039000, 29.014400 41.043900, 29.041000 41.085000, 29.056800 41.107000)', 4326)
    ),
    (
        'kadikoy-coast-walk',
        'Kadikoy Coast Walk',
        'route',
        'Short sample walking line along the Kadikoy shoreline.',
        40,
        '{"style":"route","mode":"walk"}'::jsonb,
        ST_GeomFromText('LINESTRING(29.018000 40.990000, 29.025000 40.992500, 29.037000 40.994000)', 4326)
    ),
    (
        'sultanahmet-demo-area',
        'Sultanahmet Demo Area',
        'area',
        'Compact polygon around the historic peninsula test area.',
        50,
        '{"style":"area","district":"Fatih"}'::jsonb,
        ST_GeomFromText('POLYGON((28.974600 41.008500, 28.980000 41.008900, 28.980400 41.006000, 28.975000 41.005600, 28.974600 41.008500))', 4326)
    ),
    (
        'belgrad-forest-demo-area',
        'Belgrad Forest Demo Area',
        'area',
        'Larger sample polygon used for map fitting and area styling.',
        60,
        '{"style":"area","district":"Sariyer"}'::jsonb,
        ST_GeomFromText('POLYGON((28.925000 41.191000, 28.997000 41.192000, 29.005000 41.160000, 28.930000 41.155000, 28.925000 41.191000))', 4326)
    )
ON CONFLICT (slug) DO UPDATE
SET
    name = EXCLUDED.name,
    category = EXCLUDED.category,
    description = EXCLUDED.description,
    sort_order = EXCLUDED.sort_order,
    metadata = EXCLUDED.metadata,
    geom = EXCLUDED.geom;
