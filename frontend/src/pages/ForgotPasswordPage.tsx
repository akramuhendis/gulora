import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { ArrowLeft, Mail, Lock, Eye, EyeOff, CheckCircle, Loader2 } from "lucide-react";
import { useAuth } from "../context/AuthContext";

type Step = "email" | "token" | "success";

export default function ForgotPasswordPage() {
  const { sendPasswordReset, confirmPasswordReset } = useAuth();
  const [step, setStep] = useState<Step>("email");
  const [email, setEmail] = useState("");
  const [token, setToken] = useState("");
  const [newPass, setNewPass] = useState("");
  const [newPass2, setNewPass2] = useState("");
  const [showPass, setShowPass] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSendEmail = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    if (!email.trim()) { setError("E-poçt tələb olunur."); return; }
    setLoading(true);
    await sendPasswordReset(email.trim());
    setLoading(false);
    setStep("token");
  };

  const handleConfirm = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    if (!token.trim()) { setError("Token tələb olunur."); return; }
    if (newPass.length < 6) { setError("Şifrə ən azı 6 simvol olmalıdır."); return; }
    if (newPass !== newPass2) { setError("Şifrələr uyğun gəlmir."); return; }
    setLoading(true);
    const res = await confirmPasswordReset(token.trim(), newPass);
    setLoading(false);
    if (res.ok) setStep("success");
    else setError(res.error ?? "Xəta baş verdi.");
  };

  return (
    <div className="min-h-[70vh] flex items-start justify-center px-4 py-10">
      <div className="w-full max-w-md">
        <Link to="/giris" className="inline-flex items-center gap-1.5 text-sm text-gray-500 hover:text-[#65a30d] mb-6">
          <ArrowLeft size={16} /> Girişə qayıt
        </Link>

        <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
          {step === "success" ? (
            <div className="text-center py-6">
              <CheckCircle size={52} className="text-green-500 mx-auto mb-4" />
              <h2 className="text-xl font-bold text-gray-800 mb-2">Şifrə yeniləndi!</h2>
              <p className="text-sm text-gray-500 mb-5">Yeni şifrənizlə daxil ola bilərsiniz.</p>
              <Link to="/giris" className="inline-block bg-[#65a30d] text-white px-6 py-2.5 rounded-xl font-semibold hover:bg-[#4d7c0f]">
                Giriş et
              </Link>
            </div>
          ) : step === "email" ? (
            <>
              <div className="text-center mb-6">
                <div className="w-14 h-14 bg-lime-50 rounded-full flex items-center justify-center mx-auto mb-3">
                  <Mail size={26} className="text-[#65a30d]" />
                </div>
                <h2 className="text-xl font-bold text-gray-800">Şifrəni bərpa et</h2>
                <p className="text-sm text-gray-500 mt-1">E-poçtunuzu daxil edin, bərpa linki göndərəcəyik.</p>
              </div>
              {error && <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded-xl px-4 py-3 mb-4">{error}</div>}
              <form onSubmit={handleSendEmail} className="space-y-4">
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">E-poçt</label>
                  <div className="relative">
                    <Mail size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                    <input
                      type="email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      className="w-full border border-gray-200 rounded-xl pl-9 pr-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                      placeholder="ad@nümunə.az"
                    />
                  </div>
                </div>
                <button
                  type="submit"
                  disabled={loading}
                  className="w-full bg-[#65a30d] text-white py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] disabled:opacity-60 flex items-center justify-center gap-2"
                >
                  {loading && <Loader2 size={16} className="animate-spin" />}
                  Göndər
                </button>
              </form>
            </>
          ) : (
            <>
              <div className="text-center mb-6">
                <div className="w-14 h-14 bg-lime-50 rounded-full flex items-center justify-center mx-auto mb-3">
                  <Lock size={26} className="text-[#65a30d]" />
                </div>
                <h2 className="text-xl font-bold text-gray-800">Yeni şifrə</h2>
                <p className="text-sm text-gray-500 mt-1">
                  Token konsolda görünür (demo). Yeni şifrənizi daxil edin.
                </p>
              </div>
              {error && <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded-xl px-4 py-3 mb-4">{error}</div>}
              <form onSubmit={handleConfirm} className="space-y-4">
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Bərpa tokeni</label>
                  <input
                    type="text"
                    value={token}
                    onChange={(e) => setToken(e.target.value)}
                    className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] font-mono"
                    placeholder="Konsoldan kopyalayın"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Yeni şifrə</label>
                  <div className="relative">
                    <Lock size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                    <input
                      type={showPass ? "text" : "password"}
                      value={newPass}
                      onChange={(e) => setNewPass(e.target.value)}
                      className="w-full border border-gray-200 rounded-xl pl-9 pr-10 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                      placeholder="Ən azı 6 simvol"
                    />
                    <button type="button" onClick={() => setShowPass(!showPass)} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400">
                      {showPass ? <EyeOff size={15} /> : <Eye size={15} />}
                    </button>
                  </div>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Şifrəni təkrarla</label>
                  <div className="relative">
                    <Lock size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                    <input
                      type={showPass ? "text" : "password"}
                      value={newPass2}
                      onChange={(e) => setNewPass2(e.target.value)}
                      className="w-full border border-gray-200 rounded-xl pl-9 pr-10 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                      placeholder="••••••••"
                    />
                  </div>
                </div>
                <button
                  type="submit"
                  disabled={loading}
                  className="w-full bg-[#65a30d] text-white py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] disabled:opacity-60 flex items-center justify-center gap-2"
                >
                  {loading && <Loader2 size={16} className="animate-spin" />}
                  Şifrəni yenilə
                </button>
              </form>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
