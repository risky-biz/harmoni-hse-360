import { http, HttpResponse } from 'msw';

export const healthHandlers = [
  // Get health dashboard
  http.get('/api/health/dashboard', () => {
    return HttpResponse.json({
      totalHealthRecords: 150,
      totalStudentRecords: 120,
      totalStaffRecords: 30,
      activeHealthRecords: 145,
      totalMedicalConditions: 45,
      criticalMedicalConditions: 8,
      lifeThreateningConditions: 2,
      vaccinationComplianceRate: 92.5,
      expiringVaccinations: 12,
      expiredVaccinations: 3,
      overdueVaccinations: 5,
      totalHealthIncidents: 8,
      criticalHealthIncidents: 2,
      unresolvedHealthIncidents: 3,
      recentHealthIncidents: 5,
      totalEmergencyContacts: 280,
      emergencyContactCompleteness: 95.2,
      primaryContactsMissing: 7,
      recentHealthRecords: [],
      recentHealthIncidentDetails: [],
      expiringVaccinationDetails: [],
      conditionsByCategory: [
        { category: 'Allergies', count: 15, criticalCount: 3, percentage: 33.3 },
        { category: 'Chronic Conditions', count: 12, criticalCount: 2, percentage: 26.7 },
        { category: 'Mental Health', count: 8, criticalCount: 1, percentage: 17.8 }
      ],
      vaccinationsByStatus: [
        { status: 'Compliant', count: 139, percentage: 92.5 },
        { status: 'Overdue', count: 8, percentage: 5.3 },
        { status: 'Exempted', count: 3, percentage: 2.0 }
      ],
      healthIncidentTrends: [
        { date: '2024-01-01', count: 2, criticalCount: 0 },
        { date: '2024-02-01', count: 3, criticalCount: 1 },
        { date: '2024-03-01', count: 1, criticalCount: 0 }
      ],
      fromDate: '2024-01-01',
      toDate: '2024-12-31'
    });
  }),

  // Get health records
  http.get('/api/health/records', ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
    
    return HttpResponse.json({
      items: [
        {
          id: 1,
          personId: 1001,
          personName: 'John Smith',
          personType: 'Student',
          personEmail: 'john.smith@test.com',
          personDepartment: 'Grade 10',
          dateOfBirth: '2008-05-15',
          bloodType: 'O+',
          isActive: true,
          medicalConditionsCount: 2,
          vaccinationsCount: 8,
          healthIncidentsCount: 1,
          emergencyContactsCount: 2,
          hasCriticalConditions: false,
          expiringVaccinationsCount: 1,
          criticalAllergyAlerts: ['Nuts', 'Shellfish'],
          createdAt: '2024-01-15T10:00:00Z',
          lastModifiedAt: '2024-03-10T14:30:00Z'
        },
        {
          id: 2,
          personId: 2001,
          personName: 'Sarah Johnson',
          personType: 'Staff',
          personEmail: 'sarah.johnson@test.com',
          personDepartment: 'Mathematics',
          dateOfBirth: '1985-08-22',
          bloodType: 'A+',
          isActive: true,
          medicalConditionsCount: 1,
          vaccinationsCount: 10,
          healthIncidentsCount: 0,
          emergencyContactsCount: 1,
          hasCriticalConditions: true,
          expiringVaccinationsCount: 0,
          criticalAllergyAlerts: [],
          createdAt: '2024-02-01T09:00:00Z',
          lastModifiedAt: '2024-03-15T16:45:00Z'
        }
      ],
      totalCount: 150,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(150 / pageSize),
      hasNextPage: page < Math.ceil(150 / pageSize),
      hasPreviousPage: page > 1
    });
  }),

  // Get health record by ID
  http.get('/api/health/records/:id', ({ params }) => {
    const { id } = params;
    
    return HttpResponse.json({
      id: parseInt(id as string),
      personId: 1001,
      personName: 'John Smith',
      personType: 'Student',
      personEmail: 'john.smith@test.com',
      personDepartment: 'Grade 10',
      personPosition: 'Student',
      dateOfBirth: '2008-05-15',
      bloodType: 'O+',
      medicalNotes: 'Allergic to nuts and shellfish. Requires EpiPen nearby.',
      isActive: true,
      medicalConditions: [
        {
          id: 1,
          name: 'Nut Allergy',
          type: 'Allergy',
          severity: 'Critical',
          description: 'Severe allergic reaction to tree nuts and peanuts',
          diagnosedDate: '2010-03-15',
          treatmentPlan: 'Avoid all nuts, carry EpiPen',
          emergencyInstructions: 'Administer EpiPen immediately if exposed, call 911',
          requiresEmergencyAction: true,
          medicationRequired: 'EpiPen',
          emergencyProtocol: 'Immediate medical attention required',
          isActive: true
        }
      ],
      vaccinations: [
        {
          id: 1,
          vaccineName: 'COVID-19',
          administeredDate: '2023-09-15',
          expiryDate: '2024-09-15',
          administeredBy: 'Dr. Smith',
          batchNumber: 'COV123',
          status: 'Valid',
          isMandatory: true,
          notes: 'Booster shot administered'
        }
      ],
      healthIncidents: [],
      emergencyContacts: [
        {
          id: 1,
          name: 'Mary Smith',
          relationship: 'Mother',
          primaryPhone: '+1-555-0123',
          secondaryPhone: '+1-555-0124',
          email: 'mary.smith@email.com',
          address: '123 Main St, City, State 12345',
          isPrimary: true,
          isAuthorizedForPickup: true,
          isAuthorizedForMedicalDecisions: true,
          priority: 1,
          notes: 'Primary emergency contact'
        }
      ],
      medicalConditionsCount: 1,
      vaccinationsCount: 1,
      healthIncidentsCount: 0,
      emergencyContactsCount: 1,
      hasCriticalConditions: true,
      expiringVaccinationsCount: 1,
      criticalAllergyAlerts: ['Nuts', 'Shellfish'],
      createdAt: '2024-01-15T10:00:00Z',
      lastModifiedAt: '2024-03-10T14:30:00Z',
      createdBy: 'System',
      lastModifiedBy: 'nurse@test.com'
    });
  }),

  // Create health record
  http.post('/api/health/records', async ({ request }) => {
    const body = await request.json() as any;
    
    return HttpResponse.json({
      id: 999,
      personId: body.personId,
      personName: 'New Person',
      personType: body.personType,
      personEmail: 'new.person@test.com',
      dateOfBirth: body.dateOfBirth,
      bloodType: body.bloodType,
      medicalNotes: body.medicalNotes,
      isActive: true,
      medicalConditionsCount: 0,
      vaccinationsCount: 0,
      healthIncidentsCount: 0,
      emergencyContactsCount: 0,
      hasCriticalConditions: false,
      expiringVaccinationsCount: 0,
      criticalAllergyAlerts: [],
      createdAt: new Date().toISOString(),
      lastModifiedAt: new Date().toISOString()
    }, { status: 201 });
  }),

  // Update health record
  http.put('/api/health/records/:id', async ({ params, request }) => {
    const { id } = params;
    const body = await request.json() as any;
    
    return HttpResponse.json({
      id: parseInt(id as string),
      personId: 1001,
      personName: 'John Smith',
      personType: 'Student',
      personEmail: 'john.smith@test.com',
      dateOfBirth: body.dateOfBirth,
      bloodType: body.bloodType,
      medicalNotes: body.medicalNotes,
      isActive: true,
      lastModifiedAt: new Date().toISOString()
    });
  })
];