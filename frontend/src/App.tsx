import React, { useEffect, useState } from "react";
import api from "./api";
import "./App.css";

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

type Page = "sensors" | "pump" | "alerts" | "logs";

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [adminPassword, setAdminPassword] = useState("");
  const [loginError, setLoginError] = useState<string | null>(null);
  const [page, setPage] = useState<Page>("sensors");

  // Data states
  const [sensors, setSensors] = useState<SensorReading[]>([]);
  const [pump, setPump] = useState<PumpState | null>(null);
  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [logs, setLogs] = useState<EventLog[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Login logic
  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoginError(null);
    try {
      const res = await api.post("/login", { password: adminPassword });
      if (res.data && res.data.token) {
        localStorage.setItem("jwt", res.data.token);
        setIsLoggedIn(true);
      } else {
        setLoginError("Hibás jelszó!");
      }
    } catch {
      setLoginError("Hibás jelszó!");
    }
  };

  // Fetch data for each page
  useEffect(() => {
    if (!isLoggedIn) return;
    setLoading(true);
    setError(null);
    if (page === "sensors") {
      api.get<SensorReading[]>("/sensors")
        .then(res => setSensors(res.data))
        .catch(() => setError("Nem sikerült betölteni a szenzor adatokat."))
        .finally(() => setLoading(false));
    } else if (page === "pump") {
      api.get<PumpState>("/pump")
        .then(res => setPump(res.data))
        .catch(() => setError("Nem sikerült betölteni a szivattyú állapotát."))
        .finally(() => setLoading(false));
    } else if (page === "alerts") {
      api.get<Alert[]>("/alerts")
        .then(res => setAlerts(res.data))
        .catch(() => setError("Nem sikerült betölteni a riasztásokat."))
        .finally(() => setLoading(false));
    } else if (page === "logs") {
      api.get<EventLog[]>("/logs")
        .then(res => setLogs(res.data))
        .catch(() => setError("Nem sikerült betölteni a naplót."))
        .finally(() => setLoading(false));
    }
  }, [isLoggedIn, page]);

  // Pump manual toggle
  const handlePumpToggle = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.post<PumpState>("/pump/manual-toggle");
      setPump(res.data);
    } catch {
      setError("Nem sikerült kapcsolni a szivattyút.");
    } finally {
      setLoading(false);
    }
  };

  // Acknowledge alert
  const handleAcknowledgeAlert = async (id: number) => {
    setLoading(true);
    setError(null);
    try {
      await api.post(`/alerts/${id}/acknowledge`);
      setAlerts(alerts => alerts.filter(a => a.id !== id));
    } catch {
      setError("Nem sikerült visszaigazolni a riasztást.");
    } finally {
      setLoading(false);
    }
  };

  // Download logs CSV (JWT headerrel, blob letöltés)
  const handleDownloadLogs = async () => {
    try {
      const token = localStorage.getItem("jwt");
      const response = await fetch(api.defaults.baseURL + "/logs/csv", {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
      if (!response.ok) {
        throw new Error("Sikertelen letöltés");
      }
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = "logs.csv";
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
    } catch {
      setError("Nem sikerült letölteni a naplót (CSV). Ellenőrizd a jogosultságot!");
    }
  };

  // Logout
  const handleLogout = () => {
    localStorage.removeItem("jwt");
    setIsLoggedIn(false);
    setAdminPassword("");
    setPage("sensors");
  };

  // Main navigation and content
  const [showLogin, setShowLogin] = useState(false);

  // Ha nincs bejelentkezve, csak a szenzor oldal legyen elérhető
  useEffect(() => {
    if (!isLoggedIn) {
      setPage("sensors");
    }
  }, [isLoggedIn]);

  // Login page csak akkor, ha showLogin true
  if (!isLoggedIn && showLogin) {
    return (
      <div className="login-bg">
        <form className="login-box" onSubmit={handleLogin}>
          <h2>Admin belépés</h2>
          <input
            type="password"
            placeholder="Admin jelszó"
            value={adminPassword}
            onChange={e => setAdminPassword(e.target.value)}
            autoFocus
          />
          <button type="submit">Belépés</button>
          {loginError && <div className="login-error">{loginError}</div>}
        </form>
      </div>
    );
  }

  return (
    <div className="App dark-bg">
      <div className="content-wrapper">
        <nav className="main-nav">
          <button className={page === "sensors" ? "active" : ""} onClick={() => setPage("sensors")}>Szenzor adatok</button>
          <button className={page === "pump" ? "active" : ""} onClick={() => setPage("pump")}
            disabled={!isLoggedIn} style={!isLoggedIn ? { opacity: 0.5, cursor: 'not-allowed' } : {}}>Szivattyú</button>
          <button className={page === "alerts" ? "active" : ""} onClick={() => setPage("alerts")}
            disabled={!isLoggedIn} style={!isLoggedIn ? { opacity: 0.5, cursor: 'not-allowed' } : {}}>Riasztások</button>
          <button className={page === "logs" ? "active" : ""} onClick={() => setPage("logs")}
            disabled={!isLoggedIn} style={!isLoggedIn ? { opacity: 0.5, cursor: 'not-allowed' } : {}}>Napló</button>
          {isLoggedIn ? (
            <button className="logout-btn" onClick={handleLogout}>Kijelentkezés</button>
          ) : (
            <button className="logout-btn" onClick={() => setShowLogin(true)}>Admin bejelentkezés</button>
          )}
        </nav>
        <h1 className="main-title">WaterSupplySimulator</h1>
        {loading && <p>Betöltés...</p>}
        {error && <p style={{ color: '#ff5252', textAlign: 'center' }}>{error}</p>}
        {page === "sensors" && (
          <table className="sensor-table">
            <thead>
              <tr>
                <th>Időpont</th>
                <th>Vízszint</th>
                <th>Beáramlás</th>
                <th>Kiáramlás</th>
                <th>Áram</th>
              </tr>
            </thead>
            <tbody>
              {sensors.map(s => (
                <tr key={s.id}>
                  <td>{new Date(s.timestamp).toLocaleString()}</td>
                  <td>{s.waterLevel.toFixed(2)}</td>
                  <td>{s.inflow.toFixed(2)}</td>
                  <td>{s.outflow.toFixed(2)}</td>
                  <td>
                    <span style={{ color: s.powerState ? '#4caf50' : '#ff5252', fontWeight: 600 }}>
                      {s.powerState ? 'OK' : 'Nincs'}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        {isLoggedIn && page === "pump" && pump && (
          <div className="pump-card">
            <h2>Szivattyú állapota</h2>
            <p><b>Állapot:</b> {pump.isOn ? <span style={{ color: '#4caf50' }}>BEKAPCSOLVA</span> : <span style={{ color: '#ff5252' }}>KIKAPCSOLVA</span>}</p>
            <p><b>Mód:</b> {pump.mode}</p>
            <p><b>Utoljára változott:</b> {new Date(pump.lastChanged).toLocaleString()}</p>
            <button onClick={handlePumpToggle}>Szivattyú kézi kapcsolása</button>
          </div>
        )}
        {isLoggedIn && page === "alerts" && (
          <div className="alerts-list">
            <h2>Riasztások</h2>
            {alerts.length === 0 && <p>Nincs aktív riasztás.</p>}
            {alerts.map(a => (
              <div key={a.id} className="alert-card">
                <b>{a.type}</b>: {a.message} <br />
                <span style={{ fontSize: 13, color: '#90caf9' }}>{new Date(a.createdAt).toLocaleString()}</span>
                <button onClick={() => handleAcknowledgeAlert(a.id)}>Visszaigazolás</button>
              </div>
            ))}
          </div>
        )}
        {isLoggedIn && page === "logs" && (
          <div className="logs-list">
            <h2>Eseménynapló</h2>
            <button onClick={handleDownloadLogs}>CSV letöltés</button>
            <table className="sensor-table">
              <thead>
                <tr>
                  <th>Időpont</th>
                  <th>Esemény</th>
                  <th>Üzenet</th>
                </tr>
              </thead>
              <tbody>
                {logs.map(l => (
                  <tr key={l.id}>
                    <td>{new Date(l.timestamp).toLocaleString()}</td>
                    <td>{l.eventType}</td>
                    <td>{l.message}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

export default App;
