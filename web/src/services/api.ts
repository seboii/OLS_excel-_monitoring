import axios from 'axios'

/** Merkezi API istemcisi. Geliştirmede Vite proxy '/api' → http://localhost:5080. */
export const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

// JWT eklendiğinde token interceptor'ı burada devreye girecek:
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('ols_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})
