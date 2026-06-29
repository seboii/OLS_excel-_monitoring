import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse } from '@/services/types'

export interface RiskThresholds {
  delayOrangeDays: number
  delayRedDays: number
  financeOverdueOrangeDays: number
  financeOverdueRedDays: number
}

export function useRiskThresholds() {
  return useQuery({
    queryKey: ['risk-thresholds'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<RiskThresholds>>('/settings/risk-thresholds')
      return data.data as RiskThresholds
    },
  })
}

export function useUpdateRiskThresholds() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (input: RiskThresholds) => {
      const { data } = await api.put<ApiResponse<RiskThresholds>>('/settings/risk-thresholds', input)
      if (!data.success) throw new Error(data.message ?? 'Eşikler güncellenemedi.')
      return data
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['risk-thresholds'] })
      qc.invalidateQueries({ queryKey: ['alerts'] })
      qc.invalidateQueries({ queryKey: ['dashboard-summary'] })
    },
  })
}
