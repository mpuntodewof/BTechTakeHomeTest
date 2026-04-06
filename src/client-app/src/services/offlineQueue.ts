const DB_NAME = 'AuthAssessmentOffline';
const DB_VERSION = 1;
const STORE_NAME = 'pendingTransfers';

export interface PendingTransfer {
  id: string;              // Idempotency key (UUID)
  recipientEmail: string;
  amount: number;
  notes?: string;
  createdAt: number;       // Timestamp when queued
  status: 'pending' | 'syncing' | 'failed';
  errorMessage?: string;
}

function openDB(): Promise<IDBDatabase> {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open(DB_NAME, DB_VERSION);

    request.onupgradeneeded = () => {
      const db = request.result;
      if (!db.objectStoreNames.contains(STORE_NAME)) {
        const store = db.createObjectStore(STORE_NAME, { keyPath: 'id' });
        store.createIndex('status', 'status', { unique: false });
        store.createIndex('createdAt', 'createdAt', { unique: false });
      }
    };

    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
}

export async function addPendingTransfer(transfer: Omit<PendingTransfer, 'createdAt' | 'status'>): Promise<PendingTransfer> {
  const db = await openDB();
  const record: PendingTransfer = {
    ...transfer,
    createdAt: Date.now(),
    status: 'pending',
  };

  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readwrite');
    tx.objectStore(STORE_NAME).add(record);
    tx.oncomplete = () => resolve(record);
    tx.onerror = () => reject(tx.error);
  });
}

export async function getPendingTransfers(): Promise<PendingTransfer[]> {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readonly');
    const request = tx.objectStore(STORE_NAME).index('createdAt').getAll();
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
}

export async function updateTransferStatus(
  id: string,
  status: PendingTransfer['status'],
  errorMessage?: string
): Promise<void> {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readwrite');
    const store = tx.objectStore(STORE_NAME);
    const getReq = store.get(id);

    getReq.onsuccess = () => {
      const record = getReq.result as PendingTransfer;
      if (record) {
        record.status = status;
        record.errorMessage = errorMessage;
        store.put(record);
      }
    };

    tx.oncomplete = () => resolve();
    tx.onerror = () => reject(tx.error);
  });
}

export async function removePendingTransfer(id: string): Promise<void> {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readwrite');
    tx.objectStore(STORE_NAME).delete(id);
    tx.oncomplete = () => resolve();
    tx.onerror = () => reject(tx.error);
  });
}

export async function getPendingCount(): Promise<number> {
  const transfers = await getPendingTransfers();
  return transfers.filter((t) => t.status === 'pending' || t.status === 'failed').length;
}
