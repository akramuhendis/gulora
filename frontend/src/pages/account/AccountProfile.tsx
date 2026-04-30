import { useState, type FormEvent } from "react";
import { Save, CheckCircle, Phone } from "lucide-react";
import { useAuth } from "../../context/AuthContext";

export default function AccountProfile() {
  const { user, updateProfile, sendOtp, verifyOtp } = useAuth();

  const [firstName, setFirstName] = useState(user?.firstName ?? "");
  const [lastName, setLastName] = useState(user?.lastName ?? "");
  const [email, setEmail] = useState(user?.email ?? "");
  const [phone, setPhone] = useState(user?.phone ?? "");
  const [saved, setSaved] = useState(false);

  const [otpSent, setOtpSent] = useState(false);
  const [otpCode, setOtpCode] = useState("");
  const [otpError, setOtpError] = useState("");
  const [phoneVerified, setPhoneVerified] = useState(user?.phoneVerified ?? false);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!user) return;
    updateProfile({ firstName: firstName.trim(), lastName: lastName.trim(), email: email.trim() });
    setSaved(true);
    setTimeout(() => setSaved(false), 2500);
  };

  const handleSendOtp = async () => {
    await sendOtp(phone.trim());
    setOtpSent(true);
    setOtpError("");
  };

  const handleVerifyOtp = async () => {
    const res = await verifyOtp(phone.trim(), otpCode.trim());
    if (res.ok) {
      setPhoneVerified(true);
      setOtpSent(false);
      setOtpCode("");
    } else {
      setOtpError(res.error ?? "Xəta");
    }
  };

  return (
    <div className="space-y-6 max-w-xl">
      <div>
        <h1 className="text-2xl font-bold text-gray-800">Şəxsi məlumat</h1>
        <p className="text-sm text-gray-500 mt-1">Ad, əlaqə və hesab məlumatlarınızı yeniləyin.</p>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-gray-100 p-5 space-y-4">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-xs font-semibold text-gray-600 mb-1.5">Ad</label>
            <input type="text" value={firstName} onChange={(e) => setFirstName(e.target.value)}
              className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]" />
          </div>
          <div>
            <label className="block text-xs font-semibold text-gray-600 mb-1.5">Soyad</label>
            <input type="text" value={lastName} onChange={(e) => setLastName(e.target.value)}
              className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]" />
          </div>
        </div>
        <div>
          <div className="flex items-center justify-between mb-1.5">
            <label className="text-xs font-semibold text-gray-600">E-poçt</label>
            {user?.emailVerified ? (
              <span className="text-xs text-green-600 flex items-center gap-1"><CheckCircle size={11} /> Təsdiqlənib</span>
            ) : (
              <span className="text-xs text-amber-600">Təsdiqlənməyib</span>
            )}
          </div>
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)}
            className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]" />
        </div>
        <div>
          <div className="flex items-center justify-between mb-1.5">
            <label className="text-xs font-semibold text-gray-600">Mobil nömrə</label>
            {phoneVerified ? (
              <span className="text-xs text-green-600 flex items-center gap-1"><CheckCircle size={11} /> Təsdiqlənib</span>
            ) : (
              <button type="button" onClick={handleSendOtp} className="text-xs text-[#65a30d] hover:underline flex items-center gap-1">
                <Phone size={11} /> OTP ilə təsdiqlə
              </button>
            )}
          </div>
          <input type="tel" value={phone} onChange={(e) => setPhone(e.target.value)}
            className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
            placeholder="+994 50 123 45 67" />
          {otpSent && (
            <div className="mt-2 flex gap-2">
              <input type="text" value={otpCode} onChange={(e) => setOtpCode(e.target.value.replace(/\D/g, ""))}
                placeholder="6 rəqəmli kod" maxLength={6}
                className="flex-1 border border-gray-200 rounded-xl px-3 py-2 text-sm text-center font-mono outline-none focus:border-[#65a30d]" />
              <button type="button" onClick={handleVerifyOtp} className="px-3 py-2 bg-[#65a30d] text-white rounded-xl text-sm font-semibold">
                Təsdiqlə
              </button>
            </div>
          )}
          {otpError && <p className="text-xs text-red-500 mt-1">{otpError}</p>}
          {otpSent && <p className="text-xs text-amber-600 mt-1">Demo: OTP kodu brauzerin konsolundadır.</p>}
        </div>

        {user?.memberSince && (
          <div className="text-xs text-gray-400">
            Üzv olma tarixi: {new Date(user.memberSince).toLocaleDateString("az-AZ", { day: "numeric", month: "long", year: "numeric" })}
          </div>
        )}

        <button type="submit" className="inline-flex items-center gap-2 px-5 py-2.5 bg-[#65a30d] text-white text-sm font-semibold rounded-xl hover:bg-[#4d7c0f] transition-colors">
          <Save size={16} /> Saxla
        </button>
        {saved && <p className="text-sm text-green-600 font-medium flex items-center gap-1"><CheckCircle size={14} /> Dəyişikliklər saxlanıldı.</p>}
      </form>
    </div>
  );
}
