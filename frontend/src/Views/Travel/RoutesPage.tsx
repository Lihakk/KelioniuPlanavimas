import { useEffect, useMemo, useState } from 'react';
import api from '../../api/api';
import type { PointOfInterest, RoutePoint, TravelRoute } from '../../types';
import { RouteMap } from './RouteMap';

export const RoutesPage = () => {
    const [routes, setRoutes] = useState<TravelRoute[]>([]);
    const [startCity, setStartCity] = useState('Kaunas');
    const [endCity, setEndCity] = useState('Vilnius');
    const [objects, setObjects] = useState<PointOfInterest[]>([]);
    const [selectedNames, setSelectedNames] = useState<string[]>([]);
    const [createdRoute, setCreatedRoute] = useState<TravelRoute | null>(null);
    const [editingRouteId, setEditingRouteId] = useState<number | null>(null);
    const [isSearching, setIsSearching] = useState(false);
    const [isSaving, setIsSaving] = useState(false);
    const [message, setMessage] = useState('');

    const selectedObjects = useMemo(
        () => objects.filter(object => selectedNames.includes(object.name)),
        [objects, selectedNames]
    );

    const getRoutes = () => api.get<TravelRoute[]>('/Route')
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

    const saveRoute = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSaving(true);
        setMessage('');

        try {
            const routePoints: RoutePoint[] = selectedObjects.map((object, index) => ({
                name: object.name,
                city: object.address || endCity,
                latitude: object.latitude,
                longitude: object.longitude,
                order: index + 1
            }));

            const payload = {
                id: editingRouteId ?? undefined,
                name: `${startCity} to ${endCity}`,
                length: 0,
                startingCity: startCity,
                endCity,
                polyline: '',
                travelTime: '',
                routePoints
            };

            const response = editingRouteId
                ? await api.put(`/Route/${editingRouteId}`, payload).then(async () => api.get<TravelRoute>(`/Route/${editingRouteId}`))
                : await api.post<TravelRoute>('/Route', payload);

            setCreatedRoute(response.data);
            setEditingRouteId(response.data.id ?? null);
            setMessage('Route recalculated through selected points.');
            await getRoutes();
        } catch (error) {
            console.error(error);
            setMessage('Could not calculate and save the route.');
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

    const editRoute = async (route: TravelRoute) => {
        setEditingRouteId(route.id ?? null);
        setCreatedRoute(route);
        setStartCity(route.startingCity);
        setEndCity(route.endCity);
        setSelectedNames(route.routePoints?.map(point => point.name) ?? []);
        setMessage('Route loaded for editing. Select more road POIs and recalculate.');
        const response = await api.post<PointOfInterest[]>('/Route/roadPOI', { startingCity: route.startingCity, endCity: route.endCity });
        const routePointObjects = route.routePoints?.map(point => ({
            name: point.name,
            type: 'selected',
            address: point.city,
            hasTicket: false,
            workingHours: '',
            rating: 4,
            latitude: point.latitude,
            longitude: point.longitude
        })) ?? [];
        setObjects([...routePointObjects, ...response.data].filter((object, index, array) =>
            array.findIndex(item => item.name === object.name) === index
        ));
    };

    const newRoute = () => {
        setEditingRouteId(null);
        setCreatedRoute(null);
        setSelectedNames([]);
        setMessage('New route mode.');
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
                        <span className="eyebrow">Route editor</span>
                        <h1>Select POIs between cities and recalculate the route</h1>
                    </div>
                    <button className="btn btn-outline" onClick={findObjects} disabled={isSearching || !endCity}>
                        {isSearching ? 'Loading road POIs...' : 'Find road POIs'}
                    </button>
                </div>

                <form onSubmit={saveRoute} className="planner-layout route-page-layout">
                    <div className="builder-panel">
                        <label>Starting city</label>
                        <input value={startCity} onChange={e => setStartCity(e.target.value)} required />

                        <label>Finishing city</label>
                        <input value={endCity} onChange={e => setEndCity(e.target.value)} required />

                        <div className="route-editor-actions">
                            <button className="btn btn-outline" type="button" onClick={newRoute}>New route</button>
                            <button className="btn btn-outline" type="button" onClick={findObjects}>Reload POIs</button>
                        </div>

                        <button className="btn btn-primary" type="submit" disabled={isSaving}>
                            {isSaving ? 'Recalculating route...' : editingRouteId ? 'Recalculate edited route' : 'Save calculated route'}
                        </button>

                        {message && <p className="status-line">{message}</p>}
                    </div>

                    <RouteMap
                        startCity={startCity}
                        endCity={endCity}
                        objects={objects}
                        selectedNames={selectedNames}
                        createdRoute={createdRoute}
                        onToggleObject={toggleObject}
                    />
                </form>

                <div className="object-layout">
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

                    <section>
                        <div className="section-title">
                            <h2>Saved routes</h2>
                            <button className="btn btn-outline" onClick={getRoutes}>Refresh</button>
                        </div>
                        <div className="trip-list">
                            {routes.map(route => (
                                <article className="trip-row" key={route.id}>
                                    <div>
                                        <strong>{route.name}</strong>
                                        <span>{route.startingCity} to {route.endCity}</span>
                                        <span>{route.travelTime || 'Time not estimated'} | {route.length} km</span>
                                        <span>{route.routePoints?.length ?? 0} selected POIs</span>
                                    </div>
                                    <div className="inline-actions">
                                        <button className="btn btn-outline" onClick={() => editRoute(route)}>Edit</button>
                                        <button className="btn btn-danger" onClick={() => deleteRoute(route.id)}>Delete</button>
                                    </div>
                                </article>
                            ))}
                            {routes.length === 0 && <div className="empty-state">No saved routes yet. Select road POIs and save a calculated route.</div>}
                        </div>
                    </section>
                </div>
            </section>
        </div>
    );
};
