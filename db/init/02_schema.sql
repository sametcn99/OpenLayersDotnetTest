CREATE TABLE IF NOT EXISTS public.map_features (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    slug text NOT NULL UNIQUE,
    name text NOT NULL,
    category text NOT NULL,
    description text NOT NULL,
    sort_order integer NOT NULL DEFAULT 0,
    metadata jsonb NOT NULL DEFAULT '{}'::jsonb,
    geom geometry(Geometry, 4326) NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT map_features_slug_not_blank CHECK (length(trim(slug)) > 0),
    CONSTRAINT map_features_name_not_blank CHECK (length(trim(name)) > 0),
    CONSTRAINT map_features_category_not_blank CHECK (length(trim(category)) > 0),
    CONSTRAINT map_features_geom_valid CHECK (ST_IsValid(geom)),
    CONSTRAINT map_features_geom_srid CHECK (ST_SRID(geom) = 4326)
);

CREATE INDEX IF NOT EXISTS ix_map_features_geom
    ON public.map_features
    USING gist (geom);

CREATE INDEX IF NOT EXISTS ix_map_features_category
    ON public.map_features (category);

CREATE INDEX IF NOT EXISTS ix_map_features_metadata
    ON public.map_features
    USING gin (metadata);

CREATE OR REPLACE FUNCTION public.set_updated_at()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS set_map_features_updated_at ON public.map_features;

CREATE TRIGGER set_map_features_updated_at
    BEFORE UPDATE ON public.map_features
    FOR EACH ROW
    EXECUTE FUNCTION public.set_updated_at();
