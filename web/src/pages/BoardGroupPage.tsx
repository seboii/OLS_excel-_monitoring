import { useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { RefreshCw, Search, X } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { BoardTable } from '@/components/BoardTable'
import { BoardRowDrawer } from '@/components/BoardRowDrawer'
import { useBoard, useBoards, useSyncBoard, filterBoardRows } from '@/features/boards/api'
import type { BoardRow } from '@/features/boards/api'
import { riskLevelLabels } from '@/lib/labels'
import { cn } from '@/lib/utils'

const riskOptions = ['Green', 'Yellow', 'Orange', 'Red', 'Black']
const FULL_PAGE_SIZE = 2000 // board'lar en fazla ~1300 satır; tek sayfada tamamı gelsin (sayfalama gerekmez)

/**
 * Bir taşıma grubunun (Deniz/Kara/Hava) sayfası: gruba ait her kaynak sayfası bir sekme olur,
 * seçili sekmenin tablosu kaynaktaki kolonlarla birebir gösterilir. Arama tüm kolonlarda anlık
 * (istemci tarafı) çalışır; risk filtresi sunucuya gider. Manuel senkronizasyon içerir. Satıra
 * tıklayınca detay + yorum paneli açılır. `?board=` ve `?q=` URL parametreleri (global aramadan
 * gelen deep link) ilk yüklemede sekme/aramayı otomatik ayarlar.
 */
export function BoardGroupPage({ group, title }: { group: string; title: string }) {
  const [searchParams, setSearchParams] = useSearchParams()
  const { data: boards, isLoading: boardsLoading } = useBoards()
  const groupBoards = useMemo(
    () => (boards ?? []).filter((b) => b.group === group),
    [boards, group],
  )

  const [activeKey, setActiveKey] = useState<string | undefined>()
  const [search, setSearch] = useState('')
  const [risk, setRisk] = useState('')
  const [selectedRow, setSelectedRow] = useState<BoardRow | null>(null)

  // Grup yüklendiğinde / değiştiğinde ilk sekmeyi seç — ?board=/?q= varsa onu kullan (global arama deep-link).
  useEffect(() => {
    if (!groupBoards.length || groupBoards.some((b) => b.key === activeKey)) return
    const fromUrl = searchParams.get('board')
    const initialBoard = fromUrl && groupBoards.some((b) => b.key === fromUrl) ? fromUrl : groupBoards[0].key
    setActiveKey(initialBoard)
    setSearch(searchParams.get('q') ?? '')
    setRisk('')
    if (fromUrl) setSearchParams({}, { replace: true })
  }, [groupBoards, activeKey, searchParams, setSearchParams])

  const { data: detail, isLoading, isError, isFetching } = useBoard(activeKey, {
    risk: risk || undefined,
    pageSize: FULL_PAGE_SIZE,
  })

  const filteredRows = useMemo(
    () => (detail ? filterBoardRows(detail.rows, search) : []),
    [detail, search],
  )

  const sync = useSyncBoard()
  const activeSummary = groupBoards.find((b) => b.key === activeKey)

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">{title}</h1>
        <p className="text-sm text-muted-foreground">
          {detail
            ? search
              ? `${filteredRows.length} / ${detail.total} kayıt`
              : `${detail.total} kayıt`
            : boardsLoading ? 'Yükleniyor...' : `${groupBoards.length} sekme`}
          {activeSummary?.lastSyncAt
            ? ` · Son senkron: ${new Date(activeSummary.lastSyncAt).toLocaleString('tr-TR')}`
            : ''}
        </p>
      </div>

      {/* Sekmeler */}
      <div className="flex flex-wrap gap-1.5 border-b">
        {groupBoards.map((b) => (
          <button
            key={b.key}
            onClick={() => { setActiveKey(b.key); setSearch(''); setRisk('') }}
            className={cn(
              '-mb-px flex shrink-0 items-center gap-2 rounded-t-lg border border-b-0 px-3 py-2 text-sm font-medium transition sm:px-4',
              b.key === activeKey
                ? 'border-slate-200 bg-white text-primary shadow-sm'
                : 'border-transparent text-slate-500 hover:bg-slate-100 hover:text-slate-700',
            )}
          >
            {b.title}
            <span className="rounded-full bg-slate-100 px-2 py-0.5 text-[11px] font-semibold text-slate-500">
              {b.rowCount}
            </span>
          </button>
        ))}
        {groupBoards.length === 0 && !boardsLoading && (
          <span className="py-2 text-sm text-muted-foreground">Bu grupta sekme yok.</span>
        )}
      </div>

      {/* Araç çubuğu */}
      <div className="flex flex-wrap items-center gap-2">
        <div className="relative w-full sm:w-72">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Tüm kolonlarda anlık ara..."
            className="w-full rounded-lg border bg-white py-2 pl-9 pr-8 text-sm outline-none transition focus:border-primary"
          />
          {search && (
            <button
              onClick={() => setSearch('')}
              aria-label="Aramayı temizle"
              className="absolute right-2.5 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-slate-700"
            >
              <X className="h-3.5 w-3.5" />
            </button>
          )}
        </div>
        <select
          value={risk}
          onChange={(e) => setRisk(e.target.value)}
          className="rounded-lg border bg-white px-3 py-2 text-sm outline-none focus:border-primary"
        >
          <option value="">Tüm riskler</option>
          {riskOptions.map((r) => (
            <option key={r} value={r}>
              {riskLevelLabels[r]}
            </option>
          ))}
        </select>
        <div className="ml-auto flex items-center gap-3">
          {sync.isError && <span className="text-sm text-red-600">Senkron hatası</span>}
          <button
            onClick={() => detail?.dataSourceId && sync.mutate(detail.dataSourceId)}
            disabled={!detail?.dataSourceId || sync.isPending}
            className="inline-flex items-center gap-2 rounded-lg bg-primary px-3.5 py-2 text-sm font-medium text-white transition hover:opacity-90 disabled:opacity-50"
          >
            <RefreshCw className={cn('h-4 w-4', sync.isPending && 'animate-spin')} />
            <span className="hidden sm:inline">Senkronize Et</span>
          </button>
        </div>
      </div>

      <Card className="p-1.5">
        {isLoading || (isFetching && !detail) ? (
          <div className="py-16 text-center text-sm text-muted-foreground">Yükleniyor...</div>
        ) : isError || !detail ? (
          <div className="py-16 text-center text-sm text-muted-foreground">Veri yüklenemedi.</div>
        ) : search && filteredRows.length === 0 ? (
          <div className="py-16 text-center text-sm text-muted-foreground">"{search}" için sonuç bulunamadı.</div>
        ) : (
          <BoardTable columns={detail.columns} rows={filteredRows} onRowClick={setSelectedRow} />
        )}
      </Card>

      {detail && (
        <BoardRowDrawer
          boardKey={detail.key}
          boardTitle={detail.title}
          columns={detail.columns}
          row={selectedRow}
          onClose={() => setSelectedRow(null)}
        />
      )}
    </div>
  )
}
