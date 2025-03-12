import React from 'react';
import './styles/App.css';
import DatabaseStatus from './components/DatabaseStatus';
import MeterReadingUpload from './components/MeterReadingUpload';

function App() {
  return (
    <div className="app">
      <header className="header">
        <h1>Meter Readings</h1>
      </header>
      <main className="content">
        <div className="column">
          <DatabaseStatus />
        </div>
        <div className="column">
          <MeterReadingUpload />
        </div>
      </main>
    </div>
  );
}

export default App;