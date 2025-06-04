import { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { signalRService } from '../services/signalrService';
import { RootState } from '../store';

export const useSignalR = () => {
  const token = useSelector((state: RootState) => state.auth.token);
  const isAuthenticated = useSelector((state: RootState) => state.auth.isAuthenticated);

  useEffect(() => {
    if (isAuthenticated && token) {
      // Delay SignalR connection to avoid conflicts during app initialization
      const connectionTimer = setTimeout(() => {
        signalRService.startConnection(token).catch(err => {
          console.warn('SignalR connection failed, but application will continue:', err);
        });
      }, 1000); // 1 second delay

      return () => {
        clearTimeout(connectionTimer);
        signalRService.stopConnection().catch(err => {
          console.warn('Failed to stop SignalR connection on cleanup:', err);
        });
      };
    } else {
      // Stop connection when not authenticated
      signalRService.stopConnection().catch(err => {
        console.warn('Failed to stop SignalR connection:', err);
      });
    }
  }, [isAuthenticated, token]);

  return {
    connectionState: signalRService.getConnectionState(),
    joinLocationGroup: signalRService.joinLocationGroup.bind(signalRService),
    leaveLocationGroup: signalRService.leaveLocationGroup.bind(signalRService)
  };
};