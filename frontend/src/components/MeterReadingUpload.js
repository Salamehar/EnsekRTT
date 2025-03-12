import React, { useState } from 'react';
import { Pie } from 'react-chartjs-2';
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js';
import axios from 'axios';

// Register ChartJS components
ChartJS.register(ArcElement, Tooltip, Legend);

function MeterReadingUpload() {
  const [file, setFile] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [result, setResult] = useState(null);
  const [error, setError] = useState(null);

  const handleFileChange = (e) => {
    if (e.target.files[0]) {
      setFile(e.target.files[0]);
      setError(null);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!file) {
      setError('Please select a file to upload');
      return;
    }

    if (!file.name.toLowerCase().endsWith('.csv')) {
      setError('Only CSV files are supported');
      return;
    }

    try {
      setUploading(true);
      setError(null);
      
      const formData = new FormData();
      formData.append('file', file);
      
      const response = await axios.post('/api/meter-reading-uploads', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });
      
      setResult(response.data);
    } catch (err) {
      console.error('Upload error:', err);
      setError(err.response?.data || 'An error occurred while uploading the file');
    } finally {
      setUploading(false);
    }
  };

  const chartData = result ? {
    labels: ['Successful Readings', 'Failed Readings'],
    datasets: [
      {
        data: [result.successfulReadings, result.failedReadings],
        backgroundColor: ['#4caf50', '#e53935'],
        borderColor: ['#43a047', '#d32f2f'],
        borderWidth: 1,
      },
    ],
  } : null;

  const chartOptions = {
    plugins: {
      legend: {
        position: 'bottom',
      },
      tooltip: {
        callbacks: {
          label: function(context) {
            const total = result.successfulReadings + result.failedReadings;
            const percentage = Math.round((context.raw / total) * 100);
            return `${context.label}: ${context.raw} (${percentage}%)`;
          }
        }
      }
    },
    maintainAspectRatio: false
  };

  return (
    <div className="upload-container">
      <h2>Upload Meter Readings</h2>
      
      <form onSubmit={handleSubmit} className="upload-form">
        <div className="file-input-container">
          <input 
            type="file" 
            onChange={handleFileChange} 
            accept=".csv" 
            id="file-upload"
            className="file-input"
          />
          <label htmlFor="file-upload" className="file-label">
            {file ? file.name : 'Choose CSV file'}
          </label>
        </div>
        
        <button 
          type="submit" 
          className="upload-button" 
          disabled={uploading || !file}
        >
          {uploading ? 'Uploading...' : 'Upload'}
        </button>
      </form>
      
      {error && <div className="error-message">{error}</div>}
      
      {result && (
        <div className="result-container">
          <h3>Upload Results</h3>
          
          <div className="chart-container">
            <Pie data={chartData} options={chartOptions} />
          </div>
          
          <div className="result-summary">
            <div className="result-item">
              <span className="result-label">Successful Readings:</span>
              <span className="result-value success">{result.successfulReadings}</span>
            </div>
            <div className="result-item">
              <span className="result-label">Failed Readings:</span>
              <span className="result-value failure">{result.failedReadings}</span>
            </div>
            <div className="result-item">
              <span className="result-label">Total Processed:</span>
              <span className="result-value">{result.successfulReadings + result.failedReadings}</span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default MeterReadingUpload;