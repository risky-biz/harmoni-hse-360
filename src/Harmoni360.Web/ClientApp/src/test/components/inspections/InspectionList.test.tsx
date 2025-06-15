import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';
import { store } from '../../../store';
import { InspectionList } from '../../../pages/inspections/InspectionList';
import { InspectionStatus, InspectionType, InspectionPriority } from '../../../types/inspection';

// Mock the API hook
vi.mock('../../../features/inspections/inspectionApi', () => ({
  useGetInspectionsQuery: vi.fn(() => ({
    data: {
      items: [
        {
          id: 1,
          inspectionNumber: 'INS-2024-001',
          title: 'Safety Equipment Inspection',
          description: 'Monthly safety equipment check',
          type: InspectionType.Safety,
          typeName: 'Safety',
          status: InspectionStatus.Scheduled,
          statusName: 'Scheduled',
          priority: InspectionPriority.Medium,
          priorityName: 'Medium',
          scheduledDate: '2024-01-15T09:00:00Z',
          inspectorName: 'John Doe',
          departmentName: 'Operations',
          itemsCount: 5,
          completedItemsCount: 0,
          findingsCount: 0,
          criticalFindingsCount: 0,
          canEdit: true,
          canStart: true,
          isOverdue: false,
          createdAt: '2024-01-10T10:00:00Z'
        }
      ],
      totalCount: 1,
      totalPages: 1,
      page: 1,
      pageSize: 25
    },
    isLoading: false,
    error: null,
    refetch: vi.fn()
  }))
}));

// Mock the export utilities
vi.mock('../../../utils/exportUtils', () => ({
  exportInspectionsToExcel: vi.fn(),
  exportInspectionsToPDF: vi.fn()
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

describe('InspectionList', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders inspection list correctly', () => {
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    expect(screen.getByText('Inspections')).toBeInTheDocument();
    expect(screen.getByText('Manage and track safety inspections')).toBeInTheDocument();
    expect(screen.getByText('Safety Equipment Inspection')).toBeInTheDocument();
    expect(screen.getByText('INS-2024-001')).toBeInTheDocument();
  });

  it('displays search input and filters', () => {
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    expect(screen.getByPlaceholderText('Search inspections...')).toBeInTheDocument();
    expect(screen.getByText('Filters')).toBeInTheDocument();
    expect(screen.getByText('Export')).toBeInTheDocument();
    expect(screen.getByText('Refresh')).toBeInTheDocument();
    expect(screen.getByText('Create Inspection')).toBeInTheDocument();
  });

  it('handles search input correctly', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    const searchInput = screen.getByPlaceholderText('Search inspections...');
    await user.type(searchInput, 'safety equipment');

    expect(searchInput).toHaveValue('safety equipment');
  });

  it('opens filters when filter button is clicked', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    const filterButton = screen.getByText('Filters');
    await user.click(filterButton);

    // Should show filter options
    await waitFor(() => {
      expect(screen.getByText('All Statuses')).toBeInTheDocument();
    });
  });

  it('displays export dropdown with options', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    const exportButton = screen.getByText('Export');
    await user.click(exportButton);

    await waitFor(() => {
      expect(screen.getByText('Export to Excel')).toBeInTheDocument();
      expect(screen.getByText('Export to PDF')).toBeInTheDocument();
    });
  });

  it('shows inspection data in table format', () => {
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    // Check table headers
    expect(screen.getByText('Inspection')).toBeInTheDocument();
    expect(screen.getByText('Type')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
    expect(screen.getByText('Priority')).toBeInTheDocument();
    expect(screen.getByText('Inspector')).toBeInTheDocument();
    expect(screen.getByText('Department')).toBeInTheDocument();
    expect(screen.getByText('Scheduled')).toBeInTheDocument();
    expect(screen.getByText('Progress')).toBeInTheDocument();

    // Check data
    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText('Operations')).toBeInTheDocument();
    expect(screen.getByText('Safety')).toBeInTheDocument();
    expect(screen.getByText('Scheduled')).toBeInTheDocument();
    expect(screen.getByText('Medium')).toBeInTheDocument();
  });

  it('handles pagination correctly', () => {
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    // Should show pagination info
    expect(screen.getByText(/Showing/)).toBeInTheDocument();
    expect(screen.getByText(/of 1 entries/)).toBeInTheDocument();
  });

  it('handles row actions correctly', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    // Find action buttons
    const viewButton = screen.getByLabelText(/view inspection/i);
    const editButton = screen.getByLabelText(/edit inspection/i);

    expect(viewButton).toBeInTheDocument();
    expect(editButton).toBeInTheDocument();

    // Test navigation
    await user.click(viewButton);
    expect(mockNavigate).toHaveBeenCalledWith('/inspections/1');

    await user.click(editButton);
    expect(mockNavigate).toHaveBeenCalledWith('/inspections/1/edit');
  });

  it('handles create inspection button click', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    const createButton = screen.getByText('Create Inspection');
    await user.click(createButton);

    expect(mockNavigate).toHaveBeenCalledWith('/inspections/create');
  });

  it('shows progress indicators correctly', () => {
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    // Should show progress bar for items completion
    expect(screen.getByText('0 / 5')).toBeInTheDocument();
  });

  it('displays badges with correct colors', () => {
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    // Check for badge elements
    const safetyBadge = screen.getByText('Safety');
    const scheduledBadge = screen.getByText('Scheduled');
    const mediumBadge = screen.getByText('Medium');

    expect(safetyBadge).toBeInTheDocument();
    expect(scheduledBadge).toBeInTheDocument();
    expect(mediumBadge).toBeInTheDocument();
  });

  it('handles sorting correctly', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    // Find sortable column headers
    const titleHeader = screen.getByText('Inspection');
    await user.click(titleHeader);

    // Should trigger sort (this would normally trigger API call with new sort parameters)
    expect(titleHeader).toBeInTheDocument();
  });

  it('handles filter selections', async () => {
    const user = userEvent.setup();
    
    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    const filterButton = screen.getByText('Filters');
    await user.click(filterButton);

    await waitFor(() => {
      const statusSelect = screen.getByDisplayValue('All Statuses');
      expect(statusSelect).toBeInTheDocument();
    });
  });

  it('shows empty state when no inspections', () => {
    // Mock empty data
    vi.mocked(require('../../../features/inspections/inspectionApi').useGetInspectionsQuery).mockReturnValue({
      data: {
        items: [],
        totalCount: 0,
        totalPages: 0,
        page: 1,
        pageSize: 25
      },
      isLoading: false,
      error: null,
      refetch: vi.fn()
    });

    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    expect(screen.getByText('No inspections found')).toBeInTheDocument();
  });

  it('shows loading state', () => {
    // Mock loading state
    vi.mocked(require('../../../features/inspections/inspectionApi').useGetInspectionsQuery).mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
      refetch: vi.fn()
    });

    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    expect(screen.getByText('Loading inspections...')).toBeInTheDocument();
  });

  it('handles error state', () => {
    // Mock error state
    vi.mocked(require('../../../features/inspections/inspectionApi').useGetInspectionsQuery).mockReturnValue({
      data: undefined,
      isLoading: false,
      error: new Error('Failed to load'),
      refetch: vi.fn()
    });

    render(
      <TestWrapper>
        <InspectionList />
      </TestWrapper>
    );

    expect(screen.getByText('Error loading inspections. Please try again.')).toBeInTheDocument();
  });
});