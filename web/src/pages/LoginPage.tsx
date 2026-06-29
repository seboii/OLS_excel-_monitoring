import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { ShieldCheck, LogIn, AlertCircle, Loader2 } from 'lucide-react'
import { useAuth } from '@/store/auth'

export function LoginPage() {
  const navigate = useNavigate()
  const login = useAuth((s) => s.login)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError(null)
    const result = await login(email, password)
    setLoading(false)
    if (result.ok) navigate('/')
    else setError(result.error ?? 'Giriş başarısız.')
  }

  return (
    <div className="flex min-h-screen">
      <div className="hidden flex-1 flex-col justify-between bg-sidebar p-12 text-white lg:flex">
        <div className="flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-primary text-lg font-bold">OLS</div>
          <div className="text-lg font-semibold">Operasyon Kontrol Merkezi</div>
        </div>
        <div className="space-y-4">
          <h1 className="text-4xl font-bold leading-tight">
            Her operasyonun sahibi,<br />statüsü ve riski<br />tek ekranda.
          </h1>
          <p className="max-w-md text-sidebar-foreground">
            Karayolu, deniz, hava, gümrük ve finans operasyonlarını gerçek zamanlıya yakın izleyin;
            gecikmeleri, evrak eksiklerini ve tahsilat risklerini otomatik yakalayın.
          </p>
        </div>
        <div className="text-sm text-sidebar-muted">© OLS Dış Ticaret</div>
      </div>

      <div className="flex flex-1 items-center justify-center bg-background p-8">
        <form onSubmit={submit} className="w-full max-w-sm space-y-5">
          <div className="space-y-1">
            <div className="flex items-center gap-2 text-primary">
              <ShieldCheck className="h-5 w-5" />
              <span className="text-sm font-semibold uppercase tracking-wide">Güvenli Giriş</span>
            </div>
            <h2 className="text-2xl font-bold text-slate-900">Hoş geldiniz</h2>
            <p className="text-sm text-muted-foreground">Devam etmek için giriş yapın.</p>
          </div>

          {error && (
            <div className="flex items-center gap-2 rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-700">
              <AlertCircle className="h-4 w-4 shrink-0" />
              {error}
            </div>
          )}

          <div className="space-y-1.5">
            <label className="text-sm font-medium text-slate-700">E-posta</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full rounded-lg border bg-white px-3 py-2.5 text-sm outline-none transition focus:border-primary"
              required
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-sm font-medium text-slate-700">Parola</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full rounded-lg border bg-white px-3 py-2.5 text-sm outline-none transition focus:border-primary"
              required
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="flex w-full items-center justify-center gap-2 rounded-lg bg-primary py-2.5 text-sm font-semibold text-white transition hover:bg-primary/90 disabled:opacity-60"
          >
            {loading ? <Loader2 className="h-4 w-4 animate-spin" /> : <LogIn className="h-4 w-4" />}
            {loading ? 'Giriş yapılıyor...' : 'Giriş Yap'}
          </button>
        </form>
      </div>
    </div>
  )
}
