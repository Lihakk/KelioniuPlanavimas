import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import api from '../api/api';
import type { PointOfInterest } from '../types';

export const POIView = () => {
    const { id } = useParams();
    const [poi, setPoi] = useState<PointOfInterest | null>(null);

    useEffect(() => {
        api.get(`/POI/${id}`).then(res => setPoi(res.data)); 
    }, [id]);

    if (!poi) return <p>Loading...</p>;

    return (
        <div>
            <h2>{poi.name}</h2>
            <p>Address: {poi.address}</p>
            <p>Rating: {poi.rating}/5</p>
            <Link to="/">Back</Link>
        </div>
    );
};