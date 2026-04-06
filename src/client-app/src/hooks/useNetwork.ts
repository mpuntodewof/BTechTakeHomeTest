import { useState, useEffect, useCallback } from 'react';

export interface NetworkState {
  isOnline: boolean;
  wasOffline: boolean;
  lastOnlineAt: number | null;
}

export function useNetwork(): NetworkState {
  const [isOnline, setIsOnline] = useState(navigator.onLine);
  const [wasOffline, setWasOffline] = useState(false);
  const [lastOnlineAt, setLastOnlineAt] = useState<number | null>(
    navigator.onLine ? Date.now() : null
  );

  const goOnline = useCallback(() => {
    setIsOnline(true);
    setLastOnlineAt(Date.now());
    if (!navigator.onLine) return;
    
    setWasOffline(true);
    
    setTimeout(() => setWasOffline(false), 5000);
  }, []);

  const goOffline = useCallback(() => {
    setIsOnline(false);
  }, []);

  useEffect(() => {
    window.addEventListener('online', goOnline);
    window.addEventListener('offline', goOffline);
    return () => {
      window.removeEventListener('online', goOnline);
      window.removeEventListener('offline', goOffline);
    };
  }, [goOnline, goOffline]);

  return { isOnline, wasOffline, lastOnlineAt };
}
