import { useEffect } from 'react'
import { X, Info } from 'lucide-react'
import { RiskBadge } from './RiskBadge'
import { CommentPanel } from './CommentPanel'
import type { BoardColumn, BoardRow } from '@/features/boards/api'

/**
 * Bir takip tablosu (board) satırının detay paneli: tüm kolon değerleri + yorum dizisi.
 * OperationDetailDrawer'ın board-satırı karşılığı — Comment artık Operation'a ek olarak
 * (boardKey, ref) ile de bağlanabiliyor (bkz. features/comments/api.ts).
 */
export function BoardRowDrawer({
  boardKey,
  boardTitle,
  columns,
  row,
  onClose,
}: {
  boardKey: string
  boardTitle: string
  columns: BoardColumn[]
  row: BoardRow | null
  onClose: () => void
}) {
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose() }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])

  if (row == null) return null

  return (
    <div className="fixed inset-0 z-50">
      <div className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" onClick={onClose} />
      <div className="absolute right-0 top-0 h-full w-full max-w-lg overflow-y-auto bg-background shadow-2xl">
        <div className="sticky top-0 z-10 flex items-start justify-between gap-3 border-b bg-white px-5 py-4">
          <div className="min-w-0">
            <div className="text-lg font-bold text-slate-900">
              {row.ref?.startsWith('#') ? <span className="text-slate-400">{row.ref}</span> : row.ref || '—'}
            </div>
            <div className="truncate text-sm text-muted-foreground">{boardTitle}</div>
            <div className="mt-2 flex flex-wrap items-center gap-2">
              <RiskBadge risk={row.risk} />
              {row.delayDays > 0 && (
                <span className="rounded-full bg-red-50 px-2.5 py-0.5 text-xs font-medium text-red-700">
                  {row.delayDays} gün gecikme
                </span>
              )}
            </div>
          </div>
          <button onClick={onClose} className="rounded-lg p-2 transition hover:bg-secondary" title="Kapat">
            <X className="h-5 w-5 text-slate-600" />
          </button>
        </div>

        <div className="border-t px-5 py-4">
          <div className="mb-1 flex items-center gap-2 text-sm font-semibold text-slate-900">
            <Info className="h-4 w-4 text-primary" /> Satır Bilgileri
          </div>
          <div className="divide-y divide-slate-100">
            <div className="flex justify-between gap-3 py-1.5 text-sm">
              <span className="shrink-0 text-muted-foreground">Durum</span>
              <span className="text-right font-medium text-slate-800">{row.status ?? '—'}</span>
            </div>
            {columns.map((c) => {
              const v = row.cells[c.key]
              if (!v) return null
              return (
                <div key={c.key} className="flex justify-between gap-3 py-1.5 text-sm">
                  <span className="shrink-0 text-muted-foreground">{c.label}</span>
                  <span className="max-w-[60%] text-right font-medium text-slate-800">{v}</span>
                </div>
              )
            })}
          </div>
        </div>

        <CommentPanel subject={{ boardKey, ref: row.ref }} />
      </div>
    </div>
  )
}
