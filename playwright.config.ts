import { defineConfig } from '@playwright/test';

const port = 5179;
const baseURL = `http://127.0.0.1:${port}`;

export default defineConfig({
  testDir: './e2e',
  timeout: 60_000,
  expect: {
    timeout: 10_000,
  },
  use: {
    baseURL,
    trace: 'retain-on-failure',
  },
  webServer: {
    command:
      'dotnet run --project CovenantPromptKeyWebAssembly/CovenantPromptKeyWebAssembly.csproj',
    url: baseURL,
    reuseExistingServer: !process.env.CI,
    env: {
      ASPNETCORE_URLS: baseURL,
      ASPNETCORE_ENVIRONMENT: 'Development',
    },
  },
});
