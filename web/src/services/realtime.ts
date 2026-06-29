import { useEffect } from 'react'
import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { create } from 'zustand'

/** Backend `RealtimeEvents` ile birebir aynı olmalı. */
const RealtimeEvents = {
  AlertsChanged: 'alerts-changed',
  TasksChanged: 'tasks-changed',
  CommentsChanged: 'comments-changed',
  DataSynced: 'data-synced',
} as const

/** Hub bağlantı durumu — header'daki "Canlı" göstergesi bunu okur. */
interface RealtimeState {
  connected: boolean
  setConnected: (v: boolean) => void
}

export const useRealtimeStatus = create<RealtimeState>((set) => ({
  connected: false,
  setConnected: (connected) => set({ connected }),
}))

/**
 * Tek bir SignalR hub bağlantısı kurar ve sunucu olaylarına göre TanStack Query
 * önbelleğini geçersiz kılar. AppShell içinde (oturum açıkken) bir kez çağrılmalı.
 */
export function useRealtime() {
  const qc = useQueryClient()
  const setConnected = useRealtimeStatus((s) => s.setConnected)

  useEffect(() => {
    const token = localStorage.getItem('ols_token')
    if (!token) return

    const connection: HubConnection = new HubConnectionBuilder()
      .withUrl('/api/hubs/dashboard', {
        accessTokenFactory: () => localStorage.getItem('ols_token') ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    const invalidate = (keys: unknown[][]) => {
      for (const key of keys) qc.invalidateQueries({ queryKey: key })
    }

    connection.on('serverEvent', (event: string, payload?: { operationId?: number }) => {
      switch (event) {
        case RealtimeEvents.AlertsChanged:
          invalidate([
            ['alerts'],
            ['dashboard-summary'],
            ['tasks'],
            ['kpi-boards'],
            ['kpi-groups'],
          ])
          break
        case RealtimeEvents.TasksChanged:
          invalidate([['tasks'], ['dashboard-summary']])
          break
        case RealtimeEvents.CommentsChanged:
          invalidate([payload?.operationId ? ['comments', payload.operationId] : ['comments']])
          break
        case RealtimeEvents.DataSynced:
          invalidate([
            ['dashboard-summary'],
            ['boards'],
            ['board'],
            ['alerts'],
            ['kpi-boards'],
            ['kpi-groups'],
          ])
          break
      }
    })

    connection.onreconnected(() => setConnected(true))
    connection.onreconnecting(() => setConnected(false))
    connection.onclose(() => setConnected(false))

    connection
      .start()
      .then(() => setConnected(connection.state === HubConnectionState.Connected))
      .catch(() => setConnected(false))

    return () => {
      setConnected(false)
      void connection.stop()
    }
  }, [qc, setConnected])
}
