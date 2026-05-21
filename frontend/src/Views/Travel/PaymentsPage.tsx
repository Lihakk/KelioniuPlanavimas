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

    const getPayments = () => api.get<Payment[]>('/Payment').then(res => setPayments(res.data));
    const getTrips = () => api.get<Trip[]>('/Trip').then(res => setTrips(res.data));

    const saveReservationData = async (e: React.FormEvent) => {
        e.preventDefault();
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
        await getPayments();
    };

    const processPayment = async (id?: number) => {
        if (!id) return;
        await api.post(`/Payment/${id}/processPayment`);
        await getPayments();
    };

    const requestRefund = async (id?: number) => {
        if (!id) return;
        await api.post(`/Payment/${id}/requestRefund`);
        await getPayments();
    };

    useEffect(() => {
        getTrips();
        getPayments();
        api.get<Reservation[]>('/Payment/trip/0/reservations').catch(() => setReservations([]));
    }, []);

    return (
        <div className="container page-grid">
            <section>
                <h2>Payments</h2>
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
                </div>
                {reservations.length > 0 && <small>{reservations.length} reservations loaded</small>}
            </section>

            <section className="panel">
                <h3>Create reservation payment</h3>
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
            </section>
        </div>
    );
};
