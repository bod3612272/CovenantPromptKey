import { test, expect } from '@playwright/test';

test('WASM host loads help page', async ({ page }) => {
  await page.goto('/help');
  await expect(page.getByRole('heading', { name: '使用說明' })).toBeVisible();
});
