import React from 'react';
import { useParams } from 'react-router-dom';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import WasteReportDetailComponent from '../../components/waste/WasteReportDetail';
import { ModuleType, PermissionType } from '../../types/permissions';

const WasteReportDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  if (!id) {
    return <div>Invalid waste report ID</div>;
  }

  return (
    <PermissionGuard 
      module={ModuleType.WasteManagement} 
      permission={PermissionType.Read}
      fallback={<div>You do not have permission to view waste reports.</div>}
    >
      <WasteReportDetailComponent reportId={parseInt(id, 10)} />
    </PermissionGuard>
  );
};

export default WasteReportDetail;