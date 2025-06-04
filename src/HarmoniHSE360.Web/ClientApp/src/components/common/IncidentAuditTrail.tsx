import React from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CSpinner,
  CAlert,
  CListGroup,
  CListGroupItem,
  CBadge,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faHistory, 
  faPlus, 
  faEdit, 
  faTrash, 
  faEye,
  faFileAlt,
  faExclamationTriangle,
  faUserPlus,
  faUserMinus,
  faComment,
  faDownload,
  faTasks,
  faClipboardList
} from '@fortawesome/free-solid-svg-icons';
import { useGetIncidentAuditTrailQuery, IncidentAuditLogDto } from '../../features/incidents/incidentApi';
import { formatDateTime, formatRelativeTime } from '../../utils/dateUtils';

interface IncidentAuditTrailProps {
  incidentId: number;
}

const getActionIcon = (action: string) => {
  switch (action) {
    case 'Created':
      return faPlus;
    case 'Updated':
      return faEdit;
    case 'Viewed':
      return faEye;
    case 'Status Changed':
    case 'Severity Changed':
      return faExclamationTriangle;
    case 'Attachment Added':
      return faFileAlt;
    case 'Attachment Removed':
      return faTrash;
    case 'Corrective Action Added':
    case 'Corrective Action Updated':
      return faTasks;
    case 'Corrective Action Removed':
      return faTrash;
    case 'Involved Person Added':
      return faUserPlus;
    case 'Involved Person Removed':
      return faUserMinus;
    case 'Comment Added':
      return faComment;
    case 'Exported':
      return faDownload;
    default:
      return faClipboardList;
  }
};

const getActionColor = (action: string) => {
  switch (action) {
    case 'Created':
      return 'success';
    case 'Updated':
      return 'info';
    case 'Status Changed':
    case 'Severity Changed':
      return 'warning';
    case 'Attachment Added':
    case 'Corrective Action Added':
    case 'Involved Person Added':
    case 'Comment Added':
      return 'primary';
    case 'Attachment Removed':
    case 'Corrective Action Removed':
    case 'Involved Person Removed':
      return 'danger';
    case 'Viewed':
      return 'light';
    case 'Exported':
      return 'secondary';
    default:
      return 'secondary';
  }
};

const formatChangeText = (auditLog: IncidentAuditLogDto): string => {
  if (auditLog.changeDescription) {
    return auditLog.changeDescription;
  }

  if (auditLog.oldValue && auditLog.newValue) {
    return `Changed ${auditLog.fieldName} from "${auditLog.oldValue}" to "${auditLog.newValue}"`;
  }

  if (auditLog.newValue && !auditLog.oldValue) {
    return `Set ${auditLog.fieldName} to "${auditLog.newValue}"`;
  }

  if (auditLog.oldValue && !auditLog.newValue) {
    return `Cleared ${auditLog.fieldName} (was "${auditLog.oldValue}")`;
  }

  return auditLog.action;
};

const IncidentAuditTrail: React.FC<IncidentAuditTrailProps> = ({ incidentId }) => {
  const { data: auditLogs = [], isLoading, error } = useGetIncidentAuditTrailQuery(incidentId);

  if (isLoading) {
    return (
      <CCard className="mb-4">
        <CCardHeader>
          <h6 className="mb-0">
            <FontAwesomeIcon icon={faHistory} className="me-2" />
            Activity History
          </h6>
        </CCardHeader>
        <CCardBody>
          <div className="d-flex justify-content-center p-4">
            <CSpinner size="sm" />
            <span className="ms-2">Loading activity history...</span>
          </div>
        </CCardBody>
      </CCard>
    );
  }

  if (error) {
    return (
      <CCard className="mb-4">
        <CCardHeader>
          <h6 className="mb-0">
            <FontAwesomeIcon icon={faHistory} className="me-2" />
            Activity History
          </h6>
        </CCardHeader>
        <CCardBody>
          <CAlert color="danger">
            Failed to load activity history. Please try again.
          </CAlert>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <CCard className="mb-4">
      <CCardHeader className="d-flex justify-content-between align-items-center">
        <h6 className="mb-0">
          <FontAwesomeIcon icon={faHistory} className="me-2" />
          Activity History ({auditLogs.length})
        </h6>
        {auditLogs.length > 0 && (
          <small className="text-muted">
            Last updated {formatRelativeTime(auditLogs[0].changedAt)}
          </small>
        )}
      </CCardHeader>
      
      <CCardBody>
        {auditLogs.length === 0 ? (
          <div className="text-center text-muted py-3">
            <FontAwesomeIcon icon={faHistory} size="2x" className="mb-2 opacity-50" />
            <p className="mb-0">No activity recorded yet</p>
          </div>
        ) : (
          <CListGroup flush className="audit-trail-mobile">
            {auditLogs.map((auditLog, index) => (
              <CListGroupItem 
                key={auditLog.id} 
                className={`border-start border-4 ${index === 0 ? 'border-primary' : 'border-light'}`}
                style={{ borderLeftWidth: '3px !important' }}
              >
                <div className="d-flex align-items-start">
                  <div className="me-3 mt-1">
                    <div 
                      className={`rounded-circle d-flex align-items-center justify-content-center`}
                      style={{ 
                        width: '32px', 
                        height: '32px', 
                        backgroundColor: `var(--cui-${getActionColor(auditLog.action)})`,
                        color: getActionColor(auditLog.action) === 'light' ? 'var(--cui-dark)' : 'white'
                      }}
                    >
                      <FontAwesomeIcon 
                        icon={getActionIcon(auditLog.action)} 
                        size="sm" 
                      />
                    </div>
                  </div>
                  
                  <div className="flex-grow-1">
                    <div className="d-flex justify-content-between align-items-start mb-1">
                      <div>
                        <strong className="text-dark">{auditLog.changedBy}</strong>
                        <CBadge 
                          color={getActionColor(auditLog.action)} 
                          className="ms-2"
                        >
                          {auditLog.action}
                        </CBadge>
                      </div>
                      <small className="text-muted">
                        {formatDateTime(auditLog.changedAt)}
                      </small>
                    </div>
                    
                    <div className="text-muted">
                      {formatChangeText(auditLog)}
                    </div>
                    
                    {auditLog.fieldName !== 'System' && (
                      <small className="text-muted">
                        Field: <code>{auditLog.fieldName}</code>
                      </small>
                    )}
                  </div>
                </div>
              </CListGroupItem>
            ))}
          </CListGroup>
        )}
      </CCardBody>
    </CCard>
  );
};

export default IncidentAuditTrail;