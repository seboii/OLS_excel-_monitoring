import { useEffect } from 'react'
import type { ReactNode } from 'react'
import type { LucideIcon } from 'lucide-react'
import { X, MapPin, CalendarDays, UserCog, Wallet, FileText, AlertCircle, Package } from 'lucide-react'
import { useOperation } from '@/features/operations/api'
import type { OperationDetail } from '@/services/types'
import { StatusBadge } from './StatusBadge'
import { RiskBadge } from './RiskBadge'
import { CommentPanel } from './CommentPanel'
import {
  transportTypeLabels, serviceTypeLabels, tradeDirectionLabels,
  financeStatusLabels, documentStatusLabels, delayReasonLabels, tr,
} from '@/lib/labels'
import { formatDate, formatDateTime, formatMoney } from '@/lib/utils'

function Row({ label, value }: { label: string; value?: ReactNode }) {
  return (
    <div className="flex justify-between gap-3 py-1.5 text-sm">
      <span className="shrink-0 text-muted-foreground">{label}</span>
      <span className="text-right font-medium text-slate-800">{value ?? '—'}</span>
    </div>
  )
}

function Section({ icon: Icon, title, children }: { icon: LucideIcon; title: string; children: ReactNode }) {
  return (
    <div className="border-t px-5 py-4">
      <div className="mb-1 flex items-center gap-2 text-sm font-semibold text-slate-900">
        <Icon className="h-4 w-4 text-primary" /> {title}
      </div>
      <div className="divide-y divide-slate-100">{children}</div>
    </div>
  )
}

export function OperationDetailDrawer({
  operationId,
  onClose,
}: {
  operationId: number | null
  onClose: () => void
}) {
  const { data: op, isLoading } = useOperation(operationId)

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])

  if (operationId == null) return null

  return (
    <div className="fixed inset-0 z-50">
      <div className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" onClick={onClose} />
      <div className="absolute right-0 top-0 h-full w-full max-w-lg overflow-y-auto bg-background shadow-2xl">
        {isLoading || !op ? (
          <div className="p-10 text-center text-muted-foreground">Yükleniyor...</div>
        ) : (
          <DrawerBody op={op} onClose={onClose} />
        )}
      </div>
    </div>
  )
}

function DrawerBody({ op, onClose }: { op: OperationDetail; onClose: () => void }) {
  const d = op.detail
  return (
    <div>
      <div className="sticky top-0 z-10 flex items-start justify-between gap-3 border-b bg-white px-5 py-4">
        <div className="min-w-0">
          <div className="text-lg font-bold text-slate-900">{op.operationNo ?? `#${op.id}`}</div>
          <div className="truncate text-sm text-muted-foreground">{op.customerName}</div>
          <div className="mt-2 flex flex-wrap items-center gap-2">
            <StatusBadge status={op.status} />
            <RiskBadge risk={op.riskLevel} />
          </div>
          <div className="mt-1 text-xs text-muted-foreground">
            {tr(transportTypeLabels, op.transportType)} · {tr(serviceTypeLabels, op.serviceType)} · {tr(tradeDirectionLabels, op.tradeDirection)}
          </div>
        </div>
        <button onClick={onClose} className="rounded-lg p-2 transition hover:bg-secondary" title="Kapat">
          <X className="h-5 w-5 text-slate-600" />
        </button>
      </div>

      <Section icon={MapPin} title="Rota ve Taraflar">
        <Row label="Yükleme" value={`${op.originCity ?? op.originCountry ?? '—'}`} />
        <Row label="Teslim" value={`${op.destinationCity ?? op.destinationCountry ?? '—'}`} />
        <Row label="Gönderici" value={op.shipper} />
        <Row label="Alıcı" value={op.consignee} />
      </Section>

      <Section icon={CalendarDays} title="Tarihler">
        <Row label="Yükleme Tarihi" value={formatDate(op.loadingDate)} />
        <Row label="ETD" value={formatDateTime(op.etd)} />
        <Row label="ETA" value={formatDateTime(op.eta)} />
        <Row label="Gerçekleşen Varış" value={formatDateTime(op.actualArrivalDate)} />
        <Row label="Planlanan Teslim" value={formatDate(op.plannedDeliveryDate)} />
        <Row label="Gerçekleşen Teslim" value={formatDate(op.deliveryDate)} />
      </Section>

      <Section icon={UserCog} title="Sorumluluk">
        <Row label="Sorumlu" value={op.responsibleUserName} />
        <Row label="Satış Sahibi" value={op.salesOwnerName} />
        <Row label="Departman" value={op.departmentName} />
      </Section>

      <Section icon={Wallet} title="Finans">
        <Row label="Finans Durumu" value={tr(financeStatusLabels, op.financeStatus)} />
        <Row label="Gelir" value={formatMoney(op.revenueAmount, op.currency)} />
        <Row label="Maliyet" value={formatMoney(op.costAmount, op.currency)} />
        <Row
          label="Brüt Kâr"
          value={
            <span className={op.grossProfit != null && op.grossProfit < 0 ? 'text-red-600' : 'text-emerald-600'}>
              {formatMoney(op.grossProfit, op.currency)}
            </span>
          }
        />
      </Section>

      <Section icon={AlertCircle} title="Gecikme ve Sonraki Aksiyon">
        <Row
          label="Gecikme"
          value={op.delayDays > 0 ? <span className="text-red-600">{op.delayDays} gün</span> : '—'}
        />
        <Row label="Gecikme Sebebi" value={tr(delayReasonLabels, op.delayReason)} />
        <Row label="Evrak Durumu" value={tr(documentStatusLabels, op.documentStatus)} />
        <Row label="Sonraki Aksiyon" value={op.nextActionDescription} />
        <Row label="Aksiyon Tarihi" value={formatDate(op.nextActionDate)} />
      </Section>

      {d && (
        <Section icon={op.transportType === 'Air' ? Package : FileText} title="Taşıma Detayı">
          {op.transportType === 'Sea' && (
            <>
              <Row label="BL No" value={d.blNo} />
              <Row label="Konteyner" value={d.containerNo} />
              <Row label="Konteyner Tipi" value={d.containerType} />
              <Row label="Hat" value={d.shippingLine} />
              <Row label="Gemi" value={d.vesselName} />
              <Row label="POL → POD" value={`${d.pol ?? '—'} → ${d.pod ?? '—'}`} />
              <Row label="Ordino" value={d.ordinoStatus} />
              <Row label="Free Time Bitiş" value={formatDate(d.freeTimeEndDate)} />
              <Row label="Demuraj Başlangıç" value={formatDate(d.demurrageStartDate)} />
            </>
          )}
          {op.transportType === 'Air' && (
            <>
              <Row label="HAWB" value={d.hawbNo} />
              <Row label="MAWB" value={d.mawbNo} />
              <Row label="Havayolu" value={d.airline} />
              <Row label="Uçuş No" value={d.flightNo} />
              <Row label="Güzergah" value={`${d.departureAirport ?? '—'} → ${d.arrivalAirport ?? '—'}`} />
              <Row label="Kap" value={d.pieces} />
              <Row label="Brüt Kg" value={d.grossWeightKg} />
            </>
          )}
          {op.transportType === 'Road' && (
            <>
              <Row label="Araç Plakası" value={d.vehiclePlate} />
              <Row label="Sürücü" value={d.driverName} />
              <Row label="Sınır Kapısı" value={d.borderCrossing} />
              <Row label="Doluluk" value={d.fillRate != null ? `%${Math.round(d.fillRate * 100)}` : undefined} />
            </>
          )}
        </Section>
      )}

      <CommentPanel subject={{ operationId: op.id }} />
    </div>
  )
}
