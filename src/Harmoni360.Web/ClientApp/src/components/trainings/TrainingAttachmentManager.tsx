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
  CFormCheck,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faGraduationCap,
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
  faFileAlt,
  faBook,
  faPlayCircle,
  faCertificate,
  faClipboardCheck
} from '@fortawesome/free-solid-svg-icons';
import { formatDistanceToNow } from 'date-fns';
import {
  useUploadAttachmentMutation,
  useDeleteAttachmentMutation,
} from '../../features/trainings/trainingApi';
import { TrainingAttachmentDto, TrainingAttachmentType } from '../../types/training';
import { RootState } from '../../store';

interface TrainingAttachmentManagerProps {
  trainingId?: string; // Optional for create mode
  attachments?: TrainingAttachmentDto[]; // For create mode, we'll manage locally
  onAttachmentsChange?: (attachments: PendingAttachment[]) => void; // For create mode
  allowUpload?: boolean;
  allowDelete?: boolean;
  readonly?: boolean;
}

interface PendingAttachment {
  file: File;
  attachmentType: TrainingAttachmentType;
  description: string;
  isTrainingMaterial: boolean;
  id: string; // Temporary ID for local management
}

const TRAINING_ATTACHMENT_TYPES = [
  { value: 'CourseSlides', label: 'Course Slides' },
  { value: 'Handbook', label: 'Training Handbook' },
  { value: 'Video', label: 'Training Video' },
  { value: 'Assessment', label: 'Assessment/Quiz' },
  { value: 'Certificate', label: 'Certificate Template' },
  { value: 'Reference', label: 'Reference Material' },
  { value: 'Checklist', label: 'Checklist' },
  { value: 'Procedure', label: 'Procedure Document' },
  { value: 'ComplianceDocument', label: 'Compliance Document' },
  { value: 'K3Material', label: 'K3 Training Material' },
  { value: 'Other', label: 'Other' }
];

const TrainingAttachmentManager: React.FC<TrainingAttachmentManagerProps> = ({
  trainingId,
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
  const [attachmentType, setAttachmentType] = useState<TrainingAttachmentType>('CourseSlides');
  const [description, setDescription] = useState('');
  const [isTrainingMaterial, setIsTrainingMaterial] = useState(true);
  const [uploadError, setUploadError] = useState<string | null>(null);

  // Get authentication token from Redux store
  const token = useSelector((state: RootState) => state.auth.token);
  
  const [uploadAttachment, { isLoading: isUploading }] = useUploadAttachmentMutation();
  const [deleteAttachment] = useDeleteAttachmentMutation();

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files || []);
    const validFiles: File[] = [];
    
    files.forEach((file) => {
      // Validate file type - comprehensive validation for training materials
      const validTypes = [
        'image/',
        'application/pdf',
        'application/msword',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        'application/vnd.ms-excel',
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        'application/vnd.ms-powerpoint',
        'application/vnd.openxmlformats-officedocument.presentationml.presentation',
        'text/',
        'video/',
        'audio/',
        'application/zip',
        'application/x-rar-compressed',
        'application/x-7z-compressed'
      ];

      const isValidType = validTypes.some(type => file.type.startsWith(type));
      
      if (!isValidType) {
        setUploadError(`Invalid file type: ${file.name}. Please upload images, documents, videos, or archives.`);
        return;
      }

      // Validate file size (50MB limit for training materials)
      const maxSizeInBytes = 50 * 1024 * 1024; // 50MB
      if (file.size > maxSizeInBytes) {
        setUploadError(`File too large: ${file.name}. Maximum size is 50MB.`);
        return;
      }

      validFiles.push(file);
    });

    if (validFiles.length > 0) {
      setSelectedFiles(validFiles);
      setShowUploadModal(true);
      setUploadError(null);
    }

    // Clear the input
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleUpload = async () => {
    if (selectedFiles.length === 0) return;

    try {
      setUploadError(null);

      if (trainingId) {
        // Upload to existing training
        for (const file of selectedFiles) {
          await uploadAttachment({
            trainingId: parseInt(trainingId),
            file,
            attachmentType,
            description,
            isTrainingMaterial
          }).unwrap();
        }
      } else {
        // Add to pending attachments for create mode
        const newPendingAttachments = selectedFiles.map(file => ({
          file,
          attachmentType,
          description,
          isTrainingMaterial,
          id: `pending-${Date.now()}-${Math.random()}`
        }));

        const updatedPendingAttachments = [...pendingAttachments, ...newPendingAttachments];
        setPendingAttachments(updatedPendingAttachments);
        onAttachmentsChange?.(updatedPendingAttachments);
      }

      // Reset form
      setSelectedFiles([]);
      setDescription('');
      setAttachmentType('CourseSlides');
      setIsTrainingMaterial(true);
      setShowUploadModal(false);

    } catch (error: any) {
      console.error('Upload failed:', error);
      setUploadError(error?.data?.message || 'Failed to upload file. Please try again.');
    }
  };

  const handleDeleteAttachment = async (attachmentId: number) => {
    if (!trainingId) return;

    try {
      await deleteAttachment({
        trainingId: parseInt(trainingId),
        attachmentId
      }).unwrap();
    } catch (error: any) {
      console.error('Delete failed:', error);
      setUploadError(error?.data?.message || 'Failed to delete attachment.');
    }
  };

  const handleDeletePendingAttachment = (pendingId: string) => {
    const updatedPendingAttachments = pendingAttachments.filter(att => att.id !== pendingId);
    setPendingAttachments(updatedPendingAttachments);
    onAttachmentsChange?.(updatedPendingAttachments);
  };

  const getFileIcon = (fileName: string, attachmentType: TrainingAttachmentType) => {
    const extension = fileName.split('.').pop()?.toLowerCase();
    
    // Check attachment type first for training-specific icons
    switch (attachmentType) {
      case 'CourseSlides':
        return faFilePowerpoint;
      case 'Handbook':
        return faBook;
      case 'Video':
        return faPlayCircle;
      case 'Assessment':
        return faClipboardCheck;
      case 'Certificate':
        return faCertificate;
      case 'K3Material':
        return faGraduationCap;
    }

    // Fallback to file extension
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
      case 'jpg':
      case 'jpeg':
      case 'png':
      case 'gif':
      case 'webp':
        return faImage;
      case 'mp4':
      case 'avi':
      case 'mov':
      case 'wmv':
        return faVideo;
      case 'zip':
      case 'rar':
      case '7z':
        return faFileArchive;
      case 'html':
      case 'css':
      case 'js':
      case 'json':
        return faFileCode;
      case 'txt':
        return faFileText;
      default:
        return faFileAlt;
    }
  };

  const getFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const handleDownloadAttachment = async (attachment: TrainingAttachmentDto) => {
    try {
      const response = await fetch(`/api/trainings/${trainingId}/attachments/${attachment.id}/download`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (!response.ok) {
        throw new Error('Download failed');
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = attachment.originalFileName;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Download failed:', error);
      setUploadError('Failed to download attachment. Please try again.');
    }
  };

  const allAttachments = [
    ...attachments.map(attachment => ({
      ...attachment,
      isPending: false,
      pendingId: undefined
    })),
    ...pendingAttachments.map(pending => ({
      id: 0,
      trainingId: 0,
      fileName: pending.file.name,
      originalFileName: pending.file.name,
      filePath: '',
      fileSize: pending.file.size,
      contentType: pending.file.type,
      attachmentType: pending.attachmentType,
      description: pending.description,
      uploadedBy: 'Current User',
      uploadedAt: new Date().toISOString(),
      isTrainingMaterial: pending.isTrainingMaterial,
      isPublic: false,
      version: 1,
      isApproved: false,
      isPending: true,
      pendingId: pending.id
    } as TrainingAttachmentDto & { isPending?: boolean; pendingId?: string }))
  ];

  return (
    <>
      <CCard>
        <CCardHeader className="d-flex justify-content-between align-items-center">
          <div className="d-flex align-items-center">
            <FontAwesomeIcon icon={faGraduationCap} className="me-2 text-primary" />
            <h6 className="mb-0">Training Materials & Attachments</h6>
            <CBadge color="info" className="ms-2">{allAttachments.length}</CBadge>
          </div>
          {allowUpload && !readonly && (
            <CButton
              color="primary"
              size="sm"
              onClick={() => fileInputRef.current?.click()}
              disabled={isUploading}
            >
              <FontAwesomeIcon icon={faPlus} className="me-1" />
              Add Files
            </CButton>
          )}
        </CCardHeader>
        <CCardBody>
          {uploadError && (
            <CAlert color="danger" className="mb-3">
              {uploadError}
            </CAlert>
          )}

          {allAttachments.length === 0 ? (
            <div className="text-center text-muted py-4">
              <FontAwesomeIcon icon={faFileAlt} size="2x" className="mb-2" />
              <p>No training materials uploaded yet.</p>
              {allowUpload && !readonly && (
                <CButton
                  color="primary"
                  variant="outline"
                  onClick={() => fileInputRef.current?.click()}
                >
                  <FontAwesomeIcon icon={faUpload} className="me-1" />
                  Upload First Material
                </CButton>
              )}
            </div>
          ) : (
            <CListGroup flush>
              {allAttachments.map((attachment, index) => (
                <CListGroupItem
                  key={attachment.isPending ? attachment.pendingId : attachment.id}
                  className="d-flex justify-content-between align-items-center"
                >
                  <div className="d-flex align-items-center flex-grow-1">
                    <FontAwesomeIcon
                      icon={getFileIcon(attachment.fileName, attachment.attachmentType)}
                      className="me-3 text-primary"
                      size="lg"
                    />
                    <div className="flex-grow-1">
                      <div className="d-flex align-items-center">
                        <h6 className="mb-1">{attachment.originalFileName}</h6>
                        {attachment.isTrainingMaterial && (
                          <CBadge color="success" className="ms-2">Training Material</CBadge>
                        )}
                        {attachment.isPending && (
                          <CBadge color="warning" className="ms-2">Pending Upload</CBadge>
                        )}
                      </div>
                      <small className="text-muted">
                        {attachment.attachmentType.replace(/([A-Z])/g, ' $1').trim()} • {getFileSize(attachment.fileSize)}
                        {!attachment.isPending && (
                          <> • Uploaded {formatDistanceToNow(new Date(attachment.uploadedAt))} ago by {attachment.uploadedBy}</>
                        )}
                      </small>
                      {attachment.description && (
                        <div>
                          <small className="text-muted">{attachment.description}</small>
                        </div>
                      )}
                    </div>
                  </div>
                  <div className="d-flex align-items-center">
                    {!attachment.isPending && (
                      <CButton
                        color="primary"
                        variant="outline"
                        size="sm"
                        className="me-2"
                        onClick={() => handleDownloadAttachment(attachment)}
                      >
                        <FontAwesomeIcon icon={faDownload} />
                      </CButton>
                    )}
                    {allowDelete && !readonly && (
                      <CButton
                        color="danger"
                        variant="outline"
                        size="sm"
                        onClick={() => {
                          if (attachment.isPending) {
                            handleDeletePendingAttachment(attachment.pendingId!);
                          } else {
                            handleDeleteAttachment(attachment.id);
                          }
                        }}
                      >
                        <FontAwesomeIcon icon={faTrash} />
                      </CButton>
                    )}
                  </div>
                </CListGroupItem>
              ))}
            </CListGroup>
          )}

          <input
            ref={fileInputRef}
            type="file"
            multiple
            hidden
            onChange={handleFileSelect}
            accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt,.jpg,.jpeg,.png,.gif,.mp4,.avi,.mov,.zip,.rar"
          />
        </CCardBody>
      </CCard>

      {/* Upload Modal */}
      <CModal visible={showUploadModal} onClose={() => setShowUploadModal(false)} size="lg">
        <CModalHeader>
          <CModalTitle>Upload Training Materials</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CForm>
            <div className="mb-3">
              <CFormLabel>Selected Files</CFormLabel>
              <CListGroup>
                {selectedFiles.map((file, index) => (
                  <CListGroupItem key={index} className="d-flex justify-content-between align-items-center">
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon
                        icon={getFileIcon(file.name, attachmentType)}
                        className="me-2 text-primary"
                      />
                      <div>
                        <div>{file.name}</div>
                        <small className="text-muted">{getFileSize(file.size)}</small>
                      </div>
                    </div>
                    <CButton
                      color="danger"
                      variant="outline"
                      size="sm"
                      onClick={() => setSelectedFiles(files => files.filter((_, i) => i !== index))}
                    >
                      <FontAwesomeIcon icon={faTimes} />
                    </CButton>
                  </CListGroupItem>
                ))}
              </CListGroup>
            </div>

            <CRow>
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel>Material Type</CFormLabel>
                  <CFormSelect
                    value={attachmentType}
                    onChange={(e) => setAttachmentType(e.target.value as TrainingAttachmentType)}
                  >
                    {TRAINING_ATTACHMENT_TYPES.map(type => (
                      <option key={type.value} value={type.value}>{type.label}</option>
                    ))}
                  </CFormSelect>
                </div>
              </CCol>
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel>&nbsp;</CFormLabel>
                  <div>
                    <CFormCheck
                      id="isTrainingMaterial"
                      label="This is training material"
                      checked={isTrainingMaterial}
                      onChange={(e) => setIsTrainingMaterial(e.target.checked)}
                    />
                  </div>
                </div>
              </CCol>
            </CRow>

            <div className="mb-3">
              <CFormLabel>Description</CFormLabel>
              <CFormTextarea
                rows={3}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Brief description of the material or its purpose"
              />
            </div>

            {uploadError && (
              <CAlert color="danger">
                {uploadError}
              </CAlert>
            )}
          </CForm>
        </CModalBody>
        <CModalFooter>
          <CButton
            color="secondary"
            onClick={() => setShowUploadModal(false)}
            disabled={isUploading}
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
                <CSpinner size="sm" className="me-1" />
                Uploading...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faUpload} className="me-1" />
                Upload Files
              </>
            )}
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};

export default TrainingAttachmentManager;