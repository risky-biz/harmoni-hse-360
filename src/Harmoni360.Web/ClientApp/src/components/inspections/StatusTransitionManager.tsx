import React, { useState } from 'react';
import {
  CButton,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CForm,
  CFormTextarea,
  CFormLabel,
  CAlert,
  CSpinner,
  CBadge
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlay,
  faCheck,
  faTimes,
  faExclamationTriangle,
  faInfoCircle,
  faSave
} from '@fortawesome/free-solid-svg-icons';
import { toast } from 'react-toastify';
import {
  useStartInspectionMutation,
  useCompleteInspectionMutation
} from '../../features/inspections/inspectionApi';
import { InspectionDetailDto, InspectionStatus } from '../../types/inspection';

interface StatusTransitionManagerProps {
  inspection: InspectionDetailDto;
  onSuccess: () => void;
}

type TransitionType = 'start' | 'complete' | 'cancel' | null;

export const StatusTransitionManager: React.FC<StatusTransitionManagerProps> = ({
  inspection,
  onSuccess
}) => {
  const [activeTransition, setActiveTransition] = useState<TransitionType>(null);
  const [completionSummary, setCompletionSummary] = useState('');
  const [completionRecommendations, setCompletionRecommendations] = useState('');

  const [startInspection, { isLoading: isStarting }] = useStartInspectionMutation();
  const [completeInspection, { isLoading: isCompleting }] = useCompleteInspectionMutation();

  const handleStartInspection = async () => {
    try {
      await startInspection(inspection.id).unwrap();
      toast.success('Inspection started successfully');
      setActiveTransition(null);
      onSuccess();
    } catch (error: any) {
      console.error('Failed to start inspection:', error);
      toast.error(error?.data?.message || 'Failed to start inspection');
    }
  };

  const handleCompleteInspection = async () => {
    if (!completionSummary.trim()) {
      toast.error('Please provide a summary before completing the inspection');
      return;
    }

    try {
      await completeInspection({
        inspectionId: inspection.id,
        summary: completionSummary,
        recommendations: completionRecommendations,
        findings: [] // Findings are already added through the interface
      }).unwrap();
      
      toast.success('Inspection completed successfully');
      setActiveTransition(null);
      setCompletionSummary('');
      setCompletionRecommendations('');
      onSuccess();
    } catch (error: any) {
      console.error('Failed to complete inspection:', error);
      toast.error(error?.data?.message || 'Failed to complete inspection');
    }
  };

  const getStatusBadge = (status: InspectionStatus) => {
    const statusConfig = {
      [InspectionStatus.Draft]: { color: 'secondary', text: 'Draft' },
      [InspectionStatus.Scheduled]: { color: 'info', text: 'Scheduled' },
      [InspectionStatus.InProgress]: { color: 'warning', text: 'In Progress' },
      [InspectionStatus.Completed]: { color: 'success', text: 'Completed' },
      [InspectionStatus.Cancelled]: { color: 'danger', text: 'Cancelled' }
    };
    const config = statusConfig[status] || { color: 'secondary', text: status };
    return <CBadge color={config.color}>{config.text}</CBadge>;
  };

  const getCompletionRequirements = () => {
    const requirements = [];
    
    if (inspection.items && inspection.items.length > 0) {
      const completedItems = inspection.items.filter(item => item.isCompleted).length;
      const totalItems = inspection.items.length;
      requirements.push(`Checklist: ${completedItems}/${totalItems} items completed`);
    }

    if (inspection.criticalFindingsCount > 0) {
      requirements.push(`${inspection.criticalFindingsCount} critical finding${inspection.criticalFindingsCount > 1 ? 's' : ''} recorded`);
    }

    return requirements;
  };

  const canComplete = () => {
    if (!inspection.items || inspection.items.length === 0) return true;
    
    const requiredItems = inspection.items.filter(item => item.isRequired);
    const completedRequiredItems = requiredItems.filter(item => item.isCompleted);
    
    return completedRequiredItems.length === requiredItems.length;
  };

  return (
    <>
      {/* Action Buttons */}
      <div className="d-flex gap-2">
        {inspection.canStart && (
          <CButton
            color="success"
            variant="outline"
            onClick={() => setActiveTransition('start')}
          >
            <FontAwesomeIcon icon={faPlay} className="me-1" />
            Start Inspection
          </CButton>
        )}
        
        {inspection.canComplete && (
          <CButton
            color="primary"
            onClick={() => setActiveTransition('complete')}
            disabled={!canComplete()}
          >
            <FontAwesomeIcon icon={faCheck} className="me-1" />
            Complete Inspection
          </CButton>
        )}
      </div>

      {/* Start Inspection Modal */}
      <CModal 
        visible={activeTransition === 'start'} 
        onClose={() => setActiveTransition(null)}
      >
        <CModalHeader>
          <CModalTitle>
            <FontAwesomeIcon icon={faPlay} className="me-2 text-success" />
            Start Inspection
          </CModalTitle>
        </CModalHeader>
        <CModalBody>
          <div className="mb-3">
            <h6>Current Status: {getStatusBadge(inspection.status)}</h6>
          </div>
          
          <CAlert color="info">
            <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
            Starting this inspection will:
            <ul className="mb-0 mt-2">
              <li>Change the status to "In Progress"</li>
              <li>Record the start time</li>
              <li>Enable checklist item responses</li>
              <li>Allow findings to be recorded</li>
            </ul>
          </CAlert>

          <p>Are you ready to begin this inspection?</p>
        </CModalBody>
        <CModalFooter>
          <CButton
            color="secondary"
            onClick={() => setActiveTransition(null)}
            disabled={isStarting}
          >
            Cancel
          </CButton>
          <CButton
            color="success"
            onClick={handleStartInspection}
            disabled={isStarting}
          >
            {isStarting ? (
              <>
                <CSpinner size="sm" className="me-2" />
                Starting...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faPlay} className="me-1" />
                Start Inspection
              </>
            )}
          </CButton>
        </CModalFooter>
      </CModal>

      {/* Complete Inspection Modal */}
      <CModal 
        visible={activeTransition === 'complete'} 
        onClose={() => setActiveTransition(null)}
        size="lg"
        backdrop="static"
      >
        <CModalHeader>
          <CModalTitle>
            <FontAwesomeIcon icon={faCheck} className="me-2 text-primary" />
            Complete Inspection
          </CModalTitle>
        </CModalHeader>
        <CModalBody>
          <div className="mb-4">
            <h6>Current Status: {getStatusBadge(inspection.status)}</h6>
          </div>

          {/* Completion Requirements Check */}
          <div className="mb-4">
            <h6>Completion Status</h6>
            {getCompletionRequirements().map((requirement, index) => (
              <div key={index} className="d-flex align-items-center mb-1">
                <FontAwesomeIcon 
                  icon={faCheck} 
                  className="text-success me-2" 
                />
                <span>{requirement}</span>
              </div>
            ))}
          </div>

          {!canComplete() && (
            <CAlert color="warning" className="mb-4">
              <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
              Some required checklist items are not completed. You can still complete the inspection, 
              but consider completing all required items first.
            </CAlert>
          )}

          <CForm>
            <div className="mb-3">
              <CFormLabel htmlFor="completionSummary">Summary *</CFormLabel>
              <CFormTextarea
                id="completionSummary"
                rows={4}
                value={completionSummary}
                onChange={(e) => setCompletionSummary(e.target.value)}
                placeholder="Provide a summary of the inspection results, key observations, and overall findings..."
              />
              <small className="text-muted">
                Summarize the overall inspection outcome and any significant observations.
              </small>
            </div>
            
            <div className="mb-3">
              <CFormLabel htmlFor="completionRecommendations">Recommendations</CFormLabel>
              <CFormTextarea
                id="completionRecommendations"
                rows={3}
                value={completionRecommendations}
                onChange={(e) => setCompletionRecommendations(e.target.value)}
                placeholder="Provide recommendations for improvements, follow-up actions, or next steps..."
              />
              <small className="text-muted">
                Include any recommendations based on the inspection findings and observations.
              </small>
            </div>
          </CForm>

          <CAlert color="info">
            <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
            Completing this inspection will:
            <ul className="mb-0 mt-2">
              <li>Change the status to "Completed"</li>
              <li>Record the completion time</li>
              <li>Lock the inspection for further editing</li>
              <li>Generate final inspection report</li>
              <li>Trigger any automated notifications</li>
            </ul>
          </CAlert>
        </CModalBody>
        <CModalFooter>
          <CButton
            color="secondary"
            onClick={() => {
              setActiveTransition(null);
              setCompletionSummary('');
              setCompletionRecommendations('');
            }}
            disabled={isCompleting}
          >
            Cancel
          </CButton>
          <CButton
            color="primary"
            onClick={handleCompleteInspection}
            disabled={isCompleting || !completionSummary.trim()}
          >
            {isCompleting ? (
              <>
                <CSpinner size="sm" className="me-2" />
                Completing...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faCheck} className="me-1" />
                Complete Inspection
              </>
            )}
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};