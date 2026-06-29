import { useEffect, useRef, useState } from 'react'
import { Bell, LogOut, Menu, Search, Loader2 } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '@/store/auth'
import { useRealtimeStatus } from '@/services/realtime'
import { useGlobalSearch } from '@/features/boards/api'
import { riskLevelClasses, riskLevelLabels, tr } from '@/lib/labels'
import { cn } from '@/lib/utils'

const roleLabels: Record<string, string> = {
  Admin: 'Yönetici',
  DepartmentManager: 'Departman Müdürü',
  OperationSpecialist: 'Operasyon Uzmanı',
  Finance: 'Finans',
  ReadOnly: 'Salt Okuma',
}

const groupRoute: Record<string, string> = { Deniz: '/deniz', Kara: '/karayolu', Hava: '/hava', Finans: '/finans' }

export function Header({ onMenuClick }: { onMenuClick: () => void }) {
  const navigate = useNavigate()
  const user = useAuth((s) => s.user)
  const logout = useAuth((s) => s.logout)

  const roleText = user?.roles?.length ? (roleLabels[user.roles[0]] ?? user.roles[0]) : ''
  const connected = useRealtimeStatus((s) => s.connected)

  const [input, setInput] = useState('')
  const [debounced, setDebounced] = useState('')
  const [open, setOpen] = useState(false)
  const boxRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const t = setTimeout(() => setDebounced(input), 300)
    return () => clearTimeout(t)
  }, [input])

  const { data: results, isFetching } = useGlobalSearch(debounced)

  useEffect(() => {
    const onClickOutside = (e: MouseEvent) => {
      if (boxRef.current && !boxRef.current.contains(e.target as Node)) setOpen(false)
    }
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') setOpen(false) }
    document.addEventListener('mousedown', onClickOutside)
    document.addEventListener('keydown', onKey)
    return () => {
      document.removeEventListener('mousedown', onClickOutside)
      document.removeEventListener('keydown', onKey)
    }
  }, [])

  const goToResult = (r: { group: string; boardKey: string; ref: string }) => {
    setOpen(false)
    setInput('')
    const path = groupRoute[r.group] ?? '/'
    navigate(`${path}?board=${encodeURIComponent(r.boardKey)}&q=${encodeURIComponent(r.ref)}`)
  }

  const showDropdown = open && debounced.trim().length >= 2

  return (
    <header className="flex h-16 shrink-0 items-center gap-2 border-b bg-white px-3 sm:gap-4 sm:px-6">
      <button
        onClick={onMenuClick}
        className="rounded-lg p-2 text-slate-600 transition hover:bg-secondary lg:hidden"
        aria-label="Menüyü aç/kapat"
      >
        <Menu className="h-5 w-5" />
      </button>

      <div ref={boxRef} className="relative hidden w-full max-w-md md:block">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <input
          value={input}
          onChange={(e) => { setInput(e.target.value); setOpen(true) }}
          onFocus={() => setOpen(true)}
          placeholder="Deniz, Kara, Hava, Finans — tüm sekmelerde ara..."
          className="w-full rounded-lg border bg-secondary/50 py-2 pl-9 pr-3 text-sm outline-none transition focus:border-primary focus:bg-white"
        />

        {showDropdown && (
          <div className="absolute left-0 top-full z-50 mt-1.5 max-h-96 w-full overflow-y-auto rounded-lg border bg-white shadow-lg">
            {isFetching ? (
              <div className="flex items-center gap-2 px-4 py-3 text-sm text-muted-foreground">
                <Loader2 className="h-4 w-4 animate-spin" /> Aranıyor...
              </div>
            ) : !results?.length ? (
              <div className="px-4 py-3 text-sm text-muted-foreground">"{debounced}" için sonuç bulunamadı.</div>
            ) : (
              <ul className="divide-y">
                {results.map((r, i) => (
                  <li key={`${r.boardKey}-${r.ref}-${i}`}>
                    <button
                      onClick={() => goToResult(r)}
                      className="flex w-full flex-col gap-0.5 px-4 py-2.5 text-left transition hover:bg-secondary/60"
                    >
                      <div className="flex items-center justify-between gap-2">
                        <span className="truncate text-sm font-semibold text-slate-900">{r.ref}</span>
                        <span className={cn('shrink-0 rounded-full px-2 py-0.5 text-[11px] font-medium ring-1 ring-inset', riskLevelClasses[r.risk])}>
                          {tr(riskLevelLabels, r.risk)}
                        </span>
                      </div>
                      <div className="truncate text-xs text-muted-foreground">
                        {r.boardTitle} ({r.group}) · {r.matchedField}: {r.matchedValue}
                      </div>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        )}
      </div>

      <div className="ml-auto flex items-center gap-1.5 sm:gap-3">
        <span
          title={connected ? 'Canlı bağlantı etkin' : 'Canlı bağlantı yok'}
          className={`hidden items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium sm:flex ${
            connected ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'
          }`}
        >
          <span
            className={`h-2 w-2 rounded-full ${connected ? 'animate-pulse bg-emerald-500' : 'bg-slate-400'}`}
          />
          {connected ? 'Canlı' : 'Çevrimdışı'}
        </span>

        <button className="relative rounded-lg p-2 transition hover:bg-secondary" title="Bildirimler">
          <Bell className="h-5 w-5 text-slate-600" />
          <span className="absolute right-1.5 top-1.5 h-2 w-2 rounded-full bg-red-500" />
        </button>

        <div className="flex items-center gap-2 border-l pl-2 sm:gap-3 sm:pl-3">
          <div className="hidden text-right leading-tight sm:block">
            <div className="text-sm font-semibold text-slate-900">{user?.fullName ?? 'Kullanıcı'}</div>
            <div className="text-xs text-muted-foreground">{roleText}</div>
          </div>
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
            {(user?.fullName ?? 'K').charAt(0)}
          </div>
          <button
            onClick={async () => {
              await logout()
              navigate('/login')
            }}
            title="Çıkış"
            className="rounded-lg p-2 transition hover:bg-secondary"
          >
            <LogOut className="h-5 w-5 text-slate-600" />
          </button>
        </div>
      </div>
    </header>
  )
}
