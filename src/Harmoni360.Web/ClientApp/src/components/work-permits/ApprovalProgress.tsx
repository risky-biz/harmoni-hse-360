import React from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CProgress,
  CProgressBar,
  CBadge,
  CTooltip,
  CRow,
  CCol
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faCheck,
  faClock,
  faInfoCircle
} from '@fortawesome/free-solid-svg-icons';

interface ApprovalProgressProps {
  requiredApprovalLevels: string[];
  receivedApprovalLevels: string[];
  missingApprovalLevels: string[];
  approvalProgress: number;
  workPermitType: string;
  canBypassApprovals?: boolean;
}

const ApprovalProgress: React.FC<ApprovalProgressProps> = ({
  requiredApprovalLevels,
  receivedApprovalLevels,
  missingApprovalLevels,
  approvalProgress,
  workPermitType,
  canBypassApprovals = false
}) => {
  const getApprovalRequirementsByType = (type: string): string => {
    switch (type) {
      case 'HotWork':
        return 'SafetyOfficer + DepartmentHead + HotWorkSpecialist';
      case 'ConfinedSpace':
        return 'SafetyOfficer + DepartmentHead + ConfinedSpaceSpecialist';
      case 'ElectricalWork':
        return 'SafetyOfficer + ElectricalSupervisor + DepartmentHead';
      case 'Special':
        return 'SafetyOfficer + DepartmentHead + SpecialWorkSpecialist + HSEManager';
      case 'General':
      case 'ColdWork':
      default:
        return 'SafetyOfficer + DepartmentHead';
    }
  };

  const formatApprovalLevel = (level: string): string => {
    return level.replace(/([A-Z])/g, ' $1').trim();
  };

  const getProgressColor = (): string => {
    if (canBypassApprovals) return 'info';
    if (approvalProgress === 100) return 'success';
    if (approvalProgress >= 50) return 'warning';
    return 'danger';
  };

  return (
    <CCard>
      <CCardHeader className="d-flex justify-content-between align-items-center">
        <h6 className="mb-0">Approval Progress</h6>
        <CTooltip
          content={
            <div>
              <strong>Required Approvals for {workPermitType}:</strong><br />
              {getApprovalRequirementsByType(workPermitType)}
              {canBypassApprovals && (
                <>
                  <hr className="my-2" />
                  <div className="text-info">
                    <FontAwesomeIcon icon={faInfoCircle} className="me-1" />
                    You have permission to bypass multi-level approvals
                  </div>
                </>
              )}
            </div>
          }
          placement="left"
        >
          <FontAwesomeIcon 
            icon={faInfoCircle} 
            className="text-muted" 
            style={{ cursor: 'help' }}
          />
        </CTooltip>
      </CCardHeader>
      <CCardBody>
        <div className="mb-3">
          <div className="d-flex justify-content-between align-items-center mb-2">
            <span className="small text-muted">
              {receivedApprovalLevels.length} of {requiredApprovalLevels.length} approvals received
            </span>
            <span className="small font-weight-bold">
              {approvalProgress}%
            </span>
          </div>
          <CProgress height={8}>
            <CProgressBar 
              color={getProgressColor()} 
              value={approvalProgress}
            />
          </CProgress>
          {canBypassApprovals && (
            <div className="text-info small mt-1">
              <FontAwesomeIcon icon={faInfoCircle} className="me-1" />
              Bypass available
            </div>
          )}
        </div>

        <CRow>
          <CCol>
            <div className="mb-3">
              <h6 className="small text-muted mb-2">✓ Received Approvals</h6>
              {receivedApprovalLevels.length > 0 ? (
                receivedApprovalLevels.map((level, index) => (
                  <div key={index} className="mb-1">
                    <CBadge color="success" className="me-1">
                      <FontAwesomeIcon icon={faCheck} className="me-1" />
                      {formatApprovalLevel(level)}
                    </CBadge>
                  </div>
                ))
              ) : (
                <div className="text-muted small">None received yet</div>
              )}
            </div>
          </CCol>
        </CRow>

        <CRow>
          <CCol>
            <div className="mb-3">
              <h6 className="small text-muted mb-2">⏳ Pending Approvals</h6>
              {missingApprovalLevels.length > 0 ? (
                missingApprovalLevels.map((level, index) => (
                  <div key={index} className="mb-1">
                    <CBadge color="warning" className="me-1">
                      <FontAwesomeIcon icon={faClock} className="me-1" />
                      {formatApprovalLevel(level)}
                    </CBadge>
                  </div>
                ))
              ) : (
                <div className="text-success small">All approvals received!</div>
              )}
            </div>
          </CCol>
        </CRow>
      </CCardBody>
    </CCard>
  );
};

export default ApprovalProgress;