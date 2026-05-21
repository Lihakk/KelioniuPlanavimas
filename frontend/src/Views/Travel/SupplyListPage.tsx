import { useEffect, useState } from 'react';
import api from '../../api/api';
import type { SupplyList, Trip } from '../../types';

export const SupplyListPage = () => {
    const [trips, setTrips] = useState<Trip[]>([]);
    const [tripId, setTripId] = useState('');
    const [supplyList, setSupplyList] = useState<SupplyList | null>(null);

    const requestTripData = () => api.get<Trip[]>('/Trip').then(res => setTrips(res.data));

    const createSupplyList = async () => {
        if (!tripId) return;
        const res = await api.post<SupplyList>(`/SupplyList/trip/${tripId}`);
        setSupplyList(res.data);
    };

    const resetCurrentSupplyList = async () => {
        if (!supplyList?.id) return;
        const res = await api.post<SupplyList>(`/SupplyList/${supplyList.id}/resetCurrentSupplyList`);
        setSupplyList(res.data);
    };

    useEffect(() => { requestTripData(); }, []);

    return (
        <div className="container page-grid">
            <section>
                <h2>Supply list</h2>
                <div className="data-list">
                    {supplyList?.items.map(item => (
                        <article className="data-card" key={item.id ?? item.name}>
                            <strong>{item.name}</strong>
                            <span>{item.type}</span>
                            <span>Quantity: {item.quantity}</span>
                        </article>
                    ))}
                </div>
            </section>

            <section className="panel">
                <h3>Create supply list</h3>
                <label>Trip</label>
                <select value={tripId} onChange={e => setTripId(e.target.value)}>
                    <option value="">Select trip</option>
                    {trips.map(trip => <option key={trip.id} value={trip.id}>{trip.name}</option>)}
                </select>
                <div className="inline-actions">
                    <button className="btn btn-primary" onClick={createSupplyList}>Create</button>
                    <button className="btn btn-outline" onClick={resetCurrentSupplyList}>Reset</button>
                </div>
            </section>
        </div>
    );
};
