import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { api } from '@/services/api'
import type { ApiResponse } from '@/services/types'

interface AuthUser {
  id: number
  fullName: string
  email: string
  roles: string[]
  departmentName?: string
}

interface LoginResult {
  ok: boolean
  error?: string
}

interface LoginData {
  accessToken: string
  refreshToken: string
  user: AuthUser
}

interface AuthState {
  user: AuthUser | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<LoginResult>
  logout: () => Promise<void>
}

export const useAuth = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      isAuthenticated: false,

      login: async (email, password) => {
        try {
          const { data } = await api.post<ApiResponse<LoginData>>('/auth/login', { email, password })
          if (!data.success || !data.data) return { ok: false, error: data.message ?? 'Giriş başarısız.' }
          localStorage.setItem('ols_token', data.data.accessToken)
          localStorage.setItem('ols_refresh', data.data.refreshToken)
          set({ user: data.data.user, isAuthenticated: true })
          return { ok: true }
        } catch (err) {
          const message =
            (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
            'E-posta veya parola hatalı.'
          return { ok: false, error: message }
        }
      },

      logout: async () => {
        try {
          await api.post('/auth/logout')
        } catch {
          /* yoksay */
        }
        localStorage.removeItem('ols_token')
        localStorage.removeItem('ols_refresh')
        set({ user: null, isAuthenticated: false })
      },
    }),
    {
      name: 'ols-auth',
      partialize: (s) => ({ user: s.user, isAuthenticated: s.isAuthenticated }),
    },
  ),
)
