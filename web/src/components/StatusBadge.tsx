import { Badge } from '@/components/ui/Badge'
import { statusClasses, operationStatusLabels, tr } from '@/lib/labels'

export function StatusBadge({ status }: { status: string }) {
  return (
    <Badge className={statusClasses[status] ?? 'bg-slate-100 text-slate-700'}>
      {tr(operationStatusLabels, status)}
    </Badge>
  )
}
