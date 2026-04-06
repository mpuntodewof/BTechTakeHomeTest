import { createContext, type ReactNode } from 'react';
import { useNetwork, type NetworkState } from '../hooks/useNetwork';

export const NetworkContext = createContext<NetworkState>({
  isOnline: true,
  wasOffline: false,
  lastOnlineAt: null,
});

export function NetworkProvider({ children }: { children: ReactNode }) {
  const network = useNetwork();

  return (
    <NetworkContext.Provider value={network}>
      {children}
    </NetworkContext.Provider>
  );
}
