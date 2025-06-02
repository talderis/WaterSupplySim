import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:5001/api/WaterSystem",
  withCredentials: false,
});

// JWT token automatikus hozzáadása minden kéréshez, ha van
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("jwt");
  if (token) {
    config.headers = config.headers || {};
    config.headers["Authorization"] = `Bearer ${token}`;
  }
  return config;
});

export default api;
