import { useState } from 'react'
import type { ReactNode } from 'react'
import { ShieldAlert, ListTodo, RefreshCw, CheckCircle2, ClipboardPlus } from 'lucide-react'
import { Badge } from '@/components/ui/Badge'
import { Card } from '@/components/ui/Card'
import { useAlerts, useEvaluateRisk, useResolveAlert, useCreateTaskFromAlert } from '@/features/alerts/api'
import { useTasks, useCompleteTask } from '@/features/tasks/api'
import { riskLevelClasses, riskLevelLabels, tr } from '@/lib/labels'
import { cn, formatDateTime } from '@/lib/utils'

const alertTypeLabels: Record<string, string> = {
  Delay: 'Gecikme',
  MissingDocuments: 'Evrak Eksik',
  PaymentRisk: 'Tahsilat Riski',
  OperationalDeviation: 'Operasyonel Sapma',
  CustomerInfoGap: 'Müşteri Bilgilendirme',
  EtaChange: 'ETA Değişikliği',
  FreeTimeDemurrageRisk: 'Demuraj Riski',
  Rework: 'Rework',
  CriticalCustomer: 'Kritik Müşteri',
  ManagementApproval: 'Yönetim Onayı',
  NextActionMissing: 'Sonraki Aksiyon Eksik',
}

const taskStatusLabels: Record<string, string> = {
  New: 'Yeni', InProgress: 'Devam Ediyor', OnHold: 'Beklemede',
  Completed: 'Tamamlandı', Cancelled: 'İptal', Escalated: 'Eskalasyon',
}

const priorityClasses: Record<string, string> = {
  Low: 'bg-slate-100 text-slate-600',
  Normal: 'bg-blue-100 text-blue-700',
  High: 'bg-orange-100 text-orange-700',
  Critical: 'bg-red-100 text-red-700',
}
const priorityLabels: Record<string, string> = { Low: 'Düşük', Normal: 'Normal', High: 'Yüksek', Critical: 'Kritik' }

const groupOptions = ['', 'Deniz', 'Kara', 'Hava', 'Finans']
const groupLabel: Record<string, string> = { '': 'Tüm Gruplar', Deniz: 'Deniz', Kara: 'Kara', Hava: 'Hava', Finans: 'Finans' }

export function AlertsTasksPage() {
  const [tab, setTab] = useState<'alerts' | 'tasks'>('alerts')
  const [group, setGroup] = useState('')
  const alerts = useAlerts({ pageSize: 200, group: group || undefined })
  const tasks = useTasks({ pageSize: 100 })
  const evaluate = useEvaluateRisk()
  const resolve = useResolveAlert()
  const createTask = useCreateTaskFromAlert()
  const complete = useCompleteTask()

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Uyarılar ve Görevler</h1>
          <p className="text-sm text-muted-foreground">Risk motorunun ürettiği uyarılar ve aksiyon görevleri</p>
        </div>
        <button
          onClick={() => evaluate.mutate()}
          disabled={evaluate.isPending}
          className="flex items-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-white transition hover:bg-primary/90 disabled:opacity-60"
        >
          <RefreshCw className={cn('h-4 w-4', evaluate.isPending && 'animate-spin')} />
          {evaluate.isPending ? 'Değerlendiriliyor...' : 'Risk Değerlendir'}
        </button>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-2 border-b pb-0">
        <div className="flex gap-2">
          <TabButton active={tab === 'alerts'} onClick={() => setTab('alerts')} icon={<ShieldAlert className="h-4 w-4" />}
            label={`Uyarılar (${alerts.data?.totalCount ?? 0})`} />
          <TabButton active={tab === 'tasks'} onClick={() => setTab('tasks')} icon={<ListTodo className="h-4 w-4" />}
            label={`Görevler (${tasks.data?.totalCount ?? 0})`} />
        </div>
        {tab === 'alerts' && (
          <select
            value={group}
            onChange={(e) => setGroup(e.target.value)}
            className="mb-1.5 rounded-lg border bg-white px-2.5 py-1.5 text-sm outline-none focus:border-primary"
          >
            {groupOptions.map((g) => (
              <option key={g || 'all'} value={g}>{groupLabel[g]}</option>
            ))}
          </select>
        )}
      </div>

      {tab === 'alerts' && (
        <div className="space-y-2">
          {alerts.isLoading ? (
            <Card className="py-12 text-center text-muted-foreground">Yükleniyor...</Card>
          ) : !alerts.data?.items.length ? (
            <Card className="py-12 text-center text-muted-foreground">Açık uyarı yok. 🎉</Card>
          ) : (
            alerts.data.items.map((a) => (
              <Card key={a.id} className="flex flex-wrap items-center gap-3 p-4">
                <Badge className={cn('ring-1 ring-inset', riskLevelClasses[a.riskLevel])}>
                  {tr(riskLevelLabels, a.riskLevel)}
                </Badge>
                <div className="min-w-0 flex-1">
                  <div className="text-sm font-medium text-slate-900">
                    {alertTypeLabels[a.type] ?? a.type} ·{' '}
                    {a.boardKey
                      ? <>{a.boardTitle} · {a.recordRef} <span className="text-muted-foreground">({a.group})</span></>
                      : <>{a.operationNo ?? `#${a.operationId}`} · {a.customerName}</>}
                  </div>
                  <div className="text-sm text-muted-foreground">{a.description}</div>
                  <div className="text-xs text-muted-foreground/70">{formatDateTime(a.lastTriggeredAt)} · {a.triggerCount}x</div>
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => createTask.mutate(a.id)}
                    disabled={createTask.isPending}
                    className="flex items-center gap-1 rounded-lg border px-2.5 py-1.5 text-xs font-medium text-slate-700 hover:bg-secondary"
                  >
                    <ClipboardPlus className="h-3.5 w-3.5" /> Görev
                  </button>
                  <button
                    onClick={() => resolve.mutate({ id: a.id })}
                    disabled={resolve.isPending}
                    className="flex items-center gap-1 rounded-lg border px-2.5 py-1.5 text-xs font-medium text-emerald-700 hover:bg-emerald-50"
                  >
                    <CheckCircle2 className="h-3.5 w-3.5" /> Çözüldü
                  </button>
                </div>
              </Card>
            ))
          )}
        </div>
      )}

      {tab === 'tasks' && (
        <div className="space-y-2">
          {tasks.isLoading ? (
            <Card className="py-12 text-center text-muted-foreground">Yükleniyor...</Card>
          ) : !tasks.data?.items.length ? (
            <Card className="py-12 text-center text-muted-foreground">Görev yok.</Card>
          ) : (
            tasks.data.items.map((t) => (
              <Card key={t.id} className="flex flex-wrap items-center gap-3 p-4">
                <Badge className={priorityClasses[t.priority] ?? 'bg-slate-100 text-slate-600'}>
                  {priorityLabels[t.priority] ?? t.priority}
                </Badge>
                <div className="min-w-0 flex-1">
                  <div className="text-sm font-medium text-slate-900">{t.title}</div>
                  <div className="text-xs text-muted-foreground">
                    {t.operationNo ? `${t.operationNo} · ` : ''}{taskStatusLabels[t.status] ?? t.status}
                    {t.ownerName ? ` · ${t.ownerName}` : ''}
                  </div>
                </div>
                {t.status !== 'Completed' && t.status !== 'Cancelled' && (
                  <button
                    onClick={() => complete.mutate({ id: t.id })}
                    disabled={complete.isPending}
                    className="flex items-center gap-1 rounded-lg border px-2.5 py-1.5 text-xs font-medium text-emerald-700 hover:bg-emerald-50"
                  >
                    <CheckCircle2 className="h-3.5 w-3.5" /> Tamamla
                  </button>
                )}
              </Card>
            ))
          )}
        </div>
      )}
    </div>
  )
}

function TabButton({ active, onClick, icon, label }: { active: boolean; onClick: () => void; icon: ReactNode; label: string }) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'flex items-center gap-2 border-b-2 px-4 py-2 text-sm font-medium transition',
        active ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-slate-700',
      )}
    >
      {icon}
      {label}
    </button>
  )
}
