import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse } from '@/services/types'

// ───────────── Tipler (backend DTO'larıyla birebir) ─────────────

export interface DataSource {
  id: number
  name: string
  type: string
  accessType: string
  url?: string
  defaultTransportType?: string
  departmentId?: number
  departmentName?: string
  sheetName?: string
  headerRowIndex: number
  syncIntervalMinutes: number
  isActive: boolean
  lastSyncAt?: string
  lastSuccessSyncAt?: string
  lastSyncStatus?: string
  lastSyncError?: string
  mappingCount: number
}

export interface CreateDataSourceInput {
  name: string
  type: string
  accessType: string
  url?: string | null
  defaultTransportType?: string | null
  sheetName?: string | null
  headerRowIndex?: number
  syncIntervalMinutes?: number
}

export interface UpdateDataSourceInput extends CreateDataSourceInput {
  isActive: boolean
}

export interface SheetColumn {
  index: number
  name: string
}

export interface SheetPreview {
  sheetName: string
  headerRowIndex: number
  columns: SheetColumn[]
  rows: Record<string, string | null>[]
  totalDataRows: number
}

export interface MappingSuggestion {
  sourceColumn: string
  sourceColumnIndex: number
  suggestedTargetField?: string
}

export interface ImportPreview {
  sheetNames: string[]
  sheet: SheetPreview
  suggestions: MappingSuggestion[]
}

export interface ConnectionTestResult {
  ok: boolean
  fileName: string
  sizeBytes: number
  sheetNames: string[]
}

export interface ColumnMapping {
  id: number
  sourceColumn: string
  sourceColumnIndex?: number
  targetField: string
  transformType?: string
  defaultValue?: string
  isRequired: boolean
}

export interface ColumnMappingInput {
  sourceColumn: string
  sourceColumnIndex?: number
  targetField: string
  transformType?: string | null
  defaultValue?: string | null
  isRequired: boolean
}

export interface SyncLog {
  id: number
  startedAt: string
  finishedAt?: string
  status: string
  rowsRead: number
  rowsUpserted: number
  rowsFailed: number
  message?: string
  durationMs?: number
  fileName?: string
  sheetName?: string
}

export interface SyncResult {
  rowsRead: number
  rowsUpserted: number
  rowsFailed: number
  errors: string[]
}

/** Axios hatasından backend'in Türkçe mesajını çıkarır. */
export function apiErrorMessage(err: unknown, fallback = 'Beklenmeyen bir hata oluştu.'): string {
  const e = err as { response?: { data?: { message?: string } } }
  return e?.response?.data?.message ?? fallback
}

// ───────────── Sorgular ─────────────

export function useDataSources() {
  return useQuery({
    queryKey: ['data-sources'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<DataSource[]>>('/data-sources')
      return data.data ?? []
    },
  })
}

export function useColumnMappings(id?: number) {
  return useQuery({
    queryKey: ['data-source-mappings', id],
    enabled: !!id,
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<ColumnMapping[]>>(`/data-sources/${id}/column-mappings`)
      return data.data ?? []
    },
  })
}

export function useSyncLogs(id?: number) {
  return useQuery({
    queryKey: ['data-source-logs', id],
    enabled: !!id,
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<SyncLog[]>>(`/data-sources/${id}/sync-logs`)
      return data.data ?? []
    },
  })
}

// ───────────── Mutasyonlar ─────────────

export function useCreateDataSource() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (input: CreateDataSourceInput) => {
      const { data } = await api.post<ApiResponse<DataSource>>('/data-sources', input)
      return data.data as DataSource
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['data-sources'] }),
  })
}

export function useUpdateDataSource() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (p: { id: number; input: UpdateDataSourceInput }) => {
      await api.put(`/data-sources/${p.id}`, p.input)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['data-sources'] }),
  })
}

export function useDeleteDataSource() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/data-sources/${id}`)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['data-sources'] }),
  })
}

export function useTestConnection() {
  return useMutation({
    mutationFn: async (id: number) => {
      const { data } = await api.post<ApiResponse<ConnectionTestResult>>(`/data-sources/${id}/test-connection`)
      return data.data as ConnectionTestResult
    },
  })
}

export function useDownloadPreview() {
  return useMutation({
    mutationFn: async (p: { id: number; sheetName?: string; headerRowIndex?: number }) => {
      const { data } = await api.post<ApiResponse<ImportPreview>>(`/data-sources/${p.id}/download-preview`, {
        sheetName: p.sheetName,
        headerRowIndex: p.headerRowIndex,
      })
      return data.data as ImportPreview
    },
  })
}

export function useSaveColumnMappings() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (p: { id: number; mappings: ColumnMappingInput[] }) => {
      await api.put(`/data-sources/${p.id}/column-mappings`, p.mappings)
    },
    onSuccess: (_d, p) => {
      qc.invalidateQueries({ queryKey: ['data-source-mappings', p.id] })
      qc.invalidateQueries({ queryKey: ['data-sources'] })
    },
  })
}

export function useSyncDataSource() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => {
      const { data } = await api.post<ApiResponse<SyncResult>>(`/data-sources/${id}/sync`)
      return data.data as SyncResult
    },
    onSuccess: (_d, id) => {
      qc.invalidateQueries({ queryKey: ['data-sources'] })
      qc.invalidateQueries({ queryKey: ['data-source-logs', id] })
      qc.invalidateQueries({ queryKey: ['operations'] })
      qc.invalidateQueries({ queryKey: ['dashboard-summary'] })
    },
  })
}

// ───────────── Manuel Excel upload ─────────────

export function useManualUploadPreview() {
  return useMutation({
    mutationFn: async (p: { file: File; sheetName?: string; headerRowIndex?: number }) => {
      const form = new FormData()
      form.append('file', p.file)
      if (p.sheetName) form.append('sheetName', p.sheetName)
      if (p.headerRowIndex != null) form.append('headerRowIndex', String(p.headerRowIndex))
      const { data } = await api.post<ApiResponse<ImportPreview>>('/data-sources/manual-upload/preview', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      return data.data as ImportPreview
    },
  })
}

export function useManualUploadImport() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (p: { dataSourceId: number; file: File; sheetName?: string; headerRowIndex?: number }) => {
      const form = new FormData()
      form.append('dataSourceId', String(p.dataSourceId))
      form.append('file', p.file)
      if (p.sheetName) form.append('sheetName', p.sheetName)
      if (p.headerRowIndex != null) form.append('headerRowIndex', String(p.headerRowIndex))
      const { data } = await api.post<ApiResponse<SyncResult>>('/data-sources/manual-upload/import', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      return data.data as SyncResult
    },
    onSuccess: (_d, p) => {
      qc.invalidateQueries({ queryKey: ['data-sources'] })
      qc.invalidateQueries({ queryKey: ['data-source-logs', p.dataSourceId] })
      qc.invalidateQueries({ queryKey: ['operations'] })
    },
  })
}
