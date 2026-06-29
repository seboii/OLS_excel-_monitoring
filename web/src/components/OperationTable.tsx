import type { OperationListItem } from '@/services/types'
import { StatusBadge } from './StatusBadge'
import { RiskBadge } from './RiskBadge'
import { transportTypeLabels, serviceTypeLabels, financeStatusLabels, tr } from '@/lib/labels'

export function OperationTable({
  items,
  onRowClick,
}: {
  items: OperationListItem[]
  onRowClick?: (id: number) => void
}) {
  if (items.length === 0) {
    return <div className="py-16 text-center text-sm text-muted-foreground">Bu kritere uygun kayıt bulunamadı.</div>
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b text-left text-xs uppercase tracking-wide text-muted-foreground [&>th]:px-3 [&>th]:py-2.5 [&>th]:font-medium">
            <th>Operasyon No</th>
            <th>Taşıma</th>
            <th>Müşteri</th>
            <th>Rota</th>
            <th>Statü</th>
            <th>Risk</th>
            <th>Gecikme</th>
            <th>Sorumlu</th>
            <th>Finans</th>
          </tr>
        </thead>
        <tbody>
          {items.map((o) => (
            <tr
              key={o.id}
              onClick={() => onRowClick?.(o.id)}
              className="cursor-pointer border-b transition last:border-0 hover:bg-secondary/40 [&>td]:px-3 [&>td]:py-2.5"
            >
              <td className="font-medium text-slate-900">{o.operationNo ?? '—'}</td>
              <td>
                <div className="text-slate-700">{tr(transportTypeLabels, o.transportType)}</div>
                <div className="text-xs text-muted-foreground">{tr(serviceTypeLabels, o.serviceType)}</div>
              </td>
              <td className="max-w-[200px] truncate text-slate-700">{o.customerName}</td>
              <td className="text-slate-600">
                {(o.originCity ?? o.originCountry) ?? '—'} → {(o.destinationCity ?? o.destinationCountry) ?? '—'}
              </td>
              <td><StatusBadge status={o.status} /></td>
              <td><RiskBadge risk={o.riskLevel} /></td>
              <td>
                {o.delayDays > 0
                  ? <span className="font-semibold text-red-600">{o.delayDays} gün</span>
                  : <span className="text-muted-foreground">—</span>}
              </td>
              <td className="text-slate-600">{o.responsibleUserName ?? '—'}</td>
              <td className="text-slate-600">{tr(financeStatusLabels, o.financeStatus)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
