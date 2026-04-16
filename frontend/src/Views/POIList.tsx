import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../api/api';
import type { PointOfInterest } from '../types';

export const POIList = () => {
    const [pois, setPois] = useState<PointOfInterest[]>([]);

    const loadData = () => api.get('/POI').then(res => setPois(res.data));

    useEffect(() => { loadData(); }, []);

    const handleDelete = async (id: number) => {
        if (window.confirm("Delete this POI?")) { 
            await api.delete(`/POI/${id}`); 
            loadData();
        }
    };

    return (
        <div>
            <h2>Points of Interest</h2>
            <ul>
                {pois.map(p => (
                    <li key={p.id} style={{ marginBottom: '10px' }}>
                        <strong>{p.name}</strong> ({p.type}) 
                        <Link to={`/view/${p.id}`}> [Details]</Link>
                        <Link to={`/edit/${p.id}`}> [Edit]</Link>
                        <button onClick={() => handleDelete(p.id!)}>Delete</button>
                    </li>
                ))}
            </ul>
        </div>
    );
};