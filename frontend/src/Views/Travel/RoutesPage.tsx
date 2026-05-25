import { useEffect, useMemo, useState } from 'react';
import api from '../../api/api';
import type { PointOfInterest, RoutePoint, TravelRoute } from '../../types';
import { RouteMap } from './RouteMap';

const routePointsToObjects = (route: TravelRoute): PointOfInterest[] =>
    route.routePoints?.map(point => ({
        name: point.name,
        type: 'selected',
        address: point.city,
        hasTicket: false,
        workingHours: '',
        rating: 4,
        latitude: point.latitude,
        longitude: point.longitude
    })) ?? [];

export const RoutesPage = () => {
    const [routes, setRoutes] = useState<TravelRoute[]>([]);
    const [startCity, setStartCity] = useState('Kaunas');
    const [endCity, setEndCity] = useState('Vilnius');
    const [objects, setObjects] = useState<PointOfInterest[]>([]);
    const [selectedNames, setSelectedNames] = useState<string[]>([]);
    const [createdRoute, setCreatedRoute] = useState<TravelRoute | null>(null);
    const [editingRouteId, setEditingRouteId] = useState<number | null>(null);
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [previewRoute, setPreviewRoute] = useState<TravelRoute | null>(null);
    const [viewRoute, setViewRoute] = useState<TravelRoute | null>(null);
    const [showSuccess, setShowSuccess] = useState(false);
    const [isSearching, setIsSearching] = useState(false);
    const [isSaving, setIsSaving] = useState(false);
    const [message, setMessage] = useState('');

    const selectedObjects = useMemo(
        () => objects.filter(object => selectedNames.includes(object.name)),
        [objects, selectedNames]
    );

    const getRoutes = () => api.get<TravelRoute[]>('/Route/openRoutes')
        .then(res => setRoutes(res.data))
        .catch(() => {
            setRoutes([]);
            setMessage('Route list is unavailable until the backend database is running.');
        });

    const findObjects = async () => {
        setIsSearching(true);
        setMessage('');
        setCreatedRoute(null);

        try {
            const response = await api.post<PointOfInterest[]>('/Route/roadPOI', { startingCity: startCity, endCity });
            setObjects(response.data);
            setSelectedNames(current => current.filter(name => response.data.some(object => object.name === name)));
            setMessage(response.data.length ? `${response.data.length} road POIs loaded.` : 'No objects found between these cities.');
        } catch (error) {
            console.error(error);
            setMessage('Could not load objects from map data.');
        } finally {
            setIsSearching(false);
        }
    };

    const toggleObject = (name: string) => {
        setSelectedNames(current =>
            current.includes(name)
                ? current.filter(item => item !== name)
                : [...current, name]
        );
    };

    const buildRoutePayload = () => {
        const routePoints: RoutePoint[] = selectedObjects.map((object, index) => ({
            name: object.name,
            city: object.address || endCity,
            latitude: object.latitude,
            longitude: object.longitude,
            order: index + 1
        }));

        return {
            id: editingRouteId ?? undefined,
            name: `${startCity} to ${endCity}`,
            length: 0,
            startingCity: startCity,
            endCity,
            polyline: '',
            travelTime: '',
            routePoints
        };
    };

    const previewCalculatedRoute = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSaving(true);
        setMessage('');

        try {
            const response = await api.post<TravelRoute>('/Route/preview', buildRoutePayload());
            setCreatedRoute(response.data);
            setPreviewRoute(response.data);
            setMessage('Route preview calculated. Confirm or edit before saving.');
        } catch (error) {
            console.error(error);
            setMessage('Could not calculate route preview.');
        } finally {
            setIsSaving(false);
        }
    };

    const confirmAndSaveRoute = async () => {
        if (!previewRoute) return;

        setIsSaving(true);
        setMessage('');
        try {
            const routeToSave = {
                ...previewRoute,
                id: editingRouteId ?? undefined
            };
            const response = editingRouteId
                ? await api.put(`/Route/${editingRouteId}`, routeToSave).then(async () => api.get<TravelRoute>(`/Route/${editingRouteId}`))
                : await api.post<TravelRoute>('/Route', routeToSave);

            setCreatedRoute(response.data);
            setEditingRouteId(response.data.id ?? null);
            setPreviewRoute(null);
            setIsCreateOpen(false);
            setShowSuccess(true);
            setMessage('Route saved successfully.');
            await getRoutes();
        } catch (error) {
            console.error(error);
            setMessage('Could not save confirmed route.');
        } finally {
            setIsSaving(false);
        }
    };

    const deleteRoute = async (id?: number) => {
        if (!id) return;
        await api.delete(`/Route/${id}`);
        if (editingRouteId === id) {
            setEditingRouteId(null);
            setCreatedRoute(null);
        }
        await getRoutes();
    };

    const viewSelectedRoute = async (route: TravelRoute) => {
        if (!route.id) return;
        const response = await api.get<TravelRoute>(`/Route/openRouteView/${route.id}`);
        setViewRoute(response.data);
        setShowSuccess(false);
    };

    const editRoute = async (route: TravelRoute) => {
        const response = route.id
            ? await api.get<TravelRoute>(`/Route/openRouteEdit/${route.id}`)
            : { data: route };
        const routeForEdit = response.data;
        setIsCreateOpen(true);
        setPreviewRoute(null);
        setViewRoute(null);
        setShowSuccess(false);
        setEditingRouteId(routeForEdit.id ?? null);
        setCreatedRoute(routeForEdit);
        setStartCity(routeForEdit.startingCity);
        setEndCity(routeForEdit.endCity);
        setSelectedNames(routeForEdit.routePoints?.map(point => point.name) ?? []);
        setMessage('Route loaded for editing. Select more road POIs and recalculate.');
        const poiResponse = await api.post<PointOfInterest[]>('/Route/roadPOI', { startingCity: routeForEdit.startingCity, endCity: routeForEdit.endCity });
        const routePointObjects = routePointsToObjects(routeForEdit);
        setObjects([...routePointObjects, ...poiResponse.data].filter((object, index, array) =>
            array.findIndex(item => item.name === object.name) === index
        ));
    };

    const newRoute = () => {
        setEditingRouteId(null);
        setCreatedRoute(null);
        setPreviewRoute(null);
        setSelectedNames([]);
        setMessage('New route mode.');
    };

    const openRouteCreate = async () => {
        await api.post('/Route/openRouteCreation').catch(() => undefined);
        newRoute();
        setIsCreateOpen(true);
        setViewRoute(null);
        setShowSuccess(false);
        await findObjects();
    };

    const closeRouteCreate = () => {
        setIsCreateOpen(false);
        setPreviewRoute(null);
    };

    useEffect(() => {
        getRoutes();
        findObjects();
    }, []);

    return (
        <div className="trip-workspace">
            <section className="trip-builder">
                <div className="builder-head">
                    <div>
                        <span className="eyebrow">Route list</span>
                        <h1>Routes are created in a separate RouteCreate window</h1>
                    </div>
                    <button className="btn btn-primary" onClick={openRouteCreate}>
                        Create route
                    </button>
                </div>

                <section>
                    <div className="section-title">
                        <h2>Saved routes</h2>
                        <button className="btn btn-outline" onClick={getRoutes}>Refresh</button>
                    </div>
                    <div className="trip-list route-list-grid">
                        {routes.map(route => (
                            <article className="trip-row" key={route.id}>
                                <div>
                                    <strong>{route.name}</strong>
                                    <span>{route.startingCity} to {route.endCity}</span>
                                    <span>{route.travelTime || 'Time not estimated'} | {route.length} km</span>
                                    <span>{route.routePoints?.length ?? 0} selected POIs</span>
                                </div>
                                <div className="inline-actions">
                                    <button className="btn btn-outline" onClick={() => viewSelectedRoute(route)}>View</button>
                                    <button className="btn btn-outline" onClick={() => editRoute(route)}>Edit</button>
                                    <button className="btn btn-danger" onClick={() => deleteRoute(route.id)}>Delete</button>
                                </div>
                            </article>
                        ))}
                        {routes.length === 0 && <div className="empty-state">No saved routes yet. Open RouteCreate and confirm a route.</div>}
                    </div>
                </section>
            </section>

            {isCreateOpen && (
                <div className="modal-backdrop" role="dialog" aria-modal="true" aria-label="RouteCreate">
                    <section className="route-create-modal">
                        <div className="modal-head">
                            <div>
                                <span className="eyebrow">RouteCreate</span>
                                <h2>{editingRouteId ? 'Edit route' : 'Create route'}</h2>
                            </div>
                            <button className="modal-close" onClick={closeRouteCreate} aria-label="Close route create">x</button>
                        </div>

                        <form onSubmit={previewCalculatedRoute} className="planner-layout route-page-layout">
                            <div className="builder-panel">
                                <label>Starting city</label>
                                <input value={startCity} onChange={e => setStartCity(e.target.value)} required />

                                <label>Finishing city</label>
                                <input value={endCity} onChange={e => setEndCity(e.target.value)} required />

                                <div className="route-editor-actions">
                                    <button className="btn btn-outline" type="button" onClick={newRoute}>New route</button>
                                    <button className="btn btn-outline" type="button" onClick={findObjects} disabled={isSearching}>
                                        {isSearching ? 'Loading POIs...' : 'Reload POIs'}
                                    </button>
                                </div>

                                <button className="btn btn-primary" type="submit" disabled={isSaving}>
                                    {isSaving ? 'Calculating preview...' : 'Calculate route preview'}
                                </button>

                                {message && <p className="status-line">{message}</p>}
                            </div>

                            <RouteMap
                                startCity={startCity}
                                endCity={endCity}
                                objects={objects}
                                selectedNames={selectedNames}
                                createdRoute={previewRoute ?? createdRoute}
                                onToggleObject={toggleObject}
                            />
                        </form>

                        <div className="object-layout route-create-lower">
                            <section>
                                <div className="section-title">
                                    <h2>Road points of interest</h2>
                                    <span>{selectedObjects.length} selected</span>
                                </div>
                                <div className="object-grid">
                                    {objects.map(object => {
                                        const selected = selectedNames.includes(object.name);
                                        return (
                                            <button
                                                type="button"
                                                key={`${object.name}-${object.latitude}-${object.longitude}`}
                                                className={`object-card ${selected ? 'selected' : ''}`}
                                                onClick={() => toggleObject(object.name)}
                                            >
                                                <strong>{object.name}</strong>
                                                <span>{object.type}</span>
                                                <small>{object.address || `${object.latitude}, ${object.longitude}`}</small>
                                            </button>
                                        );
                                    })}
                                    {objects.length === 0 && <div className="empty-state">Load road POIs to show selectable map points between the cities.</div>}
                                </div>
                            </section>
                        </div>
                    </section>
                </div>
            )}

            {viewRoute && (
                <div className="modal-backdrop modal-backdrop-front" role="dialog" aria-modal="true" aria-label="RouteView">
                    <section className="route-preview-modal route-view-modal">
                        <div className="modal-head">
                            <div>
                                <span className="eyebrow">RouteView</span>
                                <h2>{viewRoute.name}</h2>
                            </div>
                            <button className="modal-close" onClick={() => setViewRoute(null)} aria-label="Close route view">x</button>
                        </div>
                        <div className="preview-stats">
                            <strong>{viewRoute.length} km</strong>
                            <span>{viewRoute.travelTime || 'Time not estimated'}</span>
                            <span>{viewRoute.routePoints?.length ?? 0} POIs</span>
                        </div>
                        <RouteMap
                            startCity={viewRoute.startingCity}
                            endCity={viewRoute.endCity}
                            objects={routePointsToObjects(viewRoute)}
                            selectedNames={viewRoute.routePoints?.map(point => point.name) ?? []}
                            createdRoute={viewRoute}
                            onToggleObject={() => undefined}
                        />
                        <div className="modal-actions">
                            <button className="btn btn-outline" onClick={() => setViewRoute(null)}>Close</button>
                            <button className="btn btn-primary" onClick={() => editRoute(viewRoute)}>Edit route</button>
                        </div>
                    </section>
                </div>
            )}

            {previewRoute && (
                <div className="modal-backdrop modal-backdrop-front" role="dialog" aria-modal="true" aria-label="Route preview">
                    <section className="route-preview-modal">
                        <div className="modal-head">
                            <div>
                                <span className="eyebrow">Route preview</span>
                                <h2>Confirm calculated route</h2>
                            </div>
                            <button className="modal-close" onClick={() => setPreviewRoute(null)} aria-label="Close preview">x</button>
                        </div>
                        <div className="preview-stats">
                            <strong>{previewRoute.length} km</strong>
                            <span>{previewRoute.travelTime}</span>
                            <span>{previewRoute.routePoints?.length ?? 0} POIs</span>
                        </div>
                        <RouteMap
                            startCity={startCity}
                            endCity={endCity}
                            objects={objects}
                            selectedNames={selectedNames}
                            createdRoute={previewRoute}
                            onToggleObject={toggleObject}
                        />
                        <div className="modal-actions">
                            <button className="btn btn-outline" onClick={() => setPreviewRoute(null)}>Edit route</button>
                            <button className="btn btn-primary" onClick={confirmAndSaveRoute} disabled={isSaving}>
                                {isSaving ? 'Saving...' : 'Confirm route'}
                            </button>
                        </div>
                    </section>
                </div>
            )}

            {showSuccess && (
                <div className="modal-backdrop modal-backdrop-front" role="dialog" aria-modal="true" aria-label="Route saved">
                    <section className="success-modal">
                        <span className="eyebrow">Success</span>
                        <h2>Route saved</h2>
                        <p>The route was saved and is visible in RouteList.</p>
                        <button className="btn btn-primary" onClick={() => setShowSuccess(false)}>OK</button>
                    </section>
                </div>
            )}
        </div>
    );
};
