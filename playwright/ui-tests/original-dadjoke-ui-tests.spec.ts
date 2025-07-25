import { test, expect } from '@playwright/test';

test('find Dad Joke home page', async ({ page, baseURL }) => {
  console.log('Test: Open Dad Joke website');
  console.log('Using Base URL: ' + baseURL);
  console.log('process.env.CI: ' + process.env.CI);
  console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
  await page.goto('/');
  await expect(page).toHaveTitle(/Dad/);
});

test('find Dad Joke search page', async ({ page, baseURL }) => {
  console.log('Test: Find Search page');
  console.log('Using Base URL: ' + baseURL);
  console.log('process.env.CI: ' + process.env.CI);
  console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
  await page.goto('/');
  await page.getByRole('link', { name: 'Search' }).click();
  await expect(page.getByRole('heading', { name: 'Search the Dad-A-Base' })).toBeVisible();
  // await expect(page.getByRole('heading', { name: 'Break the Dad-A-Base UI Test' })).toBeVisible();
});

test('search for chicken jokes', async ({ page, baseURL }) => {
  console.log('Test: Search for Chicken Jokes');
  console.log('Using Base URL: ' + baseURL);
  console.log('process.env.CI: ' + process.env.CI);
  console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
  await page.goto('/Search');
  await page.waitForSelector('input[name="inputText"], #inputText, input[type="search"]', { state: 'visible' });
  await page.waitForTimeout(1000); // Give time for page to update
  await page.locator('#inputText').fill('chicken');
  await page.getByRole('button', { name: 'Search' }).click();
  await page.waitForTimeout(1000); // Give time for the page to update
  const listItems = await page.locator('main ul li').all();
  const jokeCount = listItems.length;
  console.log('Found ' + jokeCount + ' chicken jokes!');
  expect(jokeCount).toBeGreaterThan(0);
});
