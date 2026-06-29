import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse, PagedResult } from '@/services/types'

export interface TaskItem {
  id: number
  title: string
  operationId?: number
  operationNo?: string
  ownerName?: string
  departmentName?: string
  priority: string
  dueDate?: string
  status: string
  description?: string
  createdAt: string
}

export function useTasks(params: { status?: string; pageSize?: number }) {
  return useQuery({
    queryKey: ['tasks', params],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<PagedResult<TaskItem>>>('/tasks', { params })
      return data.data as PagedResult<TaskItem>
    },
  })
}

export function useCompleteTask() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (p: { id: number; note?: string }) => {
      await api.post(`/tasks/${p.id}/complete`, { note: p.note })
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['tasks'] }),
  })
}

export function useCreateTask() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (p: { title: string; priority: string; operationId?: number; description?: string }) => {
      await api.post('/tasks', p)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['tasks'] }),
  })
}
