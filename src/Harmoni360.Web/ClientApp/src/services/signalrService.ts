import * as signalR from '@microsoft/signalr';
import { store } from '../store';
import { incidentApi } from '../features/incidents/incidentApi';

class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private isConnecting = false;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectInterval = 5000; // 5 seconds

  async startConnection(token: string) {
    // Don't attempt connection if already connected or connecting
    if (
      this.hubConnection &&
      this.hubConnection.state === signalR.HubConnectionState.Connected
    ) {
      return;
    }

    if (this.isConnecting) {
      return;
    }

    this.isConnecting = true;

    try {
      // Stop existing connection if any
      if (this.hubConnection) {
        try {
          await this.stopConnection();
        } catch (err) {
          console.warn('Error stopping existing connection:', err);
        }
      }

      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/incidents', {
          accessTokenFactory: () => token,
          transport:
            signalR.HttpTransportType.WebSockets |
            signalR.HttpTransportType.ServerSentEvents,
          skipNegotiation: false,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff: 0, 2, 10, 30 seconds then every 30 seconds
            if (retryContext.previousRetryCount < 4) {
              return Math.pow(2, retryContext.previousRetryCount) * 2000;
            }
            return 30000;
          },
        })
        .configureLogging(signalR.LogLevel.Warning) // Reduce logging to warnings only
        .build();

      // Set up event handlers before starting the connection
      this.setupEventHandlers();

      await this.hubConnection.start();
      console.log('SignalR Connected successfully');
      console.log('SignalR Connection State:', this.hubConnection.state);
      this.reconnectAttempts = 0;
      this.isConnecting = false;

      // Test if we can send a message (optional)
      try {
        await this.hubConnection.invoke('JoinGroup', 'incident-updates');
        console.log('SignalR: Successfully joined incident-updates group');
      } catch (err) {
        console.warn(
          'SignalR: Failed to join group, but connection is still active:',
          err
        );
      }
    } catch (err) {
      console.warn('SignalR Connection failed:', err);
      this.isConnecting = false;
      this.hubConnection = null;

      // Don't throw the error - just log it and continue
      // This prevents SignalR connection issues from blocking incident creation
      this.scheduleReconnect(token);
    }
  }

  private scheduleReconnect(token: string) {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.warn(
        'SignalR: Maximum reconnection attempts reached. Real-time updates disabled.'
      );
      return;
    }

    this.reconnectAttempts++;
    console.log(
      `SignalR: Scheduling reconnection attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts} in ${this.reconnectInterval}ms`
    );

    setTimeout(() => {
      if (
        (!this.hubConnection ||
          this.hubConnection.state ===
            signalR.HubConnectionState.Disconnected) &&
        !this.isConnecting
      ) {
        this.startConnection(token).catch(() => {
          // Ignore errors during reconnection
        });
      }
    }, this.reconnectInterval);
  }

  private setupEventHandlers() {
    if (!this.hubConnection) return;

    // Handle new incident created
    this.hubConnection.on('IncidentCreated', (incidentId: number) => {
      console.log('SignalR: New incident created with ID:', incidentId);
      // Invalidate incident list cache to trigger refetch
      store.dispatch(
        incidentApi.util.invalidateTags(['Incident', 'IncidentStatistics'])
      );
    });

    // Handle incident updated
    this.hubConnection.on('IncidentUpdated', (incidentId: number) => {
      console.log('SignalR: Incident updated with ID:', incidentId);
      // Invalidate specific incident and list cache
      store.dispatch(
        incidentApi.util.invalidateTags([
          { type: 'Incident', id: incidentId },
          'Incident',
          'IncidentStatistics',
        ])
      );
    });

    // Handle incident deleted
    this.hubConnection.on('IncidentDeleted', (incidentId: number) => {
      console.log('SignalR: Incident deleted with ID:', incidentId);
      // Invalidate cache
      store.dispatch(
        incidentApi.util.invalidateTags([
          { type: 'Incident', id: incidentId },
          'Incident',
          'IncidentStatistics',
        ])
      );
    });

    // Handle status changed
    this.hubConnection.on(
      'IncidentStatusChanged',
      (data: { incidentId: number; newStatus: string }) => {
        console.log('Incident status changed:', data);
        // Invalidate cache
        store.dispatch(
          incidentApi.util.invalidateTags([
            { type: 'Incident', id: data.incidentId },
            'Incident',
          ])
        );
      }
    );

    // Handle dashboard updates
    this.hubConnection.on('DashboardUpdate', async () => {
      console.log('SignalR: Dashboard update received');
      // Invalidate dashboard cache to trigger refetch
      store.dispatch(
        incidentApi.util.invalidateTags(['IncidentStatistics'])
      );
      // Also invalidate PPE dashboard
      try {
        const { ppeApi } = await import('../features/ppe/ppeApi');
        store.dispatch(
          ppeApi.util.invalidateTags(['PPEDashboard'])
        );
        const { statisticsApi } = await import('../features/statistics/statisticsApi');
        store.dispatch(statisticsApi.util.invalidateTags(['HsseStatistics']));
      } catch (error) {
        console.warn('Failed to invalidate PPE dashboard cache:', error);
      }
    });

    // Handle connection events
    this.hubConnection.onclose((error) => {
      console.log('SignalR Disconnected', error);
      this.reconnectAttempts = 0; // Reset counter on clean disconnect
    });

    this.hubConnection.onreconnecting((error) => {
      console.log('SignalR Reconnecting...', error);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR Reconnected with connection ID:', connectionId);
      this.reconnectAttempts = 0; // Reset counter on successful reconnect
    });
  }

  async stopConnection() {
    this.isConnecting = false;
    if (this.hubConnection) {
      try {
        // Only stop if not already disconnected/disposed
        if (this.hubConnection.state !== signalR.HubConnectionState.Disconnected) {
          await this.hubConnection.stop();
        }
      } catch (err) {
        console.warn('Error during connection stop:', err);
      } finally {
        this.hubConnection = null;
      }
    }
  }

  getConnectionState(): signalR.HubConnectionState | null {
    return this.hubConnection?.state || null;
  }

  getConnection(): signalR.HubConnection | null {
    return this.hubConnection;
  }

  async joinLocationGroup(location: string) {
    if (
      this.hubConnection &&
      this.hubConnection.state === signalR.HubConnectionState.Connected
    ) {
      try {
        await this.hubConnection.invoke('JoinLocationGroup', location);
        console.log(`Joined location group: ${location}`);
      } catch (err) {
        console.error('Error joining location group:', err);
      }
    }
  }

  async leaveLocationGroup(location: string) {
    if (
      this.hubConnection &&
      this.hubConnection.state === signalR.HubConnectionState.Connected
    ) {
      try {
        await this.hubConnection.invoke('LeaveLocationGroup', location);
        console.log(`Left location group: ${location}`);
      } catch (err) {
        console.error('Error leaving location group:', err);
      }
    }
  }
}

export const signalRService = new SignalRService();
