import type { ReactNode } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { AppShell } from '@/layouts/AppShell'
import { DashboardPage } from '@/pages/DashboardPage'
import { OperationsPage } from '@/pages/OperationsPage'
import { BoardGroupPage } from '@/pages/BoardGroupPage'
import { FinancePage } from '@/pages/FinancePage'
import { RiskMapPage } from '@/pages/RiskMapPage'
import { LoginPage } from '@/pages/LoginPage'
import { AlertsTasksPage } from '@/pages/AlertsTasksPage'
import { KpiPage } from '@/pages/KpiPage'
import { ReportsPage } from '@/pages/ReportsPage'
import { DataSourcesPage } from '@/pages/DataSourcesPage'
import { CommentsPage } from '@/pages/CommentsPage'
import { UsersPage } from '@/pages/UsersPage'
import { AyarlarPage } from '@/pages/AyarlarPage'
import { TvPage } from '@/pages/TvPage'
import { useAuth } from '@/store/auth'

function RequireAuth({ children }: { children: ReactNode }) {
  const isAuthenticated = useAuth((s) => s.isAuthenticated)
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />
}

function RequireRole({ roles, children }: { roles: string[]; children: ReactNode }) {
  const userRoles = useAuth((s) => s.user?.roles ?? [])
  const allowed = roles.some((r) => userRoles.includes(r))
  return allowed ? <>{children}</> : <Navigate to="/" replace />
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/tv"
        element={
          <RequireAuth>
            <TvPage />
          </RequireAuth>
        }
      />
      <Route
        element={
          <RequireAuth>
            <AppShell />
          </RequireAuth>
        }
      >
        <Route index element={<DashboardPage />} />
        <Route path="genel-bakis" element={<DashboardPage />} />
        <Route path="operasyonlar" element={<OperationsPage title="Tüm Operasyonlar" />} />
        <Route path="karayolu" element={<BoardGroupPage group="Kara" title="Karayolu Operasyonları" />} />
        <Route path="deniz" element={<BoardGroupPage group="Deniz" title="Deniz Operasyonları" />} />
        <Route path="hava" element={<BoardGroupPage group="Hava" title="Hava Operasyonları" />} />
        <Route path="finans" element={<FinancePage />} />
        <Route path="uyarilar" element={<AlertsTasksPage />} />
        <Route path="risk" element={<RiskMapPage />} />
        <Route path="kpi" element={<KpiPage />} />
        <Route path="raporlar" element={<ReportsPage />} />
        <Route path="yorumlar" element={<CommentsPage />} />
        <Route
          path="veri-kaynaklari"
          element={
            <RequireRole roles={['Admin', 'DepartmentManager']}>
              <DataSourcesPage />
            </RequireRole>
          }
        />
        <Route
          path="kullanicilar"
          element={
            <RequireRole roles={['Admin']}>
              <UsersPage />
            </RequireRole>
          }
        />
        <Route path="ayarlar" element={<AyarlarPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  )
}
