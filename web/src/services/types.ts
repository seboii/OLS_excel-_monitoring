// Backend DTO'larıyla birebir eşleşen TypeScript tipleri.

export interface ApiError {
  code: string
  message: string
  field?: string
}

export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  errors?: ApiError[]
  traceId?: string
  timestamp?: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

export interface DashboardKpis {
  totalActive: number
  todayLoading: number
  todayDelivery: number
  delayed: number
  risky: number
  missingDocuments: number
  completed: number
  new24h: number
  avgDelayDays: number
  criticalCustomerOps: number
  totalOperations: number
}

export interface NameValue {
  name: string
  value: number
}

export interface RecentActivity {
  operationId: number
  operationNo?: string
  customerName: string
  status: string
  at: string
}

export interface DashboardSummary {
  kpis: DashboardKpis
  statusDistribution: NameValue[]
  transportDistribution: NameValue[]
  riskDistribution: NameValue[]
  departmentLoad: NameValue[]
  recent: RecentActivity[]
}

export interface OperationListItem {
  id: number
  operationNo?: string
  transportType: string
  serviceType: string
  customerName: string
  originCountry?: string
  originCity?: string
  destinationCountry?: string
  destinationCity?: string
  status: string
  riskLevel: string
  financeStatus: string
  documentStatus: string
  delayDays: number
  eta?: string
  responsibleUserName?: string
  departmentName?: string
  revenueAmount?: number
  grossProfit?: number
  currency: string
}

export interface OperationDetailInfo {
  blNo?: string
  containerNo?: string
  containerType?: string
  shippingLine?: string
  vesselName?: string
  pol?: string
  pod?: string
  transshipmentPort?: string
  ordinoStatus?: string
  freeTimeEndDate?: string
  demurrageStartDate?: string
  hawbNo?: string
  mawbNo?: string
  airline?: string
  flightNo?: string
  departureAirport?: string
  arrivalAirport?: string
  pieces?: number
  grossWeightKg?: number
  vehiclePlate?: string
  driverName?: string
  borderCrossing?: string
  fillRate?: number
}

export interface OperationDetail {
  id: number
  operationNo?: string
  transportType: string
  serviceType: string
  tradeDirection: string
  customerName: string
  shipper?: string
  consignee?: string
  originCountry?: string
  originCity?: string
  destinationCountry?: string
  destinationCity?: string
  loadingDate?: string
  etd?: string
  eta?: string
  actualArrivalDate?: string
  plannedDeliveryDate?: string
  deliveryDate?: string
  status: string
  riskLevel: string
  financeStatus: string
  documentStatus: string
  responsibleUserName?: string
  salesOwnerName?: string
  departmentName?: string
  nextActionDate?: string
  nextActionDescription?: string
  delayDays: number
  delayReason: string
  revenueAmount?: number
  costAmount?: number
  grossProfit?: number
  currency: string
  detail?: OperationDetailInfo
}
