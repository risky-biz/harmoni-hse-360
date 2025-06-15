import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { vi } from 'vitest';
import { TrainingList } from '../../../pages/trainings/TrainingList';
import { renderWithProviders } from '../../utils/test-utils';

// Mock react-router-dom
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => vi.fn(),
    useLocation: () => ({ pathname: '/trainings' })
  };
});

describe('TrainingList', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders training list page', async () => {
    renderWithProviders(<TrainingList />);

    // Check for main heading
    expect(screen.getByText('Training Management')).toBeInTheDocument();

    // Check for create training button
    expect(screen.getByText('Create Training')).toBeInTheDocument();

    // Wait for trainings to load
    await waitFor(() => {
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
    });
  });

  it('displays training data correctly', async () => {
    renderWithProviders(<TrainingList />);

    await waitFor(() => {
      // Check first training
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
      expect(screen.getByText('TRN-20241213-001')).toBeInTheDocument();
      expect(screen.getByText('John Smith')).toBeInTheDocument();
      expect(screen.getByText('Scheduled')).toBeInTheDocument();
      
      // Check second training
      expect(screen.getByText('Emergency Response Training')).toBeInTheDocument();
      expect(screen.getByText('TRN-20241213-002')).toBeInTheDocument();
      expect(screen.getByText('Sarah Johnson')).toBeInTheDocument();
      expect(screen.getByText('In Progress')).toBeInTheDocument();
      
      // Check third training
      expect(screen.getByText('K3 Compliance Training')).toBeInTheDocument();
      expect(screen.getByText('TRN-20241213-003')).toBeInTheDocument();
      expect(screen.getByText('Completed')).toBeInTheDocument();
    });
  });

  it('filters trainings by search term', async () => {
    renderWithProviders(<TrainingList />);

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
    });

    // Type in search box
    const searchInput = screen.getByPlaceholderText('Search trainings...');
    fireEvent.change(searchInput, { target: { value: 'Emergency' } });

    // Wait for filter to apply
    await waitFor(() => {
      expect(screen.getByText('Emergency Response Training')).toBeInTheDocument();
      expect(screen.queryByText('Safety Induction Training')).not.toBeInTheDocument();
    });
  });

  it('filters trainings by status', async () => {
    renderWithProviders(<TrainingList />);

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
    });

    // Select status filter
    const statusFilter = screen.getByLabelText('Status');
    fireEvent.change(statusFilter, { target: { value: 'Completed' } });

    // Wait for filter to apply
    await waitFor(() => {
      expect(screen.getByText('K3 Compliance Training')).toBeInTheDocument();
      expect(screen.queryByText('Safety Induction Training')).not.toBeInTheDocument();
      expect(screen.queryByText('Emergency Response Training')).not.toBeInTheDocument();
    });
  });

  it('filters trainings by type', async () => {
    renderWithProviders(<TrainingList />);

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
    });

    // Select type filter
    const typeFilter = screen.getByLabelText('Training Type');
    fireEvent.change(typeFilter, { target: { value: 'SafetyOrientation' } });

    // Wait for filter to apply
    await waitFor(() => {
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
      expect(screen.queryByText('Emergency Response Training')).not.toBeInTheDocument();
      expect(screen.queryByText('K3 Compliance Training')).not.toBeInTheDocument();
    });
  });

  it('displays action buttons based on training status', async () => {
    renderWithProviders(<TrainingList />);

    await waitFor(() => {
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
    });

    // Check for action buttons
    const viewButtons = screen.getAllByTitle('View Details');
    expect(viewButtons).toHaveLength(3); // All trainings should have view button

    const editButtons = screen.getAllByTitle('Edit Training');
    expect(editButtons).toHaveLength(1); // Only draft/scheduled trainings should have edit button

    const startButtons = screen.getAllByTitle('Start Training');
    expect(startButtons).toHaveLength(1); // Only scheduled trainings should have start button

    const completeButtons = screen.getAllByTitle('Complete Training');
    expect(completeButtons).toHaveLength(1); // Only in-progress trainings should have complete button
  });

  it('displays training statistics', async () => {
    renderWithProviders(<TrainingList />);

    await waitFor(() => {
      // Check for statistics cards
      expect(screen.getByText('Total Trainings')).toBeInTheDocument();
      expect(screen.getByText('Scheduled')).toBeInTheDocument();
      expect(screen.getByText('In Progress')).toBeInTheDocument();
      expect(screen.getByText('Completed')).toBeInTheDocument();
    });
  });

  it('handles pagination correctly', async () => {
    renderWithProviders(<TrainingList />);

    await waitFor(() => {
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
    });

    // Check pagination controls
    const pagination = screen.getByRole('navigation', { name: /pagination/i });
    expect(pagination).toBeInTheDocument();
  });

  it('clears filters when clear button is clicked', async () => {
    renderWithProviders(<TrainingList />);

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
    });

    // Apply search filter
    const searchInput = screen.getByPlaceholderText('Search trainings...');
    fireEvent.change(searchInput, { target: { value: 'Emergency' } });

    // Wait for filter to apply
    await waitFor(() => {
      expect(screen.queryByText('Safety Induction Training')).not.toBeInTheDocument();
    });

    // Clear filters
    const clearButton = screen.getByText('Clear Filters');
    fireEvent.click(clearButton);

    // Wait for filters to clear
    await waitFor(() => {
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
      expect(screen.getByText('Emergency Response Training')).toBeInTheDocument();
      expect(screen.getByText('K3 Compliance Training')).toBeInTheDocument();
    });

    // Check that search input is cleared
    expect(searchInput).toHaveValue('');
  });

  it('displays empty state when no trainings found', async () => {
    renderWithProviders(<TrainingList />);

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
    });

    // Search for non-existent training
    const searchInput = screen.getByPlaceholderText('Search trainings...');
    fireEvent.change(searchInput, { target: { value: 'NonExistentTraining' } });

    // Wait for empty state
    await waitFor(() => {
      expect(screen.getByText('No trainings found')).toBeInTheDocument();
      expect(screen.getByText('Try adjusting your search criteria or create a new training.')).toBeInTheDocument();
    });
  });

  it('handles loading state correctly', () => {
    renderWithProviders(<TrainingList />);

    // Should show loading spinner initially
    expect(screen.getByRole('status')).toBeInTheDocument();
  });

  it('displays status badges with correct colors', async () => {
    renderWithProviders(<TrainingList />);

    await waitFor(() => {
      // Check status badges
      const scheduledBadge = screen.getByText('Scheduled');
      expect(scheduledBadge).toHaveClass('badge');
      
      const inProgressBadge = screen.getByText('In Progress');
      expect(inProgressBadge).toHaveClass('badge');
      
      const completedBadge = screen.getByText('Completed');
      expect(completedBadge).toHaveClass('badge');
    });
  });

  it('displays training details in list format', async () => {
    renderWithProviders(<TrainingList />);

    await waitFor(() => {
      // Check training details
      expect(screen.getByText('25 participants')).toBeInTheDocument(); // Max participants
      expect(screen.getByText('240 minutes')).toBeInTheDocument(); // Duration
      expect(screen.getByText('High')).toBeInTheDocument(); // Priority
      expect(screen.getByText('Mandatory')).toBeInTheDocument(); // Category
    });
  });
});