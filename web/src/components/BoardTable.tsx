import { useEffect, useRef, useState } from 'react'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { RiskBadge } from '@/components/RiskBadge'
import { cn } from '@/lib/utils'
import type { BoardColumn, BoardRow } from '@/features/boards/api'

/**
 * Sayfa-başına takip tablolarını render eden genel tablo. Kolonlar sunucudan gelen metadata'ya göre
 * dinamik kurulur; sol tarafta sabit Risk/Durum/Gecikme + Dosya/Ref No sütunları durur.
 * Geniş tablolarda yatay kaydırmayı kolaylaştırmak için kenar gölgesi + sol/sağ kaydırma düğmeleri içerir
 * (mouse'lu masaüstünde shift+wheel olmadan kaydırmak zor olduğundan).
 */
export function BoardTable({
  columns,
  rows,
  onRowClick,
}: {
  columns: BoardColumn[]
  rows: BoardRow[]
  onRowClick?: (row: BoardRow) => void
}) {
  const scrollRef = useRef<HTMLDivElement>(null)
  const [canScrollLeft, setCanScrollLeft] = useState(false)
  const [canScrollRight, setCanScrollRight] = useState(false)

  const updateScrollState = () => {
    const el = scrollRef.current
    if (!el) return
    setCanScrollLeft(el.scrollLeft > 4)
    setCanScrollRight(el.scrollLeft < el.scrollWidth - el.clientWidth - 4)
  }

  useEffect(() => {
    updateScrollState()
    const el = scrollRef.current
    if (!el) return
    el.addEventListener('scroll', updateScrollState, { passive: true })
    const ro = new ResizeObserver(updateScrollState)
    ro.observe(el)
    return () => {
      el.removeEventListener('scroll', updateScrollState)
      ro.disconnect()
    }
  }, [rows, columns])

  const scrollByPage = (dir: 1 | -1) => {
    const el = scrollRef.current
    if (!el) return
    el.scrollBy({ left: dir * Math.round(el.clientWidth * 0.85), behavior: 'smooth' })
  }

  if (rows.length === 0)
    return <div className="py-16 text-center text-sm text-muted-foreground">Bu sekmede kayıt yok.</div>

  return (
    <div className="relative">
      {canScrollLeft && (
        <>
          <div className="pointer-events-none absolute left-0 top-0 z-20 h-full w-10 bg-gradient-to-r from-white to-transparent" />
          <button
            type="button"
            onClick={() => scrollByPage(-1)}
            aria-label="Sola kaydır"
            className="absolute left-1.5 top-1/2 z-30 flex h-8 w-8 -translate-y-1/2 items-center justify-center rounded-full bg-white text-slate-600 shadow-md ring-1 ring-slate-200 transition hover:bg-slate-50 hover:text-primary"
          >
            <ChevronLeft className="h-4 w-4" />
          </button>
        </>
      )}
      {canScrollRight && (
        <>
          <div className="pointer-events-none absolute right-0 top-0 z-20 h-full w-10 bg-gradient-to-l from-white to-transparent" />
          <button
            type="button"
            onClick={() => scrollByPage(1)}
            aria-label="Sağa kaydır"
            className="absolute right-1.5 top-1/2 z-30 flex h-8 w-8 -translate-y-1/2 items-center justify-center rounded-full bg-white text-slate-600 shadow-md ring-1 ring-slate-200 transition hover:bg-slate-50 hover:text-primary"
          >
            <ChevronRight className="h-4 w-4" />
          </button>
        </>
      )}

      <div ref={scrollRef} className="overflow-x-auto [-webkit-overflow-scrolling:touch]">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="border-b bg-slate-50 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
              <th className="sticky left-0 z-10 bg-slate-50 px-3 py-2.5 whitespace-nowrap">Dosya / Ref No</th>
              <th className="px-3 py-2.5 whitespace-nowrap">Risk</th>
              <th className="px-3 py-2.5 whitespace-nowrap">Gecikme</th>
              <th className="px-3 py-2.5 whitespace-nowrap">Durum</th>
              {columns.map((c) => (
                <th key={c.key} className="px-3 py-2.5 whitespace-nowrap">
                  {c.label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr
                key={row.id}
                onClick={() => onRowClick?.(row)}
                className={cn('border-b last:border-0 hover:bg-slate-50/70', onRowClick && 'cursor-pointer')}
              >
                <td className="sticky left-0 z-10 bg-white px-3 py-2 font-medium text-slate-900 whitespace-nowrap">
                  {row.ref?.startsWith('#') ? <span className="text-slate-400">{row.ref}</span> : row.ref || '—'}
                </td>
                <td className="px-3 py-2">
                  <RiskBadge risk={row.risk} />
                </td>
                <td className="px-3 py-2 whitespace-nowrap">
                  {row.delayDays > 0 ? (
                    <span className="font-semibold text-red-600">{row.delayDays} gün</span>
                  ) : (
                    <span className="text-slate-300">—</span>
                  )}
                </td>
                <td className="max-w-[260px] truncate px-3 py-2 text-slate-600" title={row.status ?? undefined}>
                  {row.status ?? <span className="text-slate-300">—</span>}
                </td>
                {columns.map((c) => {
                  const v = row.cells[c.key]
                  return (
                    <td
                      key={c.key}
                      className={cn(
                        'max-w-[280px] truncate px-3 py-2 text-slate-700 whitespace-nowrap',
                        c.type === 'date' && 'tabular-nums text-slate-500',
                      )}
                      title={v ?? undefined}
                    >
                      {v ?? <span className="text-slate-300">—</span>}
                    </td>
                  )
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
