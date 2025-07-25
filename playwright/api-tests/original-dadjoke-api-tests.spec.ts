const { test, expect, request } = require('@playwright/test');

test.describe('Original DadABase API  Tests', () => {
  const headers = {
    'Accept': 'application/json',
    'Content-Type': 'application/json',
    'ApiKey': 'Hello, hungry!'
  }

  test('should get one random joke', async ( { baseURL } ) => {
      console.log('Test: Call Dad Joke API');
      console.log('Using Base URL: ' + baseURL);
      console.log('process.env.CI: ' + process.env.CI);
      console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
      const apiContext = await request.newContext({ 
        baseURL: baseURL,
        extraHTTPHeaders: headers, 
        ignoreHTTPSErrors: true 
      });
      const response = await apiContext.get("/api/joke");  
      console.log(await response.json());
      expect(response.ok()).toBeTruthy();
      expect(response.status()).toBe(200);
  });

  test('should get all jokes in one category', async ({ baseURL }) => {
    console.log('Test: Call Dad Joke API');
    console.log('Using Base URL: ' + baseURL);
    console.log('process.env.CI: ' + process.env.CI);
    console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
    const apiContext = await request.newContext({ 
      baseURL: baseURL,
      extraHTTPHeaders: headers, 
      ignoreHTTPSErrors: true 
    });
    const response = await apiContext.get("/api/joke/category/Chickens");
    const responseBody = await response.json()
    expect(responseBody[0]).toHaveProperty("category", "Chickens");
    const jokeCount = responseBody.length;
    console.log('Found ' + jokeCount + ' chicken jokes!');
    console.log('First chicken joke:');
    console.log(responseBody[0]);
    expect(response.ok()).toBeTruthy();
    expect(response.status()).toBe(200);
  });
});
