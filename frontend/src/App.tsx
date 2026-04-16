import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import { POIList } from './Views/POIList';
import { POICreate } from './Views/POICreate';
import { POIView } from './Views/POIView';
import { POIEdit } from './Views/POIEdit';

function App() {
  return (
    <Router>
      <nav style={{ padding: '1rem', borderBottom: '1px solid #ccc' }}>
        <Link to="/" style={{ marginRight: '15px' }}>📍 POI List</Link>
        <Link to="/create">➕ Add New</Link>
      </nav>

      <main style={{ padding: '20px' }}>
        <Routes>
          <Route path="/" element={<POIList />} />
          <Route path="/create" element={<POICreate />} />
          <Route path="/view/:id" element={<POIView />} />
          <Route path="/edit/:id" element={<POIEdit />} />
        </Routes>
      </main>
    </Router>
  );
}

export default App;