import { useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { RefreshCw, Search, X, FileCheck, FileWarning, Wallet, Clock, PackageCheck, Truck } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { KpiCard } from '@/components/KpiCard'
import { BoardTable } from '@/components/BoardTable'
import { BoardRowDrawer } from '@/components/BoardRowDrawer'
import { useBoard, useBoards, useSyncBoard, filterBoardRows } from '@/features/boards/api'
import type { BoardRow } from '@/features/boards/api'
import { useFinanceSummary } from '@/features/finance/api'
import { cn } from '@/lib/utils'

const CURRENCY_LABEL: Record<string, string> = { USD: 'USD', EUR: 'EUR', RUB: 'RUB', TRY: 'TRY', Belirsiz: 'Belirsiz' }

/**
 * Finans ve Tahsilatlar sayfası — Alabora (СЧЕТА-ПЛАТЕЖИ) tahsilat takip tablosu + finans metrikleri.
 * Deniz/Kara/Hava operasyonel sayfalarından ayrı: burada risk/gecikme değil tahsilat/belge/döviz önemli.
 */
export function FinancePage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const { data: boards } = useBoards()
  const board = boards?.find((b) => b.group === 'Finans')
  const { data: summary, isLoading: summaryLoading } = useFinanceSummary()
  const [search, setSearch] = useState('')
  const [selectedRow, setSelectedRow] = useState<BoardRow | null>(null)

  // ?q= varsa global aramadan gelinmiştir (deep link) — aramayı otomatik uygula.
  useEffect(() => {
    const q = searchParams.get('q')
    if (q) {
      setSearch(q)
      setSearchParams({}, { replace: true })
    }
  }, [searchParams, setSearchParams])

  const { data: detail, isLoading, isError, isFetching } = useBoard(board?.key, { pageSize: 2000 })
  const filteredRows = useMemo(() => (detail ? filterBoardRows(detail.rows, search) : []), [detail, search])
  const sync = useSyncBoard()

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Finans ve Tahsilatlar</h1>
        <p className="text-sm text-muted-foreground">
          Alabora (СЧЕТА-ПЛАТЕЖИ) tahsilat takibi{summary ? ` · ${summary.totalFiles} dosya` : ''}
          {board?.lastSyncAt ? ` · Son senkron: ${new Date(board.lastSyncAt).toLocaleString('tr-TR')}` : ''}
        </p>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4">
        <KpiCard label="Toplam Dosya" value={summary?.totalFiles ?? '—'} icon={<Wallet className="h-5 w-5" />} tone="default" />
        <KpiCard label="Teslim / Boşaltılan" value={summary?.delivered ?? '—'} icon={<PackageCheck className="h-5 w-5" />} tone="success" />
        <KpiCard label="Yolda / Hazırlanıyor" value={summary?.inTransit ?? '—'} icon={<Truck className="h-5 w-5" />} tone="info" />
        <KpiCard label="Tahsil Edilen" value={summary?.paymentReceived ?? '—'} icon={<FileCheck className="h-5 w-5" />} tone="success" />
        <KpiCard label="Bekleyen Tahsilat" value={summary?.paymentPending ?? '—'} icon={<Clock className="h-5 w-5" />} tone="warning" />
        <KpiCard label="Belgeler Tam" value={summary?.docsComplete ?? '—'} icon={<FileCheck className="h-5 w-5" />} tone="success" />
        <KpiCard label="Belge Eksik" value={summary?.docsIncomplete ?? '—'} icon={<FileWarning className="h-5 w-5" />} tone="danger" />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Döviz Bazlı Tutarlar</CardTitle>
        </CardHeader>
        <CardContent className="pt-0">
          {!summary?.byCurrency.length ? (
            <div className="py-4 text-sm text-muted-foreground">{summaryLoading ? 'Yükleniyor...' : 'Veri yok.'}</div>
          ) : (
            <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
              {summary.byCurrency.map((c) => (
                <div key={c.currency} className="rounded-lg border bg-slate-50 p-3">
                  <div className="text-xs font-semibold uppercase text-slate-500">{CURRENCY_LABEL[c.currency] ?? c.currency}</div>
                  <div className="mt-1 text-lg font-bold text-slate-900">
                    {c.total.toLocaleString('tr-TR', { maximumFractionDigits: 0 })}
                  </div>
                  <div className="text-xs text-muted-foreground">{c.count} dosya</div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

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
