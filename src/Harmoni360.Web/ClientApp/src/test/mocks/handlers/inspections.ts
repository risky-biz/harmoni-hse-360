import { rest } from 'msw';
import { InspectionStatus, InspectionType, InspectionCategory, InspectionPriority, FindingSeverity, FindingType } from '../../../types/inspection';

// Mock data store
let mockInspections: any[] = [];
let mockInspectionItems: any[] = [];
let mockInspectionFindings: any[] = [];

// Initialize with some default data
const initializeMockData = () => {
  mockInspections = [
    {
      id: 1,
      inspectionNumber: 'INS-2024-001',
      title: 'Safety Equipment Inspection',
      description: 'Monthly safety equipment and emergency systems check',
      type: InspectionType.Safety,
      typeName: 'Safety',
      category: InspectionCategory.Routine,
      categoryName: 'Routine',
      status: InspectionStatus.Completed,
      statusName: 'Completed',
      priority: InspectionPriority.Medium,
      priorityName: 'Medium',
      scheduledDate: '2024-01-15T09:00:00Z',
      startedDate: '2024-01-15T09:05:00Z',
      completedDate: '2024-01-15T11:30:00Z',
      inspectorId: 1,
      inspectorName: 'John Doe',
      locationId: 1,
      departmentId: 1,
      departmentName: 'Operations',
      facilityId: 1,
      riskLevel: 'Medium',
      riskLevelName: 'Medium',
      summary: 'All safety equipment inspected and found to be in good working condition.',
      recommendations: 'Continue monthly inspections. Replace fire extinguisher in Building A.',
      estimatedDurationMinutes: 120,
      actualDurationMinutes: 145,
      itemsCount: 8,
      completedItemsCount: 8,
      findingsCount: 1,
      criticalFindingsCount: 0,
      attachmentsCount: 2,
      canEdit: false,
      canStart: false,
      canComplete: false,
      canCancel: false,
      isOverdue: false,
      createdAt: '2024-01-10T10:00:00Z',
      lastModifiedAt: '2024-01-15T11:30:00Z',
      createdBy: 'admin@harmoni360.com',
      lastModifiedBy: 'john.doe@harmoni360.com'
    },
    {
      id: 2,
      inspectionNumber: 'INS-2024-002',
      title: 'Environmental Compliance Audit',
      description: 'Quarterly environmental compliance and waste management audit',
      type: InspectionType.Environmental,
      typeName: 'Environmental',
      category: InspectionCategory.Audit,
      categoryName: 'Audit',
      status: InspectionStatus.InProgress,
      statusName: 'InProgress',
      priority: InspectionPriority.High,
      priorityName: 'High',
      scheduledDate: '2024-01-18T14:00:00Z',
      startedDate: '2024-01-18T14:00:00Z',
      inspectorId: 2,
      inspectorName: 'Jane Smith',
      locationId: 2,
      departmentId: 2,
      departmentName: 'Environmental',
      facilityId: 1,
      riskLevel: 'High',
      riskLevelName: 'High',
      estimatedDurationMinutes: 240,
      itemsCount: 12,
      completedItemsCount: 7,
      findingsCount: 0,
      criticalFindingsCount: 0,
      attachmentsCount: 0,
      canEdit: true,
      canStart: false,
      canComplete: true,
      canCancel: true,
      isOverdue: false,
      createdAt: '2024-01-12T09:00:00Z',
      lastModifiedAt: '2024-01-18T14:00:00Z',
      createdBy: 'admin@harmoni360.com',
      lastModifiedBy: 'jane.smith@harmoni360.com'
    },
    {
      id: 3,
      inspectionNumber: 'INS-2024-003',
      title: 'Machine Guarding Assessment',
      description: 'Safety assessment of machine guarding and protective equipment',
      type: InspectionType.Safety,
      typeName: 'Safety',
      category: InspectionCategory.Scheduled,
      categoryName: 'Scheduled',
      status: InspectionStatus.Scheduled,
      statusName: 'Scheduled',
      priority: InspectionPriority.Critical,
      priorityName: 'Critical',
      scheduledDate: '2024-01-25T08:00:00Z',
      inspectorId: 1,
      inspectorName: 'John Doe',
      locationId: 1,
      departmentId: 1,
      departmentName: 'Operations',
      facilityId: 1,
      riskLevel: 'Critical',
      riskLevelName: 'Critical',
      estimatedDurationMinutes: 180,
      itemsCount: 15,
      completedItemsCount: 0,
      findingsCount: 0,
      criticalFindingsCount: 0,
      attachmentsCount: 0,
      canEdit: true,
      canStart: true,
      canComplete: false,
      canCancel: true,
      isOverdue: false,
      createdAt: '2024-01-14T11:30:00Z',
      lastModifiedAt: '2024-01-14T11:30:00Z',
      createdBy: 'admin@harmoni360.com',
      lastModifiedBy: 'admin@harmoni360.com'
    }
  ];

  mockInspectionItems = [
    {
      id: 1,
      inspectionId: 1,
      description: 'Check fire extinguisher pressure and expiration dates',
      requiresPhoto: true,
      isRequired: true,
      isCompleted: true,
      response: 'Compliant',
      notes: 'All fire extinguishers checked and within expiration dates',
      photoAttachment: null,
      completedAt: '2024-01-15T09:30:00Z',
      completedBy: 'john.doe@harmoni360.com'
    },
    {
      id: 2,
      inspectionId: 1,
      description: 'Verify emergency exit routes are clear',
      requiresPhoto: false,
      isRequired: true,
      isCompleted: true,
      response: 'Compliant',
      notes: 'All exit routes clear and properly marked',
      completedAt: '2024-01-15T09:45:00Z',
      completedBy: 'john.doe@harmoni360.com'
    }
  ];

  mockInspectionFindings = [
    {
      id: 1,
      inspectionId: 1,
      findingNumber: 'F-001',
      description: 'Fire extinguisher in Building A lobby is overdue for replacement',
      type: FindingType.NonConformance,
      typeName: 'Non-Conformance',
      severity: FindingSeverity.Medium,
      severityName: 'Medium',
      riskLevel: 'Medium',
      riskLevelName: 'Medium',
      rootCause: 'Missed scheduled replacement due to inventory shortage',
      immediateAction: 'Temporary fire extinguisher installed from emergency stock',
      correctiveAction: 'Purchase and install new fire extinguisher, update replacement schedule',
      dueDate: '2024-01-30T00:00:00Z',
      responsiblePersonId: 3,
      responsiblePersonName: 'Bob Wilson',
      status: 'Open',
      statusName: 'Open',
      location: 'Building A - Main Lobby',
      equipment: 'Fire Extinguisher FE-A001',
      regulation: 'NFPA 10 Standard',
      isOverdue: false,
      canEdit: true,
      canClose: false,
      hasCorrectiveAction: true,
      createdAt: '2024-01-15T10:15:00Z',
      lastModifiedAt: '2024-01-15T10:15:00Z',
      createdBy: 'john.doe@harmoni360.com',
      lastModifiedBy: 'john.doe@harmoni360.com',
      attachments: []
    }
  ];
};

// Initialize mock data
initializeMockData();

export const inspectionHandlers = [
  // Get inspections with filtering and pagination
  rest.get('/api/inspections', (req, res, ctx) => {
    const url = new URL(req.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '25');
    const searchTerm = url.searchParams.get('searchTerm');
    const status = url.searchParams.get('status');
    const type = url.searchParams.get('type');
    const priority = url.searchParams.get('priority');
    const sortBy = url.searchParams.get('sortBy') || 'scheduledDate';
    const sortDescending = url.searchParams.get('sortDescending') === 'true';

    let filteredInspections = [...mockInspections];

    // Apply filters
    if (searchTerm) {
      filteredInspections = filteredInspections.filter(inspection =>
        inspection.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        inspection.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
        inspection.inspectionNumber.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    if (status) {
      filteredInspections = filteredInspections.filter(inspection => 
        inspection.statusName === status
      );
    }

    if (type) {
      filteredInspections = filteredInspections.filter(inspection => 
        inspection.typeName === type
      );
    }

    if (priority) {
      filteredInspections = filteredInspections.filter(inspection => 
        inspection.priorityName === priority
      );
    }

    // Apply sorting
    filteredInspections.sort((a, b) => {
      let aVal = a[sortBy];
      let bVal = b[sortBy];
      
      if (sortBy === 'scheduledDate' || sortBy === 'createdAt') {
        aVal = new Date(aVal).getTime();
        bVal = new Date(bVal).getTime();
      }

      if (sortDescending) {
        return bVal > aVal ? 1 : -1;
      }
      return aVal > bVal ? 1 : -1;
    });

    // Apply pagination
    const startIndex = (page - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedInspections = filteredInspections.slice(startIndex, endIndex);

    return res(
      ctx.status(200),
      ctx.json({
        items: paginatedInspections,
        totalCount: filteredInspections.length,
        totalPages: Math.ceil(filteredInspections.length / pageSize),
        page,
        pageSize
      })
    );
  }),

  // Get inspection by ID
  rest.get('/api/inspections/:id', (req, res, ctx) => {
    const { id } = req.params;
    const inspection = mockInspections.find(i => i.id === parseInt(id as string));
    
    if (!inspection) {
      return res(ctx.status(404), ctx.json({ message: 'Inspection not found' }));
    }

    // Include related data
    const items = mockInspectionItems.filter(item => item.inspectionId === inspection.id);
    const findings = mockInspectionFindings.filter(finding => finding.inspectionId === inspection.id);

    return res(
      ctx.status(200),
      ctx.json({
        ...inspection,
        items,
        findings
      })
    );
  }),

  // Create inspection
  rest.post('/api/inspections', async (req, res, ctx) => {
    const body = await req.json();
    
    const newInspection = {
      id: mockInspections.length + 1,
      inspectionNumber: `INS-2024-${String(mockInspections.length + 1).padStart(3, '0')}`,
      ...body,
      status: InspectionStatus.Draft,
      statusName: 'Draft',
      typeName: InspectionType[body.type],
      categoryName: InspectionCategory[body.category],
      priorityName: InspectionPriority[body.priority],
      itemsCount: body.items?.length || 0,
      completedItemsCount: 0,
      findingsCount: 0,
      criticalFindingsCount: 0,
      attachmentsCount: 0,
      canEdit: true,
      canStart: true,
      canComplete: false,
      canCancel: true,
      isOverdue: false,
      createdAt: new Date().toISOString(),
      lastModifiedAt: new Date().toISOString(),
      createdBy: 'test@harmoni360.com',
      lastModifiedBy: 'test@harmoni360.com'
    };

    mockInspections.push(newInspection);
    
    return res(ctx.status(201), ctx.json(newInspection));
  }),

  // Update inspection
  rest.put('/api/inspections/:id', async (req, res, ctx) => {
    const { id } = req.params;
    const body = await req.json();
    const index = mockInspections.findIndex(i => i.id === parseInt(id as string));
    
    if (index === -1) {
      return res(ctx.status(404), ctx.json({ message: 'Inspection not found' }));
    }

    mockInspections[index] = {
      ...mockInspections[index],
      ...body,
      lastModifiedAt: new Date().toISOString(),
      lastModifiedBy: 'test@harmoni360.com'
    };

    return res(ctx.status(200), ctx.json(mockInspections[index]));
  }),

  // Start inspection
  rest.post('/api/inspections/:id/start', (req, res, ctx) => {
    const { id } = req.params;
    const index = mockInspections.findIndex(i => i.id === parseInt(id as string));
    
    if (index === -1) {
      return res(ctx.status(404), ctx.json({ message: 'Inspection not found' }));
    }

    mockInspections[index] = {
      ...mockInspections[index],
      status: InspectionStatus.InProgress,
      statusName: 'InProgress',
      startedDate: new Date().toISOString(),
      canStart: false,
      canComplete: true,
      lastModifiedAt: new Date().toISOString()
    };

    return res(ctx.status(200), ctx.json(mockInspections[index]));
  }),

  // Complete inspection
  rest.post('/api/inspections/:id/complete', async (req, res, ctx) => {
    const { id } = req.params;
    const body = await req.json();
    const index = mockInspections.findIndex(i => i.id === parseInt(id as string));
    
    if (index === -1) {
      return res(ctx.status(404), ctx.json({ message: 'Inspection not found' }));
    }

    mockInspections[index] = {
      ...mockInspections[index],
      status: InspectionStatus.Completed,
      statusName: 'Completed',
      completedDate: new Date().toISOString(),
      summary: body.summary,
      recommendations: body.recommendations,
      actualDurationMinutes: body.actualDurationMinutes,
      canStart: false,
      canComplete: false,
      canEdit: false,
      lastModifiedAt: new Date().toISOString()
    };

    return res(ctx.status(200), ctx.json(mockInspections[index]));
  }),

  // Get dashboard data
  rest.get('/api/inspections/dashboard', (req, res, ctx) => {
    const totalInspections = mockInspections.length;
    const completedInspections = mockInspections.filter(i => i.status === InspectionStatus.Completed).length;
    const inProgressInspections = mockInspections.filter(i => i.status === InspectionStatus.InProgress).length;
    const overdueInspections = mockInspections.filter(i => i.isOverdue).length;
    const criticalFindings = mockInspectionFindings.filter(f => f.severity === FindingSeverity.Critical).length;

    const dashboardData = {
      totalInspections,
      scheduledInspections: mockInspections.filter(i => i.status === InspectionStatus.Scheduled).length,
      inProgressInspections,
      completedInspections,
      overdueInspections,
      criticalFindings,
      averageCompletionTime: 2.5,
      complianceRate: 94.2,
      recentInspections: mockInspections.slice(-10),
      criticalFindingsList: mockInspectionFindings.filter(f => f.severity === FindingSeverity.Critical),
      upcomingInspections: mockInspections.filter(i => 
        i.status === InspectionStatus.Scheduled && 
        new Date(i.scheduledDate) <= new Date(Date.now() + 14 * 24 * 60 * 60 * 1000)
      ),
      overdueList: mockInspections.filter(i => i.isOverdue),
      inspectionsByStatus: [
        {
          status: InspectionStatus.Completed,
          statusName: 'Completed',
          count: completedInspections,
          percentage: totalInspections > 0 ? Math.round((completedInspections / totalInspections) * 100) : 0
        },
        {
          status: InspectionStatus.InProgress,
          statusName: 'InProgress',
          count: inProgressInspections,
          percentage: totalInspections > 0 ? Math.round((inProgressInspections / totalInspections) * 100) : 0
        }
      ],
      inspectionsByType: [
        {
          type: InspectionType.Safety,
          typeName: 'Safety',
          count: mockInspections.filter(i => i.type === InspectionType.Safety).length,
          percentage: 45.0
        },
        {
          type: InspectionType.Environmental,
          typeName: 'Environmental',
          count: mockInspections.filter(i => i.type === InspectionType.Environmental).length,
          percentage: 35.0
        }
      ],
      monthlyTrends: [
        {
          month: 'Dec',
          year: 2023,
          scheduled: 25,
          completed: 23,
          overdue: 1,
          criticalFindings: 0
        },
        {
          month: 'Jan',
          year: 2024,
          scheduled: 30,
          completed: 25,
          overdue: 3,
          criticalFindings: 2
        }
      ]
    };

    return res(ctx.status(200), ctx.json(dashboardData));
  }),

  // Delete inspection
  rest.delete('/api/inspections/:id', (req, res, ctx) => {
    const { id } = req.params;
    const index = mockInspections.findIndex(i => i.id === parseInt(id as string));
    
    if (index === -1) {
      return res(ctx.status(404), ctx.json({ message: 'Inspection not found' }));
    }

    mockInspections.splice(index, 1);
    return res(ctx.status(204));
  })
];