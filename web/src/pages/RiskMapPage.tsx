import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Card } from '@/components/ui/Card'
import { RiskBadge } from '@/components/RiskBadge'
import { useAttention } from '@/features/boards/api'
import { cn } from '@/lib/utils'

const groups = ['', 'Deniz', 'Kara', 'Hava']
const groupLabel: Record<string, string> = { '': 'Tüm Gruplar', Deniz: 'Deniz', Kara: 'Kara', Hava: 'Hava' }
const riskFilters: { value: string; label: string }[] = [
  { value: 'Yellow', label: 'Takip ve üzeri' },
  { value: 'Orange', label: 'Risk ve üzeri' },
  { value: 'Red', label: 'Kritik ve üzeri' },
]
const groupRoute: Record<string, string> = { Deniz: '/deniz', Kara: '/karayolu', Hava: '/hava' }

/** Risk Haritası — tüm sekmelerdeki riskli/geciken aktif kayıtların tek listede toplandığı görünüm. */
export function RiskMapPage() {
  const navigate = useNavigate()
  const [group, setGroup] = useState('')
  const [minRisk, setMinRisk] = useState('Yellow')
  const { data, isLoading } = useAttention({ group: group || undefined, minRisk })

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Risk Haritası</h1>
        <p className="text-sm text-muted-foreground">
          Tüm sekmelerdeki dikkat gerektiren aktif kayıtlar {data ? `· ${data.length} kayıt` : ''}
        </p>
      </div>

      <div className="flex flex-wrap items-center gap-2">
        <div className="flex gap-1.5">
          {groups.map((g) => (
            <button
              key={g || 'all'}
              onClick={() => setGroup(g)}
              className={cn(
                'rounded-lg px-3 py-1.5 text-sm font-medium transition',
                group === g ? 'bg-primary text-white' : 'bg-slate-100 text-slate-600 hover:bg-slate-200',
              )}
            >
              {groupLabel[g]}
            </button>
          ))}
        </div>
        <select
          value={minRisk}
          onChange={(e) => setMinRisk(e.target.value)}
          className="ml-auto rounded-lg border bg-white px-3 py-2 text-sm outline-none focus:border-primary"
        >
          {riskFilters.map((r) => (
            <option key={r.value} value={r.value}>{r.label}</option>
          ))}
        </select>
      </div>

      <Card className="p-1.5">
        {isLoading ? (
          <div className="py-16 text-center text-sm text-muted-foreground">Yükleniyor...</div>
        ) : !data?.length ? (
          <div className="py-16 text-center text-sm text-muted-foreground">Bu filtrede dikkat gerektiren kayıt yok. 🎉</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-500 [&>th]:px-3 [&>th]:py-2.5 [&>th]:font-semibold">
                  <th>Dosya / Ref No</th><th>Sekme</th><th>Grup</th><th>Risk</th><th>Gecikme</th><th>Durum</th>
                </tr>
              </thead>
              <tbody>
                {data.map((a, i) => (
                  <tr
                    key={`${a.boardKey}-${a.ref}-${i}`}
                    onClick={() => navigate(groupRoute[a.group] ?? '/')}
                    className="cursor-pointer border-b last:border-0 hover:bg-slate-50 [&>td]:px-3 [&>td]:py-2"
                  >
                    <td className="font-medium text-slate-900 whitespace-nowrap">{a.ref}</td>
                    <td className="whitespace-nowrap text-slate-600">{a.boardTitle}</td>
                    <td className="whitespace-nowrap text-slate-500">{a.group}</td>
                    <td><RiskBadge risk={a.risk} /></td>
                    <td className="whitespace-nowrap">
                      {a.delayDays > 0 ? <span className="font-semibold text-red-600">{a.delayDays} gün</span> : <span className="text-slate-300">—</span>}
                    </td>
                    <td className="max-w-[420px] truncate text-slate-600" title={a.status ?? undefined}>{a.status ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </div>
  )
}
