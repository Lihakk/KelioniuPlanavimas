import { useEffect, useState } from 'react';
import api from '../../api/api';
import type { SupplyList, Trip } from '../../types';

export const SupplyListPage = () => {
    const [trips, setTrips] = useState<Trip[]>([]);
    const [tripId, setTripId] = useState('');
    const [supplyList, setSupplyList] = useState<SupplyList | null>(null);
    const [message, setMessage] = useState('');

    const requestTripData = () => api.get<Trip[]>('/Trip').then(res => setTrips(res.data));

    const createSupplyList = async () => {
        if (!tripId) return;
        try {
            const res = await api.post<SupplyList>(`/SupplyList/trip/${tripId}`);
            setSupplyList(res.data);
            setMessage('Supply list generated from trip conditions.');
        } catch (error) {
            console.error(error);
            setMessage('Could not generate supply list.');
        }
    };

    const resetCurrentSupplyList = async () => {
        if (!supplyList?.id) return;
        const res = await api.post<SupplyList>(`/SupplyList/${supplyList.id}/resetCurrentSupplyList`);
        setSupplyList(res.data);
        setMessage('Current supply list reset.');
    };

    useEffect(() => { requestTripData(); }, []);

    return (
        <div className="workspace">
            <header className="page-head">
                <span className="eyebrow">Supply diagram P10</span>
                <h1>Trip supply list</h1>
            </header>

            <div className="page-grid">
            <section className="panel">
                <div className="section-title">
                    <h2>Generated items</h2>
                    {supplyList && <span>{supplyList.items.length} items</span>}
                </div>
                <div className="data-list dense">
                    {supplyList?.items.map(item => (
                        <article className="data-card" key={item.id ?? item.name}>
                            <strong>{item.name}</strong>
                            <span>{item.type}</span>
                            <span>Quantity: {item.quantity}</span>
                        </article>
                    ))}
                    {!supplyList && <div className="empty-state">Select a trip and create the list.</div>}
                </div>
            </section>

            <section className="panel">
                <h2>Create supply list</h2>
                <label>Trip</label>
                <select value={tripId} onChange={e => setTripId(e.target.value)}>
                    <option value="">Select trip</option>
                    {trips.map(trip => <option key={trip.id} value={trip.id}>{trip.name}</option>)}
                </select>
                <div className="inline-actions">
                    <button className="btn btn-primary" onClick={createSupplyList}>Create</button>
                    <button className="btn btn-outline" onClick={resetCurrentSupplyList}>Reset</button>
                </div>
                {message && <p className="status-line">{message}</p>}
            </section>
            </div>
        </div>
    );
};
