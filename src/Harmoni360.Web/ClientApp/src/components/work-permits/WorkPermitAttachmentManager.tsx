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
  faFileContract,
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
import {
  useUploadAttachmentMutation,
  useDeleteAttachmentMutation,
} from '../../features/work-permits/workPermitApi';
import { ATTACHMENT_TYPES, WorkPermitAttachmentDto, WorkPermitAttachmentType } from '../../types/workPermit';
import { RootState } from '../../store';

interface WorkPermitAttachmentManagerProps {
  workPermitId?: string; // Optional for create mode
  attachments?: WorkPermitAttachmentDto[]; // For create mode, we'll manage locally
  onAttachmentsChange?: (attachments: PendingAttachment[]) => void; // For create mode
  allowUpload?: boolean;
  allowDelete?: boolean;
  readonly?: boolean;
}

interface PendingAttachment {
  file: File;
  attachmentType: WorkPermitAttachmentType;
  description: string;
  id: string; // Temporary ID for local management
}

const WorkPermitAttachmentManager: React.FC<WorkPermitAttachmentManagerProps> = ({
  workPermitId,
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
  const [attachmentType, setAttachmentType] = useState<WorkPermitAttachmentType>('WorkPlan');
  const [description, setDescription] = useState('');
  const [uploadError, setUploadError] = useState<string | null>(null);

  // Get authentication token from Redux store
  const token = useSelector((state: RootState) => state.auth.token);
  
  const [uploadAttachment, { isLoading: isUploading }] = useUploadAttachmentMutation();
  const [deleteAttachment] = useDeleteAttachmentMutation();

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files || []);
    const validFiles: File[] = [];
    
    files.forEach((file) => {
      // Validate file type - more comprehensive validation
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

      // Validate file size (100MB limit for work permits - larger than hazards due to technical documents)
      const maxSize = 100 * 1024 * 1024; // 100MB

      if (!isValidType) {
        setUploadError(
          `File "${file.name}" is not a supported type. Please upload documents, images, PDFs, or archive files.`
        );
        return;
      }

      if (file.size > maxSize) {
        setUploadError(
          `File "${file.name}" is too large. Maximum size is 100MB.`
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

  const handleUpload = async () => {
    if (selectedFiles.length === 0) return;

    try {
      if (workPermitId) {
        // Existing work permit - upload directly
        for (const file of selectedFiles) {
          await uploadAttachment({
            workPermitId,
            file,
            attachmentType,
            description,
          }).unwrap();
        }
      } else {
        // Create mode - add to pending attachments
        const newPendingAttachments = selectedFiles.map(file => ({
          file,
          attachmentType,
          description,
          id: Math.random().toString(36).substr(2, 9),
        }));
        
        const updatedPending = [...pendingAttachments, ...newPendingAttachments];
        setPendingAttachments(updatedPending);
        onAttachmentsChange?.(updatedPending);
      }

      // Reset form
      setSelectedFiles([]);
      setAttachmentType('WorkPlan');
      setDescription('');
      setUploadError(null);
      setShowUploadModal(false);
    } catch (error) {
      setUploadError('Failed to upload files. Please try again.');
      console.error('Upload error:', error);
    }
  };

  const handleDeleteAttachment = async (attachmentId: number) => {
    if (!workPermitId || !confirm('Are you sure you want to delete this attachment?')) return;

    try {
      await deleteAttachment({
        workPermitId,
        attachmentId: attachmentId.toString(),
      }).unwrap();
    } catch (error) {
      console.error('Delete error:', error);
    }
  };

  const handleDeletePending = (pendingId: string) => {
    const updated = pendingAttachments.filter(att => att.id !== pendingId);
    setPendingAttachments(updated);
    onAttachmentsChange?.(updated);
  };

  const handleDownload = async (attachment: WorkPermitAttachmentDto) => {
    try {
      if (!token || !workPermitId) {
        setUploadError('Authentication required. Please log in again.');
        return;
      }

      const downloadUrl = `/api/work-permits/${workPermitId}/attachments/${attachment.id}/download`;

      const response = await fetch(downloadUrl, {
        method: 'GET',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        throw new Error(`Failed to download file: ${response.status} ${response.statusText}`);
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = attachment.fileName;
      link.target = '_blank';

      document.body.appendChild(link);
      link.click();

      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Download failed:', error);
      setUploadError(`Failed to download ${attachment.fileName}. Please try again.`);
    }
  };

  const getFileIcon = (fileName: string) => {
    const extension = fileName.split('.').pop()?.toLowerCase();
    switch (extension) {
      case 'pdf':
        return faFilePdf;
      case 'doc':
      case 'docx':
        return faFileWord;
      case 'xls':
      case 'xlsx':
        return faFileExcel;
      case 'ppt':
      case 'pptx':
        return faFilePowerpoint;
      case 'txt':
      case 'csv':
        return faFileText;
      case 'zip':
      case 'rar':
      case '7z':
        return faFileArchive;
      case 'html':
      case 'css':
      case 'js':
      case 'json':
      case 'xml':
        return faFileCode;
      default:
        if (fileName.match(/\.(jpg|jpeg|png|gif|bmp|webp)$/i)) {
          return faImage;
        }
        if (fileName.match(/\.(mp4|avi|mov|wmv|flv|webm)$/i)) {
          return faVideo;
        }
        return faFileAlt;
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
  };

  const allAttachments = [...attachments, ...pendingAttachments];

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>
          <div className="d-flex justify-content-between align-items-center">
            <h6 className="mb-0">
              <FontAwesomeIcon icon={faFileContract} className="me-2" />
              Attachments ({allAttachments.length})
            </h6>
            {allowUpload && !readonly && (
              <CButton
                color="primary"
                size="sm"
                onClick={() => fileInputRef.current?.click()}
                disabled={isUploading}
              >
                <FontAwesomeIcon icon={faPlus} className="me-2" />
                Add Files
              </CButton>
            )}
          </div>
        </CCardHeader>
        <CCardBody>
          {uploadError && (
            <CAlert
              color="danger"
              dismissible
              onClose={() => setUploadError(null)}
              className="mb-3"
            >
              {uploadError}
            </CAlert>
          )}

          {/* Hidden file input */}
          <CFormInput
            type="file"
            ref={fileInputRef}
            onChange={handleFileSelect}
            multiple
            accept="image/*,video/*,.pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt,.csv,.zip,.rar,.7z"
            style={{ display: 'none' }}
            disabled={isUploading || readonly}
          />

          {/* Existing and Pending Attachments */}
          {allAttachments.length === 0 ? (
            <div className="text-center text-muted py-4">
              <FontAwesomeIcon
                icon={faFileContract}
                size="2x"
                className="mb-2 opacity-50"
              />
              <p className="mb-0">No attachments uploaded yet</p>
              <small>Upload work plans, safety procedures, risk assessments, and other relevant documents</small>
            </div>
          ) : (
            <CListGroup flush>
              {/* Existing attachments */}
              {attachments.map((attachment) => (
                <CListGroupItem
                  key={attachment.id}
                  className="d-flex justify-content-between align-items-center"
                >
                  <div className="d-flex align-items-center">
                    <FontAwesomeIcon
                      icon={getFileIcon(attachment.fileName)}
                      className="me-3 text-muted"
                      size="lg"
                    />
                    <div>
                      <div className="fw-medium">{attachment.fileName}</div>
                      <div className="d-flex align-items-center gap-2 mb-1">
                        <CBadge color="info" className="text-white">
                          {ATTACHMENT_TYPES.find(t => t.value === attachment.attachmentType)?.label || attachment.attachmentType}
                        </CBadge>
                        <small className="text-muted">
                          {formatFileSize(attachment.fileSize)}
                        </small>
                      </div>
                      {attachment.description && (
                        <small className="text-muted">{attachment.description}</small>
                      )}
                      <div>
                        <small className="text-muted">
                          Uploaded by {attachment.uploadedBy} • {formatDistanceToNow(new Date(attachment.uploadedAt))} ago
                        </small>
                      </div>
                    </div>
                  </div>
                  <div className="d-flex gap-2">
                    <CButton
                      size="sm"
                      color="outline-primary"
                      onClick={() => handleDownload(attachment)}
                      title="Download"
                    >
                      <FontAwesomeIcon icon={faDownload} size="sm" />
                    </CButton>
                    {allowDelete && !readonly && (
                      <CButton
                        size="sm"
                        color="outline-danger"
                        onClick={() => handleDeleteAttachment(attachment.id)}
                        disabled={isUploading}
                        title="Delete"
                      >
                        <FontAwesomeIcon icon={faTrash} size="sm" />
                      </CButton>
                    )}
                  </div>
                </CListGroupItem>
              ))}
              
              {/* Pending attachments (create mode) */}
              {pendingAttachments.map((attachment) => (
                <CListGroupItem
                  key={attachment.id}
                  className="d-flex justify-content-between align-items-center bg-light"
                >
                  <div className="d-flex align-items-center">
                    <FontAwesomeIcon
                      icon={getFileIcon(attachment.file.name)}
                      className="me-3 text-muted"
                      size="lg"
                    />
                    <div>
                      <div className="fw-medium">{attachment.file.name}</div>
                      <div className="d-flex align-items-center gap-2 mb-1">
                        <CBadge color="secondary">
                          {ATTACHMENT_TYPES.find(t => t.value === attachment.attachmentType)?.label || attachment.attachmentType}
                        </CBadge>
                        <small className="text-muted">
                          {formatFileSize(attachment.file.size)}
                        </small>
                        <CBadge color="warning" className="text-dark">
                          Pending Upload
                        </CBadge>
                      </div>
                      {attachment.description && (
                        <small className="text-muted">{attachment.description}</small>
                      )}
                    </div>
                  </div>
                  <div className="d-flex gap-2">
                    {!readonly && (
                      <CButton
                        size="sm"
                        color="outline-danger"
                        onClick={() => handleDeletePending(attachment.id)}
                        title="Remove"
                      >
                        <FontAwesomeIcon icon={faTrash} size="sm" />
                      </CButton>
                    )}
                  </div>
                </CListGroupItem>
              ))}
            </CListGroup>
          )}
        </CCardBody>
      </CCard>

      {/* Upload Modal */}
      <CModal visible={showUploadModal} onClose={() => setShowUploadModal(false)} backdrop="static">
        <CModalHeader>
          <CModalTitle>Upload Attachments</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CForm>
            <div className="mb-3">
              <CFormLabel>Selected Files ({selectedFiles.length})</CFormLabel>
              <div className="border rounded p-2 bg-light">
                {selectedFiles.map((file, index) => (
                  <div key={index} className="d-flex align-items-center mb-1">
                    <FontAwesomeIcon icon={getFileIcon(file.name)} className="me-2" />
                    <span className="flex-grow-1">{file.name}</span>
                    <small className="text-muted">{formatFileSize(file.size)}</small>
                  </div>
                ))}
              </div>
            </div>

            <CRow className="mb-3">
              <CCol>
                <CFormLabel htmlFor="attachmentType">Attachment Type *</CFormLabel>
                <CFormSelect
                  id="attachmentType"
                  value={attachmentType}
                  onChange={(e) => setAttachmentType(e.target.value as WorkPermitAttachmentType)}
                >
                  {ATTACHMENT_TYPES.map(type => (
                    <option key={type.value} value={type.value}>
                      {type.label}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
            </CRow>

            <div className="mb-3">
              <CFormLabel htmlFor="description">Description</CFormLabel>
              <CFormTextarea
                id="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Optional description of the attachment"
                rows={3}
              />
            </div>

            {uploadError && (
              <CAlert color="danger" className="mb-3">
                {uploadError}
              </CAlert>
            )}
          </CForm>
        </CModalBody>
        <CModalFooter>
          <CButton
            color="secondary"
            onClick={() => {
              setShowUploadModal(false);
              setSelectedFiles([]);
              setUploadError(null);
            }}
          >
            Cancel
          </CButton>
          <CButton
            color="primary"
            onClick={handleUpload}
            disabled={isUploading || selectedFiles.length === 0}
          >
            {isUploading ? (
              <>
                <CSpinner size="sm" className="me-2" />
                Uploading...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faUpload} className="me-2" />
                Upload {selectedFiles.length} File{selectedFiles.length !== 1 ? 's' : ''}
              </>
            )}
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};

export default WorkPermitAttachmentManager;