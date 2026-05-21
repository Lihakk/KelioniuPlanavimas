import { useMemo, useState } from 'react';
import api from '../../api/api';
import type { PointOfInterest, RoutePoint, TravelRoute, Trip } from '../../types';

const today = new Date().toISOString().slice(0, 10);

export const TripsPage = () => {
    const [trips, setTrips] = useState<Trip[]>([]);
    const [startCity, setStartCity] = useState('Kaunas');
    const [endCity, setEndCity] = useState('Vilnius');
    const [startDate, setStartDate] = useState(today);
    const [endDate, setEndDate] = useState(today);
    const [objects, setObjects] = useState<PointOfInterest[]>([]);
    const [selectedNames, setSelectedNames] = useState<string[]>([]);
    const [createdRoute, setCreatedRoute] = useState<TravelRoute | null>(null);
    const [isSearching, setIsSearching] = useState(false);
    const [isCreating, setIsCreating] = useState(false);
    const [message, setMessage] = useState('');

    const selectedObjects = useMemo(
        () => objects.filter(object => selectedNames.includes(object.name)),
        [objects, selectedNames]
    );

    const getTrips = () => api.get<Trip[]>('/Trip')
        .then(res => setTrips(res.data))
        .catch(() => {
            setTrips([]);
            setMessage('Trip list is unavailable until the backend database is running.');
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

    const createAutomaticTrip = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsCreating(true);
        setMessage('');

        try {
            const routePoints: RoutePoint[] = selectedObjects.map((object, index) => ({
                name: object.name,
                city: object.address || endCity,
                latitude: object.latitude,
                longitude: object.longitude,
                order: index + 1
            }));

            const routeResponse = await api.post<TravelRoute>('/Route', {
                name: `${startCity} to ${endCity}`,
                length: 0,
                startingCity: startCity,
                endCity,
                polyline: '',
                travelTime: '',
                routePoints
            });

            setCreatedRoute(routeResponse.data);

            await api.post<Trip>('/Trip', {
                name: `${startCity} - ${endCity}`,
                tripStatus: 'Planned',
                startDate,
                endDate,
                routeId: routeResponse.data.id,
                selectedAccommodation: '',
                selectedFlight: '',
                selectedCar: ''
            });

            setMessage('Trip created and route calculated automatically.');
            await getTrips();
        } catch (error) {
            console.error(error);
            setMessage('Could not calculate and save the trip.');
        } finally {
            setIsCreating(false);
        }
    };

    const deleteTrip = async (id?: number) => {
        if (!id) return;
        await api.delete(`/Trip/${id}`);
        await getTrips();
    };

    return (
        <div className="trip-workspace">
            <section className="trip-builder">
                <div className="builder-head">
                    <div>
                        <span className="eyebrow">Automatic trip builder</span>
                        <h1>Create a route from cities and visit objects</h1>
                    </div>
                    <button className="btn btn-outline" onClick={findObjects} disabled={isSearching || !endCity}>
                        {isSearching ? 'Loading objects...' : 'Find objects'}
                    </button>
                </div>

                <form onSubmit={createAutomaticTrip} className="builder-grid">
                    <div className="builder-panel">
                        <label>Starting city</label>
                        <input value={startCity} onChange={e => setStartCity(e.target.value)} required />

                        <label>Finishing city</label>
                        <input value={endCity} onChange={e => setEndCity(e.target.value)} required />

                        <div className="date-row">
                            <div>
                                <label>Start date</label>
                                <input type="date" value={startDate} onChange={e => setStartDate(e.target.value)} required />
                            </div>
                            <div>
                                <label>End date</label>
                                <input type="date" value={endDate} onChange={e => setEndDate(e.target.value)} required />
                            </div>
                        </div>

                        <button className="btn btn-primary" type="submit" disabled={isCreating}>
                            {isCreating ? 'Calculating route...' : 'Create trip automatically'}
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
                            <h2>Objects to visit</h2>
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
                            <h2>Created trips</h2>
                            <button className="btn btn-outline" onClick={getTrips}>Refresh</button>
                        </div>
                        <div className="trip-list">
                            {trips.map(trip => (
                                <article className="trip-row" key={trip.id}>
                                    <div>
                                        <strong>{trip.name}</strong>
                                        <span>{trip.startDate?.slice(0, 10)} - {trip.endDate?.slice(0, 10)}</span>
                                        {trip.route && <span>{trip.route.startingCity} to {trip.route.endCity} | {trip.route.length} km</span>}
                                    </div>
                                    <button className="btn btn-danger" onClick={() => deleteTrip(trip.id)}>Delete</button>
                                </article>
                            ))}
                        </div>
                    </section>
                </div>
            </section>
        </div>
    );
};
