import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import { Home } from './Views/Home';
import { POIList } from './Views/Admin/POI/POIList';
import { POICreate } from './Views/Admin/POI/POICreate';
import { POIView } from './Views/Admin/POI/POIView';
import { POIEdit } from './Views/Admin/POI/POIEdit';
import './App.css';

function App() {
  return (
    <Router>
    <nav className="navbar">
      <Link to="/" className="nav-link logo">📍 Keliones</Link>
      <div>
        <Link to="/" className="nav-link">Main</Link>
        <Link to="/list" className="nav-link">POI</Link>
      </div>
    </nav>

      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/list" element={<POIList />} />
        <Route path="/create" element={<POICreate />} />
        <Route path="/view/:id" element={<POIView />} />
        <Route path="/edit/:id" element={<POIEdit />} />
      </Routes>
    </Router>
  );
}

export default App;