import { rest } from 'msw';
import { LicenseDto, LicenseDashboardDto } from '../../../types/license';

// Mock license data
const mockLicenses: LicenseDto[] = [
  {
    id: 1,
    licenseNumber: 'LIC-ENV-001',
    title: 'Environmental Protection License',
    description: 'Authorization for environmental operations with monitoring requirements',
    type: 'Environmental',
    typeDisplay: 'Environmental',
    status: 'Active',
    statusDisplay: 'Active',
    priority: 'High',
    priorityDisplay: 'High',
    issuingAuthority: 'Environmental Protection Agency',
    holderName: 'John Smith',
    department: 'Operations',
    issuedDate: '2024-01-15',
    expiryDate: '2025-01-15',
    submittedDate: '2024-01-10',
    approvedDate: '2024-01-14',
    activatedDate: '2024-01-15',
    riskLevel: 'Medium',
    riskLevelDisplay: 'Medium',
    isCriticalLicense: false,
    requiresInsurance: true,
    requiredInsuranceAmount: 100000,
    licenseFee: 2500,
    currency: 'USD',
    scope: 'Industrial operations with environmental impact',
    restrictions: 'Must comply with emission standards',
    conditions: 'Regular monitoring and reporting required',
    statusNotes: 'Active and compliant',
    regulatoryFramework: 'Environmental Protection Act',
    applicableRegulations: 'EPA Regulation 123/2024',
    complianceStandards: 'ISO 14001:2015',
    renewalRequired: true,
    renewalPeriodDays: 90,
    nextRenewalDate: '2024-10-15',
    autoRenewal: false,
    renewalProcedure: 'Submit renewal application 90 days before expiry',
    attachments: [],
    renewals: [],
    licenseConditions: [
      {
        id: 1,
        conditionType: 'Inspection',
        description: 'Annual facility inspection required',
        isMandatory: true,
        dueDate: '2024-12-15',
        status: 'Pending',
        statusDisplay: 'Pending',
        complianceEvidence: '',
        complianceDate: null,
        verifiedBy: '',
        responsiblePerson: 'Jane Doe',
        notes: 'Schedule before year end',
        isOverdue: false,
        isCompleted: false,
        daysUntilDue: 45
      }
    ],
    createdAt: '2024-01-01T00:00:00Z',
    createdBy: 'System Admin',
    updatedAt: '2024-01-15T00:00:00Z',
    updatedBy: 'System Admin',
    canEdit: true,
    canSubmit: false,
    canApprove: false,
    canActivate: false,
    canSuspend: true,
    canRenew: false,
    isExpired: false,
    isExpiring: false,
    isActive: true,
    isHighRisk: false,
    hasRequiredInfo: true,
    isRenewalDue: false,
    daysUntilExpiry: 365,
    daysUntilRenewal: 90
  },
  {
    id: 2,
    licenseNumber: 'LIC-SAF-002',
    title: 'Safety Operations License',
    description: 'License for safety-critical operations',
    type: 'Safety',
    typeDisplay: 'Safety',
    status: 'Expired',
    statusDisplay: 'Expired',
    priority: 'Critical',
    priorityDisplay: 'Critical',
    issuingAuthority: 'Occupational Safety Administration',
    holderName: 'Sarah Johnson',
    department: 'Safety & Compliance',
    issuedDate: '2023-01-15',
    expiryDate: '2024-01-15',
    submittedDate: '2023-01-10',
    approvedDate: '2023-01-14',
    activatedDate: '2023-01-15',
    riskLevel: 'High',
    riskLevelDisplay: 'High',
    isCriticalLicense: true,
    requiresInsurance: true,
    requiredInsuranceAmount: 500000,
    licenseFee: 5000,
    currency: 'USD',
    scope: 'Safety-critical operations and procedures',
    restrictions: 'Requires trained personnel only',
    conditions: 'Monthly safety audits required',
    statusNotes: 'Expired - renewal required',
    regulatoryFramework: 'Occupational Safety Act',
    applicableRegulations: 'OSHA Standards 29 CFR 1910',
    complianceStandards: 'OHSAS 18001:2007',
    renewalRequired: true,
    renewalPeriodDays: 60,
    nextRenewalDate: null,
    autoRenewal: false,
    renewalProcedure: 'Submit renewal with updated safety assessment',
    attachments: [],
    renewals: [],
    licenseConditions: [],
    createdAt: '2023-01-01T00:00:00Z',
    createdBy: 'System Admin',
    updatedAt: '2024-01-15T00:00:00Z',
    updatedBy: 'System Admin',
    canEdit: false,
    canSubmit: false,
    canApprove: false,
    canActivate: false,
    canSuspend: false,
    canRenew: true,
    isExpired: true,
    isExpiring: false,
    isActive: false,
    isHighRisk: true,
    hasRequiredInfo: true,
    isRenewalDue: true,
    daysUntilExpiry: -30,
    daysUntilRenewal: -30
  }
];

const mockDashboard: LicenseDashboardDto = {
  totalLicenses: 25,
  draftLicenses: 2,
  pendingSubmissionLicenses: 1,
  submittedLicenses: 3,
  underReviewLicenses: 2,
  approvedLicenses: 5,
  activeLicenses: 10,
  rejectedLicenses: 1,
  expiredLicenses: 1,
  suspendedLicenses: 0,
  revokedLicenses: 0,
  pendingRenewalLicenses: 3,
  expiringThisMonth: 2,
  expiringThisQuarter: 5,
  renewalsDue: 3,
  highRiskLicenses: 4,
  criticalLicenses: 2,
  overdueConditions: 1,
  averageLicenseFee: 3500,
  totalLicenseFees: 87500,
  totalRenewalFees: 12000,
  licensesByType: [
    {
      type: 'Environmental',
      typeDisplay: 'Environmental',
      count: 8,
      percentage: 32,
      active: 6,
      expired: 1,
      expiringThisMonth: 1
    },
    {
      type: 'Safety',
      typeDisplay: 'Safety',
      count: 7,
      percentage: 28,
      active: 5,
      expired: 1,
      expiringThisMonth: 1
    },
    {
      type: 'Health',
      typeDisplay: 'Health',
      count: 5,
      percentage: 20,
      active: 4,
      expired: 0,
      expiringThisMonth: 1
    }
  ],
  monthlyTrends: [
    {
      month: '2024-01',
      totalLicenses: 20,
      newLicenses: 3,
      renewedLicenses: 2,
      expiredLicenses: 1,
      averageFee: 3200
    }
  ],
  recentLicenses: mockLicenses.slice(0, 5),
  expiringLicenses: mockLicenses.filter(l => l.isExpiring).slice(0, 5),
  highPriorityLicenses: mockLicenses.filter(l => l.priority === 'High' || l.priority === 'Critical').slice(0, 5)
};

export const licenseHandlers = [
  // Get licenses
  rest.get('/api/licenses', (req, res, ctx) => {
    const page = Number(req.url.searchParams.get('page')) || 1;
    const pageSize = Number(req.url.searchParams.get('pageSize')) || 10;
    const searchTerm = req.url.searchParams.get('searchTerm') || '';
    
    let filteredLicenses = mockLicenses;
    
    if (searchTerm) {
      filteredLicenses = mockLicenses.filter(license =>
        license.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        license.licenseNumber.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
    
    const startIndex = (page - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const items = filteredLicenses.slice(startIndex, endIndex);
    
    return res(
      ctx.status(200),
      ctx.json({
        items,
        totalCount: filteredLicenses.length,
        totalPages: Math.ceil(filteredLicenses.length / pageSize),
        currentPage: page,
        pageSize
      })
    );
  }),

  // Get license by ID
  rest.get('/api/licenses/:id', (req, res, ctx) => {
    const { id } = req.params;
    const license = mockLicenses.find(l => l.id === Number(id));
    
    if (!license) {
      return res(ctx.status(404), ctx.json({ message: 'License not found' }));
    }
    
    return res(ctx.status(200), ctx.json(license));
  }),

  // Get license dashboard
  rest.get('/api/licenses/dashboard', (req, res, ctx) => {
    return res(ctx.status(200), ctx.json(mockDashboard));
  }),

  // Get expiring licenses
  rest.get('/api/licenses/expiring', (req, res, ctx) => {
    const expiringLicenses = mockLicenses.filter(l => l.isExpiring || l.daysUntilExpiry <= 30);
    
    return res(
      ctx.status(200),
      ctx.json({
        items: expiringLicenses,
        totalCount: expiringLicenses.length,
        totalPages: 1,
        currentPage: 1,
        pageSize: 10
      })
    );
  }),

  // Create license
  rest.post('/api/licenses', (req, res, ctx) => {
    const newLicense = {
      ...mockLicenses[0],
      id: Date.now(),
      licenseNumber: `LIC-TEST-${Date.now()}`,
      status: 'Draft',
      statusDisplay: 'Draft',
      canSubmit: true,
      canEdit: true
    };
    
    return res(ctx.status(201), ctx.json(newLicense));
  }),

  // Update license
  rest.put('/api/licenses/:id', (req, res, ctx) => {
    const { id } = req.params;
    const license = mockLicenses.find(l => l.id === Number(id));
    
    if (!license) {
      return res(ctx.status(404), ctx.json({ message: 'License not found' }));
    }
    
    return res(ctx.status(200), ctx.json({ ...license, updatedAt: new Date().toISOString() }));
  }),

  // Submit license
  rest.post('/api/licenses/:id/submit', (req, res, ctx) => {
    const { id } = req.params;
    const license = mockLicenses.find(l => l.id === Number(id));
    
    if (!license) {
      return res(ctx.status(404), ctx.json({ message: 'License not found' }));
    }
    
    return res(
      ctx.status(200),
      ctx.json({
        ...license,
        status: 'Submitted',
        statusDisplay: 'Submitted',
        submittedDate: new Date().toISOString(),
        canSubmit: false,
        canEdit: false
      })
    );
  }),

  // Approve license
  rest.post('/api/licenses/:id/approve', (req, res, ctx) => {
    const { id } = req.params;
    const license = mockLicenses.find(l => l.id === Number(id));
    
    if (!license) {
      return res(ctx.status(404), ctx.json({ message: 'License not found' }));
    }
    
    return res(
      ctx.status(200),
      ctx.json({
        ...license,
        status: 'Approved',
        statusDisplay: 'Approved',
        approvedDate: new Date().toISOString(),
        canApprove: false,
        canActivate: true
      })
    );
  }),

  // Delete license
  rest.delete('/api/licenses/:id', (req, res, ctx) => {
    return res(ctx.status(204));
  }),

  // Upload attachment
  rest.post('/api/licenses/:id/attachments', (req, res, ctx) => {
    return res(
      ctx.status(201),
      ctx.json({
        id: Date.now(),
        fileName: 'test-file.pdf',
        originalFileName: 'test-file.pdf',
        contentType: 'application/pdf',
        fileSize: 1024,
        uploadedBy: 'Test User',
        uploadedAt: new Date().toISOString(),
        attachmentType: 'SupportingDocument',
        attachmentTypeDisplay: 'Supporting Document',
        description: 'Test attachment',
        isRequired: false,
        isExpired: false
      })
    );
  })
];

export default licenseHandlers;