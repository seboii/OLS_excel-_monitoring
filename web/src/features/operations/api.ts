import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse, OperationDetail, OperationListItem, PagedResult } from '@/services/types'

export interface OperationFilters {
  transport?: string
  status?: string
  risk?: string
  quick?: string
  search?: string
  page?: number
  pageSize?: number
}

export function useOperations(filters: OperationFilters) {
  return useQuery({
    queryKey: ['operations', filters],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<PagedResult<OperationListItem>>>('/operations', {
        params: filters,
      })
      return data.data as PagedResult<OperationListItem>
    },
    placeholderData: keepPreviousData,
  })
}

export function useOperation(id: number | null) {
  return useQuery({
    queryKey: ['operation', id],
    enabled: id != null,
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<OperationDetail>>(`/operations/${id}`)
      return data.data as OperationDetail
    },
  })
}
