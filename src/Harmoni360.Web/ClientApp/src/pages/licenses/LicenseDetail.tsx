import React, { useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CButtonGroup,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
  CBadge,
  CAlert,
  CSpinner,
  CTable,
  CTableHead,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell,
  CListGroup,
  CListGroupItem,
  CProgress,
  CTooltip,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CForm,
  CFormTextarea,
  CFormLabel,
  CFormInput
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faFileContract,
  faArrowLeft,
  faEdit,
  faCheckCircle,
  faPause,
  faBan,
  faRedo,
  faDownload,
  faUpload,
  faTrash,
  faEye,
  faInfoCircle,
  faShieldAlt,
  faCalendarAlt,
  faBuilding,
  faCertificate,
  faGavel,
  faClipboardCheck,
  faHistory,
  faPaperclip,
  faExclamationTriangle,
  faClock,
  faUser,
  faPhone,
  faEnvelope,
  faMapMarkerAlt,
  faCheck,
  faTimes,
  faPlus,
  faCog,
  faFileAlt,
  faExternalLinkAlt
} from '@fortawesome/free-solid-svg-icons';

import { 
  useGetLicenseByIdQuery,
  useSubmitLicenseMutation,
  useApproveLicenseMutation,
  useRejectLicenseMutation,
  useActivateLicenseMutation,
  useSuspendLicenseMutation,
  useRevokeLicenseMutation,
  useRenewLicenseMutation,
  useUploadAttachmentMutation,
  useDeleteAttachmentMutation
} from '../../features/licenses/licenseApi';
import { 
  LicenseDto,
  LicenseAttachmentDto,
  getStatusColor,
  getPriorityColor,
  getRiskLevelColor
} from '../../types/license';
import { format, isAfter, isBefore, addDays } from 'date-fns';

// License Management Icon Mappings
const LICENSE_ICONS = {
  license: faFileContract,
  back: faArrowLeft,
  edit: faEdit,
  submit: faCheckCircle,
  approve: faCheckCircle,
  reject: faTimes,
  activate: faCheckCircle,
  suspend: faPause,
  revoke: faBan,
  renew: faRedo,
  download: faDownload,
  upload: faUpload,
  delete: faTrash,
  view: faEye,
  info: faInfoCircle,
  shield: faShieldAlt,
  calendar: faCalendarAlt,
  building: faBuilding,
  certificate: faCertificate,
  gavel: faGavel,
  clipboard: faClipboardCheck,
  history: faHistory,
  attachment: faPaperclip,
  warning: faExclamationTriangle,
  clock: faClock,
  user: faUser,
  phone: faPhone,
  email: faEnvelope,
  location: faMapMarkerAlt,
  check: faCheck,
  plus: faPlus,
  settings: faCog,
  file: faFileAlt,
  external: faExternalLinkAlt
};

const LicenseDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const licenseId = parseInt(id || '0', 10);

  // State management
  const [activeTab, setActiveTab] = useState('overview');
  const [showActionModal, setShowActionModal] = useState<string | null>(null);
  const [actionNotes, setActionNotes] = useState('');
  const [actionReason, setActionReason] = useState('');
  const [uploading, setUploading] = useState(false);

  // API calls
  const { 
    data: license, 
    isLoading, 
    error,
    refetch 
  } = useGetLicenseByIdQuery(licenseId, {
    skip: !licenseId
  });

  const [submitLicense, { isLoading: isSubmitting }] = useSubmitLicenseMutation();
  const [approveLicense, { isLoading: isApproving }] = useApproveLicenseMutation();
  const [rejectLicense, { isLoading: isRejecting }] = useRejectLicenseMutation();
  const [activateLicense, { isLoading: isActivating }] = useActivateLicenseMutation();
  const [suspendLicense, { isLoading: isSuspending }] = useSuspendLicenseMutation();
  const [revokeLicense, { isLoading: isRevoking }] = useRevokeLicenseMutation();
  const [renewLicense, { isLoading: isRenewing }] = useRenewLicenseMutation();
  const [uploadAttachment] = useUploadAttachmentMutation();
  const [deleteAttachment] = useDeleteAttachmentMutation();

  // Handle license actions
  const handleLicenseAction = useCallback(async (action: string) => {
    if (!license) return;

    try {
      switch (action) {
        case 'submit':
          await submitLicense({ 
            id: license.id, 
            submissionNotes: actionNotes 
          }).unwrap();
          break;
        case 'approve':
          await approveLicense({ 
            id: license.id, 
            approvalNotes: actionNotes 
          }).unwrap();
          break;
        case 'reject':
          await rejectLicense({ 
            id: license.id, 
            rejectionReason: actionReason,
            rejectionNotes: actionNotes 
          }).unwrap();
          break;
        case 'activate':
          await activateLicense({ id: license.id }).unwrap();
          break;
        case 'suspend':
          await suspendLicense({ 
            id: license.id, 
            suspensionReason: actionReason,
            suspensionNotes: actionNotes 
          }).unwrap();
          break;
        case 'revoke':
          await revokeLicense({ 
            id: license.id, 
            revocationReason: actionReason,
            revocationNotes: actionNotes 
          }).unwrap();
          break;
        case 'renew':
          await renewLicense({ 
            id: license.id, 
            renewalNotes: actionNotes 
          }).unwrap();
          break;
      }

      setShowActionModal(null);
      setActionNotes('');
      setActionReason('');
      refetch();
    } catch (error) {
      console.error(`Error ${action} license:`, error);
    }
  }, [license, actionNotes, actionReason, submitLicense, approveLicense, rejectLicense, activateLicense, suspendLicense, revokeLicense, renewLicense, refetch]);

  // Handle file upload
  const handleFileUpload = useCallback(async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file || !license) return;

    try {
      setUploading(true);
      await uploadAttachment({
        licenseId: license.id,
        file,
        attachmentType: 'SupportingDocument',
        description: `Uploaded ${file.name}`
      }).unwrap();
      
      refetch();
    } catch (error) {
      console.error('Error uploading file:', error);
    } finally {
      setUploading(false);
      event.target.value = '';
    }
  }, [license, uploadAttachment, refetch]);

  // Handle attachment deletion
  const handleDeleteAttachment = useCallback(async (attachmentId: number) => {
    if (!license || !window.confirm('Are you sure you want to delete this attachment?')) return;

    try {
      await deleteAttachment({ 
        licenseId: license.id, 
        attachmentId 
      }).unwrap();
      refetch();
    } catch (error) {
      console.error('Error deleting attachment:', error);
    }
  }, [license, deleteAttachment, refetch]);

  // Calculate compliance progress
  const calculateComplianceProgress = useCallback(() => {
    if (!license?.licenseConditions.length) return 100;
    
    const completed = license.licenseConditions.filter(c => c.isCompleted).length;
    return Math.round((completed / license.licenseConditions.length) * 100);
  }, [license]);

  // Get status-based action buttons
  const getActionButtons = useCallback(() => {
    if (!license) return null;

    const isProcessing = isSubmitting || isApproving || isRejecting || isActivating || isSuspending || isRevoking || isRenewing;

    return (
      <CButtonGroup>
        {license.canEdit && (
          <CTooltip content="Edit License">
            <CButton
              color="warning"
              variant="outline"
              onClick={() => navigate(`/licenses/${license.id}/edit`)}
              disabled={isProcessing}
            >
              <FontAwesomeIcon icon={LICENSE_ICONS.edit} className="me-1" />
              Edit
            </CButton>
          </CTooltip>
        )}

        {license.canSubmit && (
          <CButton
            color="success"
            variant="outline"
            onClick={() => setShowActionModal('submit')}
            disabled={isProcessing}
          >
            <FontAwesomeIcon icon={LICENSE_ICONS.submit} className="me-1" />
            Submit
          </CButton>
        )}

        {license.canApprove && (
          <CButton
            color="success"
            onClick={() => setShowActionModal('approve')}
            disabled={isProcessing}
          >
            <FontAwesomeIcon icon={LICENSE_ICONS.approve} className="me-1" />
            Approve
          </CButton>
        )}

        {license.canActivate && (
          <CButton
            color="success"
            onClick={() => handleLicenseAction('activate')}
            disabled={isProcessing}
          >
            <FontAwesomeIcon icon={LICENSE_ICONS.activate} className="me-1" />
            Activate
          </CButton>
        )}

        {license.canSuspend && (
          <CButton
            color="warning"
            variant="outline"
            onClick={() => setShowActionModal('suspend')}
            disabled={isProcessing}
          >
            <FontAwesomeIcon icon={LICENSE_ICONS.suspend} className="me-1" />
            Suspend
          </CButton>
        )}

        {license.canRenew && (
          <CButton
            color="info"
            variant="outline"
            onClick={() => setShowActionModal('renew')}
            disabled={isProcessing}
          >
            <FontAwesomeIcon icon={LICENSE_ICONS.renew} className="me-1" />
            Renew
          </CButton>
        )}

        <CDropdown>
          <CDropdownToggle color="secondary" variant="outline" disabled={isProcessing}>
            <FontAwesomeIcon icon={LICENSE_ICONS.settings} className="me-1" />
            More Actions
          </CDropdownToggle>
          <CDropdownMenu>
            <CDropdownItem onClick={() => window.open(`/api/licenses/${license.id}/export`, '_blank')}>
              <FontAwesomeIcon icon={LICENSE_ICONS.download} className="me-2" />
              Download PDF
            </CDropdownItem>
            <CDropdownItem divider />
            <CDropdownItem 
              className="text-danger"
              onClick={() => setShowActionModal('reject')}
              disabled={!license.canApprove}
            >
              <FontAwesomeIcon icon={LICENSE_ICONS.reject} className="me-2" />
              Reject License
            </CDropdownItem>
            <CDropdownItem 
              className="text-danger"
              onClick={() => setShowActionModal('revoke')}
            >
              <FontAwesomeIcon icon={LICENSE_ICONS.revoke} className="me-2" />
              Revoke License
            </CDropdownItem>
          </CDropdownMenu>
        </CDropdown>

        {isProcessing && <CSpinner size="sm" className="ms-2" />}
      </CButtonGroup>
    );
  }, [license, navigate, handleLicenseAction, isSubmitting, isApproving, isRejecting, isActivating, isSuspending, isRevoking, isRenewing]);

  if (isLoading) {
    return (
      <div className="text-center p-4">
        <CSpinner />
        <div className="mt-2">Loading license details...</div>
      </div>
    );
  }

  if (error || !license) {
    return (
      <CRow>
        <CCol xs={12}>
          <CAlert color="danger">
            <FontAwesomeIcon icon={LICENSE_ICONS.warning} className="me-2" />
            License not found or error loading details.
          </CAlert>
        </CCol>
      </CRow>
    );
  }

  return (
    <>
      <CRow>
        <CCol xs={12}>
          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <div className="d-flex align-items-center">
                <CButton
                  variant="outline"
                  color="secondary"
                  className="me-3"
                  onClick={() => navigate('/licenses')}
                >
                  <FontAwesomeIcon icon={LICENSE_ICONS.back} className="me-1" />
                  Back to Licenses
                </CButton>
                <div>
                  <h4 className="mb-0">
                    <FontAwesomeIcon icon={LICENSE_ICONS.license} className="me-2" />
                    {license.title}
                  </h4>
                  <div className="text-muted">
                    License #{license.licenseNumber} • {license.typeDisplay}
                  </div>
                </div>
              </div>
              <div className="d-flex align-items-center gap-2">
                <CBadge color={getStatusColor(license.status)} size="lg">
                  {license.statusDisplay}
                </CBadge>
                <CBadge color={getPriorityColor(license.priority)}>
                  {license.priorityDisplay}
                </CBadge>
                {license.isCriticalLicense && (
                  <CBadge color="danger">
                    <FontAwesomeIcon icon={LICENSE_ICONS.warning} className="me-1" />
                    Critical
                  </CBadge>
                )}
                {getActionButtons()}
              </div>
            </CCardHeader>

            <CCardBody>
              {/* Status Alerts */}
              {license.isExpired && (
                <CAlert color="danger" className="mb-3">
                  <FontAwesomeIcon icon={LICENSE_ICONS.warning} className="me-2" />
                  This license expired {Math.abs(license.daysUntilExpiry)} days ago on {format(new Date(license.expiryDate), 'MMM dd, yyyy')}
                </CAlert>
              )}
              
              {license.isExpiring && !license.isExpired && (
                <CAlert color="warning" className="mb-3">
                  <FontAwesomeIcon icon={LICENSE_ICONS.clock} className="me-2" />
                  This license will expire in {license.daysUntilExpiry} days on {format(new Date(license.expiryDate), 'MMM dd, yyyy')}
                </CAlert>
              )}

              {license.isRenewalDue && (
                <CAlert color="info" className="mb-3">
                  <FontAwesomeIcon icon={LICENSE_ICONS.renew} className="me-2" />
                  Renewal is due in {license.daysUntilRenewal} days
                </CAlert>
              )}

              {/* Navigation Tabs */}
              <CNav variant="tabs" className="mb-3">
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'overview'}
                    onClick={() => setActiveTab('overview')}
                    style={{ cursor: 'pointer' }}
                  >
                    <FontAwesomeIcon icon={LICENSE_ICONS.info} className="me-1" />
                    Overview
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'details'}
                    onClick={() => setActiveTab('details')}
                    style={{ cursor: 'pointer' }}
                  >
                    <FontAwesomeIcon icon={LICENSE_ICONS.certificate} className="me-1" />
                    License Details
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'conditions'}
                    onClick={() => setActiveTab('conditions')}
                    style={{ cursor: 'pointer' }}
                  >
                    <FontAwesomeIcon icon={LICENSE_ICONS.clipboard} className="me-1" />
                    Conditions ({license.licenseConditions.length})
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'attachments'}
                    onClick={() => setActiveTab('attachments')}
                    style={{ cursor: 'pointer' }}
                  >
                    <FontAwesomeIcon icon={LICENSE_ICONS.attachment} className="me-1" />
                    Attachments ({license.attachments.length})
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'compliance'}
                    onClick={() => setActiveTab('compliance')}
                    style={{ cursor: 'pointer' }}
                  >
                    <FontAwesomeIcon icon={LICENSE_ICONS.shield} className="me-1" />
                    Compliance
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'history'}
                    onClick={() => setActiveTab('history')}
                    style={{ cursor: 'pointer' }}
                  >
                    <FontAwesomeIcon icon={LICENSE_ICONS.history} className="me-1" />
                    History
                  </CNavLink>
                </CNavItem>
              </CNav>

              {/* Tab Content */}
              <CTabContent>
                {/* Overview Tab */}
                <CTabPane visible={activeTab === 'overview'}>
                  <CRow>
                    <CCol md={8}>
                      <CCard className="mb-3">
                        <CCardHeader>
                          <FontAwesomeIcon icon={LICENSE_ICONS.info} className="me-2" />
                          Basic Information
                        </CCardHeader>
                        <CCardBody>
                          <CRow>
                            <CCol md={6}>
                              <div className="mb-3">
                                <strong>License Title:</strong><br />
                                {license.title}
                              </div>
                              <div className="mb-3">
                                <strong>License Number:</strong><br />
                                {license.licenseNumber}
                              </div>
                              <div className="mb-3">
                                <strong>Type:</strong><br />
                                <CBadge color="info">{license.typeDisplay}</CBadge>
                              </div>
                            </CCol>
                            <CCol md={6}>
                              <div className="mb-3">
                                <strong>Status:</strong><br />
                                <CBadge color={getStatusColor(license.status)}>
                                  {license.statusDisplay}
                                </CBadge>
                              </div>
                              <div className="mb-3">
                                <strong>Priority:</strong><br />
                                <CBadge color={getPriorityColor(license.priority)}>
                                  {license.priorityDisplay}
                                </CBadge>
                              </div>
                              <div className="mb-3">
                                <strong>Risk Level:</strong><br />
                                <CBadge color={getRiskLevelColor(license.riskLevel)}>
                                  <FontAwesomeIcon icon={LICENSE_ICONS.shield} className="me-1" />
                                  {license.riskLevelDisplay}
                                </CBadge>
                              </div>
                            </CCol>
                          </CRow>
                          {license.description && (
                            <div className="mb-3">
                              <strong>Description:</strong><br />
                              {license.description}
                            </div>
                          )}
                        </CCardBody>
                      </CCard>

                      <CCard>
                        <CCardHeader>
                          <FontAwesomeIcon icon={LICENSE_ICONS.certificate} className="me-2" />
                          License Authority & Holder
                        </CCardHeader>
                        <CCardBody>
                          <CRow>
                            <CCol md={6}>
                              <div className="mb-3">
                                <strong>
                                  <FontAwesomeIcon icon={LICENSE_ICONS.certificate} className="me-1" />
                                  Issuing Authority:
                                </strong><br />
                                {license.issuingAuthority}
                              </div>
                              <div className="mb-3">
                                <strong>
                                  <FontAwesomeIcon icon={LICENSE_ICONS.user} className="me-1" />
                                  License Holder:
                                </strong><br />
                                {license.holderName}
                              </div>
                            </CCol>
                            <CCol md={6}>
                              <div className="mb-3">
                                <strong>
                                  <FontAwesomeIcon icon={LICENSE_ICONS.building} className="me-1" />
                                  Department:
                                </strong><br />
                                {license.department}
                              </div>
                              {license.licenseFee && (
                                <div className="mb-3">
                                  <strong>License Fee:</strong><br />
                                  {license.currency} {license.licenseFee.toLocaleString()}
                                </div>
                              )}
                            </CCol>
                          </CRow>
                        </CCardBody>
                      </CCard>
                    </CCol>

                    <CCol md={4}>
                      <CCard className="mb-3">
                        <CCardHeader>
                          <FontAwesomeIcon icon={LICENSE_ICONS.calendar} className="me-2" />
                          Important Dates
                        </CCardHeader>
                        <CCardBody>
                          <div className="mb-3">
                            <strong>Issued Date:</strong><br />
                            <FontAwesomeIcon icon={LICENSE_ICONS.calendar} className="me-1" />
                            {format(new Date(license.issuedDate), 'MMM dd, yyyy')}
                          </div>
                          <div className="mb-3">
                            <strong>Expiry Date:</strong><br />
                            <FontAwesomeIcon icon={LICENSE_ICONS.calendar} className="me-1" />
                            {format(new Date(license.expiryDate), 'MMM dd, yyyy')}
                            <br />
                            <small className={`text-${license.isExpired ? 'danger' : license.isExpiring ? 'warning' : 'success'}`}>
                              {license.daysUntilExpiry > 0 
                                ? `${license.daysUntilExpiry} days remaining`
                                : `Expired ${Math.abs(license.daysUntilExpiry)} days ago`
                              }
                            </small>
                          </div>
                          {license.nextRenewalDate && (
                            <div className="mb-3">
                              <strong>Next Renewal:</strong><br />
                              <FontAwesomeIcon icon={LICENSE_ICONS.renew} className="me-1" />
                              {format(new Date(license.nextRenewalDate), 'MMM dd, yyyy')}
                            </div>
                          )}
                        </CCardBody>
                      </CCard>

                      {license.licenseConditions.length > 0 && (
                        <CCard>
                          <CCardHeader>
                            <FontAwesomeIcon icon={LICENSE_ICONS.clipboard} className="me-2" />
                            Compliance Progress
                          </CCardHeader>
                          <CCardBody>
                            <div className="mb-2">
                              <strong>{calculateComplianceProgress()}% Complete</strong>
                            </div>
                            <CProgress 
                              value={calculateComplianceProgress()} 
                              color={calculateComplianceProgress() === 100 ? 'success' : calculateComplianceProgress() >= 70 ? 'warning' : 'danger'}
                              className="mb-3"
                            />
                            <small className="text-muted">
                              {license.licenseConditions.filter(c => c.isCompleted).length} of {license.licenseConditions.length} conditions completed
                            </small>
                          </CCardBody>
                        </CCard>
                      )}
                    </CCol>
                  </CRow>
                </CTabPane>

                {/* License Details Tab */}
                <CTabPane visible={activeTab === 'details'}>
                  <CRow>
                    <CCol md={6}>
                      <CCard className="mb-3">
                        <CCardHeader>
                          <FontAwesomeIcon icon={LICENSE_ICONS.info} className="me-2" />
                          Scope and Restrictions
                        </CCardHeader>
                        <CCardBody>
                          {license.scope && (
                            <div className="mb-3">
                              <strong>Scope:</strong><br />
                              <div className="text-muted">{license.scope}</div>
                            </div>
                          )}
                          {license.restrictions && (
                            <div className="mb-3">
                              <strong>Restrictions:</strong><br />
                              <div className="text-muted">{license.restrictions}</div>
                            </div>
                          )}
                          {license.conditions && (
                            <div className="mb-3">
                              <strong>General Conditions:</strong><br />
                              <div className="text-muted">{license.conditions}</div>
                            </div>
                          )}
                        </CCardBody>
                      </CCard>
                    </CCol>

                    <CCol md={6}>
                      <CCard className="mb-3">
                        <CCardHeader>
                          <FontAwesomeIcon icon={LICENSE_ICONS.gavel} className="me-2" />
                          Regulatory Information
                        </CCardHeader>
                        <CCardBody>
                          {license.regulatoryFramework && (
                            <div className="mb-3">
                              <strong>Regulatory Framework:</strong><br />
                              <div className="text-muted">{license.regulatoryFramework}</div>
                            </div>
                          )}
                          {license.applicableRegulations && (
                            <div className="mb-3">
                              <strong>Applicable Regulations:</strong><br />
                              <div className="text-muted">{license.applicableRegulations}</div>
                            </div>
                          )}
                          {license.complianceStandards && (
                            <div className="mb-3">
                              <strong>Compliance Standards:</strong><br />
                              <div className="text-muted">{license.complianceStandards}</div>
                            </div>
                          )}
                        </CCardBody>
                      </CCard>
                    </CCol>
                  </CRow>

                  <CRow>
                    <CCol md={6}>
                      <CCard>
                        <CCardHeader>
                          <FontAwesomeIcon icon={LICENSE_ICONS.renew} className="me-2" />
                          Renewal Information
                        </CCardHeader>
                        <CCardBody>
                          <div className="mb-3">
                            <strong>Renewal Required:</strong><br />
                            <CBadge color={license.renewalRequired ? 'warning' : 'success'}>
                              {license.renewalRequired ? 'Yes' : 'No'}
                            </CBadge>
                          </div>
                          {license.renewalRequired && (
                            <>
                              <div className="mb-3">
                                <strong>Renewal Period:</strong><br />
                                {license.renewalPeriodDays} days before expiry
                              </div>
                              <div className="mb-3">
                                <strong>Auto Renewal:</strong><br />
                                <CBadge color={license.autoRenewal ? 'success' : 'secondary'}>
                                  {license.autoRenewal ? 'Enabled' : 'Disabled'}
                                </CBadge>
                              </div>
                              {license.renewalProcedure && (
                                <div className="mb-3">
                                  <strong>Renewal Procedure:</strong><br />
                                  <div className="text-muted">{license.renewalProcedure}</div>
                                </div>
                              )}
                            </>
                          )}
                        </CCardBody>
                      </CCard>
                    </CCol>

                    <CCol md={6}>
                      <CCard>
                        <CCardHeader>
                          <FontAwesomeIcon icon={LICENSE_ICONS.shield} className="me-2" />
                          Insurance & Financial
                        </CCardHeader>
                        <CCardBody>
                          <div className="mb-3">
                            <strong>Requires Insurance:</strong><br />
                            <CBadge color={license.requiresInsurance ? 'warning' : 'success'}>
                              {license.requiresInsurance ? 'Yes' : 'No'}
                            </CBadge>
                          </div>
                          {license.requiresInsurance && license.requiredInsuranceAmount && (
                            <div className="mb-3">
                              <strong>Required Insurance Amount:</strong><br />
                              {license.currency} {license.requiredInsuranceAmount.toLocaleString()}
                            </div>
                          )}
                          {license.licenseFee && (
                            <div className="mb-3">
                              <strong>License Fee:</strong><br />
                              {license.currency} {license.licenseFee.toLocaleString()}
                            </div>
                          )}
                        </CCardBody>
                      </CCard>
                    </CCol>
                  </CRow>
                </CTabPane>

                {/* Conditions Tab */}
                <CTabPane visible={activeTab === 'conditions'}>
                  {license.licenseConditions.length === 0 ? (
                    <CAlert color="info">
                      <FontAwesomeIcon icon={LICENSE_ICONS.info} className="me-2" />
                      No specific conditions have been added to this license.
                    </CAlert>
                  ) : (
                    <CTable responsive hover>
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell>Type</CTableHeaderCell>
                          <CTableHeaderCell>Description</CTableHeaderCell>
                          <CTableHeaderCell>Status</CTableHeaderCell>
                          <CTableHeaderCell>Due Date</CTableHeaderCell>
                          <CTableHeaderCell>Responsible Person</CTableHeaderCell>
                          <CTableHeaderCell>Notes</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {license.licenseConditions.map((condition) => (
                          <CTableRow key={condition.id}>
                            <CTableDataCell>
                              <CBadge color="info">
                                {condition.conditionType}
                              </CBadge>
                              {condition.isMandatory && (
                                <CBadge color="danger" className="ms-1">Mandatory</CBadge>
                              )}
                            </CTableDataCell>
                            <CTableDataCell>{condition.description}</CTableDataCell>
                            <CTableDataCell>
                              <CBadge color={condition.isCompleted ? 'success' : condition.isOverdue ? 'danger' : 'warning'}>
                                {condition.statusDisplay}
                              </CBadge>
                            </CTableDataCell>
                            <CTableDataCell>
                              {condition.dueDate ? (
                                <div>
                                  <FontAwesomeIcon icon={LICENSE_ICONS.calendar} className="me-1" />
                                  {format(new Date(condition.dueDate), 'MMM dd, yyyy')}
                                  {condition.isOverdue && (
                                    <div className="text-danger small">
                                      Overdue by {Math.abs(condition.daysUntilDue)} days
                                    </div>
                                  )}
                                </div>
                              ) : (
                                <span className="text-muted">Not specified</span>
                              )}
                            </CTableDataCell>
                            <CTableDataCell>
                              {condition.responsiblePerson || (
                                <span className="text-muted">Not assigned</span>
                              )}
                            </CTableDataCell>
                            <CTableDataCell>
                              <small className="text-muted">
                                {condition.notes || 'No notes'}
                              </small>
                            </CTableDataCell>
                          </CTableRow>
                        ))}
                      </CTableBody>
                    </CTable>
                  )}
                </CTabPane>

                {/* Attachments Tab */}
                <CTabPane visible={activeTab === 'attachments'}>
                  <div className="d-flex justify-content-between align-items-center mb-3">
                    <h6 className="mb-0">License Attachments</h6>
                    <div>
                      <input
                        type="file"
                        id="file-upload"
                        style={{ display: 'none' }}
                        onChange={handleFileUpload}
                        multiple
                      />
                      <CButton
                        color="primary"
                        variant="outline"
                        onClick={() => document.getElementById('file-upload')?.click()}
                        disabled={uploading}
                      >
                        <FontAwesomeIcon icon={uploading ? LICENSE_ICONS.clock : LICENSE_ICONS.upload} className="me-1" />
                        {uploading ? 'Uploading...' : 'Upload Files'}
                      </CButton>
                    </div>
                  </div>

                  {license.attachments.length === 0 ? (
                    <CAlert color="info">
                      <FontAwesomeIcon icon={LICENSE_ICONS.info} className="me-2" />
                      No attachments have been uploaded for this license.
                    </CAlert>
                  ) : (
                    <CTable responsive hover>
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell>File Name</CTableHeaderCell>
                          <CTableHeaderCell>Type</CTableHeaderCell>
                          <CTableHeaderCell>Size</CTableHeaderCell>
                          <CTableHeaderCell>Uploaded By</CTableHeaderCell>
                          <CTableHeaderCell>Upload Date</CTableHeaderCell>
                          <CTableHeaderCell>Actions</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {license.attachments.map((attachment) => (
                          <CTableRow key={attachment.id}>
                            <CTableDataCell>
                              <div>
                                <FontAwesomeIcon icon={LICENSE_ICONS.file} className="me-2" />
                                <strong>{attachment.originalFileName}</strong>
                                {attachment.description && (
                                  <div className="text-muted small">{attachment.description}</div>
                                )}
                              </div>
                            </CTableDataCell>
                            <CTableDataCell>
                              <CBadge color="info">
                                {attachment.attachmentTypeDisplay}
                              </CBadge>
                              {attachment.isRequired && (
                                <CBadge color="warning" className="ms-1">Required</CBadge>
                              )}
                            </CTableDataCell>
                            <CTableDataCell>
                              {(attachment.fileSize / 1024 / 1024).toFixed(2)} MB
                            </CTableDataCell>
                            <CTableDataCell>
                              <FontAwesomeIcon icon={LICENSE_ICONS.user} className="me-1" />
                              {attachment.uploadedBy}
                            </CTableDataCell>
                            <CTableDataCell>
                              <FontAwesomeIcon icon={LICENSE_ICONS.calendar} className="me-1" />
                              {format(new Date(attachment.uploadedAt), 'MMM dd, yyyy HH:mm')}
                            </CTableDataCell>
                            <CTableDataCell>
                              <CButtonGroup size="sm">
                                <CTooltip content="Download">
                                  <CButton
                                    color="info"
                                    variant="outline"
                                    onClick={() => window.open(`/api/licenses/${license.id}/attachments/${attachment.id}`, '_blank')}
                                  >
                                    <FontAwesomeIcon icon={LICENSE_ICONS.download} />
                                  </CButton>
                                </CTooltip>
                                <CTooltip content="Delete">
                                  <CButton
                                    color="danger"
                                    variant="outline"
                                    onClick={() => handleDeleteAttachment(attachment.id)}
                                  >
                                    <FontAwesomeIcon icon={LICENSE_ICONS.delete} />
                                  </CButton>
                                </CTooltip>
                              </CButtonGroup>
                            </CTableDataCell>
                          </CTableRow>
                        ))}
                      </CTableBody>
                    </CTable>
                  )}
                </CTabPane>

                {/* Compliance Tab */}
                <CTabPane visible={activeTab === 'compliance'}>
                  <CRow>
                    <CCol md={6}>
                      <CCard className="mb-3">
                        <CCardHeader>
                          <FontAwesomeIcon icon={LICENSE_ICONS.shield} className="me-2" />
                          Compliance Overview
                        </CCardHeader>
                        <CCardBody>
                          <div className="mb-3">
                            <strong>Overall Compliance Score:</strong><br />
                            <CProgress 
                              value={calculateComplianceProgress()} 
                              color={calculateComplianceProgress() === 100 ? 'success' : calculateComplianceProgress() >= 70 ? 'warning' : 'danger'}
                              className="mb-2"
                            />
                            <div className="d-flex justify-content-between">
                              <span>{calculateComplianceProgress()}% Complete</span>
                              <span className="text-muted">
                                {license.licenseConditions.filter(c => c.isCompleted).length}/{license.licenseConditions.length}
                              </span>
                            </div>
                          </div>

                          <div className="mb-3">
                            <strong>Risk Assessment:</strong><br />
                            <CBadge color={getRiskLevelColor(license.riskLevel)} size="lg">
                              <FontAwesomeIcon icon={LICENSE_ICONS.shield} className="me-1" />
                              {license.riskLevelDisplay} Risk
                            </CBadge>
                          </div>

                          <div className="mb-3">
                            <strong>Critical License:</strong><br />
                            <CBadge color={license.isCriticalLicense ? 'danger' : 'success'}>
                              {license.isCriticalLicense ? 'Yes' : 'No'}
                            </CBadge>
                          </div>
                        </CCardBody>
                      </CCard>
                    </CCol>

                    <CCol md={6}>
                      <CCard>
                        <CCardHeader>
                          <FontAwesomeIcon icon={LICENSE_ICONS.clipboard} className="me-2" />
                          Condition Summary
                        </CCardHeader>
                        <CCardBody>
                          <CListGroup flush>
                            <CListGroupItem className="d-flex justify-content-between align-items-center">
                              <span>
                                <FontAwesomeIcon icon={LICENSE_ICONS.check} className="text-success me-2" />
                                Completed Conditions
                              </span>
                              <CBadge color="success">
                                {license.licenseConditions.filter(c => c.isCompleted).length}
                              </CBadge>
                            </CListGroupItem>
                            <CListGroupItem className="d-flex justify-content-between align-items-center">
                              <span>
                                <FontAwesomeIcon icon={LICENSE_ICONS.clock} className="text-warning me-2" />
                                Pending Conditions
                              </span>
                              <CBadge color="warning">
                                {license.licenseConditions.filter(c => !c.isCompleted && !c.isOverdue).length}
                              </CBadge>
                            </CListGroupItem>
                            <CListGroupItem className="d-flex justify-content-between align-items-center">
                              <span>
                                <FontAwesomeIcon icon={LICENSE_ICONS.warning} className="text-danger me-2" />
                                Overdue Conditions
                              </span>
                              <CBadge color="danger">
                                {license.licenseConditions.filter(c => c.isOverdue).length}
                              </CBadge>
                            </CListGroupItem>
                            <CListGroupItem className="d-flex justify-content-between align-items-center">
                              <span>
                                <FontAwesomeIcon icon={LICENSE_ICONS.shield} className="text-info me-2" />
                                Mandatory Conditions
                              </span>
                              <CBadge color="info">
                                {license.licenseConditions.filter(c => c.isMandatory).length}
                              </CBadge>
                            </CListGroupItem>
                          </CListGroup>
                        </CCardBody>
                      </CCard>
                    </CCol>
                  </CRow>
                </CTabPane>

                {/* History Tab */}
                <CTabPane visible={activeTab === 'history'}>
                  <CAlert color="info">
                    <FontAwesomeIcon icon={LICENSE_ICONS.info} className="me-2" />
                    Audit trail and history tracking will be implemented in the next phase.
                  </CAlert>
                </CTabPane>
              </CTabContent>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Action Modal */}
      {showActionModal && (
        <CModal visible={true} onClose={() => setShowActionModal(null)} size="lg">
          <CModalHeader>
            <CModalTitle>
              {showActionModal === 'submit' && 'Submit License for Review'}
              {showActionModal === 'approve' && 'Approve License'}
              {showActionModal === 'reject' && 'Reject License'}
              {showActionModal === 'suspend' && 'Suspend License'}
              {showActionModal === 'revoke' && 'Revoke License'}
              {showActionModal === 'renew' && 'Renew License'}
            </CModalTitle>
          </CModalHeader>
          <CModalBody>
            <CForm>
              {(showActionModal === 'reject' || showActionModal === 'suspend' || showActionModal === 'revoke') && (
                <div className="mb-3">
                  <CFormLabel htmlFor="actionReason">
                    Reason <span className="text-danger">*</span>
                  </CFormLabel>
                  <CFormInput
                    id="actionReason"
                    value={actionReason}
                    onChange={(e) => setActionReason(e.target.value)}
                    placeholder={`Enter reason for ${showActionModal}`}
                    required
                  />
                </div>
              )}
              <div className="mb-3">
                <CFormLabel htmlFor="actionNotes">
                  Notes {(showActionModal === 'reject' || showActionModal === 'suspend' || showActionModal === 'revoke') ? '' : '(Optional)'}
                </CFormLabel>
                <CFormTextarea
                  id="actionNotes"
                  value={actionNotes}
                  onChange={(e) => setActionNotes(e.target.value)}
                  rows={3}
                  placeholder={`Additional notes for ${showActionModal}`}
                />
              </div>
            </CForm>
          </CModalBody>
          <CModalFooter>
            <CButton
              color="secondary"
              variant="outline"
              onClick={() => setShowActionModal(null)}
            >
              Cancel
            </CButton>
            <CButton
              color={showActionModal === 'reject' || showActionModal === 'suspend' || showActionModal === 'revoke' ? 'danger' : 'primary'}
              onClick={() => handleLicenseAction(showActionModal)}
              disabled={
                (showActionModal === 'reject' || showActionModal === 'suspend' || showActionModal === 'revoke') && 
                !actionReason.trim()
              }
            >
              <FontAwesomeIcon 
                icon={
                  showActionModal === 'submit' ? LICENSE_ICONS.submit :
                  showActionModal === 'approve' ? LICENSE_ICONS.approve :
                  showActionModal === 'reject' ? LICENSE_ICONS.reject :
                  showActionModal === 'suspend' ? LICENSE_ICONS.suspend :
                  showActionModal === 'revoke' ? LICENSE_ICONS.revoke :
                  LICENSE_ICONS.renew
                } 
                className="me-1" 
              />
              {showActionModal.charAt(0).toUpperCase() + showActionModal.slice(1)} License
            </CButton>
          </CModalFooter>
        </CModal>
      )}
    </>
  );
};

export default LicenseDetail;