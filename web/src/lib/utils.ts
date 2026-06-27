import { clsx, type ClassValue } from 'clsx'
import { twMerge } from 'tailwind-merge'

/** Tailwind sınıflarını koşullu birleştirir ve çakışmaları çözer. */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

/** Para birimini Türkçe biçimde gösterir. */
export function formatMoney(amount?: number | null, currency = 'EUR') {
  if (amount == null) return '—'
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency, maximumFractionDigits: 0 }).format(amount)
}

/** ISO tarihi kısa Türkçe biçimde gösterir. */
export function formatDate(value?: string | null) {
  if (!value) return '—'
  return new Intl.DateTimeFormat('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(new Date(value))
}

/** ISO tarih-saati Türkçe biçimde gösterir. */
export function formatDateTime(value?: string | null) {
  if (!value) return '—'
  return new Intl.DateTimeFormat('tr-TR', {
    day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit',
  }).format(new Date(value))
}
