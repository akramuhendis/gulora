import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

/** Gulora.API Properties/launchSettings.json — HTTP applicationUrl portu ilə eyni olmalıdır (indi 5284). */
const DEV_API_HTTP_ORIGIN = 'http://localhost:5284'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      '/api': {
        target: DEV_API_HTTP_ORIGIN,
        changeOrigin: true,
      },
      '/uploads': {
        target: DEV_API_HTTP_ORIGIN,
        changeOrigin: true,
      },
    },
  },
})
