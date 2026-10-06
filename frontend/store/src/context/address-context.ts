import { createContext } from "react";

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

export interface AddressContextType {
  addresses: Address[];
  addAddress: (addr: Omit<Address, "id">) => Promise<void>;
  updateAddress: (id: string, updates: Partial<Omit<Address, "id">>) => Promise<void>;
  deleteAddress: (id: string) => Promise<void>;
  setDefaultAddress: (id: string) => Promise<void>;
  defaultAddress: Address | null;
}

export const AddressContext = createContext<AddressContextType | undefined>(undefined);
