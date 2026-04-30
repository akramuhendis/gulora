import { useState, type FormEvent } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { Eye, EyeOff, Mail, Lock, User, Phone, ArrowLeft, CheckCircle, Loader2 } from "lucide-react";
import { useAuth } from "../context/AuthContext";

type Tab = "login" | "register";

interface OtpModalProps {
  phone: string;
  onClose: () => void;
  onVerified: () => void;
}

function OtpModal({ phone, onClose, onVerified }: OtpModalProps) {
  const { sendOtp, verifyOtp } = useAuth();
  const [code, setCode] = useState("");
  const [sent, setSent] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  const handleSend = async () => {
    setLoading(true);
    await sendOtp(phone);
    setSent(true);
    setLoading(false);
  };

  const handleVerify = async () => {
    setLoading(true);
    const res = await verifyOtp(phone, code.trim());
    setLoading(false);
    if (res.ok) {
      setSuccess(true);
      setTimeout(onVerified, 1200);
    } else {
      setError(res.error ?? "Xəta");
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-xl p-6 w-full max-w-sm">
        {success ? (
          <div className="text-center py-4">
            <CheckCircle size={48} className="text-green-500 mx-auto mb-3" />
            <p className="font-semibold text-gray-800">Nömrə təsdiqləndi!</p>
          </div>
        ) : (
          <>
            <h3 className="font-bold text-lg text-gray-800 mb-1">Telefon doğrulama</h3>
            <p className="text-sm text-gray-500 mb-4">
              {phone} nömrəsinə OTP kodu göndəriləcək.
              <br />
              <span className="text-xs text-amber-600">Demo: kodu brauzerin konsolunda görəcəksiniz.</span>
            </p>
            {!sent ? (
              <button
                onClick={handleSend}
                disabled={loading}
                className="w-full bg-[#65a30d] text-white py-2.5 rounded-xl font-semibold hover:bg-[#4d7c0f] disabled:opacity-60 flex items-center justify-center gap-2"
              >
                {loading && <Loader2 size={16} className="animate-spin" />}
                OTP Göndər
              </button>
            ) : (
              <div className="space-y-3">
                <input
                  type="text"
                  maxLength={6}
                  value={code}
                  onChange={(e) => setCode(e.target.value.replace(/\D/g, ""))}
                  placeholder="6 rəqəmli kod"
                  className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-center text-lg font-bold tracking-widest outline-none focus:border-[#65a30d]"
                />
                {error && <p className="text-xs text-red-500">{error}</p>}
                <button
                  onClick={handleVerify}
                  disabled={loading || code.length !== 6}
                  className="w-full bg-[#65a30d] text-white py-2.5 rounded-xl font-semibold hover:bg-[#4d7c0f] disabled:opacity-60 flex items-center justify-center gap-2"
                >
                  {loading && <Loader2 size={16} className="animate-spin" />}
                  Təsdiqlə
                </button>
                <button
                  onClick={handleSend}
                  disabled={loading}
                  className="w-full text-sm text-gray-500 hover:text-[#65a30d]"
                >
                  Yenidən göndər
                </button>
              </div>
            )}
            <button onClick={onClose} className="mt-3 w-full text-sm text-gray-400 hover:text-gray-600">
              Bağla
            </button>
          </>
        )}
      </div>
    </div>
  );
}

export default function LoginPage() {
  const { login, register, loginWithGoogle, loginWithApple, isLoading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: string })?.from ?? "/hesabim";

  const [tab, setTab] = useState<Tab>("login");
  const [showPass, setShowPass] = useState(false);
  const [showPass2, setShowPass2] = useState(false);
  const [error, setError] = useState("");
  const [otpPhone, setOtpPhone] = useState<string | null>(null);
  const [pendingUser, setPendingUser] = useState<{ name: string } | null>(null);

  const [loginEmail, setLoginEmail] = useState("");
  const [loginPass, setLoginPass] = useState("");

  const [regFirst, setRegFirst] = useState("");
  const [regLast, setRegLast] = useState("");
  const [regEmail, setRegEmail] = useState("");
  const [regPhone, setRegPhone] = useState("");
  const [regPass, setRegPass] = useState("");
  const [regPass2, setRegPass2] = useState("");
  const [regKvkk, setRegKvkk] = useState(false);
  const [regCampaign, setRegCampaign] = useState(false);

  const handleLogin = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    if (!loginEmail.trim() || !loginPass) { setError("E-poçt və şifrə tələb olunur."); return; }
    const res = await login(loginEmail.trim(), loginPass);
    if (res.ok) navigate(from, { replace: true });
    else setError(res.error ?? "Xəta baş verdi.");
  };

  const handleRegister = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    if (!regFirst.trim() || !regLast.trim()) { setError("Ad və soyad tələb olunur."); return; }
    if (!regEmail.trim()) { setError("E-poçt tələb olunur."); return; }
    if (regPass.length < 6) { setError("Şifrə ən azı 6 simvol olmalıdır."); return; }
    if (regPass !== regPass2) { setError("Şifrələr uyğun gəlmir."); return; }
    if (!regKvkk) { setError("Məxfilik siyasətini qəbul etməlisiniz."); return; }
    const res = await register({
      firstName: regFirst.trim(),
      lastName: regLast.trim(),
      email: regEmail.trim(),
      phone: regPhone.trim(),
      password: regPass,
    });
    if (res.ok) {
      setPendingUser({ name: regFirst });
      if (regPhone.trim().length >= 9) setOtpPhone(regPhone.trim());
      else navigate(from, { replace: true });
    } else {
      setError(res.error ?? "Xəta baş verdi.");
    }
  };

  const handleGoogleLogin = async () => {
    setError("");
    await loginWithGoogle();
    navigate(from, { replace: true });
  };

  const handleAppleLogin = async () => {
    setError("");
    await loginWithApple();
    navigate(from, { replace: true });
  };

  if (pendingUser && !otpPhone) {
    return (
      <div className="min-h-[60vh] flex items-center justify-center px-4">
        <div className="bg-white rounded-2xl border border-gray-100 p-8 max-w-sm w-full text-center shadow-sm">
          <div className="text-5xl mb-4">🎉</div>
          <h2 className="text-xl font-bold text-gray-800 mb-2">Xoş gəldiniz, {pendingUser.name}!</h2>
          <p className="text-sm text-gray-500 mb-5">Hesabınız uğurla yaradıldı.</p>
          <Link to={from} className="inline-block bg-[#65a30d] text-white px-6 py-2.5 rounded-xl font-semibold hover:bg-[#4d7c0f] transition-colors">
            Davam et
          </Link>
        </div>
      </div>
    );
  }

  return (
    <>
      {otpPhone && (
        <OtpModal
          phone={otpPhone}
          onClose={() => { setOtpPhone(null); navigate(from, { replace: true }); }}
          onVerified={() => { setOtpPhone(null); navigate(from, { replace: true }); }}
        />
      )}
      <div className="min-h-[80vh] flex items-start justify-center px-4 py-10">
        <div className="w-full max-w-md">
          <Link to="/" className="inline-flex items-center gap-1.5 text-sm text-gray-500 hover:text-[#65a30d] mb-6">
            <ArrowLeft size={16} /> Ana səhifə
          </Link>

          <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
            <div className="flex border-b border-gray-100">
              {(["login", "register"] as Tab[]).map((t) => (
                <button
                  key={t}
                  type="button"
                  onClick={() => { setTab(t); setError(""); }}
                  className={`flex-1 py-3.5 text-sm font-semibold transition-colors ${
                    tab === t ? "bg-[#65a30d] text-white" : "text-gray-600 hover:bg-gray-50"
                  }`}
                >
                  {t === "login" ? "Giriş" : "Qeydiyyat"}
                </button>
              ))}
            </div>

            <div className="p-6">
              <div className="grid grid-cols-2 gap-3 mb-5">
                <button
                  type="button"
                  onClick={handleGoogleLogin}
                  disabled={isLoading}
                  className="flex items-center justify-center gap-2 border border-gray-200 rounded-xl py-2.5 text-sm font-medium text-gray-700 hover:border-gray-300 hover:bg-gray-50 transition-colors disabled:opacity-60"
                >
                  <svg width="18" height="18" viewBox="0 0 24 24"><path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/><path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/><path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z"/><path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/></svg>
                  Google
                </button>
                <button
                  type="button"
                  onClick={handleAppleLogin}
                  disabled={isLoading}
                  className="flex items-center justify-center gap-2 border border-gray-200 rounded-xl py-2.5 text-sm font-medium text-gray-700 hover:border-gray-300 hover:bg-gray-50 transition-colors disabled:opacity-60"
                >
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor"><path d="M18.71 19.5c-.83 1.24-1.71 2.45-3.05 2.47-1.34.03-1.77-.79-3.29-.79-1.53 0-2 .77-3.27.82-1.31.05-2.3-1.32-3.14-2.53C4.25 17 2.94 12.45 4.7 9.39c.87-1.52 2.43-2.48 4.12-2.51 1.28-.02 2.5.87 3.29.87.78 0 2.26-1.07 3.8-.91.65.03 2.47.26 3.64 1.98-.09.06-2.17 1.28-2.15 3.81.03 3.02 2.65 4.03 2.68 4.04-.03.07-.42 1.44-1.38 2.83M13 3.5c.73-.83 1.94-1.46 2.94-1.5.13 1.17-.34 2.35-1.04 3.19-.69.85-1.83 1.51-2.95 1.42-.15-1.15.41-2.35 1.05-3.11z"/></svg>
                  Apple
                </button>
              </div>

              <div className="flex items-center gap-3 mb-5">
                <div className="flex-1 h-px bg-gray-100" />
                <span className="text-xs text-gray-400 font-medium">və ya</span>
                <div className="flex-1 h-px bg-gray-100" />
              </div>

              {error && (
                <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded-xl px-4 py-3 mb-4">
                  {error}
                </div>
              )}

              {tab === "login" ? (
                <form onSubmit={handleLogin} className="space-y-4">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">E-poçt</label>
                    <div className="relative">
                      <Mail size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                      <input
                        type="email"
                        value={loginEmail}
                        onChange={(e) => setLoginEmail(e.target.value)}
                        className="w-full border border-gray-200 rounded-xl pl-9 pr-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                        placeholder="ad@nümunə.az"
                        autoComplete="email"
                      />
                    </div>
                  </div>
                  <div>
                    <div className="flex items-center justify-between mb-1.5">
                      <label className="text-xs font-medium text-gray-600">Şifrə</label>
                      <Link to="/sifre-yenile" className="text-xs text-[#65a30d] hover:underline">
                        Şifrəni unutdum?
                      </Link>
                    </div>
                    <div className="relative">
                      <Lock size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                      <input
                        type={showPass ? "text" : "password"}
                        value={loginPass}
                        onChange={(e) => setLoginPass(e.target.value)}
                        className="w-full border border-gray-200 rounded-xl pl-9 pr-10 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                        placeholder="••••••••"
                        autoComplete="current-password"
                      />
                      <button
                        type="button"
                        onClick={() => setShowPass(!showPass)}
                        className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400"
                      >
                        {showPass ? <EyeOff size={15} /> : <Eye size={15} />}
                      </button>
                    </div>
                  </div>
                  <button
                    type="submit"
                    disabled={isLoading}
                    className="w-full bg-[#65a30d] text-white py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] transition-colors disabled:opacity-60 flex items-center justify-center gap-2"
                  >
                    {isLoading && <Loader2 size={16} className="animate-spin" />}
                    Giriş et
                  </button>
                </form>
              ) : (
                <form onSubmit={handleRegister} className="space-y-4">
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1.5">Ad *</label>
                      <div className="relative">
                        <User size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                        <input
                          type="text"
                          value={regFirst}
                          onChange={(e) => setRegFirst(e.target.value)}
                          className="w-full border border-gray-200 rounded-xl pl-9 pr-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                          placeholder="Aygün"
                          autoComplete="given-name"
                        />
                      </div>
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1.5">Soyad *</label>
                      <input
                        type="text"
                        value={regLast}
                        onChange={(e) => setRegLast(e.target.value)}
                        className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                        placeholder="Məmmədova"
                        autoComplete="family-name"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">E-poçt *</label>
                    <div className="relative">
                      <Mail size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                      <input
                        type="email"
                        value={regEmail}
                        onChange={(e) => setRegEmail(e.target.value)}
                        className="w-full border border-gray-200 rounded-xl pl-9 pr-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                        placeholder="ad@nümunə.az"
                        autoComplete="email"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">Telefon (OTP üçün)</label>
                    <div className="relative">
                      <Phone size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                      <input
                        type="tel"
                        value={regPhone}
                        onChange={(e) => setRegPhone(e.target.value)}
                        className="w-full border border-gray-200 rounded-xl pl-9 pr-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                        placeholder="+994 50 123 45 67"
                        autoComplete="tel"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">Şifrə *</label>
                    <div className="relative">
                      <Lock size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                      <input
                        type={showPass ? "text" : "password"}
                        value={regPass}
                        onChange={(e) => setRegPass(e.target.value)}
                        className="w-full border border-gray-200 rounded-xl pl-9 pr-10 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                        placeholder="Ən azı 6 simvol"
                        autoComplete="new-password"
                      />
                      <button type="button" onClick={() => setShowPass(!showPass)} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400">
                        {showPass ? <EyeOff size={15} /> : <Eye size={15} />}
                      </button>
                    </div>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">Şifrəni təkrarla *</label>
                    <div className="relative">
                      <Lock size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                      <input
                        type={showPass2 ? "text" : "password"}
                        value={regPass2}
                        onChange={(e) => setRegPass2(e.target.value)}
                        className="w-full border border-gray-200 rounded-xl pl-9 pr-10 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                        placeholder="••••••••"
                        autoComplete="new-password"
                      />
                      <button type="button" onClick={() => setShowPass2(!showPass2)} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400">
                        {showPass2 ? <EyeOff size={15} /> : <Eye size={15} />}
                      </button>
                    </div>
                  </div>
                  <div className="space-y-2.5">
                    <label className="flex items-start gap-2.5 cursor-pointer">
                      <input type="checkbox" checked={regKvkk} onChange={(e) => setRegKvkk(e.target.checked)} className="mt-0.5 accent-[#65a30d]" />
                      <span className="text-xs text-gray-600">
                        <Link to="/mexfilik" className="text-[#65a30d] underline" target="_blank">Məxfilik siyasəti</Link> və{" "}
                        <Link to="/istifade-sertleri" className="text-[#65a30d] underline" target="_blank">İstifadə şərtlərini</Link> oxuyub qəbul edirəm. *
                      </span>
                    </label>
                    <label className="flex items-start gap-2.5 cursor-pointer">
                      <input type="checkbox" checked={regCampaign} onChange={(e) => setRegCampaign(e.target.checked)} className="mt-0.5 accent-[#65a30d]" />
                      <span className="text-xs text-gray-600">Kampaniya və xüsusi təkliflərdən xəbərdar olmaq istəyirəm.</span>
                    </label>
                  </div>
                  <button
                    type="submit"
                    disabled={isLoading}
                    className="w-full bg-[#65a30d] text-white py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] transition-colors disabled:opacity-60 flex items-center justify-center gap-2"
                  >
                    {isLoading && <Loader2 size={16} className="animate-spin" />}
                    Qeydiyyatdan keç
                  </button>
                </form>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
