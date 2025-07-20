import { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { signalRService } from '../services/signalrService';
import { RootState } from '../store';

export const useSignalR = () => {
  const token = useSelector((state: RootState) => state.auth.token);
  const isAuthenticated = useSelector(
    (state: RootState) => state.auth.isAuthenticated
  );

  useEffect(() => {
    let cancelled = false;
    let connectionTimer: NodeJS.Timeout | null = null;

    if (isAuthenticated && token) {
      // Delay SignalR connection to avoid conflicts during app initialization
      connectionTimer = setTimeout(() => {
        if (!cancelled) {
          signalRService.startConnection(token).catch((err) => {
            if (!cancelled) {
              console.warn(
                'SignalR connection failed, but application will continue:',
                err
              );
            }
          });
        }
      }, 1000); // 1 second delay
    } else {
      // Stop connection when not authenticated
      signalRService.stopConnection().catch((err) => {
        if (!cancelled) {
          console.warn('Failed to stop SignalR connection:', err);
        }
      });
    }

    return () => {
      cancelled = true;
      if (connectionTimer) {
        clearTimeout(connectionTimer);
      }
      signalRService.stopConnection().catch((err) => {
        console.warn('Failed to stop SignalR connection on cleanup:', err);
      });
    };
  }, [isAuthenticated, token]);

  return {
    connection: signalRService.getConnection(),
    connectionState: signalRService.getConnectionState(),
    isConnected: signalRService.getConnectionState() === 'Connected',
    joinLocationGroup: signalRService.joinLocationGroup.bind(signalRService),
    leaveLocationGroup: signalRService.leaveLocationGroup.bind(signalRService),
  };
};
