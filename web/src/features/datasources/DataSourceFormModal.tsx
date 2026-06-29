import { useState } from 'react'
import { X } from 'lucide-react'
import { transportTypeLabels } from '@/lib/labels'
import {
  apiErrorMessage,
  useCreateDataSource,
  useUpdateDataSource,
  type DataSource,
} from './api'
import { sourceTypeOptions } from './fields'

interface Props {
  source?: DataSource | null
  onClose: () => void
}

const transportOptions = ['Road', 'Sea', 'Air', 'Customs', 'Other']

export function DataSourceFormModal({ source, onClose }: Props) {
  const isEdit = !!source
  const create = useCreateDataSource()
  const update = useUpdateDataSource()

  const [name, setName] = useState(source?.name ?? '')
  const [type, setType] = useState(source?.type ?? 'YandexDiskExcel')
  const [url, setUrl] = useState(source?.url ?? '')
  const [transport, setTransport] = useState(source?.defaultTransportType ?? '')
  const [sheetName, setSheetName] = useState(source?.sheetName ?? '')
  const [headerRowIndex, setHeaderRowIndex] = useState(source?.headerRowIndex ?? 1)
  const [syncInterval, setSyncInterval] = useState(source?.syncIntervalMinutes ?? 15)
  const [isActive, setIsActive] = useState(source?.isActive ?? true)
  const [error, setError] = useState<string | null>(null)

  const accessType = sourceTypeOptions.find((o) => o.value === type)?.accessType ?? 'Public'
  const isUpload = accessType === 'Upload'
  const busy = create.isPending || update.isPending

  const submit = async () => {
    setError(null)
    if (!name.trim()) {
      setError('Kaynak adı zorunludur.')
      return
    }
    if (!isUpload && !url.trim()) {
      setError('Link boş olamaz.')
      return
    }
    const payload = {
      name: name.trim(),
      type,
      accessType,
      url: isUpload ? null : url.trim(),
      defaultTransportType: transport || null,
      sheetName: sheetName.trim() || null,
      headerRowIndex: Number(headerRowIndex) || 1,
      syncIntervalMinutes: Number(syncInterval) || 15,
    }
    try {
      if (isEdit && source) {
        await update.mutateAsync({ id: source.id, input: { ...payload, isActive } })
      } else {
        await create.mutateAsync(payload)
      }
      onClose()
    } catch (err) {
      setError(apiErrorMessage(err, 'Kaydedilemedi.'))
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div
        className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-xl bg-white shadow-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b px-5 py-4">
          <h2 className="text-lg font-semibold text-slate-900">
            {isEdit ? 'Veri Kaynağını Düzenle' : 'Yeni Veri Kaynağı'}
          </h2>
          <button onClick={onClose} className="rounded-lg p-1.5 hover:bg-secondary">
            <X className="h-5 w-5 text-slate-500" />
          </button>
        </div>

        <div className="space-y-4 px-5 py-4">
          <Field label="Kaynak Adı">
            <input value={name} onChange={(e) => setName(e.target.value)} className={inputCls} placeholder="Örn. LTL_FTL (Yandex)" />
          </Field>

          <Field label="Kaynak Tipi">
            <select value={type} onChange={(e) => setType(e.target.value)} className={inputCls} disabled={isEdit}>
              {sourceTypeOptions.map((o) => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
          </Field>

          {!isUpload && (
            <Field label="Public Link">
              <input value={url} onChange={(e) => setUrl(e.target.value)} className={inputCls} placeholder="https://disk.yandex.ru/i/..." />
              <p className="mt-1 text-xs text-muted-foreground">
                Link backend üzerinden indirilir; tarayıcıdan dış servise istek gitmez.
              </p>
            </Field>
          )}
          {isUpload && (
            <p className="rounded-lg bg-blue-50 px-3 py-2 text-xs text-blue-700">
              Manuel Excel kaynağı: dosyayı kaydettikten sonra önizleme ekranından yükleyip aktarabilirsiniz.
            </p>
          )}

          <div className="grid grid-cols-2 gap-3">
            <Field label="Varsayılan Taşıma Tipi">
              <select value={transport} onChange={(e) => setTransport(e.target.value)} className={inputCls}>
                <option value="">Otomatik / Yok</option>
                {transportOptions.map((t) => (
                  <option key={t} value={t}>{transportTypeLabels[t] ?? t}</option>
                ))}
              </select>
            </Field>
            <Field label="Sheet Adı (boş = ilk)">
              <input value={sheetName} onChange={(e) => setSheetName(e.target.value)} className={inputCls} placeholder="Sayfa1" />
            </Field>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Field label="Başlık Satırı (1 = otomatik)">
              <input type="number" min={1} value={headerRowIndex} onChange={(e) => setHeaderRowIndex(+e.target.value)} className={inputCls} />
            </Field>
            <Field label="Senkron Aralığı (dk)">
              <input type="number" min={1} value={syncInterval} onChange={(e) => setSyncInterval(+e.target.value)} className={inputCls} />
            </Field>
          </div>

          {isEdit && (
            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} className="h-4 w-4" />
              Aktif (periyodik otomatik senkronizasyona dahil)
            </label>
          )}

          {error && <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}
        </div>

        <div className="flex justify-end gap-2 border-t px-5 py-4">
          <button onClick={onClose} className="rounded-lg border px-4 py-2 text-sm font-medium text-slate-600 hover:bg-secondary">
            Vazgeç
          </button>
          <button onClick={submit} disabled={busy} className="rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-white hover:bg-primary/90 disabled:opacity-60">
            {busy ? 'Kaydediliyor...' : isEdit ? 'Güncelle' : 'Oluştur'}
          </button>
        </div>
      </div>
    </div>
  )
}

const inputCls =
  'w-full rounded-lg border bg-white px-3 py-2 text-sm outline-none transition focus:border-primary'

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="mb-1 block text-sm font-medium text-slate-700">{label}</label>
      {children}
    </div>
  )
}
