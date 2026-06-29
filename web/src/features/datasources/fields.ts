// Kolon eşleştirme için sistem hedef alanları.
// value → backend OperationUpsertService'in tanıdığı kanonik alan adı (camelCase).
// operationNo upsert anahtarıdır → zorunludur.

export interface SystemField {
  value: string
  label: string
  required?: boolean
}

export const SYSTEM_FIELDS: SystemField[] = [
  { value: 'operationNo', label: 'Operasyon No (zorunlu)', required: true },
  { value: 'customerName', label: 'Müşteri' },
  { value: 'status', label: 'Durum' },
  { value: 'transportType', label: 'Taşıma Tipi' },
  { value: 'serviceType', label: 'Hizmet Tipi' },
  { value: 'tradeDirection', label: 'Yön (İthalat/İhracat)' },
  { value: 'financeStatus', label: 'Tahsilat Durumu' },
  { value: 'documentStatus', label: 'Evrak Durumu' },
  { value: 'eta', label: 'ETA (Varış)' },
  { value: 'etd', label: 'ETD (Çıkış)' },
  { value: 'loadingDate', label: 'Yükleme Tarihi' },
  { value: 'plannedDeliveryDate', label: 'Planlanan Teslim' },
  { value: 'deliveryDate', label: 'Teslim Tarihi' },
  { value: 'originCountry', label: 'Çıkış Ülkesi' },
  { value: 'originCity', label: 'Çıkış Şehri' },
  { value: 'destinationCountry', label: 'Varış Ülkesi' },
  { value: 'destinationCity', label: 'Varış Şehri' },
  { value: 'shipper', label: 'Gönderici' },
  { value: 'consignee', label: 'Alıcı' },
  { value: 'revenue', label: 'Gelir' },
  { value: 'cost', label: 'Maliyet' },
  { value: 'currency', label: 'Para Birimi' },
  { value: 'delayReason', label: 'Gecikme Nedeni' },
  { value: 'nextAction', label: 'Sonraki Aksiyon' },
  { value: 'blNo', label: 'BL No' },
  { value: 'containerNo', label: 'Konteyner No' },
  { value: 'containerType', label: 'Konteyner Tipi' },
  { value: 'vesselName', label: 'Gemi Adı' },
  { value: 'shippingLine', label: 'Acente / Hat' },
  { value: 'pol', label: 'Yükleme Limanı (POL)' },
  { value: 'pod', label: 'Varış Limanı (POD)' },
  { value: 'vehiclePlate', label: 'Plaka' },
  { value: 'driverName', label: 'Şoför' },
  { value: 'hawbNo', label: 'HAWB No' },
  { value: 'mawbNo', label: 'MAWB No' },
  { value: 'flightNo', label: 'Uçuş No' },
  { value: 'airline', label: 'Havayolu' },
]

export const sourceTypeOptions = [
  { value: 'YandexDiskExcel', label: 'Yandex Disk Public Excel', accessType: 'Public' },
  { value: 'SharePointExcel', label: 'SharePoint Public Excel', accessType: 'Public' },
  { value: 'ManualExcel', label: 'Manuel Excel Upload', accessType: 'Upload' },
]

export const sourceTypeLabels: Record<string, string> = {
  YandexDiskExcel: 'Yandex Disk',
  SharePointExcel: 'SharePoint',
  ManualExcel: 'Manuel Excel',
  OneDriveExcel: 'OneDrive',
  GoogleSheets: 'Google Sheets',
  Api: 'API',
  Csv: 'CSV',
}

export const syncStatusLabels: Record<string, string> = {
  Running: 'Çalışıyor',
  Success: 'Başarılı',
  PartialSuccess: 'Kısmi Başarılı',
  Failed: 'Başarısız',
}

export const syncStatusClasses: Record<string, string> = {
  Running: 'bg-blue-100 text-blue-700',
  Success: 'bg-emerald-100 text-emerald-700',
  PartialSuccess: 'bg-amber-100 text-amber-700',
  Failed: 'bg-red-100 text-red-700',
}
