# Testing Framework for Harmoni360

## Overview

This directory contains a comprehensive testing framework for the Harmoni360 frontend application using Vitest, React Testing Library, and MSW (Mock Service Worker).

## Testing Structure

```
src/test/
├── components/          # Component unit tests
├── pages/              # Page integration tests
├── features/           # API and Redux tests
├── integration/        # Cross-component integration tests
├── mocks/             # MSW mock handlers
├── utils/             # Testing utilities
└── setup.ts           # Test environment setup
```

## Testing Libraries

- **Vitest**: Fast unit test runner with Jest-compatible API
- **React Testing Library**: Component testing with user-centric approach
- **MSW**: API mocking for realistic testing
- **Jest-DOM**: Extended Jest matchers for DOM testing
- **User Event**: Realistic user interaction simulation

## Available Scripts

```bash
# Run tests in watch mode
npm run test

# Run tests once with coverage
npm run test:coverage

# Run tests with UI interface
npm run test:ui

# Run tests in CI mode
npm run test:run
```

## Test Categories

### 1. Unit Tests
- Individual component functionality
- Utility function testing
- Redux slice testing
- API endpoint testing

Example:
```typescript
describe('StatsCard Component', () => {
  it('renders with basic props correctly', () => {
    render(<StatsCard title="Test" value="100" icon={faHeart} />);
    expect(screen.getByText('Test')).toBeInTheDocument();
  });
});
```

### 2. Integration Tests
- Component interaction testing
- API + UI integration
- Multi-step user workflows
- Error handling scenarios

Example:
```typescript
describe('Health Record Workflow', () => {
  it('allows creating a new health record', async () => {
    render(<CreateHealthRecord />);
    // ... test complete workflow
  });
});
```

### 3. API Tests
- RTK Query endpoint testing
- Request/response validation
- Error handling
- Cache behavior testing

Example:
```typescript
describe('Health API', () => {
  it('fetches dashboard data successfully', async () => {
    const result = await store.dispatch(
      healthApi.endpoints.getHealthDashboard.initiate({})
    );
    expect(result.data).toBeDefined();
  });
});
```

## Mock Data Strategy

### MSW Handlers
Mock API responses are defined in `mocks/handlers/` directory:
- `auth.ts` - Authentication endpoints
- `health.ts` - Health management endpoints
- `incidents.ts` - Incident management endpoints
- `ppe.ts` - PPE management endpoints
- `hazards.ts` - Hazard management endpoints

### Realistic Test Data
Mock data closely mirrors production API responses with:
- Proper TypeScript types
- Realistic data relationships
- Edge cases and error scenarios
- Pagination and filtering support

## Testing Best Practices

### 1. User-Centric Testing
```typescript
// ✅ Good - Test user interactions
await user.click(screen.getByRole('button', { name: /create record/i }));

// ❌ Avoid - Testing implementation details
expect(wrapper.find('.create-button')).toHaveBeenClicked();
```

### 2. Accessible Queries
```typescript
// ✅ Good - Use accessible queries
screen.getByRole('button', { name: /submit/i })
screen.getByLabelText(/email address/i)

// ❌ Avoid - Fragile selectors
screen.getByClassName('submit-btn')
```

### 3. Async Testing
```typescript
// ✅ Good - Wait for async operations
await waitFor(() => {
  expect(screen.getByText(/success message/i)).toBeInTheDocument();
});

// ❌ Avoid - Not waiting for async operations
expect(screen.getByText(/success message/i)).toBeInTheDocument();
```

### 4. Error Testing
```typescript
it('handles API errors gracefully', async () => {
  // Mock API error
  server.use(
    http.get('/api/health/records', () => {
      return HttpResponse.json(
        { message: 'Server error' },
        { status: 500 }
      );
    })
  );
  
  render(<HealthList />);
  
  await waitFor(() => {
    expect(screen.getByText(/error loading/i)).toBeInTheDocument();
  });
});
```

## Test Coverage Goals

- **Unit Tests**: >80% line coverage
- **Integration Tests**: All critical user workflows
- **API Tests**: All endpoints and error cases
- **Accessibility**: All interactive elements

## Running Tests in CI/CD

Tests are configured to run in GitHub Actions with:
- Node.js environment setup
- Dependency caching
- Coverage reporting
- Failure notifications

## Mock Service Worker (MSW) Setup

MSW intercepts network requests and provides controlled responses:

1. **Development**: Helps with frontend development when backend is unavailable
2. **Testing**: Provides predictable API responses for reliable tests
3. **Demo**: Can be used for demo environments with mock data

### MSW Configuration

```typescript
// src/test/mocks/server.ts
export const server = setupServer(
  ...authHandlers,
  ...healthHandlers,
  // ... other handlers
);

// Automatically starts/stops with test lifecycle
beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

## Debugging Tests

### 1. Visual Debugging
```bash
# Open Vitest UI for interactive debugging
npm run test:ui
```

### 2. Debug Specific Tests
```typescript
import { screen } from '@testing-library/react';

it('debug test', () => {
  render(<Component />);
  
  // Print current DOM
  screen.debug();
  
  // Print specific element
  screen.debug(screen.getByRole('button'));
});
```

### 3. Check Element Queries
```typescript
import { logRoles } from '@testing-library/react';

it('check available roles', () => {
  const { container } = render(<Component />);
  logRoles(container);
});
```

## Future Enhancements

### 1. E2E Testing with Playwright
- Cross-browser testing
- Real user scenarios
- Performance testing
- Visual regression testing

### 2. Component Visual Testing
- Storybook integration
- Visual diff testing
- Component documentation

### 3. Performance Testing
- Component render performance
- API response time testing
- Memory leak detection

### 4. Accessibility Testing
- Automated a11y testing
- Screen reader compatibility
- Keyboard navigation testing

## Contributing

When adding new tests:

1. **Choose appropriate test type** (unit vs integration)
2. **Use descriptive test names** that explain the behavior
3. **Follow AAA pattern** (Arrange, Act, Assert)
4. **Mock external dependencies** appropriately
5. **Test error scenarios** not just happy paths
6. **Ensure tests are deterministic** and don't rely on timing
7. **Update mock data** when API changes

## Example Test Template

```typescript
import { describe, it, expect, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render, createAuthenticatedState } from '../../utils/test-utils';
import YourComponent from '../../../path/to/YourComponent';

describe('YourComponent', () => {
  it('does something specific', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<YourComponent />, {
      initialState: createAuthenticatedState()
    });
    
    // Act
    await user.click(screen.getByRole('button', { name: /action/i }));
    
    // Assert
    await waitFor(() => {
      expect(screen.getByText(/expected result/i)).toBeInTheDocument();
    });
  });
});
```

This testing framework provides comprehensive coverage for the Harmoni360 application while maintaining fast execution and reliable results.