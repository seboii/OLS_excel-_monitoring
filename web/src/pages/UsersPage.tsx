import { useState, type ReactNode } from 'react'
import { UserPlus, Pencil, KeyRound, Trash2, Loader2, X, ShieldCheck } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import {
  useUsers, useRoles, useDepartments, useCreateUser, useUpdateUser, useResetPassword, useDeleteUser,
  type UserItem,
} from '@/features/users/api'
import { cn } from '@/lib/utils'

const roleLabels: Record<string, string> = {
  Admin: 'Yönetici',
  DepartmentManager: 'Departman Müdürü',
  OperationSpecialist: 'Operasyon Uzmanı',
  Finance: 'Finans',
  ReadOnly: 'Salt Okuma',
}

function fmtDate(iso?: string) {
  return iso ? new Date(iso).toLocaleDateString('tr-TR') : '—'
}

export function UsersPage() {
  const { data: users, isLoading } = useUsers()
  const [editing, setEditing] = useState<UserItem | null>(null)
  const [creating, setCreating] = useState(false)

  const resetPw = useResetPassword()
  const del = useDeleteUser()

  const onReset = async (u: UserItem) => {
    const pw = window.prompt(`${u.fullName} için yeni parola (en az 8 karakter):`)
    if (!pw) return
    try {
      await resetPw.mutateAsync({ id: u.id, newPassword: pw })
      window.alert('Parola sıfırlandı.')
    } catch (e) {
      window.alert((e as Error).message)
    }
  }

  const onDelete = async (u: UserItem) => {
    if (!window.confirm(`${u.fullName} adlı kullanıcı silinsin mi?`)) return
    try {
      await del.mutateAsync(u.id)
    } catch (e) {
      window.alert((e as Error).message)
    }
  }

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Kullanıcılar ve Roller</h1>
          <p className="text-sm text-muted-foreground">Kullanıcı ekleyin, rol atayın, parola sıfırlayın (yalnızca Yönetici).</p>
        </div>
        <button
          onClick={() => setCreating(true)}
          className="inline-flex items-center gap-2 rounded-lg bg-primary px-3.5 py-2 text-sm font-medium text-white transition hover:bg-primary/90"
        >
          <UserPlus className="h-4 w-4" /> Yeni Kullanıcı
        </button>
      </div>

      <Card className="overflow-x-auto p-1.5">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b text-left text-xs uppercase tracking-wide text-muted-foreground [&>th]:px-3 [&>th]:py-2.5 [&>th]:font-medium">
              <th>Ad Soyad</th><th>E-posta</th><th>Roller</th><th>Departman</th><th>Durum</th><th>Son Giriş</th><th></th>
            </tr>
          </thead>
          <tbody>
            {users?.map((u) => (
              <tr key={u.id} className="border-b last:border-0 [&>td]:px-3 [&>td]:py-2.5">
                <td className="font-medium text-slate-900">{u.fullName}</td>
                <td className="text-slate-600">{u.email}</td>
                <td>
                  <div className="flex flex-wrap gap-1">
                    {u.roles.map((r) => (
                      <span key={r} className="rounded-full bg-secondary px-2 py-0.5 text-[11px] font-medium text-slate-700">
                        {roleLabels[r] ?? r}
                      </span>
                    ))}
                  </div>
                </td>
                <td className="text-slate-500">{u.departmentName ?? '—'}</td>
                <td>
                  <span className={cn('rounded-full px-2 py-0.5 text-[11px] font-medium',
                    u.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-200 text-slate-500')}>
                    {u.isActive ? 'Aktif' : 'Pasif'}
                  </span>
                </td>
                <td className="text-slate-500">{fmtDate(u.lastLoginAt)}</td>
                <td>
                  <div className="flex items-center justify-end gap-1">
                    <button onClick={() => setEditing(u)} title="Düzenle" className="rounded p-1.5 text-slate-500 transition hover:bg-secondary hover:text-slate-900">
                      <Pencil className="h-4 w-4" />
                    </button>
                    <button onClick={() => onReset(u)} title="Parola sıfırla" className="rounded p-1.5 text-slate-500 transition hover:bg-secondary hover:text-amber-600">
                      <KeyRound className="h-4 w-4" />
                    </button>
                    <button onClick={() => onDelete(u)} title="Sil" className="rounded p-1.5 text-slate-500 transition hover:bg-secondary hover:text-red-600">
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {isLoading && (
          <div className="flex items-center gap-2 px-4 py-8 text-sm text-muted-foreground">
            <Loader2 className="h-4 w-4 animate-spin" /> Yükleniyor...
          </div>
        )}
        {!isLoading && !users?.length && <div className="py-12 text-center text-muted-foreground">Kullanıcı yok.</div>}
      </Card>

      {(creating || editing) && (
        <UserModal user={editing} onClose={() => { setCreating(false); setEditing(null) }} />
      )}
    </div>
  )
}

function UserModal({ user, onClose }: { user: UserItem | null; onClose: () => void }) {
  const isEdit = user != null
  const { data: roles } = useRoles()
  const { data: departments } = useDepartments()
  const create = useCreateUser()
  const update = useUpdateUser()

  const [fullName, setFullName] = useState(user?.fullName ?? '')
  const [email, setEmail] = useState(user?.email ?? '')
  const [password, setPassword] = useState('')
  const [departmentId, setDepartmentId] = useState<number | ''>(user?.departmentId ?? '')
  const [selectedRoles, setSelectedRoles] = useState<string[]>(user?.roles ?? [])
  const [isActive, setIsActive] = useState(user?.isActive ?? true)
  const [error, setError] = useState<string | null>(null)

  const toggleRole = (code: string) =>
    setSelectedRoles((prev) => (prev.includes(code) ? prev.filter((r) => r !== code) : [...prev, code]))

  const submit = async () => {
    setError(null)
    try {
      if (isEdit && user) {
        await update.mutateAsync({
          id: user.id,
          input: { fullName, roles: selectedRoles, departmentId: departmentId === '' ? null : departmentId, isActive },
        })
      } else {
        await create.mutateAsync({
          fullName, email, password, roles: selectedRoles,
          departmentId: departmentId === '' ? null : departmentId,
        })
      }
      onClose()
    } catch (e) {
      setError((e as Error).message)
    }
  }

  const pending = create.isPending || update.isPending

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div className="w-full max-w-md rounded-xl bg-white shadow-xl" onClick={(e) => e.stopPropagation()}>
        <div className="flex items-center justify-between border-b px-5 py-3.5">
          <h2 className="flex items-center gap-2 text-base font-semibold text-slate-900">
            <ShieldCheck className="h-4 w-4 text-primary" />
            {isEdit ? 'Kullanıcıyı Düzenle' : 'Yeni Kullanıcı'}
          </h2>
          <button onClick={onClose} className="rounded p-1 text-slate-400 hover:bg-secondary"><X className="h-4 w-4" /></button>
        </div>

        <div className="space-y-3.5 px-5 py-4">
          <Field label="Ad Soyad">
            <input value={fullName} onChange={(e) => setFullName(e.target.value)} className={inputCls} placeholder="Adı Soyadı" />
          </Field>
          <Field label="E-posta">
            <input value={email} onChange={(e) => setEmail(e.target.value)} disabled={isEdit}
              className={cn(inputCls, isEdit && 'cursor-not-allowed bg-secondary/60')} placeholder="ornek@ols.local" />
          </Field>
          {!isEdit && (
            <Field label="Parola (en az 8 karakter)">
              <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} className={inputCls} placeholder="••••••••" />
            </Field>
          )}
          <Field label="Departman">
            <select value={departmentId} onChange={(e) => setDepartmentId(e.target.value === '' ? '' : Number(e.target.value))} className={inputCls}>
              <option value="">— Yok —</option>
              {departments?.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select>
          </Field>
          <Field label="Roller">
            <div className="flex flex-wrap gap-1.5">
              {roles?.map((r) => (
                <button key={r.code} type="button" onClick={() => toggleRole(r.code)}
                  className={cn('rounded-full border px-2.5 py-1 text-xs font-medium transition',
                    selectedRoles.includes(r.code) ? 'border-primary bg-primary/10 text-primary' : 'border-slate-200 text-slate-600 hover:bg-secondary')}>
                  {roleLabels[r.code] ?? r.name}
                </button>
              ))}
            </div>
          </Field>
          {isEdit && (
            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} className="h-4 w-4 rounded" />
              Hesap aktif
            </label>
          )}
          {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
        </div>

        <div className="flex justify-end gap-2 border-t px-5 py-3">
          <button onClick={onClose} className="rounded-lg px-3.5 py-2 text-sm font-medium text-slate-600 transition hover:bg-secondary">İptal</button>
          <button onClick={submit} disabled={pending}
            className="inline-flex items-center gap-2 rounded-lg bg-primary px-3.5 py-2 text-sm font-medium text-white transition hover:bg-primary/90 disabled:opacity-60">
            {pending && <Loader2 className="h-4 w-4 animate-spin" />}
            {isEdit ? 'Kaydet' : 'Oluştur'}
          </button>
        </div>
      </div>
    </div>
  )
}

const inputCls = 'w-full rounded-lg border bg-white px-3 py-2 text-sm outline-none transition focus:border-primary'

function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="space-y-1">
      <label className="text-xs font-medium text-slate-600">{label}</label>
      {children}
    </div>
  )
}
