import { useQuery } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse } from '@/services/types'

export interface FinanceCurrencyTotal {
  currency: string
  count: number
  total: number
}

export interface FinanceSummary {
  totalFiles: number
  delivered: number
  inTransit: number
  unknown: number
  paymentReceived: number
  paymentPending: number
  docsComplete: number
  docsIncomplete: number
  byCurrency: FinanceCurrencyTotal[]
  lastSyncAt: string | null
}

export function useFinanceSummary() {
  return useQuery({
    queryKey: ['finance-summary'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<FinanceSummary>>('/finance/summary')
      return data.data as FinanceSummary
    },
  })
}
