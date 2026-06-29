import { Badge } from '@/components/ui/Badge'
import { riskLevelClasses, riskLevelLabels, tr } from '@/lib/labels'
import { cn } from '@/lib/utils'

export function RiskBadge({ risk }: { risk: string }) {
  return (
    <Badge className={cn('ring-1 ring-inset', riskLevelClasses[risk] ?? 'bg-slate-100 text-slate-700')}>
      {tr(riskLevelLabels, risk)}
    </Badge>
  )
}
