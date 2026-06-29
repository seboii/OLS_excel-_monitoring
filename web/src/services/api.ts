import axios from 'axios'

/** Merkezi API istemcisi. Geliştirmede Vite proxy '/api' → http://localhost:5080. */
export const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

// İstek: JWT access token ekle
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('ols_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Yanıt: 401 → oturumu temizle ve giriş ekranına dön
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status
    const url: string = error?.config?.url ?? ''
    if (status === 401 && !url.includes('/auth/')) {
      localStorage.removeItem('ols_token')
      localStorage.removeItem('ols_refresh')
      if (window.location.pathname !== '/login') window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)
