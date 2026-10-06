import type { ServerResponse } from 'node:http'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

/** Gulora.API Properties/launchSettings.json — HTTP applicationUrl portu ilə eyni olmalıdır (indi 5284). */
const DEV_API_HTTP_ORIGIN = 'http://localhost:5284'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5174,
    proxy: {
      '/api': {
        target: DEV_API_HTTP_ORIGIN,
        changeOrigin: true,
        configure(proxy) {
          proxy.on('error', (err, _req, res) => {
            console.error(
              `[vite proxy] /api → ${DEV_API_HTTP_ORIGIN} uğursuz:`,
              err.message
            )
            if (res && !(res as ServerResponse).headersSent) {
              const out = res as ServerResponse
              out.writeHead(502, { 'Content-Type': 'application/json' })
              out.end(
                JSON.stringify({
                  title: 'API əlçatan deyil',
                  detail: `Vite proxy ${DEV_API_HTTP_ORIGIN} ünvanına qoşula bilmədi. Gulora.API-i işə salın; port launchSettings.json ilə uyğun olmalıdır.`,
                })
              )
            }
          })
        },
      },
      '/uploads': {
        target: DEV_API_HTTP_ORIGIN,
        changeOrigin: true,
      },
    },
  },
})
