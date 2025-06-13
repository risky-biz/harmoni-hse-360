import { http, HttpResponse } from 'msw';

export const authHandlers = [
  // Login endpoint
  http.post('/api/auth/login', async ({ request }) => {
    const { email, password } = await request.json() as any;
    
    if (email === 'admin@test.com' && password === 'password') {
      return HttpResponse.json({
        token: 'mock-jwt-token',
        refreshToken: 'mock-refresh-token',
        user: {
          id: '1',
          email: 'admin@test.com',
          name: 'Test Admin',
          roles: ['Administrator']
        }
      });
    }
    
    return HttpResponse.json(
      { message: 'Invalid credentials' },
      { status: 401 }
    );
  }),

  // Get user profile
  http.get('/api/auth/profile', () => {
    return HttpResponse.json({
      id: '1',
      email: 'admin@test.com',
      name: 'Test Admin',
      roles: ['Administrator'],
      department: 'Administration'
    });
  }),

  // Refresh token
  http.post('/api/auth/refresh', () => {
    return HttpResponse.json({
      token: 'new-mock-jwt-token',
      refreshToken: 'new-mock-refresh-token'
    });
  })
];