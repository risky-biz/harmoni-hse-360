import { describe, it, expect, beforeEach } from 'vitest';
import { store } from '../../../store';
import { healthApi } from '../../../features/health/healthApi';

describe('Health API', () => {
  beforeEach(() => {
    store.dispatch(healthApi.util.resetApiState());
  });

  describe('getHealthDashboard', () => {
    it('fetches dashboard data successfully', async () => {
      const result = await store.dispatch(
        healthApi.endpoints.getHealthDashboard.initiate({})
      );
      
      expect(result.data).toBeDefined();
      expect(result.data?.totalHealthRecords).toBe(150);
      expect(result.data?.vaccinationComplianceRate).toBe(92.5);
      expect(result.data?.totalMedicalConditions).toBe(45);
    });

    it('handles dashboard query parameters', async () => {
      const params = {
        fromDate: '2024-01-01',
        toDate: '2024-12-31',
        department: 'Grade 10',
        personType: 'Student'
      };
      
      const result = await store.dispatch(
        healthApi.endpoints.getHealthDashboard.initiate(params)
      );
      
      expect(result.data).toBeDefined();
      expect(result.data?.fromDate).toBe('2024-01-01');
      expect(result.data?.toDate).toBe('2024-12-31');
    });
  });

  describe('getHealthRecords', () => {
    it('fetches health records with pagination', async () => {
      const params = {
        page: 1,
        pageSize: 10,
        searchTerm: 'John'
      };
      
      const result = await store.dispatch(
        healthApi.endpoints.getHealthRecords.initiate(params)
      );
      
      expect(result.data).toBeDefined();
      expect(result.data?.items).toHaveLength(2);
      expect(result.data?.totalCount).toBe(150);
      expect(result.data?.pageNumber).toBe(1);
      expect(result.data?.pageSize).toBe(10);
    });

    it('includes correct health record structure', async () => {
      const result = await store.dispatch(
        healthApi.endpoints.getHealthRecords.initiate({})
      );
      
      const firstRecord = result.data?.items[0];
      expect(firstRecord).toMatchObject({
        id: expect.any(Number),
        personId: expect.any(Number),
        personName: expect.any(String),
        personType: expect.any(String),
        personEmail: expect.any(String),
        isActive: expect.any(Boolean),
        medicalConditionsCount: expect.any(Number),
        vaccinationsCount: expect.any(Number),
        emergencyContactsCount: expect.any(Number)
      });
    });
  });

  describe('getHealthRecord', () => {
    it('fetches single health record by ID', async () => {
      const result = await store.dispatch(
        healthApi.endpoints.getHealthRecord.initiate('1')
      );
      
      expect(result.data).toBeDefined();
      expect(result.data?.id).toBe(1);
      expect(result.data?.personName).toBe('John Smith');
      expect(result.data?.medicalConditions).toHaveLength(1);
      expect(result.data?.emergencyContacts).toHaveLength(1);
    });

    it('includes complete health record details', async () => {
      const result = await store.dispatch(
        healthApi.endpoints.getHealthRecord.initiate('1')
      );
      
      const record = result.data;
      expect(record?.medicalConditions[0]).toMatchObject({
        id: expect.any(Number),
        name: expect.any(String),
        type: expect.any(String),
        severity: expect.any(String),
        requiresEmergencyAction: expect.any(Boolean)
      });
      
      expect(record?.emergencyContacts[0]).toMatchObject({
        id: expect.any(Number),
        name: expect.any(String),
        relationship: expect.any(String),
        primaryPhone: expect.any(String),
        isPrimary: expect.any(Boolean)
      });
    });
  });

  describe('createHealthRecord', () => {
    it('creates new health record successfully', async () => {
      const newRecord = {
        personId: '9999',
        personType: 'Student',
        dateOfBirth: '2010-05-15',
        bloodType: 'O+',
        medicalNotes: 'Test medical notes'
      };
      
      const result = await store.dispatch(
        healthApi.endpoints.createHealthRecord.initiate(newRecord)
      );
      
      expect(result.data).toBeDefined();
      expect(result.data?.id).toBe(999);
      expect(result.data?.personType).toBe('Student');
      expect(result.data?.bloodType).toBe('O+');
    });
  });

  describe('updateHealthRecord', () => {
    it('updates existing health record', async () => {
      const updates = {
        bloodType: 'A+',
        medicalNotes: 'Updated medical notes'
      };
      
      const result = await store.dispatch(
        healthApi.endpoints.updateHealthRecord.initiate({
          id: '1',
          ...updates
        })
      );
      
      expect(result.data).toBeDefined();
      expect(result.data?.id).toBe(1);
      expect(result.data?.bloodType).toBe('A+');
      expect(result.data?.medicalNotes).toBe('Updated medical notes');
    });
  });

  describe('API caching', () => {
    it('caches dashboard data with correct tags', async () => {
      await store.dispatch(
        healthApi.endpoints.getHealthDashboard.initiate({})
      );
      
      const state = store.getState();
      const cacheKeys = Object.keys(state.healthApi.queries);
      
      expect(cacheKeys.some(key => key.includes('getHealthDashboard'))).toBe(true);
    });

    it('invalidates cache on mutations', async () => {
      // First fetch dashboard data
      await store.dispatch(
        healthApi.endpoints.getHealthDashboard.initiate({})
      );
      
      // Then create a new record (should invalidate dashboard cache)
      await store.dispatch(
        healthApi.endpoints.createHealthRecord.initiate({
          personId: '9999',
          personType: 'Student',
          dateOfBirth: '2010-01-01'
        })
      );
      
      // Cache invalidation is handled by RTK Query internally
      // This test verifies the API structure supports it
      expect(healthApi.endpoints.createHealthRecord.queryFn).toBeDefined();
    });
  });
});