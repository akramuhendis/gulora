import { useState } from "react";
import { MapPin, Plus, Pencil, Trash2, Star, X } from "lucide-react";
import { useAddresses } from "../../hooks/useAddresses";
import type { Address } from "../../context/address-context";
import { cities } from "../../data/cities";

interface AddressFormProps {
  initial?: Partial<Address>;
  onSave: (data: Omit<Address, "id">) => void;
  onCancel: () => void;
}

function AddressForm({ initial, onSave, onCancel }: AddressFormProps) {
  const [title, setTitle] = useState(initial?.title ?? "");
  const [recipientName, setRecipientName] = useState(initial?.recipientName ?? "");
  const [recipientPhone, setRecipientPhone] = useState(initial?.recipientPhone ?? "");
  const [city, setCity] = useState(initial?.city ?? "Bakı");
  const [district, setDistrict] = useState(initial?.district ?? "");
  const [line, setLine] = useState(initial?.line ?? "");
  const [postalCode, setPostalCode] = useState(initial?.postalCode ?? "");
  const [isDefault, setIsDefault] = useState(initial?.isDefault ?? false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const cityObj = cities.find((c) => c.name === city);
  const districts = cityObj?.districts ?? [];

  const validate = () => {
    const e: Record<string, string> = {};
    if (!title.trim()) e.title = "Başlıq tələb olunur.";
    if (!recipientName.trim()) e.recipientName = "Ad tələb olunur.";
    if (recipientPhone.replace(/\D/g, "").length < 9) e.recipientPhone = "Telefon tələb olunur.";
    if (!city) e.city = "Şəhər seçin.";
    if (!district) e.district = "Rayon seçin.";
    if (line.trim().length < 5) e.line = "Tam ünvanı daxil edin.";
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSave = () => {
    if (!validate()) return;
    onSave({ title, recipientName, recipientPhone, city, district, line, postalCode, isDefault });
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4 overflow-y-auto">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md my-4">
        <div className="flex items-center justify-between p-5 border-b border-gray-100">
          <h3 className="font-bold text-gray-800">{initial?.id ? "Ünvanı redaktə et" : "Yeni ünvan əlavə et"}</h3>
          <button onClick={onCancel}><X size={18} className="text-gray-400" /></button>
        </div>
        <div className="p-5 space-y-4">
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1.5">Başlıq (Məs. Ev, Ofis) *</label>
            <input type="text" value={title} onChange={(e) => setTitle(e.target.value)}
              className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] ${errors.title ? "border-red-300" : "border-gray-200"}`}
              placeholder="Ev" />
            {errors.title && <p className="text-xs text-red-500 mt-1">{errors.title}</p>}
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Alıcı adı *</label>
              <input type="text" value={recipientName} onChange={(e) => setRecipientName(e.target.value)}
                className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] ${errors.recipientName ? "border-red-300" : "border-gray-200"}`}
                placeholder="Ad Soyad" />
              {errors.recipientName && <p className="text-xs text-red-500 mt-1">{errors.recipientName}</p>}
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Telefon *</label>
              <input type="tel" value={recipientPhone} onChange={(e) => setRecipientPhone(e.target.value)}
                className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] ${errors.recipientPhone ? "border-red-300" : "border-gray-200"}`}
                placeholder="+994 50 ..." />
              {errors.recipientPhone && <p className="text-xs text-red-500 mt-1">{errors.recipientPhone}</p>}
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Şəhər *</label>
              <select value={city} onChange={(e) => { setCity(e.target.value); setDistrict(""); }}
                className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] bg-white ${errors.city ? "border-red-300" : "border-gray-200"}`}>
                {cities.map((c) => <option key={c.name} value={c.name}>{c.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Rayon *</label>
              <select value={district} onChange={(e) => setDistrict(e.target.value)}
                className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] bg-white ${errors.district ? "border-red-300" : "border-gray-200"}`}>
                <option value="">Seçin</option>
                {districts.map((d) => <option key={d.name} value={d.name}>{d.name}</option>)}
              </select>
              {errors.district && <p className="text-xs text-red-500 mt-1">{errors.district}</p>}
            </div>
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1.5">Küçə, bina, mənzil *</label>
            <textarea value={line} onChange={(e) => setLine(e.target.value)} rows={2}
              className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] resize-none ${errors.line ? "border-red-300" : "border-gray-200"}`}
              placeholder="Nizami küç. 203, m.4, mənzil 12" />
            {errors.line && <p className="text-xs text-red-500 mt-1">{errors.line}</p>}
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1.5">Poçt indeksi</label>
            <input type="text" value={postalCode} onChange={(e) => setPostalCode(e.target.value)}
              className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
              placeholder="AZ 1000" />
          </div>
          <label className="flex items-center gap-2.5 cursor-pointer">
            <input type="checkbox" checked={isDefault} onChange={(e) => setIsDefault(e.target.checked)} className="accent-[#65a30d]" />
            <span className="text-sm text-gray-700">Defolt ünvan kimi qeyd et</span>
          </label>
        </div>
        <div className="flex gap-3 p-5 border-t border-gray-100">
          <button onClick={handleSave} className="flex-1 bg-[#65a30d] text-white py-2.5 rounded-xl text-sm font-semibold hover:bg-[#4d7c0f]">
            Yadda saxla
          </button>
          <button onClick={onCancel} className="flex-1 border border-gray-200 text-gray-600 py-2.5 rounded-xl text-sm font-semibold hover:bg-gray-50">
            Ləğv et
          </button>
        </div>
      </div>
    </div>
  );
}

export default function AccountAddresses() {
  const { addresses, addAddress, updateAddress, deleteAddress, setDefaultAddress } = useAddresses();
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const handleAdd = async (data: Omit<Address, "id">) => {
    await addAddress(data);
    setShowForm(false);
  };

  const handleEdit = async (data: Omit<Address, "id">) => {
    if (!editingId) return;
    await updateAddress(editingId, data);
    setEditingId(null);
  };

  const editingAddress = editingId ? addresses.find((a) => a.id === editingId) : undefined;

  return (
    <>
      {showForm && <AddressForm onSave={handleAdd} onCancel={() => setShowForm(false)} />}
      {editingId && <AddressForm initial={editingAddress} onSave={handleEdit} onCancel={() => setEditingId(null)} />}

      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-800">Ünvanlarım</h1>
            <p className="text-sm text-gray-500 mt-1">Çatdırılma ünvanlarınızı idarə edin.</p>
          </div>
          <button type="button" onClick={() => setShowForm(true)}
            className="flex items-center gap-2 bg-[#65a30d] text-white px-4 py-2 rounded-xl text-sm font-semibold hover:bg-[#4d7c0f]">
            <Plus size={16} /> Yeni ünvan
          </button>
        </div>

        {addresses.length === 0 ? (
          <div className="bg-white rounded-2xl border border-gray-100 p-12 text-center">
            <MapPin className="mx-auto text-gray-300 mb-3" size={40} />
            <p className="text-gray-600 font-medium">Hələ ünvan əlavə edilməyib.</p>
            <button onClick={() => setShowForm(true)} className="mt-4 inline-flex items-center gap-2 bg-[#65a30d] text-white px-4 py-2 rounded-xl text-sm font-semibold hover:bg-[#4d7c0f]">
              <Plus size={16} /> Ünvan əlavə et
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {addresses.map((addr) => (
              <div key={addr.id} className={`bg-white rounded-2xl border p-4 relative ${addr.isDefault ? "border-[#65a30d]" : "border-gray-100"}`}>
                {addr.isDefault && (
                  <span className="absolute top-3 right-3 flex items-center gap-1 text-[10px] text-[#65a30d] font-semibold">
                    <Star size={10} fill="currentColor" /> Defolt
                  </span>
                )}
                <div className="flex items-center gap-2 mb-2">
                  <div className="w-8 h-8 bg-lime-50 rounded-xl flex items-center justify-center">
                    <MapPin size={16} className="text-[#65a30d]" />
                  </div>
                  <span className="font-semibold text-gray-800">{addr.title}</span>
                </div>
                <div className="space-y-0.5 text-sm text-gray-600 mb-4">
                  <p className="font-medium text-gray-800">{addr.recipientName}</p>
                  <p>{addr.recipientPhone}</p>
                  <p>{addr.city}, {addr.district}</p>
                  <p className="text-gray-500">{addr.line}</p>
                  {addr.postalCode && <p className="text-gray-400 text-xs">{addr.postalCode}</p>}
                </div>
                <div className="flex flex-wrap gap-2">
                  {!addr.isDefault && (
                    <button type="button" onClick={() => void setDefaultAddress(addr.id)}
                      className="text-xs text-gray-500 border border-gray-200 px-3 py-1.5 rounded-xl hover:border-lime-300 hover:text-[#65a30d]">
                      Defolt et
                    </button>
                  )}
                  <button type="button" onClick={() => setEditingId(addr.id)}
                    className="flex items-center gap-1 text-xs text-gray-500 border border-gray-200 px-3 py-1.5 rounded-xl hover:border-blue-300 hover:text-blue-600">
                    <Pencil size={11} /> Redaktə
                  </button>
                  <button type="button" onClick={() => void deleteAddress(addr.id)}
                    className="flex items-center gap-1 text-xs text-red-500 border border-red-100 px-3 py-1.5 rounded-xl hover:bg-red-50">
                    <Trash2 size={11} /> Sil
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </>
  );
}
