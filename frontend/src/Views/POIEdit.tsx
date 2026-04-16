import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/api';
import type { PointOfInterest } from '../types';

export const POIEdit = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [form, setForm] = useState<PointOfInterest | null>(null);

    useEffect(() => {
        api.get(`/POI/${id}`).then(res => setForm(res.data)); 
    }, [id]);

    const update = async (e: React.FormEvent) => {
        e.preventDefault();
        if (form) {
            await api.put(`/POI/${id}`, form);
            navigate('/');
        }
    };

    if (!form) return <p>Loading...</p>;

    return (
        <form onSubmit={update}>
            <h2>Edit POI</h2>
            <input value={form.name} onChange={e => setForm({...form, name: e.target.value})} />
            <button type="submit">Update</button>
        </form>
    );
};