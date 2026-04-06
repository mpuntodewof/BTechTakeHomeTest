import { useContext } from 'react';
import { Alert, Collapse } from '@mui/material';
import { WifiOff, Wifi } from '@mui/icons-material';
import { NetworkContext } from '../context/NetworkContext';

export default function OfflineBanner() {
  const { isOnline, wasOffline } = useContext(NetworkContext);

  return (
    <>
      <Collapse in={!isOnline}>
        <Alert
          severity="warning"
          icon={<WifiOff />}
          sx={{ borderRadius: 0 }}
        >
          You are offline. Transfers will be queued and sent when your connection returns.
        </Alert>
      </Collapse>
      <Collapse in={isOnline && wasOffline}>
        <Alert
          severity="success"
          icon={<Wifi />}
          sx={{ borderRadius: 0 }}
        >
          Connection restored. Syncing pending transfers...
        </Alert>
      </Collapse>
    </>
  );
}
