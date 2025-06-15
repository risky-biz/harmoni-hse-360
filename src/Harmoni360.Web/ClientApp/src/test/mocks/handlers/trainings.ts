import { http, HttpResponse } from 'msw';
import { TrainingDto, TrainingParticipantDto, TrainingAttachmentDto, TrainingDashboardDto } from '../../../types/training';
import { TrainingType, TrainingStatus, TrainingCategory, TrainingPriority, TrainingDeliveryMethod, ParticipantStatus } from '../../../types/training';

// Mock training data
const mockTrainings: TrainingDto[] = [
  {
    id: 1,
    trainingNumber: 'TRN-20241213-001',
    title: 'Safety Induction Training',
    description: 'Comprehensive safety training for new employees covering workplace hazards, emergency procedures, and safety protocols.',
    type: TrainingType.SafetyOrientation,
    category: TrainingCategory.Mandatory,
    priority: TrainingPriority.High,
    status: TrainingStatus.Scheduled,
    deliveryMethod: TrainingDeliveryMethod.Classroom,
    scheduledDate: new Date('2024-12-20T09:00:00Z'),
    startTime: null,
    endTime: null,
    completedDate: null,
    trainerId: 2,
    trainerName: 'John Smith',
    locationId: 1,
    location: 'Training Room A',
    departmentId: 1,
    departmentName: 'Safety Department',
    facilityId: null,
    facilityName: null,
    estimatedDurationMinutes: 240,
    actualDurationMinutes: null,
    maxParticipants: 25,
    minParticipants: 5,
    enrolledParticipants: 18,
    attendedParticipants: 0,
    passedParticipants: 0,
    isRecurring: false,
    recurrenceIntervalMonths: null,
    nextScheduledDate: null,
    isMandatory: true,
    requiresCertification: true,
    certificationValidityMonths: 12,
    passingScore: 80.0,
    competencyStandard: 'ISO 45001:2018',
    regulatoryRequirement: 'OSHA 1926.95',
    learningObjectives: 'Understand workplace safety protocols, identify common hazards, and learn emergency response procedures.',
    trainingContent: null,
    assessmentMethod: null,
    feedbackSummary: null,
    overallEffectivenessScore: null,
    effectivenessRating: null,
    isOverdue: false,
    isExpiring: false,
    canEdit: true,
    canStart: true,
    canComplete: false,
    canCancel: true,
    canEnroll: true,
    createdAt: new Date('2024-12-13T10:00:00Z'),
    createdBy: 'Admin User',
    modifiedAt: new Date('2024-12-13T10:00:00Z'),
    modifiedBy: 'Admin User',
    requirements: [],
    attachments: [],
    comments: []
  },
  {
    id: 2,
    trainingNumber: 'TRN-20241213-002',
    title: 'Emergency Response Training',
    description: 'Training on emergency response procedures and evacuation protocols.',
    type: TrainingType.EmergencyResponse,
    category: TrainingCategory.Mandatory,
    priority: TrainingPriority.High,
    status: TrainingStatus.InProgress,
    deliveryMethod: TrainingDeliveryMethod.Workshop,
    scheduledDate: new Date('2024-12-18T14:00:00Z'),
    startTime: new Date('2024-12-18T14:00:00Z'),
    endTime: null,
    completedDate: null,
    trainerId: 3,
    trainerName: 'Sarah Johnson',
    locationId: 2,
    location: 'Training Room B',
    departmentId: 1,
    departmentName: 'Safety Department',
    facilityId: null,
    facilityName: null,
    estimatedDurationMinutes: 180,
    actualDurationMinutes: null,
    maxParticipants: 20,
    minParticipants: 5,
    enrolledParticipants: 15,
    attendedParticipants: 12,
    passedParticipants: 0,
    isRecurring: true,
    recurrenceIntervalMonths: 6,
    nextScheduledDate: new Date('2025-06-18T14:00:00Z'),
    isMandatory: true,
    requiresCertification: true,
    certificationValidityMonths: 6,
    passingScore: 85.0,
    competencyStandard: 'Emergency Response Standard',
    regulatoryRequirement: 'Local Emergency Regulations',
    learningObjectives: 'Master emergency response procedures and evacuation protocols.',
    trainingContent: null,
    assessmentMethod: 'Practical demonstration and written test',
    feedbackSummary: null,
    overallEffectivenessScore: null,
    effectivenessRating: null,
    isOverdue: false,
    isExpiring: false,
    canEdit: false,
    canStart: false,
    canComplete: true,
    canCancel: true,
    canEnroll: false,
    createdAt: new Date('2024-12-10T15:30:00Z'),
    createdBy: 'Safety Manager',
    modifiedAt: new Date('2024-12-18T14:00:00Z'),
    modifiedBy: 'Sarah Johnson',
    requirements: [],
    attachments: [],
    comments: []
  },
  {
    id: 3,
    trainingNumber: 'TRN-20241213-003',
    title: 'K3 Compliance Training',
    description: 'Indonesian K3 (Keselamatan dan Kesehatan Kerja) compliance training.',
    type: TrainingType.K3Training,
    category: TrainingCategory.Regulatory,
    priority: TrainingPriority.Critical,
    status: TrainingStatus.Completed,
    deliveryMethod: TrainingDeliveryMethod.Blended,
    scheduledDate: new Date('2024-12-15T08:00:00Z'),
    startTime: new Date('2024-12-15T08:00:00Z'),
    endTime: new Date('2024-12-15T12:00:00Z'),
    completedDate: new Date('2024-12-15T12:00:00Z'),
    trainerId: 2,
    trainerName: 'John Smith',
    locationId: 1,
    location: 'Training Room A',
    departmentId: 2,
    departmentName: 'Operations',
    facilityId: 1,
    facilityName: 'Main Facility',
    estimatedDurationMinutes: 240,
    actualDurationMinutes: 235,
    maxParticipants: 30,
    minParticipants: 10,
    enrolledParticipants: 25,
    attendedParticipants: 23,
    passedParticipants: 21,
    isRecurring: true,
    recurrenceIntervalMonths: 12,
    nextScheduledDate: new Date('2025-12-15T08:00:00Z'),
    isMandatory: true,
    requiresCertification: true,
    certificationValidityMonths: 12,
    passingScore: 75.0,
    competencyStandard: 'Indonesian K3 Standards',
    regulatoryRequirement: 'Permenaker No. 5 Tahun 2018',
    learningObjectives: 'Comply with Indonesian K3 regulations and workplace safety standards.',
    trainingContent: 'K3 regulations, workplace safety, hazard identification, risk assessment.',
    assessmentMethod: 'Written examination and practical assessment',
    feedbackSummary: 'Excellent engagement from participants. Strong understanding of K3 principles.',
    overallEffectivenessScore: 92.5,
    effectivenessRating: 'Excellent',
    isOverdue: false,
    isExpiring: false,
    canEdit: false,
    canStart: false,
    canComplete: false,
    canCancel: false,
    canEnroll: false,
    createdAt: new Date('2024-12-01T09:00:00Z'),
    createdBy: 'K3 Coordinator',
    modifiedAt: new Date('2024-12-15T12:00:00Z'),
    modifiedBy: 'John Smith',
    requirements: [],
    attachments: [],
    comments: []
  }
];

// Mock dashboard data
const mockDashboard: TrainingDashboardDto = {
  totalTrainings: 15,
  scheduledTrainings: 5,
  inProgressTrainings: 2,
  completedTrainings: 7,
  cancelledTrainings: 1,
  overdueTrainings: 3,
  totalParticipants: 125,
  averageAttendanceRate: 87.5,
  averagePassRate: 92.3,
  averageEffectivenessScore: 89.2,
  upcomingTrainings: [
    {
      id: 1,
      title: 'Safety Induction Training',
      scheduledDate: new Date('2024-12-20T09:00:00Z'),
      trainerName: 'John Smith',
      enrolledParticipants: 18,
      maxParticipants: 25
    },
    {
      id: 4,
      title: 'Equipment Operation Training',
      scheduledDate: new Date('2024-12-22T13:00:00Z'),
      trainerName: 'Mike Wilson',
      enrolledParticipants: 12,
      maxParticipants: 15
    }
  ],
  trainingsByType: [
    { type: 'Safety Orientation', count: 5 },
    { type: 'Emergency Response', count: 3 },
    { type: 'K3 Training', count: 4 },
    { type: 'Technical', count: 2 },
    { type: 'Equipment', count: 1 }
  ],
  trainingsByStatus: [
    { status: 'Scheduled', count: 5 },
    { status: 'In Progress', count: 2 },
    { status: 'Completed', count: 7 },
    { status: 'Cancelled', count: 1 }
  ],
  monthlyStatistics: [
    { month: 'Nov 2024', trainingsCompleted: 8, totalParticipants: 95, averageScore: 88.5 },
    { month: 'Dec 2024', trainingsCompleted: 7, totalParticipants: 125, averageScore: 89.2 }
  ],
  expiringCertifications: [
    {
      participantId: 1,
      participantName: 'Jane Doe',
      trainingTitle: 'Safety Orientation',
      certificationNumber: 'CERT-20231215-001',
      expiryDate: new Date('2025-01-15T00:00:00Z'),
      daysUntilExpiry: 33
    }
  ]
};

export const trainingHandlers = [
  // Get all trainings with filtering and pagination
  http.get('/api/trainings', ({ request }) => {
    const url = new URL(request.url);
    const pageNumber = parseInt(url.searchParams.get('pageNumber') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
    const searchTerm = url.searchParams.get('searchTerm');
    const status = url.searchParams.get('status');
    const type = url.searchParams.get('type');
    const trainerId = url.searchParams.get('trainerId');

    let filteredTrainings = [...mockTrainings];

    // Apply filters
    if (searchTerm) {
      filteredTrainings = filteredTrainings.filter(t => 
        t.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        t.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
        t.trainingNumber.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    if (status) {
      filteredTrainings = filteredTrainings.filter(t => t.status === status);
    }

    if (type) {
      filteredTrainings = filteredTrainings.filter(t => t.type === type);
    }

    if (trainerId) {
      filteredTrainings = filteredTrainings.filter(t => t.trainerId.toString() === trainerId);
    }

    // Apply pagination
    const startIndex = (pageNumber - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedTrainings = filteredTrainings.slice(startIndex, endIndex);

    return HttpResponse.json({
      data: paginatedTrainings,
      pageNumber,
      pageSize,
      totalCount: filteredTrainings.length,
      totalPages: Math.ceil(filteredTrainings.length / pageSize),
      hasPreviousPage: pageNumber > 1,
      hasNextPage: endIndex < filteredTrainings.length
    });
  }),

  // Get training by ID
  http.get('/api/trainings/:id', ({ params }) => {
    const id = parseInt(params.id as string);
    const training = mockTrainings.find(t => t.id === id);
    
    if (!training) {
      return new HttpResponse(null, { status: 404 });
    }

    return HttpResponse.json(training);
  }),

  // Create training
  http.post('/api/trainings', async ({ request }) => {
    const body = await request.json() as any;
    
    const newTraining: TrainingDto = {
      id: mockTrainings.length + 1,
      trainingNumber: `TRN-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${String(mockTrainings.length + 1).padStart(3, '0')}`,
      title: body.title,
      description: body.description,
      type: body.type,
      category: body.category,
      priority: body.priority,
      status: TrainingStatus.Draft,
      deliveryMethod: body.deliveryMethod,
      scheduledDate: new Date(body.scheduledDate),
      startTime: null,
      endTime: null,
      completedDate: null,
      trainerId: body.trainerId,
      trainerName: 'John Smith', // Mock trainer name
      locationId: body.locationId || null,
      location: body.locationId ? 'Training Room A' : null,
      departmentId: body.departmentId || null,
      departmentName: body.departmentId ? 'Safety Department' : null,
      facilityId: body.facilityId || null,
      facilityName: body.facilityId ? 'Main Facility' : null,
      estimatedDurationMinutes: body.estimatedDurationMinutes,
      actualDurationMinutes: null,
      maxParticipants: body.maxParticipants,
      minParticipants: body.minParticipants,
      enrolledParticipants: 0,
      attendedParticipants: 0,
      passedParticipants: 0,
      isRecurring: body.isRecurring || false,
      recurrenceIntervalMonths: body.recurrenceIntervalMonths || null,
      nextScheduledDate: null,
      isMandatory: body.isMandatory,
      requiresCertification: body.requiresCertification,
      certificationValidityMonths: body.certificationValidityMonths || null,
      passingScore: body.passingScore || null,
      competencyStandard: body.competencyStandard || null,
      regulatoryRequirement: body.regulatoryRequirement || null,
      learningObjectives: body.learningObjectives,
      trainingContent: body.trainingContent || null,
      assessmentMethod: body.assessmentMethod || null,
      feedbackSummary: null,
      overallEffectivenessScore: null,
      effectivenessRating: null,
      isOverdue: false,
      isExpiring: false,
      canEdit: true,
      canStart: false,
      canComplete: false,
      canCancel: true,
      canEnroll: true,
      createdAt: new Date(),
      createdBy: 'Test User',
      modifiedAt: new Date(),
      modifiedBy: 'Test User',
      requirements: body.requirements || [],
      attachments: [],
      comments: []
    };

    mockTrainings.push(newTraining);

    return HttpResponse.json(newTraining, { 
      status: 201,
      headers: {
        'Location': `/api/trainings/${newTraining.id}`
      }
    });
  }),

  // Update training
  http.put('/api/trainings/:id', async ({ params, request }) => {
    const id = parseInt(params.id as string);
    const body = await request.json() as any;
    const trainingIndex = mockTrainings.findIndex(t => t.id === id);
    
    if (trainingIndex === -1) {
      return new HttpResponse(null, { status: 404 });
    }

    const existingTraining = mockTrainings[trainingIndex];
    const updatedTraining: TrainingDto = {
      ...existingTraining,
      title: body.title,
      description: body.description,
      type: body.type,
      category: body.category,
      priority: body.priority,
      deliveryMethod: body.deliveryMethod,
      scheduledDate: new Date(body.scheduledDate),
      trainerId: body.trainerId,
      locationId: body.locationId || null,
      departmentId: body.departmentId || null,
      facilityId: body.facilityId || null,
      estimatedDurationMinutes: body.estimatedDurationMinutes,
      maxParticipants: body.maxParticipants,
      minParticipants: body.minParticipants,
      isRecurring: body.isRecurring || false,
      recurrenceIntervalMonths: body.recurrenceIntervalMonths || null,
      isMandatory: body.isMandatory,
      requiresCertification: body.requiresCertification,
      certificationValidityMonths: body.certificationValidityMonths || null,
      passingScore: body.passingScore || null,
      competencyStandard: body.competencyStandard || null,
      regulatoryRequirement: body.regulatoryRequirement || null,
      learningObjectives: body.learningObjectives,
      trainingContent: body.trainingContent || null,
      assessmentMethod: body.assessmentMethod || null,
      modifiedAt: new Date(),
      modifiedBy: 'Test User',
      requirements: body.requirements || existingTraining.requirements
    };

    mockTrainings[trainingIndex] = updatedTraining;
    return HttpResponse.json(updatedTraining);
  }),

  // Delete training
  http.delete('/api/trainings/:id', ({ params }) => {
    const id = parseInt(params.id as string);
    const trainingIndex = mockTrainings.findIndex(t => t.id === id);
    
    if (trainingIndex === -1) {
      return new HttpResponse(null, { status: 404 });
    }

    mockTrainings.splice(trainingIndex, 1);
    return new HttpResponse(null, { status: 204 });
  }),

  // Get training dashboard
  http.get('/api/trainings/dashboard', () => {
    return HttpResponse.json(mockDashboard);
  }),

  // Get my trainings
  http.get('/api/trainings/my-trainings', ({ request }) => {
    const url = new URL(request.url);
    const pageNumber = parseInt(url.searchParams.get('pageNumber') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');

    // Filter trainings for current user (mock)
    const myTrainings = mockTrainings.filter(t => t.trainerId === 2 || t.createdBy === 'Test User');
    
    const startIndex = (pageNumber - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedTrainings = myTrainings.slice(startIndex, endIndex);

    return HttpResponse.json({
      data: paginatedTrainings,
      pageNumber,
      pageSize,
      totalCount: myTrainings.length,
      totalPages: Math.ceil(myTrainings.length / pageSize),
      hasPreviousPage: pageNumber > 1,
      hasNextPage: endIndex < myTrainings.length
    });
  }),

  // Schedule training
  http.post('/api/trainings/:id/schedule', async ({ params, request }) => {
    const id = parseInt(params.id as string);
    const body = await request.json() as any;
    const training = mockTrainings.find(t => t.id === id);
    
    if (!training) {
      return new HttpResponse(null, { status: 404 });
    }

    training.status = TrainingStatus.Scheduled;
    training.scheduledDate = new Date(body.scheduledDate);
    training.estimatedDurationMinutes = body.estimatedDurationMinutes;
    training.modifiedAt = new Date();

    return HttpResponse.json(training);
  }),

  // Start training
  http.post('/api/trainings/:id/start', ({ params }) => {
    const id = parseInt(params.id as string);
    const training = mockTrainings.find(t => t.id === id);
    
    if (!training) {
      return new HttpResponse(null, { status: 404 });
    }

    if (training.status !== TrainingStatus.Scheduled) {
      return new HttpResponse(null, { status: 400 });
    }

    training.status = TrainingStatus.InProgress;
    training.startTime = new Date();
    training.canStart = false;
    training.canComplete = true;
    training.modifiedAt = new Date();

    return HttpResponse.json(training);
  }),

  // Complete training
  http.post('/api/trainings/:id/complete', async ({ params, request }) => {
    const id = parseInt(params.id as string);
    const body = await request.json() as any;
    const training = mockTrainings.find(t => t.id === id);
    
    if (!training) {
      return new HttpResponse(null, { status: 404 });
    }

    if (training.status !== TrainingStatus.InProgress) {
      return new HttpResponse(null, { status: 400 });
    }

    training.status = TrainingStatus.Completed;
    training.completedDate = new Date();
    training.endTime = new Date();
    training.feedbackSummary = body.feedbackSummary || null;
    training.overallEffectivenessScore = body.effectivenessScore || null;
    training.actualDurationMinutes = body.actualDurationMinutes || training.estimatedDurationMinutes;
    training.canEdit = false;
    training.canStart = false;
    training.canComplete = false;
    training.canCancel = false;
    training.modifiedAt = new Date();

    return HttpResponse.json(training);
  }),

  // Cancel training
  http.post('/api/trainings/:id/cancel', async ({ params, request }) => {
    const id = parseInt(params.id as string);
    const body = await request.json() as any;
    const training = mockTrainings.find(t => t.id === id);
    
    if (!training) {
      return new HttpResponse(null, { status: 404 });
    }

    training.status = TrainingStatus.Cancelled;
    training.canEdit = false;
    training.canStart = false;
    training.canComplete = false;
    training.canCancel = false;
    training.modifiedAt = new Date();

    return HttpResponse.json(training);
  })
];