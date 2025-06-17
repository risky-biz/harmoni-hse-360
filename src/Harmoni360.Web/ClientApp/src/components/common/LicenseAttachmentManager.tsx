import React, { useState, useRef } from 'react';
import { useSelector } from 'react-redux';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CFormInput,
  CInputGroup,
  CBadge,
  CSpinner,
  CAlert,
  CListGroup,
  CListGroupItem,
  CFormSelect,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faUpload,
  faDownload,
  faTrash,
  faPaperclip,
  faFileAlt,
  faFilePdf,
  faFileWord,
  faFileImage,
  faFileArchive,
  faFile,
} from '@fortawesome/free-solid-svg-icons';
import { format } from 'date-fns';
import {
  useUploadAttachmentMutation,
  useDeleteAttachmentMutation,
} from '../../features/licenses/licenseApi';
import { LicenseDto, LicenseAttachmentDto, ATTACHMENT_TYPES } from '../../types/license';
import { RootState } from '../../store';

interface LicenseAttachmentManagerProps {
  license: LicenseDto;
  allowUpload?: boolean;
  allowDelete?: boolean;
  onAttachmentsChange?: () => void;
}

const LicenseAttachmentManager: React.FC<LicenseAttachmentManagerProps> = ({
  license,
  allowUpload = true,
  allowDelete = true,
  onAttachmentsChange,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [attachmentType, setAttachmentType] = useState('SupportingDocument');
  const [description, setDescription] = useState('');

  // Get authentication token from Redux store
  const token = useSelector((state: RootState) => state.auth.token);

  const [uploadAttachment, { isLoading: isUploading }] = useUploadAttachmentMutation();
  const [deleteAttachment, { isLoading: isDeleting }] = useDeleteAttachmentMutation();

  const attachments = license.attachments || [];

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
        'application/zip',
        'application/x-zip-compressed',
      ];
      const isValidType = validTypes.some((type) => file.type.startsWith(type)) ||
                         file.name.toLowerCase().match(/\.(docx|xlsx|zip|rar|7z)$/);

      // Validate file size (10MB limit)
      const maxSize = 10 * 1024 * 1024; // 10MB

      if (!isValidType) {
        setUploadError(
          `File "${file.name}" is not a supported type. Please upload documents, images, or archives.`
        );
        return;
      }

      if (file.size > maxSize) {
        setUploadError(
          `File "${file.name}" is too large. Maximum size is 10MB.`
        );
        return;
      }

      validFiles.push(file);
    });

    if (validFiles.length > 0) {
      setSelectedFiles((prev) => [...prev, ...validFiles]);
      setUploadError(null);
    }

    // Clear the input
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const removeSelectedFile = (index: number) => {
    setSelectedFiles((prev) => prev.filter((_, i) => i !== index));
  };

  const handleUpload = async () => {
    if (selectedFiles.length === 0) return;

    try {
      // Upload each file individually
      for (const file of selectedFiles) {
        await uploadAttachment({
          licenseId: license.id,
          file,
          attachmentType,
          description: description || `Uploaded ${file.name}`,
        }).unwrap();
      }

      setSelectedFiles([]);
      setDescription('');
      setUploadError(null);
      onAttachmentsChange?.();
    } catch (error) {
      setUploadError('Failed to upload files. Please try again.');
      console.error('Upload error:', error);
    }
  };

  const handleDeleteAttachment = async (attachmentId: number) => {
    if (!confirm('Are you sure you want to delete this attachment?')) return;

    try {
      await deleteAttachment({
        licenseId: license.id,
        attachmentId,
      }).unwrap();

      onAttachmentsChange?.();
    } catch (error) {
      console.error('Delete error:', error);
      setUploadError('Failed to delete attachment. Please try again.');
    }
  };

  const handleDownload = async (attachment: LicenseAttachmentDto) => {
    try {
      if (!token) {
        setUploadError('Authentication required. Please log in again.');
        return;
      }

      // Open the download URL in a new tab
      const downloadUrl = `/api/licenses/${license.id}/attachments/${attachment.id}`;
      window.open(downloadUrl, '_blank');
    } catch (error) {
      console.error('Download failed:', error);
      setUploadError(`Failed to download ${attachment.originalFileName}. Please try again.`);
    }
  };

  const getFileIcon = (fileName: string, contentType?: string) => {
    const extension = fileName.split('.').pop()?.toLowerCase();
    
    if (contentType?.startsWith('image/') || /\.(jpg|jpeg|png|gif|bmp|svg)$/i.test(fileName)) {
      return faFileImage;
    }
    
    switch (extension) {
      case 'pdf':
        return faFilePdf;
      case 'doc':
      case 'docx':
        return faFileWord;
      case 'zip':
      case 'rar':
      case '7z':
        return faFileArchive;
      default:
        return faFileAlt;
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  return (
    <CCard className="mb-4">
      <CCardHeader>
        <h6 className="mb-0">
          <FontAwesomeIcon icon={faPaperclip} className="me-2" />
          License Attachments ({attachments.length})
        </h6>
      </CCardHeader>
      <CCardBody>
        {uploadError && (
          <CAlert
            color="danger"
            dismissible
            onClose={() => setUploadError(null)}
          >
            {uploadError}
          </CAlert>
        )}

        {/* Upload Section */}
        {allowUpload && (
          <div className="mb-4">
            <div className="row g-3 mb-3">
              <div className="col-md-6">
                <label className="form-label small">Attachment Type</label>
                <CFormSelect
                  value={attachmentType}
                  onChange={(e) => setAttachmentType(e.target.value)}
                  disabled={isUploading}
                >
                  {ATTACHMENT_TYPES.map((type) => (
                    <option key={type.value} value={type.value}>
                      {type.label}
                    </option>
                  ))}
                </CFormSelect>
              </div>
              <div className="col-md-6">
                <label className="form-label small">Description (Optional)</label>
                <CFormInput
                  type="text"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder="Brief description of the attachment"
                  disabled={isUploading}
                />
              </div>
            </div>

            <div className="mb-3">
              <CInputGroup>
                <CFormInput
                  type="file"
                  ref={fileInputRef}
                  onChange={handleFileSelect}
                  multiple
                  accept=".pdf,.doc,.docx,.xls,.xlsx,.jpg,.jpeg,.png,.gif,.txt,.zip,.rar"
                  disabled={isUploading}
                />
                <CButton
                  color="outline-primary"
                  onClick={() => fileInputRef.current?.click()}
                  disabled={isUploading}
                >
                  <FontAwesomeIcon icon={faUpload} className="me-2" />
                  Select Files
                </CButton>
              </CInputGroup>
              <small className="text-muted">
                Supported: PDF, Word, Excel, Images, Text, Archives (Max 10MB each)
              </small>
            </div>

            {/* Selected Files */}
            {selectedFiles.length > 0 && (
              <div className="mb-3">
                <div className="d-flex flex-wrap gap-2 mb-3">
                  {selectedFiles.map((file, index) => (
                    <CBadge
                      key={index}
                      color="light"
                      className="d-flex align-items-center gap-2 p-2"
                    >
                      <FontAwesomeIcon icon={getFileIcon(file.name, file.type)} size="sm" />
                      <span>{file.name}</span>
                      <CButton
                        size="sm"
                        color="light"
                        variant="ghost"
                        onClick={() => removeSelectedFile(index)}
                        disabled={isUploading}
                      >
                        ×
                      </CButton>
                    </CBadge>
                  ))}
                </div>
                <CButton
                  color="primary"
                  onClick={handleUpload}
                  disabled={isUploading}
                  size="sm"
                >
                  {isUploading ? (
                    <>
                      <CSpinner size="sm" className="me-2" />
                      Uploading...
                    </>
                  ) : (
                    <>
                      <FontAwesomeIcon icon={faUpload} className="me-2" />
                      Upload Files ({selectedFiles.length})
                    </>
                  )}
                </CButton>
              </div>
            )}
          </div>
        )}

        {/* Existing Attachments */}
        {attachments.length === 0 ? (
          <div className="text-center text-muted py-3">
            <FontAwesomeIcon
              icon={faPaperclip}
              size="2x"
              className="mb-2 opacity-50"
            />
            <p className="mb-0">No attachments uploaded yet</p>
          </div>
        ) : (
          <CListGroup flush>
            {attachments.map((attachment) => (
              <CListGroupItem
                key={attachment.id}
                className="d-flex justify-content-between align-items-center"
              >
                <div className="d-flex align-items-center">
                  <FontAwesomeIcon
                    icon={getFileIcon(attachment.originalFileName, attachment.contentType)}
                    className="me-3 text-muted"
                  />
                  <div>
                    <div className="fw-medium">{attachment.originalFileName}</div>
                    <small className="text-muted">
                      {formatFileSize(attachment.fileSize)} • 
                      <CBadge color="info" className="ms-1 me-1">
                        {attachment.attachmentTypeDisplay}
                      </CBadge>
                      {attachment.isRequired && (
                        <CBadge color="warning" className="me-1">Required</CBadge>
                      )}
                      • Uploaded by {attachment.uploadedBy} • 
                      {format(new Date(attachment.uploadedAt), 'MMM dd, yyyy HH:mm')}
                    </small>
                    {attachment.description && (
                      <div className="text-muted small mt-1">{attachment.description}</div>
                    )}
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
                  {allowDelete && (
                    <CButton
                      size="sm"
                      color="outline-danger"
                      onClick={() => handleDeleteAttachment(attachment.id)}
                      disabled={isDeleting}
                      title="Delete"
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
  );
};

export default LicenseAttachmentManager;