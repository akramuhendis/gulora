import { createContext, useContext, useState, useEffect } from "react";
import type { ReactNode } from "react";

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

interface AuthContextType {
  user: AuthUser | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<{ ok: boolean; error?: string }>;
  register: (data: RegisterData) => Promise<{ ok: boolean; error?: string }>;
  loginWithGoogle: () => Promise<{ ok: boolean }>;
  loginWithApple: () => Promise<{ ok: boolean }>;
  sendOtp: (phone: string) => Promise<{ ok: boolean }>;
  verifyOtp: (phone: string, code: string) => Promise<{ ok: boolean; error?: string }>;
  logout: () => void;
  updateProfile: (updates: Partial<AuthUser>) => void;
  changePassword: (oldPass: string, newPass: string) => Promise<{ ok: boolean; error?: string }>;
  sendPasswordReset: (email: string) => Promise<{ ok: boolean }>;
  confirmPasswordReset: (token: string, newPass: string) => Promise<{ ok: boolean; error?: string }>;
  deleteAccount: () => void;
}

export interface RegisterData {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  password: string;
}

const STORAGE_KEY = "gulora_auth_user";
const ACCOUNTS_KEY = "gulora_accounts";
const OTP_KEY = "gulora_otp_";

function loadUser(): AuthUser | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

function saveUser(u: AuthUser | null) {
  if (u) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(u));
  } else {
    localStorage.removeItem(STORAGE_KEY);
  }
}

type AccountRecord = AuthUser & { passwordHash: string };

function loadAccounts(): AccountRecord[] {
  try {
    const raw = localStorage.getItem(ACCOUNTS_KEY);
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

function saveAccounts(accounts: AccountRecord[]) {
  localStorage.setItem(ACCOUNTS_KEY, JSON.stringify(accounts));
}

function fakeHash(pw: string): string {
  return btoa(pw + "_gulora_salt");
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(loadUser);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    saveUser(user);
  }, [user]);

  const login = async (email: string, password: string) => {
    setIsLoading(true);
    await new Promise((r) => setTimeout(r, 900));
    const accounts = loadAccounts();
    const found = accounts.find(
      (a) => a.email.toLowerCase() === email.toLowerCase() && a.passwordHash === fakeHash(password)
    );
    setIsLoading(false);
    if (!found) return { ok: false, error: "E-poçt və ya şifrə yanlışdır." };
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const { passwordHash: _ph, ...authUser } = found;
    setUser(authUser);
    return { ok: true };
  };

  const register = async (data: RegisterData) => {
    setIsLoading(true);
    await new Promise((r) => setTimeout(r, 1100));
    const accounts = loadAccounts();
    if (accounts.find((a) => a.email.toLowerCase() === data.email.toLowerCase())) {
      setIsLoading(false);
      return { ok: false, error: "Bu e-poçt artıq qeydiyyatdan keçib." };
    }
    const newUser: AuthUser = {
      id: `u_${Date.now()}`,
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      phone: data.phone,
      memberSince: new Date().toISOString().slice(0, 10),
      emailVerified: false,
      phoneVerified: false,
      notifEmail: true,
      notifSms: true,
      notifCampaign: true,
    };
    const record: AccountRecord = { ...newUser, passwordHash: fakeHash(data.password) };
    accounts.push(record);
    saveAccounts(accounts);
    setUser(newUser);
    setIsLoading(false);
    return { ok: true };
  };

  const loginWithGoogle = async () => {
    setIsLoading(true);
    await new Promise((r) => setTimeout(r, 1200));
    const googleUser: AuthUser = {
      id: `g_${Date.now()}`,
      firstName: "Google",
      lastName: "İstifadəçi",
      email: `google_${Date.now()}@gmail.com`,
      phone: "",
      memberSince: new Date().toISOString().slice(0, 10),
      emailVerified: true,
      phoneVerified: false,
      notifEmail: true,
      notifSms: false,
      notifCampaign: true,
    };
    setUser(googleUser);
    setIsLoading(false);
    return { ok: true };
  };

  const loginWithApple = async () => {
    setIsLoading(true);
    await new Promise((r) => setTimeout(r, 1200));
    const appleUser: AuthUser = {
      id: `a_${Date.now()}`,
      firstName: "Apple",
      lastName: "İstifadəçi",
      email: `apple_${Date.now()}@icloud.com`,
      phone: "",
      memberSince: new Date().toISOString().slice(0, 10),
      emailVerified: true,
      phoneVerified: false,
      notifEmail: true,
      notifSms: false,
      notifCampaign: false,
    };
    setUser(appleUser);
    setIsLoading(false);
    return { ok: true };
  };

  const sendOtp = async (phone: string) => {
    const code = String(Math.floor(100000 + Math.random() * 900000));
    localStorage.setItem(OTP_KEY + phone, code);
    await new Promise((r) => setTimeout(r, 800));
    console.info(`[Gülora Demo] OTP for ${phone}: ${code}`);
    return { ok: true };
  };

  const verifyOtp = async (phone: string, code: string) => {
    await new Promise((r) => setTimeout(r, 600));
    const saved = localStorage.getItem(OTP_KEY + phone);
    if (!saved || saved !== code) return { ok: false, error: "OTP kodu yanlışdır." };
    localStorage.removeItem(OTP_KEY + phone);
    if (user) {
      const updated = { ...user, phone, phoneVerified: true };
      setUser(updated);
      const accounts = loadAccounts();
      const idx = accounts.findIndex((a) => a.id === user.id);
      if (idx >= 0) { accounts[idx] = { ...accounts[idx], phone, phoneVerified: true }; saveAccounts(accounts); }
    }
    return { ok: true };
  };

  const logout = () => {
    setUser(null);
  };

  const updateProfile = (updates: Partial<AuthUser>) => {
    if (!user) return;
    const updated = { ...user, ...updates };
    setUser(updated);
    const accounts = loadAccounts();
    const idx = accounts.findIndex((a) => a.id === user.id);
    if (idx >= 0) { accounts[idx] = { ...accounts[idx], ...updates }; saveAccounts(accounts); }
  };

  const changePassword = async (oldPass: string, newPass: string) => {
    if (!user) return { ok: false, error: "Giriş edilməyib." };
    await new Promise((r) => setTimeout(r, 700));
    const accounts = loadAccounts();
    const idx = accounts.findIndex((a) => a.id === user.id);
    if (idx < 0) return { ok: false, error: "Hesab tapılmadı." };
    if (accounts[idx].passwordHash !== fakeHash(oldPass)) return { ok: false, error: "Köhnə şifrə yanlışdır." };
    accounts[idx].passwordHash = fakeHash(newPass);
    saveAccounts(accounts);
    return { ok: true };
  };

  const sendPasswordReset = async (email: string) => {
    await new Promise((r) => setTimeout(r, 900));
    const token = btoa(`reset_${email}_${Date.now()}`);
    localStorage.setItem("gulora_reset_token", JSON.stringify({ token, email, expires: Date.now() + 15 * 60000 }));
    console.info(`[Gülora Demo] Password reset token for ${email}: ${token}`);
    return { ok: true };
  };

  const confirmPasswordReset = async (token: string, newPass: string) => {
    await new Promise((r) => setTimeout(r, 700));
    const raw = localStorage.getItem("gulora_reset_token");
    if (!raw) return { ok: false, error: "Token tapılmadı." };
    const data = JSON.parse(raw);
    if (data.token !== token || Date.now() > data.expires) return { ok: false, error: "Token etibarsızdır və ya müddəti bitib." };
    const accounts = loadAccounts();
    const idx = accounts.findIndex((a) => a.email.toLowerCase() === data.email.toLowerCase());
    if (idx < 0) return { ok: false, error: "İstifadəçi tapılmadı." };
    accounts[idx].passwordHash = fakeHash(newPass);
    saveAccounts(accounts);
    localStorage.removeItem("gulora_reset_token");
    return { ok: true };
  };

  const deleteAccount = () => {
    if (!user) return;
    const accounts = loadAccounts().filter((a) => a.id !== user.id);
    saveAccounts(accounts);
    localStorage.removeItem(`gulora_orders_${user.id}`);
    localStorage.removeItem(`gulora_addresses_${user.id}`);
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user, isLoading, login, register, loginWithGoogle, loginWithApple,
        sendOtp, verifyOtp, logout, updateProfile, changePassword,
        sendPasswordReset, confirmPasswordReset, deleteAccount,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
