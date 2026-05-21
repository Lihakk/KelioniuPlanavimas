import { Link } from 'react-router-dom';

export const Home = () => {
    return (
        <div className="container">
            <section className="hero">
                <h1>Kelioniu planuoklis</h1>
                <p style={{ fontSize: '1.2rem', color: '#555', marginBottom: '2rem' }}>
                    Trip planning, routes, POI, reservations, payments, reviews, and supply lists.
                </p>
                <div className="hero-actions">
                    <Link to="/trips" className="btn btn-primary">Open trips</Link>
                    <Link to="/routes" className="btn btn-outline">Open routes</Link>
                </div>
            </section>
        </div>
    );
};
