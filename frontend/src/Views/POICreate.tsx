import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/api';
import type { PointOfInterest } from '../types';

export const POICreate = () => {
    const navigate = useNavigate();
    const [form, setForm] = useState<PointOfInterest>({
        name: '', type: '', address: '', hasTicket: false,
        workingHours: '', rating: 0, longitude: '', latitude: ''
    });

    const save = async (e: React.FormEvent) => {
        e.preventDefault();
        await api.post('/POI', form); 
        navigate('/');
    };

    return (
        <form onSubmit={save}>
            <h2>Create New POI</h2>
            <input placeholder="Name" onChange={e => setForm({...form, name: e.target.value})} required /><br/>
            <input placeholder="Type" onChange={e => setForm({...form, type: e.target.value})} /><br/>
            <input placeholder="Address" onChange={e => setForm({...form, address: e.target.value})} /><br/>
            <button type="submit">Save</button>
        </form>
    );
};