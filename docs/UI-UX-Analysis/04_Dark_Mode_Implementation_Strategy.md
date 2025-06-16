# Dark Mode Implementation Strategy for HarmoniHSE360

## Executive Summary

This document outlines a comprehensive strategy for implementing dark mode in HarmoniHSE360, including technical implementation, user preference management, and accessibility considerations specific to HSSE applications.

## Current State Assessment

### Existing Dark Mode Implementation
**Location:** `src/Harmoni360.Web/ClientApp/src/styles/hsse-dashboard.css`

**Current Implementation (Limited):**
```css
@media (prefers-color-scheme: dark) {
  .card {
    background-color: #2d3748;
    border-color: #4a5568;
  }
  
  .text-muted {
    color: #a0aec0 !important;
  }
  
  .bg-light {
    background-color: #4a5568 !important;
  }
}
```

**Issues with Current Implementation:**
- Only covers cards and muted text
- No user control over theme preference
- Missing dark variants for most UI elements
- Potential contrast violations not addressed
- No systematic approach to color management

## Dark Mode Design Principles for HSSE

### 1. Safety-Critical Visibility
- **Emergency information** must remain highly visible in dark mode
- **Risk indicators** must maintain semantic meaning and contrast
- **Status colors** must be clearly distinguishable
- **Critical alerts** must have maximum visibility

### 2. Professional Appearance
- **Consistent with brand identity** while adapting to dark theme
- **Suitable for regulatory compliance** documentation and audits
- **Professional appearance** for management and stakeholder presentations
- **Reduced eye strain** for extended use during incident management

### 3. Accessibility Compliance
- **WCAG 2.1 AA compliance** maintained in dark mode
- **Color-blind accessibility** preserved with alternative indicators
- **High contrast mode** compatibility
- **Reduced motion** preferences respected

## Technical Implementation Strategy

### 1. CSS Custom Properties Approach

#### Theme Variable Structure
```css
:root {
  /* Light mode (default) */
  --theme-bg-primary: var(--bg-primary-light);
  --theme-bg-secondary: var(--bg-secondary-light);
  --theme-bg-tertiary: var(--bg-tertiary-light);
  --theme-text-primary: var(--text-primary-light);
  --theme-text-secondary: var(--text-secondary-light);
  --theme-text-muted: var(--text-muted-light);
  --theme-border: var(--border-light);
  --theme-border-muted: var(--border-muted-light);
  
  /* Semantic colors - maintain in both modes */
  --theme-risk-critical: var(--risk-critical-light);
  --theme-risk-high: var(--risk-high-light);
  --theme-risk-medium: var(--risk-medium-light);
  --theme-risk-low: var(--risk-low-light);
  --theme-risk-none: var(--risk-none-light);
}

[data-theme="dark"] {
  /* Dark mode overrides */
  --theme-bg-primary: var(--bg-primary-dark);
  --theme-bg-secondary: var(--bg-secondary-dark);
  --theme-bg-tertiary: var(--bg-tertiary-dark);
  --theme-text-primary: var(--text-primary-dark);
  --theme-text-secondary: var(--text-secondary-dark);
  --theme-text-muted: var(--text-muted-dark);
  --theme-border: var(--border-dark);
  --theme-border-muted: var(--border-muted-dark);
  
  /* Dark mode semantic colors */
  --theme-risk-critical: var(--risk-critical-dark);
  --theme-risk-high: var(--risk-high-dark);
  --theme-risk-medium: var(--risk-medium-dark);
  --theme-risk-low: var(--risk-low-dark);
  --theme-risk-none: var(--risk-none-dark);
}
```

#### System Preference Detection
```css
@media (prefers-color-scheme: dark) {
  :root:not([data-theme]) {
    /* Apply dark theme when no explicit preference set */
    --theme-bg-primary: var(--bg-primary-dark);
    --theme-bg-secondary: var(--bg-secondary-dark);
    --theme-text-primary: var(--text-primary-dark);
    /* ... other dark mode variables */
  }
}
```

### 2. React Implementation Strategy

#### Theme Context Provider
```typescript
// src/contexts/ThemeContext.tsx
interface ThemeContextType {
  theme: 'light' | 'dark' | 'system';
  setTheme: (theme: 'light' | 'dark' | 'system') => void;
  effectiveTheme: 'light' | 'dark';
}

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [theme, setTheme] = useState<'light' | 'dark' | 'system'>('system');
  const [systemTheme, setSystemTheme] = useState<'light' | 'dark'>('light');
  
  // Listen for system theme changes
  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    setSystemTheme(mediaQuery.matches ? 'dark' : 'light');
    
    const handleChange = (e: MediaQueryListEvent) => {
      setSystemTheme(e.matches ? 'dark' : 'light');
    };
    
    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, []);
  
  // Apply theme to document
  useEffect(() => {
    const effectiveTheme = theme === 'system' ? systemTheme : theme;
    document.documentElement.setAttribute('data-theme', effectiveTheme);
  }, [theme, systemTheme]);
  
  const effectiveTheme = theme === 'system' ? systemTheme : theme;
  
  return (
    <ThemeContext.Provider value={{ theme, setTheme, effectiveTheme }}>
      {children}
    </ThemeContext.Provider>
  );
};
```

#### Theme Toggle Component
```typescript
// src/components/common/ThemeToggle.tsx
export const ThemeToggle: React.FC = () => {
  const { theme, setTheme, effectiveTheme } = useTheme();
  
  const themeOptions = [
    { value: 'light', label: 'Light', icon: faSun },
    { value: 'dark', label: 'Dark', icon: faMoon },
    { value: 'system', label: 'System', icon: faDesktop }
  ];
  
  return (
    <CDropdown>
      <CDropdownToggle color="ghost" className="theme-toggle">
        <FontAwesomeIcon 
          icon={effectiveTheme === 'dark' ? faMoon : faSun} 
          className="me-2" 
        />
        Theme
      </CDropdownToggle>
      <CDropdownMenu>
        {themeOptions.map(option => (
          <CDropdownItem
            key={option.value}
            onClick={() => setTheme(option.value as any)}
            active={theme === option.value}
          >
            <FontAwesomeIcon icon={option.icon} className="me-2" />
            {option.label}
          </CDropdownItem>
        ))}
      </CDropdownMenu>
    </CDropdown>
  );
};
```

### 3. Component-Level Implementation

#### Updated Base Styles
```scss
// src/styles/themes.scss
.card {
  background-color: var(--theme-bg-primary);
  border-color: var(--theme-border);
  color: var(--theme-text-primary);
  
  .card-header {
    background-color: var(--theme-bg-secondary);
    border-bottom-color: var(--theme-border);
    color: var(--theme-text-primary);
  }
  
  .card-body {
    color: var(--theme-text-primary);
  }
}

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

.header {
  background-color: var(--theme-bg-primary);
  border-bottom-color: var(--theme-border);
  color: var(--theme-text-primary);
}
```

#### Status Badge Updates
```scss
// Fix for the critical contrast issue
.badge {
  &.badge-info {
    background-color: var(--info-bg-light);
    color: var(--info-text-light);
    
    [data-theme="dark"] & {
      background-color: var(--info-bg-dark);
      color: var(--info-text-dark);
    }
  }
  
  &.badge-danger {
    background-color: var(--theme-risk-critical);
    color: white;
  }
  
  &.badge-warning {
    background-color: var(--theme-risk-medium);
    color: var(--theme-text-primary);
  }
  
  &.badge-success {
    background-color: var(--theme-risk-low);
    color: white;
  }
}
```

## User Experience Design

### 1. Theme Preference Management

#### User Settings Integration
```typescript
// Integration with user preferences API
interface UserPreferences {
  theme: 'light' | 'dark' | 'system';
  highContrast: boolean;
  reducedMotion: boolean;
  colorBlindMode: boolean;
}

export const useUserPreferences = () => {
  const [preferences, setPreferences] = useState<UserPreferences>();
  
  // Load preferences from API
  useEffect(() => {
    userApi.getPreferences().then(setPreferences);
  }, []);
  
  // Save preferences to API
  const updatePreferences = useCallback(async (updates: Partial<UserPreferences>) => {
    const newPreferences = { ...preferences, ...updates };
    await userApi.updatePreferences(newPreferences);
    setPreferences(newPreferences);
  }, [preferences]);
  
  return { preferences, updatePreferences };
};
```

#### Persistent Storage
```typescript
// Local storage fallback for theme preference
export const useThemeStorage = () => {
  const [storedTheme, setStoredTheme] = useState<string>(() => {
    return localStorage.getItem('harmoni-theme') || 'system';
  });
  
  const updateStoredTheme = useCallback((theme: string) => {
    localStorage.setItem('harmoni-theme', theme);
    setStoredTheme(theme);
  }, []);
  
  return { storedTheme, updateStoredTheme };
};
```

### 2. Accessibility Enhancements

#### High Contrast Mode Support
```css
@media (prefers-contrast: high) {
  :root {
    /* Enhanced contrast ratios for high contrast mode */
    --theme-text-primary: #000000;
    --theme-bg-primary: #FFFFFF;
    --theme-border: #000000;
  }
  
  [data-theme="dark"] {
    --theme-text-primary: #FFFFFF;
    --theme-bg-primary: #000000;
    --theme-border: #FFFFFF;
  }
  
  /* Ensure critical elements have maximum contrast */
  .btn-primary {
    background-color: #000000 !important;
    color: #FFFFFF !important;
    border: 2px solid #FFFFFF !important;
  }
  
  .alert-danger {
    background-color: #000000 !important;
    color: #FFFFFF !important;
    border: 3px solid #FFFFFF !important;
  }
}
```

#### Reduced Motion Support
```css
@media (prefers-reduced-motion: reduce) {
  [data-theme] {
    transition: none !important;
  }
  
  .theme-toggle {
    animation: none !important;
  }
  
  .card {
    transition: none !important;
  }
}
```

## Implementation Phases

### Phase 1: Foundation (Week 1)
1. **Create theme CSS custom properties**
2. **Implement ThemeContext and provider**
3. **Fix critical contrast violations**
4. **Add basic theme toggle component**

### Phase 2: Core Components (Week 2)
1. **Update sidebar and navigation**
2. **Update cards and content areas**
3. **Update forms and inputs**
4. **Update buttons and interactive elements**

### Phase 3: Module-Specific Updates (Week 3)
1. **Update incident management components**
2. **Update hazard reporting interfaces**
3. **Update audit management screens**
4. **Update PPE management interfaces**

### Phase 4: Advanced Features (Week 4)
1. **Implement user preference persistence**
2. **Add high contrast mode support**
3. **Comprehensive accessibility testing**
4. **Performance optimization**

## Testing Strategy

### 1. Automated Testing
```typescript
// Theme switching tests
describe('Theme Management', () => {
  it('should apply dark theme when selected', () => {
    render(<App />, { wrapper: ThemeProvider });
    
    fireEvent.click(screen.getByText('Dark'));
    
    expect(document.documentElement).toHaveAttribute('data-theme', 'dark');
  });
  
  it('should respect system preference', () => {
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      value: jest.fn().mockImplementation(query => ({
        matches: query === '(prefers-color-scheme: dark)',
        media: query,
        addEventListener: jest.fn(),
        removeEventListener: jest.fn(),
      })),
    });
    
    render(<App />, { wrapper: ThemeProvider });
    
    expect(document.documentElement).toHaveAttribute('data-theme', 'dark');
  });
});
```

### 2. Visual Regression Testing
- **Screenshot comparison** between light and dark modes
- **Contrast ratio verification** for all color combinations
- **Color-blind simulation** testing
- **High contrast mode** validation

### 3. User Acceptance Testing
- **HSSE manager workflow** testing in both themes
- **Emergency scenario** testing for visibility
- **Extended use** testing for eye strain
- **Accessibility user** testing with screen readers

---
*Dark mode strategy designed: June 2025*
*HarmoniHSE360 UI/UX Enhancement Project*
