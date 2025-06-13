import { DashboardWidget, WidgetData } from '../types/dashboard';
import { useGetIncidentDashboardQuery } from '../features/incidents/incidentApi';
import { store } from '../store';

export class DashboardDataService {
  private static instance: DashboardDataService;
  private dataCache: Map<string, { data: WidgetData; timestamp: number }> = new Map();
  private readonly CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

  static getInstance(): DashboardDataService {
    if (!DashboardDataService.instance) {
      DashboardDataService.instance = new DashboardDataService();
    }
    return DashboardDataService.instance;
  }

  // Helper method to get auth headers
  private getAuthHeaders(): HeadersInit {
    const state = store.getState();
    const token = state.auth.token;
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };
    
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
    
    return headers;
  }

  // Transform incident dashboard data to widget format
  private transformIncidentDashboardData(dashboardData: any): Map<string, WidgetData> {
    const widgetDataMap = new Map<string, WidgetData>();

    if (!dashboardData) return widgetDataMap;

    // Transform stats data
    const stats = dashboardData.overallStats || {};
    
    widgetDataMap.set('total-incidents-stats', {
      stats: {
        value: stats.totalIncidents || 0,
        subtitle: 'incidents',
      },
      lastUpdated: new Date().toISOString(),
    });

    widgetDataMap.set('open-incidents-stats', {
      stats: {
        value: stats.openIncidents || 0,
        subtitle: 'open',
        trend: stats.openIncidentsTrend ? {
          value: stats.openIncidentsTrend,
          isPositive: stats.openIncidentsTrend <= 0, // Negative trend is good for open incidents
          label: 'vs last month',
        } : undefined,
      },
      lastUpdated: new Date().toISOString(),
    });

    widgetDataMap.set('critical-incidents-stats', {
      stats: {
        value: stats.criticalIncidents || 0,
        subtitle: 'critical',
        trend: stats.criticalIncidentsTrend ? {
          value: Math.abs(stats.criticalIncidentsTrend),
          isPositive: stats.criticalIncidentsTrend <= 0,
          label: 'vs last month',
        } : undefined,
      },
      lastUpdated: new Date().toISOString(),
    });

    widgetDataMap.set('resolved-incidents-stats', {
      stats: {
        value: stats.closedIncidents || 0,
        subtitle: 'resolved',
        trend: stats.resolvedIncidentsTrend ? {
          value: stats.resolvedIncidentsTrend,
          isPositive: stats.resolvedIncidentsTrend >= 0,
          label: 'vs last month',
        } : undefined,
      },
      lastUpdated: new Date().toISOString(),
    });

    // Transform recent incidents
    if (dashboardData.recentIncidents) {
      const recentItems = dashboardData.recentIncidents.map((incident: any) => ({
        id: incident.id.toString(),
        title: incident.title,
        subtitle: `${incident.reporterName} • ${incident.location}`,
        metadata: {
          status: incident.status,
          statusColor: this.getStatusColor(incident.status),
          severity: incident.severity,
          timestamp: incident.createdAt || incident.incidentDate,
          isOverdue: incident.isOverdue,
        },
        clickAction: () => window.location.href = `/incidents/${incident.id}`,
      }));

      widgetDataMap.set('recent-incidents-list', {
        listData: recentItems,
        lastUpdated: new Date().toISOString(),
      });
    }

    // Transform category stats for donut chart
    if (dashboardData.categoryStats) {
      const chartData = dashboardData.categoryStats.map((category: any, index: number) => ({
        label: category.category,
        value: category.count,
        color: `hsl(${index * 45}, 70%, 60%)`,
        metadata: {
          percentage: category.percentage,
          criticalCount: category.criticalCount,
          averageResolutionDays: category.averageResolutionDays,
        },
      }));

      widgetDataMap.set('incident-categories-chart', {
        chartData,
        lastUpdated: new Date().toISOString(),
      });
    }

    // Transform department stats for bar chart
    if (dashboardData.departmentStats) {
      const chartData = dashboardData.departmentStats.map((dept: any) => ({
        label: dept.department,
        value: dept.incidentCount,
        metadata: {
          complianceScore: dept.complianceScore,
          criticalCount: dept.criticalCount,
          averageResolutionDays: dept.averageResolutionDays,
        },
      }));

      widgetDataMap.set('department-performance-chart', {
        chartData,
        lastUpdated: new Date().toISOString(),
      });
    }

    return widgetDataMap;
  }

  private getStatusColor(status: string): string {
    switch (status) {
      case 'Resolved':
      case 'Closed':
        return 'success';
      case 'Critical':
      case 'AwaitingAction':
        return 'danger';
      case 'UnderInvestigation':
        return 'warning';
      case 'Reported':
      default:
        return 'primary';
    }
  }

  async getWidgetData(widget: DashboardWidget): Promise<WidgetData | null> {
    try {
      // Check cache first
      const cached = this.dataCache.get(widget.id);
      if (cached && Date.now() - cached.timestamp < this.CACHE_DURATION) {
        return cached.data;
      }

      // For widgets that don't need API data
      if (widget.type === 'quick-actions') {
        return {
          customData: {},
          lastUpdated: new Date().toISOString(),
        };
      }

      // Handle different data sources
      switch (widget.config.dataSource) {
        case 'incident-dashboard':
          return await this.getIncidentDashboardData(widget);
        
        case 'my-incidents':
          return await this.getMyIncidentsData(widget);
        
        case 'overdue-incidents':
          return await this.getOverdueIncidentsData(widget);
        
        case 'safety-tips':
          return await this.getSafetyTipsData(widget);
        
        case 'compliance-metrics':
          return await this.getComplianceMetricsData(widget);
        
        default:
          console.warn(`Unknown data source: ${widget.config.dataSource}`);
          return null;
      }
    } catch (error) {
      console.error(`Error loading data for widget ${widget.id}:`, error);
      return {
        error: 'Failed to load data',
        lastUpdated: new Date().toISOString(),
      };
    }
  }

  private async getIncidentDashboardData(widget: DashboardWidget): Promise<WidgetData | null> {
    try {
      const response = await fetch('/api/incident/dashboard', {
        headers: this.getAuthHeaders(),
      });
      
      // Check if response is HTML (likely 404 page) instead of JSON
      const contentType = response.headers.get('content-type');
      if (contentType && contentType.includes('text/html')) {
        throw new Error('API endpoint returned HTML instead of JSON');
      }
      
      if (!response.ok) {
        if (response.status === 401) {
          throw new Error('Authentication required');
        }
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }
      
      const data = await response.json();
      const transformedData = this.transformIncidentDashboardData(data);
      const widgetData = transformedData.get(widget.id);
      
      if (widgetData) {
        // Cache the data
        this.dataCache.set(widget.id, {
          data: widgetData,
          timestamp: Date.now(),
        });
      }
      
      return widgetData || null;
    } catch (error) {
      // Provide helpful guidance for development mode
      if (import.meta.env.DEV) {
        console.warn(`DashboardService: API unavailable for ${widget.id}. Please ensure the backend is running and database is seeded.`);
      }
      console.error('Error fetching incident dashboard data:', error);
      return {
        error: 'Failed to load incident data. Please ensure the backend is running.',
        lastUpdated: new Date().toISOString(),
      };
    }
  }

  private async getMyIncidentsData(widget: DashboardWidget): Promise<WidgetData> {
    // Mock implementation for my incidents
    return {
      stats: {
        value: 3,
        subtitle: 'my reports',
        trend: {
          value: 1,
          isPositive: true,
          label: 'this month',
        },
      },
      lastUpdated: new Date().toISOString(),
    };
  }

  private async getOverdueIncidentsData(widget: DashboardWidget): Promise<WidgetData> {
    // Mock implementation for overdue incidents
    return {
      customData: {
        type: 'overdue-incidents',
        count: 2,
        incidents: [
          {
            title: 'Chemical spill in Lab A',
            daysOverdue: 3,
          },
          {
            title: 'Equipment malfunction',
            daysOverdue: 1,
          },
        ],
      },
      lastUpdated: new Date().toISOString(),
    };
  }

  private async getSafetyTipsData(widget: DashboardWidget): Promise<WidgetData> {
    const tips = [
      "Always wear appropriate PPE when handling chemicals",
      "Report near-miss incidents immediately to prevent future accidents",
      "Keep emergency exits clear and accessible at all times",
      "Regularly inspect safety equipment and report any issues",
      "Follow proper lifting techniques to prevent back injuries",
    ];

    const randomTip = tips[Math.floor(Math.random() * tips.length)];

    return {
      customData: {
        type: 'safety-tip',
        message: randomTip,
        source: 'HSE Department',
      },
      lastUpdated: new Date().toISOString(),
    };
  }

  private async getComplianceMetricsData(widget: DashboardWidget): Promise<WidgetData> {
    // Mock implementation for compliance metrics
    return {
      stats: {
        value: 92,
        subtitle: '% compliance',
        trend: {
          value: 3,
          isPositive: true,
          label: 'vs last quarter',
        },
      },
      lastUpdated: new Date().toISOString(),
    };
  }


  // Clear cache for specific widget or all widgets
  clearCache(widgetId?: string): void {
    if (widgetId) {
      this.dataCache.delete(widgetId);
    } else {
      this.dataCache.clear();
    }
  }
}

export const dashboardService = DashboardDataService.getInstance();