import { http, HttpResponse } from 'msw';

export const ppeHandlers = [
  // Get PPE dashboard
  http.get('/api/ppe/dashboard', () => {
    return HttpResponse.json({
      totalItems: 250,
      availableItems: 180,
      assignedItems: 60,
      maintenanceItems: 8,
      retiredItems: 2,
      totalValue: 15000,
      lowStockAlerts: 5,
      expiringItems: 12,
      categoryBreakdown: [
        { category: 'Hard Hats', available: 45, assigned: 15, maintenance: 2, total: 62 },
        { category: 'Safety Vests', available: 38, assigned: 12, maintenance: 1, total: 51 },
        { category: 'Safety Boots', available: 22, assigned: 18, maintenance: 3, total: 43 }
      ],
      recentActivity: []
    });
  }),

  // Get PPE items
  http.get('/api/ppe', ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('pageNumber') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
    
    return HttpResponse.json({
      items: [
        {
          id: 1,
          name: 'Hard Hat - Standard',
          description: 'Standard construction hard hat',
          category: 'Head Protection',
          size: 'Medium',
          condition: 'New',
          status: 'Available',
          serialNumber: 'HH001',
          purchaseDate: '2024-01-15',
          cost: 45.00,
          storageLocation: 'Storage Room A',
          assignedTo: null,
          expiryDate: '2026-01-15',
          lastInspectionDate: '2024-03-01',
          nextInspectionDate: '2024-06-01'
        }
      ],
      totalCount: 250,
      pageNumber: page,
      pageSize: pageSize,
      totalPages: Math.ceil(250 / pageSize),
      hasNextPage: page < Math.ceil(250 / pageSize),
      hasPreviousPage: page > 1
    });
  })
];