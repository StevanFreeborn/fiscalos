import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'
import mkcert from 'vite-plugin-mkcert';

export default defineConfig({
  publicDir: 'static',
  plugins: [
    vue(),
    vueJsx(),
    vueDevTools(),
    mkcert(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    host: true,
    port: parseInt(process.env.PORT ?? '5173'),
    proxy: {
      '/api': {
        target: process.env.services__api__https__0 || process.env.services__api__http__0,
        changeOrigin: true,
        rewrite: path => path.replace(/^\/api/, ''),
        secure: false,
      },
    },
  },
})
