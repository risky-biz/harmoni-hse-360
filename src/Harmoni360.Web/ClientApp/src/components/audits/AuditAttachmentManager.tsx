import React, { useState, useRef } from 'react';
import { useSelector } from 'react-redux';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CFormInput,
  CFormSelect,
  CFormTextarea,
  CInputGroup,
  CBadge,
  CSpinner,
  CAlert,
  CListGroup,
  CListGroupItem,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CForm,
  CFormLabel,
  CRow,
  CCol,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faClipboardCheck,
  faUpload,
  faDownload,
  faTrash,
  faPlus,
  faTimes,
  faFile,
  faImage,
  faVideo,
  faFilePdf,
  faFileWord,
  faFileText,
  faFileExcel,
  faFilePowerpoint,
  faFileArchive,
  faFileCode,
  faFileAlt
} from '@fortawesome/free-solid-svg-icons';
import { formatDistanceToNow } from 'date-fns';
import { RootState } from '../../store';

// Define audit-specific attachment types
export type AuditAttachmentType = 
  | 'Evidence'
  | 'Checklist' 
  | 'Report'
  | 'Photo'
  | 'Document'
  | 'Certificate'
  | 'Standard'
  | 'Procedure'
  | 'Other';

export const AUDIT_ATTACHMENT_TYPES: Array<{ value: AuditAttachmentType; label: string }> = [
  { value: 'Evidence', label: 'Evidence Documentation' },
  { value: 'Checklist', label: 'Audit Checklist' },
  { value: 'Report', label: 'Audit Report' },
  { value: 'Photo', label: 'Photographic Evidence' },
  { value: 'Document', label: 'Supporting Document' },
  { value: 'Certificate', label: 'Certificate/Certification' },
  { value: 'Standard', label: 'Standard/Regulation' },
  { value: 'Procedure', label: 'Procedure/Method Statement' },
  { value: 'Other', label: 'Other' }
];

interface AuditAttachmentDto {
  id: number;
  auditId: number;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  filePath: string;
  uploadedBy: string;
  uploadedAt: string;
  attachmentType: AuditAttachmentType;
  description: string;
  category?: string;
  isEvidence: boolean;
  auditItemId?: number;
}

interface AuditAttachmentManagerProps {
  auditId?: string; // Optional for create mode
  attachments?: AuditAttachmentDto[]; // For create mode, we'll manage locally
  onAttachmentsChange?: (attachments: PendingAttachment[]) => void; // For create mode
  allowUpload?: boolean;
  allowDelete?: boolean;
  readonly?: boolean;
}

interface PendingAttachment {
  file: File;
  attachmentType: AuditAttachmentType;
  description: string;
  id: string; // Temporary ID for local management
}

const AuditAttachmentManager: React.FC<AuditAttachmentManagerProps> = ({
  auditId,
  attachments = [],
  onAttachmentsChange,
  allowUpload = true,
  allowDelete = true,
  readonly = false,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [pendingAttachments, setPendingAttachments] = useState<PendingAttachment[]>([]);
  const [showUploadModal, setShowUploadModal] = useState(false);
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [attachmentType, setAttachmentType] = useState<AuditAttachmentType>('Evidence');
  const [description, setDescription] = useState('');
  const [uploadError, setUploadError] = useState<string | null>(null);

  // Get authentication token from Redux store
  const token = useSelector((state: RootState) => state.auth.token);

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files || []);
    const validFiles: File[] = [];
    
    files.forEach((file) => {
      // Validate file type - comprehensive validation for audit documents
      const validTypes = [
        'image/',
        'video/',
        'application/pdf',
        'application/msword',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        'application/vnd.ms-excel',
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        'application/vnd.ms-powerpoint',
        'application/vnd.openxmlformats-officedocument.presentationml.presentation',
        'text/plain',
        'text/csv',
        'application/zip',
        'application/x-rar-compressed',
        'application/x-7z-compressed',
      ];
      
      const isValidType = validTypes.some((type) => file.type.startsWith(type)) ||
        file.name.toLowerCase().match(/\.(docx?|xlsx?|pptx?|txt|csv|zip|rar|7z)$/);

      // Validate file size (50MB limit for audit attachments)
      const maxSize = 50 * 1024 * 1024; // 50MB

      if (!isValidType) {
        setUploadError(
          `File "${file.name}" is not a supported type. Please upload documents, images, PDFs, or archive files.`
        );
        return;
      }

      if (file.size > maxSize) {
        setUploadError(
          `File "${file.name}" is too large. Maximum size is 50MB.`
        );
        return;
      }

      validFiles.push(file);
    });

    if (validFiles.length > 0) {
      setSelectedFiles(validFiles);
      setUploadError(null);
      setShowUploadModal(true);
    }

    // Clear the input
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleUpload = () => {
    if (selectedFiles.length === 0 || !description.trim()) {
      setUploadError('Please select files and provide a description.');
      return;
    }

    const newAttachments: PendingAttachment[] = selectedFiles.map((file) => ({
      file,
      attachmentType,
      description: description.trim(),
      id: `pending-${Date.now()}-${Math.random()}`,
    }));

    // Add to pending attachments
    const updatedPendingAttachments = [...pendingAttachments, ...newAttachments];
    setPendingAttachments(updatedPendingAttachments);

    // Notify parent component for create mode
    if (onAttachmentsChange) {
      onAttachmentsChange(updatedPendingAttachments);
    }

    // Reset form
    setSelectedFiles([]);
    setDescription('');
    setAttachmentType('Evidence');
    setShowUploadModal(false);
    setUploadError(null);
  };

  const handleRemovePendingAttachment = (attachmentId: string) => {
    const updatedPendingAttachments = pendingAttachments.filter(
      (attachment) => attachment.id !== attachmentId
    );
    setPendingAttachments(updatedPendingAttachments);

    // Notify parent component for create mode
    if (onAttachmentsChange) {
      onAttachmentsChange(updatedPendingAttachments);
    }
  };

  const getFileIcon = (fileName: string, contentType?: string) => {
    const extension = fileName.toLowerCase().split('.').pop();
    const type = contentType?.toLowerCase();

    if (type?.startsWith('image/')) return faImage;
    if (type?.startsWith('video/')) return faVideo;
    if (type === 'application/pdf' || extension === 'pdf') return faFilePdf;
    if (type?.includes('word') || ['doc', 'docx'].includes(extension || '')) return faFileWord;
    if (type?.includes('excel') || type?.includes('spreadsheet') || ['xls', 'xlsx'].includes(extension || '')) return faFileExcel;
    if (type?.includes('powerpoint') || type?.includes('presentation') || ['ppt', 'pptx'].includes(extension || '')) return faFilePowerpoint;
    if (['zip', 'rar', '7z'].includes(extension || '')) return faFileArchive;
    if (['txt', 'csv'].includes(extension || '')) return faFileText;
    if (['js', 'html', 'css', 'json'].includes(extension || '')) return faFileCode;
    
    return faFileAlt;
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const openFileSelector = () => {
    if (fileInputRef.current) {
      fileInputRef.current.click();
    }
  };

  return (
    <div>
      {/* Upload Controls */}
      {allowUpload && !readonly && (
        <div className="mb-3">
          <CButton 
            color="primary" 
            onClick={openFileSelector}
            disabled={false}
          >
            <FontAwesomeIcon icon={faPlus} className="me-2" />
            Add Attachment
          </CButton>
          
          <input
            ref={fileInputRef}
            type="file"
            multiple
            onChange={handleFileSelect}
            style={{ display: 'none' }}
            accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt,.csv,.zip,.rar,.7z,image/*,video/*"
          />
        </div>
      )}

      {/* Error Alert */}
      {uploadError && (
        <CAlert color="danger" dismissible onClose={() => setUploadError(null)}>
          {uploadError}
        </CAlert>
      )}

      {/* Pending Attachments (Create Mode) */}
      {pendingAttachments.length > 0 && (
        <CCard className="mb-3">
          <CCardHeader>
            <FontAwesomeIcon icon={faUpload} className="me-2" />
            Pending Uploads ({pendingAttachments.length})
          </CCardHeader>
          <CCardBody>
            <CListGroup flush>
              {pendingAttachments.map((attachment) => (
                <CListGroupItem key={attachment.id} className="d-flex justify-content-between align-items-center">
                  <div className="d-flex align-items-center">
                    <FontAwesomeIcon
                      icon={getFileIcon(attachment.file.name, attachment.file.type)}
                      className="me-3 text-muted"
                      size="lg"
                    />
                    <div>
                      <div className="fw-semibold">{attachment.file.name}</div>
                      <small className="text-muted">
                        {formatFileSize(attachment.file.size)} • {attachment.attachmentType}
                        {attachment.description && ` • ${attachment.description}`}
                      </small>
                    </div>
                  </div>
                  {allowDelete && (
                    <CButton
                      color="danger"
                      variant="outline"
                      size="sm"
                      onClick={() => handleRemovePendingAttachment(attachment.id)}
                    >
                      <FontAwesomeIcon icon={faTrash} />
                    </CButton>
                  )}
                </CListGroupItem>
              ))}
            </CListGroup>
          </CCardBody>
        </CCard>
      )}

      {/* Existing Attachments */}
      {attachments.length > 0 && (
        <CCard>
          <CCardHeader>
            <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
            Attachments ({attachments.length})
          </CCardHeader>
          <CCardBody>
            <CListGroup flush>
              {attachments.map((attachment) => (
                <CListGroupItem key={attachment.id} className="d-flex justify-content-between align-items-center">
                  <div className="d-flex align-items-center">
                    <FontAwesomeIcon
                      icon={getFileIcon(attachment.originalFileName, attachment.contentType)}
                      className="me-3 text-muted"
                      size="lg"
                    />
                    <div>
                      <div className="fw-semibold">{attachment.originalFileName}</div>
                      <small className="text-muted">
                        {formatFileSize(attachment.fileSize)} • 
                        <CBadge color="info" className="ms-1 me-1">{attachment.attachmentType}</CBadge>
                        • Uploaded {formatDistanceToNow(new Date(attachment.uploadedAt), { addSuffix: true })} by {attachment.uploadedBy}
                      </small>
                      {attachment.description && (
                        <div className="small text-muted mt-1">{attachment.description}</div>
                      )}
                    </div>
                  </div>
                  <div className="d-flex gap-2">
                    <CButton
                      color="primary"
                      variant="outline"
                      size="sm"
                      onClick={() => {
                        // Handle download - would need API endpoint
                        console.log('Download attachment:', attachment.id);
                      }}
                    >
                      <FontAwesomeIcon icon={faDownload} />
                    </CButton>
                    {allowDelete && !readonly && (
                      <CButton
                        color="danger"
                        variant="outline"
                        size="sm"
                        onClick={() => {
                          // Handle delete - would need API endpoint
                          console.log('Delete attachment:', attachment.id);
                        }}
                      >
                        <FontAwesomeIcon icon={faTrash} />
                      </CButton>
                    )}
                  </div>
                </CListGroupItem>
              ))}
            </CListGroup>
          </CCardBody>
        </CCard>
      )}

      {/* No Attachments Message */}
      {attachments.length === 0 && pendingAttachments.length === 0 && (
        <CAlert color="info">
          <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
          No attachments added yet. Upload supporting documents, evidence, checklists, or other relevant files for this audit.
        </CAlert>
      )}

      {/* Upload Modal */}
      <CModal visible={showUploadModal} onClose={() => setShowUploadModal(false)} size="lg">
        <CModalHeader onClose={() => setShowUploadModal(false)}>
          <CModalTitle>
            <FontAwesomeIcon icon={faUpload} className="me-2" />
            Upload Audit Attachments
          </CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CForm>
            {selectedFiles.length > 0 && (
              <div className="mb-3">
                <CFormLabel>Selected Files ({selectedFiles.length})</CFormLabel>
                <CListGroup>
                  {selectedFiles.map((file, index) => (
                    <CListGroupItem key={index} className="d-flex justify-content-between align-items-center">
                      <div className="d-flex align-items-center">
                        <FontAwesomeIcon
                          icon={getFileIcon(file.name, file.type)}
                          className="me-2 text-muted"
                        />
                        <div>
                          <div className="fw-semibold">{file.name}</div>
                          <small className="text-muted">{formatFileSize(file.size)}</small>
                        </div>
                      </div>
                    </CListGroupItem>
                  ))}
                </CListGroup>
              </div>
            )}

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="attachmentType">Attachment Type *</CFormLabel>
                <CFormSelect
                  id="attachmentType"
                  value={attachmentType}
                  onChange={(e) => setAttachmentType(e.target.value as AuditAttachmentType)}
                >
                  {AUDIT_ATTACHMENT_TYPES.map((type) => (
                    <option key={type.value} value={type.value}>
                      {type.label}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
            </CRow>

            <div className="mb-3">
              <CFormLabel htmlFor="description">Description *</CFormLabel>
              <CFormTextarea
                id="description"
                rows={3}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Describe the content and purpose of these files..."
              />
            </div>

            {uploadError && (
              <CAlert color="danger">{uploadError}</CAlert>
            )}
          </CForm>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowUploadModal(false)}>
            <FontAwesomeIcon icon={faTimes} className="me-2" />
            Cancel
          </CButton>
          <CButton 
            color="primary" 
            onClick={handleUpload}
            disabled={selectedFiles.length === 0 || !description.trim()}
          >
            <FontAwesomeIcon icon={faUpload} className="me-2" />
            Add Attachments
          </CButton>
        </CModalFooter>
      </CModal>
    </div>
  );
};

export default AuditAttachmentManager;