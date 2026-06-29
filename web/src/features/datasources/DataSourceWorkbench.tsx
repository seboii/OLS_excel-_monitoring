import { useMemo, useState } from 'react'
import { X, Eye, Save, RefreshCw, Upload, CheckCircle2, AlertTriangle, FileSpreadsheet } from 'lucide-react'
import { cn, formatDateTime } from '@/lib/utils'
import {
  apiErrorMessage,
  useColumnMappings,
  useDownloadPreview,
  useManualUploadImport,
  useManualUploadPreview,
  useSaveColumnMappings,
  useSyncDataSource,
  useSyncLogs,
  type DataSource,
  type ImportPreview,
  type SyncResult,
} from './api'
import { SYSTEM_FIELDS, syncStatusClasses, syncStatusLabels } from './fields'

interface Props {
  source: DataSource
  onClose: () => void
}

export function DataSourceWorkbench({ source, onClose }: Props) {
  const isUpload = source.accessType === 'Upload'

  const savedMappings = useColumnMappings(source.id)
  const logs = useSyncLogs(source.id)
  const downloadPreview = useDownloadPreview()
  const manualPreview = useManualUploadPreview()
  const saveMappings = useSaveColumnMappings()
  const sync = useSyncDataSource()
  const manualImport = useManualUploadImport()

  const [preview, setPreview] = useState<ImportPreview | null>(null)
  const [sheet, setSheet] = useState<string | undefined>(undefined)
  const [mapping, setMapping] = useState<Record<string, string>>({})
  const [file, setFile] = useState<File | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [result, setResult] = useState<SyncResult | null>(null)

  const previewBusy = downloadPreview.isPending || manualPreview.isPending

  /** Önizleme geldikten sonra eşleştirme tablosunu kurar (kayıtlı eşleştirme > otomatik tahmin). */
  const applyPreview = (p: ImportPreview) => {
    setPreview(p)
    setSheet(p.sheet.sheetName)
    const saved = new Map((savedMappings.data ?? []).map((m) => [m.sourceColumn, m.targetField]))
    const next: Record<string, string> = {}
    for (const col of p.sheet.columns) {
      const fromSaved = saved.get(col.name)
      const fromSuggestion = p.suggestions.find((s) => s.sourceColumn === col.name)?.suggestedTargetField
      next[col.name] = fromSaved ?? fromSuggestion ?? ''
    }
    setMapping(next)
  }

  const loadPreview = async (sheetName?: string) => {
    setError(null)
    setResult(null)
    try {
      if (isUpload) {
        if (!file) {
          setError('Önce bir Excel dosyası seçin.')
          return
        }
        applyPreview(await manualPreview.mutateAsync({ file, sheetName }))
      } else {
        applyPreview(await downloadPreview.mutateAsync({ id: source.id, sheetName }))
      }
    } catch (err) {
      setError(apiErrorMessage(err, 'Önizleme alınamadı.'))
    }
  }

  const operationNoMapped = useMemo(
    () => Object.values(mapping).includes('operationNo'),
    [mapping],
  )

  const persistMappings = async () => {
    setError(null)
    const inputs = Object.entries(mapping)
      .filter(([, target]) => target)
      .map(([sourceColumn, targetField]) => ({
        sourceColumn,
        sourceColumnIndex: preview?.sheet.columns.find((c) => c.name === sourceColumn)?.index,
        targetField,
        isRequired: targetField === 'operationNo',
      }))
    try {
      await saveMappings.mutateAsync({ id: source.id, mappings: inputs })
    } catch (err) {
      setError(apiErrorMessage(err, 'Eşleştirmeler kaydedilemedi.'))
    }
  }

  const runSync = async () => {
    setError(null)
    setResult(null)
    if (!operationNoMapped) {
      setError('Zorunlu kolon eşleştirilmemiş: "Operasyon No" bir kaynak kolona eşlenmeli.')
      return
    }
    try {
      await persistMappings()
      const res = isUpload
        ? await manualImport.mutateAsync({ dataSourceId: source.id, file: file!, sheetName: sheet })
        : await sync.mutateAsync(source.id)
      setResult(res)
      logs.refetch()
    } catch (err) {
      setError(apiErrorMessage(err, 'Senkronizasyon sırasında hata oluştu.'))
    }
  }

  const syncBusy = sync.isPending || manualImport.isPending || saveMappings.isPending

  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-black/40" onClick={onClose}>
      <div
        className="flex h-full w-full max-w-5xl flex-col bg-slate-50 shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Başlık */}
        <div className="flex items-center justify-between border-b bg-white px-6 py-4">
          <div>
            <h2 className="text-lg font-semibold text-slate-900">{source.name}</h2>
            <p className="text-xs text-muted-foreground">
              Önizleme · kolon eşleştirme · senkronizasyon · loglar
            </p>
          </div>
          <button onClick={onClose} className="rounded-lg p-1.5 hover:bg-secondary">
            <X className="h-5 w-5 text-slate-500" />
          </button>
        </div>

        <div className="flex-1 space-y-5 overflow-y-auto p-6">
          {/* 1) Önizleme al */}
          <section className="rounded-xl border bg-white p-4">
            <h3 className="mb-3 text-sm font-semibold text-slate-800">1 · Excel Önizleme</h3>

            {isUpload && (
              <input
                type="file"
                accept=".xlsx,.xls,.csv"
                onChange={(e) => setFile(e.target.files?.[0] ?? null)}
                className="mb-3 block w-full text-sm text-slate-600 file:mr-3 file:rounded-lg file:border-0 file:bg-primary/10 file:px-3 file:py-2 file:text-sm file:font-medium file:text-primary"
              />
            )}

            <div className="flex flex-wrap items-center gap-2">
              <button
                onClick={() => loadPreview(sheet)}
                disabled={previewBusy}
                className="flex items-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-white hover:bg-primary/90 disabled:opacity-60"
              >
                <Eye className={cn('h-4 w-4', previewBusy && 'animate-pulse')} />
                {previewBusy ? 'İndiriliyor...' : 'Önizleme Al'}
              </button>

              {preview && preview.sheetNames.length > 1 && (
                <select
                  value={sheet}
                  onChange={(e) => {
                    setSheet(e.target.value)
                    loadPreview(e.target.value)
                  }}
                  className="rounded-lg border bg-white px-3 py-2 text-sm outline-none focus:border-primary"
                >
                  {preview.sheetNames.map((s) => (
                    <option key={s} value={s}>{s}</option>
                  ))}
                </select>
              )}

              {preview && (
                <span className="text-xs text-muted-foreground">
                  {preview.sheet.columns.length} kolon · {preview.sheet.totalDataRows} satır
                  (ilk {preview.sheet.rows.length} gösteriliyor)
                </span>
              )}
            </div>

            {preview && <PreviewTable preview={preview} />}
          </section>

          {/* 2) Kolon eşleştirme */}
          {preview && (
            <section className="rounded-xl border bg-white p-4">
              <div className="mb-3 flex items-center justify-between">
                <h3 className="text-sm font-semibold text-slate-800">2 · Kolon Eşleştirme</h3>
                <button
                  onClick={persistMappings}
                  disabled={saveMappings.isPending}
                  className="flex items-center gap-2 rounded-lg border px-3 py-1.5 text-sm font-medium text-slate-600 hover:bg-secondary disabled:opacity-60"
                >
                  <Save className="h-4 w-4" />
                  Eşleştirmeleri Kaydet
                </button>
              </div>

              {!operationNoMapped && (
                <p className="mb-3 flex items-center gap-2 rounded-lg bg-amber-50 px-3 py-2 text-xs text-amber-700">
                  <AlertTriangle className="h-4 w-4" />
                  Zorunlu alan "Operasyon No" henüz bir kaynak kolona eşlenmedi. Senkronizasyon için gereklidir.
                </p>
              )}

              <div className="grid gap-2 sm:grid-cols-2">
                {preview.sheet.columns.map((col) => (
                  <div key={col.name} className="flex items-center gap-2 rounded-lg border bg-slate-50/50 px-3 py-2">
                    <span className="flex-1 truncate text-sm font-medium text-slate-700" title={col.name}>
                      {col.name}
                    </span>
                    <span className="text-slate-300">→</span>
                    <select
                      value={mapping[col.name] ?? ''}
                      onChange={(e) => setMapping((m) => ({ ...m, [col.name]: e.target.value }))}
                      className={cn(
                        'w-44 rounded-lg border bg-white px-2 py-1.5 text-sm outline-none focus:border-primary',
                        mapping[col.name] === 'operationNo' && 'border-primary font-medium text-primary',
                      )}
                    >
                      <option value="">— Eşleştirme yok —</option>
                      {SYSTEM_FIELDS.map((f) => (
                        <option key={f.value} value={f.value}>{f.label}</option>
                      ))}
                    </select>
                  </div>
                ))}
              </div>
            </section>
          )}

          {/* 3) Senkronize / içe aktar */}
          {preview && (
            <section className="rounded-xl border bg-white p-4">
              <h3 className="mb-3 text-sm font-semibold text-slate-800">3 · Operations'a Aktar</h3>
              <button
                onClick={runSync}
                disabled={syncBusy}
                className="flex items-center gap-2 rounded-lg bg-emerald-600 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-700 disabled:opacity-60"
              >
                {isUpload ? <Upload className="h-4 w-4" /> : <RefreshCw className={cn('h-4 w-4', syncBusy && 'animate-spin')} />}
                {syncBusy ? 'Aktarılıyor...' : isUpload ? 'Dosyayı İçe Aktar' : 'Senkronize Et'}
              </button>

              {result && (
                <div className="mt-3 rounded-lg bg-emerald-50 px-3 py-2 text-sm text-emerald-800">
                  <div className="flex items-center gap-2 font-medium">
                    <CheckCircle2 className="h-4 w-4" />
                    {result.rowsUpserted} satır aktarıldı, {result.rowsFailed} satır hatalı
                    (toplam {result.rowsRead}).
                  </div>
                  {result.errors.length > 0 && (
                    <ul className="mt-1 list-disc pl-6 text-xs text-amber-700">
                      {result.errors.slice(0, 5).map((e, i) => <li key={i}>{e}</li>)}
                    </ul>
                  )}
                </div>
              )}
            </section>
          )}

          {error && (
            <p className="flex items-center gap-2 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700">
              <AlertTriangle className="h-4 w-4 shrink-0" />
              {error}
            </p>
          )}

          {/* 4) Sync logları */}
          <section className="rounded-xl border bg-white p-4">
            <h3 className="mb-3 text-sm font-semibold text-slate-800">4 · Senkronizasyon Logları</h3>
            {logs.isLoading ? (
              <p className="text-sm text-muted-foreground">Yükleniyor...</p>
            ) : (logs.data?.length ?? 0) === 0 ? (
              <p className="text-sm text-muted-foreground">Henüz senkronizasyon kaydı yok.</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead className="border-b text-xs uppercase text-muted-foreground">
                    <tr>
                      <th className="py-2 pr-3">Tarih</th>
                      <th className="py-2 pr-3">Durum</th>
                      <th className="py-2 pr-3">Okunan</th>
                      <th className="py-2 pr-3">Aktarılan</th>
                      <th className="py-2 pr-3">Hatalı</th>
                      <th className="py-2 pr-3">Süre</th>
                      <th className="py-2">Mesaj</th>
                    </tr>
                  </thead>
                  <tbody>
                    {logs.data!.map((l) => (
                      <tr key={l.id} className="border-b last:border-0">
                        <td className="py-2 pr-3 whitespace-nowrap">{formatDateTime(l.startedAt)}</td>
                        <td className="py-2 pr-3">
                          <span className={cn('rounded-full px-2 py-0.5 text-xs font-medium', syncStatusClasses[l.status] ?? 'bg-slate-100 text-slate-600')}>
                            {syncStatusLabels[l.status] ?? l.status}
                          </span>
                        </td>
                        <td className="py-2 pr-3">{l.rowsRead}</td>
                        <td className="py-2 pr-3 text-emerald-700">{l.rowsUpserted}</td>
                        <td className="py-2 pr-3 text-red-600">{l.rowsFailed}</td>
                        <td className="py-2 pr-3 whitespace-nowrap">{l.durationMs != null ? `${l.durationMs} ms` : '—'}</td>
                        <td className="py-2 max-w-xs truncate text-xs text-muted-foreground" title={l.message ?? ''}>{l.message ?? '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </div>
      </div>
    </div>
  )
}

function PreviewTable({ preview }: { preview: ImportPreview }) {
  const cols = preview.sheet.columns
  return (
    <div className="mt-3 overflow-x-auto rounded-lg border">
      <table className="w-full text-left text-xs">
        <thead className="bg-slate-100 text-slate-600">
          <tr>
            {cols.map((c) => (
              <th key={c.name} className="whitespace-nowrap px-3 py-2 font-semibold">{c.name}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {preview.sheet.rows.map((row, i) => (
            <tr key={i} className="border-t">
              {cols.map((c) => (
                <td key={c.name} className="max-w-[200px] truncate px-3 py-1.5 text-slate-600" title={row[c.name] ?? ''}>
                  {row[c.name] ?? ''}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
      {preview.sheet.rows.length === 0 && (
        <div className="flex items-center gap-2 px-3 py-4 text-sm text-muted-foreground">
          <FileSpreadsheet className="h-4 w-4" /> Bu sayfada gösterilecek satır bulunamadı.
        </div>
      )}
    </div>
  )
}
