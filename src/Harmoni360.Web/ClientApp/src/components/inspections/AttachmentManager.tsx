import React, { useState, useRef } from 'react';
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
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
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
  faFileAlt,
  faEllipsisV,
  faEye
} from '@fortawesome/free-solid-svg-icons';
import { formatDistanceToNow } from 'date-fns';
import {
  useUploadAttachmentMutation,
  useDeleteAttachmentMutation,
  useLazyDownloadAttachmentQuery,
} from '../../features/inspections/inspectionApi';
import { toast } from 'react-toastify';

interface InspectionAttachmentDto {
  id: number;
  fileName: string;
  originalFileName: string;
  filePath: string;
  fileSize: number;
  contentType: string;
  description?: string;
  category?: string;
  uploadedAt: string;
  uploadedBy: string;
  uploadedByName: string;
}

interface AttachmentManagerProps {
  inspectionId?: number;
  attachments?: InspectionAttachmentDto[];
  allowUpload?: boolean;
  allowDelete?: boolean;
  allowView?: boolean;
  compact?: boolean;
  onAttachmentsChange?: (attachments: InspectionAttachmentDto[]) => void;
}

const InspectionAttachmentManager: React.FC<AttachmentManagerProps> = ({
  inspectionId,
  attachments = [],
  allowUpload = true,
  allowDelete = true,
  allowView = true,
  compact = false,
  onAttachmentsChange,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [showUploadModal, setShowUploadModal] = useState(false);
  const [uploadDescription, setUploadDescription] = useState('');
  const [uploadCategory, setUploadCategory] = useState('');

  const [uploadAttachment, { isLoading: isUploading }] = useUploadAttachmentMutation();
  const [deleteAttachment, { isLoading: isDeleting }] = useDeleteAttachmentMutation();
  const [downloadAttachment] = useLazyDownloadAttachmentQuery();

  const attachmentCategories = [
    { value: '', label: 'General' },
    { value: 'Photos', label: 'Photos' },
    { value: 'Documents', label: 'Documents' },
    { value: 'Reports', label: 'Reports' },
    { value: 'Certificates', label: 'Certificates' },
    { value: 'Plans', label: 'Plans/Drawings' },
    { value: 'Other', label: 'Other' },
  ];

  const getFileIcon = (contentType: string, fileName: string) => {
    if (contentType.startsWith('image/')) return faImage;
    if (contentType.startsWith('video/')) return faVideo;
    if (contentType === 'application/pdf') return faFilePdf;
    if (contentType.includes('word') || fileName.endsWith('.docx')) return faFileWord;
    if (contentType.includes('excel') || fileName.endsWith('.xlsx')) return faFileExcel;
    if (contentType.includes('powerpoint') || fileName.endsWith('.pptx')) return faFilePowerpoint;
    if (contentType.includes('zip') || contentType.includes('rar')) return faFileArchive;
    if (contentType === 'text/plain') return faFileText;
    if (contentType.includes('code') || fileName.match(/\.(js|ts|jsx|tsx|html|css|json)$/)) return faFileCode;
    return faFileAlt;
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files || []);
    const validFiles: File[] = [];

    files.forEach((file) => {
      // Validate file type
      const validTypes = [
        'image/',
        'video/',
        'application/pdf',
        'application/msword',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        'application/vnd.ms-excel',
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        'text/plain',
        'text/csv',
      ];
      const isValidType = validTypes.some((type) => file.type.startsWith(type));

      // Validate file size (50MB limit)
      const maxSize = 50 * 1024 * 1024; // 50MB

      if (!isValidType) {
        toast.error(`File "${file.name}" is not a supported type.`);
        return;
      }

      if (file.size > maxSize) {
        toast.error(`File "${file.name}" is too large. Maximum size is 50MB.`);
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
    if (!inspectionId || selectedFiles.length === 0) return;

    try {
      for (const file of selectedFiles) {
        await uploadAttachment({
          inspectionId,
          file,
          description: uploadDescription || undefined,
          category: uploadCategory || undefined,
        }).unwrap();
      }

      toast.success(`${selectedFiles.length} file(s) uploaded successfully!`);
      setSelectedFiles([]);
      setUploadDescription('');
      setUploadCategory('');
      setShowUploadModal(false);
      
      // Trigger refresh if callback provided
      if (onAttachmentsChange) {
        // This would typically trigger a refetch of the inspection data
        onAttachmentsChange(attachments);
      }
    } catch (error: any) {
      console.error('Upload error:', error);
      toast.error(error?.data?.message || 'Failed to upload files');
    }
  };

  const handleDelete = async (attachmentId: number, fileName: string) => {
    if (!inspectionId || !window.confirm(`Are you sure you want to delete "${fileName}"?`)) return;

    try {
      await deleteAttachment({ inspectionId, attachmentId }).unwrap();
      toast.success('Attachment deleted successfully');
      
      // Trigger refresh if callback provided
      if (onAttachmentsChange) {
        onAttachmentsChange(attachments.filter(a => a.id !== attachmentId));
      }
    } catch (error: any) {
      console.error('Delete error:', error);
      toast.error(error?.data?.message || 'Failed to delete attachment');
    }
  };

  const handleDownload = async (attachmentId: number, fileName: string) => {
    if (!inspectionId) return;

    try {
      const result = await downloadAttachment({ inspectionId, attachmentId });
      if (result.data) {
        const url = window.URL.createObjectURL(result.data);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      }
    } catch (error: any) {
      console.error('Download error:', error);
      toast.error(error?.data?.message || 'Failed to download attachment');
    }
  };

  const handleView = (attachment: InspectionAttachmentDto) => {
    if (attachment.contentType.startsWith('image/') || attachment.contentType === 'application/pdf') {
      // For images and PDFs, we can open them in a new tab
      const viewUrl = `/api/inspections/${inspectionId}/attachments/${attachment.id}/download`;
      window.open(viewUrl, '_blank');
    } else {
      // For other file types, download them
      handleDownload(attachment.id, attachment.originalFileName);
    }
  };

  if (compact) {
    return (
      <div className="inspection-attachments-compact">
        {attachments.length > 0 && (
          <div className="mb-3">
            <h6 className="fw-semibold mb-2">
              <FontAwesomeIcon icon={faFileContract} className="me-2" />
              Attachments ({attachments.length})
            </h6>
            <div className="d-flex flex-wrap gap-2">
              {attachments.map((attachment) => (
                <CBadge
                  key={attachment.id}
                  color="light"
                  className="d-flex align-items-center p-2 cursor-pointer"
                  onClick={() => allowView && handleView(attachment)}
                  title={`${attachment.originalFileName} (${formatFileSize(attachment.fileSize)})`}
                >
                  <FontAwesomeIcon 
                    icon={getFileIcon(attachment.contentType, attachment.originalFileName)} 
                    className="me-1" 
                  />
                  {attachment.originalFileName.length > 20
                    ? `${attachment.originalFileName.substring(0, 20)}...`
                    : attachment.originalFileName
                  }
                </CBadge>
              ))}
            </div>
          </div>
        )}

        {allowUpload && inspectionId && (
          <div>
            <input
              ref={fileInputRef}
              type="file"
              multiple
              onChange={handleFileSelect}
              style={{ display: 'none' }}
              accept="image/*,video/*,.pdf,.doc,.docx,.txt,.csv,.xls,.xlsx"
            />
            <CButton
              color="primary"
              variant="outline"
              size="sm"
              onClick={() => fileInputRef.current?.click()}
              disabled={isUploading}
            >
              <FontAwesomeIcon icon={faUpload} className="me-1" />
              {isUploading ? 'Uploading...' : 'Add Files'}
            </CButton>
          </div>
        )}

        {/* Upload Modal */}
        <CModal visible={showUploadModal} onClose={() => setShowUploadModal(false)}>
          <CModalHeader onClose={() => setShowUploadModal(false)}>
            <CModalTitle>Upload Files</CModalTitle>
          </CModalHeader>
          <CModalBody>
            <div className="mb-3">
              <strong>Selected Files:</strong>
              <ul className="mt-2">
                {selectedFiles.map((file, index) => (
                  <li key={index}>
                    {file.name} ({formatFileSize(file.size)})
                  </li>
                ))}
              </ul>
            </div>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="upload-category">Category</CFormLabel>
                <CFormSelect
                  id="upload-category"
                  value={uploadCategory}
                  onChange={(e) => setUploadCategory(e.target.value)}
                >
                  {attachmentCategories.map((category) => (
                    <option key={category.value} value={category.value}>
                      {category.label}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
            </CRow>

            <div className="mb-3">
              <CFormLabel htmlFor="upload-description">Description (Optional)</CFormLabel>
              <CFormTextarea
                id="upload-description"
                value={uploadDescription}
                onChange={(e) => setUploadDescription(e.target.value)}
                placeholder="Enter a description for these files..."
                rows={3}
              />
            </div>
          </CModalBody>
          <CModalFooter>
            <CButton color="secondary" onClick={() => setShowUploadModal(false)}>
              Cancel
            </CButton>
            <CButton color="primary" onClick={handleUpload} disabled={isUploading}>
              {isUploading ? (
                <>
                  <CSpinner size="sm" className="me-2" />
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
      </div>
    );
  }

  return (
    <CCard>
      <CCardHeader className="d-flex justify-content-between align-items-center">
        <div>
          <h5 className="mb-0">
            <FontAwesomeIcon icon={faFileContract} className="me-2" />
            Attachments
          </h5>
          {attachments.length > 0 && (
            <small className="text-medium-emphasis">
              {attachments.length} file(s) attached
            </small>
          )}
        </div>
        {allowUpload && inspectionId && (
          <div>
            <input
              ref={fileInputRef}
              type="file"
              multiple
              onChange={handleFileSelect}
              style={{ display: 'none' }}
              accept="image/*,video/*,.pdf,.doc,.docx,.txt,.csv,.xls,.xlsx"
            />
            <CButton
              color="primary"
              size="sm"
              onClick={() => fileInputRef.current?.click()}
              disabled={isUploading}
            >
              <FontAwesomeIcon icon={faPlus} className="me-1" />
              {isUploading ? 'Uploading...' : 'Add Files'}
            </CButton>
          </div>
        )}
      </CCardHeader>
      <CCardBody>
        {uploadError && (
          <CAlert color="danger" className="mb-3">
            {uploadError}
          </CAlert>
        )}

        {attachments.length === 0 ? (
          <div className="text-center py-4 text-muted">
            <FontAwesomeIcon icon={faFileContract} size="2x" className="mb-3" />
            <div>No attachments found</div>
            {allowUpload && inspectionId && (
              <div className="mt-2">
                <small>Click "Add Files" to upload documents, images, or other files</small>
              </div>
            )}
          </div>
        ) : (
          <CListGroup flush>
            {attachments.map((attachment) => (
              <CListGroupItem
                key={attachment.id}
                className="d-flex justify-content-between align-items-center"
              >
                <div className="d-flex align-items-center flex-grow-1">
                  <FontAwesomeIcon
                    icon={getFileIcon(attachment.contentType, attachment.originalFileName)}
                    className="me-3 text-primary"
                    size="lg"
                  />
                  <div className="flex-grow-1">
                    <div className="fw-semibold">{attachment.originalFileName}</div>
                    <div className="text-muted small">
                      {formatFileSize(attachment.fileSize)} • 
                      Uploaded {formatDistanceToNow(new Date(attachment.uploadedAt), { addSuffix: true })} by {attachment.uploadedByName}
                    </div>
                    {attachment.description && (
                      <div className="text-muted small mt-1">{attachment.description}</div>
                    )}
                    {attachment.category && (
                      <CBadge color="secondary" className="mt-1">
                        {attachment.category}
                      </CBadge>
                    )}
                  </div>
                </div>
                <div className="d-flex align-items-center">
                  <CDropdown>
                    <CDropdownToggle color="ghost" size="sm" caret={false}>
                      <FontAwesomeIcon icon={faEllipsisV} />
                    </CDropdownToggle>
                    <CDropdownMenu>
                      {allowView && (
                        <CDropdownItem onClick={() => handleView(attachment)}>
                          <FontAwesomeIcon icon={faEye} className="me-2" />
                          View
                        </CDropdownItem>
                      )}
                      <CDropdownItem onClick={() => handleDownload(attachment.id, attachment.originalFileName)}>
                        <FontAwesomeIcon icon={faDownload} className="me-2" />
                        Download
                      </CDropdownItem>
                      {allowDelete && (
                        <>
                          <CDropdownItem divider />
                          <CDropdownItem
                            onClick={() => handleDelete(attachment.id, attachment.originalFileName)}
                            className="text-danger"
                            disabled={isDeleting}
                          >
                            <FontAwesomeIcon icon={faTrash} className="me-2" />
                            Delete
                          </CDropdownItem>
                        </>
                      )}
                    </CDropdownMenu>
                  </CDropdown>
                </div>
              </CListGroupItem>
            ))}
          </CListGroup>
        )}
      </CCardBody>

      {/* Upload Modal */}
      <CModal visible={showUploadModal} onClose={() => setShowUploadModal(false)}>
        <CModalHeader onClose={() => setShowUploadModal(false)}>
          <CModalTitle>Upload Files</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <div className="mb-3">
            <strong>Selected Files:</strong>
            <ul className="mt-2">
              {selectedFiles.map((file, index) => (
                <li key={index}>
                  {file.name} ({formatFileSize(file.size)})
                </li>
              ))}
            </ul>
          </div>

          <CRow className="mb-3">
            <CCol md={6}>
              <CFormLabel htmlFor="upload-category">Category</CFormLabel>
              <CFormSelect
                id="upload-category"
                value={uploadCategory}
                onChange={(e) => setUploadCategory(e.target.value)}
              >
                {attachmentCategories.map((category) => (
                  <option key={category.value} value={category.value}>
                    {category.label}
                  </option>
                ))}
              </CFormSelect>
            </CCol>
          </CRow>

          <div className="mb-3">
            <CFormLabel htmlFor="upload-description">Description (Optional)</CFormLabel>
            <CFormTextarea
              id="upload-description"
              value={uploadDescription}
              onChange={(e) => setUploadDescription(e.target.value)}
              placeholder="Enter a description for these files..."
              rows={3}
            />
          </div>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowUploadModal(false)}>
            Cancel
          </CButton>
          <CButton color="primary" onClick={handleUpload} disabled={isUploading}>
            {isUploading ? (
              <>
                <CSpinner size="sm" className="me-2" />
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
    </CCard>
  );
};

export default InspectionAttachmentManager;