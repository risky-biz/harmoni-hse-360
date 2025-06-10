import { configureStore } from '@reduxjs/toolkit';
import { setupListeners } from '@reduxjs/toolkit/query';
import authSlice from '../features/auth/authSlice';
import { authApi } from '../features/auth/authApi';
import { incidentApi } from '../features/incidents/incidentApi';
import { ppeApi } from '../features/ppe/ppeApi';
import { ppeManagementApi } from '../features/ppe/ppeManagementApi';
import { hazardApi } from '../features/hazards/hazardApi';
import { healthApi } from '../features/health/healthApi';
import { configurationApi } from '../api/configurationApi';
import { applicationModeApi } from '../api/applicationModeApi';

export const store = configureStore({
  reducer: {
    auth: authSlice,
    [authApi.reducerPath]: authApi.reducer,
    [incidentApi.reducerPath]: incidentApi.reducer,
    [ppeApi.reducerPath]: ppeApi.reducer,
    [ppeManagementApi.reducerPath]: ppeManagementApi.reducer,
    [hazardApi.reducerPath]: hazardApi.reducer,
    [healthApi.reducerPath]: healthApi.reducer,
    [configurationApi.reducerPath]: configurationApi.reducer,
    [applicationModeApi.reducerPath]: applicationModeApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: ['persist/PERSIST'],
      },
    }).concat(
      authApi.middleware, 
      incidentApi.middleware, 
      ppeApi.middleware,
      ppeManagementApi.middleware,
      hazardApi.middleware,
      healthApi.middleware,
      configurationApi.middleware,
      applicationModeApi.middleware
    ),
});

setupListeners(store.dispatch);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
