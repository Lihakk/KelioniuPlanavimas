import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../../../api/api';
import type { PointOfInterest } from '../../../types';

export const POIList = () => {
    const [pois, setPois] = useState<PointOfInterest[]>([]);

    const loadData = () => api.get<PointOfInterest[]>('/POI').then(res => setPois(res.data));
    useEffect(() => { loadData(); }, []);

    return (
        <div className="container">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h2>Lankytinų vietų sąrašas</h2>
            </div>

            <div className="poi-grid">
                <Link to="/create" className="btn btn-primary">Pridėti naują lankytiną vietą</Link>
                {pois.map(p => (
                    <div key={p.id} className="poi-card">
                        <div>
                            <span className="badge-type" style={{ fontSize: '0.7rem' }}>{p.type}</span>
                            <h3 style={{ margin: '10px 0' }}>{p.name}</h3>
                            <p style={{ fontSize: '0.9rem', color: '#555' }}>📍 {p.address}</p>
                        </div>
                        
                        <div className="card-actions">
                            <Link to={`/view/${p.id}`} className="btn btn-primary" style={{ width: '100%', textAlign: 'center' }}>
                                Peržiūrėti
                            </Link>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};