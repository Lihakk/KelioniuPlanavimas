import { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import api from '../../../api/api';
import type { PointOfInterest } from '../../../types';

export const POIView = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [poi, setPoi] = useState<PointOfInterest | null>(null);

    useEffect(() => {
        api.get<PointOfInterest>(`/POI/${id}`).then(res => setPoi(res.data));
    }, [id]);

    const handleDelete = async () => {
        if (window.confirm(`Ar tikrai norite ištrinti "${poi?.name}"?`)) {
            try {
                await api.delete(`/POI/${id}`);
                navigate('/list'); // Go back to list after successful deletion
            } catch (err) {
                console.error("Ištrynimas nepavyko", err);
                alert("Klaida trinant įrašą.");
            }
        }
    };

    if (!poi) return <div className="container">Kraunama informacija...</div>;

    return (
        <div className="container">
            <Link to="/list" className="nav-link" style={{ marginLeft: 0, color: 'var(--primary)', marginBottom: '1rem', display: 'inline-block' }}>
                ← Atgal į sąrašą
            </Link>

            <div className="view-card">
                <div className="view-header">
                    <div>
                        <h1 style={{ margin: 0 }}>{poi.name}</h1>
                        <p style={{ color: '#666', marginTop: '5px' }}>📍 {poi.address || 'Nėra adreso'}</p>
                    </div>
                    <span className="badge-type">{poi.type}</span>
                </div>

                <div className="info-grid">
                    <div className="info-item">
                        <span className="info-label">Darbo laikas</span>
                        <span className="info-value">🕒 {poi.workingHours || 'Nenurodyta'}</span>
                    </div>
                    <div className="info-item">
                        <span className="info-label">Įvertinimas</span>
                        <span className="info-value">⭐ {poi.rating}/5</span>
                    </div>
                    <div className="info-item">
                        <span className="info-label">Bilietai</span>
                        <span className="info-value">{poi.hasTicket ? "🎫 Reikalingas" : "🆓 Nemokama"}</span>
                    </div>
                    <div className="info-item">
                        <span className="info-label">Koordinatės</span>
                        <span className="info-value">{poi.latitude}, {poi.longitude}</span>
                    </div>
                </div>
                <div style={{ marginTop: '40px', display: 'flex', gap: '15px' }}>
                    <Link to={`/edit/${poi.id}`} className="btn btn-outline" style={{ flex: 1, textAlign: 'center' }}>
                        Redaguoti
                    </Link>
                    <button onClick={handleDelete} className="btn btn-danger" style={{ flex: 1 }}>
                        Ištrinti tašką
                    </button>
                </div>
            </div>
        </div>
    );
};