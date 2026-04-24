import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import api from '../../../api/api';
import type { PointOfInterest } from '../../../types';

export const POIEdit = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [loading, setLoading] = useState(true);
    const [form, setForm] = useState<PointOfInterest>({
        name: '', type: '', address: '', hasTicket: false,
        workingHours: '', rating: 0, longitude: '', latitude: ''
    });

    useEffect(() => {
        if (id) {
            api.get<PointOfInterest>(`/POI/${id}`)
                .then((res) => {
                    setForm(res.data);
                    setLoading(false);
                })
                .catch((err) => {
                    console.error("Could not fetch POI", err);
                    alert("Error loading data.");
                    navigate('/list');
                });
        }
    }, [id, navigate]);


    const handleUpdate = async (e: React.FormEvent) => {
        e.preventDefault();
        if (id) {
            try {
                const updatedData = { ...form, id: parseInt(id) };
                
                await api.put(`/POI/${id}`, updatedData);
                alert("Atnaujinta sėkmingai!");
                navigate(`/view/${id}`);
            } catch (err) {
                console.error("Update failed", err);
                alert("Klaida saugant duomenis.");
            }
        }
    };

    if (loading) return <div className="container">Kraunama...</div>;

    return (
        <div className="container">
            <div className="form-container">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                    <h2>✏️ Redaguoti: {form.name}</h2>
                    <Link to="/list" className="btn btn-outline">Atšaukti</Link>
                </div>

                <form onSubmit={handleUpdate}>
                    <div className="form-group">
                        <label>Pavadinimas</label>
                        <input 
                            className="form-control" 
                            value={form.name} 
                            onChange={e => setForm({...form, name: e.target.value})} 
                            required 
                        />
                    </div>

                    <div className="form-group">
                        <label>Tipas (pvz. Muziejus, Parkas)</label>
                        <input 
                            className="form-control" 
                            value={form.type} 
                            onChange={e => setForm({...form, type: e.target.value})} 
                        />
                    </div>

                    <div className="form-group">
                        <label>Adresas</label>
                        <input 
                            className="form-control" 
                            value={form.address} 
                            onChange={e => setForm({...form, address: e.target.value})} 
                        />
                    </div>

                    <div className="form-group">
                        <label>Darbo valandos</label>
                        <input 
                            className="form-control" 
                            value={form.workingHours} 
                            onChange={e => setForm({...form, workingHours: e.target.value})} 
                        />
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '15px' }}>
                        <div className="form-group">
                            <label>Platuma (Lat)</label>
                            <input 
                                className="form-control" 
                                value={form.latitude} 
                                onChange={e => setForm({...form, latitude: e.target.value})} 
                            />
                        </div>
                        <div className="form-group">
                            <label>Ilguma (Lon)</label>
                            <input 
                                className="form-control" 
                                value={form.longitude} 
                                onChange={e => setForm({...form, longitude: e.target.value})} 
                            />
                        </div>
                    </div>

                    <div className="form-group">
                        <label>Reitingas (0-5)</label>
                        <input 
                            type="number" 
                            step="0.1" 
                            min="0" 
                            max="5" 
                            className="form-control" 
                            value={form.rating} 
                            onChange={e => setForm({...form, rating: parseFloat(e.target.value)})} 
                        />
                    </div>

                    <div className="form-group checkbox-group">
                        <input 
                            type="checkbox" 
                            checked={form.hasTicket} 
                            onChange={e => setForm({...form, hasTicket: e.target.checked})} 
                        />
                        <label>Ar reikia bilieto?</label>
                    </div>

                    <button type="submit" className="btn btn-primary" style={{ width: '100%', marginTop: '10px' }}>
                        Išsaugoti pakeitimus
                    </button>
                </form>
            </div>
        </div>
    );
};