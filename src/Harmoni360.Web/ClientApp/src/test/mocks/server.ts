import { setupServer } from 'msw/node';
import { authHandlers } from './handlers/auth';
import { healthHandlers } from './handlers/health';
import { incidentHandlers } from './handlers/incidents';
import { ppeHandlers } from './handlers/ppe';
import { hazardHandlers } from './handlers/hazards';
import { inspectionHandlers } from './handlers/inspections';
import { trainingHandlers } from './handlers/trainings';

// This configures a request mocking server with the given request handlers
export const server = setupServer(
  ...authHandlers,
  ...healthHandlers,
  ...incidentHandlers,
  ...ppeHandlers,
  ...hazardHandlers,
  ...inspectionHandlers,
  ...trainingHandlers
);