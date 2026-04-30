import { createContext, useContext, useState, useEffect, useCallback } from "react";
import type { ReactNode } from "react";
import { useAuth } from "./AuthContext";

export interface Address {
  id: string;
  title: string;
  recipientName: string;
  recipientPhone: string;
  city: string;
  district: string;
  line: string;
  postalCode?: string;
  isDefault: boolean;
}

interface AddressContextType {
  addresses: Address[];
  addAddress: (addr: Omit<Address, "id">) => void;
  updateAddress: (id: string, updates: Partial<Omit<Address, "id">>) => void;
  deleteAddress: (id: string) => void;
  setDefaultAddress: (id: string) => void;
  defaultAddress: Address | null;
}

const AddressContext = createContext<AddressContextType | undefined>(undefined);

function storageKey(userId?: string) {
  return userId ? `gulora_addresses_${userId}` : "gulora_addresses_guest";
}

function loadAddresses(userId?: string): Address[] {
  try {
    const raw = localStorage.getItem(storageKey(userId));
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

function saveAddresses(addrs: Address[], userId?: string) {
  localStorage.setItem(storageKey(userId), JSON.stringify(addrs));
}

export function AddressProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const [addresses, setAddresses] = useState<Address[]>(() => loadAddresses(user?.id));

  useEffect(() => {
    setAddresses(loadAddresses(user?.id));
  }, [user?.id]);

  useEffect(() => {
    saveAddresses(addresses, user?.id);
  }, [addresses, user?.id]);

  const addAddress = useCallback((addr: Omit<Address, "id">) => {
    const newAddr: Address = { ...addr, id: `addr_${Date.now()}` };
    setAddresses((prev) => {
      let updated = addr.isDefault
        ? prev.map((a) => ({ ...a, isDefault: false }))
        : [...prev];
      return [...updated, newAddr];
    });
  }, []);

  const updateAddress = useCallback((id: string, updates: Partial<Omit<Address, "id">>) => {
    setAddresses((prev) => {
      let updated = prev.map((a) => (a.id === id ? { ...a, ...updates } : a));
      if (updates.isDefault) {
        updated = updated.map((a) => (a.id !== id ? { ...a, isDefault: false } : a));
      }
      return updated;
    });
  }, []);

  const deleteAddress = useCallback((id: string) => {
    setAddresses((prev) => prev.filter((a) => a.id !== id));
  }, []);

  const setDefaultAddress = useCallback((id: string) => {
    setAddresses((prev) => prev.map((a) => ({ ...a, isDefault: a.id === id })));
  }, []);

  const defaultAddress = addresses.find((a) => a.isDefault) ?? addresses[0] ?? null;

  return (
    <AddressContext.Provider value={{ addresses, addAddress, updateAddress, deleteAddress, setDefaultAddress, defaultAddress }}>
      {children}
    </AddressContext.Provider>
  );
}

export function useAddresses() {
  const ctx = useContext(AddressContext);
  if (!ctx) throw new Error("useAddresses must be used within AddressProvider");
  return ctx;
}
