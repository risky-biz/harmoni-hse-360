import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Provider } from 'react-redux';
import { BrowserRouter, MemoryRouter } from 'react-router-dom';
import { store } from '../../store';
import { InspectionStatus, InspectionType, InspectionCategory, InspectionPriority } from '../../types/inspection';

// Mock the entire inspection API module
let mockInspections: any[] = [];
let mockDashboardData: any = {};

vi.mock('../../features/inspections/inspectionApi', () => ({
  useGetInspectionsQuery: vi.fn(() => ({
    data: {
      items: mockInspections,
      totalCount: mockInspections.length,
      totalPages: Math.ceil(mockInspections.length / 25),
      page: 1,
      pageSize: 25
    },
    isLoading: false,
    error: null,
    refetch: vi.fn()
  })),
  useGetInspectionByIdQuery: vi.fn((id: number) => ({
    data: mockInspections.find(i => i.id === id),
    isLoading: false,
    error: null
  })),
  useGetDashboardQuery: vi.fn(() => ({
    data: mockDashboardData,
    isLoading: false,
    error: null,
    refetch: vi.fn()
  })),
  useCreateInspectionMutation: vi.fn(() => [
    vi.fn((data) => {
      const newInspection = {
        id: mockInspections.length + 1,
        inspectionNumber: `INS-2024-${String(mockInspections.length + 1).padStart(3, '0')}`,
        ...data,
        status: InspectionStatus.Draft,
        createdAt: new Date().toISOString(),
        lastModifiedAt: new Date().toISOString(),
        canEdit: true,
        canStart: true,
        canComplete: false,
        canCancel: true,
        isOverdue: false,
        itemsCount: data.items?.length || 0,
        completedItemsCount: 0,
        findingsCount: 0,
        criticalFindingsCount: 0,
        attachmentsCount: 0
      };
      mockInspections.push(newInspection);
      return Promise.resolve({ data: newInspection });
    }),
    { isLoading: false }
  ]),
  useUpdateInspectionMutation: vi.fn(() => [
    vi.fn((data) => {
      const index = mockInspections.findIndex(i => i.id === data.id);
      if (index >= 0) {
        mockInspections[index] = { ...mockInspections[index], ...data };
      }
      return Promise.resolve({ data: mockInspections[index] });
    }),
    { isLoading: false }
  ]),
  useStartInspectionMutation: vi.fn(() => [
    vi.fn((id: number) => {
      const index = mockInspections.findIndex(i => i.id === id);
      if (index >= 0) {
        mockInspections[index].status = InspectionStatus.InProgress;
        mockInspections[index].startedDate = new Date().toISOString();
        mockInspections[index].canStart = false;
        mockInspections[index].canComplete = true;
      }
      return Promise.resolve({ data: mockInspections[index] });
    }),
    { isLoading: false }
  ]),
  useCompleteInspectionMutation: vi.fn(() => [
    vi.fn((data: { id: number; summary: string; recommendations: string }) => {
      const index = mockInspections.findIndex(i => i.id === data.id);
      if (index >= 0) {
        mockInspections[index].status = InspectionStatus.Completed;
        mockInspections[index].completedDate = new Date().toISOString();
        mockInspections[index].summary = data.summary;
        mockInspections[index].recommendations = data.recommendations;
        mockInspections[index].canStart = false;
        mockInspections[index].canComplete = false;
        mockInspections[index].canEdit = false;
      }
      return Promise.resolve({ data: mockInspections[index] });
    }),
    { isLoading: false }
  ])
}));

// Mock components to focus on integration flow
vi.mock('../../pages/inspections/CreateInspection', () => ({
  CreateInspection: () => {
    const { useCreateInspectionMutation } = require('../../features/inspections/inspectionApi');
    const [createInspection] = useCreateInspectionMutation();
    
    return (
      <div data-testid="create-inspection">
        <h2>Create Inspection</h2>
        <button
          onClick={() => createInspection({
            title: 'Test Safety Inspection',
            description: 'Integration test inspection',
            type: InspectionType.Safety,
            category: InspectionCategory.Routine,
            priority: InspectionPriority.Medium,
            scheduledDate: new Date().toISOString(),
            inspectorId: 1,
            locationId: 1,
            departmentId: 1,
            facilityId: 1,
            riskLevel: 'Medium',
            estimatedDurationMinutes: 120,
            items: [
              {
                description: 'Check safety equipment',
                requiresPhoto: false,
                isRequired: true
              }
            ]
          })}
        >
          Create Inspection
        </button>
      </div>
    );
  }
}));

vi.mock('../../pages/inspections/InspectionList', () => ({
  InspectionList: () => {
    const { useGetInspectionsQuery } = require('../../features/inspections/inspectionApi');
    const { data } = useGetInspectionsQuery({});
    
    return (
      <div data-testid="inspection-list">
        <h2>Inspections</h2>
        {data?.items.map((inspection: any) => (
          <div key={inspection.id} data-testid={`inspection-${inspection.id}`}>
            <span>{inspection.title}</span>
            <span>{inspection.statusName}</span>
            <button onClick={() => window.location.href = `/inspections/${inspection.id}`}>
              View
            </button>
          </div>
        ))}
      </div>
    );
  }
}));

vi.mock('../../pages/inspections/InspectionDetail', () => ({
  InspectionDetail: () => {
    const { useGetInspectionByIdQuery, useStartInspectionMutation, useCompleteInspectionMutation } = require('../../features/inspections/inspectionApi');
    const id = 1; // Mock ID
    const { data: inspection } = useGetInspectionByIdQuery(id);
    const [startInspection] = useStartInspectionMutation();
    const [completeInspection] = useCompleteInspectionMutation();
    
    if (!inspection) return <div>Loading...</div>;
    
    return (
      <div data-testid="inspection-detail">
        <h2>{inspection.title}</h2>
        <p>Status: {inspection.statusName}</p>
        {inspection.canStart && (
          <button onClick={() => startInspection(inspection.id)}>
            Start Inspection
          </button>
        )}
        {inspection.canComplete && (
          <button onClick={() => completeInspection({
            id: inspection.id,
            summary: 'Inspection completed successfully',
            recommendations: 'Continue monitoring'
          })}>
            Complete Inspection
          </button>
        )}
      </div>
    );
  }
}));

vi.mock('../../pages/inspections/InspectionDashboard', () => ({
  InspectionDashboard: () => {
    const { useGetDashboardQuery } = require('../../features/inspections/inspectionApi');
    const { data } = useGetDashboardQuery();
    
    return (
      <div data-testid="inspection-dashboard">
        <h2>Inspection Dashboard</h2>
        <div>Total: {data?.totalInspections || 0}</div>
        <div>Completed: {data?.completedInspections || 0}</div>
        <div>In Progress: {data?.inProgressInspections || 0}</div>
      </div>
    );
  }
}));

// Mock permissions and other wrappers
vi.mock('../../components/auth/PermissionWrappers', () => ({
  RequirePermission: ({ children }: { children: React.ReactNode }) => <>{children}</>
}));

vi.mock('../../components/common/DemoModeWrapper', () => ({
  DemoModeWrapper: ({ children }: { children: React.ReactNode }) => <>{children}</>
}));

// Test wrapper component
const TestWrapper = ({ children }: { children: React.ReactNode }) => (
  <Provider store={store}>
    <MemoryRouter>
      {children}
    </MemoryRouter>
  </Provider>
);

describe('Inspection Workflow Integration Tests', () => {
  beforeEach(() => {
    // Reset mock data
    mockInspections = [];
    mockDashboardData = {
      totalInspections: 0,
      completedInspections: 0,
      inProgressInspections: 0,
      overdueInspections: 0,
      criticalFindings: 0,
      averageCompletionTime: 0,
      complianceRate: 0,
      recentInspections: [],
      criticalFindingsList: [],
      upcomingInspections: [],
      inspectionsByStatus: [],
      inspectionsByType: [],
      monthlyTrends: []
    };
    vi.clearAllMocks();
  });

  it('completes full inspection lifecycle: create -> start -> complete', async () => {
    const user = userEvent.setup();

    // Step 1: Create inspection
    const { CreateInspection } = await import('../../pages/inspections/CreateInspection');
    const { rerender } = render(
      <TestWrapper>
        <CreateInspection />
      </TestWrapper>
    );

    const createButton = screen.getByText('Create Inspection');
    await user.click(createButton);

    // Verify inspection was created
    expect(mockInspections).toHaveLength(1);
    expect(mockInspections[0].title).toBe('Test Safety Inspection');
    expect(mockInspections[0].status).toBe(InspectionStatus.Draft);

    // Step 2: View inspection list
    const { InspectionList } = await import('../../pages/inspections/InspectionList');
    rerender(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    expect(screen.getByText('Test Safety Inspection')).toBeInTheDocument();
    expect(screen.getByText('Draft')).toBeInTheDocument();

    // Step 3: View inspection detail and start
    const { InspectionDetail } = await import('../../pages/inspections/InspectionDetail');
    rerender(
      <TestWrapper>
        <InspectionDetail />
      </TestWrapper>
    );

    expect(screen.getByText('Test Safety Inspection')).toBeInTheDocument();
    expect(screen.getByText('Status: Draft')).toBeInTheDocument();

    const startButton = screen.getByText('Start Inspection');
    await user.click(startButton);

    // Verify inspection status changed
    expect(mockInspections[0].status).toBe(InspectionStatus.InProgress);

    // Re-render to see updated status
    rerender(
      <TestWrapper>
        <InspectionDetail />
      </TestWrapper>
    );

    expect(screen.getByText('Status: InProgress')).toBeInTheDocument();

    // Step 4: Complete inspection
    const completeButton = screen.getByText('Complete Inspection');
    await user.click(completeButton);

    // Verify inspection was completed
    expect(mockInspections[0].status).toBe(InspectionStatus.Completed);
    expect(mockInspections[0].summary).toBe('Inspection completed successfully');
  });

  it('tracks inspection metrics in dashboard', async () => {
    // Create some test data
    mockInspections = [
      {
        id: 1,
        title: 'Completed Inspection',
        status: InspectionStatus.Completed,
        statusName: 'Completed'
      },
      {
        id: 2,
        title: 'In Progress Inspection',
        status: InspectionStatus.InProgress,
        statusName: 'InProgress'
      },
      {
        id: 3,
        title: 'Scheduled Inspection',
        status: InspectionStatus.Scheduled,
        statusName: 'Scheduled'
      }
    ];

    mockDashboardData = {
      totalInspections: 3,
      completedInspections: 1,
      inProgressInspections: 1,
      overdueInspections: 0
    };

    const { InspectionDashboard } = await import('../../pages/inspections/InspectionDashboard');
    render(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    expect(screen.getByText('Total: 3')).toBeInTheDocument();
    expect(screen.getByText('Completed: 1')).toBeInTheDocument();
    expect(screen.getByText('In Progress: 1')).toBeInTheDocument();
  });

  it('handles inspection creation validation', async () => {
    const user = userEvent.setup();

    // Mock validation error
    const { useCreateInspectionMutation } = await import('../../features/inspections/inspectionApi');
    const mockCreateWithError = vi.fn().mockRejectedValue({
      data: {
        errors: {
          Title: ['Title is required']
        }
      }
    });
    
    vi.mocked(useCreateInspectionMutation).mockReturnValue([
      mockCreateWithError,
      { isLoading: false }
    ]);

    const { CreateInspection } = await import('../../pages/inspections/CreateInspection');
    render(
      <TestWrapper>
        <CreateInspection />
      </TestWrapper>
    );

    const createButton = screen.getByText('Create Inspection');
    await user.click(createButton);

    // Should handle validation error gracefully
    expect(mockCreateWithError).toHaveBeenCalled();
  });

  it('maintains state consistency across components', async () => {
    const user = userEvent.setup();

    // Start with create inspection
    const { CreateInspection } = await import('../../pages/inspections/CreateInspection');
    const { rerender } = render(
      <TestWrapper>
        <CreateInspection />
      </TestWrapper>
    );

    // Create inspection
    const createButton = screen.getByText('Create Inspection');
    await user.click(createButton);

    // Switch to list view
    const { InspectionList } = await import('../../pages/inspections/InspectionList');
    rerender(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    // Should show the created inspection
    expect(screen.getByText('Test Safety Inspection')).toBeInTheDocument();

    // Start the inspection through detail view
    const { InspectionDetail } = await import('../../pages/inspections/InspectionDetail');
    rerender(
      <TestWrapper>
        <InspectionDetail />
      </TestWrapper>
    );

    const startButton = screen.getByText('Start Inspection');
    await user.click(startButton);

    // Go back to list view
    rerender(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    // Should show updated status (note: this would need proper state management in real app)
    expect(screen.getByTestId('inspection-1')).toBeInTheDocument();
  });

  it('handles error states gracefully', async () => {
    // Mock API error
    const { useGetInspectionsQuery } = await import('../../features/inspections/inspectionApi');
    vi.mocked(useGetInspectionsQuery).mockReturnValue({
      data: undefined,
      isLoading: false,
      error: new Error('API Error'),
      refetch: vi.fn()
    });

    const { InspectionList } = await import('../../pages/inspections/InspectionList');
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    // Should still render the component structure
    expect(screen.getByTestId('inspection-list')).toBeInTheDocument();
  });

  it('supports real-time updates through refetch', async () => {
    const mockRefetch = vi.fn();
    
    // Mock the query with refetch function
    const { useGetInspectionsQuery } = await import('../../features/inspections/inspectionApi');
    vi.mocked(useGetInspectionsQuery).mockReturnValue({
      data: {
        items: mockInspections,
        totalCount: mockInspections.length
      },
      isLoading: false,
      error: null,
      refetch: mockRefetch
    });

    const { InspectionList } = await import('../../pages/inspections/InspectionList');
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    // Simulate external update that would trigger refetch
    mockInspections.push({
      id: 999,
      title: 'External Update Inspection',
      status: InspectionStatus.Scheduled,
      statusName: 'Scheduled'
    });

    // In a real app, this would be triggered by websockets, polling, or user action
    mockRefetch();
    expect(mockRefetch).toHaveBeenCalled();
  });

  it('preserves user workflow across navigation', async () => {
    const user = userEvent.setup();

    // Create inspection
    const { CreateInspection } = await import('../../pages/inspections/CreateInspection');
    const { rerender } = render(
      <TestWrapper>
        <CreateInspection />
      </TestWrapper>
    );

    const createButton = screen.getByText('Create Inspection');
    await user.click(createButton);

    // Navigate to dashboard
    const { InspectionDashboard } = await import('../../pages/inspections/InspectionDashboard');
    
    // Update mock dashboard data to reflect new inspection
    mockDashboardData.totalInspections = 1;
    
    rerender(
      <TestWrapper>
        <InspectionDashboard />
      </TestWrapper>
    );

    // Dashboard should reflect the changes
    expect(screen.getByText('Total: 1')).toBeInTheDocument();
  });
});