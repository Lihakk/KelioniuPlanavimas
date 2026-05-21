import { useEffect, useState } from 'react';
import api from '../../api/api';
import type { Payment, Reservation, ReservationType, Trip } from '../../types';

export const PaymentsPage = () => {
    const [trips, setTrips] = useState<Trip[]>([]);
    const [payments, setPayments] = useState<Payment[]>([]);
    const [reservations, setReservations] = useState<Reservation[]>([]);
    const [tripId, setTripId] = useState('');
    const [reservationType, setReservationType] = useState<ReservationType>('Flight');
    const [description, setDescription] = useState('');
    const [amount, setAmount] = useState(0);
    const [message, setMessage] = useState('');

    const getPayments = () => api.get<Payment[]>('/Payment')
        .then(res => setPayments(res.data))
        .catch(() => {
            setPayments([]);
            setMessage('Payment history is unavailable until the backend database is running.');
        });

    const getTrips = () => api.get<Trip[]>('/Trip')
        .then(res => {
            setTrips(res.data);
            if (!tripId && res.data[0]?.id) {
                setTripId(String(res.data[0].id));
            }
        })
        .catch(() => setTrips([]));

    const getReservations = (selectedTripId: string) => {
        if (!selectedTripId) {
            setReservations([]);
            return;
        }

        api.get<Reservation[]>(`/Payment/trip/${selectedTripId}/reservations`)
            .then(res => setReservations(res.data))
            .catch(() => setReservations([]));
    };

    const saveReservationData = async (e: React.FormEvent) => {
        e.preventDefault();
        setMessage('');
        try {
            const reservation = await api.post<Reservation>('/Payment/reservation', {
                tripId: Number(tripId),
                reservationDate: new Date().toISOString(),
                reservationStatus: 'WaitingForPayment',
                reservationType,
                provider: reservationType,
                description,
                price: amount
            });

            await api.post('/Payment', {
                tripId: Number(tripId),
                reservationId: reservation.data.id,
                date: new Date().toISOString(),
                amount,
                paymentStatus: 'Waiting'
            });

            setDescription('');
            setAmount(0);
            setMessage('Reservation and payment request created.');
            await getPayments();
            getReservations(tripId);
        } catch (error) {
            console.error(error);
            setMessage('Could not create reservation payment.');
        }
    };

    const processPayment = async (id?: number) => {
        if (!id) return;
        await api.post(`/Payment/${id}/processPayment`);
        setMessage('Payment completed.');
        await getPayments();
    };

    const requestRefund = async (id?: number) => {
        if (!id) return;
        await api.post(`/Payment/${id}/requestRefund`);
        setMessage('Refund requested and reservation updated.');
        await getPayments();
    };

    useEffect(() => {
        getTrips();
        getPayments();
    }, []);

    useEffect(() => { getReservations(tripId); }, [tripId]);

    return (
        <div className="workspace">
            <header className="page-head">
                <span className="eyebrow">Payment diagrams M1-M3</span>
                <h1>Reservations, payment and refunds</h1>
            </header>

            <div className="page-grid">
                <section>
                    <div className="section-title">
                        <h2>Payment history</h2>
                        <button className="btn btn-outline" onClick={getPayments}>Refresh</button>
                    </div>
                    <div className="data-list">
                        {payments.map(payment => (
                            <article className="data-card" key={payment.id}>
                                <strong>{payment.amount} EUR</strong>
                                <span>Status: {payment.paymentStatus}</span>
                                <span>Trip: {payment.tripId || 'Not linked'}</span>
                                <div className="inline-actions">
                                    <button className="btn btn-outline" onClick={() => processPayment(payment.id)}>Pay</button>
                                    <button className="btn btn-danger" onClick={() => requestRefund(payment.id)}>Refund</button>
                                </div>
                            </article>
                        ))}
                        {payments.length === 0 && <div className="empty-state">No payments yet.</div>}
                    </div>
                </section>

                <section className="panel">
                    <h2>Request payment</h2>
                    <form onSubmit={saveReservationData}>
                        <label>Trip</label>
                        <select value={tripId} onChange={e => setTripId(e.target.value)} required>
                            <option value="">Select trip</option>
                            {trips.map(trip => <option key={trip.id} value={trip.id}>{trip.name}</option>)}
                        </select>
                        <label>Reservation type</label>
                        <select value={reservationType} onChange={e => setReservationType(e.target.value as ReservationType)}>
                            <option value="Flight">Flight</option>
                            <option value="Accommodation">Accommodation</option>
                            <option value="Car">Car</option>
                        </select>
                        <label>Description</label>
                        <input value={description} onChange={e => setDescription(e.target.value)} required />
                        <label>Amount</label>
                        <input type="number" min="0" value={amount} onChange={e => setAmount(Number(e.target.value))} />
                        <button className="btn btn-primary" type="submit">Request payment</button>
                    </form>
                    {message && <p className="status-line">{message}</p>}

                    <div className="section-title stacked">
                        <h3>Trip reservations</h3>
                        <span>{reservations.length} loaded</span>
                    </div>
                    <div className="data-list dense">
                        {reservations.map(reservation => (
                            <article className="data-card" key={reservation.id}>
                                <strong>{reservation.description}</strong>
                                <span>{reservation.reservationType} | {reservation.reservationStatus}</span>
                                <span>{reservation.price} EUR</span>
                            </article>
                        ))}
                    </div>
                </section>
            </div>
        </div>
    );
};
