import { Link } from 'react-router-dom'
import { X } from 'lucide-react'
import { useDashboardSummary } from '@/features/dashboard/api'

export function TvPage() {
  const { data } = useDashboardSummary()
  const k = data?.kpis

  const tiles: { label: string; value: number | undefined; accent: string }[] = [
    { label: 'Güncel İş', value: k?.current, accent: 'text-sky-400' },
    { label: 'Geciken', value: k?.delayed, accent: 'text-red-400' },
    { label: 'Riskli', value: k?.risky, accent: 'text-orange-400' },
    { label: 'Kritik', value: k?.critical, accent: 'text-red-400' },
    { label: 'Tamamlanan', value: k?.completed, accent: 'text-emerald-400' },
    { label: 'Arşiv', value: k?.archived, accent: 'text-slate-200' },
  ]

  return (
    <div className="min-h-screen bg-sidebar p-10 text-white">
      <div className="mb-10 flex items-center justify-between">
        <div className="flex items-center gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary text-xl font-bold">OLS</div>
          <div>
            <div className="text-2xl font-bold">Operasyon Kontrol Merkezi</div>
            <div className="text-sm text-sidebar-muted">Canlı Durum Ekranı</div>
          </div>
        </div>
        <Link to="/" className="flex items-center gap-2 rounded-lg bg-white/10 px-4 py-2 text-sm hover:bg-white/20">
          <X className="h-4 w-4" /> Çıkış
        </Link>
      </div>

      <div className="grid grid-cols-2 gap-6 md:grid-cols-3">
        {tiles.map((t) => (
          <div key={t.label} className="rounded-2xl bg-white/5 p-8 ring-1 ring-white/10">
            <div className={`text-6xl font-bold ${t.accent}`}>{t.value ?? '—'}</div>
            <div className="mt-2 text-lg text-sidebar-foreground">{t.label}</div>
          </div>
        ))}
      </div>

      <div className="mt-10">
        <div className="mb-3 text-sm font-semibold uppercase tracking-wider text-sidebar-muted">Dikkat Listesi</div>
        <div className="space-y-2">
          {data?.attention.slice(0, 8).map((a, i) => (
            <div key={`${a.boardKey}-${a.ref}-${i}`} className="flex items-center justify-between rounded-lg bg-white/5 px-5 py-3">
              <span className="font-medium">{a.ref} · <span className="text-sidebar-muted">{a.boardTitle}</span></span>
              <span className="flex items-center gap-4 text-sidebar-foreground">
                {a.delayDays > 0 && <span className="text-red-400">{a.delayDays} gün</span>}
                <span className="truncate max-w-[40ch]">{a.status ?? '—'}</span>
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
