import { X, CheckCircle, AlertCircle, Info, AlertTriangle } from "lucide-react";
import { useToast } from "../context/ToastContext";
import type { ToastType } from "../context/ToastContext";

const META: Record<ToastType, { Icon: typeof CheckCircle; bar: string; icon: string }> = {
  success: { Icon: CheckCircle, bar: "bg-green-500", icon: "text-green-500" },
  error:   { Icon: AlertCircle,  bar: "bg-red-500",   icon: "text-red-500" },
  info:    { Icon: Info,          bar: "bg-blue-500",  icon: "text-blue-500" },
  warning: { Icon: AlertTriangle, bar: "bg-amber-500", icon: "text-amber-500" },
};

export default function ToastContainer() {
  const { toasts, removeToast } = useToast();

  if (toasts.length === 0) return null;

  return (
    <div className="fixed top-4 right-4 z-[9999] flex flex-col gap-2 w-80 max-w-[calc(100vw-2rem)] pointer-events-none">
      {toasts.map((toast) => {
        const { Icon, bar, icon } = META[toast.type];
        return (
          <div
            key={toast.id}
            className="pointer-events-auto bg-white rounded-xl overflow-hidden shadow-lg border border-gray-100 toast-enter"
          >
            <div className={`h-1 ${bar}`} />
            <div className="flex items-start gap-3 px-4 py-3">
              <Icon size={18} className={`shrink-0 mt-0.5 ${icon}`} />
              <span className="text-sm font-medium text-gray-800 flex-1 leading-snug">
                {toast.message}
              </span>
              <button
                type="button"
                onClick={() => removeToast(toast.id)}
                className="shrink-0 text-gray-300 hover:text-gray-500 transition-colors"
              >
                <X size={14} />
              </button>
            </div>
          </div>
        );
      })}
    </div>
  );
}
