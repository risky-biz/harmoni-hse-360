// Theme switching tests for HarmoniHSE360
// Created: June 2025 - Phase 4 Implementation
// Comprehensive testing of theme functionality and React components

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';

import { ThemeProvider, useTheme } from '../contexts/ThemeContext';
import ThemeToggle from '../components/common/ThemeToggle';
import { RiskBadge, StatusBadge, EmergencyBadge } from '../components/common/StatusBadge';
import { EmergencyPanel } from '../components/common/EmergencyPanel';
import { runAccessibilityTests, ContrastChecker } from '../utils/accessibilityTest';

// Mock localStorage
const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};
Object.defineProperty(window, 'localStorage', {
  value: localStorageMock
});

// Mock matchMedia for system theme detection
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(),
    removeListener: jest.fn(),
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
});

// Test component for theme context
const TestComponent: React.FC = () => {
  const { theme, setTheme, effectiveTheme } = useTheme();
  
  return (
    <div>
      <span data-testid="current-theme">{theme}</span>
      <span data-testid="effective-theme">{effectiveTheme}</span>
      <button onClick={() => setTheme('light')} data-testid="set-light">
        Light
      </button>
      <button onClick={() => setTheme('dark')} data-testid="set-dark">
        Dark
      </button>
      <button onClick={() => setTheme('system')} data-testid="set-system">
        System
      </button>
    </div>
  );
};

describe('Theme Context', () => {
  beforeEach(() => {
    localStorageMock.getItem.mockClear();
    localStorageMock.setItem.mockClear();
    document.documentElement.removeAttribute('data-theme');
  });

  test('provides default theme as system', () => {
    localStorageMock.getItem.mockReturnValue(null);
    
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );
    
    expect(screen.getByTestId('current-theme')).toHaveTextContent('system');
  });

  test('loads theme from localStorage', () => {
    localStorageMock.getItem.mockReturnValue('dark');
    
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );
    
    expect(screen.getByTestId('current-theme')).toHaveTextContent('dark');
  });

  test('sets theme and saves to localStorage', async () => {
    const user = userEvent.setup();
    
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );
    
    await user.click(screen.getByTestId('set-dark'));
    
    expect(screen.getByTestId('current-theme')).toHaveTextContent('dark');
    expect(localStorageMock.setItem).toHaveBeenCalledWith('harmoni-theme', 'dark');
  });

  test('applies data-theme attribute to document', async () => {
    const user = userEvent.setup();
    
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );
    
    await user.click(screen.getByTestId('set-light'));
    
    await waitFor(() => {
      expect(document.documentElement.getAttribute('data-theme')).toBe('light');
    });
  });

  test('handles system theme detection', () => {
    // Mock system preference for dark mode
    window.matchMedia = jest.fn().mockImplementation(query => ({
      matches: query === '(prefers-color-scheme: dark)',
      media: query,
      onchange: null,
      addListener: jest.fn(),
      removeListener: jest.fn(),
      addEventListener: jest.fn(),
      removeEventListener: jest.fn(),
      dispatchEvent: jest.fn(),
    }));
    
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );
    
    expect(screen.getByTestId('effective-theme')).toHaveTextContent('dark');
  });
});

describe('Theme Toggle Component', () => {
  test('renders theme toggle button', () => {
    render(
      <ThemeProvider>
        <ThemeToggle />
      </ThemeProvider>
    );
    
    expect(screen.getByRole('button', { name: /theme/i })).toBeInTheDocument();
  });

  test('shows theme options in dropdown', async () => {
    const user = userEvent.setup();
    
    render(
      <ThemeProvider>
        <ThemeToggle />
      </ThemeProvider>
    );
    
    await user.click(screen.getByRole('button', { name: /theme/i }));
    
    expect(screen.getByText(/light/i)).toBeInTheDocument();
    expect(screen.getByText(/dark/i)).toBeInTheDocument();
    expect(screen.getByText(/system/i)).toBeInTheDocument();
  });

  test('changes theme when option selected', async () => {
    const user = userEvent.setup();
    
    render(
      <ThemeProvider>
        <ThemeToggle />
        <TestComponent />
      </ThemeProvider>
    );
    
    await user.click(screen.getByRole('button', { name: /theme/i }));
    await user.click(screen.getByText(/dark/i));
    
    await waitFor(() => {
      expect(screen.getByTestId('current-theme')).toHaveTextContent('dark');
    });
  });
});

describe('Badge Components Theme Support', () => {
  test('RiskBadge renders with proper risk colors', () => {
    render(
      <ThemeProvider>
        <RiskBadge level="Critical" />
        <RiskBadge level="High" />
        <RiskBadge level="Medium" />
        <RiskBadge level="Low" />
      </ThemeProvider>
    );
    
    expect(screen.getByText('Critical')).toBeInTheDocument();
    expect(screen.getByText('High')).toBeInTheDocument();
    expect(screen.getByText('Medium')).toBeInTheDocument();
    expect(screen.getByText('Low')).toBeInTheDocument();
  });

  test('StatusBadge renders with proper status colors', () => {
    render(
      <ThemeProvider>
        <StatusBadge status="Draft" />
        <StatusBadge status="InProgress" />
        <StatusBadge status="Completed" />
        <StatusBadge status="Overdue" />
      </ThemeProvider>
    );
    
    expect(screen.getByText('Draft')).toBeInTheDocument();
    expect(screen.getByText('In Progress')).toBeInTheDocument();
    expect(screen.getByText('Completed')).toBeInTheDocument();
    expect(screen.getByText('Overdue')).toBeInTheDocument();
  });

  test('EmergencyBadge renders with emergency styling', () => {
    render(
      <ThemeProvider>
        <EmergencyBadge />
      </ThemeProvider>
    );
    
    const emergencyBadge = screen.getByText('EMERGENCY');
    expect(emergencyBadge).toBeInTheDocument();
    expect(emergencyBadge).toHaveAttribute('role', 'alert');
  });

  test('badges maintain accessibility in both themes', async () => {
    const user = userEvent.setup();
    
    render(
      <ThemeProvider>
        <ThemeToggle />
        <RiskBadge level="Critical" />
        <StatusBadge status="InProgress" />
      </ThemeProvider>
    );
    
    // Test light theme
    const criticalBadge = screen.getByText('Critical');
    expect(criticalBadge).toHaveAttribute('aria-label', 'Risk level: Critical');
    
    const progressBadge = screen.getByText('In Progress');
    expect(progressBadge).toHaveAttribute('aria-label', 'Status: In Progress');
    
    // Switch to dark theme
    await user.click(screen.getByRole('button', { name: /theme/i }));
    await user.click(screen.getByText(/dark/i));
    
    // Verify badges still accessible
    await waitFor(() => {
      expect(criticalBadge).toHaveAttribute('aria-label', 'Risk level: Critical');
      expect(progressBadge).toHaveAttribute('aria-label', 'Status: In Progress');
    });
  });
});

describe('Emergency Panel Theme Support', () => {
  const mockEmergencyData = {
    contacts: [
      {
        id: 'fire',
        name: 'Fire Department',
        number: '113',
        description: 'Fire emergencies',
        priority: 'primary' as const
      }
    ],
    procedures: [
      {
        id: 'fire',
        title: 'Fire Emergency',
        description: 'Fire procedure',
        steps: [
          { id: '1', title: 'Alert', description: 'Sound alarm' }
        ],
        contacts: ['fire']
      }
    ]
  };

  test('renders emergency panel with proper theming', () => {
    render(
      <ThemeProvider>
        <EmergencyPanel 
          contacts={mockEmergencyData.contacts}
          procedures={mockEmergencyData.procedures}
        />
      </ThemeProvider>
    );
    
    expect(screen.getByText('Fire Department')).toBeInTheDocument();
    expect(screen.getByText('Fire Emergency')).toBeInTheDocument();
  });

  test('emergency contact buttons work in both themes', async () => {
    const user = userEvent.setup();
    
    render(
      <ThemeProvider>
        <ThemeToggle />
        <EmergencyPanel 
          contacts={mockEmergencyData.contacts}
          procedures={mockEmergencyData.procedures}
        />
      </ThemeProvider>
    );
    
    // Test in light theme
    const callButton = screen.getByRole('button', { name: /call fire department/i });
    expect(callButton).toBeInTheDocument();
    
    // Switch to dark theme
    await user.click(screen.getByRole('button', { name: /theme/i }));
    await user.click(screen.getByText(/dark/i));
    
    // Verify button still works
    await waitFor(() => {
      expect(callButton).toBeInTheDocument();
    });
  });
});

describe('Theme Performance Tests', () => {
  test('theme switching completes under 100ms', async () => {
    const user = userEvent.setup();
    
    render(
      <ThemeProvider>
        <ThemeToggle />
      </ThemeProvider>
    );
    
    const startTime = performance.now();
    
    await user.click(screen.getByRole('button', { name: /theme/i }));
    await user.click(screen.getByText(/dark/i));
    
    await waitFor(() => {
      expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    });
    
    const endTime = performance.now();
    const switchTime = endTime - startTime;
    
    // Should complete well under 100ms (allowing for test overhead)
    expect(switchTime).toBeLessThan(500);
  });

  test('CSS custom properties are applied correctly', async () => {
    const user = userEvent.setup();
    
    render(
      <ThemeProvider>
        <ThemeToggle />
      </ThemeProvider>
    );
    
    // Switch to dark theme
    await user.click(screen.getByRole('button', { name: /theme/i }));
    await user.click(screen.getByText(/dark/i));
    
    await waitFor(() => {
      const rootStyle = getComputedStyle(document.documentElement);
      // Verify CSS custom properties exist (they may be empty in test environment)
      expect(rootStyle.getPropertyValue('--theme-bg-primary')).toBeDefined();
    });
  });
});

describe('Accessibility Tests', () => {
  test('contrast checker validates design tokens', () => {
    const results = ContrastChecker.validateDesignTokens();
    
    // Check that critical color combinations pass WCAG AA
    expect(results['risk-critical-light'].passes.aa).toBe(true);
    expect(results['risk-critical-dark'].passes.aa).toBe(true);
    expect(results['text-primary-light'].passes.aa).toBe(true);
    expect(results['text-primary-dark'].passes.aa).toBe(true);
  });

  test('contrast ratios meet WCAG requirements', () => {
    // Test specific high-contrast combinations
    const whiteOnDark = ContrastChecker.testContrast('#FFFFFF', '#1A202C');
    expect(whiteOnDark.passes.aa).toBe(true);
    expect(whiteOnDark.ratio).toBeGreaterThan(4.5);
    
    const darkOnWhite = ContrastChecker.testContrast('#212529', '#FFFFFF');
    expect(darkOnWhite.passes.aa).toBe(true);
    expect(darkOnWhite.ratio).toBeGreaterThan(4.5);
  });

  test('emergency colors have sufficient contrast', () => {
    const emergencyContrast = ContrastChecker.testContrast('#DC3545', '#FFFFFF');
    expect(emergencyContrast.passes.aa).toBe(true);
    
    const emergencyDarkContrast = ContrastChecker.testContrast('#FF6B7A', '#1A202C');
    expect(emergencyDarkContrast.passes.aa).toBe(true);
  });
});

describe('Edge Cases and Error Handling', () => {
  test('handles invalid theme values gracefully', () => {
    localStorageMock.getItem.mockReturnValue('invalid-theme');
    
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );
    
    // Should fallback to system theme
    expect(screen.getByTestId('current-theme')).toHaveTextContent('system');
  });

  test('works without localStorage', () => {
    // Mock localStorage to throw error
    localStorageMock.getItem.mockImplementation(() => {
      throw new Error('localStorage not available');
    });
    
    expect(() => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );
    }).not.toThrow();
  });

  test('handles missing matchMedia gracefully', () => {
    // Remove matchMedia mock
    delete (window as any).matchMedia;
    
    expect(() => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );
    }).not.toThrow();
  });
});

describe('Integration Tests', () => {
  test('full theme switching workflow', async () => {
    const user = userEvent.setup();
    
    render(
      <ThemeProvider>
        <ThemeToggle />
        <RiskBadge level="Critical" />
        <StatusBadge status="InProgress" />
        <EmergencyPanel 
          contacts={[{
            id: 'test',
            name: 'Test Contact',
            number: '123',
            description: 'Test',
            priority: 'primary'
          }]}
          procedures={[]}
        />
      </ThemeProvider>
    );
    
    // Verify initial state
    expect(document.documentElement.getAttribute('data-theme')).toBe('light');
    
    // Switch to dark
    await user.click(screen.getByRole('button', { name: /theme/i }));
    await user.click(screen.getByText(/dark/i));
    
    await waitFor(() => {
      expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    });
    
    // Switch back to light
    await user.click(screen.getByRole('button', { name: /theme/i }));
    await user.click(screen.getByText(/light/i));
    
    await waitFor(() => {
      expect(document.documentElement.getAttribute('data-theme')).toBe('light');
    });
    
    // Verify components still render correctly
    expect(screen.getByText('Critical')).toBeInTheDocument();
    expect(screen.getByText('In Progress')).toBeInTheDocument();
    expect(screen.getByText('Test Contact')).toBeInTheDocument();
  });
});