import { useCallback, useEffect, useState } from 'react'
import { Loader2, ShieldCheck, User } from 'lucide-react'
import { ApiError, authApi, type CurrentUserResponse } from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'

function formatIso(iso: string | null): string {
  if (!iso) return '—'
  return new Date(iso).toLocaleString('az-AZ', { dateStyle: 'medium', timeStyle: 'short' })
}

export default function AccountPage() {
  usePageMeta('Hesabım')

  const [user, setUser] = useState<CurrentUserResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const me = await authApi.me()
      setUser(me)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Profil yüklənmədi.')
      setUser(null)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    queueMicrotask(() => void load())
  }, [load])

  return (
    <div>
      <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">Hesabım</h1>
      <p className="mt-2 max-w-3xl text-slate-600">
        Şəxsi məlumatlar JWT sessiyası ilə `/auth/me` üzərindən gəlir; redaktə üçün API əlavə
        olunduqda bu forma bağlanacaq.
      </p>

      {error && (
        <p className="mt-6 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {error}
        </p>
      )}

      {loading ? (
        <div className="mt-16 flex justify-center text-slate-500">
          <Loader2 className="h-10 w-10 animate-spin" aria-label="Yüklənir" />
        </div>
      ) : user ? (
        <div className="mt-10 grid gap-6 lg:grid-cols-3">
          <section
            className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm lg:col-span-2"
            aria-labelledby="profile-heading"
          >
            <div className="flex items-start gap-4">
              <span className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-[#006437]/10 text-[#006437]">
                <User className="h-7 w-7" aria-hidden />
              </span>
              <div className="min-w-0 flex-1">
                <h2 id="profile-heading" className="text-lg font-semibold text-slate-900">
                  {user.fullName}
                </h2>
                <p className="mt-1 truncate text-sm text-slate-600" title={user.email}>
                  {user.email}
                </p>
                {user.phoneNumber && (
                  <p className="mt-1 text-sm text-slate-600">{user.phoneNumber}</p>
                )}
              </div>
            </div>
            <dl className="mt-8 grid gap-4 sm:grid-cols-2">
              <div>
                <dt className="text-xs font-semibold uppercase tracking-wide text-slate-500">Rol</dt>
                <dd className="mt-1 text-sm font-medium text-slate-900">{user.role}</dd>
              </div>
              <div>
                <dt className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                  Status
                </dt>
                <dd className="mt-1 text-sm font-medium text-slate-900">
                  {user.isActive ? 'Aktiv' : 'Deaktiv'}
                </dd>
              </div>
              <div>
                <dt className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                  E-poçt təsdiqi
                </dt>
                <dd className="mt-1 text-sm font-medium text-slate-900">
                  {user.emailVerified ? 'Təsdiqlənib' : 'Gözləyir'}
                </dd>
              </div>
              <div>
                <dt className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                  Telefon təsdiqi
                </dt>
                <dd className="mt-1 text-sm font-medium text-slate-900">
                  {user.phoneVerified ? 'Təsdiqlənib' : 'Gözləyir'}
                </dd>
              </div>
              <div className="sm:col-span-2">
                <dt className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                  Son giriş
                </dt>
                <dd className="mt-1 text-sm text-slate-800">{formatIso(user.lastLoginAt)}</dd>
              </div>
              <div className="sm:col-span-2">
                <dt className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                  Qeydiyyat tarixi
                </dt>
                <dd className="mt-1 text-sm text-slate-800">{formatIso(user.createdAt)}</dd>
              </div>
            </dl>
          </section>

          <aside className="rounded-xl border border-slate-200 bg-slate-50 p-6">
            <div className="flex items-center gap-2 text-slate-800">
              <ShieldCheck className="h-5 w-5 text-[#006437]" aria-hidden />
              <p className="font-semibold">Təhlükəsizlik</p>
            </div>
            <p className="mt-3 text-sm text-slate-600">
              Şifrə dəyişimi və iki faktorlu doğrulama təhlükəsizlik parametrləri əlavə olunduqda burada
              göstəriləcək.
            </p>
          </aside>
        </div>
      ) : null}
    </div>
  )
}
