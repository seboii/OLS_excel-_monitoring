import { useEffect, useRef, useState } from 'react'
import { Bell, CheckCheck, Loader2 } from 'lucide-react'
import {
  useNotifications,
  useMarkAllNotificationsRead,
  useMarkNotificationRead,
  type NotificationItem,
} from '@/features/notifications/api'
import { cn } from '@/lib/utils'

const levelDot: Record<string, string> = {
  Info: 'bg-sky-500',
  Warning: 'bg-amber-500',
  Critical: 'bg-red-500',
  ManagementIntervention: 'bg-fuchsia-600',
}

function relativeTime(iso: string): string {
  const diffMs = Date.now() - new Date(iso).getTime()
  const m = Math.floor(diffMs / 60000)
  if (m < 1) return 'az önce'
  if (m < 60) return `${m} dk önce`
  const h = Math.floor(m / 60)
  if (h < 24) return `${h} saat önce`
  const d = Math.floor(h / 24)
  if (d < 30) return `${d} gün önce`
  return new Date(iso).toLocaleDateString('tr-TR')
}

export function NotificationBell() {
  const [open, setOpen] = useState(false)
  const boxRef = useRef<HTMLDivElement>(null)

  const { data, isLoading } = useNotifications(20)
  const markRead = useMarkNotificationRead()
  const markAll = useMarkAllNotificationsRead()

  const unread = data?.unreadCount ?? 0
  const items = data?.items ?? []

  useEffect(() => {
    const onClickOutside = (e: MouseEvent) => {
      if (boxRef.current && !boxRef.current.contains(e.target as Node)) setOpen(false)
    }
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') setOpen(false) }
    document.addEventListener('mousedown', onClickOutside)
    document.addEventListener('keydown', onKey)
    return () => {
      document.removeEventListener('mousedown', onClickOutside)
      document.removeEventListener('keydown', onKey)
    }
  }, [])

  const onItemClick = (n: NotificationItem) => {
    if (!n.isRead) markRead.mutate(n.id)
  }

  return (
    <div ref={boxRef} className="relative">
      <button
        onClick={() => setOpen((v) => !v)}
        className="relative rounded-lg p-2 transition hover:bg-secondary"
        title="Bildirimler"
        aria-label="Bildirimler"
      >
        <Bell className="h-5 w-5 text-slate-600" />
        {unread > 0 && (
          <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
            {unread > 99 ? '99+' : unread}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 top-full z-50 mt-2 w-80 overflow-hidden rounded-lg border bg-white shadow-lg sm:w-96">
          <div className="flex items-center justify-between border-b px-4 py-2.5">
            <span className="text-sm font-semibold text-slate-900">
              Bildirimler{unread > 0 ? ` (${unread})` : ''}
            </span>
            {unread > 0 && (
              <button
                onClick={() => markAll.mutate()}
                disabled={markAll.isPending}
                className="flex items-center gap-1 text-xs font-medium text-primary transition hover:underline disabled:opacity-50"
              >
                <CheckCheck className="h-3.5 w-3.5" /> Tümünü okundu işaretle
              </button>
            )}
          </div>

          <div className="max-h-96 overflow-y-auto">
            {isLoading ? (
              <div className="flex items-center gap-2 px-4 py-6 text-sm text-muted-foreground">
                <Loader2 className="h-4 w-4 animate-spin" /> Yükleniyor...
              </div>
            ) : items.length === 0 ? (
              <div className="px-4 py-8 text-center text-sm text-muted-foreground">Henüz bildirim yok.</div>
            ) : (
              <ul className="divide-y">
                {items.map((n) => (
                  <li key={n.id}>
                    <button
                      onClick={() => onItemClick(n)}
                      className={cn(
                        'flex w-full gap-3 px-4 py-3 text-left transition hover:bg-secondary/60',
                        !n.isRead && 'bg-primary/5',
                      )}
                    >
                      <span className={cn('mt-1.5 h-2 w-2 shrink-0 rounded-full', levelDot[n.level] ?? 'bg-slate-400')} />
                      <div className="min-w-0 flex-1">
                        <div className="flex items-start justify-between gap-2">
                          <span className={cn('text-sm', n.isRead ? 'font-medium text-slate-700' : 'font-semibold text-slate-900')}>
                            {n.title}
                          </span>
                          <span className="shrink-0 text-[11px] text-muted-foreground">{relativeTime(n.createdAt)}</span>
                        </div>
                        <p className="mt-0.5 whitespace-pre-line text-xs text-muted-foreground line-clamp-3">{n.body}</p>
                      </div>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
