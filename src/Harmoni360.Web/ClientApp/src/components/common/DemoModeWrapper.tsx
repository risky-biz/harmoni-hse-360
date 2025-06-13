import React from 'react';
import { CAlert, CSpinner, CButton } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faLock, faInfoCircle, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';
import { useApplicationMode } from '../../hooks/useApplicationMode';

interface DemoModeWrapperProps {
  children: React.ReactNode;
  requiresOperation?: string;
  requiresFeature?: string;
  fallbackContent?: React.ReactNode;
  showLimitation?: boolean;
  alternativeAction?: {
    label: string;
    onClick: () => void;
    color?: string;
  };
}

const DemoModeWrapper: React.FC<DemoModeWrapperProps> = ({
  children,
  requiresOperation,
  requiresFeature,
  fallbackContent,
  showLimitation = true,
  alternativeAction,
}) => {
  const {
    isLoading,
    error,
    isDemoMode,
    isFeatureDisabled,
    getOperationLimitation,
  } = useApplicationMode();

  // Show loading state
  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center p-3">
        <CSpinner size="sm" className="me-2" />
        <span>Loading...</span>
      </div>
    );
  }

  // Show error state
  if (error) {
    return (
      <CAlert color="warning">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        Unable to determine application mode. Proceeding with limited functionality.
      </CAlert>
    );
  }

  // Check if feature is disabled
  if (requiresFeature && isFeatureDisabled(requiresFeature)) {
    if (fallbackContent) {
      return <>{fallbackContent}</>;
    }

    if (!showLimitation) {
      return null;
    }

    return (
      <CAlert color="info">
        <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
        <strong>Feature Disabled:</strong> The "{requiresFeature}" feature is not available in demo mode.
        {alternativeAction && (
          <div className="mt-2">
            <CButton 
              color={alternativeAction.color || 'primary'} 
              size="sm"
              onClick={alternativeAction.onClick}
            >
              {alternativeAction.label}
            </CButton>
          </div>
        )}
      </CAlert>
    );
  }

  // Check operation limitation
  if (requiresOperation) {
    const limitation = getOperationLimitation(requiresOperation);
    if (limitation) {
      if (fallbackContent) {
        return <>{fallbackContent}</>;
      }

      if (!showLimitation) {
        return null;
      }

      return (
        <CAlert color="warning">
          <FontAwesomeIcon icon={faLock} className="me-2" />
          <strong>Operation Restricted:</strong> {limitation}
          {alternativeAction && (
            <div className="mt-2">
              <CButton 
                color={alternativeAction.color || 'primary'} 
                size="sm"
                onClick={alternativeAction.onClick}
              >
                {alternativeAction.label}
              </CButton>
            </div>
          )}
        </CAlert>
      );
    }
  }

  // Render children if no restrictions
  return <>{children}</>;
};

export default DemoModeWrapper;

// Convenience component for demo mode only content
export const DemoModeOnly: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isDemoMode, isLoading } = useApplicationMode();

  if (isLoading) return null;
  if (!isDemoMode) return null;

  return <>{children}</>;
};

// Convenience component for production mode only content
export const ProductionModeOnly: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isProductionMode, isLoading } = useApplicationMode();

  if (isLoading) return null;
  if (!isProductionMode) return null;

  return <>{children}</>;
};

// Convenience component for non-demo content
export const NonDemoModeOnly: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isDemoMode, isLoading } = useApplicationMode();

  if (isLoading) return null;
  if (isDemoMode) return null;

  return <>{children}</>;
};