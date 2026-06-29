import type { ReactNode } from 'react'
import { cn } from '@/lib/utils'

type Tone = 'default' | 'danger' | 'warning' | 'success' | 'info'

const toneStyles: Record<Tone, { icon: string; value: string }> = {
  default: { icon: 'bg-slate-100 text-slate-600', value: 'text-slate-900' },
  danger: { icon: 'bg-red-100 text-red-600', value: 'text-red-600' },
  warning: { icon: 'bg-orange-100 text-orange-600', value: 'text-orange-600' },
  success: { icon: 'bg-emerald-100 text-emerald-600', value: 'text-emerald-600' },
  info: { icon: 'bg-blue-100 text-blue-600', value: 'text-blue-700' },
}

interface KpiCardProps {
  label: string
  value: ReactNode
  icon?: ReactNode
  tone?: Tone
  hint?: string
  onClick?: () => void
}

export function KpiCard({ label, value, icon, tone = 'default', hint, onClick }: KpiCardProps) {
  const styles = toneStyles[tone]
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        'group flex items-center gap-4 rounded-xl border bg-card p-4 text-left shadow-card transition',
        onClick && 'hover:border-primary/40 hover:shadow-md cursor-pointer',
      )}
    >
      {icon && (
        <div className={cn('flex h-11 w-11 shrink-0 items-center justify-center rounded-lg', styles.icon)}>
          {icon}
        </div>
      )}
      <div className="min-w-0">
        <div className={cn('text-2xl font-bold leading-tight', styles.value)}>{value}</div>
        <div className="truncate text-sm text-muted-foreground">{label}</div>
        {hint && <div className="text-xs text-muted-foreground/70">{hint}</div>}
      </div>
    </button>
  )
}
