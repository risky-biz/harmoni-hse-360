import { describe, it, expect, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { render, createAuthenticatedState } from '../../utils/test-utils';
import HealthDashboard from '../../../pages/health/HealthDashboard';

describe('HealthDashboard Component', () => {
  it('renders dashboard with health metrics', async () => {
    render(<HealthDashboard />, {
      initialState: createAuthenticatedState()
    });
    
    // Wait for data to load
    await waitFor(() => {
      expect(screen.getByText(/health overview/i)).toBeInTheDocument();
    });
    
    // Check for key metrics
    expect(screen.getByText(/150/)).toBeInTheDocument(); // Total records
    expect(screen.getByText(/92.5%/)).toBeInTheDocument(); // Compliance rate
    expect(screen.getByText(/45/)).toBeInTheDocument(); // Medical conditions
  });

  it('displays vaccination compliance chart', async () => {
    render(<HealthDashboard />, {
      initialState: createAuthenticatedState()
    });
    
    await waitFor(() => {
      expect(screen.getByText(/vaccination compliance/i)).toBeInTheDocument();
    });
    
    // Check for compliance data
    expect(screen.getByText(/compliant/i)).toBeInTheDocument();
    expect(screen.getByText(/overdue/i)).toBeInTheDocument();
    expect(screen.getByText(/exempted/i)).toBeInTheDocument();
  });

  it('shows critical health alerts', async () => {
    render(<HealthDashboard />, {
      initialState: createAuthenticatedState()
    });
    
    await waitFor(() => {
      expect(screen.getByText(/critical conditions/i)).toBeInTheDocument();
    });
    
    // Check for critical condition count
    expect(screen.getByText(/8/)).toBeInTheDocument(); // Critical conditions
    expect(screen.getByText(/2/)).toBeInTheDocument(); // Life threatening
  });

  it('displays recent health activity', async () => {
    render(<HealthDashboard />, {
      initialState: createAuthenticatedState()
    });
    
    await waitFor(() => {
      expect(screen.getByText(/recent activity/i)).toBeInTheDocument();
    });
    
    // Check for activity sections
    expect(screen.getByText(/health records/i)).toBeInTheDocument();
    expect(screen.getByText(/health incidents/i)).toBeInTheDocument();
    expect(screen.getByText(/expiring vaccinations/i)).toBeInTheDocument();
  });

  it('shows loading state while fetching data', () => {
    render(<HealthDashboard />, {
      initialState: createAuthenticatedState()
    });
    
    expect(screen.getByRole('status')).toBeInTheDocument(); // Loading spinner
  });

  it('handles error state gracefully', async () => {
    // Mock API error
    vi.mock('../../../features/health/healthApi', () => ({
      useGetHealthDashboardQuery: () => ({
        data: null,
        isLoading: false,
        error: { message: 'Failed to fetch dashboard data' },
        refetch: vi.fn()
      })
    }));
    
    render(<HealthDashboard />, {
      initialState: createAuthenticatedState()
    });
    
    await waitFor(() => {
      expect(screen.getByText(/error loading dashboard/i)).toBeInTheDocument();
    });
    
    expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
  });

  it('allows refreshing dashboard data', async () => {
    const mockRefetch = vi.fn();
    
    vi.mock('../../../features/health/healthApi', () => ({
      useGetHealthDashboardQuery: () => ({
        data: { /* mock data */ },
        isLoading: false,
        error: null,
        refetch: mockRefetch
      })
    }));
    
    render(<HealthDashboard />, {
      initialState: createAuthenticatedState()
    });
    
    const refreshButton = screen.getByRole('button', { name: /refresh/i });
    await user.click(refreshButton);
    
    expect(mockRefetch).toHaveBeenCalled();
  });
});