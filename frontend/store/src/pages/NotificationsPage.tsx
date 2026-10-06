import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Bell, CheckCheck, Loader2 } from "lucide-react";
import { notificationsApi, ApiError } from "../services/api";
import { useAuth } from "../hooks/useAuth";
import { usePageMeta } from "../hooks/usePageMeta";

type NotificationRow = {
  id: string;
  title: string;
  body: string;
  isRead: boolean;
  createdAt: string;
};

export default function NotificationsPage() {
  usePageMeta("Bildirişlər");
  const { user } = useAuth();
  const [items, setItems] = useState<NotificationRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!user) return;
    setLoading(true);
    setError(null);
    try {
      const res = await notificationsApi.list(1, 50);
      setItems(
        res.items.map((n) => ({
          id: n.id,
          title: n.title,
          body: n.body,
          isRead: n.isRead,
          createdAt: n.createdAt,
        }))
      );
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Bildirişlər yüklənmədi.");
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    void load();
  }, [load]);

  const markAllRead = async () => {
    await notificationsApi.markRead();
    setItems((prev) => prev.map((n) => ({ ...n, isRead: true })));
  };

  if (!user) {
    return (
      <div className="max-w-2xl mx-auto px-4 py-16 text-center">
        <p className="text-gray-600 mb-4">Bildirişləri görmək üçün daxil olun.</p>
        <Link to="/giris" className="text-[#65a30d] font-semibold hover:underline">
          Giriş
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800 flex items-center gap-2">
          <Bell size={22} className="text-[#65a30d]" aria-hidden />
          Bildirişlər
        </h1>
        {items.some((n) => !n.isRead) && (
          <button
            type="button"
            onClick={() => void markAllRead()}
            className="text-sm text-[#65a30d] font-medium flex items-center gap-1 hover:underline"
          >
            <CheckCheck size={16} aria-hidden />
            Hamısını oxunmuş et
          </button>
        )}
      </div>

      {loading && (
        <div className="flex justify-center py-16" role="status" aria-label="Yüklənir">
          <Loader2 className="animate-spin text-[#65a30d]" size={32} />
        </div>
      )}

      {error && (
        <p className="rounded-xl bg-red-50 border border-red-200 text-red-700 px-4 py-3 text-sm" role="alert">
          {error}
        </p>
      )}

      {!loading && !error && items.length === 0 && (
        <p className="text-center text-gray-500 py-12">Hələ bildiriş yoxdur.</p>
      )}

      <ul className="space-y-3">
        {items.map((n) => (
          <li
            key={n.id}
            className={`rounded-xl border p-4 ${n.isRead ? "bg-white border-gray-100" : "bg-lime-50/60 border-lime-100"}`}
          >
            <p className="font-semibold text-gray-800">{n.title}</p>
            <p className="text-sm text-gray-600 mt-1">{n.body}</p>
            <p className="text-xs text-gray-400 mt-2">
              {new Date(n.createdAt).toLocaleString("az-AZ")}
            </p>
          </li>
        ))}
      </ul>
    </div>
  );
}
