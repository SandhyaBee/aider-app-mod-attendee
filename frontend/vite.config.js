import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../backend/wwwroot', // Output to backend's wwwroot for direct serving
    emptyOutDir: true, // Clean the output directory before each build
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000', // Matches the .NET launch profile
        changeOrigin: true,
        secure: false,
      }
    }
  }
})