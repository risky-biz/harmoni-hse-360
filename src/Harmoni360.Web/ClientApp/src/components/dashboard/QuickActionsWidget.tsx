import React from 'react';
import { useNavigate } from 'react-router-dom';
import { CCard, CCardBody, CCardHeader, CButton } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { getQuickActions } from '../../config/dashboardLayouts';
import { usePermissions } from '../../hooks/usePermissions';
import { ACTION_ICONS } from '../../utils/iconMappings';

interface QuickActionsWidgetProps {
  title?: string;
  actions?: string[]; // Array of action IDs to include
  maxActions?: number;
  layout?: 'grid' | 'list';
}

const QuickActionsWidget: React.FC<QuickActionsWidgetProps> = ({
  title = 'Quick Actions',
  actions,
  maxActions = 6,
  layout = 'list',
}) => {
  const navigate = useNavigate();
  const permissions = usePermissions();

  const allActions = getQuickActions(navigate);
  
  // Filter actions based on permissions and selection
  const availableActions = allActions.filter(action => {
    // Check if action should be included
    if (actions && !actions.includes(action.id)) {
      return false;
    }
    
    // Check permissions
    if (action.permissions && !action.permissions.every(perm => permissions.permissions.includes(perm))) {
      return false;
    }
    
    // Check visibility
    return action.isVisible !== false;
  }).slice(0, maxActions);

  const renderGridLayout = () => (
    <div className="row g-2">
      {availableActions.map((action) => (
        <div key={action.id} className="col-6">
          <CButton
            color={action.color}
            variant={action.variant === 'solid' ? undefined : action.variant}
            className="w-100 text-start d-flex align-items-center justify-content-between p-3"
            onClick={action.action}
          >
            <div className="d-flex align-items-center">
              <FontAwesomeIcon icon={action.icon} className="me-2" />
              <span className="small">{action.label}</span>
            </div>
            {action.badge && (
              <span className={`badge bg-${action.badge.color} ms-2`}>
                {action.badge.text}
              </span>
            )}
          </CButton>
        </div>
      ))}
    </div>
  );

  const renderListLayout = () => (
    <div className="d-grid gap-2">
      {availableActions.map((action) => (
        <CButton
          key={action.id}
          color={action.color}
          variant={action.variant === 'solid' ? undefined : action.variant}
          className="d-flex align-items-center justify-content-between p-3"
          onClick={action.action}
        >
          <div className="d-flex align-items-center">
            <FontAwesomeIcon icon={action.icon} className="me-2" />
            <span>{action.label}</span>
          </div>
          <div className="d-flex align-items-center">
            {action.badge && (
              <span className={`badge bg-${action.badge.color} me-2`}>
                {action.badge.text}
              </span>
            )}
            <FontAwesomeIcon icon={ACTION_ICONS.next} size="sm" />
          </div>
        </CButton>
      ))}
    </div>
  );

  if (availableActions.length === 0) {
    return (
      <CCard className="h-100">
        <CCardHeader>
          <h5 className="mb-0">{title}</h5>
        </CCardHeader>
        <CCardBody className="d-flex align-items-center justify-content-center">
          <div className="text-center text-muted">
            <p>No actions available</p>
          </div>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <CCard className="h-100">
      <CCardHeader>
        <h5 className="mb-0">{title}</h5>
      </CCardHeader>
      <CCardBody>
        {layout === 'grid' ? renderGridLayout() : renderListLayout()}
      </CCardBody>
    </CCard>
  );
};

export default QuickActionsWidget;