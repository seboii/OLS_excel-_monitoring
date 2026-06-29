import { NavLink } from 'react-router-dom'
import {
  LayoutDashboard, Activity, Truck, Ship, Plane, Wallet, FileCheck,
  Users, Bell, ShieldAlert, BarChart3, FileBarChart, MessagesSquare,
  Database, Settings, MonitorPlay,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { cn } from '@/lib/utils'

interface MenuItem {
  to: string
  label: string
  icon: LucideIcon
  end?: boolean
}

const groups: { title: string; items: MenuItem[] }[] = [
  {
    title: 'Genel',
    items: [
      { to: '/', label: 'Ana Sayfa', icon: LayoutDashboard, end: true },
      { to: '/genel-bakis', label: 'Genel Bakış', icon: Activity },
    ],
  },
  {
    title: 'Operasyonlar',
    items: [
      { to: '/karayolu', label: 'Karayolu', icon: Truck },
      { to: '/deniz', label: 'Deniz', icon: Ship },
      { to: '/hava', label: 'Hava', icon: Plane },
      { to: '/finans', label: 'Finans ve Tahsilatlar', icon: Wallet },
      { to: '/gumruk', label: 'Gümrük ve Evrak', icon: FileCheck },
    ],
  },
  {
    title: 'Takip & Analiz',
    items: [
      { to: '/musteri', label: 'Müşteri ve Bildirim', icon: Users },
      { to: '/uyarilar', label: 'Uyarılar ve Görevler', icon: Bell },
      { to: '/risk', label: 'Risk Haritası', icon: ShieldAlert },
      { to: '/kpi', label: 'KPI ve Performans', icon: BarChart3 },
    ],
  },
  {
    title: 'Sistem',
    items: [
      { to: '/raporlar', label: 'Raporlar', icon: FileBarChart },
      { to: '/yorumlar', label: 'Yorumlar', icon: MessagesSquare },
      { to: '/veri-kaynaklari', label: 'Veri Kaynakları', icon: Database },
      { to: '/ayarlar', label: 'Ayarlar', icon: Settings },
    ],
  },
]

/**
 * Masaüstünde (lg+) her zaman görünen statik sütun; mobil/tablette sol kenardan açılan,
 * arka plan örtülü (backdrop) bir çekmece (drawer). `open`/`onClose` mobil çekmece durumunu yönetir.
 */
export function Sidebar({ open, onClose }: { open: boolean; onClose: () => void }) {
  return (
    <>
      {open && (
        <div className="fixed inset-0 z-40 bg-black/40 lg:hidden" onClick={onClose} aria-hidden="true" />
      )}

      <aside
        className={cn(
          'fixed inset-y-0 left-0 z-50 flex w-64 shrink-0 flex-col bg-sidebar text-sidebar-foreground transition-transform duration-200 ease-out',
          'lg:static lg:translate-x-0',
          open ? 'translate-x-0' : '-translate-x-full',
        )}
      >
        <div className="flex h-16 items-center gap-3 border-b border-sidebar-border px-5">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary text-sm font-bold text-white">
            OLS
          </div>
          <div className="leading-tight">
            <div className="text-sm font-semibold text-white">Operasyon Kontrol</div>
            <div className="text-[11px] text-sidebar-muted">Merkezi</div>
          </div>
        </div>

        <nav className="flex-1 space-y-6 overflow-y-auto px-3 py-4">
          {groups.map((group) => (
            <div key={group.title}>
              <div className="px-3 pb-2 text-[11px] font-semibold uppercase tracking-wider text-sidebar-muted">
                {group.title}
              </div>
              <div className="space-y-0.5">
                {group.items.map((item) => (
                  <NavLink
                    key={item.to}
                    to={item.to}
                    end={item.end}
                    onClick={onClose}
                    className={({ isActive }) =>
                      cn(
                        'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition',
                        isActive
                          ? 'bg-sidebar-active text-white shadow-sm'
                          : 'text-sidebar-foreground hover:bg-sidebar-hover hover:text-white',
                      )
                    }
                  >
                    <item.icon className="h-[18px] w-[18px]" />
                    <span className="truncate">{item.label}</span>
                  </NavLink>
                ))}
              </div>
            </div>
          ))}
        </nav>

        <div className="border-t border-sidebar-border p-3">
          <NavLink
            to="/tv"
            onClick={onClose}
            className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-sidebar-foreground transition hover:bg-sidebar-hover hover:text-white"
          >
            <MonitorPlay className="h-[18px] w-[18px]" />
            <span>TV Modu</span>
          </NavLink>
        </div>
      </aside>
    </>
  )
}
