import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CRow,
  CCol,
  CAlert,
  CButton,
  CFormSelect,
  CFormLabel,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faVideo,
  faPlus,
  faInfoCircle,
} from '@fortawesome/free-solid-svg-icons';
import { VideoUploadComponent } from './VideoUploadComponent';
import { VideoList } from './VideoList';
import {
  WorkPermitSettingsResponse,
} from '../../../types/workPermitSettings';

interface VideoManagementTabProps {
  settings: WorkPermitSettingsResponse[];
  onDataChange?: () => void;
}

export const VideoManagementTab: React.FC<VideoManagementTabProps> = ({ settings, onDataChange }) => {
  const [selectedSettingsId, setSelectedSettingsId] = useState<string>('');
  const [showUpload, setShowUpload] = useState(false);

  const selectedSettings = settings.find(s => s.id === Number(selectedSettingsId));
  const activeSettings = settings.find(s => s.isActive);
  const settingsWithVideoRequired = settings.filter(s => s.requireSafetyInduction);

  if (settings.length === 0) {
    return (
      <CAlert color="warning">
        <h5>No Configurations Available</h5>
        <p>Please create a work permit configuration in the "Form Configuration" tab first.</p>
      </CAlert>
    );
  }

  if (settingsWithVideoRequired.length === 0) {
    return (
      <CAlert color="info">
        <h5>Safety Videos Not Required</h5>
        <p>None of your current configurations require safety induction videos.</p>
        <p>To use this feature, enable "Safety Video Required" in a configuration.</p>
      </CAlert>
    );
  }

  return (
    <div className="video-management-tab">
      {activeSettings?.requireSafetyInduction && (
        <CAlert color="info" className="mb-4">
          <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
          <strong>Active Configuration:</strong> Work Permit Settings requires users to watch safety videos 
          before submitting work permits.
        </CAlert>
      )}

      <CRow className="mb-3 mb-md-4">
        <CCol lg={6} className="mb-3 mb-lg-0">
          <div>
            <CFormLabel htmlFor="settingsSelect">Select Configuration</CFormLabel>
            <CFormSelect
              id="settingsSelect"
              value={selectedSettingsId}
              onChange={(e) => {
                setSelectedSettingsId(e.target.value);
                setShowUpload(false);
              }}
              style={{ fontSize: '16px' }} // Prevent zoom on iOS
            >
              <option value="">-- Select a configuration --</option>
              {settingsWithVideoRequired.map(setting => (
                <option key={setting.id} value={setting.id}>
                  Work Permit Configuration {setting.isActive && '(Active)'}
                </option>
              ))}
            </CFormSelect>
          </div>
        </CCol>
        <CCol lg={6} className="d-flex align-items-end">
          {selectedSettingsId && !showUpload && (
            <CButton
              color="primary"
              onClick={() => setShowUpload(true)}
              className="w-100 w-lg-auto"
            >
              <FontAwesomeIcon icon={faPlus} className="me-2" />
              <span className="d-none d-sm-inline">Upload Video</span>
              <span className="d-sm-none">Upload</span>
            </CButton>
          )}
        </CCol>
      </CRow>

      {selectedSettings && (
        <>
          {showUpload && (
            <CCard className="mb-4">
              <CCardBody>
                <h5 className="mb-3">Upload Safety Induction Video</h5>
                <VideoUploadComponent
                  settingsId={selectedSettingsId}
                  onUploadComplete={() => {
                    setShowUpload(false);
                    if (onDataChange) {
                      onDataChange();
                    }
                  }}
                  onCancel={() => setShowUpload(false)}
                />
              </CCardBody>
            </CCard>
          )}

          <VideoList
            settingsId={selectedSettingsId}
            settings={selectedSettings!}
          />
        </>
      )}

      {!selectedSettingsId && (
        <CCard>
          <CCardBody className="text-center py-5">
            <FontAwesomeIcon icon={faVideo} size="3x" className="text-muted mb-3" />
            <h5>Select a Configuration</h5>
            <p className="text-muted">
              Choose a configuration above to manage its safety induction videos.
            </p>
          </CCardBody>
        </CCard>
      )}
    </div>
  );
};