import { useState } from 'react'
import {
  ResponsiveContainer, LineChart, Line, XAxis, YAxis, Tooltip, CartesianGrid, Legend,
} from 'recharts'
import { TrendingUp, Loader2 } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { useKpiTrends } from '@/features/kpi/api'
import { cn } from '@/lib/utils'

const RANGES = [
  { label: '7 gün', days: 7 },
  { label: '30 gün', days: 30 },
  { label: '90 gün', days: 90 },
] as const

const SERIES = [
  { key: 'current', name: 'Güncel', color: '#0ea5e9' },
  { key: 'delayed', name: 'Geciken', color: '#f59e0b' },
  { key: 'risky', name: 'Riskli', color: '#f97316' },
  { key: 'critical', name: 'Kritik', color: '#ef4444' },
  { key: 'openAlerts', name: 'Açık Uyarı', color: '#8b5cf6' },
] as const

function fmtDate(iso: string) {
  const d = new Date(iso)
  return `${String(d.getDate()).padStart(2, '0')}.${String(d.getMonth() + 1).padStart(2, '0')}`
}

export function KpiTrendChart() {
  const [days, setDays] = useState(30)
  const { data, isLoading } = useKpiTrends(days)

  const points = (data ?? []).map((p) => ({ ...p, label: fmtDate(p.date) }))

  return (
    <Card className="p-4">
      <div className="mb-3 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <TrendingUp className="h-4 w-4 text-primary" />
          <h3 className="text-sm font-semibold text-slate-900">Operasyon Trendi</h3>
        </div>
        <div className="flex gap-1">
          {RANGES.map((r) => (
            <button
              key={r.days}
              onClick={() => setDays(r.days)}
              className={cn(
                'rounded-md px-2.5 py-1 text-xs font-medium transition',
                days === r.days ? 'bg-primary text-white' : 'bg-secondary text-slate-600 hover:bg-secondary/70',
              )}
            >
              {r.label}
            </button>
          ))}
        </div>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center gap-2 text-sm text-muted-foreground">
          <Loader2 className="h-4 w-4 animate-spin" /> Yükleniyor...
        </div>
      ) : points.length < 2 ? (
        <div className="flex h-64 flex-col items-center justify-center gap-2 text-center">
          <TrendingUp className="h-8 w-8 text-slate-300" />
          <p className="text-sm text-muted-foreground">Trend için henüz yeterli geçmiş veri yok.</p>
          <p className="max-w-md text-xs text-muted-foreground">
            Her senkronizasyonda ve her gün otomatik anlık görüntü alınır; grafik birkaç gün içinde dolmaya başlar.
          </p>
        </div>
      ) : (
        <ResponsiveContainer width="100%" height={280}>
          <LineChart data={points} margin={{ top: 8, right: 12, bottom: 0, left: -16 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
            <XAxis dataKey="label" tick={{ fontSize: 11 }} stroke="#94a3b8" />
            <YAxis tick={{ fontSize: 11 }} stroke="#94a3b8" allowDecimals={false} />
            <Tooltip contentStyle={{ fontSize: 12, borderRadius: 8 }} />
            <Legend wrapperStyle={{ fontSize: 12 }} />
            {SERIES.map((s) => (
              <Line
                key={s.key}
                type="monotone"
                dataKey={s.key}
                name={s.name}
                stroke={s.color}
                strokeWidth={2}
                dot={false}
                activeDot={{ r: 4 }}
              />
            ))}
          </LineChart>
        </ResponsiveContainer>
      )}
    </Card>
  )
}
