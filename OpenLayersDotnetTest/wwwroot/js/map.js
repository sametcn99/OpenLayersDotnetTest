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
const Draw = olNamespace.interaction.Draw;
const Modify = olNamespace.interaction.Modify;
const Snap = olNamespace.interaction.Snap;
const Stroke = olNamespace.style.Stroke;
const Style = olNamespace.style.Style;
const fromLonLat = olNamespace.proj.fromLonLat;

const dataProjection = 'EPSG:4326';
const featureProjection = 'EPSG:3857';

const categoryColors = {
    area: '#5f7382',
    landmark: '#c9d5df',
    route: '#7ea0ba'
};

const defaultCategoryByGeometry = {
    LineString: 'route',
    Point: 'landmark',
    Polygon: 'area'
};

const selectionColor = '#f6f8fb';

export async function initializeMap(containerId, statusId, detailsId, editorId) {
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
            dataProjection,
            featureProjection
        });

        const vectorSource = new VectorSource({ features });
        const vectorLayer = new VectorLayer({
            source: vectorSource,
            style: createFeatureStyle
        });
        const draftSource = new VectorSource();
        const draftLayer = new VectorLayer({
            source: draftSource,
            style: createDraftStyle
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
                vectorLayer,
                draftLayer
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

        const state = {
            activeSketchFeature: null,
            categoryTouched: false,
            container,
            contextMenuHandler: null,
            details,
            draftLayer,
            draftSource,
            drawInteraction: null,
            editor: resolveFeatureEditor(editorId),
            format,
            isSaving: false,
            keydownHandler: null,
            map,
            selectedFeature: null,
            slugSeed: createShortId(),
            slugTouched: false,
            snapInteraction: null,
            status,
            vectorLayer,
            vectorSource
        };

        if (state.editor) {
            state.modifyInteraction = new Modify({ source: draftSource });
            map.addInteraction(state.modifyInteraction);
            bindFeatureEditor(state);
        }

        bindDrawingExitHandlers(state);

        map.on('singleclick', event => {
            if (state.drawInteraction) {
                return;
            }

            const feature = map.forEachFeatureAtPixel(event.pixel, candidate => candidate);

            if (state.selectedFeature) {
                state.selectedFeature.set('selected', false);
            }

            state.selectedFeature = feature ?? null;

            if (state.selectedFeature) {
                state.selectedFeature.set('selected', true);
                renderFeatureDetails(details, state.selectedFeature);
            } else {
                setText(details, 'No feature selected');
            }

            vectorLayer.changed();
        });

        map.on('pointermove', event => {
            const hasFeature = map.hasFeatureAtPixel(event.pixel);
            container.style.cursor = state.drawInteraction ? 'crosshair' : hasFeature ? 'pointer' : '';
        });

        mapInstances.set(containerId, state);
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
    const state = mapInstances.get(containerId);

    if (!state) {
        return;
    }

    stopDrawing(state);
    unbindDrawingExitHandlers(state);
    state.map.setTarget(undefined);
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

function createDraftStyle(feature) {
    const geometryType = feature.getGeometry()?.getType();
    const stroke = new Stroke({ color: '#f2b84b', width: 2.5, lineDash: [8, 8] });

    if (geometryType === 'Point' || geometryType === 'MultiPoint') {
        return new Style({
            image: new CircleStyle({
                radius: 6,
                fill: new Fill({ color: '#f2b84b' }),
                stroke: new Stroke({ color: '#11161d', width: 2 })
            })
        });
    }

    if (geometryType === 'LineString' || geometryType === 'MultiLineString') {
        return new Style({ stroke });
    }

    return new Style({
        fill: new Fill({ color: 'rgba(242, 184, 75, 0.16)' }),
        stroke
    });
}

function resolveFeatureEditor(editorId) {
    if (!editorId) {
        return null;
    }

    const root = document.getElementById(editorId);

    if (!root) {
        return null;
    }

    const editor = {
        category: root.querySelector('#feature-category'),
        clearButton: root.querySelector('#feature-clear'),
        description: root.querySelector('#feature-description'),
        form: root,
        geometryType: root.querySelector('#feature-geometry'),
        name: root.querySelector('#feature-name'),
        saveButton: root.querySelector('#feature-save'),
        slug: root.querySelector('#feature-slug'),
        startButton: root.querySelector('#feature-start-drawing'),
        status: root.querySelector('#feature-editor-status')
    };

    return Object.values(editor).every(Boolean) ? editor : null;
}

function bindFeatureEditor(state) {
    const { editor } = state;

    editor.name.addEventListener('input', () => {
        if (!state.slugTouched) {
            editor.slug.value = createSlug(editor.name.value, state.slugSeed);
        }
    });

    editor.slug.addEventListener('input', () => {
        state.slugTouched = true;
    });

    editor.geometryType.addEventListener('change', () => {
        const category = defaultCategoryByGeometry[editor.geometryType.value] ?? 'landmark';

        if (!state.categoryTouched) {
            editor.category.value = category;
        }

        clearDraft(state, `Ready for ${formatGeometryLabel(editor.geometryType.value)}`);
    });

    editor.category.addEventListener('change', () => {
        state.categoryTouched = true;
        state.draftLayer.changed();
    });

    editor.startButton.addEventListener('click', () => {
        startDrawing(state);
    });

    editor.clearButton.addEventListener('click', () => {
        clearDraft(state, 'Draft cleared');
    });

    editor.form.addEventListener('submit', async event => {
        event.preventDefault();
        await saveDraftFeature(state);
    });

    editor.category.value = defaultCategoryByGeometry[editor.geometryType.value] ?? 'landmark';
    updateEditorControls(state, 'Ready');
}

function startDrawing(state) {
    const geometryType = state.editor.geometryType.value;

    stopDrawing(state);
    state.draftSource.clear();

    const drawInteraction = new Draw({
        source: state.draftSource,
        style: createDraftStyle,
        type: geometryType
    });

    drawInteraction.on('drawstart', () => {
        state.draftSource.clear();
        state.activeSketchFeature = null;
        updateEditorControls(state, `Drawing ${formatGeometryLabel(geometryType)}`);
    });

    drawInteraction.on('drawstart', event => {
        state.activeSketchFeature = event.feature;
    });

    drawInteraction.on('drawend', event => {
        state.activeSketchFeature = null;
        event.feature.set('category', state.editor.category.value);
        event.feature.set('draft', true);

        globalThis.setTimeout(() => {
            stopDrawing(state);
            updateEditorControls(state, `${formatGeometryLabel(geometryType)} ready`);
        }, 0);
    });

    state.map.addInteraction(drawInteraction);
    state.drawInteraction = drawInteraction;

    const snapInteraction = new Snap({ source: state.draftSource });
    state.map.addInteraction(snapInteraction);
    state.snapInteraction = snapInteraction;

    updateEditorControls(state, `Drawing ${formatGeometryLabel(geometryType)}`);
}

function stopDrawing(state) {
    state.activeSketchFeature = null;

    if (state.drawInteraction) {
        state.map.removeInteraction(state.drawInteraction);
        state.drawInteraction = null;
    }

    if (state.snapInteraction) {
        state.map.removeInteraction(state.snapInteraction);
        state.snapInteraction = null;
    }

    state.container.classList.remove('is-drawing');
}

function bindDrawingExitHandlers(state) {
    state.keydownHandler = event => {
        if (event.key !== 'Escape' || !state.drawInteraction || isWithinEditor(event.target, state.editor?.form)) {
            return;
        }

        event.preventDefault();
        exitActiveDrawing(state);
    };

    state.contextMenuHandler = event => {
        if (!state.drawInteraction) {
            return;
        }

        event.preventDefault();
        exitActiveDrawing(state);
    };

    globalThis.addEventListener('keydown', state.keydownHandler);
    state.container.addEventListener('contextmenu', state.contextMenuHandler);
}

function unbindDrawingExitHandlers(state) {
    if (state.keydownHandler) {
        globalThis.removeEventListener('keydown', state.keydownHandler);
        state.keydownHandler = null;
    }

    if (state.contextMenuHandler) {
        state.container.removeEventListener('contextmenu', state.contextMenuHandler);
        state.contextMenuHandler = null;
    }
}

function exitActiveDrawing(state) {
    const geometryType = state.editor?.geometryType?.value;

    if (canFinishCurrentSketch(state)) {
        state.drawInteraction.finishDrawing();
        return;
    }

    stopDrawing(state);

    if (geometryType === 'Polygon' || geometryType === 'LineString') {
        updateEditorControls(state, `${formatGeometryLabel(geometryType)} drawing stopped`);
        return;
    }

    updateEditorControls(state, 'Drawing stopped');
}

function canFinishCurrentSketch(state) {
    const geometry = state.activeSketchFeature?.getGeometry();

    if (!geometry) {
        return false;
    }

    const geometryType = geometry.getType();

    if (geometryType === 'LineString') {
        return geometry.getCoordinates().length >= 2;
    }

    if (geometryType === 'Polygon') {
        const coordinates = geometry.getCoordinates();
        return Array.isArray(coordinates[0]) && coordinates[0].length >= 4;
    }

    return false;
}

function isWithinEditor(target, editorForm) {
    return target instanceof Element && editorForm instanceof Element && editorForm.contains(target);
}

function clearDraft(state, message) {
    stopDrawing(state);
    state.draftSource.clear();
    updateEditorControls(state, message);
}

async function saveDraftFeature(state) {
    const { editor } = state;
    const draftFeature = state.draftSource.getFeatures()[0];

    if (!draftFeature) {
        updateEditorControls(state, 'No draft geometry');
        return;
    }

    if (!editor.form.reportValidity()) {
        return;
    }

    state.isSaving = true;
    updateEditorControls(state, 'Saving feature');

    try {
        const geometry = state.format.writeGeometryObject(draftFeature.getGeometry(), {
            dataProjection,
            featureProjection
        });
        const payload = {
            category: editor.category.value,
            description: editor.description.value.trim(),
            geometry,
            metadata: {
                geometryType: geometry.type,
                source: 'browser-drawing'
            },
            name: editor.name.value.trim(),
            slug: editor.slug.value.trim(),
            sortOrder: (state.vectorSource.getFeatures().length + 1) * 10
        };
        const response = await fetch('/api/features', {
            body: JSON.stringify(payload),
            headers: {
                Accept: 'application/geo+json',
                'Content-Type': 'application/json'
            },
            method: 'POST'
        });

        if (!response.ok) {
            throw new Error(await readApiError(response));
        }

        const createdFeature = await response.json();
        const mapFeature = state.format.readFeature(createdFeature, {
            dataProjection,
            featureProjection
        });

        if (state.selectedFeature) {
            state.selectedFeature.set('selected', false);
        }

        mapFeature.set('selected', true);
        state.selectedFeature = mapFeature;
        state.vectorSource.addFeature(mapFeature);
        state.vectorLayer.changed();
        state.draftSource.clear();

        setText(state.status, buildStatusText(state.vectorSource.getFeatures()));
        renderFeatureDetails(state.details, mapFeature);
        resetEditorForNextFeature(state, geometry.type);
        updateEditorControls(state, 'Saved');
    } catch (error) {
        const message = error instanceof Error ? error.message : 'Feature could not be saved.';

        console.error('Failed to save drawn feature.', error);
        updateEditorControls(state, message);
    } finally {
        state.isSaving = false;
        updateEditorControls(state);
    }
}

function resetEditorForNextFeature(state, geometryType) {
    const { editor } = state;

    state.slugSeed = createShortId();
    state.slugTouched = false;
    state.categoryTouched = false;

    editor.name.value = '';
    editor.slug.value = '';
    editor.description.value = '';
    editor.geometryType.value = geometryType;
    editor.category.value = defaultCategoryByGeometry[geometryType] ?? 'landmark';
}

function updateEditorControls(state, message) {
    const { editor } = state;
    const hasDraft = state.draftSource.getFeatures().length > 0;
    const isDrawing = Boolean(state.drawInteraction);

    editor.saveButton.disabled = !hasDraft || state.isSaving;
    editor.clearButton.disabled = (!hasDraft && !isDrawing) || state.isSaving;
    editor.startButton.disabled = state.isSaving;
    editor.startButton.textContent = isDrawing ? 'Drawing' : 'Draw';

    state.container.classList.toggle('is-drawing', isDrawing);

    if (message) {
        setText(editor.status, message);
    }
}

async function readApiError(response) {
    const text = await response.text();

    try {
        const problem = JSON.parse(text);

        if (problem.detail) {
            return problem.detail;
        }

        if (problem.title) {
            return problem.title;
        }
    } catch {
        if (text) {
            return text;
        }
    }

    return `Feature endpoint returned ${response.status}`;
}

function createSlug(value, suffix) {
    const slug = value
        .trim()
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-+|-+$/g, '')
        .replace(/-{2,}/g, '-');

    return `${slug || 'feature'}-${suffix}`;
}

function createShortId() {
    return Date.now().toString(36).slice(-5);
}

function formatGeometryLabel(geometryType) {
    return geometryType === 'LineString' ? 'Line' : geometryType;
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