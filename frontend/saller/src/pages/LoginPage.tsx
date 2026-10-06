import { useState, type FormEvent } from 'react'
import { Navigate } from 'react-router-dom'
import { Loader2 } from 'lucide-react'
import { useAuth } from '../hooks/useAuth'
import { ApiError } from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'

export default function LoginPage() {
  usePageMeta('Satıcı girişi')
  const { user, loading, login } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [pending, setPending] = useState(false)

  if (!loading && user) {
    return <Navigate to="/" replace />
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setPending(true)
    try {
      await login(email.trim(), password)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Giriş alınmadı.')
    } finally {
      setPending(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-linear-to-br from-green-950 via-green-900 to-slate-900 px-4">
      <div className="w-full max-w-md rounded-2xl bg-white p-8 shadow-xl">
        <h1 className="text-center text-xl font-bold text-slate-900">Satıcı girişi</h1>
        <p className="mt-1 text-center text-sm text-slate-500">
          Gülora satıcı hesabınızla daxil olun
        </p>
        <form className="mt-8 space-y-4" onSubmit={(e) => void onSubmit(e)}>
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-slate-700">
              E-poçt
            </label>
            <input
              id="email"
              type="email"
              autoComplete="username"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 outline-none focus:border-[#006437] focus:ring-2 focus:ring-[#006437]"
            />
          </div>
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-slate-700">
              Şifrə
            </label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 outline-none focus:border-[#006437] focus:ring-2 focus:ring-[#006437]"
            />
          </div>
          {error && (
            <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-800" role="alert">
              {error}
            </p>
          )}
          <button
            type="submit"
            disabled={pending || loading}
            className="flex w-full items-center justify-center gap-2 rounded-lg bg-[#006437] py-2.5 text-sm font-semibold text-white hover:bg-[#005029] disabled:opacity-60 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
          >
            {pending ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
                Gözləyin…
              </>
            ) : (
              'Daxil ol'
            )}
          </button>
        </form>
      </div>
    </div>
  )
}
