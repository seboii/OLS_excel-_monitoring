import { useState } from 'react'
import {
  Database, Plus, Plug, Eye, Pencil, Trash2, CheckCircle2, XCircle, RefreshCw,
} from 'lucide-react'
import { cn, formatDateTime } from '@/lib/utils'
import {
  apiErrorMessage,
  useDataSources,
  useDeleteDataSource,
  useTestConnection,
  type DataSource,
} from '@/features/datasources/api'
import { sourceTypeLabels, syncStatusClasses, syncStatusLabels } from '@/features/datasources/fields'
import { DataSourceFormModal } from '@/features/datasources/DataSourceFormModal'
import { DataSourceWorkbench } from '@/features/datasources/DataSourceWorkbench'

export function DataSourcesPage() {
  const sources = useDataSources()
  const del = useDeleteDataSource()
  const test = useTestConnection()

  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<DataSource | null>(null)
  const [workbench, setWorkbench] = useState<DataSource | null>(null)
  const [testingId, setTestingId] = useState<number | null>(null)
  const [toast, setToast] = useState<{ ok: boolean; text: string } | null>(null)

  const openCreate = () => { setEditing(null); setShowForm(true) }
  const openEdit = (s: DataSource) => { setEditing(s); setShowForm(true) }

  const runTest = async (s: DataSource) => {
    setTestingId(s.id)
    setToast(null)
    try {
      const res = await test.mutateAsync(s.id)
      setToast({ ok: true, text: `Bağlantı başarılı: "${res.fileName}" (${(res.sizeBytes / 1024).toFixed(0)} KB, ${res.sheetNames.length} sayfa).` })
    } catch (err) {
      setToast({ ok: false, text: apiErrorMessage(err, 'Bağlantı testi başarısız.') })
    } finally {
      setTestingId(null)
    }
  }

  const remove = async (s: DataSource) => {
    if (!confirm(`"${s.name}" kaynağını silmek istediğinize emin misiniz?`)) return
    try {
      await del.mutateAsync(s.id)
      setToast({ ok: true, text: 'Veri kaynağı silindi.' })
    } catch (err) {
      setToast({ ok: false, text: apiErrorMessage(err, 'Silinemedi.') })
    }
  }

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Veri Kaynakları</h1>
          <p className="text-sm text-muted-foreground">
            Yandex / SharePoint public Excel ve manuel yükleme kaynakları — indir, önizle, eşleştir, aktar.
          </p>
        </div>
        <button
          onClick={openCreate}
          className="flex items-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-white transition hover:bg-primary/90"
        >
          <Plus className="h-4 w-4" /> Kaynak Ekle
        </button>
      </div>

      {toast && (
        <div className={cn(
          'flex items-center gap-2 rounded-lg px-4 py-3 text-sm',
          toast.ok ? 'bg-emerald-50 text-emerald-800' : 'bg-red-50 text-red-700',
        )}>
          {toast.ok ? <CheckCircle2 className="h-4 w-4" /> : <XCircle className="h-4 w-4" />}
          {toast.text}
        </div>
      )}

      <div className="overflow-hidden rounded-xl border bg-white">
        {sources.isLoading ? (
          <div className="p-8 text-center text-sm text-muted-foreground">Yükleniyor...</div>
        ) : (sources.data?.length ?? 0) === 0 ? (
          <div className="flex flex-col items-center gap-2 p-10 text-center text-muted-foreground">
            <Database className="h-8 w-8" />
            <p className="text-sm">Henüz veri kaynağı yok. "Kaynak Ekle" ile başlayın.</p>
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead className="border-b bg-slate-50 text-xs uppercase text-muted-foreground">
              <tr>
                <th className="px-4 py-3">Kaynak</th>
                <th className="px-4 py-3">Tip</th>
                <th className="px-4 py-3">Son Senkron</th>
                <th className="px-4 py-3">Durum</th>
                <th className="px-4 py-3">Eşleştirme</th>
                <th className="px-4 py-3 text-right">İşlemler</th>
              </tr>
            </thead>
            <tbody>
              {sources.data!.map((s) => (
                <tr key={s.id} className="border-b last:border-0 hover:bg-slate-50/50">
                  <td className="px-4 py-3">
                    <div className="font-medium text-slate-900">{s.name}</div>
                    {s.url && <div className="max-w-xs truncate text-xs text-muted-foreground" title={s.url}>{s.url}</div>}
                    {!s.isActive && <span className="text-xs text-amber-600">Pasif</span>}
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap text-slate-600">{sourceTypeLabels[s.type] ?? s.type}</td>
                  <td className="px-4 py-3 whitespace-nowrap text-slate-600">{s.lastSyncAt ? formatDateTime(s.lastSyncAt) : '—'}</td>
                  <td className="px-4 py-3">
                    {s.lastSyncStatus ? (
                      <span className={cn('rounded-full px-2 py-0.5 text-xs font-medium', syncStatusClasses[s.lastSyncStatus] ?? 'bg-slate-100 text-slate-600')}>
                        {syncStatusLabels[s.lastSyncStatus] ?? s.lastSyncStatus}
                      </span>
                    ) : <span className="text-xs text-muted-foreground">—</span>}
                    {s.lastSyncError && <div className="max-w-xs truncate text-xs text-red-500" title={s.lastSyncError}>{s.lastSyncError}</div>}
                  </td>
                  <td className="px-4 py-3 text-slate-600">{s.mappingCount}</td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-1">
                      {s.accessType !== 'Upload' && (
                        <IconBtn title="Bağlantı testi" onClick={() => runTest(s)} disabled={testingId === s.id}>
                          {testingId === s.id ? <RefreshCw className="h-4 w-4 animate-spin" /> : <Plug className="h-4 w-4" />}
                        </IconBtn>
                      )}
                      <IconBtn title="Önizle & Eşleştir & Senkronize" onClick={() => setWorkbench(s)}>
                        <Eye className="h-4 w-4" />
                      </IconBtn>
                      <IconBtn title="Düzenle" onClick={() => openEdit(s)}>
                        <Pencil className="h-4 w-4" />
                      </IconBtn>
                      <IconBtn title="Sil" onClick={() => remove(s)} danger>
                        <Trash2 className="h-4 w-4" />
                      </IconBtn>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {showForm && <DataSourceFormModal source={editing} onClose={() => setShowForm(false)} />}
      {workbench && <DataSourceWorkbench source={workbench} onClose={() => setWorkbench(null)} />}
    </div>
  )
}

function IconBtn({
  children, title, onClick, disabled, danger,
}: {
  children: React.ReactNode; title: string; onClick: () => void; disabled?: boolean; danger?: boolean
}) {
  return (
    <button
      title={title}
      onClick={onClick}
      disabled={disabled}
      className={cn(
        'rounded-lg p-2 transition hover:bg-secondary disabled:opacity-50',
        danger ? 'text-red-500 hover:bg-red-50' : 'text-slate-600',
      )}
    >
      {children}
    </button>
  )
}
