import { useState, useEffect, useCallback, startTransition } from "react";
import type { ReactNode } from "react";
import { useAuth } from "../hooks/useAuth";
import { userApi, type AddressWire } from "../services/api";
import { AddressContext, type Address } from "./address-context";

function storageKey(userId?: string) {
  return userId ? `gulora_addresses_${userId}` : "gulora_addresses_guest";
}

function loadGuestAddresses(userId?: string): Address[] {
  try {
    const raw = localStorage.getItem(storageKey(userId));
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

function saveGuestAddresses(addrs: Address[], userId?: string) {
  localStorage.setItem(storageKey(userId), JSON.stringify(addrs));
}

function mapWire(w: AddressWire): Address {
  return {
    id: w.id,
    title: w.title,
    recipientName: w.recipientName,
    recipientPhone: w.phoneNumber,
    city: w.city,
    district: w.district,
    line: w.fullLine,
    postalCode: w.postalCode ?? undefined,
    isDefault: w.isDefault,
  };
}

export function AddressProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const [addresses, setAddresses] = useState<Address[]>([]);

  const reload = useCallback(async () => {
    if (!user) {
      setAddresses(loadGuestAddresses(undefined));
      return;
    }
    try {
      const profile = await userApi.getProfile();
      setAddresses(profile.addresses.map(mapWire));
    } catch {
      setAddresses([]);
    }
  }, [user]);

  useEffect(() => {
    startTransition(() => {
      void reload();
    });
  }, [reload]);

  useEffect(() => {
    if (!user) {
      saveGuestAddresses(addresses, undefined);
    }
  }, [addresses, user]);

  const addAddress = useCallback(
    async (addr: Omit<Address, "id">) => {
      if (!user) {
        const newAddr: Address = { ...addr, id: `addr_${Date.now()}` };
        setAddresses((prev) => {
          const base = addr.isDefault ? prev.map((a) => ({ ...a, isDefault: false })) : [...prev];
          return [...base, newAddr];
        });
        return;
      }
      await userApi.addAddress({
        title: addr.title,
        recipientName: addr.recipientName,
        phoneNumber: addr.recipientPhone,
        city: addr.city,
        district: addr.district,
        fullLine: addr.line,
        postalCode: addr.postalCode ?? null,
        isDefault: addr.isDefault,
      });
      await reload();
    },
    [user, reload]
  );

  const updateAddress = useCallback(
    async (id: string, updates: Partial<Omit<Address, "id">>) => {
      if (!user) {
        setAddresses((prev) => {
          let updated = prev.map((a) => (a.id === id ? { ...a, ...updates } : a));
          if (updates.isDefault) {
            updated = updated.map((a) => (a.id !== id ? { ...a, isDefault: false } : a));
          }
          return updated;
        });
        return;
      }
      const cur = addresses.find((a) => a.id === id);
      if (!cur) return;
      await userApi.updateAddress(id, {
        title: updates.title ?? cur.title,
        recipientName: updates.recipientName ?? cur.recipientName,
        phoneNumber: updates.recipientPhone ?? cur.recipientPhone,
        city: updates.city ?? cur.city,
        district: updates.district ?? cur.district,
        fullLine: updates.line ?? cur.line,
        postalCode: updates.postalCode ?? cur.postalCode ?? null,
      });
      await reload();
    },
    [user, addresses, reload]
  );

  const deleteAddress = useCallback(
    async (id: string) => {
      if (!user) {
        setAddresses((prev) => prev.filter((a) => a.id !== id));
        return;
      }
      await userApi.deleteAddress(id);
      await reload();
    },
    [user, reload]
  );

  const setDefaultAddress = useCallback(async (id: string) => {
    setAddresses((prev) => prev.map((a) => ({ ...a, isDefault: a.id === id })));
  }, []);

  const defaultAddress = addresses.find((a) => a.isDefault) ?? addresses[0] ?? null;

  return (
    <AddressContext.Provider
      value={{
        addresses,
        addAddress,
        updateAddress,
        deleteAddress,
        setDefaultAddress,
        defaultAddress,
      }}
    >
      {children}
    </AddressContext.Provider>
  );
}

export type { Address } from "./address-context";
