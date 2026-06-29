import { useQuery } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse, DashboardSummary } from '@/services/types'

export function useDashboardSummary() {
  return useQuery({
    queryKey: ['dashboard-summary'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<DashboardSummary>>('/dashboard/summary')
      return data.data as DashboardSummary
    },
  })
}
