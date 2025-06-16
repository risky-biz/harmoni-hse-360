import { configureStore } from '@reduxjs/toolkit';
import { setupListeners } from '@reduxjs/toolkit/query';
import authSlice from '../features/auth/authSlice';
import { authApi } from '../features/auth/authApi';
import { incidentApi } from '../features/incidents/incidentApi';
import { ppeApi } from '../features/ppe/ppeApi';
import { ppeManagementApi } from '../features/ppe/ppeManagementApi';
import { hazardApi } from '../features/hazards/hazardApi';
import { healthApi } from '../features/health/healthApi';
import { riskAssessmentApi } from '../features/risk-assessments/riskAssessmentApi';
import { workPermitApi } from '../features/work-permits/workPermitApi';
import { auditApi } from '../features/audits/auditApi';
import { inspectionApi } from '../features/inspections/inspectionApi';
import { licenseApi } from '../features/licenses/licenseApi';
import { securityApi } from '../features/security/securityApi';
import { wasteApi } from '../features/waste-management/wasteApi';
import { wasteManagementApi } from '../api/wasteManagementApi';
import { disposalProvidersApi } from '../api/disposalProvidersApi';
import { trainingApi } from '../features/trainings/trainingApi';
import { statisticsApi } from '../features/statistics/statisticsApi';
import { configurationApi } from '../api/configurationApi';
import { companyConfigurationApi } from '../services/companyConfigurationService';
import { hazardConfigurationApi } from '../api/hazardConfigurationApi';
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
    [riskAssessmentApi.reducerPath]: riskAssessmentApi.reducer,
    [workPermitApi.reducerPath]: workPermitApi.reducer,
    [inspectionApi.reducerPath]: inspectionApi.reducer,
    [licenseApi.reducerPath]: licenseApi.reducer,
    [auditApi.reducerPath]: auditApi.reducer,
    [securityApi.reducerPath]: securityApi.reducer,
    [wasteApi.reducerPath]: wasteApi.reducer,
    [wasteManagementApi.reducerPath]: wasteManagementApi.reducer,
    [disposalProvidersApi.reducerPath]: disposalProvidersApi.reducer,
    [trainingApi.reducerPath]: trainingApi.reducer,
    [statisticsApi.reducerPath]: statisticsApi.reducer,
    [configurationApi.reducerPath]: configurationApi.reducer,
    [companyConfigurationApi.reducerPath]: companyConfigurationApi.reducer,
    [hazardConfigurationApi.reducerPath]: hazardConfigurationApi.reducer,
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
      riskAssessmentApi.middleware,
      workPermitApi.middleware,
      inspectionApi.middleware,
      licenseApi.middleware,
      auditApi.middleware,
      securityApi.middleware,
      wasteApi.middleware,
      wasteManagementApi.middleware,
      disposalProvidersApi.middleware,
      trainingApi.middleware,
      statisticsApi.middleware,
      configurationApi.middleware,
      companyConfigurationApi.middleware,
      hazardConfigurationApi.middleware,
      applicationModeApi.middleware
    ),
});

setupListeners(store.dispatch);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
