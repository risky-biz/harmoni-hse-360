import React, { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';
import { configureStore } from '@reduxjs/toolkit';
import authSlice from '../../features/auth/authSlice';
import { authApi } from '../../features/auth/authApi';
import { healthApi } from '../../features/health/healthApi';
import { incidentApi } from '../../features/incidents/incidentApi';
import { ppeApi } from '../../features/ppe/ppeApi';
import { hazardApi } from '../../features/hazards/hazardApi';

// Create a test store with all the APIs
const createTestStore = (preloadedState = {}) =>
  configureStore({
    reducer: {
      auth: authSlice,
      [authApi.reducerPath]: authApi.reducer,
      [healthApi.reducerPath]: healthApi.reducer,
      [incidentApi.reducerPath]: incidentApi.reducer,
      [ppeApi.reducerPath]: ppeApi.reducer,
      [hazardApi.reducerPath]: hazardApi.reducer,
    },
    middleware: (getDefaultMiddleware) =>
      getDefaultMiddleware({
        serializableCheck: {
          ignoredActions: ['persist/PERSIST'],
        },
      }).concat(
        authApi.middleware,
        healthApi.middleware,
        incidentApi.middleware,
        ppeApi.middleware,
        hazardApi.middleware
      ),
    preloadedState,
  });

interface AllTheProvidersProps {
  children: React.ReactNode;
  initialState?: any;
}

const AllTheProviders: React.FC<AllTheProvidersProps> = ({ 
  children, 
  initialState = {} 
}) => {
  const store = createTestStore(initialState);
  
  return (
    <Provider store={store}>
      <BrowserRouter>
        {children}
      </BrowserRouter>
    </Provider>
  );
};

const customRender = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'> & { initialState?: any }
) => {
  const { initialState, ...renderOptions } = options || {};
  
  return render(ui, {
    wrapper: (props) => <AllTheProviders {...props} initialState={initialState} />,
    ...renderOptions,
  });
};

// Mock authenticated user state
export const createAuthenticatedState = (userOverrides = {}) => ({
  auth: {
    token: 'mock-jwt-token',
    refreshToken: 'mock-refresh-token',
    user: {
      id: '1',
      email: 'admin@test.com',
      name: 'Test Admin',
      roles: ['Administrator'],
      department: 'Administration',
      ...userOverrides,
    },
    isAuthenticated: true,
    isLoading: false,
    error: null,
  },
});

// Re-export everything
export * from '@testing-library/react';
export { customRender as render };