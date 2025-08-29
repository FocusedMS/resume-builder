import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Vite configuration for the resume builder frontend. Includes the
// React plugin and sets a default dev server port. Adjust as needed.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173
  },
  build: {
    outDir: 'dist'
  }
});