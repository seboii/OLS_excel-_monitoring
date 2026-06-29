import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse } from '@/services/types'

export interface BoardColumn {
  key: string
  label: string
  type: string // text | date | number
}

export interface BoardSummary {
  key: string
  title: string
  group: string
  dataSourceId: number | null
  lastSyncAt: string | null
  rowCount: number
  riskCounts: Record<string, number>
}

export interface BoardRow {
  id: number
  ref: string
  status: string | null
  risk: string
  delayDays: number
  archived: boolean
  cells: Record<string, string | null>
}

export interface BoardDetail {
  key: string
  title: string
  group: string
  dataSourceId: number | null
  columns: BoardColumn[]
  rows: BoardRow[]
  total: number
  lastSyncAt: string | null
}

export interface BoardQuery {
  q?: string
  risk?: string
  page?: number
  pageSize?: number
}

/** Tüm sekmelerin (sayfa-başına tablo) özetini döner. */
export function useBoards() {
  return useQuery({
    queryKey: ['boards'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<BoardSummary[]>>('/boards')
      return data.data ?? []
    },
  })
}

/** Tek bir sekmenin kolon metadata'sı + sayfalı satırları. */
export function useBoard(key: string | undefined, params: BoardQuery) {
  return useQuery({
    queryKey: ['board', key, params],
    enabled: !!key,
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<BoardDetail>>(`/boards/${key}`, { params })
      return data.data as BoardDetail
    },
  })
}

export interface AttentionRow {
  boardKey: string
  boardTitle: string
  group: string
  ref: string
  risk: string
  delayDays: number
  status?: string
}

/** Tüm sekmelerdeki riskli/geciken aktif kayıtlar — Risk Haritası için. */
export function useAttention(params: { group?: string; minRisk?: string }) {
  return useQuery({
    queryKey: ['boards-attention', params],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<AttentionRow[]>>('/boards/attention', { params })
      return data.data ?? []
    },
  })
}

export interface BoardSearchResult {
  boardKey: string
  boardTitle: string
  group: string
  ref: string
  risk: string
  delayDays: number
  matchedField: string
  matchedValue: string
}

/** Tüm sekmelerde (Deniz+Kara+Hava+Finans) tek seferde arama — header'daki global arama kutusu. */
export function useGlobalSearch(query: string) {
  const q = query.trim()
  return useQuery({
    queryKey: ['boards-search', q],
    enabled: q.length >= 2,
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<BoardSearchResult[]>>('/boards/search', { params: { q } })
      return data.data ?? []
    },
  })
}

/**
 * Bir sekmenin satırlarını TÜM görünür kolonlarda (dosya no, durum, her hücre) anlık olarak filtreler.
 * Sunucuya gitmeden istemcide çalışır — yazarken Enter'a basmaya gerek kalmaz.
 */
export function filterBoardRows(rows: BoardRow[], query: string): BoardRow[] {
  const q = query.trim().toLocaleLowerCase('tr')
  if (!q) return rows
  return rows.filter((row) => {
    if (row.ref?.toLocaleLowerCase('tr').includes(q)) return true
    if (row.status?.toLocaleLowerCase('tr').includes(q)) return true
    return Object.values(row.cells).some((v) => v?.toLocaleLowerCase('tr').includes(q))
  })
}

/** Bir veri kaynağını (sekmeyi) yeniden senkronize eder; önbelleği tazeler. */
export function useSyncBoard() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (dataSourceId: number) => {
      const { data } = await api.post<ApiResponse<unknown>>(`/data-sources/${dataSourceId}/sync`)
      return data
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['boards'] })
      qc.invalidateQueries({ queryKey: ['board'] })
    },
  })
}
