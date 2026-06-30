import { useQuery } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse } from '@/services/types'

export interface BoardKpi {
  key: string
  title: string
  group: string
  total: number
  active: number
  archived: number
  delayed: number
  risky: number
  critical: number
  avgDelayDays: number
  riskRatio: number
}

export interface GroupKpi {
  group: string
  boards: number
  total: number
  active: number
  delayed: number
  risky: number
  critical: number
  avgDelayDays: number
}

export function useBoardKpi() {
  return useQuery({
    queryKey: ['kpi-boards'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<BoardKpi[]>>('/kpi/boards')
      return data.data as BoardKpi[]
    },
  })
}

export function useGroupKpi() {
  return useQuery({
    queryKey: ['kpi-groups'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<GroupKpi[]>>('/kpi/groups')
      return data.data as GroupKpi[]
    },
  })
}

export interface KpiTrendPoint {
  date: string
  total: number
  current: number
  completed: number
  delayed: number
  risky: number
  critical: number
  avgDelay: number
  openAlerts: number
}

export function useKpiTrends(days = 30) {
  return useQuery({
    queryKey: ['kpi-trends', days],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<KpiTrendPoint[]>>('/kpi/trends', { params: { days } })
      return data.data as KpiTrendPoint[]
    },
  })
}
