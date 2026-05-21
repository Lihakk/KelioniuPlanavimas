import { BrowserRouter as Router, Routes, Route, NavLink } from 'react-router-dom';
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
import { AccountsPage } from './Views/Travel/AccountsPage';
import './App.css';

function App() {
  const navClass = ({ isActive }: { isActive: boolean }) => `nav-link${isActive ? ' active' : ''}`;

  return (
    <Router>
      <nav className="navbar">
        <NavLink to="/" className={({ isActive }) => `nav-link logo${isActive ? ' active' : ''}`}>Keliones</NavLink>
        <div className="nav-items">
          <NavLink to="/" className={navClass}>Main</NavLink>
          <NavLink to="/trips" className={navClass}>Trips</NavLink>
          <NavLink to="/routes" className={navClass}>Routes</NavLink>
          <NavLink to="/payments" className={navClass}>Payments</NavLink>
          <NavLink to="/reviews" className={navClass}>Reviews</NavLink>
          <NavLink to="/supplies" className={navClass}>Supplies</NavLink>
          <NavLink to="/list" className={navClass}>POI</NavLink>
          <NavLink to="/accounts" className={navClass}>Accounts</NavLink>
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
        <Route path="/accounts" element={<AccountsPage />} />
      </Routes>
    </Router>
  );
}

export default App;
