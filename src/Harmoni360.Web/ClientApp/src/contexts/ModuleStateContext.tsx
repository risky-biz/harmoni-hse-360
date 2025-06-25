import React, { createContext, useContext, useReducer, ReactNode } from 'react';
import { ModuleType } from '../types/permissions';

export interface ModuleState {
  isVisible: boolean;
  status: 'disabled' | 'maintenance' | 'coming-soon' | null;
}

export type ModuleStateMap = {
  [key in ModuleType]: ModuleState;
};

interface ModuleStateContextType {
  moduleStates: ModuleStateMap;
  hideModule: (moduleType: ModuleType) => void;
  showModule: (moduleType: ModuleType) => void;
  toggleModule: (moduleType: ModuleType) => void;
  setModuleStatus: (moduleType: ModuleType, status: 'disabled' | 'maintenance' | 'coming-soon' | null) => void;
}

type ModuleStateAction =
  | { type: 'HIDE_MODULE'; payload: ModuleType }
  | { type: 'SHOW_MODULE'; payload: ModuleType }
  | { type: 'TOGGLE_MODULE'; payload: ModuleType }
  | { type: 'SET_MODULE_STATUS'; payload: { moduleType: ModuleType; status: 'disabled' | 'maintenance' | 'coming-soon' | null } };

const initialState: ModuleStateMap = Object.values(ModuleType).reduce((acc, moduleType) => {
  acc[moduleType] = {
    isVisible: true,
    status: null
  };
  return acc;
}, {} as ModuleStateMap);

const moduleStateReducer = (state: ModuleStateMap, action: ModuleStateAction): ModuleStateMap => {
  switch (action.type) {
    case 'HIDE_MODULE':
      return {
        ...state,
        [action.payload]: {
          ...state[action.payload],
          isVisible: false
        }
      };
    case 'SHOW_MODULE':
      return {
        ...state,
        [action.payload]: {
          ...state[action.payload],
          isVisible: true
        }
      };
    case 'TOGGLE_MODULE':
      return {
        ...state,
        [action.payload]: {
          ...state[action.payload],
          isVisible: !state[action.payload].isVisible
        }
      };
    case 'SET_MODULE_STATUS':
      return {
        ...state,
        [action.payload.moduleType]: {
          ...state[action.payload.moduleType],
          status: action.payload.status
        }
      };
    default:
      return state;
  }
};

const ModuleStateContext = createContext<ModuleStateContextType | undefined>(undefined);

export const useModuleState = () => {
  const context = useContext(ModuleStateContext);
  if (context === undefined) {
    throw new Error('useModuleState must be used within a ModuleStateProvider');
  }
  return context;
};

export const useModuleManager = () => {
  const { hideModule, showModule, toggleModule, setModuleStatus } = useModuleState();
  
  return {
    hideModule,
    showModule,
    toggleModule,
    setModuleStatus
  };
};

interface ModuleStateProviderProps {
  children: ReactNode;
}

export const ModuleStateProvider: React.FC<ModuleStateProviderProps> = ({ children }) => {
  const [moduleStates, dispatch] = useReducer(moduleStateReducer, initialState);

  const hideModule = (moduleType: ModuleType) => {
    dispatch({ type: 'HIDE_MODULE', payload: moduleType });
  };

  const showModule = (moduleType: ModuleType) => {
    dispatch({ type: 'SHOW_MODULE', payload: moduleType });
  };

  const toggleModule = (moduleType: ModuleType) => {
    dispatch({ type: 'TOGGLE_MODULE', payload: moduleType });
  };

  const setModuleStatus = (moduleType: ModuleType, status: 'disabled' | 'maintenance' | 'coming-soon' | null) => {
    dispatch({ type: 'SET_MODULE_STATUS', payload: { moduleType, status } });
  };

  const value: ModuleStateContextType = {
    moduleStates,
    hideModule,
    showModule,
    toggleModule,
    setModuleStatus
  };

  return (
    <ModuleStateContext.Provider value={value}>
      {children}
    </ModuleStateContext.Provider>
  );
};