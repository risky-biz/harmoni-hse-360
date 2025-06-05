import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CRow,
  CCol,
  CButton,
  CBadge,
  CSpinner,
  CAlert,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
  CTable,
  CTableHead,
  CTableBody,
  CTableHeaderCell,
  CTableDataCell,
  CTableRow,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faEdit, faArrowLeft, faFileAlt } from '@fortawesome/free-solid-svg-icons';
import { useGetHazardQuery } from '../../features/hazards/hazardApi';

const HazardDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const hazardId = parseInt(id || '0');
  
  const { data: hazard, isLoading, error } = useGetHazardQuery({
    id: hazardId,
    includeAttachments: true,
    includeRiskAssessments: true,
    includeMitigationActions: true,
    includeReassessments: true,
  });

  const [activeTab, setActiveTab] = React.useState('overview');

  const getSeverityColor = (severity: string) => {
    const colors: Record<string, string> = {
      'Negligible': 'success',
      'Minor': 'info',
      'Moderate': 'warning',
      'Major': 'danger',
      'Catastrophic': 'dark'
    };
    return colors[severity] || 'secondary';
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      'Reported': 'info',
      'UnderAssessment': 'warning',
      'ActionRequired': 'danger',
      'Mitigating': 'primary',
      'Monitoring': 'warning',
      'Resolved': 'success',
      'Closed': 'secondary'
    };
    return colors[status] || 'secondary';
  };

  if (isLoading) {
    return (
      <div className="text-center py-4">
        <CSpinner />
      </div>
    );
  }

  if (error || !hazard) {
    return (
      <CAlert color="danger">
        Failed to load hazard details. Please try again.
      </CAlert>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <div className="mb-3">
          <CButton 
            color="secondary" 
            variant="outline"
            onClick={() => navigate('/hazards')}
            className="me-2"
          >
            <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
            Back to Hazards
          </CButton>
          <CButton 
            color="primary"
            onClick={() => navigate(`/hazards/${hazard.id}/edit`)}
          >
            <FontAwesomeIcon icon={faEdit} className="me-1" />
            Edit Hazard
          </CButton>
        </div>

        <CCard className="mb-4">
          <CCardHeader>
            <div className="d-flex justify-content-between align-items-center">
              <h4 className="mb-0">{hazard.title}</h4>
              <div>
                <CBadge color={getSeverityColor(hazard.severity)} className="me-2">
                  {hazard.severity}
                </CBadge>
                <CBadge color={getStatusColor(hazard.status)}>
                  {hazard.status}
                </CBadge>
              </div>
            </div>
          </CCardHeader>
          <CCardBody>
            <CNav variant="tabs" className="mb-3">
              <CNavItem>
                <CNavLink
                  active={activeTab === 'overview'}
                  onClick={() => setActiveTab('overview')}
                  style={{ cursor: 'pointer' }}
                >
                  Overview
                </CNavLink>
              </CNavItem>
              <CNavItem>
                <CNavLink
                  active={activeTab === 'assessments'}
                  onClick={() => setActiveTab('assessments')}
                  style={{ cursor: 'pointer' }}
                >
                  Risk Assessments ({hazard.riskAssessmentsCount})
                </CNavLink>
              </CNavItem>
              <CNavItem>
                <CNavLink
                  active={activeTab === 'actions'}
                  onClick={() => setActiveTab('actions')}
                  style={{ cursor: 'pointer' }}
                >
                  Mitigation Actions ({hazard.mitigationActionsCount})
                </CNavLink>
              </CNavItem>
              <CNavItem>
                <CNavLink
                  active={activeTab === 'attachments'}
                  onClick={() => setActiveTab('attachments')}
                  style={{ cursor: 'pointer' }}
                >
                  Attachments ({hazard.attachmentsCount})
                </CNavLink>
              </CNavItem>
            </CNav>

            <CTabContent>
              <CTabPane visible={activeTab === 'overview'}>
                <CRow>
                  <CCol md={6}>
                    <h5>Basic Information</h5>
                    <table className="table table-borderless">
                      <tbody>
                        <tr>
                          <td><strong>Category:</strong></td>
                          <td>{hazard.category}</td>
                        </tr>
                        <tr>
                          <td><strong>Type:</strong></td>
                          <td>{hazard.type}</td>
                        </tr>
                        <tr>
                          <td><strong>Location:</strong></td>
                          <td>{hazard.location}</td>
                        </tr>
                        <tr>
                          <td><strong>Identified Date:</strong></td>
                          <td>{new Date(hazard.identifiedDate).toLocaleDateString()}</td>
                        </tr>
                        {hazard.expectedResolutionDate && (
                          <tr>
                            <td><strong>Expected Resolution:</strong></td>
                            <td>{new Date(hazard.expectedResolutionDate).toLocaleDateString()}</td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </CCol>
                  <CCol md={6}>
                    <h5>Reporter Information</h5>
                    <table className="table table-borderless">
                      <tbody>
                        <tr>
                          <td><strong>Reporter:</strong></td>
                          <td>{hazard.reporterName}</td>
                        </tr>
                        <tr>
                          <td><strong>Department:</strong></td>
                          <td>{hazard.reporterDepartment}</td>
                        </tr>
                        {hazard.reporterEmail && (
                          <tr>
                            <td><strong>Email:</strong></td>
                            <td>{hazard.reporterEmail}</td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </CCol>
                </CRow>
                
                <div className="mt-4">
                  <h5>Description</h5>
                  <p>{hazard.description}</p>
                </div>

                {hazard.currentRiskLevel && (
                  <div className="mt-4">
                    <h5>Current Risk Assessment</h5>
                    <p>
                      <strong>Risk Level:</strong> {hazard.currentRiskLevel} 
                      {hazard.currentRiskScore && (
                        <span className="ms-2">
                          (Score: {hazard.currentRiskScore})
                        </span>
                      )}
                    </p>
                    {hazard.lastAssessmentDate && (
                      <p><strong>Last Assessment:</strong> {new Date(hazard.lastAssessmentDate).toLocaleDateString()}</p>
                    )}
                  </div>
                )}
              </CTabPane>

              <CTabPane visible={activeTab === 'assessments'}>
                {hazard.riskAssessments && hazard.riskAssessments.length > 0 ? (
                  <CTable striped hover responsive>
                    <CTableHead>
                      <CTableRow>
                        <CTableHeaderCell>Type</CTableHeaderCell>
                        <CTableHeaderCell>Assessor</CTableHeaderCell>
                        <CTableHeaderCell>Risk Level</CTableHeaderCell>
                        <CTableHeaderCell>Score</CTableHeaderCell>
                        <CTableHeaderCell>Assessment Date</CTableHeaderCell>
                        <CTableHeaderCell>Status</CTableHeaderCell>
                      </CTableRow>
                    </CTableHead>
                    <CTableBody>
                      {hazard.riskAssessments.map(assessment => (
                        <CTableRow key={assessment.id}>
                          <CTableDataCell>{assessment.type}</CTableDataCell>
                          <CTableDataCell>{assessment.assessorName}</CTableDataCell>
                          <CTableDataCell>
                            <CBadge color={assessment.riskLevel === 'Critical' ? 'danger' : 'warning'}>
                              {assessment.riskLevel}
                            </CBadge>
                          </CTableDataCell>
                          <CTableDataCell>{assessment.riskScore}</CTableDataCell>
                          <CTableDataCell>
                            {new Date(assessment.assessmentDate).toLocaleDateString()}
                          </CTableDataCell>
                          <CTableDataCell>
                            <CBadge color={assessment.isActive ? 'success' : 'secondary'}>
                              {assessment.isActive ? 'Active' : 'Inactive'}
                            </CBadge>
                          </CTableDataCell>
                        </CTableRow>
                      ))}
                    </CTableBody>
                  </CTable>
                ) : (
                  <CAlert color="info">No risk assessments have been completed for this hazard.</CAlert>
                )}
              </CTabPane>

              <CTabPane visible={activeTab === 'actions'}>
                {hazard.mitigationActions && hazard.mitigationActions.length > 0 ? (
                  <CTable striped hover responsive>
                    <CTableHead>
                      <CTableRow>
                        <CTableHeaderCell>Action</CTableHeaderCell>
                        <CTableHeaderCell>Type</CTableHeaderCell>
                        <CTableHeaderCell>Priority</CTableHeaderCell>
                        <CTableHeaderCell>Assigned To</CTableHeaderCell>
                        <CTableHeaderCell>Target Date</CTableHeaderCell>
                        <CTableHeaderCell>Status</CTableHeaderCell>
                      </CTableRow>
                    </CTableHead>
                    <CTableBody>
                      {hazard.mitigationActions.map(action => (
                        <CTableRow key={action.id}>
                          <CTableDataCell>{action.actionDescription}</CTableDataCell>
                          <CTableDataCell>{action.type}</CTableDataCell>
                          <CTableDataCell>
                            <CBadge color={action.priority === 'Critical' ? 'danger' : 'warning'}>
                              {action.priority}
                            </CBadge>
                          </CTableDataCell>
                          <CTableDataCell>{action.assignedToName}</CTableDataCell>
                          <CTableDataCell>
                            {new Date(action.targetDate).toLocaleDateString()}
                          </CTableDataCell>
                          <CTableDataCell>
                            <CBadge color={action.status === 'Completed' ? 'success' : 'warning'}>
                              {action.status}
                            </CBadge>
                          </CTableDataCell>
                        </CTableRow>
                      ))}
                    </CTableBody>
                  </CTable>
                ) : (
                  <CAlert color="info">No mitigation actions have been created for this hazard.</CAlert>
                )}
              </CTabPane>

              <CTabPane visible={activeTab === 'attachments'}>
                {hazard.attachments && hazard.attachments.length > 0 ? (
                  <CTable striped hover responsive>
                    <CTableHead>
                      <CTableRow>
                        <CTableHeaderCell>File Name</CTableHeaderCell>
                        <CTableHeaderCell>Size</CTableHeaderCell>
                        <CTableHeaderCell>Uploaded By</CTableHeaderCell>
                        <CTableHeaderCell>Upload Date</CTableHeaderCell>
                        <CTableHeaderCell>Actions</CTableHeaderCell>
                      </CTableRow>
                    </CTableHead>
                    <CTableBody>
                      {hazard.attachments.map(attachment => (
                        <CTableRow key={attachment.id}>
                          <CTableDataCell>
                            <FontAwesomeIcon icon={faFileAlt} className="me-2" />
                            {attachment.fileName}
                          </CTableDataCell>
                          <CTableDataCell>
                            {(attachment.fileSize / 1024).toFixed(1)} KB
                          </CTableDataCell>
                          <CTableDataCell>{attachment.uploadedBy}</CTableDataCell>
                          <CTableDataCell>
                            {new Date(attachment.uploadedAt).toLocaleDateString()}
                          </CTableDataCell>
                          <CTableDataCell>
                            <CButton
                              color="primary"
                              variant="outline"
                              size="sm"
                              href={attachment.downloadUrl}
                              target="_blank"
                            >
                              Download
                            </CButton>
                          </CTableDataCell>
                        </CTableRow>
                      ))}
                    </CTableBody>
                  </CTable>
                ) : (
                  <CAlert color="info">No attachments have been uploaded for this hazard.</CAlert>
                )}
              </CTabPane>
            </CTabContent>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default HazardDetail;