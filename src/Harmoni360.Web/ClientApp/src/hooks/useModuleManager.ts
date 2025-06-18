import { useModuleState } from '../contexts/ModuleStateContext';
import { ModuleType } from '../types/permissions';

export const useModuleManager = () => {
  const { hideModule, showModule, toggleModule, setModuleStatus } = useModuleState();

  return {
    hideModule: (moduleType: ModuleType) => hideModule(moduleType),
    showModule: (moduleType: ModuleType) => showModule(moduleType),
    toggleModule: (moduleType: ModuleType) => toggleModule(moduleType),
    setModuleStatus: (moduleType: ModuleType, status: 'disabled' | 'maintenance' | 'coming-soon' | null) => 
      setModuleStatus(moduleType, status)
  };
};