import { createContext } from 'react'
import type { CurrentUserResponse } from '../services/api'

export type AuthContextValue = {
  user: CurrentUserResponse | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
  refreshUser: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)
