import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';
import { store } from '../../../store';
import { InspectionDashboard } from '../../../pages/inspections/InspectionDashboard';
import { InspectionStatus, InspectionType } from '../../../types/inspection';

// Mock the chart components
vi.mock('../../../components/inspections/charts', () => ({
  DonutChart: ({ data, title }: any) => (
    <div data-testid="donut-chart">
      <div>{title}</div>
      <div>{data.labels.join(', ')}</div>
    </div>
  ),
  BarChart: ({ data, title }: any) => (
    <div data-testid="bar-chart">
      <div>{title}</div>
      <div>{data.labels.join(', ')}</div>
    </div>
  ),
  LineChart: ({ data, title }: any) => (
    <div data-testid="line-chart">
      <div>{title}</div>
      <div>{data.labels.join(', ')}</div>
    </div>
  )
}));

// Mock the API hooks
vi.mock('../../../features/inspections/inspectionApi', () => ({
  useGetDashboardQuery: vi.fn(() => ({
    data: {
      totalInspections: 125,
      inProgressInspections: 8,
      completedInspections: 98,
      overdueInspections: 3,
      criticalFindings: 2,
      averageCompletionTime: 2.5,
      complianceRate: 94.2,
      recentInspections: [
        {
          id: 1,
          inspectionNumber: 'INS-2024-001',
          title: 'Safety Equipment Check',
          type: InspectionType.Safety,
          typeName: 'Safety',
          status: InspectionStatus.Completed,
          statusName: 'Completed',
          scheduledDate: '2024-01-15T09:00:00Z',
          inspectorName: 'John Doe',
          departmentName: 'Operations'
        }
      ],
      criticalFindingsList: [
        {
          id: 1,
          inspectionId: 1,
          description: 'Fire extinguisher missing from emergency station',
          severity: 'Critical',
          status: 'Open',
          dueDate: '2024-01-20T00:00:00Z',
          responsiblePersonName: 'Jane Smith'
        }
      ],
      upcomingInspections: [
        {
          id: 2,
          inspectionNumber: 'INS-2024-002',
          title: 'Environmental Compliance Audit',
          type: InspectionType.Environmental,
          typeName: 'Environmental',
          scheduledDate: '2024-01-18T14:00:00Z',
          inspectorName: 'Bob Wilson',
          departmentName: 'Environmental'
        }
      ],
      overdueList: [
        {
          id: 3,
          inspectionNumber: 'INS-2024-003',
          title: 'Overdue Safety Inspection',
          scheduledDate: '2024-01-10T09:00:00Z',
          inspectorName: 'Alice Brown'
        }
      ],
      inspectionsByStatus: [
        {
          status: InspectionStatus.Completed,
          statusName: 'Completed',
          count: 98,
          percentage: 78.4
        },
        {
          status: InspectionStatus.Scheduled,
          statusName: 'Scheduled',
          count: 16,
          percentage: 12.8
        },
        {
          status: InspectionStatus.InProgress,
          statusName: 'In Progress',
          count: 8,
          percentage: 6.4
        },
        {
          status: InspectionStatus.Cancelled,
          statusName: 'Cancelled',
          count: 3,
          percentage: 2.4
        }
      ],
      inspectionsByType: [
        {
          type: InspectionType.Safety,
          typeName: 'Safety',
          count: 45,
          percentage: 36.0
        },
        {
          type: InspectionType.Environmental,
          typeName: 'Environmental',
          count: 35,
          percentage: 28.0
        },
        {
          type: InspectionType.Quality,
          typeName: 'Quality',
          count: 25,
          percentage: 20.0
        },
        {
          type: InspectionType.Maintenance,
          typeName: 'Maintenance',
          count: 20,
          percentage: 16.0
        }
      ],
      monthlyTrends: [
        {
          month: 'Nov',
          year: 2023,
          scheduled: 20,
          completed: 18,
          overdue: 2,
          criticalFindings: 1
        },
        {
          month: 'Dec',
          year: 2023,
          scheduled: 25,
          completed: 23,
          overdue: 1,
          criticalFindings: 0
        },
        {
          month: 'Jan',
          year: 2024,
          scheduled: 30,
          completed: 25,
          overdue: 3,
          criticalFindings: 2
        }
      ]
    },
    isLoading: false,
    error: null,
    refetch: vi.fn()
  })),
  useGetStatisticsQuery: vi.fn(() => ({
    data: {},
    isLoading: false
  }))
}));

// Mock the export utilities
vi.mock('../../../utils/exportUtils', () => ({
  exportDashboardToPDF: vi.fn(),
  exportInspectionsToExcel: vi.fn()
}));

// Mock the permissions
vi.mock('../../../components/auth/PermissionWrappers', () => ({
  RequirePermission: ({ children }: { children: React.ReactNode }) => <>{children}</>
}));

// Mock the demo mode wrapper
vi.mock('../../../components/common/DemoModeWrapper', () => ({
  DemoModeWrapper: ({ children }: { children: React.ReactNode }) => <>{children}</>
}));

// Mock react-router-dom
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    BrowserRouter: ({ children }: { children: React.ReactNode }) => <div>{children}</div>
  };
});

// Test wrapper component
const TestWrapper = ({ children }: { children: React.ReactNode }) => (
  <Provider store={store}>
    <BrowserRouter>
      {children}
    </BrowserRouter>
  </Provider>
);

describe('InspectionDashboard', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders dashboard header correctly', () => {
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    expect(screen.getByText('Inspection Dashboard')).toBeInTheDocument();
    expect(screen.getByText('Overview of inspection metrics and performance')).toBeInTheDocument();
  });

  it('displays KPI cards with correct values', () => {
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    expect(screen.getByText('125')).toBeInTheDocument(); // Total inspections
    expect(screen.getByText('8')).toBeInTheDocument(); // In progress
    expect(screen.getByText('3')).toBeInTheDocument(); // Overdue
    expect(screen.getByText('94.2%')).toBeInTheDocument(); // Compliance rate
  });

  it('displays secondary metrics correctly', () => {
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    expect(screen.getByText('2')).toBeInTheDocument(); // Critical findings
    expect(screen.getByText('2.5h')).toBeInTheDocument(); // Avg completion time
  });

  it('renders status distribution chart', () => {
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    const statusChart = screen.getByTestId('donut-chart');
    expect(statusChart).toBeInTheDocument();
    expect(screen.getByText('Status Distribution')).toBeInTheDocument();
    expect(statusChart).toHaveTextContent('Completed, Scheduled, In Progress, Cancelled');
  });

  it('renders type distribution chart', () => {
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    const typeChart = screen.getByTestId('bar-chart');
    expect(typeChart).toBeInTheDocument();
    expect(screen.getByText('Type Distribution')).toBeInTheDocument();
    expect(typeChart).toHaveTextContent('Safety, Environmental, Quality, Maintenance');
  });

  it('renders monthly trends chart', () => {
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    const trendsChart = screen.getByTestId('line-chart');
    expect(trendsChart).toBeInTheDocument();
    expect(screen.getByText('Monthly Inspection Trends')).toBeInTheDocument();
    expect(trendsChart).toHaveTextContent('Nov 2023, Dec 2023, Jan 2024');
  });

  it('displays recent inspections table', () => {
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    expect(screen.getByText('Recent Inspections')).toBeInTheDocument();
    expect(screen.getByText('Safety Equipment Check')).toBeInTheDocument();
    expect(screen.getByText('INS-2024-001')).toBeInTheDocument();
    expect(screen.getByText('John Doe')).toBeInTheDocument();
  });

  it('displays critical findings list', () => {
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    expect(screen.getByText('Critical Findings')).toBeInTheDocument();
    expect(screen.getByText('Fire extinguisher missing from emergency station')).toBeInTheDocument();
    expect(screen.getByText('Jane Smith')).toBeInTheDocument();
  });

  it('displays upcoming inspections', () => {
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    expect(screen.getByText('Upcoming Inspections')).toBeInTheDocument();
    expect(screen.getByText('Environmental Compliance Audit')).toBeInTheDocument();
    expect(screen.getByText('INS-2024-002')).toBeInTheDocument();
    expect(screen.getByText('Bob Wilson')).toBeInTheDocument();
  });

  it('handles time range selection', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    const timeRangeSelect = screen.getByDisplayValue('Last 30 Days');
    await user.click(timeRangeSelect);
    
    const sevenDaysOption = screen.getByText('Last 7 Days');
    await user.click(sevenDaysOption);

    expect(timeRangeSelect).toHaveValue('7days');
  });

  it('handles department filter selection', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    const departmentSelect = screen.getByDisplayValue('All Departments');
    expect(departmentSelect).toBeInTheDocument();
  });

  it('displays actions dropdown with correct options', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    const actionsButton = screen.getByText('Actions');
    await user.click(actionsButton);

    await waitFor(() => {
      expect(screen.getByText('Create Inspection')).toBeInTheDocument();
      expect(screen.getByText('View All Inspections')).toBeInTheDocument();
      expect(screen.getByText('My Inspections')).toBeInTheDocument();
      expect(screen.getByText('Export Dashboard (PDF)')).toBeInTheDocument();
      expect(screen.getByText('Export Recent Inspections (Excel)')).toBeInTheDocument();
    });
  });

  it('handles navigation from actions menu', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    const actionsButton = screen.getByText('Actions');
    await user.click(actionsButton);

    await waitFor(async () => {
      const createButton = screen.getByText('Create Inspection');
      await user.click(createButton);
    });

    expect(mockNavigate).toHaveBeenCalledWith('/inspections/create');
  });

  it('handles export functionality', async () => {
    const user = userEvent.setup();
    const { exportDashboardToPDF, exportInspectionsToExcel } = require('../../../utils/exportUtils');
    
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    const actionsButton = screen.getByText('Actions');
    await user.click(actionsButton);

    await waitFor(async () => {
      const exportDashboardButton = screen.getByText('Export Dashboard (PDF)');
      await user.click(exportDashboardButton);
    });

    expect(exportDashboardToPDF).toHaveBeenCalled();
  });

  it('handles refresh functionality', async () => {
    const user = userEvent.setup();
    const mockRefetch = vi.fn();
    
    // Mock the refetch function
    vi.mocked(require('../../../features/inspections/inspectionApi').useGetDashboardQuery).mockReturnValue({
      data: expect.any(Object),
      isLoading: false,
      error: null,
      refetch: mockRefetch
    });

    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    const refreshButton = screen.getByText('Refresh');
    await user.click(refreshButton);

    expect(mockRefetch).toHaveBeenCalled();
  });

  it('shows loading state', () => {
    // Mock loading state
    vi.mocked(require('../../../features/inspections/inspectionApi').useGetDashboardQuery).mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
      refetch: vi.fn()
    });

    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    expect(screen.getByText('Loading dashboard...')).toBeInTheDocument();
  });

  it('handles error state', () => {
    // Mock error state
    vi.mocked(require('../../../features/inspections/inspectionApi').useGetDashboardQuery).mockReturnValue({
      data: undefined,
      isLoading: false,
      error: new Error('Failed to load'),
      refetch: vi.fn()
    });

    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    expect(screen.getByText('Error loading dashboard data. Please try again.')).toBeInTheDocument();
  });

  it('displays empty states when no data available', () => {
    // Mock empty data
    vi.mocked(require('../../../features/inspections/inspectionApi').useGetDashboardQuery).mockReturnValue({
      data: {
        totalInspections: 0,
        recentInspections: [],
        criticalFindingsList: [],
        upcomingInspections: [],
        inspectionsByStatus: [],
        inspectionsByType: [],
        monthlyTrends: []
      },
      isLoading: false,
      error: null,
      refetch: vi.fn()
    });

    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    expect(screen.getByText('No status data available')).toBeInTheDocument();
    expect(screen.getByText('No type data available')).toBeInTheDocument();
    expect(screen.getByText('No trend data available')).toBeInTheDocument();
  });
});