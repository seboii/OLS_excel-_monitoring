import { useNavigate } from 'react-router-dom'
import {
  PieChart, Pie, Cell, ResponsiveContainer, BarChart, Bar, XAxis, YAxis, Tooltip,
} from 'recharts'
import {
  Activity, Clock, AlertTriangle, Flame, CheckCircle2, Archive, Timer, LayoutGrid,
} from 'lucide-react'
import { KpiCard } from '@/components/KpiCard'
import { ChartCard } from '@/components/ChartCard'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { RiskBadge } from '@/components/RiskBadge'
import { AiSummaryCard } from '@/components/AiSummaryCard'
import { useDashboardSummary } from '@/features/dashboard/api'
import { riskLevelLabels, tr } from '@/lib/labels'

const RISK_COLORS: Record<string, string> = { Green: '#16a34a', Yellow: '#eab308', Orange: '#f97316', Red: '#dc2626', Black: '#1f2937' }
const GROUP_COLORS: Record<string, string> = { Deniz: '#0ea5e9', Kara: '#f59e0b', Hava: '#6366f1' }
const groupRoute: Record<string, string> = { Deniz: '/deniz', Kara: '/karayolu', Hava: '/hava' }

export function DashboardPage() {
  const navigate = useNavigate()
  const { data, isLoading, isError } = useDashboardSummary()

  if (isLoading) return <DashboardSkeleton />
  if (isError || !data) {
    return (
      <div className="rounded-xl border bg-card p-10 text-center text-muted-foreground">
        Veri yüklenemedi. Backend API çalışıyor mu?
      </div>
    )
  }

  const k = data.kpis
  const riskData = data.riskDistribution.map((d) => ({ name: tr(riskLevelLabels, d.name), value: d.value, key: d.name }))
  const groupData = data.groupDistribution

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Yönetim Paneli</h1>
        <p className="text-sm text-muted-foreground">
          Deniz · Kara · Hava canlı özeti · {k.totalRecords} kayıt · {k.boards} sekme
          {' '}<span className="text-slate-400">(Alabora tahsilat ayrı, bkz. Finans sayfası)</span>
        </p>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4">
        <KpiCard label="Güncel İş" value={k.current} icon={<Activity className="h-5 w-5" />} tone="info" />
        <KpiCard label="Tamamlanan" value={k.completed} icon={<CheckCircle2 className="h-5 w-5" />} tone="success" />
        <KpiCard label="Arşiv" value={k.archived} icon={<Archive className="h-5 w-5" />} tone="default" />
        <KpiCard label="Geciken" value={k.delayed} icon={<Clock className="h-5 w-5" />} tone="danger" />
        <KpiCard label="Riskli" value={k.risky} icon={<AlertTriangle className="h-5 w-5" />} tone="warning" />
        <KpiCard label="Kritik" value={k.critical} icon={<Flame className="h-5 w-5" />} tone="danger" />
        <KpiCard label="Ort. Gecikme (gün)" value={k.avgDelayDays} icon={<Timer className="h-5 w-5" />} tone="warning" />
        <KpiCard label="Sekme Sayısı" value={k.boards} icon={<LayoutGrid className="h-5 w-5" />} tone="info" />
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <ChartCard title="Risk Dağılımı (güncel)">
          <div className="flex items-center gap-4">
            <div className="h-44 w-44 shrink-0">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie data={riskData} dataKey="value" nameKey="name" innerRadius={48} outerRadius={82} paddingAngle={2}>
                    {riskData.map((d) => <Cell key={d.key} fill={RISK_COLORS[d.key] ?? '#94a3b8'} />)}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </div>
            <ul className="flex-1 space-y-1.5 text-sm">
              {riskData.map((d) => (
                <li key={d.key} className="flex items-center justify-between gap-2">
                  <span className="flex items-center gap-2 text-slate-600">
                    <span className="h-2.5 w-2.5 rounded-full" style={{ background: RISK_COLORS[d.key] ?? '#94a3b8' }} />
                    {d.name}
                  </span>
                  <span className="font-semibold text-slate-900">{d.value}</span>
                </li>
              ))}
            </ul>
          </div>
        </ChartCard>

        <ChartCard title="Grup Dağılımı (güncel)">
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={groupData} margin={{ top: 8, right: 8, bottom: 0, left: -20 }}>
              <XAxis dataKey="name" tickLine={false} axisLine={false} fontSize={12} />
              <YAxis allowDecimals={false} tickLine={false} axisLine={false} fontSize={12} />
              <Tooltip cursor={{ fill: 'rgba(37,99,235,0.06)' }} />
              <Bar dataKey="value" radius={[6, 6, 0, 0]} maxBarSize={64}>
                {groupData.map((d) => <Cell key={d.name} fill={GROUP_COLORS[d.name] ?? '#2563eb'} />)}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Sekme Yükü (güncel)">
          <ResponsiveContainer width="100%" height={Math.max(180, data.boardLoad.length * 26)}>
            <BarChart data={data.boardLoad} layout="vertical" margin={{ top: 4, right: 16, bottom: 4, left: 12 }}>
              <XAxis type="number" allowDecimals={false} hide />
              <YAxis type="category" dataKey="name" width={130} tickLine={false} axisLine={false} fontSize={11} />
              <Tooltip cursor={{ fill: 'rgba(37,99,235,0.06)' }} />
              <Bar dataKey="value" fill="#0ea5e9" radius={[0, 6, 6, 0]} maxBarSize={20} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Dikkat Listesi — en riskli / geciken kayıtlar</CardTitle>
        </CardHeader>
        <CardContent className="pt-0">
          {data.attention.length === 0 ? (
            <div className="py-8 text-center text-sm text-muted-foreground">Riskli veya geciken aktif kayıt yok. 🎉</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left text-xs uppercase tracking-wide text-muted-foreground [&>th]:px-3 [&>th]:py-2 [&>th]:font-medium">
                    <th>Dosya / Ref No</th><th>Sekme</th><th>Risk</th><th>Gecikme</th><th>Durum</th>
                  </tr>
                </thead>
                <tbody>
                  {data.attention.map((a, i) => (
                    <tr
                      key={`${a.boardKey}-${a.ref}-${i}`}
                      onClick={() => navigate(groupRoute[a.group] ?? '/')}
                      className="cursor-pointer border-b last:border-0 hover:bg-slate-50 [&>td]:px-3 [&>td]:py-2"
                    >
                      <td className="font-medium text-slate-900 whitespace-nowrap">{a.ref}</td>
                      <td className="whitespace-nowrap text-slate-600">{a.boardTitle}</td>
                      <td><RiskBadge risk={a.risk} /></td>
                      <td className="whitespace-nowrap">
                        {a.delayDays > 0 ? <span className="font-semibold text-red-600">{a.delayDays} gün</span> : <span className="text-slate-300">—</span>}
                      </td>
                      <td className="max-w-[360px] truncate text-slate-600" title={a.status ?? undefined}>{a.status ?? '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      <AiSummaryCard />
    </div>
  )
}

function DashboardSkeleton() {
  return (
    <div className="space-y-6">
      <div className="h-8 w-48 animate-pulse rounded bg-slate-200" />
      <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <div key={i} className="h-20 animate-pulse rounded-xl bg-slate-200" />
        ))}
      </div>
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="h-64 animate-pulse rounded-xl bg-slate-200" />
        ))}
      </div>
    </div>
  )
}
