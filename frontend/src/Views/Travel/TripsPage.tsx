import { useEffect, useMemo, useState } from 'react';
import api from '../../api/api';
import type { TravelOffer, TravelRoute, Trip } from '../../types';

const today = new Date().toISOString().slice(0, 10);

export const TripsPage = () => {
    const [trips, setTrips] = useState<Trip[]>([]);
    const [routes, setRoutes] = useState<TravelRoute[]>([]);
    const [routeId, setRouteId] = useState('');
    const [tripName, setTripName] = useState('My planned trip');
    const [startDate, setStartDate] = useState(today);
    const [endDate, setEndDate] = useState(today);
    const [activeTripId, setActiveTripId] = useState('');
    const [accommodations, setAccommodations] = useState<TravelOffer[]>([]);
    const [flights, setFlights] = useState<TravelOffer[]>([]);
    const [cars, setCars] = useState<TravelOffer[]>([]);
    const [isCreating, setIsCreating] = useState(false);
    const [isLoadingOffers, setIsLoadingOffers] = useState(false);
    const [message, setMessage] = useState('');
    const [serviceView, setServiceView] = useState<'accommodation' | 'flight' | 'car'>('accommodation');

    const selectedRoute = useMemo(
        () => routes.find(route => String(route.id) === routeId) ?? null,
        [routes, routeId]
    );

    const activeTrip = useMemo(
        () => trips.find(trip => String(trip.id) === activeTripId) ?? null,
        [trips, activeTripId]
    );

    const getTrips = () => api.get<Trip[]>('/Trip')
        .then(res => {
            setTrips(res.data);
            if (!activeTripId && res.data[0]?.id) {
                setActiveTripId(String(res.data[0].id));
            }
        })
        .catch(() => {
            setTrips([]);
            setMessage('Trip list is unavailable until the backend database is running.');
        });

    const getRoutes = () => api.get<TravelRoute[]>('/Route')
        .then(res => {
            setRoutes(res.data);
            if (!routeId && res.data[0]?.id) {
                setRouteId(String(res.data[0].id));
                setTripName(res.data[0].name);
            }
        })
        .catch(() => {
            setRoutes([]);
            setMessage('Create a route first before creating trips.');
        });

    const requestTravelOffers = async (route: TravelRoute | null = selectedRoute) => {
        if (!route) {
            setMessage('Select a route first.');
            return;
        }

        setIsLoadingOffers(true);
        setMessage('');
        const payload = {
            startCity: route.startingCity,
            destinationCity: route.endCity,
            startDate,
            endDate
        };

        try {
            const [accommodationResponse, flightResponse, carResponse] = await Promise.all([
                api.post<TravelOffer[]>('/Trip/accommodation/list', payload),
                api.post<TravelOffer[]>('/Trip/flight/list', payload),
                api.post<TravelOffer[]>('/Trip/car/list', payload)
            ]);

            setAccommodations(accommodationResponse.data);
            setFlights(flightResponse.data);
            setCars(carResponse.data);
            setMessage('Hotel, flight and car choices loaded for the selected route.');
        } catch (error) {
            console.error(error);
            setMessage('Could not load travel service choices.');
        } finally {
            setIsLoadingOffers(false);
        }
    };

    const createTripFromSelection = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedRoute) {
            setMessage('Select a route first.');
            return;
        }

        setIsCreating(true);
        setMessage('');
        try {
            const tripResponse = await api.post<Trip>('/Trip', {
                name: tripName || selectedRoute.name,
                tripStatus: 'Planned',
                startDate,
                endDate,
                routeId: selectedRoute.id,
                selectedAccommodation: '',
                selectedFlight: '',
                selectedCar: ''
            });

            if (tripResponse.data.id) {
                setActiveTripId(String(tripResponse.data.id));
            }
            setMessage('Trip created from selected route.');
            await getTrips();
            await requestTravelOffers(selectedRoute);
        } catch (error) {
            console.error(error);
            setMessage('Could not save the trip selection.');
        } finally {
            setIsCreating(false);
        }
    };

    const deleteTrip = async (id?: number) => {
        if (!id) return;
        await api.delete(`/Trip/${id}`);
        await getTrips();
    };

    const assignOffer = async (type: 'accommodation' | 'flight' | 'car', offer: TravelOffer) => {
        if (!activeTripId) {
            setMessage('Select or create a trip first.');
            return;
        }

        await api.post(`/Trip/${activeTripId}/${type}`, offer);
        setMessage(`${offer.name} assigned to trip.`);
        await getTrips();
    };

    useEffect(() => {
        getRoutes();
        getTrips();
    }, []);

    useEffect(() => {
        if (selectedRoute) {
            requestTravelOffers(selectedRoute);
        }
    }, [routeId]);

    return (
        <div className="trip-workspace">
            <section className="planner-hero trip-selection-hero">
                <div>
                    <span className="eyebrow">Trip selection</span>
                    <h1>Select route, hotel, flight and car</h1>
                    <p>Routes and POIs are edited on the Routes page. Trips only combine the finished route with travel services.</p>
                </div>
                <button className="btn btn-outline" onClick={() => requestTravelOffers()} disabled={isLoadingOffers || !selectedRoute}>
                    {isLoadingOffers ? 'Loading choices...' : 'Refresh service choices'}
                </button>
            </section>

            <section className="trip-selection-layout">
                <form className="trip-selection-panel" onSubmit={createTripFromSelection}>
                    <h2>Trip setup</h2>
                    <label>Route</label>
                    <select value={routeId} onChange={e => {
                        setRouteId(e.target.value);
                        const route = routes.find(item => String(item.id) === e.target.value);
                        if (route) setTripName(route.name);
                    }} required>
                        <option value="">Select route</option>
                        {routes.map(route => (
                            <option key={route.id} value={route.id}>
                                {route.name} | {route.length} km | {route.routePoints?.length ?? 0} POIs
                            </option>
                        ))}
                    </select>
                    {routes.length === 0 && <div className="empty-state">Create a route on the Routes page first.</div>}

                    {selectedRoute && (
                        <div className="route-choice-card">
                            <span>{selectedRoute.startingCity} to {selectedRoute.endCity}</span>
                            <strong>{selectedRoute.length} km</strong>
                            <span>{selectedRoute.travelTime || 'Time not estimated'} | {selectedRoute.routePoints?.length ?? 0} route POIs</span>
                        </div>
                    )}

                    <label>Trip name</label>
                    <input value={tripName} onChange={e => setTripName(e.target.value)} required />

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

                    <button className="btn btn-primary" type="submit" disabled={isCreating || !selectedRoute}>
                        {isCreating ? 'Saving trip...' : 'Create trip from selection'}
                    </button>
                    {message && <p className="status-line">{message}</p>}
                </form>

                <section className="trip-current-card">
                    <span className="eyebrow">Active trip</span>
                    <select value={activeTripId} onChange={e => setActiveTripId(e.target.value)} aria-label="Active trip">
                        <option value="">Select saved trip</option>
                        {trips.map(trip => <option key={trip.id} value={trip.id}>{trip.name}</option>)}
                    </select>
                    {activeTrip ? (
                        <div className="selection-summary">
                            <strong>{activeTrip.name}</strong>
                            <span>{activeTrip.route?.startingCity} to {activeTrip.route?.endCity}</span>
                            <span>Hotel: {activeTrip.selectedAccommodation || 'Not selected'}</span>
                            <span>Flight: {activeTrip.selectedFlight || 'Not selected'}</span>
                            <span>Car: {activeTrip.selectedCar || 'Not selected'}</span>
                        </div>
                    ) : (
                        <div className="empty-state">Create or select a trip before assigning services.</div>
                    )}
                </section>
            </section>

            <section className="service-marketplace trip-service-marketplace">
                <div className="section-title">
                    <div>
                        <span className="eyebrow">Hotel, flight and car choices</span>
                        <h2>{selectedRoute ? `${selectedRoute.startingCity} to ${selectedRoute.endCity}` : 'Select a route'}</h2>
                    </div>
                </div>

                <div className="service-tabs">
                    <button className={serviceView === 'accommodation' ? 'active' : ''} onClick={() => setServiceView('accommodation')}>Hotels</button>
                    <button className={serviceView === 'flight' ? 'active' : ''} onClick={() => setServiceView('flight')}>Flights</button>
                    <button className={serviceView === 'car' ? 'active' : ''} onClick={() => setServiceView('car')}>Cars</button>
                </div>

                <div className="featured-offers">
                    {serviceView === 'accommodation' && <OfferColumn title="Hotels" offers={accommodations} selectedValue={activeTrip?.selectedAccommodation} onSelect={offer => assignOffer('accommodation', offer)} />}
                    {serviceView === 'flight' && <OfferColumn title="Flights" offers={flights} selectedValue={activeTrip?.selectedFlight} onSelect={offer => assignOffer('flight', offer)} />}
                    {serviceView === 'car' && <OfferColumn title="Cars" offers={cars} selectedValue={activeTrip?.selectedCar} onSelect={offer => assignOffer('car', offer)} />}
                </div>
            </section>

            <section className="panel">
                <div className="section-title">
                    <h2>Saved trips</h2>
                    <button className="btn btn-outline" onClick={getTrips}>Refresh</button>
                </div>
                <div className="trip-list">
                    {trips.map(trip => (
                        <article className={`trip-row ${activeTripId === String(trip.id) ? 'selected-row' : ''}`} key={trip.id}>
                            <div>
                                <strong>{trip.name}</strong>
                                <span>{trip.startDate?.slice(0, 10)} - {trip.endDate?.slice(0, 10)}</span>
                                {trip.route && <span>{trip.route.startingCity} to {trip.route.endCity} | {trip.route.length} km</span>}
                                {(trip.selectedAccommodation || trip.selectedFlight || trip.selectedCar) && (
                                    <span>{[trip.selectedFlight, trip.selectedAccommodation, trip.selectedCar].filter(Boolean).join(' | ')}</span>
                                )}
                            </div>
                            <div className="inline-actions">
                                <button className="btn btn-outline" onClick={() => setActiveTripId(String(trip.id))}>Select</button>
                                <button className="btn btn-danger" onClick={() => deleteTrip(trip.id)}>Delete</button>
                            </div>
                        </article>
                    ))}
                    {trips.length === 0 && <div className="empty-state">No saved trips yet. Select a route and create one.</div>}
                </div>
            </section>
        </div>
    );
};

const OfferColumn = ({
    title,
    offers,
    selectedValue,
    onSelect
}: {
    title: string;
    offers: TravelOffer[];
    selectedValue?: string;
    onSelect: (offer: TravelOffer) => void;
}) => (
    <section className="offer-column">
        <h3>{title}</h3>
        {offers.map(offer => (
            <article className={`offer-card ${selectedValue === offer.name ? 'selected-offer' : ''}`} key={`${title}-${offer.provider}-${offer.name}`}>
                <div>
                    <span>{offer.provider}</span>
                    <strong>{offer.name}</strong>
                    <small>{offer.description || 'Offer details'}</small>
                </div>
                <footer>
                    <b>{offer.price} EUR</b>
                    <button className="btn btn-outline" onClick={() => onSelect(offer)}>
                        {selectedValue === offer.name ? 'Selected' : 'Select'}
                    </button>
                </footer>
            </article>
        ))}
        {offers.length === 0 && <div className="empty-state">Choices are loading or unavailable.</div>}
    </section>
);
