import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse } from '@/services/types'

export interface UserItem {
  id: number
  fullName: string
  email: string
  isActive: boolean
  roles: string[]
  departmentId?: number
  departmentName?: string
  lastLoginAt?: string
  createdAt: string
}

export interface RoleItem {
  id: number
  code: string
  name: string
  description?: string
}

export interface DepartmentItem {
  id: number
  name: string
}

export interface CreateUserInput {
  fullName: string
  email: string
  password: string
  roles: string[]
  departmentId?: number | null
}

export interface UpdateUserInput {
  fullName: string
  roles: string[]
  departmentId?: number | null
  isActive: boolean
}

export function useUsers() {
  return useQuery({
    queryKey: ['users'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<UserItem[]>>('/users')
      return data.data as UserItem[]
    },
  })
}

export function useRoles() {
  return useQuery({
    queryKey: ['users', 'roles'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<RoleItem[]>>('/users/roles')
      return data.data as RoleItem[]
    },
  })
}

export function useDepartments() {
  return useQuery({
    queryKey: ['users', 'departments'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<DepartmentItem[]>>('/users/departments')
      return data.data as DepartmentItem[]
    },
  })
}

export function useCreateUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (input: CreateUserInput) => {
      const { data } = await api.post<ApiResponse<UserItem>>('/users', input)
      if (!data.success) throw new Error(data.message ?? 'Kullanıcı oluşturulamadı.')
      return data.data
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  })
}

export function useUpdateUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, input }: { id: number; input: UpdateUserInput }) => {
      const { data } = await api.put<ApiResponse<UserItem>>(`/users/${id}`, input)
      if (!data.success) throw new Error(data.message ?? 'Kullanıcı güncellenemedi.')
      return data.data
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  })
}

export function useResetPassword() {
  return useMutation({
    mutationFn: async ({ id, newPassword }: { id: number; newPassword: string }) => {
      const { data } = await api.post<ApiResponse<unknown>>(`/users/${id}/reset-password`, { newPassword })
      if (!data.success) throw new Error(data.message ?? 'Parola sıfırlanamadı.')
    },
  })
}

export function useDeleteUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => {
      const { data } = await api.delete<ApiResponse<unknown>>(`/users/${id}`)
      if (!data.success) throw new Error(data.message ?? 'Kullanıcı silinemedi.')
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  })
}
