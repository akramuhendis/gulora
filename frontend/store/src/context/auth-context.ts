import { createContext } from "react";

export interface AuthUser {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  avatar?: string;
  memberSince: string;
  emailVerified: boolean;
  phoneVerified: boolean;
  notifEmail: boolean;
  notifSms: boolean;
  notifCampaign: boolean;
}

export interface RegisterData {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  password: string;
}

export interface AuthContextType {
  user: AuthUser | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<{ ok: boolean; error?: string }>;
  register: (data: RegisterData) => Promise<{ ok: boolean; error?: string }>;
  sendOtp: (phone: string) => Promise<{ ok: boolean; message?: string }>;
  verifyOtp: (phone: string, code: string) => Promise<{ ok: boolean; error?: string }>;
  logout: () => Promise<void>;
  updateProfile: (updates: Partial<AuthUser>) => void;
  changePassword: (oldPass: string, newPass: string) => Promise<{ ok: boolean; error?: string }>;
  sendPasswordReset: (email: string) => Promise<{ ok: boolean; error?: string }>;
  confirmPasswordReset: (token: string, newPass: string) => Promise<{ ok: boolean; error?: string }>;
  deleteAccount: () => void;
  refreshSessionUser: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);
