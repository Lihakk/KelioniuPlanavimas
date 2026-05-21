import { useMemo, useState } from 'react';
import api from '../../api/api';
import type { PointOfInterest, RoutePoint, TravelRoute } from '../../types';

export const RoutesPage = () => {
    const [routes, setRoutes] = useState<TravelRoute[]>([]);
    const [startCity, setStartCity] = useState('Kaunas');
    const [endCity, setEndCity] = useState('Vilnius');
    const [objects, setObjects] = useState<PointOfInterest[]>([]);
    const [selectedNames, setSelectedNames] = useState<string[]>([]);
    const [createdRoute, setCreatedRoute] = useState<TravelRoute | null>(null);
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
            const response = await api.post<PointOfInterest[]>('/POI/sendRouteData', { city: endCity, type: '' });
            setObjects(response.data);
            setSelectedNames([]);
            setMessage(response.data.length ? 'Objects loaded from OSM.' : 'No objects found for this city.');
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

            const response = await api.post<TravelRoute>('/Route', {
                name: `${startCity} to ${endCity}`,
                length: 0,
                startingCity: startCity,
                endCity,
                polyline: '',
                travelTime: '',
                routePoints
            });

            setCreatedRoute(response.data);
            setMessage('Route calculated and saved automatically.');
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
        await getRoutes();
    };

    return (
        <div className="trip-workspace">
            <section className="trip-builder">
                <div className="builder-head">
                    <div>
                        <span className="eyebrow">Automatic route creation</span>
                        <h1>Calculate a route from selected visit objects</h1>
                    </div>
                    <button className="btn btn-outline" onClick={findObjects} disabled={isSearching || !endCity}>
                        {isSearching ? 'Loading objects...' : 'Find objects'}
                    </button>
                </div>

                <form onSubmit={saveRoute} className="builder-grid">
                    <div className="builder-panel">
                        <label>Starting city</label>
                        <input value={startCity} onChange={e => setStartCity(e.target.value)} required />

                        <label>Finishing city</label>
                        <input value={endCity} onChange={e => setEndCity(e.target.value)} required />

                        <button className="btn btn-primary" type="submit" disabled={isSaving}>
                            {isSaving ? 'Calculating route...' : 'Save calculated route'}
                        </button>

                        {message && <p className="status-line">{message}</p>}
                    </div>

                    <div className="map-preview" aria-label="Route preview">
                        <div className="map-city start">{startCity || 'Start'}</div>
                        <div className="route-line">
                            {selectedObjects.map(object => (
                                <button
                                    type="button"
                                    key={object.name}
                                    className="route-stop"
                                    title={object.name}
                                    onClick={() => toggleObject(object.name)}
                                >
                                    {object.name.slice(0, 2).toUpperCase()}
                                </button>
                            ))}
                        </div>
                        <div className="map-city end">{endCity || 'Finish'}</div>
                        <div className="route-summary">
                            <strong>{createdRoute ? `${createdRoute.length} km` : `${selectedObjects.length} selected objects`}</strong>
                            <span>{createdRoute?.travelTime || 'Route time appears after calculation'}</span>
                        </div>
                    </div>
                </form>

                <div className="object-layout">
                    <section>
                        <div className="section-title">
                            <h2>Objects from map data</h2>
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
                                    </div>
                                    <button className="btn btn-danger" onClick={() => deleteRoute(route.id)}>Delete</button>
                                </article>
                            ))}
                        </div>
                    </section>
                </div>
            </section>
        </div>
    );
};
