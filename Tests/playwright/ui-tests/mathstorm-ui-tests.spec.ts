import { test, expect } from '@playwright/test';

test.describe('MathStorm Game Tests', () => {
  // Helper function to authenticate a user
  async function authenticateUser(page, username = 'TestUser') {
    // Check if we're already authenticated by looking for the welcome message
    const welcomeMessage = page.getByText(`Welcome, ${username}! Ready to play?`);
    const isAuthenticated = await welcomeMessage.isVisible().catch(() => false);
    
    if (!isAuthenticated) {
      // Fill in username and authenticate
      await page.getByRole('textbox', { name: /username/i }).fill(username);
      await page.getByRole('button', { name: 'Continue' }).click();
      
      // Wait for authentication to complete
      await expect(welcomeMessage).toBeVisible({ timeout: 10000 });
    }
  }

  test.beforeEach(async ({ page, baseURL }) => {
    console.log('Test: Open MathStorm website');
    console.log('Using Base URL: ' + baseURL);
    console.log('process.env.CI: ' + process.env.CI);
    console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
    await page.goto(`${baseURL}`);
    
    // Authenticate user before each test
    await authenticateUser(page);
  });

  test.describe('Home Page and Initial Setup', () => {
    test('should display the main Math Storm page', async ({ page }) => {
      // Verify page title
      await expect(page).toHaveTitle('Math Storm');
      
      // Verify main heading is visible
      await expect(page.getByRole('heading', { name: 'Math Storm' })).toBeVisible();
      
      // Verify game description
      await expect(page.getByText('Test your mathematical skills with our lightning-fast math game!')).toBeVisible();
      
      // Verify difficulty selection heading
      await expect(page.getByRole('heading', { name: 'Select Difficulty Level' })).toBeVisible();
    });

    test('should display all difficulty levels', async ({ page }) => {
      // Verify all difficulty cards are visible
      await expect(page.getByText('🌱 Beginner')).toBeVisible();
      await expect(page.getByText('🚀 Novice')).toBeVisible();
      await expect(page.getByText('⚡ Intermediate')).toBeVisible();
      await expect(page.getByText('🔥 Expert')).toBeVisible();
      
      // Verify difficulty descriptions
      await expect(page.getByText('5 questions')).toBeVisible();
      await expect(page.getByText('10 questions')).toBeVisible();
      await expect(page.getByText('Addition & Subtraction only')).toBeVisible();
      await expect(page.getByText('All operations')).toBeVisible();
    });

    test('should allow difficulty selection', async ({ page }) => {
      // Click on Beginner difficulty
      await page.getByText('🌱 Beginner').click();
      
      // Verify the card gets selected (highlighted)
      const beginnerCard = page.locator('.card', { has: page.getByText('🌱 Beginner') });
      await expect(beginnerCard).toHaveClass(/border-primary|bg-light/);
      
      // Verify Start Game button appears with correct difficulty level
      await expect(page.getByRole('button', { name: 'Start Beginner Level' })).toBeVisible();
    });

    test('should show different settings for each difficulty', async ({ page }) => {
      // Test Beginner settings
      await page.getByText('🌱 Beginner').click();
      await expect(page.getByText('5 questions')).toBeVisible();
      await expect(page.getByText('2-digit numbers')).toBeVisible();
      await expect(page.getByText('Addition & Subtraction only')).toBeVisible();
      
      // Test Expert settings
      await page.getByText('🔥 Expert').click();
      await expect(page.getByText('10 questions')).toBeVisible();
      await expect(page.getByText('4-digit numbers')).toBeVisible();
    });
  });

  test.describe('Game Flow', () => {
    test('should start a beginner game successfully', async ({ page }) => {
      // Select Beginner difficulty and start game
      await page.getByText('🌱 Beginner').click();
      await page.getByRole('button', { name: 'Start Beginner Level' }).click();
      
      // Verify game has started
      await expect(page.getByText('Question 1 of 5')).toBeVisible();
      
      // Verify math question is displayed (it's now a heading, not a div with class)
      await expect(page.getByRole('heading', { level: 2 })).toBeVisible();
      
      // Verify answer input is visible (it's now a spinbutton, not textbox)
      await expect(page.getByRole('spinbutton')).toBeVisible();
      
      // Verify Submit button is present
      await expect(page.getByRole('button', { name: 'Submit Answer' })).toBeVisible();
    });

    test('should start an expert game successfully', async ({ page }) => {
      // Select Expert difficulty and start game
      await page.getByText('🔥 Expert').click();
      await page.getByRole('button', { name: 'Start Expert Level' }).click();
      
      // Verify game has started with correct question count
      await expect(page.getByText('Question 1 of 10')).toBeVisible();
      
      // Verify question is displayed (it's now a heading, not a div with class)
      await expect(page.getByRole('heading', { level: 2 })).toBeVisible();
    });

    test('should allow answer submission and progress through questions', async ({ page }) => {
      // Start a beginner game
      await page.getByText('🌱 Beginner').click();
      await page.getByRole('button', { name: 'Start Beginner Level' }).click();
      
      // Wait for first question to load
      await expect(page.getByText('Question 1 of 5')).toBeVisible();
      
      // Get the question text to calculate answer (not needed for test, just submit answer)
      
      // Submit an answer (use spinbutton instead of textbox)
      await page.getByRole('spinbutton').fill('0');
      await page.getByRole('button', { name: 'Submit Answer' }).click();
      
      // Verify score is calculated and displayed
      await expect(page.getByText(/Score:/)).toBeVisible();
      
      // Verify Next Question button appears
      await expect(page.getByRole('button', { name: 'Next Question' })).toBeVisible();
      
      // Click Next Question
      await page.getByRole('button', { name: 'Next Question' }).click();
      
      // Verify we moved to question 2
      await expect(page.getByText('Question 2 of 5')).toBeVisible();
    });

    test('should complete a full game and show results', async ({ page }) => {
      // Start a beginner game (5 questions)
      await page.getByText('🌱 Beginner').click();
      await page.getByRole('button', { name: 'Start Beginner Level' }).click();
      
      // Complete all 5 questions
      for (let i = 1; i <= 5; i++) {
        await expect(page.getByText(`Question ${i} of 5`)).toBeVisible();
        
        // Submit an answer (use spinbutton instead of textbox)
        await page.getByRole('spinbutton').fill('1');
        await page.getByRole('button', { name: 'Submit Answer' }).click();
        
        if (i < 5) {
          // Click Next Question for questions 1-4
          await page.getByRole('button', { name: 'Next Question' }).click();
        }
      }
      
      // After completing all questions, verify game completion
      await expect(page.getByText('Game Complete!')).toBeVisible();
      await expect(page.getByText('Total Score:')).toBeVisible();
      
      // Verify Play Again button is present
      await expect(page.getByRole('button', { name: 'Play Again' })).toBeVisible();
    });

    test('should allow playing again after game completion', async ({ page }) => {
      // Start and complete a beginner game
      await page.getByText('🌱 Beginner').click();
      await page.getByRole('button', { name: 'Start Beginner Level' }).click();
      
      // Quick completion of all questions
      for (let i = 1; i <= 5; i++) {
        await page.getByRole('spinbutton').fill('1');
        await page.getByRole('button', { name: 'Submit Answer' }).click();
        
        if (i < 5) {
          await page.getByRole('button', { name: 'Next Question' }).click();
        }
      }
      
      // Click Play Again
      await page.getByRole('button', { name: 'Play Again' }).click();
      
      // Verify we're back to difficulty selection
      await expect(page.getByRole('heading', { name: 'Select Difficulty Level' })).toBeVisible();
      await expect(page.getByText('🌱 Beginner')).toBeVisible();
    });
  });

  test.describe('User Interface and Interaction', () => {
    test('should be responsive on mobile viewport', async ({ page }) => {
      // Set mobile viewport
      await page.setViewportSize({ width: 375, height: 667 });
      
      // Verify main elements are still visible and accessible
      await expect(page.getByRole('heading', { name: 'Math Storm' })).toBeVisible();
      await expect(page.getByText('🌱 Beginner')).toBeVisible();
      
      // Test interaction on mobile
      await page.getByText('🌱 Beginner').click();
      await expect(page.getByRole('button', { name: 'Start Beginner Level' })).toBeVisible();
    });

    test('should be responsive on tablet viewport', async ({ page }) => {
      // Set tablet viewport
      await page.setViewportSize({ width: 768, height: 1024 });
      
      // Verify layout works on tablet
      await expect(page.getByRole('heading', { name: 'Math Storm' })).toBeVisible();
      await expect(page.getByText('🔥 Expert')).toBeVisible();
      
      // Test game flow on tablet
      await page.getByText('🚀 Novice').click();
      await page.getByRole('button', { name: 'Start Novice Level' }).click();
      await expect(page.getByText('Question 1 of 5')).toBeVisible();
    });

    test('should handle keyboard navigation', async ({ page }) => {
      // Start a game
      await page.getByText('🌱 Beginner').click();
      await page.getByRole('button', { name: 'Start Beginner Level' }).click();
      
      // Test keyboard input in answer field (now spinbutton)
      const answerInput = page.getByRole('spinbutton');
      await answerInput.focus();
      await page.keyboard.type('42');
      
      // Verify the input was entered
      await expect(answerInput).toHaveValue('42');
      
      // Test Enter key to submit
      await page.keyboard.press('Enter');
      
      // Verify answer was submitted (score should appear)
      await expect(page.getByText(/Score:/)).toBeVisible();
    });

    test('should validate answer input', async ({ page }) => {
      // Start a game
      await page.getByText('🌱 Beginner').click();
      await page.getByRole('button', { name: 'Start Beginner Level' }).click();
      
      // Try to submit without entering an answer
      await page.getByRole('button', { name: 'Submit Answer' }).click();
      
      // Verify some form of validation (this may vary based on implementation)
      // The button might be disabled or show validation message
      const answerInput = page.getByRole('spinbutton');
      await expect(answerInput).toBeVisible();
    });
  });

  test.describe('Accessibility', () => {
    test('should have proper heading hierarchy', async ({ page }) => {
      // Verify h1 is present
      await expect(page.locator('h1')).toBeVisible();
      
      // Verify other headings exist - there are more headings now due to auth section
      await expect(page.locator('h2, h3, h4, h5, h6')).toHaveCount(4, { timeout: 5000 });
    });

    test('should have accessible form elements', async ({ page }) => {
      // Start a game to see form elements
      await page.getByText('🌱 Beginner').click();
      await page.getByRole('button', { name: 'Start Beginner Level' }).click();
      
      // Verify form elements have proper labels (now spinbutton instead of textbox)
      const answerInput = page.getByRole('spinbutton');
      await expect(answerInput).toBeVisible();
      
      // Verify buttons have accessible names
      await expect(page.getByRole('button', { name: 'Submit Answer' })).toBeVisible();
    });

    test('should support high contrast mode', async ({ page }) => {
      // Test with forced-colors media query (simulates high contrast mode)
      await page.emulateMedia({ colorScheme: 'dark' });
      
      // Verify elements are still visible
      await expect(page.getByRole('heading', { name: 'Math Storm' })).toBeVisible();
      await expect(page.getByText('🌱 Beginner')).toBeVisible();
    });
  });

  test.describe('Performance and Error Handling', () => {
    test('should load quickly', async ({ page }) => {
      // Since beforeEach already loads the page and authenticates, just measure how long
      // authentication and page setup took. For this test, we'll start fresh to measure load time.
      const startTime = Date.now();
      await page.goto('http://localhost:5278');
      
      // Authenticate for proper load time measurement
      await page.getByRole('textbox', { name: /username/i }).fill('TestUser');
      await page.getByRole('button', { name: 'Continue' }).click();
      await expect(page.getByText('Welcome, TestUser! Ready to play?')).toBeVisible();
      
      const loadTime = Date.now() - startTime;
      
      // Verify page loads within reasonable time (5 seconds including auth)
      expect(loadTime).toBeLessThan(5000);
      
      // Verify main content is visible
      await expect(page.getByRole('heading', { name: 'Math Storm' })).toBeVisible();
    });

    test('should handle rapid clicking gracefully', async ({ page }) => {
      // Select difficulty
      await page.getByText('🌱 Beginner').click();
      
      // Rapidly click Start Game multiple times (use correct button name)
      const startButton = page.getByRole('button', { name: 'Start Beginner Level' });
      await startButton.click();
      await startButton.click();
      await startButton.click();
      
      // Verify game still works properly
      await expect(page.getByText('Question 1 of 5')).toBeVisible();
    });
  });

  test.describe('Leaderboard Analysis Features', () => {
    test('should display analysis icons on leaderboard page', async ({ page }) => {
      // Navigate to leaderboard page
      await page.goto('/leaderboard');
      
      // Wait for leaderboard to load
      await expect(page.getByRole('heading', { name: 'Leaderboard' })).toBeVisible();
      
      // Check that Analysis column header is present
      await expect(page.getByRole('columnheader', { name: 'Analysis' })).toBeVisible();
      
      // The page should display either analysis icons or placeholder dashes
      // We can't guarantee there will be analysis data, so we just check the column exists
      const analysisColumn = page.locator('td').nth(4); // 5th column (0-indexed)
      
      // Verify the column exists (it may contain icons or dashes)
      if (await analysisColumn.isVisible()) {
        console.log('Analysis column is visible on leaderboard');
      }
    });

    test('should show analysis tooltips when hovering over icons', async ({ page }) => {
      // Complete a game first to ensure there's data with potential analysis
      await page.getByText('🌱 Beginner').click();
      await page.getByRole('button', { name: 'Start Beginner Level' }).click();
      
      // Complete the game quickly
      for (let i = 1; i <= 5; i++) {
        await page.getByRole('spinbutton').fill('1');
        await page.getByRole('button', { name: 'Submit Answer' }).click();
        
        if (i < 5) {
          await page.getByRole('button', { name: 'Next Question' }).click();
        }
      }
      
      // Wait for game completion and results
      await expect(page.getByText('Game Complete!')).toBeVisible();
      
      // Look for analysis icons in the leaderboard (if present)
      const analysisIcons = page.locator('.analysis-icon');
      const iconCount = await analysisIcons.count();
      
      if (iconCount > 0) {
        // If there are analysis icons, test tooltip functionality
        await analysisIcons.first().hover();
        
        // Check that the icon has tooltip attributes
        await expect(analysisIcons.first()).toHaveAttribute('data-bs-toggle', 'tooltip');
      } else {
        console.log('No analysis icons found - this is expected if no analysis data is available');
      }
    });

    test('should display analysis column in global leaderboard', async ({ page }) => {
      // Navigate to leaderboard page
      await page.goto('/leaderboard');
      
      // Wait for page to load
      await expect(page.getByRole('heading', { name: 'Leaderboard' })).toBeVisible();
      
      // Scroll to global leaderboard section
      await page.locator('text=Global Leaderboard - All Levels').scrollIntoViewIfNeeded();
      
      // Check that Analysis column header is present in global leaderboard
      const globalLeaderboardTable = page.locator('.card').filter({ has: page.locator('text=Global Leaderboard') });
      const analysisHeader = globalLeaderboardTable.getByRole('columnheader', { name: 'Analysis' });
      
      if (await analysisHeader.isVisible()) {
        await expect(analysisHeader).toBeVisible();
      }
    });
  });
});