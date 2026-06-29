import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { KeyRound, AlertCircle, CheckCircle2, Loader2, User, SlidersHorizontal } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { useChangePassword } from '@/features/auth/api'
import { useRiskThresholds, useUpdateRiskThresholds } from '@/features/settings/api'
import { useAuth } from '@/store/auth'

const roleLabels: Record<string, string> = {
  Admin: 'Yönetici',
  DepartmentManager: 'Departman Müdürü',
  OperationSpecialist: 'Operasyon Uzmanı',
  Finance: 'Finans',
  ReadOnly: 'Salt Okuma',
}

export function AyarlarPage() {
  const navigate = useNavigate()
  const user = useAuth((s) => s.user)
  const logout = useAuth((s) => s.logout)
  const changePassword = useChangePassword()

  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setSuccess(false)

    if (newPassword.length < 8) {
      setError('Yeni parola en az 8 karakter olmalı.')
      return
    }
    if (newPassword !== confirmPassword) {
      setError('Yeni parolalar birbiriyle eşleşmiyor.')
      return
    }

    try {
      await changePassword.mutateAsync({ currentPassword, newPassword })
      setSuccess(true)
      setCurrentPassword('')
      setNewPassword('')
      setConfirmPassword('')
      // Backend parola değişince oturumu (refresh token) geçersiz kılıyor — güvenlik için tekrar giriş gerekir.
      setTimeout(async () => {
        await logout()
        navigate('/login')
      }, 1800)
    } catch (err) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        'Parola değiştirilemedi.'
      setError(message)
    }
  }

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Ayarlar</h1>
        <p className="text-sm text-muted-foreground">Hesap bilgileri ve güvenlik</p>
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader className="flex-row items-center gap-2">
            <User className="h-5 w-5 text-primary" />
            <CardTitle>Hesap Bilgileri</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2 pt-0 text-sm">
            <div className="flex justify-between border-b py-2">
              <span className="text-muted-foreground">Ad Soyad</span>
              <span className="font-medium text-slate-900">{user?.fullName ?? '—'}</span>
            </div>
            <div className="flex justify-between border-b py-2">
              <span className="text-muted-foreground">E-posta</span>
              <span className="font-medium text-slate-900">{user?.email ?? '—'}</span>
            </div>
            <div className="flex justify-between py-2">
              <span className="text-muted-foreground">Rol</span>
              <span className="font-medium text-slate-900">
                {user?.roles?.length ? (roleLabels[user.roles[0]] ?? user.roles[0]) : '—'}
              </span>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex-row items-center gap-2">
            <KeyRound className="h-5 w-5 text-primary" />
            <CardTitle>Parola Değiştir</CardTitle>
          </CardHeader>
          <CardContent className="pt-0">
            <form onSubmit={submit} className="space-y-3">
              {error && (
                <div className="flex items-center gap-2 rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-700">
                  <AlertCircle className="h-4 w-4 shrink-0" /> {error}
                </div>
              )}
              {success && (
                <div className="flex items-center gap-2 rounded-lg bg-emerald-50 px-3 py-2.5 text-sm text-emerald-700">
                  <CheckCircle2 className="h-4 w-4 shrink-0" /> Parola güncellendi, tekrar giriş ekranına yönlendiriliyorsunuz...
                </div>
              )}

              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Mevcut Parola</label>
                <input
                  type="password"
                  value={currentPassword}
                  onChange={(e) => setCurrentPassword(e.target.value)}
                  className="w-full rounded-lg border bg-white px-3 py-2 text-sm outline-none transition focus:border-primary"
                  required
                  autoComplete="current-password"
                />
              </div>

              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Yeni Parola</label>
                <input
                  type="password"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  className="w-full rounded-lg border bg-white px-3 py-2 text-sm outline-none transition focus:border-primary"
                  required
                  minLength={8}
                  autoComplete="new-password"
                />
              </div>

              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Yeni Parola (tekrar)</label>
                <input
                  type="password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  className="w-full rounded-lg border bg-white px-3 py-2 text-sm outline-none transition focus:border-primary"
                  required
                  minLength={8}
                  autoComplete="new-password"
                />
              </div>

              <button
                type="submit"
                disabled={changePassword.isPending}
                className="flex w-full items-center justify-center gap-2 rounded-lg bg-primary py-2.5 text-sm font-semibold text-white transition hover:bg-primary/90 disabled:opacity-60"
              >
                {changePassword.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <KeyRound className="h-4 w-4" />}
                {changePassword.isPending ? 'Güncelleniyor...' : 'Parolayı Güncelle'}
              </button>
            </form>
          </CardContent>
        </Card>

        <RiskThresholdsCard />
      </div>
    </div>
  )
}

function RiskThresholdsCard() {
  const { data, isLoading } = useRiskThresholds()
  const update = useUpdateRiskThresholds()

  const [delayOrangeDays, setDelayOrangeDays] = useState(7)
  const [delayRedDays, setDelayRedDays] = useState(15)
  const [financeOverdueOrangeDays, setFinanceOverdueOrangeDays] = useState(21)
  const [financeOverdueRedDays, setFinanceOverdueRedDays] = useState(45)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  useEffect(() => {
    if (!data) return
    setDelayOrangeDays(data.delayOrangeDays)
    setDelayRedDays(data.delayRedDays)
    setFinanceOverdueOrangeDays(data.financeOverdueOrangeDays)
    setFinanceOverdueRedDays(data.financeOverdueRedDays)
  }, [data])

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setSuccess(false)
    try {
      await update.mutateAsync({ delayOrangeDays, delayRedDays, financeOverdueOrangeDays, financeOverdueRedDays })
      setSuccess(true)
      setTimeout(() => setSuccess(false), 3000)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Eşikler güncellenemedi.')
    }
  }

  return (
    <Card className="lg:col-span-2">
      <CardHeader className="flex-row items-center gap-2">
        <SlidersHorizontal className="h-5 w-5 text-primary" />
        <CardTitle>Risk Eşikleri</CardTitle>
      </CardHeader>
      <CardContent className="pt-0">
        <p className="mb-4 text-sm text-muted-foreground">
          Tahsilat eşiği kaydedince hemen etkili olur; gecikme eşiği bir sonraki senkronizasyonda satırlara işlenir.
          Kod değişikliği veya yeniden dağıtım gerekmez.
        </p>

        {isLoading ? (
          <div className="py-4 text-sm text-muted-foreground">Yükleniyor...</div>
        ) : (
          <form onSubmit={submit} className="space-y-4">
            {error && (
              <div className="flex items-center gap-2 rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-700">
                <AlertCircle className="h-4 w-4 shrink-0" /> {error}
              </div>
            )}
            {success && (
              <div className="flex items-center gap-2 rounded-lg bg-emerald-50 px-3 py-2.5 text-sm text-emerald-700">
                <CheckCircle2 className="h-4 w-4 shrink-0" /> Eşikler güncellendi, risk motoru yeniden çalıştı.
              </div>
            )}

            <div>
              <div className="mb-2 text-sm font-medium text-slate-700">Gecikme risk eşiği (Deniz/Kara/Hava — gün)</div>
              <div className="grid grid-cols-2 gap-3">
                <NumberField label="Turuncu eşiği (gün üzeri)" value={delayOrangeDays} onChange={setDelayOrangeDays} />
                <NumberField label="Kırmızı eşiği (gün üzeri)" value={delayRedDays} onChange={setDelayRedDays} />
              </div>
            </div>

            <div>
              <div className="mb-2 text-sm font-medium text-slate-700">Alabora tahsilat gecikme eşiği (gün)</div>
              <div className="grid grid-cols-2 gap-3">
                <NumberField label="Turuncu eşiği (gün üzeri)" value={financeOverdueOrangeDays} onChange={setFinanceOverdueOrangeDays} />
                <NumberField label="Kırmızı eşiği (gün üzeri)" value={financeOverdueRedDays} onChange={setFinanceOverdueRedDays} />
              </div>
            </div>

            <button
              type="submit"
              disabled={update.isPending}
              className="flex items-center justify-center gap-2 rounded-lg bg-primary px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-primary/90 disabled:opacity-60"
            >
              {update.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <SlidersHorizontal className="h-4 w-4" />}
              {update.isPending ? 'Kaydediliyor...' : 'Eşikleri Kaydet'}
            </button>
          </form>
        )}
      </CardContent>
    </Card>
  )
}

function NumberField({ label, value, onChange }: { label: string; value: number; onChange: (v: number) => void }) {
  return (
    <div className="space-y-1.5">
      <label className="text-xs font-medium text-slate-600">{label}</label>
      <input
        type="number"
        min={1}
        value={value}
        onChange={(e) => onChange(Math.max(1, Number(e.target.value) || 1))}
        className="w-full rounded-lg border bg-white px-3 py-2 text-sm outline-none transition focus:border-primary"
      />
    </div>
  )
}
