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
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Icon } from '../common/Icon';
import { ACTION_ICONS, FILE_TYPE_ICONS } from '../../utils/iconMappings';
import { formatDateTime } from '../../utils/dateUtils';
import {
  useGetHazardAttachmentsQuery,
  useUploadHazardAttachmentsMutation,
  useDeleteHazardAttachmentMutation,
  HazardAttachmentDto,
} from '../../features/hazards/hazardApi';
import { RootState } from '../../store';

interface HazardAttachmentManagerProps {
  hazardId: number;
  allowUpload?: boolean;
  allowDelete?: boolean;
}

const HazardAttachmentManager: React.FC<HazardAttachmentManagerProps> = ({
  hazardId,
  allowUpload = true,
  allowDelete = true,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [uploadError, setUploadError] = useState<string | null>(null);

  // Get authentication token from Redux store
  const token = useSelector((state: RootState) => state.auth.token);

  const {
    data: attachments = [],
    isLoading,
    refetch,
  } = useGetHazardAttachmentsQuery(hazardId);
  
  const [uploadAttachments, { isLoading: isUploading }] = useUploadHazardAttachmentsMutation();
  const [deleteAttachment] = useDeleteHazardAttachmentMutation();

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
        'text/plain',
      ];
      const isValidType =
        validTypes.some((type) => file.type.startsWith(type)) ||
        file.name.toLowerCase().endsWith('.docx');

      // Validate file size (50MB limit)
      const maxSize = 50 * 1024 * 1024; // 50MB

      if (!isValidType) {
        setUploadError(
          `File "${file.name}" is not a supported type. Please upload images, videos, PDFs, or documents.`
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
      await uploadAttachments({
        hazardId,
        files: selectedFiles,
      }).unwrap();

      setSelectedFiles([]);
      setUploadError(null);
      refetch();
    } catch (error) {
      setUploadError('Failed to upload files. Please try again.');
      console.error('Upload error:', error);
    }
  };

  const handleDeleteAttachment = async (attachmentId: number) => {
    if (!confirm('Are you sure you want to delete this attachment?')) return;

    try {
      await deleteAttachment({
        hazardId,
        attachmentId,
      }).unwrap();

      refetch();
    } catch (error) {
      console.error('Delete error:', error);
    }
  };

  const handleDownload = async (attachment: HazardAttachmentDto) => {
    try {
      if (!token) {
        setUploadError('Authentication required. Please log in again.');
        return;
      }

      // Construct the download URL
      const downloadUrl = `/api/hazard/${hazardId}/attachments/${attachment.id}/download`;

      // Fetch the file with authentication headers
      const response = await fetch(downloadUrl, {
        method: 'GET',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        throw new Error(
          `Failed to download file: ${response.status} ${response.statusText}`
        );
      }

      // Get the file blob
      const blob = await response.blob();

      // Create a download link
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = attachment.fileName;
      link.target = '_blank';

      // Trigger download
      document.body.appendChild(link);
      link.click();

      // Cleanup
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Download failed:', error);
      setUploadError(
        `Failed to download ${attachment.fileName}. Please try again.`
      );
    }
  };

  const getFileIcon = (fileName: string) => {
    const extension = fileName.split('.').pop()?.toLowerCase();
    switch (extension) {
      case 'pdf':
        return FILE_TYPE_ICONS.pdf;
      case 'doc':
      case 'docx':
        return FILE_TYPE_ICONS.document;
      case 'txt':
        return FILE_TYPE_ICONS.text;
      default:
        if (fileName.match(/\.(jpg|jpeg|png|gif)$/i)) {
          return FILE_TYPE_ICONS.image;
        }
        if (fileName.match(/\.(mp4|avi|mov)$/i)) {
          return FILE_TYPE_ICONS.video;
        }
        return FILE_TYPE_ICONS.default;
    }
  };

  return (
    <CCard className="mb-4">
      <CCardHeader>
        <h6 className="mb-0">
          <FontAwesomeIcon icon={FILE_TYPE_ICONS.default} className="me-2" />
          Attachments ({attachments.length})
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
            <div className="mb-3">
              <CInputGroup>
                <CFormInput
                  type="file"
                  ref={fileInputRef}
                  onChange={handleFileSelect}
                  multiple
                  accept="image/*,video/*,.pdf,.doc,.docx,.txt"
                  disabled={isUploading}
                />
                <CButton
                  color="outline-primary"
                  onClick={() => fileInputRef.current?.click()}
                  disabled={isUploading}
                >
                  <FontAwesomeIcon
                    icon={ACTION_ICONS.upload}
                    className="me-2"
                  />
                  Select Files
                </CButton>
              </CInputGroup>
              <small className="text-muted">
                Supported: Images, Videos, PDFs, Documents (Max 50MB each)
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
                      <Icon icon={getFileIcon(file.name)} size="sm" />
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
                      <Icon icon={ACTION_ICONS.upload} className="me-2" />
                      Upload Files ({selectedFiles.length})
                    </>
                  )}
                </CButton>
              </div>
            )}
          </div>
        )}

        {/* Existing Attachments */}
        {isLoading ? (
          <div className="text-center py-3">
            <CSpinner size="sm" className="me-2" />
            Loading attachments...
          </div>
        ) : attachments.length === 0 ? (
          <div className="text-center text-muted py-3">
            <FontAwesomeIcon
              icon={FILE_TYPE_ICONS.default}
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
                  <Icon
                    icon={getFileIcon(attachment.fileName)}
                    className="me-3 text-muted"
                  />
                  <div>
                    <div className="fw-medium">{attachment.fileName}</div>
                    <small className="text-muted">
                      {(attachment.fileSize / 1024).toFixed(1)} KB • Uploaded by{' '}
                      {attachment.uploadedBy} •{' '}
                      {formatDateTime(attachment.uploadedAt)}
                    </small>
                  </div>
                </div>
                <div className="d-flex gap-2">
                  <CButton
                    size="sm"
                    color="outline-primary"
                    onClick={() => handleDownload(attachment)}
                    title="Download"
                  >
                    <FontAwesomeIcon icon={ACTION_ICONS.download} size="sm" />
                  </CButton>
                  {allowDelete && (
                    <CButton
                      size="sm"
                      color="outline-danger"
                      onClick={() => handleDeleteAttachment(attachment.id)}
                      disabled={isUploading}
                      title="Delete"
                    >
                      <FontAwesomeIcon icon={ACTION_ICONS.delete} size="sm" />
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

export default HazardAttachmentManager;