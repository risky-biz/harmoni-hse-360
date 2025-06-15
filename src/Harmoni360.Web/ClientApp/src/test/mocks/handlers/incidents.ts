import { http, HttpResponse } from 'msw';

export const incidentHandlers = [
  // Get incidents dashboard
  http.get('/api/incident/dashboard', () => {
    return HttpResponse.json({
      totalIncidents: 25,
      openIncidents: 8,
      resolvedIncidents: 17,
      criticalIncidents: 3,
      incidentsByStatus: [
        { status: 'Reported', count: 5, percentage: 20 },
        { status: 'Under Investigation', count: 3, percentage: 12 },
        { status: 'Resolved', count: 17, percentage: 68 }
      ],
      incidentsBySeverity: [
        { severity: 'Critical', count: 3, percentage: 12 },
        { severity: 'Serious', count: 8, percentage: 32 },
        { severity: 'Moderate', count: 14, percentage: 56 }
      ],
      recentIncidents: [],
      monthlyTrends: []
    });
  }),

  // Get incidents
  http.get('/api/incident', ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('pageNumber') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
    
    return HttpResponse.json({
      items: [
        {
          id: 1,
          title: 'Slip and Fall in Cafeteria',
          description: 'Student slipped on wet floor',
          severity: 'Moderate',
          status: 'Under Investigation',
          location: 'Main Cafeteria',
          reportedAt: '2024-03-15T10:30:00Z',
          reporterName: 'John Teacher',
          reporterEmail: 'john.teacher@test.com',
          isInjuryIncident: true,
          injuryType: 'Minor',
          bodyPartAffected: 'Ankle',
          witnessNames: ['Student A', 'Student B'],
          immediateActionsTaken: 'Applied first aid, cleaned up spill'
        }
      ],
      totalCount: 25,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(25 / pageSize),
      hasNextPage: page < Math.ceil(25 / pageSize),
      hasPreviousPage: page > 1
    });
  }),

  // Create incident
  http.post('/api/incident', async ({ request }) => {
    const body = await request.json() as any;
    
    return HttpResponse.json({
      id: 999,
      title: body.title,
      description: body.description,
      severity: body.severity,
      status: 'Reported',
      location: body.location,
      reportedAt: new Date().toISOString(),
      reporterName: body.reporterName,
      reporterEmail: body.reporterEmail
    }, { status: 201 });
  })
];