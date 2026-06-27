// Backend enum (string) → Türkçe görünen ad + renk eşleştirmeleri.

export const operationStatusLabels: Record<string, string> = {
  New: 'Yeni',
  Preparing: 'Hazırlıkta',
  AwaitingLoading: 'Yükleme Bekliyor',
  InTransit: 'Yolda',
  AtPort: 'Limanda',
  InCustoms: 'Gümrükte',
  InWarehouse: 'Depoda',
  OutForDelivery: 'Teslimatta',
  Completed: 'Tamamlandı',
  OnHold: 'Beklemede',
  Delayed: 'Gecikmiş',
  FinancialHold: 'Finansal Hold',
  MissingDocuments: 'Evrak Eksik',
  Cancelled: 'İptal',
}

export const transportTypeLabels: Record<string, string> = {
  Road: 'Karayolu',
  Sea: 'Deniz',
  Air: 'Hava',
  Customs: 'Gümrük',
  Other: 'Diğer',
}

export const serviceTypeLabels: Record<string, string> = {
  Ltl: 'LTL',
  Ftl: 'FTL',
  Fcl: 'FCL',
  Lcl: 'LCL',
  AirCargo: 'Hava Kargo',
  Customs: 'Gümrük',
  Parcel: 'Parsiyel',
  Other: 'Diğer',
}

export const riskLevelLabels: Record<string, string> = {
  Green: 'Normal',
  Yellow: 'Takip',
  Orange: 'Risk',
  Red: 'Kritik',
  Black: 'Acil Müdahale',
}

export const financeStatusLabels: Record<string, string> = {
  Collected: 'Tahsil Edildi',
  Pending: 'Bekleniyor',
  PartiallyCollected: 'Kısmi Tahsil',
  Overdue: 'Vadesi Geçti',
  AwaitingCustomerApproval: 'Müşteri Onayı',
  UnderBankReview: 'Banka Kontrolünde',
  FinancialHold: 'Finansal Hold',
  Cancelled: 'İptal',
}

export const documentStatusLabels: Record<string, string> = {
  Complete: 'Tamam',
  Missing: 'Eksik',
  Pending: 'Bekliyor',
}

export const tradeDirectionLabels: Record<string, string> = {
  None: '—',
  Import: 'İthalat',
  Export: 'İhracat',
  Transit: 'Transit',
}

/** Risk seviyesi → Tailwind sınıfları (rozet için). */
export const riskLevelClasses: Record<string, string> = {
  Green: 'bg-emerald-100 text-emerald-700 ring-emerald-600/20',
  Yellow: 'bg-amber-100 text-amber-700 ring-amber-600/20',
  Orange: 'bg-orange-100 text-orange-700 ring-orange-600/20',
  Red: 'bg-red-100 text-red-700 ring-red-600/20',
  Black: 'bg-slate-800 text-slate-100 ring-slate-700',
}

/** Operasyon statüsü → Tailwind sınıfları (rozet için). */
export const statusClasses: Record<string, string> = {
  New: 'bg-slate-100 text-slate-700',
  Preparing: 'bg-slate-100 text-slate-700',
  AwaitingLoading: 'bg-blue-100 text-blue-700',
  InTransit: 'bg-blue-100 text-blue-700',
  AtPort: 'bg-cyan-100 text-cyan-700',
  InCustoms: 'bg-violet-100 text-violet-700',
  InWarehouse: 'bg-cyan-100 text-cyan-700',
  OutForDelivery: 'bg-indigo-100 text-indigo-700',
  Completed: 'bg-emerald-100 text-emerald-700',
  OnHold: 'bg-amber-100 text-amber-700',
  Delayed: 'bg-red-100 text-red-700',
  FinancialHold: 'bg-orange-100 text-orange-700',
  MissingDocuments: 'bg-orange-100 text-orange-700',
  Cancelled: 'bg-slate-200 text-slate-500',
}

export const tr = (map: Record<string, string>, key?: string | null) =>
  key ? (map[key] ?? key) : '—'
