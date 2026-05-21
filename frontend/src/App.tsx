import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import { Home } from './Views/Home';
import { POIList } from './Views/Admin/POI/POIList';
import { POICreate } from './Views/Admin/POI/POICreate';
import { POIView } from './Views/Admin/POI/POIView';
import { POIEdit } from './Views/Admin/POI/POIEdit';
import { RoutesPage } from './Views/Travel/RoutesPage';
import { TripsPage } from './Views/Travel/TripsPage';
import { PaymentsPage } from './Views/Travel/PaymentsPage';
import { ReviewsPage } from './Views/Travel/ReviewsPage';
import { SupplyListPage } from './Views/Travel/SupplyListPage';
import './App.css';

function App() {
  return (
    <Router>
      <nav className="navbar">
        <Link to="/" className="nav-link logo">Keliones</Link>
        <div className="nav-items">
          <Link to="/" className="nav-link">Main</Link>
          <Link to="/list" className="nav-link">POI</Link>
          <Link to="/routes" className="nav-link">Routes</Link>
          <Link to="/trips" className="nav-link">Trips</Link>
          <Link to="/payments" className="nav-link">Payments</Link>
          <Link to="/reviews" className="nav-link">Reviews</Link>
          <Link to="/supplies" className="nav-link">Supplies</Link>
        </div>
      </nav>

      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/list" element={<POIList />} />
        <Route path="/create" element={<POICreate />} />
        <Route path="/view/:id" element={<POIView />} />
        <Route path="/edit/:id" element={<POIEdit />} />
        <Route path="/routes" element={<RoutesPage />} />
        <Route path="/trips" element={<TripsPage />} />
        <Route path="/payments" element={<PaymentsPage />} />
        <Route path="/reviews" element={<ReviewsPage />} />
        <Route path="/supplies" element={<SupplyListPage />} />
      </Routes>
    </Router>
  );
}

export default App;
