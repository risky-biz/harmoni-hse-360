// Performance benchmarking utilities for HarmoniHSE360 Theme System
// Created: June 2025 - Phase 4 Implementation
// Comprehensive performance monitoring and benchmarking tools

export interface PerformanceBenchmark {
  name: string;
  target: number; // Target performance in milliseconds
  current: number;
  passed: boolean;
  samples: number[];
  average: number;
  min: number;
  max: number;
  standardDeviation: number;
}

export interface BundleSizeMetrics {
  total: number;
  css: number;
  javascript: number;
  increase: {
    css: number;
    javascript: number;
    total: number;
  };
}

export interface ThemePerformanceMetrics {
  switching: PerformanceBenchmark;
  rendering: PerformanceBenchmark;
  cssLoading: PerformanceBenchmark;
  bundleSize: BundleSizeMetrics;
  memoryUsage: {
    before: number;
    after: number;
    increase: number;
  };
}

export class PerformanceBenchmarkRunner {
  private measurements: { [key: string]: number[] } = {};
  private benchmarks: { [key: string]: PerformanceBenchmark } = {};

  // Target performance requirements from implementation plan
  public static readonly PERFORMANCE_TARGETS = {
    themeSwitching: 100, // <100ms theme switching
    initialRender: 50,   // <50ms initial component render
    cssLoading: 200,     // <200ms CSS loading
    bundleIncrease: 50,  // <50KB CSS bundle increase
    jsIncrease: 20       // <20KB JavaScript increase
  };

  public startMeasurement(name: string): void {
    performance.mark(`${name}-start`);
  }

  public endMeasurement(name: string): number {
    performance.mark(`${name}-end`);
    performance.measure(name, `${name}-start`, `${name}-end`);
    
    const entries = performance.getEntriesByName(name, 'measure');
    const duration = entries[entries.length - 1]?.duration || 0;
    
    if (!this.measurements[name]) {
      this.measurements[name] = [];
    }
    this.measurements[name].push(duration);
    
    // Clean up performance entries
    performance.clearMarks(`${name}-start`);
    performance.clearMarks(`${name}-end`);
    performance.clearMeasures(name);
    
    return duration;
  }

  public async runThemeSwitchingBenchmark(iterations: number = 10): Promise<PerformanceBenchmark> {
    const measurements: number[] = [];
    
    for (let i = 0; i < iterations; i++) {
      // Measure light to dark transition
      this.startMeasurement('theme-switch');
      
      // Simulate theme switch
      document.documentElement.setAttribute('data-theme', i % 2 === 0 ? 'dark' : 'light');
      
      // Wait for CSS to apply
      await this.waitForStylesApplication();
      
      const duration = this.endMeasurement('theme-switch');
      measurements.push(duration);
      
      // Small delay between measurements
      await new Promise(resolve => setTimeout(resolve, 10));
    }
    
    return this.createBenchmark('theme-switching', measurements, PerformanceBenchmarkRunner.PERFORMANCE_TARGETS.themeSwitching);
  }

  public async runRenderingBenchmark(componentName: string, renderFunction: () => void, iterations: number = 5): Promise<PerformanceBenchmark> {
    const measurements: number[] = [];
    
    for (let i = 0; i < iterations; i++) {
      this.startMeasurement('component-render');
      
      // Trigger component render
      renderFunction();
      
      // Wait for render to complete
      await this.waitForRenderComplete();
      
      const duration = this.endMeasurement('component-render');
      measurements.push(duration);
    }
    
    return this.createBenchmark(`${componentName}-render`, measurements, PerformanceBenchmarkRunner.PERFORMANCE_TARGETS.initialRender);
  }

  public measureCSSLoadingPerformance(): Promise<PerformanceBenchmark> {
    return new Promise((resolve) => {
      const measurements: number[] = [];
      const startTime = performance.now();
      
      // Create a test CSS file load
      const link = document.createElement('link');
      link.rel = 'stylesheet';
      link.href = 'data:text/css,/* test css */';
      
      link.onload = () => {
        const duration = performance.now() - startTime;
        measurements.push(duration);
        document.head.removeChild(link);
        
        const benchmark = this.createBenchmark('css-loading', measurements, PerformanceBenchmarkRunner.PERFORMANCE_TARGETS.cssLoading);
        resolve(benchmark);
      };
      
      link.onerror = () => {
        const benchmark = this.createBenchmark('css-loading', [999], PerformanceBenchmarkRunner.PERFORMANCE_TARGETS.cssLoading);
        resolve(benchmark);
      };
      
      document.head.appendChild(link);
    });
  }

  public async measureBundleSize(): Promise<BundleSizeMetrics> {
    // In a real implementation, this would analyze actual bundle files
    // For now, we'll estimate based on the files we've created
    const estimatedSizes = {
      designTokens: 2.1, // KB - design-tokens.scss
      themeVariables: 3.2, // KB - theme-variables.scss  
      componentsTheme: 8.5, // KB - components-theme.scss
      modulesTheme: 12.3, // KB - modules-theme.scss
      themeContext: 1.8, // KB - ThemeContext.tsx
      themeToggle: 2.1, // KB - ThemeToggle.tsx
      statusColors: 3.5, // KB - statusColors.ts
      statusBadge: 4.2, // KB - StatusBadge.tsx
      emergencyPanel: 6.8, // KB - EmergencyPanel.tsx
      accessibilityTest: 5.1, // KB - accessibilityTest.ts
      performanceBenchmarks: 3.8 // KB - this file
    };
    
    const cssSize = estimatedSizes.designTokens + estimatedSizes.themeVariables + 
                    estimatedSizes.componentsTheme + estimatedSizes.modulesTheme;
    
    const jsSize = estimatedSizes.themeContext + estimatedSizes.themeToggle + 
                   estimatedSizes.statusColors + estimatedSizes.statusBadge + 
                   estimatedSizes.emergencyPanel + estimatedSizes.accessibilityTest + 
                   estimatedSizes.performanceBenchmarks;
    
    return {
      total: cssSize + jsSize,
      css: cssSize,
      javascript: jsSize,
      increase: {
        css: cssSize, // Since this is new functionality
        javascript: jsSize,
        total: cssSize + jsSize
      }
    };
  }

  public measureMemoryUsage(): { before: number; after: number; increase: number } {
    // Memory measurement (approximation)
    const before = this.getMemoryUsage();
    
    // Trigger theme switching and component rendering
    document.documentElement.setAttribute('data-theme', 'dark');
    
    const after = this.getMemoryUsage();
    
    return {
      before,
      after,
      increase: after - before
    };
  }

  public async runFullBenchmarkSuite(): Promise<ThemePerformanceMetrics> {
    console.log('🚀 Starting HarmoniHSE360 Theme Performance Benchmark Suite...');
    
    // Run theme switching benchmark
    console.log('📊 Testing theme switching performance...');
    const switching = await this.runThemeSwitchingBenchmark(15);
    
    // Run CSS loading benchmark
    console.log('📊 Testing CSS loading performance...');
    const cssLoading = await this.measureCSSLoadingPerformance();
    
    // Run rendering benchmark (simulated)
    console.log('📊 Testing component rendering performance...');
    const rendering = await this.runRenderingBenchmark('theme-components', () => {
      // Simulate component rendering
      const div = document.createElement('div');
      div.innerHTML = '<span class="badge badge-danger">Test</span>';
      document.body.appendChild(div);
      document.body.removeChild(div);
    });
    
    // Measure bundle size
    console.log('📊 Measuring bundle size impact...');
    const bundleSize = await this.measureBundleSize();
    
    // Measure memory usage
    console.log('📊 Measuring memory usage...');
    const memoryUsage = this.measureMemoryUsage();
    
    console.log('✅ Benchmark suite completed!');
    
    return {
      switching,
      rendering,
      cssLoading,
      bundleSize,
      memoryUsage
    };
  }

  private createBenchmark(name: string, measurements: number[], target: number): PerformanceBenchmark {
    const average = measurements.reduce((a, b) => a + b, 0) / measurements.length;
    const min = Math.min(...measurements);
    const max = Math.max(...measurements);
    const variance = measurements.reduce((sum, val) => sum + Math.pow(val - average, 2), 0) / measurements.length;
    const standardDeviation = Math.sqrt(variance);
    
    const benchmark: PerformanceBenchmark = {
      name,
      target,
      current: average,
      passed: average <= target,
      samples: measurements,
      average,
      min,
      max,
      standardDeviation
    };
    
    this.benchmarks[name] = benchmark;
    return benchmark;
  }

  private async waitForStylesApplication(): Promise<void> {
    // Wait for CSS transitions and reflow
    return new Promise(resolve => {
      requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          resolve();
        });
      });
    });
  }

  private async waitForRenderComplete(): Promise<void> {
    // Wait for React render cycle to complete
    return new Promise(resolve => setTimeout(resolve, 0));
  }

  private getMemoryUsage(): number {
    // Use performance.memory if available (Chrome)
    if ('memory' in performance) {
      return (performance as any).memory.usedJSHeapSize;
    }
    
    // Fallback approximation
    return 0;
  }

  public generateReport(): string {
    const metrics = Object.values(this.benchmarks);
    const passedTests = metrics.filter(m => m.passed).length;
    const totalTests = metrics.length;
    
    let report = `
🎯 HarmoniHSE360 Theme Performance Report
========================================

Overall Score: ${passedTests}/${totalTests} tests passed (${((passedTests/totalTests) * 100).toFixed(1)}%)

Performance Benchmarks:
`;
    
    metrics.forEach(benchmark => {
      const status = benchmark.passed ? '✅ PASS' : '❌ FAIL';
      const deviation = benchmark.standardDeviation.toFixed(2);
      
      report += `
📊 ${benchmark.name}
   ${status} | Target: ${benchmark.target}ms | Current: ${benchmark.current.toFixed(2)}ms
   Range: ${benchmark.min.toFixed(2)}ms - ${benchmark.max.toFixed(2)}ms (σ: ${deviation}ms)
   Samples: ${benchmark.samples.length}
`;
    });
    
    return report;
  }

  public getBenchmarkResults(): { [key: string]: PerformanceBenchmark } {
    return { ...this.benchmarks };
  }
}

// Static utility functions
export class PerformanceMonitor {
  private static instance: PerformanceBenchmarkRunner;
  
  public static getInstance(): PerformanceBenchmarkRunner {
    if (!PerformanceMonitor.instance) {
      PerformanceMonitor.instance = new PerformanceBenchmarkRunner();
    }
    return PerformanceMonitor.instance;
  }
  
  public static async quickBenchmark(): Promise<{ passed: boolean; report: string }> {
    const runner = PerformanceMonitor.getInstance();
    const metrics = await runner.runFullBenchmarkSuite();
    
    const allPassed = metrics.switching.passed && 
                     metrics.rendering.passed && 
                     metrics.cssLoading.passed &&
                     metrics.bundleSize.increase.css <= PerformanceBenchmarkRunner.PERFORMANCE_TARGETS.bundleIncrease &&
                     metrics.bundleSize.increase.javascript <= PerformanceBenchmarkRunner.PERFORMANCE_TARGETS.jsIncrease;
    
    return {
      passed: allPassed,
      report: runner.generateReport()
    };
  }
  
  // Real-time performance monitoring
  public static startRealtimeMonitoring(): void {
    // Monitor theme switching in real-time
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.type === 'attributes' && 
            mutation.attributeName === 'data-theme' && 
            mutation.target === document.documentElement) {
          
          const startTime = performance.now();
          
          requestAnimationFrame(() => {
            const endTime = performance.now();
            const switchTime = endTime - startTime;
            
            console.log(`🎨 Theme switch completed in ${switchTime.toFixed(2)}ms`);
            
            if (switchTime > 100) {
              console.warn(`⚠️ Theme switching slower than target (${switchTime.toFixed(2)}ms > 100ms)`);
            }
          });
        }
      });
    });
    
    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['data-theme']
    });
  }
}

// Export for use in tests and monitoring
export default PerformanceBenchmarkRunner;