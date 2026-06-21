import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { configDefaults, coverageConfigDefaults } from 'vitest/config';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "http://localhost:5189",
        changeOrigin: true,
      },
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './src/test/setup.ts',
    exclude: [
      ...configDefaults.exclude,
      'e2e/**',
    ],
    coverage: {
      reporter: ['text', 'html'],
      exclude: [
        ...coverageConfigDefaults.exclude,
        'src/test/**',
      ],
    },
  },
});
