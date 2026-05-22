import { useEffect, useState } from 'react';
import api from '../../api/api';
import type { SupplyList, Trip } from '../../types';

export const SupplyListPage = () => {
    const [trips, setTrips] = useState<Trip[]>([]);
    const [tripId, setTripId] = useState('');
    const [supplyList, setSupplyList] = useState<SupplyList | null>(null);
    const [message, setMessage] = useState('');
    const [hasLaundry, setHasLaundry] = useState(false);

    const requestTripData = () =>
        api.get<Trip[]>('/Trip').then(res => setTrips(res.data));

    const loadSupplyListForTrip = async (selectedTripId: string) => {
        if (!selectedTripId) {
            setSupplyList(null);
            setMessage('');
            return;
        }

        try {
            const res = await api.get<SupplyList>(`/SupplyList/trip/${selectedTripId}`);
            setSupplyList(res.data);
            setMessage('Saved supply list loaded.');
        } catch (error) {
            console.error(error);
            setSupplyList(null);
            setMessage('No saved supply list for this trip yet.');
        }
    };

    const createSupplyList = async () => {
        if (!tripId) return;

        try {
            const res = await api.post<SupplyList>(`/SupplyList/trip/${tripId}`, {
                hasLaundry
            });

            setSupplyList(res.data);
            setMessage('Supply list generated from trip conditions.');
        } catch (error) {
            console.error(error);
            setMessage('Could not generate supply list.');
        }
    };

    const saveSupplyList = async () => {
        if (!supplyList?.id) return;

        try {
            await api.put(`/SupplyList/${supplyList.id}`, {
                items: supplyList.items.map(item => ({
                    id: item.id,
                    name: item.name,
                    type: item.type,
                    quantity: item.quantity
                }))
            });

            setMessage('Supply list saved.');
        } catch (error) {
            console.error(error);
            setMessage('Could not save supply list.');
        }
    };

    const regenerateSupplyList = async () => {
        if (!supplyList?.id) return;

        try {
            const res = await api.post<SupplyList>(
                `/SupplyList/${supplyList.id}/regenerate`,
                { hasLaundry }
            );

            setSupplyList(res.data);
            setMessage('Supply list regenerated.');
        } catch (error) {
            console.error(error);
            setMessage('Could not regenerate supply list.');
        }
    };

    const resetCurrentSupplyList = async () => {
        if (!supplyList?.id) return;

        try {
            const res = await api.post<SupplyList>(
                `/SupplyList/${supplyList.id}/resetCurrentSupplyList`
            );

            setSupplyList(res.data);
            setMessage('Current supply list reset.');
        } catch (error) {
            console.error(error);
            setMessage('Could not reset supply list.');
        }
    };

    const updateItemQuantity = (itemId: number | undefined, value: string) => {
        if (!supplyList) return;

        setSupplyList({
            ...supplyList,
            items: supplyList.items.map(item =>
                item.id === itemId
                    ? { ...item, quantity: Number(value) }
                    : item
            )
        });
    };

    const updateItemName = (itemId: number | undefined, value: string) => {
        if (!supplyList) return;

        setSupplyList({
            ...supplyList,
            items: supplyList.items.map(item =>
                item.id === itemId
                    ? { ...item, name: value }
                    : item
            )
        });
    };

    useEffect(() => {
        requestTripData();
    }, []);

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

                    {supplyList?.weatherSummary && (
                        <div className="info-box">
                            {supplyList.weatherSummary}
                        </div>
                    )}

                    {supplyList && (
                        <p className="status-line muted">
                            Created: {new Date(supplyList.dateCreated).toLocaleString()}
                        </p>
                    )}

                    <div className="data-list dense">
                        {supplyList?.items.map(item => (
                            <article className="data-card supply-item" key={item.id ?? item.name}>
                                <div className="supply-item-top">
                                    <input
                                        className="item-name-input"
                                        value={item.name}
                                        onChange={e => updateItemName(item.id, e.target.value)}
                                    />

                                    <span className="item-type">{item.type}</span>
                                </div>

                                <div className="supply-item-bottom">
                                    <div className="quantity-group">
                                        <label>Qty</label>

                                        <input
                                            className="quantity-input"
                                            type="number"
                                            min="1"
                                            value={item.quantity}
                                            onChange={e => updateItemQuantity(item.id, e.target.value)}
                                        />
                                    </div>
                                </div>

                                {item.reason && (
                                    <small className="item-reason">{item.reason}</small>
                                )}
                            </article>
                        ))}

                        {!supplyList && (
                            <div className="empty-state">
                                Select a trip and create the list.
                            </div>
                        )}
                    </div>
                </section>

                <section className="panel">
                    <h2>Create supply list</h2>

                    <div className="form-group">
                        <label>Trip</label>

                        <select
                            value={tripId}
                            onChange={e => {
                                const selectedTripId = e.target.value;
                                setTripId(selectedTripId);
                                loadSupplyListForTrip(selectedTripId);
                            }}
                        >
                            <option value="">Select trip</option>
                            {trips.map(trip => (
                                <option key={trip.id} value={trip.id}>
                                    {trip.name}
                                </option>
                            ))}
                        </select>
                    </div>

                    <label className="checkbox-row">
                        <input
                            type="checkbox"
                            checked={hasLaundry}
                            onChange={e => setHasLaundry(e.target.checked)}
                        />
                        <span>Accommodation has laundry</span>
                    </label>

                    <div className="supply-actions">
                        <button className="btn btn-primary" onClick={createSupplyList}>
                            Create
                        </button>

                        <button className="btn btn-outline" onClick={saveSupplyList}>
                            Save
                        </button>

                        <button className="btn btn-outline" onClick={regenerateSupplyList}>
                            Regenerate
                        </button>

                        <button className="btn btn-outline" onClick={resetCurrentSupplyList}>
                            Reset
                        </button>
                    </div>

                    {message && <p className="status-line">{message}</p>}
                </section>
            </div>
        </div>
    );
};