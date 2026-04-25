const mapInstances = new globalThis.Map();

const olNamespace = globalThis.ol;

if (!olNamespace) {
    throw new Error('OpenLayers bundle is not available on window.ol.');
}

const GeoJSON = olNamespace.format.GeoJSON;
const OlMap = olNamespace.Map;
const XYZ = olNamespace.source.XYZ;
const TileLayer = olNamespace.layer.Tile;
const VectorLayer = olNamespace.layer.Vector;
const VectorSource = olNamespace.source.Vector;
const View = olNamespace.View;
const CircleStyle = olNamespace.style.Circle;
const Fill = olNamespace.style.Fill;
const Stroke = olNamespace.style.Stroke;
const Style = olNamespace.style.Style;
const fromLonLat = olNamespace.proj.fromLonLat;

const categoryColors = {
    area: '#5f7382',
    landmark: '#c9d5df',
    route: '#7ea0ba'
};

const selectionColor = '#f6f8fb';

export async function initializeMap(containerId, statusId, detailsId) {
    disposeMap(containerId);

    const container = document.getElementById(containerId);
    const status = document.getElementById(statusId);
    const details = document.getElementById(detailsId);

    if (!container || !status || !details) {
        return;
    }

    try {
        setText(status, 'Loading spatial features...');

        const featureCollection = await fetchFeatureCollection();
        const format = new GeoJSON();
        const features = format.readFeatures(featureCollection, {
            dataProjection: 'EPSG:4326',
            featureProjection: 'EPSG:3857'
        });

        const vectorSource = new VectorSource({ features });
        const vectorLayer = new VectorLayer({
            source: vectorSource,
            style: createFeatureStyle
        });

        const map = new OlMap({
            target: container,
            layers: [
                new TileLayer({
                    source: new XYZ({
                        attributions: '© OpenStreetMap contributors © CARTO',
                        crossOrigin: 'anonymous',
                        maxZoom: 20,
                        url: 'https://basemaps.cartocdn.com/dark_all/{z}/{x}/{y}.png'
                    })
                }),
                vectorLayer
            ],
            view: new View({
                center: fromLonLat([29.02, 41.04]),
                zoom: 10.5
            })
        });

        if (features.length > 0) {
            map.getView().fit(vectorSource.getExtent(), {
                padding: [56, 56, 56, 56],
                maxZoom: 14,
                duration: 500
            });
        }

        let selectedFeature = null;

        map.on('singleclick', event => {
            const feature = map.forEachFeatureAtPixel(event.pixel, candidate => candidate);

            if (selectedFeature) {
                selectedFeature.set('selected', false);
            }

            selectedFeature = feature ?? null;

            if (selectedFeature) {
                selectedFeature.set('selected', true);
                renderFeatureDetails(details, selectedFeature);
            } else {
                setText(details, 'No feature selected');
            }

            vectorLayer.changed();
        });

        map.on('pointermove', event => {
            const hasFeature = map.hasFeatureAtPixel(event.pixel);
            container.style.cursor = hasFeature ? 'pointer' : '';
        });

        mapInstances.set(containerId, map);
        setText(status, buildStatusText(features));
        renderFeatureDetails(details, features[0]);
    } catch (error) {
        const message = error instanceof Error ? error.message : 'Map initialization failed';

        console.error('Failed to initialize GeoJSON map.', error);
        setText(status, message);
        setText(details, 'Map data could not be loaded.');
    }
}

export function disposeMap(containerId) {
    const map = mapInstances.get(containerId);

    if (!map) {
        return;
    }

    map.setTarget(undefined);
    mapInstances.delete(containerId);
}

function createFeatureStyle(feature) {
    const category = feature.get('category') ?? 'landmark';
    const geometryType = feature.getGeometry()?.getType();
    const color = categoryColors[category] ?? '#475569';
    const selected = feature.get('selected') === true;
    const strokeColor = selected ? selectionColor : color;

    if (geometryType === 'Point' || geometryType === 'MultiPoint') {
        return new Style({
            image: new CircleStyle({
                radius: selected ? 6 : 5,
                fill: new Fill({ color: selected ? selectionColor : '#11161d' }),
                stroke: new Stroke({ color: strokeColor, width: selected ? 2.5 : 1.5 })
            })
        });
    }

    if (geometryType === 'LineString' || geometryType === 'MultiLineString') {
        return new Style({
            stroke: new Stroke({
                color: strokeColor,
                width: selected ? 4 : 2.5
            })
        });
    }

    return new Style({
        fill: new Fill({ color: selected ? 'rgba(246, 248, 251, 0.14)' : 'rgba(95, 115, 130, 0.16)' }),
        stroke: new Stroke({ color: strokeColor, width: selected ? 2.5 : 1.5 })
    });
}

async function fetchFeatureCollection() {
    const maxAttempts = 4;

    for (let attempt = 1; attempt <= maxAttempts; attempt += 1) {
        try {
            const response = await fetch('/api/features', {
                cache: 'no-store',
                headers: {
                    Accept: 'application/geo+json'
                }
            });

            if (!response.ok) {
                throw new Error(`GeoJSON endpoint returned ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            if (attempt === maxAttempts) {
                throw new Error('GeoJSON data could not be loaded after retrying.');
            }

            await wait(attempt * 400);
        }
    }

    throw new Error('GeoJSON data could not be loaded.');
}

function renderFeatureDetails(container, feature) {
    if (!feature) {
        setText(container, 'No feature selected');
        return;
    }

    const title = document.createElement('strong');
    title.textContent = feature.get('name') ?? 'Unnamed feature';

    const category = document.createElement('span');
    category.className = 'feature-pill';
    category.textContent = `${feature.get('category') ?? 'feature'} / ${feature.getGeometry()?.getType() ?? 'Geometry'}`;

    const description = document.createElement('p');
    description.textContent = feature.get('description') ?? '';

    container.replaceChildren(title, category, description);
}

function buildStatusText(features) {
    const categories = features.reduce((accumulator, feature) => {
        const category = feature.get('category') ?? 'feature';
        accumulator[category] = (accumulator[category] ?? 0) + 1;
        return accumulator;
    }, {});

    const categoryText = Object.entries(categories)
        .map(([category, count]) => `${category}: ${count}`)
        .join(' / ');

    return `${features.length} features loaded (${categoryText})`;
}

function setText(element, value) {
    element.textContent = value;
}

function wait(duration) {
    return new Promise(resolve => {
        globalThis.setTimeout(resolve, duration);
    });
}