import { fileURLToPath, URL } from 'node:url'
import { PrimeVueResolver } from '@primevue/auto-import-resolver'
import vue from '@vitejs/plugin-vue'
import Components from 'unplugin-vue-components/vite'
import { defineConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig({
  base: '/resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc',
  plugins: [
    vue(),
    Components({
      resolvers: [PrimeVueResolver()]
    }),
    {
      name: 'inject-resqueue-config',
      transformIndexHtml(html) {
        return html.replace(
          '<!-- inject:resqueue-config-script -->',
          '<script src="/resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc/config.js"></script>'
        )
      }
    }
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})
