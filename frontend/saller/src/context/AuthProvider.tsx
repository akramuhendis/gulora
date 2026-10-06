import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { ApiError, authApi, type CurrentUserResponse } from '../services/api'
import { AuthContext } from './auth-context'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUserResponse | null>(null)
  const [loading, setLoading] = useState(true)

  const refreshUser = useCallback(async () => {
    const token = localStorage.getItem('gulora_access_token')
    if (!token) {
      setUser(null)
      return
    }
    const me = await authApi.me()
    if (me.role.toLowerCase() !== 'seller') {
      await authApi.logout()
      setUser(null)
      throw new ApiError(
        403,
        'Bu hesabın satıcı rolü yoxdur. Zəhmət olmasa satıcı hesabı ilə daxil olun.'
      )
    }
    setUser(me)
  }, [])

  useEffect(() => {
    let cancelled = false
    queueMicrotask(() => {
      void (async () => {
        try {
          await refreshUser()
        } catch {
          if (!cancelled) setUser(null)
        } finally {
          if (!cancelled) setLoading(false)
        }
      })()
    })
    return () => {
      cancelled = true
    }
  }, [refreshUser])

  const login = useCallback(async (email: string, password: string) => {
    const auth = await authApi.login(email, password)
    if (auth.user.role.toLowerCase() !== 'seller') {
      throw new ApiError(
        403,
        'Bu hesab satıcı paneli üçün uyğun deyil. Yalnız təsdiqlənmiş satıcı hesabları daxil ola bilər.'
      )
    }
    authApi.persistSession(auth)
    const me = await authApi.me()
    setUser(me)
  }, [])

  const logout = useCallback(async () => {
    await authApi.logout()
    setUser(null)
  }, [])

  const value = useMemo(
    () => ({ user, loading, login, logout, refreshUser }),
    [user, loading, login, logout, refreshUser]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
