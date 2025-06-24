import React from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CSpinner,
  CAlert,
  CRow,
  CCol,
  CBadge,
  CListGroup,
  CListGroupItem,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
  CButton,
  CButtonGroup,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faRecycle,
  faEdit,
  faTrash,
  faDownload,
  faCheck,
  faTimes,
  faArchive,
  faExclamationTriangle,
  faMapMarkerAlt,
  faCalendarAlt,
  faUser,
  faWeight,
  faDollarSign,
  faFileAlt,
  faComment,
  faHistory,
  faCheckCircle,
  faTimesCircle,
  faClock,
} from '@fortawesome/free-solid-svg-icons';
import { useParams, useNavigate } from 'react-router-dom';
import { useGetWasteReportByIdQuery } from '../../services/wasteReportsApi';
import { formatDateTime, formatDateOnly } from '../../utils/dateUtils';
import { formatFileSize } from '../../utils/formatters';
import { WasteReportDto } from '../../types/wasteReports';
import { PermissionGuard } from '../auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import WasteAuditTrail from './WasteAuditTrail';
import WasteComments from './WasteComments';

interface WasteReportDetailProps {
  reportId?: number;
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Draft':
      return 'secondary';
    case 'UnderReview':
      return 'warning';
    case 'Approved':
      return 'success';
    case 'Disposed':
      return 'primary';
    case 'Rejected':
      return 'danger';
    default:
      return 'secondary';
  }
};

const getStatusIcon = (status: string) => {
  switch (status) {
    case 'Draft':
      return faFileAlt;
    case 'UnderReview':
      return faClock;
    case 'Approved':
      return faCheckCircle;
    case 'Disposed':
      return faCheck;
    case 'Rejected':
      return faTimesCircle;
    default:
      return faFileAlt;
  }
};

const getClassificationColor = (classification: string) => {
  switch (classification) {
    case 'HazardousChemical':
    case 'HazardousBiological':
    case 'HazardousRadioactive':
      return 'danger';
    case 'Medical':
      return 'dark';
    case 'Electronic':
      return 'info';
    case 'Recyclable':
      return 'success';
    case 'Organic':
      return 'warning';
    case 'NonHazardous':
    default:
      return 'secondary';
  }
};

const WasteReportDetail: React.FC<WasteReportDetailProps> = ({ reportId: propReportId }) => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const reportId = propReportId || parseInt(id || '0', 10);
  const [activeTab, setActiveTab] = React.useState('details');

  const {
    data: report,
    isLoading,
    error,
    refetch,
  } = useGetWasteReportByIdQuery(reportId ?? 0, {
    skip: !reportId || reportId <= 0,
  });

  const handleEdit = () => {
    navigate(`/waste/reports/edit/${reportId}`);
  };

  const handleBack = () => {
    navigate('/waste/reports');
  };

  if (isLoading) {
    return (
      <CCard>
        <CCardBody>
          <div className="d-flex justify-content-center p-4">
            <CSpinner size="sm" />
            <span className="ms-2">Loading waste report...</span>
          </div>
        </CCardBody>
      </CCard>
    );
  }

  if (error || (!report && !isLoading && reportId)) {
    return (
      <CCard>
        <CCardBody>
          <CAlert color="danger">
            Failed to load waste report. Please try again.
          </CAlert>
          {reportId && (
            <CButton color="primary" onClick={() => refetch()}>
              Retry
            </CButton>
          )}
        </CCardBody>
      </CCard>
    );
  }

  if (!reportId) {
    return (
      <CCard>
        <CCardBody>
          <CAlert color="warning">
            Invalid waste report ID.
          </CAlert>
        </CCardBody>
      </CCard>
    );
  }

  // Additional safety check to ensure report is defined
  if (!report) {
    return (
      <CCard>
        <CCardBody>
          <CAlert color="info">
            No waste report data available.
          </CAlert>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <div className="waste-report-detail">
      {/* Header */}
      <CCard className="mb-4">
        <CCardHeader className="d-flex justify-content-between align-items-center">
          <div>
            <h5 className="mb-1">
              <FontAwesomeIcon icon={faRecycle} className="me-2" />
              {report.title}
            </h5>
            <div className="d-flex align-items-center gap-2 mb-0">
              <CBadge color={getStatusColor(report.statusDisplay)}>
                <FontAwesomeIcon icon={getStatusIcon(report.statusDisplay)} className="me-1" />
                {report.statusDisplay}
              </CBadge>
              <CBadge color={getClassificationColor(report.classificationDisplay)} className="ms-1">
                {report.classificationDisplay}
              </CBadge>
              {report.isHighRisk && (
                <CBadge color="danger">
                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
                  High Risk
                </CBadge>
              )}
              {report.isOverdue && (
                <CBadge color="warning">
                  <FontAwesomeIcon icon={faClock} className="me-1" />
                  Overdue
                </CBadge>
              )}
            </div>
          </div>
          <CButtonGroup>
            <CButton
              color="secondary"
              variant="outline"
              onClick={handleBack}
            >
              Back to List
            </CButton>
            <PermissionGuard 
              module={ModuleType.WasteManagement} 
              permission={PermissionType.Update}
            >
              {report.canEdit && (
                <CButton
                  color="primary"
                  onClick={handleEdit}
                >
                  <FontAwesomeIcon icon={faEdit} className="me-1" />
                  Edit
                </CButton>
              )}
            </PermissionGuard>
            <CButton
              color="info"
              variant="outline"
            >
              <FontAwesomeIcon icon={faDownload} className="me-1" />
              Export
            </CButton>
          </CButtonGroup>
        </CCardHeader>

        {/* Navigation Tabs */}
        <CNav variant="tabs" className="px-3">
          <CNavItem>
            <CNavLink
              active={activeTab === 'details'}
              onClick={() => setActiveTab('details')}
              style={{ cursor: 'pointer' }}
            >
              <FontAwesomeIcon icon={faFileAlt} className="me-2" />
              Details
            </CNavLink>
          </CNavItem>
          {report.hasComments && (
            <CNavItem>
              <CNavLink
                active={activeTab === 'comments'}
                onClick={() => setActiveTab('comments')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faComment} className="me-2" />
                Comments ({report.commentsCount})
              </CNavLink>
            </CNavItem>
          )}
          <CNavItem>
            <CNavLink
              active={activeTab === 'history'}
              onClick={() => setActiveTab('history')}
              style={{ cursor: 'pointer' }}
            >
              <FontAwesomeIcon icon={faHistory} className="me-2" />
              Activity History
            </CNavLink>
          </CNavItem>
        </CNav>
      </CCard>

      {/* Tab Content */}
      <CTabContent>
        {/* Details Tab */}
        <CTabPane visible={activeTab === 'details'}>
          <CRow>
            <CCol lg={8}>
              <CCard className="mb-4">
                <CCardHeader>
                  <h6 className="mb-0">Waste Report Information</h6>
                </CCardHeader>
                <CCardBody>
                  <CRow>
                    <CCol md={6}>
                      <CListGroup flush>
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>
                            <FontAwesomeIcon icon={faRecycle} className="me-2 text-muted" />
                            Classification:
                          </strong>
                          <CBadge color={getClassificationColor(report.classificationDisplay)}>
                            {report.classificationDisplay}
                          </CBadge>
                        </CListGroupItem>
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>
                            <FontAwesomeIcon icon={faCalendarAlt} className="me-2 text-muted" />
                            Report Date:
                          </strong>
                          <span>{formatDateOnly(report.reportDate)}</span>
                        </CListGroupItem>
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>
                            <FontAwesomeIcon icon={faUser} className="me-2 text-muted" />
                            Reported By:
                          </strong>
                          <span>{report.reportedBy}</span>
                        </CListGroupItem>
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>
                            <FontAwesomeIcon icon={faMapMarkerAlt} className="me-2 text-muted" />
                            Location:
                          </strong>
                          <span>{report.location || 'Not specified'}</span>
                        </CListGroupItem>
                      </CListGroup>
                    </CCol>
                    <CCol md={6}>
                      <CListGroup flush>
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>
                            <FontAwesomeIcon icon={faWeight} className="me-2 text-muted" />
                            Estimated Quantity:
                          </strong>
                          <span>
                            {report.estimatedQuantity 
                              ? `${report.estimatedQuantity} ${report.quantityUnit || 'kg'}`
                              : 'Not specified'
                            }
                          </span>
                        </CListGroupItem>
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>Disposal Method:</strong>
                          <span>{report.disposalMethod || 'Not specified'}</span>
                        </CListGroupItem>
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>Disposal Date:</strong>
                          <span>{report.disposalDate ? formatDateOnly(report.disposalDate) : 'Not scheduled'}</span>
                        </CListGroupItem>
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>
                            <FontAwesomeIcon icon={faDollarSign} className="me-2 text-muted" />
                            Disposal Cost:
                          </strong>
                          <span>
                            {report.disposalCost 
                              ? `$${report.disposalCost.toLocaleString()}`
                              : 'Not specified'
                            }
                          </span>
                        </CListGroupItem>
                      </CListGroup>
                    </CCol>
                  </CRow>

                  {report.description && (
                    <>
                      <hr />
                      <div>
                        <h6>Description</h6>
                        <p className="text-muted mb-0">{report.description}</p>
                      </div>
                    </>
                  )}

                  {report.notes && (
                    <>
                      <hr />
                      <div>
                        <h6>Additional Notes</h6>
                        <p className="text-muted mb-0">{report.notes}</p>
                      </div>
                    </>
                  )}
                </CCardBody>
              </CCard>

              {(report.contractorName || report.manifestNumber || report.treatment) && (
                <CCard className="mb-4">
                  <CCardHeader>
                    <h6 className="mb-0">Disposal Information</h6>
                  </CCardHeader>
                  <CCardBody>
                    <CListGroup flush>
                      {report.contractorName && (
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>Contractor:</strong>
                          <span>{report.contractorName}</span>
                        </CListGroupItem>
                      )}
                      {report.manifestNumber && (
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>Manifest Number:</strong>
                          <span>{report.manifestNumber}</span>
                        </CListGroupItem>
                      )}
                      {report.treatment && (
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>Treatment Method:</strong>
                          <span>{report.treatment}</span>
                        </CListGroupItem>
                      )}
                      {report.disposedBy && (
                        <CListGroupItem className="d-flex justify-content-between">
                          <strong>Disposed By:</strong>
                          <span>{report.disposedBy}</span>
                        </CListGroupItem>
                      )}
                    </CListGroup>
                  </CCardBody>
                </CCard>
              )}
            </CCol>

            <CCol lg={4}>
              <CCard className="mb-4">
                <CCardHeader>
                  <h6 className="mb-0">Status & Timeline</h6>
                </CCardHeader>
                <CCardBody>
                  <CListGroup flush>
                    <CListGroupItem className="d-flex justify-content-between">
                      <strong>Current Status:</strong>
                      <CBadge color={getStatusColor(report.statusDisplay)}>
                        <FontAwesomeIcon icon={getStatusIcon(report.statusDisplay)} className="me-1" />
                        {report.statusDisplay}
                      </CBadge>
                    </CListGroupItem>
                    <CListGroupItem className="d-flex justify-content-between">
                      <strong>Created:</strong>
                      <span>{formatDateTime(report.createdAt)}</span>
                    </CListGroupItem>
                    <CListGroupItem className="d-flex justify-content-between">
                      <strong>Created By:</strong>
                      <span>{report.createdBy}</span>
                    </CListGroupItem>
                    {report.updatedAt && (
                      <CListGroupItem className="d-flex justify-content-between">
                        <strong>Last Updated:</strong>
                        <span>{formatDateTime(report.updatedAt)}</span>
                      </CListGroupItem>
                    )}
                    {report.updatedBy && (
                      <CListGroupItem className="d-flex justify-content-between">
                        <strong>Updated By:</strong>
                        <span>{report.updatedBy}</span>
                      </CListGroupItem>
                    )}
                  </CListGroup>
                </CCardBody>
              </CCard>

              {(report.isOverdue || report.isHighRisk) && (
                <CCard className="mb-4 border-warning">
                  <CCardHeader className="bg-warning bg-opacity-10">
                    <h6 className="mb-0 text-warning">
                      <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                      Alerts
                    </h6>
                  </CCardHeader>
                  <CCardBody>
                    {report.isOverdue && (
                      <CAlert color="warning" className="mb-2">
                        <strong>Overdue:</strong> This waste report is {report.daysOverdue} days overdue for disposal.
                      </CAlert>
                    )}
                    {report.isHighRisk && (
                      <CAlert color="danger" className="mb-0">
                        <strong>High Risk:</strong> This waste type requires special handling and disposal procedures.
                      </CAlert>
                    )}
                  </CCardBody>
                </CCard>
              )}
            </CCol>
          </CRow>
        </CTabPane>

        {/* Comments Tab */}
        <CTabPane visible={activeTab === 'comments'}>
          <WasteComments reportId={reportId} />
        </CTabPane>

        {/* History Tab */}
        <CTabPane visible={activeTab === 'history'}>
          <WasteAuditTrail reportId={reportId} />
        </CTabPane>
      </CTabContent>
    </div>
  );
};

export default WasteReportDetail;