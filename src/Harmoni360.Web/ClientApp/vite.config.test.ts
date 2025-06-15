import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
  plugins: [react()],
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
    deps: {
      inline: ['@testing-library/jest-dom']
    }
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, './src'),
    },
  },
});