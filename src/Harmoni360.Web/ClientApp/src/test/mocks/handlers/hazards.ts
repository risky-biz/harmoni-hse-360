import { http, HttpResponse } from 'msw';

export const hazardHandlers = [
  // Get hazard dashboard
  http.get('/api/hazard/dashboard', () => {
    return HttpResponse.json({
      totalHazards: 18,
      openHazards: 6,
      resolvedHazards: 12,
      criticalHazards: 2,
      highRiskHazards: 4,
      mediumRiskHazards: 8,
      lowRiskHazards: 4,
      hazardsByCategory: [
        { category: 'Physical', count: 8, percentage: 44.4 },
        { category: 'Chemical', count: 4, percentage: 22.2 },
        { category: 'Environmental', count: 6, percentage: 33.3 }
      ],
      hazardsByStatus: [
        { status: 'Open', count: 6, percentage: 33.3 },
        { status: 'Under Review', count: 4, percentage: 22.2 },
        { status: 'Resolved', count: 8, percentage: 44.4 }
      ],
      riskMatrix: [
        { likelihood: 1, severity: 1, count: 2 },
        { likelihood: 2, severity: 2, count: 4 },
        { likelihood: 3, severity: 3, count: 6 }
      ],
      monthlyTrends: []
    });
  }),

  // Get hazards
  http.get('/api/hazard', ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('pageNumber') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
    
    return HttpResponse.json({
      items: [
        {
          id: 1,
          title: 'Wet Floor in Gymnasium',
          description: 'Water leak causing slippery surface',
          category: 'Physical',
          status: 'Open',
          severity: 'High',
          likelihood: 'Medium',
          riskLevel: 'High',
          location: 'Main Gymnasium',
          reportedDate: '2024-03-15T09:00:00Z',
          reportedBy: 'Maintenance Staff',
          priorityScore: 15
        }
      ],
      totalCount: 18,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(18 / pageSize),
      hasNextPage: page < Math.ceil(18 / pageSize),
      hasPreviousPage: page > 1
    });
  })
];