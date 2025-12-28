import { test, expect } from '@playwright/test';

test('WASM host loads home page', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('#app')).toBeHidden({ timeout: 10_000 }).catch(() => undefined);
  await expect(page).toHaveTitle(/CovenantPromptKey/i);
});
