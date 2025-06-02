import * as React from 'react';
import { useState, useEffect } from 'react';
import { createRoot } from 'react-dom/client';
import axios from 'axios';
import Chart from 'chart.js/auto';

interface SensorReading {
  id: number;
  timestamp: string;
  waterLevel: number;
  inflow: number;
  outflow: number;
  powerState: boolean;
}

interface PumpState {
  id: number;
  isOn: boolean;
  mode: string;
  lastChanged: string;
}

interface Alert {
  id: number;
  type: string;
  message: string;
  createdAt: string;
  isAcknowledged: boolean;
}

interface EventLog {
  id: number;
  eventType: string;
  message: string;
  timestamp: string;
}

const App: React.FC = () => {
  const [sensorData, setSensorData] = useState<SensorReading[]>([]);
  const [pumpState, setPumpState] = useState<PumpState | null>(null);
  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [logs, setLogs] = useState<EventLog[]>([]);
  const chartRef = React.useRef<HTMLCanvasElement>(null);
  let chartInstance: Chart | null = null;

  // Fetch sensor data
  useEffect(() => {
    const fetchSensorData = async () => {
      try {
        const response = await axios.get('/api/WaterSystem/sensors');
        setSensorData(response.data);
      } catch (error) {
        console.error('Error fetching sensor data:', error);
      }
    };

    fetchSensorData();
    const interval = setInterval(fetchSensorData, 5000);
    return () => clearInterval(interval);
  }, []);

  // Fetch pump state
  useEffect(() => {
    const fetchPumpState = async () => {
      try {
        const response = await axios.get('/api/WaterSystem/pump');
        setPumpState(response.data);
      } catch (error) {
        console.error('Error fetching pump state:', error);
      }
    };

    fetchPumpState();
    const interval = setInterval(fetchPumpState, 5000);
    return () => clearInterval(interval);
  }, []);

  // Fetch alerts
  useEffect(() => {
    const fetchAlerts = async () => {
      try {
        const response = await axios.get('/api/WaterSystem/alerts');
        setAlerts(response.data);
      } catch (error) {
        console.error('Error fetching alerts:', error);
      }
    };

    fetchAlerts();
    const interval = setInterval(fetchAlerts, 5000);
    return () => clearInterval(interval);
  }, []);

  // Fetch logs
  useEffect(() => {
    const fetchLogs = async () => {
      try {
        const response = await axios.get('/api/WaterSystem/logs');
        setLogs(response.data);
      } catch (error) {
        console.error('Error fetching logs:', error);
      }
    };

    fetchLogs();
    const interval = setInterval(fetchLogs, 5000);
    return () => clearInterval(interval);
  }, []);

  // Initialize chart
  useEffect(() => {
    if (chartRef.current && sensorData.length > 0) {
      if (chartInstance) {
        chartInstance.destroy();
      }

      chartInstance = new Chart(chartRef.current, {
        type: 'line',
        data: {
          labels: sensorData.map(data => new Date(data.timestamp).toLocaleTimeString()),
          datasets: [
            {
              label: 'V�zszint (%)',
              data: sensorData.map(data => data.waterLevel),
              borderColor: '#007bff',
              fill: false,
            },
          ],
        },
        options: {
          scales: {
            y: {
              beginAtZero: true,
              max: 100,
            },
          },
        },
      });
    }

    return () => {
      if (chartInstance) {
        chartInstance.destroy();
      }
    };
  }, [sensorData]);

  // Toggle pump
  const togglePump = async () => {
    try {
      const response = await axios.post('/api/WaterSystem/pump/manual-toggle');
      setPumpState(response.data);
    } catch (error) {
      console.error('Error toggling pump:', error);
    }
  };

  // Acknowledge alert
  const acknowledgeAlert = async (id: number) => {
    try {
      await axios.post(/api/WaterSystem/alerts//acknowledge);
      setAlerts(alerts.filter(alert => alert.id !== id));
    } catch (error) {
      console.error('Error acknowledging alert:', error);
    }
  };

  return (
    <div className=""p-4"">
      <h1 className=""text-2xl font-bold mb-4"">V�zell�t� Rendszer Dashboard</h1>

      <div className=""grid grid-cols-1 md:grid-cols-2 gap-4"">
        {/* Water Level Chart */}
        <div className=""bg-white p-4 rounded shadow"">
          <h2 className=""text-lg font-semibold mb-2"">V�zszint Grafikon</h2>
          <canvas ref={chartRef}></canvas>
        </div>

        {/* Pump State */}
        <div className=""bg-white p-4 rounded shadow"">
          <h2 className=""text-lg font-semibold mb-2"">Szivatty� �llapot</h2>
          {pumpState ? (
            <div>
              <p>�llapot: <span className={pumpState.isOn ? 'text-green-500' : 'text-red-500'}>
                {pumpState.isOn ? 'BE' : 'KI'}
              </span></p>
              <p>M�d: {pumpState.mode}</p>
              <p>Utolj�ra m�dos�tva: {new Date(pumpState.lastChanged).toLocaleString()}</p>
              <button
                onClick={togglePump}
                className=""mt-2 bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600""
              >
                Szivatty� �tkapcsol�sa
              </button>
            </div>
          ) : (
            <p>Bet�lt�s...</p>
          )}
        </div>

        {/* Alerts */}
        <div className=""bg-white p-4 rounded shadow"">
          <h2 className=""text-lg font-semibold mb-2"">Akt�v Riaszt�sok</h2>
          {alerts.length > 0 ? (
            <ul className=""space-y-2"">
              {alerts.map(alert => (
                <li key={alert.id} className=""border-b py-2"">
                  <p><strong>T�pus:</strong> {alert.type}</p>
                  <p><strong>�zenet:</strong> {alert.message}</p>
                  <p><strong>Id�pont:</strong> {new Date(alert.createdAt).toLocaleString()}</p>
                  <button
                    onClick={() => acknowledgeAlert(alert.id)}
                    className=""text-blue-500 hover:underline""
                  >
                    Olvasottra jel�l�s
                  </button>
                </li>
              ))}
            </ul>
          ) : (
            <p>Nincsenek akt�v riaszt�sok.</p>
          )}
        </div>

        {/* Event Logs */}
        <div className=""bg-white p-4 rounded shadow"">
          <h2 className=""text-lg font-semibold mb-2"">Esem�nynapl�k</h2>
          <a
            href=""/api/WaterSystem/logs/csv""
            className=""text-blue-500 hover:underline mb-2 inline-block""
          >
            Napl�k let�lt�se CSV-ben
          </a>
          {logs.length > 0 ? (
            <ul className=""space-y-2"">
              {logs.map(log => (
                <li key={log.id} className=""border-b py-2"">
                  <p><strong>T�pus:</strong> {log.eventType}</p>
                  <p><strong>�zenet:</strong> {log.message}</p>
                  <p><strong>Id�pont:</strong> {new Date(log.timestamp).toLocaleString()}</p>
                </li>
              ))}
            </ul>
          ) : (
            <p>Nincsenek esem�nynapl�k.</p>
          )}
        </div>
      </div>
    </div>
  );
};

const root = createRoot(document.getElementById('root')!);
root.render(<App />);
