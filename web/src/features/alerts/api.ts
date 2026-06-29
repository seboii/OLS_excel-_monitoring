import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse, PagedResult } from '@/services/types'

export interface AlertItem {
  id: number
  operationId?: number
  operationNo?: string
  customerName?: string
  boardKey?: string
  boardTitle?: string
  group?: string
  recordRef?: string
  type: string
  riskLevel: string
  ruleCode: string
  description: string
  status: string
  deadline?: string
  responsibleUserName?: string
  lastTriggeredAt: string
  triggerCount: number
}

export function useAlerts(params: { status?: string; risk?: string; type?: string; group?: string; pageSize?: number }) {
  return useQuery({
    queryKey: ['alerts', params],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<PagedResult<AlertItem>>>('/alerts', { params })
      return data.data as PagedResult<AlertItem>
    },
  })
}

export function useEvaluateRisk() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      const { data } = await api.post<ApiResponse<{ triggered: number }>>('/alerts/evaluate')
      return data
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['alerts'] })
      qc.invalidateQueries({ queryKey: ['dashboard-summary'] })
      qc.invalidateQueries({ queryKey: ['operations'] })
    },
  })
}

export function useResolveAlert() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (p: { id: number; note?: string }) => {
      await api.put(`/alerts/${p.id}/resolve`, { note: p.note })
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['alerts'] }),
  })
}

export function useCreateTaskFromAlert() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => {
      await api.post(`/alerts/${id}/create-task`)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['alerts'] })
      qc.invalidateQueries({ queryKey: ['tasks'] })
    },
  })
}
