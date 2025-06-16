import React, { useState, useEffect } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CBadge,
  CProgress,
  CAlert,
  CSpinner,
  CRow,
  CCol,
  CTooltip,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faRefresh,
  faClock,
  faDatabase,
  faExclamationTriangle,
  faCheckCircle,
  faInfoCircle,
} from '@fortawesome/free-solid-svg-icons';
import { demoResetService, DemoResetStatus, DemoResetResult } from '../../services/demoResetService';
import { useApplicationMode } from '../../hooks/useApplicationMode';

interface DemoResetStatusProps {
  className?: string;
  showManualReset?: boolean;
  compact?: boolean;
}

export const DemoResetStatusComponent: React.FC<DemoResetStatusProps> = ({
  className = '',
  showManualReset = true,
  compact = false,
}) => {
  const { isDemoMode } = useApplicationMode();
  const [status, setStatus] = useState<DemoResetStatus | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isResetting, setIsResetting] = useState(false);
  const [lastResetResult, setLastResetResult] = useState<DemoResetResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isDemoMode) return;

    loadStatus();
    const interval = setInterval(loadStatus, 60000); // Update every minute

    return () => clearInterval(interval);
  }, [isDemoMode]);

  const loadStatus = async () => {
    try {
      setError(null);
      const resetStatus = await demoResetService.getResetStatus();
      setStatus(resetStatus);
      setIsResetting(resetStatus.isResetInProgress);
    } catch (err) {
      setError('Failed to load reset status');
      console.error('Failed to load demo reset status:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleManualReset = async () => {
    try {
      setIsResetting(true);
      setError(null);
      const result = await demoResetService.manualReset();
      setLastResetResult(result);
    } catch (err) {
      if (err instanceof Error && err.message.includes('cancelled')) {
        // User cancelled, don't show error
        return;
      }
      setError('Failed to perform manual reset');
      console.error('Manual reset failed:', err);
    } finally {
      setIsResetting(false);
      await loadStatus();
    }
  };

  const getTimeUntilResetProgress = (): number => {
    if (!status) return 0;
    
    const now = new Date();
    const nextReset = new Date(status.nextResetAt);
    const lastReset = new Date(status.lastResetAt);
    
    const totalInterval = nextReset.getTime() - lastReset.getTime();
    const elapsed = now.getTime() - lastReset.getTime();
    
    return Math.min(100, Math.max(0, (elapsed / totalInterval) * 100));
  };

  const formatNextResetTime = (): string => {
    if (!status) return '';
    const nextReset = new Date(status.nextResetAt);
    return nextReset.toLocaleString();
  };

  if (!isDemoMode) {
    return null;
  }

  if (isLoading) {
    return (
      <CCard className={className}>
        <CCardBody className="text-center">
          <CSpinner size="sm" className="me-2" />
          Loading reset status...
        </CCardBody>
      </CCard>
    );
  }

  if (compact) {
    return (
      <CAlert color="info" className={`d-flex align-items-center ${className}`}>
        <FontAwesomeIcon icon={faClock} className="me-2" />
        <span className="me-auto">
          Next reset: {status?.timeUntilNextReset || 'Unknown'}
        </span>
        {showManualReset && (
          <CButton
            color="secondary"
            size="sm"
            onClick={handleManualReset}
            disabled={isResetting}
          >
            {isResetting ? (
              <>
                <CSpinner size="sm" className="me-1" />
                Resetting...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faRefresh} className="me-1" />
                Reset Now
              </>
            )}
          </CButton>
        )}
      </CAlert>
    );
  }

  return (
    <CCard className={className}>
      <CCardHeader className="d-flex align-items-center justify-content-between">
        <div className="d-flex align-items-center">
          <FontAwesomeIcon icon={faDatabase} className="me-2 text-info" />
          <strong>Demo Environment Status</strong>
        </div>
        <CBadge color="info">
          <FontAwesomeIcon icon={faInfoCircle} className="me-1" />
          Demo Mode
        </CBadge>
      </CCardHeader>

      <CCardBody>
        {error && (
          <CAlert color="danger" className="mb-3">
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
            {error}
          </CAlert>
        )}

        {lastResetResult && (
          <CAlert color="success" className="mb-3">
            <FontAwesomeIcon icon={faCheckCircle} className="me-2" />
            Reset completed successfully! {lastResetResult.itemsReset.incidents + 
            lastResetResult.itemsReset.workPermits + lastResetResult.itemsReset.audits} 
            items were reset.
          </CAlert>
        )}

        {isResetting && (
          <CAlert color="warning" className="mb-3">
            <CSpinner size="sm" className="me-2" />
            Demo reset in progress... Please wait.
          </CAlert>
        )}

        {status && (
          <CRow>
            <CCol md={6}>
              <div className="mb-3">
                <div className="d-flex align-items-center justify-content-between mb-2">
                  <span className="text-muted">Time until next reset:</span>
                  <CBadge color="secondary">{status.timeUntilNextReset}</CBadge>
                </div>
                <CProgress
                  value={getTimeUntilResetProgress()}
                  color="info"
                  className="mb-2"
                />
                <small className="text-muted">
                  Scheduled: {formatNextResetTime()}
                </small>
              </div>
            </CCol>

            <CCol md={6}>
              <div className="mb-3">
                <div className="d-flex align-items-center justify-content-between mb-2">
                  <span className="text-muted">Last reset:</span>
                  <span className="text-body">
                    {new Date(status.lastResetAt).toLocaleDateString()}
                  </span>
                </div>
                <div className="d-flex align-items-center justify-content-between">
                  <span className="text-muted">Total resets:</span>
                  <CBadge color="secondary">{status.totalResets}</CBadge>
                </div>
              </div>
            </CCol>
          </CRow>
        )}

        <hr />

        <div className="d-flex align-items-center justify-content-between">
          <div>
            <CTooltip content="This demo environment automatically resets every 24 hours to maintain a clean state">
              <span className="text-muted">
                <FontAwesomeIcon icon={faInfoCircle} className="me-1" />
                Automatic 24-hour reset cycle
              </span>
            </CTooltip>
          </div>

          {showManualReset && (
            <CButton
              color="warning"
              variant="outline"
              onClick={handleManualReset}
              disabled={isResetting}
            >
              {isResetting ? (
                <>
                  <CSpinner size="sm" className="me-2" />
                  Resetting...
                </>
              ) : (
                <>
                  <FontAwesomeIcon icon={faRefresh} className="me-2" />
                  Manual Reset
                </>
              )}
            </CButton>
          )}
        </div>

        <CAlert color="info" className="mt-3 mb-0">
          <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
          <strong>Demo Environment:</strong> All functionality is enabled for demonstration purposes. 
          Data is automatically reset to showcase the full capabilities of HarmoniHSE360.
        </CAlert>
      </CCardBody>
    </CCard>
  );
};

export default DemoResetStatusComponent;