import { useState } from 'react'
import type { ReactNode } from 'react'
import { Layers, LayoutGrid, TrendingUp } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { KpiTrendChart } from '@/components/KpiTrendChart'
import { useBoardKpi, useGroupKpi } from '@/features/kpi/api'
import { cn } from '@/lib/utils'

const GROUP_COLORS: Record<string, string> = { Deniz: 'bg-sky-500', Kara: 'bg-amber-500', Hava: 'bg-indigo-500' }

export function KpiPage() {
  const [tab, setTab] = useState<'trend' | 'group' | 'board'>('trend')
  const group = useGroupKpi()
  const board = useBoardKpi()

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">KPI ve Performans</h1>
        <p className="text-sm text-muted-foreground">Grup (Deniz/Kara/Hava) ve sekme bazlı operasyon performansı</p>
      </div>

      <div className="flex gap-2 border-b">
        <Tab active={tab === 'trend'} onClick={() => setTab('trend')} icon={<TrendingUp className="h-4 w-4" />} label="Trend" />
        <Tab active={tab === 'group'} onClick={() => setTab('group')} icon={<LayoutGrid className="h-4 w-4" />} label="Grup" />
        <Tab active={tab === 'board'} onClick={() => setTab('board')} icon={<Layers className="h-4 w-4" />} label="Sekme" />
      </div>

      {tab === 'trend' && <KpiTrendChart />}

      {tab === 'group' && (
        <Card className="overflow-x-auto p-1.5">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-xs uppercase tracking-wide text-muted-foreground [&>th]:px-3 [&>th]:py-2.5 [&>th]:font-medium">
                <th>Grup</th><th>Sekme</th><th>Toplam</th><th>Aktif</th><th>Geciken</th><th>Riskli</th><th>Kritik</th><th>Ort. Gecikme</th>
              </tr>
            </thead>
            <tbody>
              {group.data?.map((g) => (
                <tr key={g.group} className="border-b last:border-0 [&>td]:px-3 [&>td]:py-2.5">
                  <td className="font-medium text-slate-900">
                    <span className="inline-flex items-center gap-2">
                      <span className={cn('h-2.5 w-2.5 rounded-full', GROUP_COLORS[g.group] ?? 'bg-slate-400')} />
                      {g.group}
                    </span>
                  </td>
                  <td>{g.boards}</td><td>{g.total}</td><td>{g.active}</td>
                  <td className={g.delayed > 0 ? 'font-semibold text-red-600' : ''}>{g.delayed}</td>
                  <td className={g.risky > 0 ? 'font-semibold text-orange-600' : ''}>{g.risky}</td>
                  <td className={g.critical > 0 ? 'font-semibold text-red-700' : ''}>{g.critical}</td>
                  <td>{g.avgDelayDays}</td>
                </tr>
              ))}
            </tbody>
          </table>
          {!group.isLoading && !group.data?.length && <div className="py-12 text-center text-muted-foreground">Veri yok.</div>}
        </Card>
      )}

      {tab === 'board' && (
        <Card className="overflow-x-auto p-1.5">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-xs uppercase tracking-wide text-muted-foreground [&>th]:px-3 [&>th]:py-2.5 [&>th]:font-medium">
                <th>Sekme</th><th>Grup</th><th>Toplam</th><th>Aktif</th><th>Arşiv</th><th>Geciken</th><th>Riskli</th><th>Kritik</th><th>Ort. Gecikme</th><th>Risk Oranı</th>
              </tr>
            </thead>
            <tbody>
              {board.data?.map((b) => (
                <tr key={b.key} className="border-b last:border-0 [&>td]:px-3 [&>td]:py-2.5">
                  <td className="font-medium text-slate-900">{b.title}</td>
                  <td className="text-slate-500">{b.group}</td>
                  <td>{b.total}</td><td>{b.active}</td><td className="text-slate-400">{b.archived}</td>
                  <td className={b.delayed > 0 ? 'font-semibold text-red-600' : ''}>{b.delayed}</td>
                  <td className={b.risky > 0 ? 'font-semibold text-orange-600' : ''}>{b.risky}</td>
                  <td className={b.critical > 0 ? 'font-semibold text-red-700' : ''}>{b.critical}</td>
                  <td>{b.avgDelayDays}</td>
                  <td>
                    <div className="flex items-center gap-2">
                      <div className="h-2 w-20 overflow-hidden rounded-full bg-slate-200">
                        <div
                          className={cn('h-full rounded-full', b.riskRatio >= 0.4 ? 'bg-red-500' : b.riskRatio >= 0.2 ? 'bg-amber-500' : 'bg-emerald-500')}
                          style={{ width: `${Math.round(b.riskRatio * 100)}%` }}
                        />
                      </div>
                      <span className="text-xs font-semibold text-slate-600">%{Math.round(b.riskRatio * 100)}</span>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {!board.isLoading && !board.data?.length && <div className="py-12 text-center text-muted-foreground">Veri yok.</div>}
        </Card>
      )}
    </div>
  )
}

function Tab({ active, onClick, icon, label }: { active: boolean; onClick: () => void; icon: ReactNode; label: string }) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'flex items-center gap-2 border-b-2 px-4 py-2 text-sm font-medium transition',
        active ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-slate-700',
      )}
    >
      {icon}{label}
    </button>
  )
}
