// Accessibility testing utilities for HarmoniHSE360
// Created: June 2025 - Phase 4 Implementation
// Runtime accessibility validation and testing helpers

export interface AccessibilityTestResult {
  passed: boolean;
  score: number;
  violations: AccessibilityViolation[];
  recommendations: string[];
}

export interface AccessibilityViolation {
  id: string;
  impact: 'minor' | 'moderate' | 'serious' | 'critical';
  description: string;
  element: string;
  help: string;
  helpUrl: string;
}

export interface ContrastTestResult {
  ratio: number;
  passes: {
    aa: boolean;
    aaa: boolean;
    aaLarge: boolean;
    aaaLarge: boolean;
  };
  foregroundColor: string;
  backgroundColor: string;
}

// Color contrast calculation utilities
export class ContrastChecker {
  private static hexToRgb(hex: string): { r: number; g: number; b: number } | null {
    // Remove hash if present
    hex = hex.replace('#', '');
    
    // Handle 3-character hex codes
    if (hex.length === 3) {
      hex = hex.split('').map(char => char + char).join('');
    }
    
    const result = /^([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
      r: parseInt(result[1], 16),
      g: parseInt(result[2], 16),
      b: parseInt(result[3], 16)
    } : null;
  }

  private static getLuminance(r: number, g: number, b: number): number {
    const [rs, gs, bs] = [r, g, b].map(channel => {
      channel = channel / 255;
      return channel <= 0.03928
        ? channel / 12.92
        : Math.pow((channel + 0.055) / 1.055, 2.4);
    });
    
    return 0.2126 * rs + 0.7152 * gs + 0.0722 * bs;
  }

  private static getContrastRatio(color1: string, color2: string): number {
    const rgb1 = this.hexToRgb(color1);
    const rgb2 = this.hexToRgb(color2);
    
    if (!rgb1 || !rgb2) return 0;
    
    const lum1 = this.getLuminance(rgb1.r, rgb1.g, rgb1.b);
    const lum2 = this.getLuminance(rgb2.r, rgb2.g, rgb2.b);
    
    const brightest = Math.max(lum1, lum2);
    const darkest = Math.min(lum1, lum2);
    
    return (brightest + 0.05) / (darkest + 0.05);
  }

  public static testContrast(foreground: string, background: string): ContrastTestResult {
    const ratio = this.getContrastRatio(foreground, background);
    
    return {
      ratio,
      passes: {
        aa: ratio >= 4.5,
        aaa: ratio >= 7,
        aaLarge: ratio >= 3,
        aaaLarge: ratio >= 4.5
      },
      foregroundColor: foreground,
      backgroundColor: background
    };
  }

  public static validateDesignTokens(): { [key: string]: ContrastTestResult } {
    const tokenTests: { [key: string]: ContrastTestResult } = {};
    
    // Test critical color combinations from design tokens
    const criticalTests = [
      { name: 'risk-critical-light', fg: '#DC3545', bg: '#FFFFFF' },
      { name: 'risk-critical-dark', fg: '#FF6B7A', bg: '#1A202C' },
      { name: 'risk-high-light', fg: '#FF5722', bg: '#FFFFFF' },
      { name: 'risk-high-dark', fg: '#FF8A65', bg: '#1A202C' },
      { name: 'info-light', fg: '#0DCAF0', bg: '#FFFFFF' },
      { name: 'info-dark', fg: '#9EEAF9', bg: '#1A202C' },
      { name: 'text-primary-light', fg: '#212529', bg: '#FFFFFF' },
      { name: 'text-primary-dark', fg: '#F8F9FA', bg: '#1A202C' }
    ];
    
    criticalTests.forEach(test => {
      tokenTests[test.name] = this.testContrast(test.fg, test.bg);
    });
    
    return tokenTests;
  }
}

// Theme switching performance utilities
export class ThemePerformanceMonitor {
  private static measurements: { [key: string]: number[] } = {};

  public static startMeasurement(name: string): void {
    performance.mark(`${name}-start`);
  }

  public static endMeasurement(name: string): number {
    performance.mark(`${name}-end`);
    performance.measure(name, `${name}-start`, `${name}-end`);
    
    const entries = performance.getEntriesByName(name, 'measure');
    const duration = entries[entries.length - 1]?.duration || 0;
    
    if (!this.measurements[name]) {
      this.measurements[name] = [];
    }
    this.measurements[name].push(duration);
    
    // Clean up marks
    performance.clearMarks(`${name}-start`);
    performance.clearMarks(`${name}-end`);
    performance.clearMeasures(name);
    
    return duration;
  }

  public static getAverageTime(name: string): number {
    const times = this.measurements[name] || [];
    return times.length > 0 ? times.reduce((a, b) => a + b, 0) / times.length : 0;
  }

  public static getPerformanceReport(): { [key: string]: { average: number; count: number; max: number; min: number } } {
    const report: { [key: string]: any } = {};
    
    Object.keys(this.measurements).forEach(name => {
      const times = this.measurements[name];
      report[name] = {
        average: this.getAverageTime(name),
        count: times.length,
        max: Math.max(...times),
        min: Math.min(...times)
      };
    });
    
    return report;
  }

  public static clearMeasurements(): void {
    this.measurements = {};
  }
}

// Runtime accessibility validation
export class AccessibilityValidator {
  public static validateElement(element: Element): AccessibilityViolation[] {
    const violations: AccessibilityViolation[] = [];
    
    // Check for missing alt text on images
    if (element.tagName === 'IMG' && !element.getAttribute('alt')) {
      violations.push({
        id: 'image-alt',
        impact: 'serious',
        description: 'Image missing alternative text',
        element: element.tagName.toLowerCase(),
        help: 'Images must have alternative text',
        helpUrl: 'https://www.w3.org/WAI/WCAG21/Understanding/non-text-content.html'
      });
    }
    
    // Check for missing form labels
    if (element.tagName === 'INPUT' && element.getAttribute('type') !== 'hidden') {
      const id = element.getAttribute('id');
      const hasLabel = id && document.querySelector(`label[for="${id}"]`);
      const hasAriaLabel = element.getAttribute('aria-label');
      const hasAriaLabelledBy = element.getAttribute('aria-labelledby');
      
      if (!hasLabel && !hasAriaLabel && !hasAriaLabelledBy) {
        violations.push({
          id: 'form-field-label',
          impact: 'serious',
          description: 'Form field missing accessible label',
          element: element.tagName.toLowerCase(),
          help: 'Form fields must have accessible labels',
          helpUrl: 'https://www.w3.org/WAI/WCAG21/Understanding/labels-or-instructions.html'
        });
      }
    }
    
    // Check for insufficient color contrast
    const computedStyle = window.getComputedStyle(element);
    const color = computedStyle.color;
    const backgroundColor = computedStyle.backgroundColor;
    
    if (color && backgroundColor && color !== 'rgba(0, 0, 0, 0)' && backgroundColor !== 'rgba(0, 0, 0, 0)') {
      // Convert RGB to hex for contrast testing (simplified)
      const textContent = element.textContent?.trim();
      if (textContent && textContent.length > 0) {
        // This is a simplified check - in production, use a proper color contrast library
        const colorValue = this.extractColorValue(color);
        const bgValue = this.extractColorValue(backgroundColor);
        
        if (colorValue && bgValue) {
          const contrastResult = ContrastChecker.testContrast(colorValue, bgValue);
          if (!contrastResult.passes.aa) {
            violations.push({
              id: 'color-contrast',
              impact: 'serious',
              description: `Insufficient color contrast ratio: ${contrastResult.ratio.toFixed(2)}:1`,
              element: element.tagName.toLowerCase(),
              help: 'Elements must have sufficient color contrast',
              helpUrl: 'https://www.w3.org/WAI/WCAG21/Understanding/contrast-minimum.html'
            });
          }
        }
      }
    }
    
    return violations;
  }

  private static extractColorValue(rgbString: string): string | null {
    // Simple RGB to hex conversion - in production, use a proper library
    const match = rgbString.match(/rgb\((\d+),\s*(\d+),\s*(\d+)\)/);
    if (match) {
      const [, r, g, b] = match;
      return `#${[r, g, b].map(x => parseInt(x).toString(16).padStart(2, '0')).join('')}`;
    }
    return null;
  }

  public static validatePage(): AccessibilityTestResult {
    const violations: AccessibilityViolation[] = [];
    const elements = document.querySelectorAll('*');
    
    elements.forEach(element => {
      const elementViolations = this.validateElement(element);
      violations.push(...elementViolations);
    });
    
    const criticalCount = violations.filter(v => v.impact === 'critical').length;
    const seriousCount = violations.filter(v => v.impact === 'serious').length;
    const moderateCount = violations.filter(v => v.impact === 'moderate').length;
    const minorCount = violations.filter(v => v.impact === 'minor').length;
    
    // Calculate score (100 - weighted violation count)
    const score = Math.max(0, 100 - (criticalCount * 25 + seriousCount * 15 + moderateCount * 10 + minorCount * 5));
    
    const recommendations: string[] = [];
    if (criticalCount > 0) recommendations.push('Fix critical accessibility violations immediately');
    if (seriousCount > 0) recommendations.push('Address serious accessibility issues');
    if (moderateCount > 0) recommendations.push('Improve moderate accessibility issues');
    if (violations.length === 0) recommendations.push('Excellent accessibility compliance!');
    
    return {
      passed: criticalCount === 0 && seriousCount === 0,
      score,
      violations,
      recommendations
    };
  }
}

// Theme testing utilities
export class ThemeTestRunner {
  public static async testThemeSwitching(): Promise<{ success: boolean; errors: string[]; performance: any }> {
    const errors: string[] = [];
    let performance: any = {};
    
    try {
      // Test light to dark transition
      ThemePerformanceMonitor.startMeasurement('light-to-dark');
      document.documentElement.setAttribute('data-theme', 'dark');
      await this.waitForTransition();
      const lightToDark = ThemePerformanceMonitor.endMeasurement('light-to-dark');
      
      // Test dark to light transition
      ThemePerformanceMonitor.startMeasurement('dark-to-light');
      document.documentElement.setAttribute('data-theme', 'light');
      await this.waitForTransition();
      const darkToLight = ThemePerformanceMonitor.endMeasurement('dark-to-light');
      
      performance = {
        lightToDark,
        darkToLight,
        average: (lightToDark + darkToLight) / 2
      };
      
      // Validate performance requirements
      if (performance.average > 100) {
        errors.push(`Theme switching too slow: ${performance.average.toFixed(2)}ms (target: <100ms)`);
      }
      
      // Test CSS custom properties are applied
      const rootStyle = getComputedStyle(document.documentElement);
      const bgPrimary = rootStyle.getPropertyValue('--theme-bg-primary').trim();
      
      if (!bgPrimary) {
        errors.push('Theme CSS custom properties not applied correctly');
      }
      
    } catch (error) {
      errors.push(`Theme switching test failed: ${error}`);
    }
    
    return {
      success: errors.length === 0,
      errors,
      performance
    };
  }

  private static waitForTransition(): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, 50));
  }

  public static validateThemeColors(): { [theme: string]: ContrastTestResult[] } {
    const results: { [theme: string]: ContrastTestResult[] } = {};
    
    ['light', 'dark'].forEach(theme => {
      document.documentElement.setAttribute('data-theme', theme);
      
      const rootStyle = getComputedStyle(document.documentElement);
      const testCombinations = [
        { name: 'primary-text-bg', fg: '--theme-text-primary', bg: '--theme-bg-primary' },
        { name: 'secondary-text-bg', fg: '--theme-text-secondary', bg: '--theme-bg-secondary' },
        { name: 'critical-risk', fg: '--theme-risk-critical', bg: '--theme-risk-critical-bg' },
        { name: 'info-colors', fg: '--theme-info-text', bg: '--theme-info-bg' }
      ];
      
      results[theme] = testCombinations.map(combo => {
        const fg = rootStyle.getPropertyValue(combo.fg).trim();
        const bg = rootStyle.getPropertyValue(combo.bg).trim();
        
        if (fg && bg) {
          return ContrastChecker.testContrast(fg, bg);
        }
        
        return {
          ratio: 0,
          passes: { aa: false, aaa: false, aaLarge: false, aaaLarge: false },
          foregroundColor: fg,
          backgroundColor: bg
        };
      });
    });
    
    return results;
  }
}

// Export test runner for external use
export const runAccessibilityTests = async (): Promise<{
  accessibility: AccessibilityTestResult;
  performance: any;
  contrast: any;
  themes: any;
}> => {
  const accessibility = AccessibilityValidator.validatePage();
  const themeTest = await ThemeTestRunner.testThemeSwitching();
  const contrast = ContrastChecker.validateDesignTokens();
  const themes = ThemeTestRunner.validateThemeColors();
  
  return {
    accessibility,
    performance: themeTest.performance,
    contrast,
    themes
  };
};

export default {
  AccessibilityValidator,
  ContrastChecker,
  ThemePerformanceMonitor,
  ThemeTestRunner,
  runAccessibilityTests
};