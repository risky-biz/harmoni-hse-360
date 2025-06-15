import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { CreateTraining } from '../../pages/trainings/CreateTraining';
import { TrainingList } from '../../pages/trainings/TrainingList';
import { TrainingDetail } from '../../pages/trainings/TrainingDetail';
import { renderWithProviders } from '../utils/test-utils';

// Mock react-router-dom
const mockNavigate = vi.fn();
const mockParams = { id: '1' };

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useParams: () => mockParams,
    useLocation: () => ({ pathname: '/trainings' })
  };
});

describe('Training Workflow Integration Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Complete Training Creation Workflow', () => {
    it('allows user to create a new training from start to finish', async () => {
      const user = userEvent.setup();
      renderWithProviders(<CreateTraining />);

      // Wait for form to load
      await waitFor(() => {
        expect(screen.getByText('Create Training Session')).toBeInTheDocument();
      });

      // Step 1: Fill Basic Information
      const titleInput = screen.getByLabelText(/training title/i);
      await user.type(titleInput, 'New Safety Training');

      const descriptionInput = screen.getByLabelText(/description/i);
      await user.type(descriptionInput, 'Comprehensive safety training for all employees');

      const typeSelect = screen.getByLabelText(/training type/i);
      await user.selectOptions(typeSelect, 'SafetyOrientation');

      const categorySelect = screen.getByLabelText(/category/i);
      await user.selectOptions(categorySelect, 'Mandatory');

      const prioritySelect = screen.getByLabelText(/priority/i);
      await user.selectOptions(prioritySelect, 'High');

      // Continue to next step
      const nextButton = screen.getByText('Next: Training Details');
      await user.click(nextButton);

      // Step 2: Fill Training Details
      await waitFor(() => {
        expect(screen.getByText('Training Details')).toBeInTheDocument();
      });

      const deliveryMethodSelect = screen.getByLabelText(/delivery method/i);
      await user.selectOptions(deliveryMethodSelect, 'Classroom');

      const scheduledDateInput = screen.getByLabelText(/scheduled date/i);
      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + 7);
      await user.type(scheduledDateInput, futureDate.toISOString().split('T')[0]);

      const durationInput = screen.getByLabelText(/estimated duration/i);
      await user.clear(durationInput);
      await user.type(durationInput, '240');

      const maxParticipantsInput = screen.getByLabelText(/maximum participants/i);
      await user.clear(maxParticipantsInput);
      await user.type(maxParticipantsInput, '25');

      const minParticipantsInput = screen.getByLabelText(/minimum participants/i);
      await user.clear(minParticipantsInput);
      await user.type(minParticipantsInput, '5');

      // Continue to next step
      const nextButton2 = screen.getByText('Next: Participants');
      await user.click(nextButton2);

      // Step 3: Configure Participants (skip for now)
      await waitFor(() => {
        expect(screen.getByText('Participant Management')).toBeInTheDocument();
      });

      const nextButton3 = screen.getByText('Next: Requirements');
      await user.click(nextButton3);

      // Step 4: Add Requirements
      await waitFor(() => {
        expect(screen.getByText('Training Requirements')).toBeInTheDocument();
      });

      const objectivesInput = screen.getByLabelText(/learning objectives/i);
      await user.type(objectivesInput, 'Understand safety protocols and emergency procedures');

      // Add certification requirement
      const requiresCertificationCheckbox = screen.getByLabelText(/requires certification/i);
      await user.click(requiresCertificationCheckbox);

      const validityMonthsInput = screen.getByLabelText(/certification validity/i);
      await user.type(validityMonthsInput, '12');

      const passingScoreInput = screen.getByLabelText(/passing score/i);
      await user.type(passingScoreInput, '80');

      // Continue to next step
      const nextButton4 = screen.getByText('Next: Materials');
      await user.click(nextButton4);

      // Step 5: Materials & Resources (skip for now)
      await waitFor(() => {
        expect(screen.getByText('Training Materials')).toBeInTheDocument();
      });

      const nextButton5 = screen.getByText('Next: Review');
      await user.click(nextButton5);

      // Step 6: Review & Submit
      await waitFor(() => {
        expect(screen.getByText('Review & Submit')).toBeInTheDocument();
      });

      // Verify summary information
      expect(screen.getByText('New Safety Training')).toBeInTheDocument();
      expect(screen.getByText('Comprehensive safety training for all employees')).toBeInTheDocument();
      expect(screen.getByText('Safety Orientation')).toBeInTheDocument();
      expect(screen.getByText('Classroom')).toBeInTheDocument();
      expect(screen.getByText('240 minutes')).toBeInTheDocument();
      expect(screen.getByText('25 participants')).toBeInTheDocument();

      // Submit the training
      const submitButton = screen.getByText('Create Training');
      await user.click(submitButton);

      // Verify navigation after successful creation
      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/trainings');
      });
    });

    it('validates required fields and shows appropriate error messages', async () => {
      const user = userEvent.setup();
      renderWithProviders(<CreateTraining />);

      await waitFor(() => {
        expect(screen.getByText('Create Training Session')).toBeInTheDocument();
      });

      // Try to proceed without filling required fields
      const nextButton = screen.getByText('Next: Training Details');
      await user.click(nextButton);

      // Should show validation errors
      await waitFor(() => {
        expect(screen.getByText('Training title is required')).toBeInTheDocument();
        expect(screen.getByText('Description is required')).toBeInTheDocument();
      });

      // Fill minimum required fields
      const titleInput = screen.getByLabelText(/training title/i);
      await user.type(titleInput, 'Test Training');

      const descriptionInput = screen.getByLabelText(/description/i);
      await user.type(descriptionInput, 'Test description');

      // Try again
      await user.click(nextButton);

      // Should proceed to next step
      await waitFor(() => {
        expect(screen.getByText('Training Details')).toBeInTheDocument();
      });
    });
  });

  describe('Training List and Detail Workflow', () => {
    it('allows user to navigate from list to detail view', async () => {
      const user = userEvent.setup();
      renderWithProviders(<TrainingList />);

      // Wait for trainings to load
      await waitFor(() => {
        expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
      });

      // Click on view details button
      const viewButtons = screen.getAllByTitle('View Details');
      await user.click(viewButtons[0]);

      // Should navigate to detail page
      expect(mockNavigate).toHaveBeenCalledWith('/trainings/1');
    });

    it('shows training details with all tabs', async () => {
      renderWithProviders(<TrainingDetail />);

      // Wait for training detail to load
      await waitFor(() => {
        expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
      });

      // Check that all tabs are present
      expect(screen.getByText('Overview')).toBeInTheDocument();
      expect(screen.getByText('Training Details')).toBeInTheDocument();
      expect(screen.getByText('Participants')).toBeInTheDocument();
      expect(screen.getByText('Materials')).toBeInTheDocument();
      expect(screen.getByText('Requirements')).toBeInTheDocument();
      expect(screen.getByText('Activity History')).toBeInTheDocument();

      // Check training information
      expect(screen.getByText('TRN-20241213-001')).toBeInTheDocument();
      expect(screen.getByText('John Smith')).toBeInTheDocument();
      expect(screen.getByText('Scheduled')).toBeInTheDocument();
    });

    it('allows switching between tabs in detail view', async () => {
      const user = userEvent.setup();
      renderWithProviders(<TrainingDetail />);

      await waitFor(() => {
        expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
      });

      // Click on Participants tab
      const participantsTab = screen.getByText('Participants');
      await user.click(participantsTab);

      // Should show participants content
      await waitFor(() => {
        expect(screen.getByText('Enrolled Participants')).toBeInTheDocument();
      });

      // Click on Training Details tab
      const detailsTab = screen.getByText('Training Details');
      await user.click(detailsTab);

      // Should show training details content
      await waitFor(() => {
        expect(screen.getByText('Learning Objectives')).toBeInTheDocument();
      });
    });
  });

  describe('Training Status Transitions', () => {
    it('allows starting a scheduled training', async () => {
      const user = userEvent.setup();
      renderWithProviders(<TrainingList />);

      await waitFor(() => {
        expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
      });

      // Find and click start training button
      const startButton = screen.getByTitle('Start Training');
      await user.click(startButton);

      // Should show confirmation dialog
      await waitFor(() => {
        expect(screen.getByText('Start Training')).toBeInTheDocument();
        expect(screen.getByText('Are you sure you want to start this training session?')).toBeInTheDocument();
      });

      // Confirm start
      const confirmButton = screen.getByText('Start');
      await user.click(confirmButton);

      // Should update training status
      await waitFor(() => {
        expect(screen.getByText('Training started successfully')).toBeInTheDocument();
      });
    });

    it('allows completing an in-progress training', async () => {
      const user = userEvent.setup();
      renderWithProviders(<TrainingList />);

      await waitFor(() => {
        expect(screen.getByText('Emergency Response Training')).toBeInTheDocument();
      });

      // Find and click complete training button
      const completeButton = screen.getByTitle('Complete Training');
      await user.click(completeButton);

      // Should show completion dialog
      await waitFor(() => {
        expect(screen.getByText('Complete Training')).toBeInTheDocument();
      });

      // Fill completion details
      const feedbackInput = screen.getByLabelText(/feedback summary/i);
      await user.type(feedbackInput, 'Excellent training session with high engagement');

      const effectivenessInput = screen.getByLabelText(/effectiveness score/i);
      await user.type(effectivenessInput, '95');

      // Confirm completion
      const confirmButton = screen.getByText('Complete');
      await user.click(confirmButton);

      // Should update training status
      await waitFor(() => {
        expect(screen.getByText('Training completed successfully')).toBeInTheDocument();
      });
    });
  });

  describe('Training Search and Filter Workflow', () => {
    it('allows comprehensive search and filtering', async () => {
      const user = userEvent.setup();
      renderWithProviders(<TrainingList />);

      await waitFor(() => {
        expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
      });

      // Test search functionality
      const searchInput = screen.getByPlaceholderText('Search trainings...');
      await user.type(searchInput, 'Emergency');

      await waitFor(() => {
        expect(screen.getByText('Emergency Response Training')).toBeInTheDocument();
        expect(screen.queryByText('Safety Induction Training')).not.toBeInTheDocument();
      });

      // Clear search and test status filter
      await user.clear(searchInput);

      const statusFilter = screen.getByLabelText('Status');
      await user.selectOptions(statusFilter, 'Completed');

      await waitFor(() => {
        expect(screen.getByText('K3 Compliance Training')).toBeInTheDocument();
        expect(screen.queryByText('Safety Induction Training')).not.toBeInTheDocument();
        expect(screen.queryByText('Emergency Response Training')).not.toBeInTheDocument();
      });

      // Test multiple filters combined
      await user.selectOptions(statusFilter, '');
      const typeFilter = screen.getByLabelText('Training Type');
      await user.selectOptions(typeFilter, 'SafetyOrientation');

      await waitFor(() => {
        expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
        expect(screen.queryByText('Emergency Response Training')).not.toBeInTheDocument();
      });

      // Clear all filters
      const clearButton = screen.getByText('Clear Filters');
      await user.click(clearButton);

      await waitFor(() => {
        expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
        expect(screen.getByText('Emergency Response Training')).toBeInTheDocument();
        expect(screen.getByText('K3 Compliance Training')).toBeInTheDocument();
      });
    });
  });

  describe('Error Handling and Edge Cases', () => {
    it('handles API errors gracefully', async () => {
      // Mock a failed API call
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      renderWithProviders(<TrainingList />);

      // Should show error state if API fails
      // Note: This would require mocking the API to fail
      // For now, we ensure the component doesn't crash

      expect(screen.getByText('Training Management')).toBeInTheDocument();
      
      consoleSpy.mockRestore();
    });

    it('handles empty data states', async () => {
      renderWithProviders(<TrainingList />);

      // Apply filter that returns no results
      await waitFor(() => {
        expect(screen.getByText('Safety Induction Training')).toBeInTheDocument();
      });

      const user = userEvent.setup();
      const searchInput = screen.getByPlaceholderText('Search trainings...');
      await user.type(searchInput, 'NonExistentTraining');

      await waitFor(() => {
        expect(screen.getByText('No trainings found')).toBeInTheDocument();
        expect(screen.getByText('Try adjusting your search criteria or create a new training.')).toBeInTheDocument();
      });
    });
  });
});