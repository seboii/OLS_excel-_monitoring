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
