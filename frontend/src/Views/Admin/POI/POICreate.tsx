import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../../api/api';
import type { PointOfInterest } from '../../../types';

export const POICreate = () => {
    const navigate = useNavigate();
    const [form, setForm] = useState<PointOfInterest>({
        name: '', type: '', address: '', hasTicket: false,
        workingHours: '', rating: 0, longitude: '', latitude: ''
    });

    const save = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await api.post('/POI', form);
            navigate('/list');
        } catch (err) { 
            console.error("Klaida saugant:", err); 
            alert("Nepavyko išsaugoti vietos.");
        }
    };

    return (
        <div className="form-container">
            <h2>➕ Sukurti naują lankytiną vietą</h2>
            <form onSubmit={save}>
                <div className="form-group">
                    <label>Pavadinimas</label>
                    <input 
                        className="form-control" 
                        placeholder="pvz. Gedimino pilis"
                        value={form.name} 
                        onChange={e => setForm({...form, name: e.target.value})} 
                        required 
                    />
                </div>

                <div className="form-group">
                    <label>Tipas (pvz. Muziejus, Parkas)</label>
                    <input 
                        className="form-control" 
                        placeholder="Įveskite vietos tipą"
                        value={form.type} 
                        onChange={e => setForm({...form, type: e.target.value})} 
                    />
                </div>

                <div className="form-group">
                    <label>Adresas</label>
                    <input 
                        className="form-control" 
                        placeholder="Gatvė, miestas"
                        value={form.address} 
                        onChange={e => setForm({...form, address: e.target.value})} 
                    />
                </div>

                <div className="form-group">
                    <label>Darbo valandos</label>
                    <input 
                        className="form-control" 
                        placeholder="pvz. 09:00 - 18:00" 
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
                    <label>Įvertinimas (0-5)</label>
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
                        id="hasTicket"
                        checked={form.hasTicket} 
                        onChange={e => setForm({...form, hasTicket: e.target.checked})} 
                    />
                    <label htmlFor="hasTicket">Ar reikalingas bilietas?</label>
                </div>

                <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>
                    Išsaugoti vietą
                </button>
            </form>
        </div>
    );
};