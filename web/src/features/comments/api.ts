import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse } from '@/services/types'

export interface CommentItem {
  id: number
  operationId?: number
  boardKey?: string
  boardTitle?: string
  group?: string
  recordRef?: string
  authorName: string
  type: string
  body: string
  mentions: string[]
  createdAt: string
}

/** Bir yorum dizisinin bağlandığı kaynak: eski Operation modeli VEYA bir board satırı. */
export type CommentSubject = { operationId: number } | { boardKey: string; ref: string }

function subjectPath(s: CommentSubject) {
  return 'operationId' in s
    ? `/operations/${s.operationId}/comments`
    : `/boards/${s.boardKey}/comments?ref=${encodeURIComponent(s.ref)}`
}

function subjectKey(s: CommentSubject): unknown[] {
  return 'operationId' in s ? ['comments', 'op', s.operationId] : ['comments', 'board', s.boardKey, s.ref]
}

export function useComments(subject: CommentSubject | null) {
  return useQuery({
    queryKey: subject ? subjectKey(subject) : ['comments', 'none'],
    enabled: subject != null,
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<CommentItem[]>>(subjectPath(subject!))
      return data.data as CommentItem[]
    },
  })
}

/** Merkezi "Yorumlar" akışı: tüm operasyon + board satırlarından en yeni yorumlar. */
export function useRecentComments(group?: string, take = 50) {
  return useQuery({
    queryKey: ['comments', 'recent', group ?? 'all', take],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<CommentItem[]>>('/comments/recent', {
        params: { group, take },
      })
      return data.data as CommentItem[]
    },
  })
}

export function useAddComment(subject: CommentSubject | null) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (p: { type: string; body: string; mentions?: string[] }) => {
      if (!subject) throw new Error('Yorum kaynağı belirtilmedi.')
      await api.post(subjectPath(subject), p)
    },
    onSuccess: () => {
      if (subject) qc.invalidateQueries({ queryKey: subjectKey(subject) })
    },
  })
}
