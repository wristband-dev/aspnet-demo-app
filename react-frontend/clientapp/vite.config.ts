import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
  server: {
    allowedHosts: ['localhost'],
    host: true,
    port: parseInt(process.env.PORT ?? '6001'),
  },
  plugins: [react()],
});
