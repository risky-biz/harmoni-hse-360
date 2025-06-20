import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CRow,
  CCol,
  CTable,
  CTableHead,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell,
  CBadge,
  CButton,
  CSpinner,
  CAlert,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CFormInput,
  CFormTextarea,
  CForm,
  CFormLabel,
  CTooltip,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CButtonGroup
} from '@coreui/react';
import { Icon } from '../common/Icon';
import { cilSettings, cilPowerStandby, cilWarning, cilInfo, cilPencil, cilHistory } from '@coreui/icons';
import {
  useGetModuleConfigurationsQuery,
  useEnableModuleMutation,
  useDisableModuleMutation,
  useUpdateModuleSettingsMutation,
  useCanDisableModuleQuery,
  ModuleConfigurationDto,
  ModuleType,
  getModuleDisplayName,
  getModuleIcon
} from '../../services/moduleConfigurationApi';

interface ModuleToggleProps {
  module: ModuleConfigurationDto;
  onToggle: (moduleType: ModuleType, enabled: boolean) => void;
  isLoading: boolean;
}

const ModuleToggle: React.FC<ModuleToggleProps> = ({ module, onToggle, isLoading }) => {
  const { data: canDisableData } = useCanDisableModuleQuery(module.moduleType, {
    skip: module.isEnabled === false
  });

  const handleToggle = () => {
    if (!isLoading) {
      onToggle(module.moduleType, !module.isEnabled);
    }
  };

  const buttonColor = module.isEnabled ? 'success' : 'secondary';
  const buttonVariant = module.isEnabled ? undefined : 'outline';
  const icon = module.isEnabled ? cilPowerStandby : cilPowerStandby;
  const text = module.isEnabled ? 'Enabled' : 'Disabled';

  const hasWarnings = !module.isEnabled || (canDisableData && !canDisableData.canDisable);

  return (
    <div className="d-flex align-items-center gap-2">
      <CButton
        color={buttonColor}
        variant={buttonVariant}
        size="sm"
        onClick={handleToggle}
        disabled={isLoading || (!module.isEnabled && !module.canBeDisabled)}
      >
        {isLoading ? (
          <CSpinner size="sm" className="me-1" />
        ) : (
          <Icon icon={icon} className="me-1" />
        )}
        {text}
      </CButton>
      
      {hasWarnings && (
        <CTooltip content={
          module.disableWarnings.length > 0 
            ? module.disableWarnings.join(', ')
            : 'This module cannot be disabled'
        }>
          <Icon icon={cilWarning} className="text-warning" />
        </CTooltip>
      )}
    </div>
  );
};

interface ModuleSettingsModalProps {
  module: ModuleConfigurationDto | null;
  isOpen: boolean;
  onClose: () => void;
  onSave: (moduleType: ModuleType, settings: string, context?: string) => void;
  isLoading: boolean;
}

const ModuleSettingsModal: React.FC<ModuleSettingsModalProps> = ({
  module,
  isOpen,
  onClose,
  onSave,
  isLoading
}) => {
  const [settings, setSettings] = useState('');
  const [context, setContext] = useState('');

  React.useEffect(() => {
    if (module) {
      setSettings(module.settings || '');
      setContext('');
    }
  }, [module]);

  const handleSave = () => {
    if (module) {
      onSave(module.moduleType, settings, context || undefined);
    }
  };

  return (
    <CModal visible={isOpen} onClose={onClose} size="lg">
      <CModalHeader>
        <CModalTitle>
          {module && (
            <>
              <Icon icon={getModuleIcon(module.moduleType)} className="me-2" />
              Configure {module.displayName}
            </>
          )}
        </CModalTitle>
      </CModalHeader>
      <CModalBody>
        {module && (
          <CForm>
            <div className="mb-3">
              <CFormLabel htmlFor="moduleContext">Change Context (Optional)</CFormLabel>
              <CFormInput
                id="moduleContext"
                placeholder="Reason for configuration change..."
                value={context}
                onChange={(e) => setContext(e.target.value)}
              />
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="moduleSettings">Settings JSON</CFormLabel>
              <CFormTextarea
                id="moduleSettings"
                rows={10}
                placeholder="Enter module settings as JSON..."
                value={settings}
                onChange={(e) => setSettings(e.target.value)}
              />
              <small className="text-muted">
                Enter configuration settings in JSON format. Leave empty to clear settings.
              </small>
            </div>
          </CForm>
        )}
      </CModalBody>
      <CModalFooter>
        <CButton color="secondary" onClick={onClose}>
          Cancel
        </CButton>
        <CButton 
          color="primary" 
          onClick={handleSave}
          disabled={isLoading}
        >
          {isLoading && <CSpinner size="sm" className="me-1" />}
          Save Settings
        </CButton>
      </CModalFooter>
    </CModal>
  );
};

interface ModuleConfigurationListProps {
  showDashboardLink?: boolean;
}

export const ModuleConfigurationList: React.FC<ModuleConfigurationListProps> = ({
  showDashboardLink = true
}) => {
  const [filterEnabled, setFilterEnabled] = useState<boolean | undefined>(undefined);
  const [selectedModule, setSelectedModule] = useState<ModuleConfigurationDto | null>(null);
  const [settingsModalOpen, setSettingsModalOpen] = useState(false);

  const {
    data: modules = [],
    isLoading: modulesLoading,
    error: modulesError,
    refetch
  } = useGetModuleConfigurationsQuery({
    isEnabled: filterEnabled,
    includeDependencies: true,
    includeSubModules: true,
    includeAuditProperties: false
  });

  const [enableModule, { isLoading: enableLoading }] = useEnableModuleMutation();
  const [disableModule, { isLoading: disableLoading }] = useDisableModuleMutation();
  const [updateSettings, { isLoading: settingsLoading }] = useUpdateModuleSettingsMutation();

  const handleModuleToggle = async (moduleType: ModuleType, enabled: boolean) => {
    try {
      if (enabled) {
        await enableModule({ 
          moduleType, 
          request: { context: 'Enabled via Module Configuration List' } 
        }).unwrap();
      } else {
        await disableModule({ 
          moduleType, 
          request: { context: 'Disabled via Module Configuration List' } 
        }).unwrap();
      }
      refetch();
    } catch (error) {
      console.error('Failed to toggle module:', error);
    }
  };

  const handleSettingsUpdate = async (moduleType: ModuleType, settings: string, context?: string) => {
    try {
      await updateSettings({
        moduleType,
        request: { settings: settings || undefined, context }
      }).unwrap();
      setSettingsModalOpen(false);
      setSelectedModule(null);
      refetch();
    } catch (error) {
      console.error('Failed to update module settings:', error);
    }
  };

  const openSettingsModal = (module: ModuleConfigurationDto) => {
    setSelectedModule(module);
    setSettingsModalOpen(true);
  };

  const getStatusBadge = (module: ModuleConfigurationDto) => {
    if (module.isEnabled) {
      return <CBadge color="success">Enabled</CBadge>;
    } else {
      return <CBadge color="secondary">Disabled</CBadge>;
    }
  };

  const getDependenciesInfo = (module: ModuleConfigurationDto) => {
    const depCount = module.dependencies?.length || 0;
    const subModuleCount = module.subModules?.length || 0;
    
    if (depCount === 0 && subModuleCount === 0) {
      return <span className="text-muted">No dependencies</span>;
    }
    
    return (
      <div className="small">
        {depCount > 0 && <div>{depCount} dependencies</div>}
        {subModuleCount > 0 && <div>{subModuleCount} sub-modules</div>}
      </div>
    );
  };

  if (modulesLoading) {
    return (
      <CCard>
        <CCardBody className="text-center">
          <CSpinner />
          <div className="mt-2">Loading module configurations...</div>
        </CCardBody>
      </CCard>
    );
  }

  if (modulesError) {
    return (
      <CCard>
        <CCardBody>
          <CAlert color="danger">
            Failed to load module configurations. Please try again.
          </CAlert>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <>
      <CCard>
        <CCardHeader>
          <CRow className="align-items-center">
            <CCol>
              <h5 className="mb-0">
                <Icon icon={cilSettings} className="me-2" />
                Module Configuration
              </h5>
            </CCol>
            <CCol xs="auto">
              <div className="d-flex gap-2">
                <CButtonGroup>
                  <CButton
                    color="primary"
                    variant={filterEnabled === undefined ? undefined : 'outline'}
                    size="sm"
                    onClick={() => setFilterEnabled(undefined)}
                  >
                    All
                  </CButton>
                  <CButton
                    color="success"
                    variant={filterEnabled === true ? undefined : 'outline'}
                    size="sm"
                    onClick={() => setFilterEnabled(true)}
                  >
                    Enabled
                  </CButton>
                  <CButton
                    color="secondary"
                    variant={filterEnabled === false ? undefined : 'outline'}
                    size="sm"
                    onClick={() => setFilterEnabled(false)}
                  >
                    Disabled
                  </CButton>
                </CButtonGroup>
                
                {showDashboardLink && (
                  <CButton
                    color="info"
                    variant="outline"
                    size="sm"
                    href="#/settings/modules/dashboard"
                  >
                    <Icon icon={cilInfo} className="me-1" />
                    Dashboard
                  </CButton>
                )}
              </div>
            </CCol>
          </CRow>
        </CCardHeader>
        <CCardBody>
          <CTable responsive hover>
            <CTableHead>
              <CTableRow>
                <CTableHeaderCell>Module</CTableHeaderCell>
                <CTableHeaderCell>Status</CTableHeaderCell>
                <CTableHeaderCell>Dependencies</CTableHeaderCell>
                <CTableHeaderCell>Description</CTableHeaderCell>
                <CTableHeaderCell>Actions</CTableHeaderCell>
              </CTableRow>
            </CTableHead>
            <CTableBody>
              {modules.map((module) => (
                <CTableRow key={module.id}>
                  <CTableDataCell>
                    <div className="d-flex align-items-center">
                      <Icon icon={getModuleIcon(module.moduleType)} className="me-2" />
                      <div>
                        <div className="fw-semibold">{module.displayName}</div>
                        <small className="text-muted">Order: {module.displayOrder}</small>
                      </div>
                    </div>
                  </CTableDataCell>
                  <CTableDataCell>
                    <ModuleToggle
                      module={module}
                      onToggle={handleModuleToggle}
                      isLoading={enableLoading || disableLoading}
                    />
                  </CTableDataCell>
                  <CTableDataCell>
                    {getDependenciesInfo(module)}
                  </CTableDataCell>
                  <CTableDataCell>
                    <div className="small text-muted">
                      {module.description || 'No description available'}
                    </div>
                  </CTableDataCell>
                  <CTableDataCell>
                    <CDropdown>
                      <CDropdownToggle variant="outline" size="sm">
                        Actions
                      </CDropdownToggle>
                      <CDropdownMenu>
                        <CDropdownItem onClick={() => openSettingsModal(module)}>
                          <Icon icon={cilPencil} className="me-2" />
                          Configure Settings
                        </CDropdownItem>
                        <CDropdownItem href={`#/settings/modules/${module.moduleType}/audit`}>
                          <Icon icon={cilHistory} className="me-2" />
                          View Audit Trail
                        </CDropdownItem>
                        <CDropdownItem href={`#/settings/modules/${module.moduleType}/dependencies`}>
                          <Icon icon={cilInfo} className="me-2" />
                          View Dependencies
                        </CDropdownItem>
                      </CDropdownMenu>
                    </CDropdown>
                  </CTableDataCell>
                </CTableRow>
              ))}
            </CTableBody>
          </CTable>

          {modules.length === 0 && (
            <div className="text-center py-4">
              <Icon icon={cilInfo} size="lg" className="text-muted mb-2" />
              <div className="text-muted">No modules found matching the current filter.</div>
            </div>
          )}
        </CCardBody>
      </CCard>

      <ModuleSettingsModal
        module={selectedModule}
        isOpen={settingsModalOpen}
        onClose={() => {
          setSettingsModalOpen(false);
          setSelectedModule(null);
        }}
        onSave={handleSettingsUpdate}
        isLoading={settingsLoading}
      />
    </>
  );
};