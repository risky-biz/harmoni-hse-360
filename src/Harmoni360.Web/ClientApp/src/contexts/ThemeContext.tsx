import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';

// Theme mode types
type ThemeMode = 'light' | 'dark' | 'system';

// Theme context interface
interface ThemeContextType {
  theme: ThemeMode;
  setTheme: (theme: ThemeMode) => void;
  effectiveTheme: 'light' | 'dark';
}

// Create the context
const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

// Custom hook to use the theme context
export const useTheme = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};

// Theme Provider component
export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  // Initialize theme - force light mode for now
  const [theme, setThemeState] = useState<ThemeMode>(() => {
    // Temporarily force light mode until theme system improvements
    return 'light';
  });
  
  // System theme detection disabled - always use light
  const [systemTheme, setSystemTheme] = useState<'light' | 'dark'>('light');

  // System theme change listener disabled - keeping light mode only
  useEffect(() => {
    // Temporarily disabled until theme system improvements
    // const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    // const handleChange = (e: MediaQueryListEvent) => {
    //   setSystemTheme(e.matches ? 'dark' : 'light');
    // };
    // mediaQuery.addEventListener('change', handleChange);
    // return () => mediaQuery.removeEventListener('change', handleChange);
  }, []);

  // Calculate effective theme (what's actually applied)
  const effectiveTheme = theme === 'system' ? systemTheme : theme;

  // Apply theme to document
  useEffect(() => {
    // Add transition blocking class
    document.documentElement.classList.add('theme-transition');
    
    // Set the theme attribute
    document.documentElement.setAttribute('data-theme', effectiveTheme);
    
    // Update meta theme-color for mobile browsers
    const metaThemeColor = document.querySelector('meta[name="theme-color"]');
    if (metaThemeColor) {
      metaThemeColor.setAttribute('content', 
        effectiveTheme === 'dark' ? '#1A202C' : '#0097A7'
      );
    } else {
      // Create meta tag if it doesn't exist
      const meta = document.createElement('meta');
      meta.name = 'theme-color';
      meta.content = effectiveTheme === 'dark' ? '#1A202C' : '#0097A7';
      document.head.appendChild(meta);
    }
    
    // Remove transition blocking class after a frame
    requestAnimationFrame(() => {
      document.documentElement.classList.remove('theme-transition');
    });
  }, [effectiveTheme]);

  // Set theme function disabled - keeping light mode only
  const setTheme = useCallback((newTheme: ThemeMode) => {
    // Temporarily disabled until theme system improvements
    console.log('Theme switching temporarily disabled. Keeping light mode.');
    // setThemeState(newTheme);
    // localStorage.setItem('harmoni-theme', newTheme);
  }, []);

  // Context value
  const contextValue: ThemeContextType = {
    theme,
    setTheme,
    effectiveTheme
  };

  return (
    <ThemeContext.Provider value={contextValue}>
      {children}
    </ThemeContext.Provider>
  );
};

// Export everything
export default ThemeContext;