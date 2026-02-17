import { useEffect, useState } from 'react';
import ProductList from './components/ProductList';
import Cart from './components/Cart';
import LatencyWidget from './components/LatencyWidget';
import './App.css';

function App() {
  const [sessionId, setSessionId] = useState('');

  useEffect(() => {
    // Generate a session ID for the user if one doesn't exist
    let storedSession = localStorage.getItem('styleverse_session');
    if (!storedSession) {
      storedSession = 'user-' + Math.random().toString(36).substr(2, 9);
      localStorage.setItem('styleverse_session', storedSession);
    }
    setSessionId(storedSession);
  }, []);

  return (
    <div className="app-container">
      <header className="header">
        <h1>StyleVerse <span className="beta-tag">BETA</span></h1>
        <p>Global Fashion. Local Lag.</p>
      </header>
      
      <main className="main-content">
        <div className="shop-section">
          <h2>New Arrivals</h2>
          <ProductList sessionId={sessionId} />
        </div>
        
        <div className="cart-section">
          <h2>Your Cart</h2>
          <Cart sessionId={sessionId} />
        </div>
      </main>

      {/* The Widget tracking our optimization progress */}
      <LatencyWidget />
    </div>
  );
}

export default App;