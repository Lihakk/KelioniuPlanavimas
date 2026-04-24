import { Link } from 'react-router-dom';

export const Home = () => {
    return (
        <div className="container">
            <section className="hero">
                <h1>🌍 Kelionių Planuoklis</h1>
                <p style={{ fontSize: '1.4rem', color: '#555', marginBottom: '2rem' }}>
                    POI CRUD.
                </p>
                <div className="hero-actions">
                    <Link to="/list" className="btn btn-primary" style={{ fontSize: '1.2rem' }}>
                        POI List
                    </Link>
                </div>
            </section>
        </div>
    );
};