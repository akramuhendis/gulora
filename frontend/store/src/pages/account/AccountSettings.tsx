import { useState, type FormEvent } from "react";
import { Bell, Lock, Mail, MessageSquare, Megaphone, Trash2, Eye, EyeOff, CheckCircle, AlertTriangle } from "lucide-react";
import { useAuth } from "../../hooks/useAuth";
import { useNavigate } from "react-router-dom";

export default function AccountSettings() {
  const { user, changePassword, updateProfile, deleteAccount } = useAuth();
  const navigate = useNavigate();

  const [notifEmail, setNotifEmail] = useState(user?.notifEmail ?? true);
  const [notifSms, setNotifSms] = useState(user?.notifSms ?? true);
  const [notifCampaign, setNotifCampaign] = useState(user?.notifCampaign ?? true);
  const [notifSaved, setNotifSaved] = useState(false);

  const [currentPw, setCurrentPw] = useState("");
  const [newPw, setNewPw] = useState("");
  const [newPw2, setNewPw2] = useState("");
  const [showPw, setShowPw] = useState(false);
  const [pwError, setPwError] = useState("");
  const [pwSuccess, setPwSuccess] = useState(false);

  const [showDelete, setShowDelete] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState("");

  const handleNotifSave = () => {
    updateProfile({ notifEmail, notifSms, notifCampaign });
    setNotifSaved(true);
    setTimeout(() => setNotifSaved(false), 2000);
  };

  const handlePasswordChange = async (e: FormEvent) => {
    e.preventDefault();
    setPwError("");
    if (newPw.length < 6) { setPwError("Yeni şifrə ən azı 6 simvol olmalıdır."); return; }
    if (newPw !== newPw2) { setPwError("Şifrələr uyğun gəlmir."); return; }
    const res = await changePassword(currentPw, newPw);
    if (res.ok) {
      setPwSuccess(true);
      setCurrentPw(""); setNewPw(""); setNewPw2("");
      setTimeout(() => setPwSuccess(false), 3000);
    } else {
      setPwError(res.error ?? "Xəta");
    }
  };

  const handleDeleteAccount = () => {
    if (deleteConfirm !== "SİL") return;
    deleteAccount();
    navigate("/", { replace: true });
  };

  return (
    <div className="space-y-8 max-w-xl">
      <div>
        <h1 className="text-2xl font-bold text-gray-800">Tənzimləmələr</h1>
        <p className="text-sm text-gray-500 mt-1">Bildirişlər, təhlükəsizlik və hesab idarəetmə.</p>
      </div>

      {/* Notifications */}
      <section className="bg-white rounded-2xl border border-gray-100 p-5">
        <h2 className="font-semibold text-gray-800 flex items-center gap-2 mb-4">
          <Bell size={18} className="text-[#65a30d]" /> Bildirişlər
        </h2>
        <div className="space-y-1">
          {[
            { icon: Mail, label: "Sifariş statusu (e-poçt)", value: notifEmail, setter: setNotifEmail },
            { icon: MessageSquare, label: "Sifariş statusu (SMS)", value: notifSms, setter: setNotifSms },
            { icon: Megaphone, label: "Kampaniya və endirim xəbərləri", value: notifCampaign, setter: setNotifCampaign },
          ].map(({ icon: Icon, label, value, setter }) => (
            <label key={label} className="flex items-center justify-between gap-4 py-3 border-b border-gray-50 cursor-pointer last:border-0">
              <span className="text-sm text-gray-700 flex items-center gap-2">
                <Icon size={16} className="text-gray-400" /> {label}
              </span>
              <button type="button" onClick={() => setter(!value)}
                className={`relative w-10 h-5.5 rounded-full transition-colors ${value ? "bg-[#65a30d]" : "bg-gray-200"}`}
                style={{ minWidth: 40, height: 22 }}>
                <span className={`absolute top-0.5 w-4 h-4 bg-white rounded-full shadow transition-transform ${value ? "translate-x-5" : "translate-x-0.5"}`} />
              </button>
            </label>
          ))}
        </div>
        <button type="button" onClick={handleNotifSave}
          className="mt-4 px-4 py-2 bg-[#65a30d] text-white text-sm font-semibold rounded-xl hover:bg-[#4d7c0f]">
          Saxla
        </button>
        {notifSaved && <span className="ml-3 text-sm text-green-600 inline-flex items-center gap-1"><CheckCircle size={14} /> Saxlanıldı</span>}
      </section>

      {/* Password */}
      <section className="bg-white rounded-2xl border border-gray-100 p-5">
        <h2 className="font-semibold text-gray-800 flex items-center gap-2 mb-4">
          <Lock size={18} className="text-[#65a30d]" /> Şifrəni dəyiş
        </h2>
        <form onSubmit={handlePasswordChange} className="space-y-3">
          <div>
            <label className="block text-xs font-semibold text-gray-600 mb-1.5">Cari şifrə</label>
            <div className="relative">
              <input type={showPw ? "text" : "password"} value={currentPw} onChange={(e) => setCurrentPw(e.target.value)}
                className="w-full border border-gray-200 rounded-xl px-3 pr-10 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                autoComplete="current-password" />
              <button type="button" onClick={() => setShowPw(!showPw)} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400">
                {showPw ? <EyeOff size={15} /> : <Eye size={15} />}
              </button>
            </div>
          </div>
          <div>
            <label className="block text-xs font-semibold text-gray-600 mb-1.5">Yeni şifrə</label>
            <input type={showPw ? "text" : "password"} value={newPw} onChange={(e) => setNewPw(e.target.value)}
              className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
              autoComplete="new-password" placeholder="Ən azı 6 simvol" />
          </div>
          <div>
            <label className="block text-xs font-semibold text-gray-600 mb-1.5">Yeni şifrə (təkrar)</label>
            <input type={showPw ? "text" : "password"} value={newPw2} onChange={(e) => setNewPw2(e.target.value)}
              className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
              autoComplete="new-password" />
          </div>
          {pwError && <p className="text-xs text-red-500">{pwError}</p>}
          {pwSuccess && <p className="text-xs text-green-600 flex items-center gap-1"><CheckCircle size={12} /> Şifrə dəyişdirildi.</p>}
          <button type="submit" className="px-5 py-2.5 bg-gray-800 text-white text-sm font-semibold rounded-xl hover:bg-gray-700">
            Şifrəni yenilə
          </button>
        </form>
      </section>

      {/* Delete account */}
      <section className="bg-white rounded-2xl border border-red-100 p-5">
        <h2 className="font-semibold text-red-700 flex items-center gap-2 mb-3">
          <AlertTriangle size={18} /> Hesabı sil
        </h2>
        <p className="text-sm text-gray-600 mb-4">
          Hesabınızı silsəniz bütün məlumatlarınız (sifarişlər, ünvanlar, seçilmişlər) birdəfəlik silinəcək. Bu əməliyyat geri qaytarıla bilməz.
        </p>
        {!showDelete ? (
          <button type="button" onClick={() => setShowDelete(true)}
            className="flex items-center gap-2 text-sm text-red-600 border border-red-200 px-4 py-2 rounded-xl hover:bg-red-50">
            <Trash2 size={14} /> Hesabı sil
          </button>
        ) : (
          <div className="space-y-3">
            <p className="text-sm text-red-700 font-medium">Davam etmək üçün aşağıya "SİL" yazın:</p>
            <input type="text" value={deleteConfirm} onChange={(e) => setDeleteConfirm(e.target.value.toUpperCase())}
              className="w-full border border-red-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-red-400 font-bold tracking-widest"
              placeholder="SİL" />
            <div className="flex gap-2">
              <button type="button" onClick={handleDeleteAccount} disabled={deleteConfirm !== "SİL"}
                className="flex-1 bg-red-600 text-white py-2.5 rounded-xl text-sm font-semibold hover:bg-red-700 disabled:opacity-40">
                Hesabı sil
              </button>
              <button type="button" onClick={() => { setShowDelete(false); setDeleteConfirm(""); }}
                className="flex-1 border border-gray-200 text-gray-600 py-2.5 rounded-xl text-sm font-semibold hover:bg-gray-50">
                Ləğv et
              </button>
            </div>
          </div>
        )}
      </section>
    </div>
  );
}
