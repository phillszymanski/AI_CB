import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react({
      babel: {
        plugins: [['babel-plugin-react-compiler']],
      },
    }),
  ],
  server: {
    proxy: {
      // Forward /api requests to the backend running on https://localhost:5010
      '/api': {
        target: 'https://localhost:5010',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
