import React from 'react';
import { CDropdown, CDropdownToggle, CDropdownMenu, CDropdownItem } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSun, faMoon, faDesktop, faCheck } from '@fortawesome/free-solid-svg-icons';
import { useTheme } from '../../contexts/ThemeContext';

// Theme option interface
interface ThemeOption {
  value: 'light' | 'dark' | 'system';
  label: string;
  icon: typeof faSun;
}

export const ThemeToggle: React.FC = () => {
  const { theme, setTheme, effectiveTheme } = useTheme();

  // Available theme options
  const themeOptions: ThemeOption[] = [
    { value: 'light', label: 'Light', icon: faSun },
    { value: 'dark', label: 'Dark', icon: faMoon },
    { value: 'system', label: 'System', icon: faDesktop }
  ];

  // Get current icon based on effective theme
  const currentIcon = effectiveTheme === 'dark' ? faMoon : faSun;

  return (
    <CDropdown variant="nav-item" className="theme-toggle-dropdown">
      <CDropdownToggle
        color="ghost"
        caret={false}
        className="theme-toggle"
        aria-label="Change theme"
        title="Change theme"
      >
        <FontAwesomeIcon
          icon={currentIcon}
          className="me-2"
          aria-hidden="true"
        />
        <span className="d-none d-md-inline">Theme</span>
      </CDropdownToggle>
      <CDropdownMenu className="theme-dropdown-menu">
        <h6 className="dropdown-header">Choose theme</h6>
        {themeOptions.map(option => (
          <CDropdownItem
            key={option.value}
            onClick={() => setTheme(option.value)}
            active={theme === option.value}
            className="d-flex align-items-center justify-content-between theme-option"
          >
            <div className="d-flex align-items-center">
              <FontAwesomeIcon 
                icon={option.icon} 
                className="me-2" 
                fixedWidth
                aria-hidden="true"
              />
              <span>{option.label}</span>
            </div>
            {theme === option.value && (
              <FontAwesomeIcon 
                icon={faCheck} 
                className="text-success ms-2" 
                aria-label="Selected"
              />
            )}
          </CDropdownItem>
        ))}
        <div className="dropdown-divider" />
        <div className="px-3 py-1 text-muted small">
          <small>
            Current: {effectiveTheme === 'dark' ? 'Dark' : 'Light'} mode
            {theme === 'system' && ' (System)'}
          </small>
        </div>
      </CDropdownMenu>
    </CDropdown>
  );
};

// Inline styles for the theme toggle
const themeToggleStyles = `
  .theme-toggle-dropdown {
    margin-right: 1rem;
  }

  .theme-toggle {
    padding: 0.375rem 0.75rem;
    border-radius: 0.25rem;
    transition: background-color 0.15s ease-in-out;
  }

  .theme-toggle:hover {
    background-color: rgba(0, 151, 167, 0.1);
  }

  .theme-toggle:focus {
    box-shadow: 0 0 0 0.2rem rgba(0, 151, 167, 0.25);
  }

  .theme-dropdown-menu {
    min-width: 200px;
  }

  .theme-option {
    padding: 0.5rem 1rem;
    transition: background-color 0.15s ease-in-out;
  }

  .theme-option:hover {
    background-color: var(--theme-bg-tertiary);
  }

  .theme-option.active {
    background-color: rgba(0, 151, 167, 0.1);
    color: var(--harmoni-teal-600);
  }

  [data-theme="dark"] .theme-toggle {
    color: var(--theme-text-primary);
  }

  [data-theme="dark"] .theme-toggle:hover {
    background-color: rgba(99, 179, 237, 0.1);
  }

  [data-theme="dark"] .theme-option.active {
    background-color: rgba(99, 179, 237, 0.1);
    color: var(--status-draft-dark);
  }
`;

// Add styles to document if not already present
if (typeof document !== 'undefined' && !document.getElementById('theme-toggle-styles')) {
  const styleElement = document.createElement('style');
  styleElement.id = 'theme-toggle-styles';
  styleElement.textContent = themeToggleStyles;
  document.head.appendChild(styleElement);
}

export default ThemeToggle;