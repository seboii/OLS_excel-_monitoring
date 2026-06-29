import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Search } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { OperationTable } from '@/components/OperationTable'
import { OperationDetailDrawer } from '@/components/OperationDetailDrawer'
import { useOperations } from '@/features/operations/api'

const quickLabels: Record<string, string> = {
  active: 'Aktif',
  delayed: 'Geciken',
  risky: 'Riskli',
  missingDocs: 'Evrak Eksik',
  financialHold: 'Finansal Hold',
  completed: 'Tamamlanan',
  critical: 'Kritik Müşteri',
}

export function OperationsPage({ transport, title }: { transport?: string; title: string }) {
  const [searchParams] = useSearchParams()
  const quick = searchParams.get('quick') ?? undefined
  const [search, setSearch] = useState('')
  const [selectedId, setSelectedId] = useState<number | null>(null)

  const { data, isLoading, isError } = useOperations({
    transport,
    quick,
    search: search.trim() || undefined,
    pageSize: 100,
  })

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">{title}</h1>
          <p className="text-sm text-muted-foreground">
            {data ? `${data.totalCount} kayıt` : 'Yükleniyor...'}
            {quick ? ` · Filtre: ${quickLabels[quick] ?? quick}` : ''}
          </p>
        </div>
        <div className="relative w-72">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Müşteri veya operasyon no ara..."
            className="w-full rounded-lg border bg-white py-2 pl-9 pr-3 text-sm outline-none transition focus:border-primary"
          />
        </div>
      </div>

      <Card className="p-1.5">
        {isLoading ? (
          <div className="py-16 text-center text-sm text-muted-foreground">Yükleniyor...</div>
        ) : isError || !data ? (
          <div className="py-16 text-center text-sm text-muted-foreground">Veri yüklenemedi.</div>
        ) : (
          <OperationTable items={data.items} onRowClick={setSelectedId} />
        )}
      </Card>

      <OperationDetailDrawer operationId={selectedId} onClose={() => setSelectedId(null)} />
    </div>
  )
}
