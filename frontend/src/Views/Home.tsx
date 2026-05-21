import { Link } from 'react-router-dom';

const flows = [
    { code: 'P1-P3, P7, P11, P13', title: 'Routes', text: 'Create and edit OSM routes, select road POIs, recalculate distance and time.', href: '/routes' },
    { code: 'P4-P6, P12, P14', title: 'Trips', text: 'Select a finished route, then choose hotel, flight and car.', href: '/trips' },
    { code: 'M1-M3', title: 'Payments', text: 'Create payment requests, process payments and request refunds.', href: '/payments' },
    { code: 'P8-P9, K1-K2', title: 'Reviews', text: 'Review completed trips and personalize recommendations.', href: '/reviews' },
    { code: 'P10', title: 'Supplies', text: 'Generate and update a supply list from trip and weather data.', href: '/supplies' },
    { code: 'K3-K7, A1', title: 'Accounts', text: 'Register, login, edit profile and administer accounts.', href: '/accounts' }
];

export const Home = () => {
    return (
        <div className="workspace">
            <section className="home-hero">
                <div>
                    <span className="eyebrow">Travel planning system</span>
                    <h1>Kelioniu planuoklis</h1>
                    <p>Routes handle map planning and POIs. Trips combine the finished route with hotel, flight and car choices.</p>
                </div>
                <div className="hero-actions">
                    <Link to="/routes" className="btn btn-primary">Build route</Link>
                    <Link to="/trips" className="btn btn-outline">Select trip</Link>
                </div>
            </section>

            <section className="flow-board">
                {flows.map(flow => (
                    <Link to={flow.href} className="flow-card" key={flow.code}>
                        <span>{flow.code}</span>
                        <strong>{flow.title}</strong>
                        <small>{flow.text}</small>
                    </Link>
                ))}
            </section>
        </div>
    );
};
