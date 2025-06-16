// Demo Reset Service - 24-hour automated reset mechanism
// This service handles automated database reseeding and cleanup for demo environment

export interface DemoResetConfig {
  enabled: boolean;
  intervalHours: number;
  resetTime: string; // HH:MM format (24-hour)
  preserveSystemData: boolean;
  notifyBeforeReset: boolean;
  notificationMinutes: number;
}

export interface DemoResetStatus {
  lastResetAt: string;
  nextResetAt: string;
  isResetInProgress: boolean;
  timeUntilNextReset: string;
  totalResets: number;
}

export interface DemoResetResult {
  success: boolean;
  resetId: string;
  startedAt: string;
  completedAt: string;
  itemsReset: {
    incidents: number;
    workPermits: number;
    audits: number;
    trainings: number;
    hazards: number;
    users: number;
    attachments: number;
  };
  errors?: string[];
}

class DemoResetService {
  private static instance: DemoResetService;
  private resetTimer: NodeJS.Timeout | null = null;
  private notificationTimer: NodeJS.Timeout | null = null;
  private isResetInProgress = false;
  private config: DemoResetConfig = {
    enabled: true,
    intervalHours: 24,
    resetTime: '02:00', // 2 AM daily
    preserveSystemData: true,
    notifyBeforeReset: true,
    notificationMinutes: 15,
  };

  private constructor() {
    this.loadConfiguration();
    this.scheduleNextReset();
  }

  public static getInstance(): DemoResetService {
    if (!DemoResetService.instance) {
      DemoResetService.instance = new DemoResetService();
    }
    return DemoResetService.instance;
  }

  private async loadConfiguration(): Promise<void> {
    try {
      // Load configuration from backend API
      const response = await fetch('/api/demo/reset-config');
      if (response.ok) {
        const config = await response.json();
        this.config = { ...this.config, ...config };
      }
    } catch (error) {
      console.warn('Failed to load demo reset configuration, using defaults:', error);
    }
  }

  public async getResetStatus(): Promise<DemoResetStatus> {
    try {
      const response = await fetch('/api/demo/reset-status');
      if (response.ok) {
        return await response.json();
      }
    } catch (error) {
      console.error('Failed to get reset status:', error);
    }

    // Return default status if API call fails
    const now = new Date();
    const nextReset = this.calculateNextResetTime();
    
    return {
      lastResetAt: new Date(now.getTime() - 24 * 60 * 60 * 1000).toISOString(),
      nextResetAt: nextReset.toISOString(),
      isResetInProgress: this.isResetInProgress,
      timeUntilNextReset: this.formatTimeUntilReset(nextReset),
      totalResets: 0,
    };
  }

  private calculateNextResetTime(): Date {
    const now = new Date();
    const [hours, minutes] = this.config.resetTime.split(':').map(Number);
    
    const nextReset = new Date();
    nextReset.setHours(hours, minutes, 0, 0);
    
    // If the reset time has already passed today, schedule for tomorrow
    if (nextReset <= now) {
      nextReset.setDate(nextReset.getDate() + 1);
    }
    
    return nextReset;
  }

  private formatTimeUntilReset(nextReset: Date): string {
    const now = new Date();
    const diffMs = nextReset.getTime() - now.getTime();
    const hours = Math.floor(diffMs / (1000 * 60 * 60));
    const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
    
    if (hours > 0) {
      return `${hours}h ${minutes}m`;
    }
    return `${minutes}m`;
  }

  private scheduleNextReset(): void {
    if (!this.config.enabled) {
      console.log('Demo reset is disabled');
      return;
    }

    const nextReset = this.calculateNextResetTime();
    const now = new Date();
    const msUntilReset = nextReset.getTime() - now.getTime();

    console.log(`Next demo reset scheduled for: ${nextReset.toLocaleString()}`);

    // Clear existing timers
    if (this.resetTimer) {
      clearTimeout(this.resetTimer);
    }
    if (this.notificationTimer) {
      clearTimeout(this.notificationTimer);
    }

    // Schedule the reset
    this.resetTimer = setTimeout(() => {
      this.performReset();
    }, msUntilReset);

    // Schedule notification before reset
    if (this.config.notifyBeforeReset) {
      const notificationMs = msUntilReset - (this.config.notificationMinutes * 60 * 1000);
      if (notificationMs > 0) {
        this.notificationTimer = setTimeout(() => {
          this.notifyUsersBeforeReset();
        }, notificationMs);
      }
    }
  }

  private async notifyUsersBeforeReset(): Promise<void> {
    try {
      // Show notification to all connected users
      if (window.harmoniNotifications) {
        window.harmoniNotifications.showWarning(
          `Demo Reset Scheduled`,
          `The demo environment will be reset in ${this.config.notificationMinutes} minutes. All user data will be cleared.`,
          { persistent: true, duration: this.config.notificationMinutes * 60 * 1000 }
        );
      }

      // Also send via SignalR if available
      if (window.harmoniSignalR) {
        await window.harmoniSignalR.sendNotification('DemoResetWarning', {
          message: `Demo reset in ${this.config.notificationMinutes} minutes`,
          minutesUntilReset: this.config.notificationMinutes,
        });
      }
    } catch (error) {
      console.error('Failed to notify users before reset:', error);
    }
  }

  public async performReset(): Promise<DemoResetResult> {
    if (this.isResetInProgress) {
      throw new Error('Reset is already in progress');
    }

    this.isResetInProgress = true;
    const startedAt = new Date().toISOString();

    try {
      console.log('Starting demo environment reset...');

      // Show notification to users
      if (window.harmoniNotifications) {
        window.harmoniNotifications.showInfo(
          'Demo Reset in Progress',
          'The demo environment is being reset. Please wait...',
          { persistent: true }
        );
      }

      // Call backend reset endpoint
      const response = await fetch('/api/demo/reset', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          preserveSystemData: this.config.preserveSystemData,
          resetId: this.generateResetId(),
        }),
      });

      if (!response.ok) {
        throw new Error(`Reset failed with status: ${response.status}`);
      }

      const result: DemoResetResult = await response.json();
      result.startedAt = startedAt;
      result.completedAt = new Date().toISOString();

      console.log('Demo reset completed successfully:', result);

      // Show success notification
      if (window.harmoniNotifications) {
        window.harmoniNotifications.showSuccess(
          'Demo Reset Complete',
          'The demo environment has been successfully reset to its initial state.',
          { duration: 10000 }
        );
      }

      // Force page reload to reflect clean state
      setTimeout(() => {
        window.location.reload();
      }, 3000);

      return result;

    } catch (error) {
      console.error('Demo reset failed:', error);
      
      // Show error notification
      if (window.harmoniNotifications) {
        window.harmoniNotifications.showError(
          'Demo Reset Failed',
          'Failed to reset demo environment. Please contact support if this continues.',
          { duration: 15000 }
        );
      }

      throw error;
    } finally {
      this.isResetInProgress = false;
      // Schedule next reset
      this.scheduleNextReset();
    }
  }

  private generateResetId(): string {
    return `reset_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  public async manualReset(): Promise<DemoResetResult> {
    const confirmed = window.confirm(
      'Are you sure you want to manually reset the demo environment? ' +
      'This will delete all user-generated data and cannot be undone.'
    );

    if (!confirmed) {
      throw new Error('Manual reset cancelled by user');
    }

    return this.performReset();
  }

  public getConfiguration(): DemoResetConfig {
    return { ...this.config };
  }

  public async updateConfiguration(newConfig: Partial<DemoResetConfig>): Promise<void> {
    try {
      const response = await fetch('/api/demo/reset-config', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(newConfig),
      });

      if (response.ok) {
        this.config = { ...this.config, ...newConfig };
        // Reschedule with new configuration
        this.scheduleNextReset();
      }
    } catch (error) {
      console.error('Failed to update reset configuration:', error);
      throw error;
    }
  }

  public destroy(): void {
    if (this.resetTimer) {
      clearTimeout(this.resetTimer);
      this.resetTimer = null;
    }
    if (this.notificationTimer) {
      clearTimeout(this.notificationTimer);
      this.notificationTimer = null;
    }
  }
}

// Global type declarations for window objects
declare global {
  interface Window {
    harmoniNotifications?: {
      showSuccess: (title: string, message: string, options?: any) => void;
      showError: (title: string, message: string, options?: any) => void;
      showWarning: (title: string, message: string, options?: any) => void;
      showInfo: (title: string, message: string, options?: any) => void;
    };
    harmoniSignalR?: {
      sendNotification: (type: string, data: any) => Promise<void>;
    };
    demoResetService?: DemoResetService;
  }
}

// Initialize service when module loads
export const demoResetService = DemoResetService.getInstance();

// Make it globally available
if (typeof window !== 'undefined') {
  window.demoResetService = demoResetService;
}

export default DemoResetService;