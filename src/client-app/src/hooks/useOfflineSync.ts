import { useEffect, useCallback, useContext, useState } from 'react';
import { NetworkContext } from '../context/NetworkContext';
import {
  getPendingTransfers,
  updateTransferStatus,
  removePendingTransfer,
  type PendingTransfer,
} from '../services/offlineQueue';
import api from '../services/api';

export interface SyncState {
  isSyncing: boolean;
  pendingCount: number;
  lastSyncResult: SyncResult | null;
  syncNow: () => Promise<void>;
}

export interface SyncResult {
  synced: number;
  failed: number;
  errors: string[];
}

export function useOfflineSync(onSyncComplete?: () => void): SyncState {
  const { isOnline, wasOffline } = useContext(NetworkContext);
  const [isSyncing, setIsSyncing] = useState(false);
  const [pendingCount, setPendingCount] = useState(0);
  const [lastSyncResult, setLastSyncResult] = useState<SyncResult | null>(null);

  const refreshPendingCount = useCallback(async () => {
    try {
      const transfers = await getPendingTransfers();
      setPendingCount(
        transfers.filter((t) => t.status === 'pending' || t.status === 'failed').length
      );
    } catch {
      // IndexedDB may not be available
    }
  }, []);

  const syncNow = useCallback(async () => {
    if (isSyncing || !navigator.onLine) return;

    setIsSyncing(true);
    const result: SyncResult = { synced: 0, failed: 0, errors: [] };

    try {
      const transfers = await getPendingTransfers();
      const pending = transfers.filter(
        (t) => t.status === 'pending' || t.status === 'failed'
      );

      for (const transfer of pending) {
        await updateTransferStatus(transfer.id, 'syncing');

        try {
          await api.post('/api/transactions/transfer', {
            idempotencyKey: transfer.id,
            recipientEmail: transfer.recipientEmail,
            amount: transfer.amount,
            notes: transfer.notes,
          });

          await removePendingTransfer(transfer.id);
          result.synced++;
        } catch (err: unknown) {
          const message = extractErrorMessage(err);

          // If already processed (idempotency hit), remove from queue
          if (message.includes('already been processed')) {
            await removePendingTransfer(transfer.id);
            result.synced++;
          } else {
            await updateTransferStatus(transfer.id, 'failed', message);
            result.failed++;
            result.errors.push(`${transfer.recipientEmail}: ${message}`);
          }
        }
      }
    } finally {
      setIsSyncing(false);
      setLastSyncResult(result);
      await refreshPendingCount();
      if (result.synced > 0) {
        onSyncComplete?.();
      }
    }
  }, [isSyncing, onSyncComplete, refreshPendingCount]);

  // Auto-sync when connection is restored
  useEffect(() => {
    if (isOnline && wasOffline) {
      syncNow();
    }
  }, [isOnline, wasOffline, syncNow]);

  // Refresh count on mount
  useEffect(() => {
    refreshPendingCount();
  }, [refreshPendingCount]);

  return { isSyncing, pendingCount, lastSyncResult, syncNow };
}

function extractErrorMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'response' in err) {
    const axiosErr = err as { response?: { data?: { error?: string; message?: string } | string } };
    const data = axiosErr.response?.data;
    if (typeof data === 'string') return data;
    if (data && typeof data === 'object') return data.error || data.message || 'Transfer failed';
  }
  return 'Network error';
}
