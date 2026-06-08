import { defineConfig } from 'orval';

export default defineConfig({
  bookStorage: {
    input: {
      target: 'http://localhost:5189/openapi/v1.json',
    },
    output: {
      target: './src/api/generated/',
      client: 'react-query',
      schemas: './src/api/generated/dtos/',
      mode: 'single',
      clean: true,
    },
    override: {
      query: {
        useQueryKey: true,
        usePrefetch: false,
      },
    },
  },
});