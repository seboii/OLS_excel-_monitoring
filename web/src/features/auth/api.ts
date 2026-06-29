import { useMutation } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse } from '@/services/types'

export interface ChangePasswordInput {
  currentPassword: string
  newPassword: string
}

export function useChangePassword() {
  return useMutation({
    mutationFn: async (input: ChangePasswordInput) => {
      const { data } = await api.post<ApiResponse<unknown>>('/auth/change-password', input)
      if (!data.success) throw new Error(data.message ?? 'Parola değiştirilemedi.')
      return data
    },
  })
}
