import { useQuery } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse } from '@/services/types'

export interface AiSection {
  title: string
  body: string
}
export interface AiSummary {
  sections: AiSection[]
  generatedAt: string
  aiGenerated: boolean
}

export function useAiSummary() {
  return useQuery({
    queryKey: ['ai-summary'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<AiSummary>>('/dashboard/ai-summary')
      return data.data as AiSummary
    },
  })
}
