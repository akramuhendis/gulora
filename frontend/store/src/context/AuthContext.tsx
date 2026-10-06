import { useState, useEffect, useCallback } from "react";
import type { ReactNode } from "react";
import {
  authApi,
  clearAuthTokens,
  setAuthTokens,
  userApi,
  ApiError,
  type CurrentUserWire,
  type UserProfileWire,
} from "../services/api";
import { AuthContext, type AuthUser, type RegisterData } from "./auth-context";

const STORAGE_KEY = "gulora_auth_user_prefs";

function loadPrefs(): Partial<
  Pick<AuthUser, "notifEmail" | "notifSms" | "notifCampaign">
> {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : {};
  } catch {
    return {};
  }
}

function savePrefs(p: Partial<Pick<AuthUser, "notifEmail" | "notifSms" | "notifCampaign">>) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(p));
}

function mapCurrentUser(u: CurrentUserWire): AuthUser {
  const prefs = loadPrefs();
  return {
    id: u.id,
    firstName: u.firstName,
    lastName: u.lastName,
    email: u.email,
    phone: u.phoneNumber ?? "",
    memberSince: u.createdAt.slice(0, 10),
    emailVerified: u.emailVerified,
    phoneVerified: u.phoneVerified,
    notifEmail: prefs.notifEmail ?? true,
    notifSms: prefs.notifSms ?? true,
    notifCampaign: prefs.notifCampaign ?? true,
  };
}

function mapProfile(p: UserProfileWire): AuthUser {
  const prefs = loadPrefs();
  return {
    id: p.id,
    firstName: p.firstName,
    lastName: p.lastName,
    email: p.email,
    phone: p.phoneNumber ?? "",
    memberSince: p.createdAt.slice(0, 10),
    emailVerified: p.emailVerified,
    phoneVerified: false,
    notifEmail: prefs.notifEmail ?? true,
    notifSms: prefs.notifSms ?? true,
    notifCampaign: prefs.notifCampaign ?? true,
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshSessionUser = useCallback(async () => {
    try {
      const me = await authApi.currentUser();
      setUser(mapCurrentUser(me));
    } catch {
      clearAuthTokens();
      setUser(null);
    }
  }, []);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      const token = localStorage.getItem("gulora_store_access_token");
      if (!token) {
        if (!cancelled) setIsLoading(false);
        return;
      }
      try {
        const me = await authApi.currentUser();
        if (!cancelled) setUser(mapCurrentUser(me));
      } catch {
        clearAuthTokens();
        if (!cancelled) setUser(null);
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const login = async (email: string, password: string) => {
    setIsLoading(true);
    try {
      const auth = await authApi.login(email, password);
      setAuthTokens(auth.accessToken, auth.refreshToken);
      const me = await authApi.currentUser();
      setUser(mapCurrentUser(me));
      return { ok: true as const };
    } catch (e) {
      const msg =
        e instanceof ApiError
          ? e.status === 401
            ? "E-poçt və ya şifrə yanlışdır."
            : e.message
          : "Giriş mümkün olmadı.";
      return { ok: false as const, error: msg };
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (data: RegisterData) => {
    setIsLoading(true);
    try {
      const auth = await authApi.register({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
        phoneNumber: data.phone.trim() || null,
      });
      setAuthTokens(auth.accessToken, auth.refreshToken);
      const me = await authApi.currentUser();
      setUser(mapCurrentUser(me));
      return { ok: true as const };
    } catch (e) {
      const msg =
        e instanceof ApiError
          ? e.status === 409
            ? "Bu e-poçt artıq qeydiyyatdan keçib."
            : e.message
          : "Qeydiyyat tamamlanmadı.";
      return { ok: false as const, error: msg };
    } finally {
      setIsLoading(false);
    }
  };

  const sendOtp = async (phone: string) => {
    try {
      const r = await authApi.sendOtp(phone);
      return { ok: true as const, message: r.message ?? undefined };
    } catch {
      return { ok: false as const };
    }
  };

  const verifyOtp = async (phone: string, code: string) => {
    try {
      const r = await authApi.verifyOtp(phone, code);
      if (!r.isVerified) {
        return { ok: false as const, error: r.message ?? "OTP təsdiqlənmədi." };
      }
      await refreshSessionUser();
      return { ok: true as const };
    } catch (e) {
      return {
        ok: false as const,
        error: e instanceof ApiError ? e.message : "OTP təsdiqlənmədi.",
      };
    }
  };

  const logout = async () => {
    const rt = localStorage.getItem("gulora_store_refresh_token");
    if (rt) {
      try {
        await authApi.logout(rt);
      } catch {
        /* ignore */
      }
    }
    clearAuthTokens();
    setUser(null);
  };

  const updateProfile = (updates: Partial<AuthUser>) => {
    if (!user) return;
    const syncPrefs =
      updates.notifEmail !== undefined ||
      updates.notifSms !== undefined ||
      updates.notifCampaign !== undefined;

    if (syncPrefs) {
      const prefs = {
        notifEmail: updates.notifEmail ?? user.notifEmail,
        notifSms: updates.notifSms ?? user.notifSms,
        notifCampaign: updates.notifCampaign ?? user.notifCampaign,
      };
      savePrefs(prefs);
      setUser({ ...user, ...updates });
      return;
    }

    const fn = updates.firstName ?? user.firstName;
    const ln = updates.lastName ?? user.lastName;
    const phone = updates.phone ?? user.phone;

    setUser({ ...user, ...updates });

    void userApi
      .updateProfile(fn, ln, phone.trim() || null)
      .then((p) => setUser(mapProfile(p)))
      .catch(() => refreshSessionUser());
  };

  const changePassword = async (oldPass: string, newPass: string) => {
    try {
      await userApi.changePassword(oldPass, newPass);
      return { ok: true as const };
    } catch (e) {
      return {
        ok: false as const,
        error: e instanceof ApiError ? e.message : "Şifrə dəyişdirilmədi.",
      };
    }
  };

  const sendPasswordReset = async (email: string) => {
    try {
      await authApi.forgotPassword(email);
      return { ok: true as const };
    } catch (e) {
      return {
        ok: false as const,
        error: e instanceof ApiError ? e.message : "Sorğu göndərilmədi.",
      };
    }
  };

  const confirmPasswordReset = async (token: string, newPass: string) => {
    try {
      await authApi.resetPassword(token, newPass, newPass);
      return { ok: true as const };
    } catch (e) {
      return {
        ok: false as const,
        error: e instanceof ApiError ? e.message : "Token etibarsızdır və ya müddəti bitib.",
      };
    }
  };

  const deleteAccount = () => {
    clearAuthTokens();
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        login,
        register,
        sendOtp,
        verifyOtp,
        logout,
        updateProfile,
        changePassword,
        sendPasswordReset,
        confirmPasswordReset,
        deleteAccount,
        refreshSessionUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export type { AuthUser, RegisterData } from "./auth-context";
