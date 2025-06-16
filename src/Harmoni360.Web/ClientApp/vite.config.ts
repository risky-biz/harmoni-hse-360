/// <reference types="vitest" />
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';
import path from 'path';

export default defineConfig({
  base: '/',
  publicDir: 'public',
  css: {
    preprocessorOptions: {
      scss: {
        api: 'modern-compiler', // This uses the modern Sass JS API
        silenceDeprecations: ['mixed-decls', 'color-functions', 'global-builtin', 'import']
      }
    }
  },
  optimizeDeps: {
    exclude: ['@coreui/icons'] // Exclude problematic dependencies from optimization
  },
  esbuild: {
    logOverride: { 
      'this-is-undefined-in-esm': 'silent',
      'direct-eval': 'silent'
    },
    jsxDev: false
  },
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      manifest: {
        name: 'Harmoni360',
        short_name: 'HSE360',
        theme_color: '#0097A7',
        background_color: '#ffffff',
        display: 'standalone',
        start_url: '/',
        icons: [
          {
            src: '/icon-192.png',
            sizes: '192x192',
            type: 'image/png'
          },
          {
            src: '/icon-512.png',
            sizes: '512x512',
            type: 'image/png'
          }
        ]
      }
    })
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    }
  },
  server: {
    host: true, // Listen on all interfaces
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/hubs': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
        ws: true
      }
    }
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    css: true,
    reporter: ['verbose'],
    coverage: {
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'src/test/',
        '**/*.d.ts',
        '**/*.config.*',
        '**/dist/**',
        '**/*.test.*',
        '**/*.spec.*',
      ],
    },
  },
  build: {
    outDir: 'dist',
    sourcemap: process.env.NODE_ENV === 'development',
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,
        drop_debugger: true,
      },
    },
    copyPublicDir: true,
    rollupOptions: {
      onwarn(warning, warn) {
        // Ignore certain warnings
        if (warning.message?.includes('sourcemap') || 
            warning.message?.includes('Unexpected end of file')) {
          return;
        }
        warn(warning);
      },
      output: {
        manualChunks: {
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          'redux-vendor': ['@reduxjs/toolkit', 'react-redux'],
          'coreui-vendor': ['@coreui/react', '@coreui/icons-react'],
          'fontawesome-vendor': ['@fortawesome/react-fontawesome', '@fortawesome/free-solid-svg-icons'],
          'chart-vendor': ['chart.js', 'react-chartjs-2'],
          'health-pages': [
            './src/pages/health/HealthDashboard',
            './src/pages/health/HealthList',
            './src/pages/health/HealthDetail'
          ],
          'incident-pages': [
            './src/pages/incidents/IncidentDashboard',
            './src/pages/incidents/IncidentList',
            './src/pages/incidents/IncidentDetail'
          ],
          'ppe-pages': [
            './src/pages/ppe/PPEDashboard',
            './src/pages/ppe/PPEList',
            './src/pages/ppe/PPEDetail'
          ],
          'hazard-pages': [
            './src/pages/hazards/HazardDashboard',
            './src/pages/hazards/HazardList',
            './src/pages/hazards/HazardDetail'
          ],
          'training-pages': [
            './src/pages/trainings/TrainingDashboard',
            './src/pages/trainings/TrainingList',
            './src/pages/trainings/TrainingDetail',
            './src/pages/trainings/CreateTraining',
            './src/pages/trainings/EditTraining',
            './src/pages/trainings/MyTrainings'
          ],
          'work-permit-pages': [
            './src/pages/work-permits/WorkPermitDashboard',
            './src/pages/work-permits/WorkPermitList', 
            './src/pages/work-permits/WorkPermitDetail',
            './src/pages/work-permits/CreateWorkPermit',
            './src/pages/work-permits/EditWorkPermit'
          ],
          'security-pages': [
            './src/pages/security/SecurityDashboard',
            './src/pages/security/SecurityIncidentList',
            './src/pages/security/SecurityIncidentDetail',
            './src/pages/security/CreateSecurityIncident'
          ],
          'risk-assessment-pages': [
            './src/pages/risk-assessments/RiskAssessmentList',
            './src/pages/risk-assessments/RiskAssessmentDetail',
            './src/pages/risk-assessments/CreateRiskAssessment'
          ],
          'inspection-pages': [
            './src/pages/inspections/InspectionDashboard',
            './src/pages/inspections/InspectionList',
            './src/pages/inspections/InspectionDetail',
            './src/pages/inspections/CreateInspection'
          ]
        },
        assetFileNames: (assetInfo) => {
          const info = assetInfo.name.split('.');
          const extType = info[info.length - 1];
          if (/png|jpe?g|svg|gif|tiff|bmp|ico/i.test(extType)) {
            return `assets/images/[name]-[hash][extname]`;
          }
          if (/css/i.test(extType)) {
            return `assets/css/[name]-[hash][extname]`;
          }
          return `assets/[ext]/[name]-[hash][extname]`;
        },
        chunkFileNames: 'assets/js/[name]-[hash].js',
        entryFileNames: 'assets/js/[name]-[hash].js'
      }
    },
    chunkSizeWarningLimit: 1000,
    reportCompressedSize: false,
    assetsInlineLimit: 4096,
  }
});