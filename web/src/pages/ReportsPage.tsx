import { useState } from 'react'
import { FileSpreadsheet, Download, ShieldAlert, Ship, Truck, Plane, Wallet, LayoutGrid } from 'lucide-react'
import type { ReactNode } from 'react'
import { Card } from '@/components/ui/Card'
import { api } from '@/services/api'

async function download(path: string, filename: string) {
  const res = await api.get(path, { responseType: 'blob' })
  const url = URL.createObjectURL(res.data as Blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

interface ReportDef {
  key: string
  title: string
  description: string
  icon: ReactNode
  path: string
  filename: string
}

const reports: ReportDef[] = [
  {
    key: 'all',
    title: 'Tüm Sekmeler',
    description: 'Deniz, Kara, Hava ve Finans — 9 sekmenin tamamı, sekme-başına bir Excel sayfası.',
    icon: <LayoutGrid className="h-6 w-6" />,
    path: '/reports/boards/excel',
    filename: 'operasyonlar_tum_sekmeler.xlsx',
  },
  {
    key: 'deniz',
    title: 'Deniz Operasyonları',
    description: 'Denizyolu Transit, İthalat, İhracat, Karayolu Transit (4 sekme).',
    icon: <Ship className="h-6 w-6" />,
    path: '/reports/boards/excel?group=Deniz',
    filename: 'operasyonlar_deniz.xlsx',
  },
  {
    key: 'kara',
    title: 'Karayolu Operasyonları',
    description: 'Yoldaki Yükler + Arşiv (Muratbey/Kerry/Mirlog).',
    icon: <Truck className="h-6 w-6" />,
    path: '/reports/boards/excel?group=Kara',
    filename: 'operasyonlar_kara.xlsx',
  },
  {
    key: 'hava',
    title: 'Hava Operasyonları',
    description: 'Operasyon Bilgileri + Günlük Liste.',
    icon: <Plane className="h-6 w-6" />,
    path: '/reports/boards/excel?group=Hava',
    filename: 'operasyonlar_hava.xlsx',
  },
  {
    key: 'finans',
    title: 'Finans (Alabora Tahsilat)',
    description: 'Alabora (СЧЕТА-ПЛАТЕЖИ) tahsilat takip tablosu.',
    icon: <Wallet className="h-6 w-6" />,
    path: '/reports/boards/excel?group=Finans',
    filename: 'operasyonlar_finans.xlsx',
  },
  {
    key: 'alerts',
    title: 'Açık Uyarılar',
    description: 'Risk motorunun ürettiği tüm açık uyarılar (gecikme, risk, tahsilat, belge).',
    icon: <ShieldAlert className="h-6 w-6" />,
    path: '/reports/alerts/excel',
    filename: 'acik_uyarilar.xlsx',
  },
]

export function ReportsPage() {
  const [busy, setBusy] = useState<string | null>(null)

  const run = async (r: ReportDef) => {
    setBusy(r.key)
    try {
      await download(r.path, r.filename)
    } finally {
      setBusy(null)
    }
  }

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Raporlar</h1>
        <p className="text-sm text-muted-foreground">Takip tablolarından (gerçek operasyon verisi) Excel raporları</p>
      </div>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        {reports.map((r) => (
          <Card key={r.key} className="flex flex-col gap-3 p-5">
            <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary/10 text-primary">
              {r.icon}
            </div>
            <div>
              <div className="font-semibold text-slate-900">{r.title}</div>
              <p className="text-sm text-muted-foreground">{r.description}</p>
            </div>
            <button
              onClick={() => run(r)}
              disabled={busy === r.key}
              className="mt-auto flex items-center justify-center gap-2 rounded-lg bg-primary py-2 text-sm font-semibold text-white transition hover:bg-primary/90 disabled:opacity-60"
            >
              {busy === r.key ? <FileSpreadsheet className="h-4 w-4 animate-pulse" /> : <Download className="h-4 w-4" />}
              {busy === r.key ? 'Hazırlanıyor...' : 'Excel İndir'}
            </button>
          </Card>
        ))}
      </div>
    </div>
  )
}
