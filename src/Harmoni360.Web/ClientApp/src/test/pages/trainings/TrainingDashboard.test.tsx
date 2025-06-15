import { render, screen, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import { TrainingDashboard } from '../../../pages/trainings/TrainingDashboard';
import { renderWithProviders } from '../../utils/test-utils';

// Mock react-router-dom
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => vi.fn(),
    useLocation: () => ({ pathname: '/trainings/dashboard' })
  };
});

// Mock Chart.js
vi.mock('react-chartjs-2', () => ({
  Doughnut: ({ data }: any) => (
    <div data-testid="doughnut-chart">
      {data.labels?.map((label: string, index: number) => (
        <div key={index}>{label}: {data.datasets[0].data[index]}</div>
      ))}
    </div>
  ),
  Line: ({ data }: any) => (
    <div data-testid="line-chart">
      {data.labels?.map((label: string, index: number) => (
        <div key={index}>{label}: {data.datasets[0].data[index]}</div>
      ))}
    </div>
  ),
  Bar: ({ data }: any) => (
    <div data-testid="bar-chart">
      {data.labels?.map((label: string, index: number) => (
        <div key={index}>{label}: {data.datasets[0].data[index]}</div>
      ))}
    </div>
  )
}));

describe('TrainingDashboard', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders training dashboard page', async () => {
    renderWithProviders(<TrainingDashboard />);

    // Check for main heading
    expect(screen.getByText('Training Dashboard')).toBeInTheDocument();

    // Wait for dashboard data to load
    await waitFor(() => {
      expect(screen.getByText('Total Trainings')).toBeInTheDocument();
    });
  });

  it('displays training statistics correctly', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check statistics cards
      expect(screen.getByText('Total Trainings')).toBeInTheDocument();
      expect(screen.getByText('15')).toBeInTheDocument(); // Total count

      expect(screen.getByText('Scheduled Trainings')).toBeInTheDocument();
      expect(screen.getByText('5')).toBeInTheDocument(); // Scheduled count

      expect(screen.getByText('In Progress')).toBeInTheDocument();
      expect(screen.getByText('2')).toBeInTheDocument(); // In progress count

      expect(screen.getByText('Completed')).toBeInTheDocument();
      expect(screen.getByText('7')).toBeInTheDocument(); // Completed count

      expect(screen.getByText('Total Participants')).toBeInTheDocument();
      expect(screen.getByText('125')).toBeInTheDocument(); // Total participants
    });
  });

  it('displays performance metrics', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check performance metrics
      expect(screen.getByText('Average Attendance')).toBeInTheDocument();
      expect(screen.getByText('87.5%')).toBeInTheDocument();

      expect(screen.getByText('Average Pass Rate')).toBeInTheDocument();
      expect(screen.getByText('92.3%')).toBeInTheDocument();

      expect(screen.getByText('Effectiveness Score')).toBeInTheDocument();
      expect(screen.getByText('89.2%')).toBeInTheDocument();
    });
  });

  it('displays upcoming trainings list', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check upcoming trainings section
      expect(screen.getByText('Upcoming Trainings')).toBeInTheDocument();
      
      // Check individual training items
      expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
      expect(screen.getByText('Equipment Operation Training')).toBeInTheDocument();
      
      // Check trainer names
      expect(screen.getByText('John Smith')).toBeInTheDocument();
      expect(screen.getByText('Mike Wilson')).toBeInTheDocument();
      
      // Check enrollment info
      expect(screen.getByText('18/25 enrolled')).toBeInTheDocument();
      expect(screen.getByText('12/15 enrolled')).toBeInTheDocument();
    });
  });

  it('displays training type distribution chart', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check chart section
      expect(screen.getByText('Training Distribution by Type')).toBeInTheDocument();
      
      // Check chart data (mocked)
      const chart = screen.getByTestId('doughnut-chart');
      expect(chart).toBeInTheDocument();
      
      // Check if chart contains training types
      expect(chart).toHaveTextContent('Safety Orientation');
      expect(chart).toHaveTextContent('Emergency Response');
      expect(chart).toHaveTextContent('K3 Training');
      expect(chart).toHaveTextContent('Technical');
      expect(chart).toHaveTextContent('Equipment');
    });
  });

  it('displays training status distribution chart', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check status chart section
      expect(screen.getByText('Training Status Distribution')).toBeInTheDocument();
      
      // Check chart data (mocked)
      const chart = screen.getAllByTestId('doughnut-chart')[1]; // Second doughnut chart
      expect(chart).toBeInTheDocument();
      
      // Check if chart contains statuses
      expect(chart).toHaveTextContent('Scheduled');
      expect(chart).toHaveTextContent('In Progress');
      expect(chart).toHaveTextContent('Completed');
      expect(chart).toHaveTextContent('Cancelled');
    });
  });

  it('displays monthly statistics trend', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check monthly trends section
      expect(screen.getByText('Monthly Training Statistics')).toBeInTheDocument();
      
      // Check chart data (mocked)
      const chart = screen.getByTestId('line-chart');
      expect(chart).toBeInTheDocument();
      
      // Check if chart contains months
      expect(chart).toHaveTextContent('Nov 2024');
      expect(chart).toHaveTextContent('Dec 2024');
    });
  });

  it('displays expiring certifications alert', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check expiring certifications section
      expect(screen.getByText('Expiring Certifications')).toBeInTheDocument();
      
      // Check certification details
      expect(screen.getByText('Jane Doe')).toBeInTheDocument();
      expect(screen.getByText('Safety Orientation')).toBeInTheDocument();
      expect(screen.getByText('CERT-20231215-001')).toBeInTheDocument();
      expect(screen.getByText('33 days')).toBeInTheDocument();
    });
  });

  it('displays quick action buttons', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check quick action buttons
      expect(screen.getByText('Create Training')).toBeInTheDocument();
      expect(screen.getByText('View All Trainings')).toBeInTheDocument();
      expect(screen.getByText('My Trainings')).toBeInTheDocument();
    });
  });

  it('handles loading state correctly', () => {
    renderWithProviders(<TrainingDashboard />);

    // Should show loading spinner initially
    expect(screen.getByRole('status')).toBeInTheDocument();
  });

  it('displays warning alerts for overdue trainings', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check for overdue trainings alert
      expect(screen.getByText('3')).toBeInTheDocument(); // Overdue count
      
      // Check if there's a warning indicator
      const overdueSection = screen.getByText('Overdue Trainings');
      expect(overdueSection).toBeInTheDocument();
    });
  });

  it('displays training effectiveness metrics with appropriate styling', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check effectiveness score styling
      const effectivenessScore = screen.getByText('89.2%');
      expect(effectivenessScore).toBeInTheDocument();
      
      // The score should be displayed prominently
      expect(effectivenessScore.closest('.metric-value')).toBeTruthy();
    });
  });

  it('displays responsive grid layout', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check that dashboard has proper grid structure
      const dashboard = screen.getByText('Training Dashboard').closest('.container-fluid');
      expect(dashboard).toBeInTheDocument();
      
      // Check for row and column structure
      const rows = dashboard?.querySelectorAll('.row');
      expect(rows).toBeTruthy();
    });
  });

  it('shows proper date formatting for upcoming trainings', async () => {
    renderWithProviders(<TrainingDashboard />);

    await waitFor(() => {
      // Check that dates are properly formatted
      // The mock data has dates that should be formatted nicely
      const upcomingSection = screen.getByText('Upcoming Trainings').closest('.card');
      expect(upcomingSection).toBeInTheDocument();
      
      // Should contain properly formatted dates (mocked data should show dates)
      expect(upcomingSection).toHaveTextContent('Dec');
    });
  });
});