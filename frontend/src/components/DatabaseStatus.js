import React, { useState, useEffect } from 'react';
import axios from 'axios';

function DatabaseStatus() {
  const [status, setStatus] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchStatus = async () => {
      try {
        setLoading(true);
        const response = await axios.get('/api/db-status');
        setStatus(response.data);
        setError(null);
      } catch (err) {
        console.error('Error fetching database status:', err);
        setError('Failed to fetch database status');
      } finally {
        setLoading(false);
      }
    };

    fetchStatus();
    
    const interval = setInterval(fetchStatus, 30000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="status-container">
      <h2>Database Status</h2>
      
      {loading && (
        <div className="status-card loading">
          Loading database status...
        </div>
      )}
      
      {error && (
        <div className="status-card error">
          {error}
        </div>
      )}
      
      {!loading && !error && status && (
        <div className="status-card">
          <div className="status-item">
            <span className="status-label">Connection:</span>
            <span className={`status-value ${status.canConnect ? 'success' : 'failure'}`}>
              {status.canConnect ? 'Connected' : 'Disconnected'}
            </span>
          </div>
          
          <div className="status-item">
            <span className="status-label">Pending Migrations:</span>
            <span className="status-value">
              {status.pendingMigrations.length === 0 ? 
                'None' : 
                <ul className="migrations-list">
                  {status.pendingMigrations.map((migration, index) => (
                    <li key={index}>{migration}</li>
                  ))}
                </ul>
              }
            </span>
          </div>
          
          <div className="status-item">
            <span className="status-label">Applied Migrations:</span>
            <span className="status-value">
              {status.appliedMigrations.length === 0 ? 
                'None' : 
                <ul className="migrations-list">
                  {status.appliedMigrations.map((migration, index) => (
                    <li key={index}>{migration}</li>
                  ))}
                </ul>
              }
            </span>
          </div>
        </div>
      )}
    </div>
  );
}

export default DatabaseStatus;