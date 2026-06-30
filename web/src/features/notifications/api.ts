import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/services/api'
import type { ApiResponse } from '@/services/types'

export interface NotificationItem {
  id: number
  type: string
  level: string
  title: string
  body: string
  isRead: boolean
  relatedEntityType?: string
  relatedEntityId?: number
  createdAt: string
}

export interface NotificationList {
  items: NotificationItem[]
  unreadCount: number
}

export function useNotifications(take = 20) {
  return useQuery({
    queryKey: ['notifications', take],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<NotificationList>>('/notifications', { params: { take } })
      return data.data as NotificationList
    },
    refetchInterval: 60_000,
  })
}

export function useMarkNotificationRead() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => {
      await api.post(`/notifications/${id}/read`)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['notifications'] }),
  })
}

export function useMarkAllNotificationsRead() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      await api.post('/notifications/read-all')
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['notifications'] }),
  })
}
