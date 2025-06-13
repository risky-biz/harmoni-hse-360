import { describe, it, expect } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render, createAuthenticatedState } from '../utils/test-utils';
import HealthDashboard from '../../pages/health/HealthDashboard';
import HealthList from '../../pages/health/HealthList';
import CreateHealthRecord from '../../pages/health/CreateHealthRecord';
import HealthDetail from '../../pages/health/HealthDetail';

// Mock react-router-dom for navigation testing
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useParams: () => ({ id: '1' }),
  };
});

describe('Health Management Integration Tests', () => {
  const authenticatedState = createAuthenticatedState({
    roles: ['HealthManager']
  });

  describe('Complete Health Record Workflow', () => {
    it('allows user to navigate from dashboard to create new health record', async () => {
      const user = userEvent.setup();
      
      // Start at dashboard
      render(<HealthDashboard />, { initialState: authenticatedState });
      
      await waitFor(() => {
        expect(screen.getByText(/health overview/i)).toBeInTheDocument();
      });
      
      // Find and click create new record button
      const createButton = screen.getByRole('button', { name: /create.*record/i });
      await user.click(createButton);
      
      expect(mockNavigate).toHaveBeenCalledWith('/health/create');
    });

    it('allows user to create a new health record with validation', async () => {
      const user = userEvent.setup();
      
      render(<CreateHealthRecord />, { initialState: authenticatedState });
      
      // Try to submit empty form
      const submitButton = screen.getByRole('button', { name: /create.*record/i });
      await user.click(submitButton);
      
      // Should show validation errors
      await waitFor(() => {
        expect(screen.getByText(/person id is required/i)).toBeInTheDocument();
        expect(screen.getByText(/date of birth is required/i)).toBeInTheDocument();
      });
      
      // Fill out the form correctly
      await user.type(screen.getByLabelText(/person id/i), 'ST2024001');
      await user.selectOptions(screen.getByLabelText(/person type/i), 'Student');
      await user.type(screen.getByLabelText(/date of birth/i), '2010-05-15');
      await user.selectOptions(screen.getByLabelText(/blood type/i), 'O+');
      await user.type(screen.getByLabelText(/medical notes/i), 'No known allergies');
      
      // Submit the form
      await user.click(submitButton);
      
      // Should redirect to the created record
      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/health/999');
      });
    });

    it('displays health record list with filtering and pagination', async () => {
      const user = userEvent.setup();
      
      render(<HealthList />, { initialState: authenticatedState });
      
      // Wait for data to load
      await waitFor(() => {
        expect(screen.getByText('John Smith')).toBeInTheDocument();
        expect(screen.getByText('Sarah Johnson')).toBeInTheDocument();
      });
      
      // Test search functionality
      const searchInput = screen.getByPlaceholderText(/search.*name/i);
      await user.type(searchInput, 'John');
      
      // Should filter results (mocked to show same data for simplicity)
      await waitFor(() => {
        expect(screen.getByText('John Smith')).toBeInTheDocument();
      });
      
      // Test person type filter
      const personTypeFilter = screen.getByLabelText(/person type/i);
      await user.selectOptions(personTypeFilter, 'Student');
      
      // Test pagination
      const nextPageButton = screen.getByRole('button', { name: /next/i });
      if (nextPageButton && !nextPageButton.disabled) {
        await user.click(nextPageButton);
      }
    });

    it('shows detailed health record information', async () => {
      render(<HealthDetail />, { initialState: authenticatedState });
      
      // Wait for data to load
      await waitFor(() => {
        expect(screen.getByText('John Smith')).toBeInTheDocument();
        expect(screen.getByText('Student')).toBeInTheDocument();
        expect(screen.getByText('O+')).toBeInTheDocument();
      });
      
      // Check for medical conditions
      expect(screen.getByText('Nut Allergy')).toBeInTheDocument();
      expect(screen.getByText('Critical')).toBeInTheDocument();
      
      // Check for emergency contacts
      expect(screen.getByText('Mary Smith')).toBeInTheDocument();
      expect(screen.getByText('Mother')).toBeInTheDocument();
      expect(screen.getByText('+1-555-0123')).toBeInTheDocument();
      
      // Check for vaccinations
      expect(screen.getByText('COVID-19')).toBeInTheDocument();
      expect(screen.getByText('Dr. Smith')).toBeInTheDocument();
    });

    it('handles emergency contact interactions', async () => {
      const user = userEvent.setup();
      
      render(<HealthDetail />, { initialState: authenticatedState });
      
      await waitFor(() => {
        expect(screen.getByText('Mary Smith')).toBeInTheDocument();
      });
      
      // Test call emergency contact
      const callButton = screen.getByRole('button', { name: /call/i });
      await user.click(callButton);
      
      // Should trigger tel: link (mocked in test environment)
      // In real implementation, this would open phone app
      
      // Test email emergency contact
      const emailButton = screen.getByRole('button', { name: /email/i });
      await user.click(emailButton);
      
      // Should trigger mailto: link (mocked in test environment)
      
      // Test emergency notification
      const notifyButton = screen.getByRole('button', { name: /notify/i });
      await user.click(notifyButton);
      
      // Should open notification modal
      await waitFor(() => {
        expect(screen.getByText(/send emergency notification/i)).toBeInTheDocument();
      });
    });
  });

  describe('Health Dashboard Analytics', () => {
    it('displays comprehensive health metrics', async () => {
      render(<HealthDashboard />, { initialState: authenticatedState });
      
      await waitFor(() => {
        // Total records
        expect(screen.getByText('150')).toBeInTheDocument();
        
        // Compliance rate
        expect(screen.getByText('92.5%')).toBeInTheDocument();
        
        // Critical conditions
        expect(screen.getByText('8')).toBeInTheDocument();
        
        // Emergency contacts completeness
        expect(screen.getByText('95.2%')).toBeInTheDocument();
      });
    });

    it('shows vaccination compliance breakdown', async () => {
      render(<HealthDashboard />, { initialState: authenticatedState });
      
      await waitFor(() => {
        expect(screen.getByText(/vaccination compliance/i)).toBeInTheDocument();
        
        // Compliance categories
        expect(screen.getByText(/compliant/i)).toBeInTheDocument();
        expect(screen.getByText(/overdue/i)).toBeInTheDocument();
        expect(screen.getByText(/exempted/i)).toBeInTheDocument();
      });
    });

    it('displays medical condition categories', async () => {
      render(<HealthDashboard />, { initialState: authenticatedState });
      
      await waitFor(() => {
        expect(screen.getByText('Allergies')).toBeInTheDocument();
        expect(screen.getByText('Chronic Conditions')).toBeInTheDocument();
        expect(screen.getByText('Mental Health')).toBeInTheDocument();
      });
    });
  });

  describe('Error Handling and Edge Cases', () => {
    it('handles API errors gracefully', async () => {
      // Mock API error
      vi.mock('../../features/health/healthApi', () => ({
        useGetHealthRecordsQuery: () => ({
          data: null,
          isLoading: false,
          error: { message: 'Network error' },
          refetch: vi.fn()
        })
      }));
      
      render(<HealthList />, { initialState: authenticatedState });
      
      await waitFor(() => {
        expect(screen.getByText(/error.*loading/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
      });
    });

    it('shows loading states appropriately', () => {
      vi.mock('../../features/health/healthApi', () => ({
        useGetHealthDashboardQuery: () => ({
          data: null,
          isLoading: true,
          error: null,
          refetch: vi.fn()
        })
      }));
      
      render(<HealthDashboard />, { initialState: authenticatedState });
      
      expect(screen.getByRole('status')).toBeInTheDocument(); // Loading spinner
    });

    it('handles empty data states', async () => {
      vi.mock('../../features/health/healthApi', () => ({
        useGetHealthRecordsQuery: () => ({
          data: { items: [], totalCount: 0, pageNumber: 1, pageSize: 10 },
          isLoading: false,
          error: null,
          refetch: vi.fn()
        })
      }));
      
      render(<HealthList />, { initialState: authenticatedState });
      
      await waitFor(() => {
        expect(screen.getByText(/no health records found/i)).toBeInTheDocument();
      });
    });
  });
});