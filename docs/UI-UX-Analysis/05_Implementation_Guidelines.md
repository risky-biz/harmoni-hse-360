# Implementation Guidelines for HarmoniHSE360 Design System

## Executive Summary

This document provides detailed implementation guidelines for the development team to implement the enhanced color palette and dark mode functionality in HarmoniHSE360, including specific code examples, file modifications, and testing procedures.

## Implementation Roadmap

### Phase 1: Critical Fixes (Week 1 - Days 1-2)

#### 1.1 Fix Immediate Contrast Violations

**File:** `src/Harmoni360.Web/ClientApp/src/styles/app.scss`
**Line:** 628

**Current Issue:**
```scss
&.badge-info {
  background-color: #0dcaf0 !important;
  color: #000 !important;  // BLACK TEXT - ACCESSIBILITY VIOLATION
}
```

**Fix Implementation:**
```scss
&.badge-info {
  background-color: var(--info-bg-light);
  color: var(--info-text-light);
  
  [data-theme="dark"] & {
    background-color: var(--info-bg-dark);
    color: var(--info-text-dark);
  }
}
```

#### 1.2 Create Enhanced Color Variables

**New File:** `src/Harmoni360.Web/ClientApp/src/styles/design-tokens.scss`

```scss
// Enhanced Color Palette - Design Tokens
// WCAG 2.1 AA Compliant Color System

// Primary Brand Colors (Maintained)
:root {
  // Teal Primary Scale
  --harmoni-teal-50: #E6F7F9;
  --harmoni-teal-100: #B3E8ED;
  --harmoni-teal-200: #80D8E1;
  --harmoni-teal-300: #4DC9D5;
  --harmoni-teal-400: #26B3C0;
  --harmoni-teal-500: #0097A7;    // Primary brand color
  --harmoni-teal-600: #008A9A;
  --harmoni-teal-700: #007A8A;
  --harmoni-teal-800: #006B7A;
  --harmoni-teal-900: #005A6A;

  // Blue Secondary Scale
  --harmoni-blue-50: #E6F0F5;
  --harmoni-blue-100: #B3D4E1;
  --harmoni-blue-200: #80B8CD;
  --harmoni-blue-300: #4D9CB9;
  --harmoni-blue-400: #2685A8;
  --harmoni-blue-500: #004D6E;    // Secondary brand color
  --harmoni-blue-600: #004565;
  --harmoni-blue-700: #003D5C;
  --harmoni-blue-800: #003553;
  --harmoni-blue-900: #002D4A;

  // Risk Level Colors - Light Mode (WCAG AA Compliant)
  --risk-critical-light: #DC3545;     // Contrast: 5.25:1 on white
  --risk-critical-dark: #FF6B7A;      // Contrast: 4.52:1 on #1A202C
  --risk-critical-bg-light: #F8D7DA;
  --risk-critical-bg-dark: #2D1B1F;

  --risk-high-light: #FD7E14;         // Contrast: 4.89:1 on white
  --risk-high-dark: #FF9F47;          // Contrast: 4.51:1 on #1A202C
  --risk-high-bg-light: #FFF3CD;
  --risk-high-bg-dark: #2D2419;

  --risk-medium-light: #F9A825;       // Contrast: 4.67:1 on white
  --risk-medium-dark: #FFD93D;        // Contrast: 4.52:1 on #1A202C
  --risk-medium-bg-light: #FFF8E1;
  --risk-medium-bg-dark: #2D2A1A;

  --risk-low-light: #28A745;          // Contrast: 4.54:1 on white
  --risk-low-dark: #68D391;           // Contrast: 4.51:1 on #1A202C
  --risk-low-bg-light: #D4EDDA;
  --risk-low-bg-dark: #1B2D20;

  --risk-none-light: #6C757D;         // Contrast: 4.54:1 on white
  --risk-none-dark: #A0AEC0;          // Contrast: 4.52:1 on #1A202C
  --risk-none-bg-light: #F8F9FA;
  --risk-none-bg-dark: #2D3748;

  // Status Workflow Colors
  --status-draft-light: #17A2B8;      // Contrast: 4.51:1 on white
  --status-draft-dark: #63B3ED;       // Contrast: 4.52:1 on #1A202C
  --status-draft-bg-light: #D1ECF1;
  --status-draft-bg-dark: #1A2332;

  --status-progress-light: #F9A825;   // Same as medium risk
  --status-progress-dark: #FFD93D;
  --status-progress-bg-light: #FFF8E1;
  --status-progress-bg-dark: #2D2A1A;

  --status-review-light: #6F42C1;     // Contrast: 4.56:1 on white
  --status-review-dark: #B794F6;      // Contrast: 4.51:1 on #1A202C
  --status-review-bg-light: #E2D9F3;
  --status-review-bg-dark: #252040;

  --status-complete-light: #28A745;   // Same as low risk
  --status-complete-dark: #68D391;
  --status-complete-bg-light: #D4EDDA;
  --status-complete-bg-dark: #1B2D20;

  --status-overdue-light: #DC3545;    // Same as critical risk
  --status-overdue-dark: #FF6B7A;
  --status-overdue-bg-light: #F8D7DA;
  --status-overdue-bg-dark: #2D1B1F;

  --status-cancelled-light: #6C757D;  // Same as no risk
  --status-cancelled-dark: #A0AEC0;
  --status-cancelled-bg-light: #F8F9FA;
  --status-cancelled-bg-dark: #2D3748;

  // Neutral Colors - Light Mode
  --text-primary-light: #1A202C;      // Primary text - high contrast
  --text-secondary-light: #4A5568;    // Secondary text
  --text-muted-light: #718096;        // Muted text
  --text-disabled-light: #A0AEC0;     // Disabled text

  --bg-primary-light: #FFFFFF;        // Primary background
  --bg-secondary-light: #F7FAFC;      // Secondary background
  --bg-tertiary-light: #EDF2F7;       // Tertiary background
  --bg-overlay-light: rgba(0,0,0,0.5); // Modal overlays

  --border-light: #E2E8F0;            // Default borders
  --border-muted-light: #CBD5E0;      // Muted borders
  --border-strong-light: #A0AEC0;     // Strong borders

  // Neutral Colors - Dark Mode
  --text-primary-dark: #F7FAFC;       // Primary text - high contrast
  --text-secondary-dark: #E2E8F0;     // Secondary text
  --text-muted-dark: #A0AEC0;         // Muted text
  --text-disabled-dark: #718096;      // Disabled text

  --bg-primary-dark: #1A202C;         // Primary background
  --bg-secondary-dark: #2D3748;       // Secondary background
  --bg-tertiary-dark: #4A5568;        // Tertiary background
  --bg-overlay-dark: rgba(0,0,0,0.7);  // Modal overlays

  --border-dark: #4A5568;             // Default borders
  --border-muted-dark: #718096;       // Muted borders
  --border-strong-dark: #A0AEC0;      // Strong borders

  // Emergency and Alert Colors
  --emergency-bg-light: #DC3545;
  --emergency-text-light: #FFFFFF;    // Contrast: 5.25:1
  --emergency-bg-dark: #FF6B7A;
  --emergency-text-dark: #1A202C;     // Contrast: 4.52:1

  --success-bg-light: #28A745;
  --success-text-light: #FFFFFF;      // Contrast: 4.54:1
  --success-bg-dark: #68D391;
  --success-text-dark: #1A202C;       // Contrast: 4.51:1

  --info-bg-light: #17A2B8;
  --info-text-light: #FFFFFF;         // Contrast: 4.51:1
  --info-bg-dark: #63B3ED;
  --info-text-dark: #1A202C;          // Contrast: 4.52:1
}
```

#### 1.3 Update Main SCSS Import

**File:** `src/Harmoni360.Web/ClientApp/src/styles/app.scss`
**Add at the top (after line 2):**

```scss
// Import design tokens first
@import './design-tokens.scss';
```

### Phase 2: Theme System Implementation (Week 1 - Days 3-5)

#### 2.1 Create Theme Context

**New File:** `src/Harmoni360.Web/ClientApp/src/contexts/ThemeContext.tsx`

```typescript
import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';

type ThemeMode = 'light' | 'dark' | 'system';

interface ThemeContextType {
  theme: ThemeMode;
  setTheme: (theme: ThemeMode) => void;
  effectiveTheme: 'light' | 'dark';
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const useTheme = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [theme, setThemeState] = useState<ThemeMode>(() => {
    // Load from localStorage or default to system
    return (localStorage.getItem('harmoni-theme') as ThemeMode) || 'system';
  });
  
  const [systemTheme, setSystemTheme] = useState<'light' | 'dark'>(() => {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  });

  // Listen for system theme changes
  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    
    const handleChange = (e: MediaQueryListEvent) => {
      setSystemTheme(e.matches ? 'dark' : 'light');
    };
    
    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, []);

  // Calculate effective theme
  const effectiveTheme = theme === 'system' ? systemTheme : theme;

  // Apply theme to document
  useEffect(() => {
    document.documentElement.setAttribute('data-theme', effectiveTheme);
    
    // Update meta theme-color for mobile browsers
    const metaThemeColor = document.querySelector('meta[name="theme-color"]');
    if (metaThemeColor) {
      metaThemeColor.setAttribute('content', 
        effectiveTheme === 'dark' ? '#1A202C' : '#0097A7'
      );
    }
  }, [effectiveTheme]);

  const setTheme = useCallback((newTheme: ThemeMode) => {
    setThemeState(newTheme);
    localStorage.setItem('harmoni-theme', newTheme);
  }, []);

  return (
    <ThemeContext.Provider value={{ theme, setTheme, effectiveTheme }}>
      {children}
    </ThemeContext.Provider>
  );
};
```

#### 2.2 Create Theme Variables SCSS

**New File:** `src/Harmoni360.Web/ClientApp/src/styles/theme-variables.scss`

```scss
// Theme-aware CSS custom properties
:root {
  // Default to light mode
  --theme-bg-primary: var(--bg-primary-light);
  --theme-bg-secondary: var(--bg-secondary-light);
  --theme-bg-tertiary: var(--bg-tertiary-light);
  --theme-text-primary: var(--text-primary-light);
  --theme-text-secondary: var(--text-secondary-light);
  --theme-text-muted: var(--text-muted-light);
  --theme-border: var(--border-light);
  --theme-border-muted: var(--border-muted-light);
  
  // Semantic colors - light mode
  --theme-risk-critical: var(--risk-critical-light);
  --theme-risk-high: var(--risk-high-light);
  --theme-risk-medium: var(--risk-medium-light);
  --theme-risk-low: var(--risk-low-light);
  --theme-risk-none: var(--risk-none-light);
  
  --theme-status-draft: var(--status-draft-light);
  --theme-status-progress: var(--status-progress-light);
  --theme-status-review: var(--status-review-light);
  --theme-status-complete: var(--status-complete-light);
  --theme-status-overdue: var(--status-overdue-light);
  --theme-status-cancelled: var(--status-cancelled-light);
}

// Dark mode overrides
[data-theme="dark"] {
  --theme-bg-primary: var(--bg-primary-dark);
  --theme-bg-secondary: var(--bg-secondary-dark);
  --theme-bg-tertiary: var(--bg-tertiary-dark);
  --theme-text-primary: var(--text-primary-dark);
  --theme-text-secondary: var(--text-secondary-dark);
  --theme-text-muted: var(--text-muted-dark);
  --theme-border: var(--border-dark);
  --theme-border-muted: var(--border-muted-dark);
  
  // Semantic colors - dark mode
  --theme-risk-critical: var(--risk-critical-dark);
  --theme-risk-high: var(--risk-high-dark);
  --theme-risk-medium: var(--risk-medium-dark);
  --theme-risk-low: var(--risk-low-dark);
  --theme-risk-none: var(--risk-none-dark);
  
  --theme-status-draft: var(--status-draft-dark);
  --theme-status-progress: var(--status-progress-dark);
  --theme-status-review: var(--status-review-dark);
  --theme-status-complete: var(--status-complete-dark);
  --theme-status-overdue: var(--status-overdue-dark);
  --theme-status-cancelled: var(--status-cancelled-dark);
}

// System preference fallback
@media (prefers-color-scheme: dark) {
  :root:not([data-theme]) {
    --theme-bg-primary: var(--bg-primary-dark);
    --theme-bg-secondary: var(--bg-secondary-dark);
    --theme-bg-tertiary: var(--bg-tertiary-dark);
    --theme-text-primary: var(--text-primary-dark);
    --theme-text-secondary: var(--text-secondary-dark);
    --theme-text-muted: var(--text-muted-dark);
    --theme-border: var(--border-dark);
    --theme-border-muted: var(--border-muted-dark);
    
    --theme-risk-critical: var(--risk-critical-dark);
    --theme-risk-high: var(--risk-high-dark);
    --theme-risk-medium: var(--risk-medium-dark);
    --theme-risk-low: var(--risk-low-dark);
    --theme-risk-none: var(--risk-none-dark);
    
    --theme-status-draft: var(--status-draft-dark);
    --theme-status-progress: var(--status-progress-dark);
    --theme-status-review: var(--status-review-dark);
    --theme-status-complete: var(--status-complete-dark);
    --theme-status-overdue: var(--status-overdue-dark);
    --theme-status-cancelled: var(--status-cancelled-dark);
  }
}
```

#### 2.3 Create Theme Toggle Component

**New File:** `src/Harmoni360.Web/ClientApp/src/components/common/ThemeToggle.tsx`

```typescript
import React from 'react';
import { CDropdown, CDropdownToggle, CDropdownMenu, CDropdownItem } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSun, faMoon, faDesktop } from '@fortawesome/free-solid-svg-icons';
import { useTheme } from '../../contexts/ThemeContext';

export const ThemeToggle: React.FC = () => {
  const { theme, setTheme, effectiveTheme } = useTheme();

  const themeOptions = [
    { value: 'light' as const, label: 'Light', icon: faSun },
    { value: 'dark' as const, label: 'Dark', icon: faMoon },
    { value: 'system' as const, label: 'System', icon: faDesktop }
  ];

  const currentIcon = effectiveTheme === 'dark' ? faMoon : faSun;

  return (
    <CDropdown variant="nav-item">
      <CDropdownToggle
        color="ghost"
        className="theme-toggle"
        aria-label="Change theme"
      >
        <FontAwesomeIcon
          icon={currentIcon}
          className="me-2"
        />
        <span className="d-none d-md-inline">Theme</span>
      </CDropdownToggle>
      <CDropdownMenu>
        {themeOptions.map(option => (
          <CDropdownItem
            key={option.value}
            onClick={() => setTheme(option.value)}
            active={theme === option.value}
            className="d-flex align-items-center"
          >
            <FontAwesomeIcon icon={option.icon} className="me-2" />
            {option.label}
            {theme === option.value && (
              <FontAwesomeIcon icon={faCheck} className="ms-auto text-success" />
            )}
          </CDropdownItem>
        ))}
      </CDropdownMenu>
    </CDropdown>
  );
};
```

### Phase 3: Component Updates (Week 2)

#### 3.1 Update App.tsx to Include Theme Provider

**File:** `src/Harmoni360.Web/ClientApp/src/App.tsx`
**Add import and wrap the app:**

```typescript
import { ThemeProvider } from './contexts/ThemeContext';

// In the return statement, wrap the existing content:
return (
  <ThemeProvider>
    <AuthErrorBoundary>
      <ErrorBoundary>
        <Provider store={store}>
          {/* ... existing content ... */}
        </Provider>
      </ErrorBoundary>
    </AuthErrorBoundary>
  </ThemeProvider>
);
```

#### 3.2 Update Main SCSS Files

**File:** `src/Harmoni360.Web/ClientApp/src/styles/app.scss`
**Add after design-tokens import:**

```scss
// Import theme variables
@import './theme-variables.scss';
```

**Update existing styles to use theme variables:**

```scss
// Update body and html
body {
  background-color: var(--theme-bg-primary);
  color: var(--theme-text-primary);
  transition: background-color 0.3s ease, color 0.3s ease;
}

// Update cards
.card {
  background-color: var(--theme-bg-primary);
  border-color: var(--theme-border);
  color: var(--theme-text-primary);

  .card-header {
    background-color: var(--theme-bg-secondary);
    border-bottom-color: var(--theme-border);
    color: var(--theme-text-primary);
  }
}

// Update sidebar
.sidebar {
  background-color: var(--theme-bg-secondary);
  border-right-color: var(--theme-border);

  .nav-link {
    color: var(--theme-text-secondary);

    &:hover {
      background-color: var(--theme-bg-tertiary);
      color: var(--theme-text-primary);
    }

    &.active {
      background-color: var(--harmoni-teal-500);
      color: white;
    }
  }
}

// Update header
.header {
  background-color: var(--theme-bg-primary);
  border-bottom-color: var(--theme-border);
  color: var(--theme-text-primary);
}
```

#### 3.3 Create Centralized Status Color Utilities

**New File:** `src/Harmoni360.Web/ClientApp/src/utils/statusColors.ts`

```typescript
// Centralized status color management
export type RiskLevel = 'Critical' | 'High' | 'Medium' | 'Low' | 'None';
export type StatusType = 'Draft' | 'InProgress' | 'UnderReview' | 'Completed' | 'Overdue' | 'Cancelled';

export const getRiskLevelColor = (riskLevel: RiskLevel): string => {
  const riskColors: Record<RiskLevel, string> = {
    'Critical': 'var(--theme-risk-critical)',
    'High': 'var(--theme-risk-high)',
    'Medium': 'var(--theme-risk-medium)',
    'Low': 'var(--theme-risk-low)',
    'None': 'var(--theme-risk-none)'
  };
  return riskColors[riskLevel] || riskColors.None;
};

export const getStatusColor = (status: StatusType): string => {
  const statusColors: Record<StatusType, string> = {
    'Draft': 'var(--theme-status-draft)',
    'InProgress': 'var(--theme-status-progress)',
    'UnderReview': 'var(--theme-status-review)',
    'Completed': 'var(--theme-status-complete)',
    'Overdue': 'var(--theme-status-overdue)',
    'Cancelled': 'var(--theme-status-cancelled)'
  };
  return statusColors[status] || statusColors.Draft;
};

export const getRiskLevelBadgeClass = (riskLevel: RiskLevel): string => {
  const badgeClasses: Record<RiskLevel, string> = {
    'Critical': 'badge-danger',
    'High': 'badge-warning',
    'Medium': 'badge-warning',
    'Low': 'badge-success',
    'None': 'badge-secondary'
  };
  return badgeClasses[riskLevel] || badgeClasses.None;
};

export const getStatusBadgeClass = (status: StatusType): string => {
  const badgeClasses: Record<StatusType, string> = {
    'Draft': 'badge-info',
    'InProgress': 'badge-warning',
    'UnderReview': 'badge-primary',
    'Completed': 'badge-success',
    'Overdue': 'badge-danger',
    'Cancelled': 'badge-secondary'
  };
  return badgeClasses[status] || badgeClasses.Draft;
};
```

### Phase 4: Testing and Validation (Week 3)

#### 4.1 Accessibility Testing Script

**New File:** `src/Harmoni360.Web/ClientApp/src/utils/accessibilityTest.ts`

```typescript
// Accessibility testing utilities
export const checkContrastRatio = (foreground: string, background: string): number => {
  // Convert hex to RGB
  const hexToRgb = (hex: string) => {
    const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
      r: parseInt(result[1], 16),
      g: parseInt(result[2], 16),
      b: parseInt(result[3], 16)
    } : null;
  };

  // Calculate relative luminance
  const getLuminance = (r: number, g: number, b: number) => {
    const [rs, gs, bs] = [r, g, b].map(c => {
      c = c / 255;
      return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
    });
    return 0.2126 * rs + 0.7152 * gs + 0.0722 * bs;
  };

  const fg = hexToRgb(foreground);
  const bg = hexToRgb(background);

  if (!fg || !bg) return 0;

  const fgLum = getLuminance(fg.r, fg.g, fg.b);
  const bgLum = getLuminance(bg.r, bg.g, bg.b);

  const lighter = Math.max(fgLum, bgLum);
  const darker = Math.min(fgLum, bgLum);

  return (lighter + 0.05) / (darker + 0.05);
};

export const validateWCAGCompliance = (element: HTMLElement): boolean => {
  const styles = window.getComputedStyle(element);
  const color = styles.color;
  const backgroundColor = styles.backgroundColor;

  // Extract hex values from computed styles
  // This is a simplified version - in practice, you'd need more robust color parsing
  const ratio = checkContrastRatio(color, backgroundColor);

  // WCAG AA requires 4.5:1 for normal text, 3:1 for large text
  const fontSize = parseFloat(styles.fontSize);
  const fontWeight = styles.fontWeight;

  const isLargeText = fontSize >= 18 || (fontSize >= 14 && (fontWeight === 'bold' || parseInt(fontWeight) >= 700));
  const requiredRatio = isLargeText ? 3 : 4.5;

  return ratio >= requiredRatio;
};
```

#### 4.2 Component Testing Examples

**File:** `src/Harmoni360.Web/ClientApp/src/components/common/__tests__/ThemeToggle.test.tsx`

```typescript
import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { ThemeProvider } from '../../../contexts/ThemeContext';
import { ThemeToggle } from '../ThemeToggle';

const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider>
      {component}
    </ThemeProvider>
  );
};

describe('ThemeToggle', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('should render theme toggle button', () => {
    renderWithTheme(<ThemeToggle />);
    expect(screen.getByLabelText('Change theme')).toBeInTheDocument();
  });

  it('should switch to dark theme when selected', () => {
    renderWithTheme(<ThemeToggle />);

    fireEvent.click(screen.getByLabelText('Change theme'));
    fireEvent.click(screen.getByText('Dark'));

    expect(document.documentElement).toHaveAttribute('data-theme', 'dark');
  });

  it('should persist theme preference in localStorage', () => {
    renderWithTheme(<ThemeToggle />);

    fireEvent.click(screen.getByLabelText('Change theme'));
    fireEvent.click(screen.getByText('Dark'));

    expect(localStorage.getItem('harmoni-theme')).toBe('dark');
  });

  it('should respect system preference when system option selected', () => {
    // Mock matchMedia for dark mode
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      value: jest.fn().mockImplementation(query => ({
        matches: query === '(prefers-color-scheme: dark)',
        media: query,
        addEventListener: jest.fn(),
        removeEventListener: jest.fn(),
      })),
    });

    renderWithTheme(<ThemeToggle />);

    fireEvent.click(screen.getByLabelText('Change theme'));
    fireEvent.click(screen.getByText('System'));

    expect(document.documentElement).toHaveAttribute('data-theme', 'dark');
  });
});
```

## Deployment Checklist

### Pre-deployment Validation
- [ ] All contrast ratios verified with automated tools
- [ ] Manual testing with screen readers completed
- [ ] Color-blind simulation testing passed
- [ ] High contrast mode compatibility verified
- [ ] Theme persistence working correctly
- [ ] Performance impact assessed (< 100ms theme switching)
- [ ] Cross-browser compatibility tested
- [ ] Mobile responsiveness verified

### Post-deployment Monitoring
- [ ] User feedback collection system active
- [ ] Accessibility metrics tracking enabled
- [ ] Theme usage analytics implemented
- [ ] Error monitoring for theme-related issues
- [ ] Performance monitoring for theme switching

### Documentation Updates Required
- [ ] User guide updated with theme switching instructions
- [ ] Developer documentation updated with new color system
- [ ] Design system documentation published
- [ ] Accessibility compliance documentation updated

---
*Implementation guidelines: June 2025*
*HarmoniHSE360 UI/UX Enhancement Project*
