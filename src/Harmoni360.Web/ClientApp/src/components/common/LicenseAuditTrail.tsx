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
  faClipboardList,
  faCheck,
  faTimes,
  faPause,
  faBan,
  faRedo,
  faUpload,
  faGavel,
  faShieldAlt,
  faFileContract,
  faCertificate,
  faKey,
  faCalendarAlt,
} from '@fortawesome/free-solid-svg-icons';
import {
  useGetLicenseAuditTrailQuery,
} from '../../features/licenses/licenseApi';
import { LicenseAuditLogDto } from '../../types/license';
import { format } from 'date-fns';

interface LicenseAuditTrailProps {
  licenseId: number;
}

const getActionIcon = (action: string) => {
  switch (action.toLowerCase()) {
    case 'created':
      return faPlus;
    case 'updated':
    case 'modified':
      return faEdit;
    case 'viewed':
      return faEye;
    case 'submitted':
      return faFileContract;
    case 'approved':
      return faCheck;
    case 'rejected':
      return faTimes;
    case 'activated':
      return faKey;
    case 'suspended':
      return faPause;
    case 'revoked':
      return faBan;
    case 'renewed':
      return faRedo;
    case 'expired':
      return faCalendarAlt;
    case 'status changed':
    case 'priority changed':
    case 'risk level changed':
      return faExclamationTriangle;
    case 'attachment added':
    case 'document uploaded':
      return faUpload;
    case 'attachment removed':
    case 'document deleted':
      return faTrash;
    case 'condition added':
    case 'condition updated':
      return faTasks;
    case 'condition completed':
      return faCheck;
    case 'condition removed':
      return faTrash;
    case 'renewal initiated':
    case 'renewal completed':
      return faRedo;
    case 'compliance updated':
      return faShieldAlt;
    case 'regulatory updated':
      return faGavel;
    case 'certificate issued':
      return faCertificate;
    case 'exported':
      return faDownload;
    case 'comment added':
      return faComment;
    default:
      return faClipboardList;
  }
};

const getActionColor = (action: string) => {
  switch (action.toLowerCase()) {
    case 'created':
    case 'approved':
    case 'activated':
    case 'renewed':
    case 'condition completed':
    case 'certificate issued':
      return 'success';
    case 'updated':
    case 'modified':
    case 'submitted':
    case 'compliance updated':
    case 'regulatory updated':
      return 'info';
    case 'status changed':
    case 'priority changed':
    case 'risk level changed':
    case 'suspended':
    case 'expired':
    case 'renewal initiated':
      return 'warning';
    case 'attachment added':
    case 'document uploaded':
    case 'condition added':
    case 'condition updated':
    case 'comment added':
      return 'primary';
    case 'rejected':
    case 'revoked':
    case 'attachment removed':
    case 'document deleted':
    case 'condition removed':
      return 'danger';
    case 'viewed':
    case 'exported':
      return 'light';
    default:
      return 'secondary';
  }
};

const formatChangeText = (auditLog: LicenseAuditLogDto): string => {
  if (auditLog.comments) {
    return auditLog.comments;
  }

  if (auditLog.oldValues && auditLog.newValues) {
    try {
      const oldData = JSON.parse(auditLog.oldValues);
      const newData = JSON.parse(auditLog.newValues);
      
      const changes: string[] = [];
      Object.keys(newData).forEach(key => {
        if (oldData[key] !== newData[key]) {
          changes.push(`${key}: "${oldData[key]}" → "${newData[key]}"`);
        }
      });
      
      return changes.length > 0 ? changes.join(', ') : auditLog.actionDescription;
    } catch {
      return auditLog.actionDescription;
    }
  }

  return auditLog.actionDescription;
};

const formatRelativeTime = (dateString: string): string => {
  const date = new Date(dateString);
  const now = new Date();
  const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);
  
  if (diffInSeconds < 60) return 'Just now';
  if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)} minutes ago`;
  if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)} hours ago`;
  if (diffInSeconds < 604800) return `${Math.floor(diffInSeconds / 86400)} days ago`;
  
  return format(date, 'MMM dd, yyyy');
};

const LicenseAuditTrail: React.FC<LicenseAuditTrailProps> = ({
  licenseId,
}) => {
  const {
    data: auditData,
    isLoading,
    error,
  } = useGetLicenseAuditTrailQuery({
    licenseId,
    page: 1,
    pageSize: 50,
  });

  const auditLogs = auditData?.items || [];

  if (isLoading) {
    return (
      <CCard className="mb-4">
        <CCardHeader>
          <h6 className="mb-0">
            <FontAwesomeIcon icon={faHistory} className="me-2" />
            License History
          </h6>
        </CCardHeader>
        <CCardBody>
          <div className="d-flex justify-content-center p-4">
            <CSpinner size="sm" />
            <span className="ms-2">Loading license history...</span>
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
            License History
          </h6>
        </CCardHeader>
        <CCardBody>
          <CAlert color="danger">
            Failed to load license history. Please try again.
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
          License History & Audit Trail ({auditLogs.length})
        </h6>
        {auditLogs.length > 0 && (
          <small className="text-muted">
            Last updated {formatRelativeTime(auditLogs[0].performedAt)}
          </small>
        )}
      </CCardHeader>

      <CCardBody>
        {auditLogs.length === 0 ? (
          <div className="text-center text-muted py-3">
            <FontAwesomeIcon
              icon={faHistory}
              size="2x"
              className="mb-2 opacity-50"
            />
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
                        color:
                          getActionColor(auditLog.action) === 'light'
                            ? 'var(--cui-dark)'
                            : 'white',
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
                        <strong className="text-dark">
                          {auditLog.performedBy}
                        </strong>
                        <CBadge
                          color={getActionColor(auditLog.action)}
                          className="ms-2"
                        >
                          {auditLog.actionDescription || auditLog.action}
                        </CBadge>
                      </div>
                      <small className="text-muted">
                        {format(new Date(auditLog.performedAt), 'MMM dd, yyyy HH:mm')}
                      </small>
                    </div>

                    <div className="text-muted">
                      {formatChangeText(auditLog)}
                    </div>

                    {auditLog.ipAddress && (
                      <small className="text-muted">
                        IP: <code>{auditLog.ipAddress}</code>
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

export default LicenseAuditTrail;