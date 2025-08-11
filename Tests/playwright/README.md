# Playwright UI Tests - Updated for Authentication Flow

## Overview
The Playwright tests have been updated to handle the new user authentication flow that was added to the MathStorm application.

## Key Changes Made

### 1. Authentication Integration
- **Added**: `authenticateUser()` helper function in test file
- **Updated**: `beforeEach` hook to authenticate users before each test
- **Purpose**: Handle the required username entry before accessing game features

### 2. Selector Updates
The following selectors were updated to match the current UI:

| Element | Old Selector | New Selector | Reason |
|---------|-------------|--------------|---------|
| Start button | `'Start Game'` | `'Start [Difficulty] Level'` | Button text now includes difficulty |
| Answer input | `textbox` with answer name | `spinbutton` | Input type changed |
| Question display | `.question-display` | `heading` with level 2 | Questions now use semantic headings |
| Performance test URL | `localhost:5000` | `localhost:5278` | Correct port |

### 3. Test Flow Updates
- **Authentication**: All tests now authenticate with "TestUser" username
- **Timeouts**: Added proper timeout for authentication wait (10 seconds)
- **Expectations**: Updated heading count from 2 to 4 (includes auth headers)

## Test Structure

### Authentication Helper Function
```typescript
async function authenticateUser(page, username = 'TestUser') {
  const welcomeMessage = page.getByText(`Welcome, ${username}! Ready to play?`);
  const isAuthenticated = await welcomeMessage.isVisible().catch(() => false);
  
  if (!isAuthenticated) {
    await page.getByRole('textbox', { name: /username/i }).fill(username);
    await page.getByRole('button', { name: 'Continue' }).click();
    await expect(welcomeMessage).toBeVisible({ timeout: 10000 });
  }
}
```

### Updated Test Flow
1. **Navigation**: Go to http://localhost:5278
2. **Authentication**: Automatically authenticate with test user
3. **Game Testing**: Proceed with original test scenarios
4. **Cleanup**: Each test starts fresh with new authentication

## UI Elements Tested

### Authentication Screen
- Username input field
- PIN input field (optional)
- Remember me checkbox
- Continue button
- Loading/success states

### Game Interface (Post-Authentication)
- Welcome message with username
- Switch User button
- Difficulty selection cards
- Dynamic start button text
- Game flow (questions, answers, scoring)
- Play Again functionality

## Test Categories

### 1. Home Page and Initial Setup (4 tests)
- Main page display verification
- Difficulty level display
- Difficulty selection functionality
- Settings verification per difficulty

### 2. Game Flow (5 tests)
- Beginner game start
- Expert game start
- Answer submission and progression
- Full game completion
- Play Again functionality

### 3. User Interface and Interaction (4 tests)
- Mobile viewport responsiveness
- Tablet viewport responsiveness
- Keyboard navigation
- Input validation

### 4. Accessibility (3 tests)
- Heading hierarchy
- Form element accessibility
- High contrast mode support

### 5. Performance and Error Handling (2 tests)
- Page load time verification
- Rapid clicking handling

## Running the Tests

### Prerequisites
1. MathStorm web application running on http://localhost:5278
2. Playwright installed with browsers: `npx playwright install`

### Commands
```bash
# Run all tests
npx playwright test

# Run specific test suite
npx playwright test --grep "Game Flow"

# Run with UI mode
npx playwright test --ui

# Run single test
npx playwright test --grep "should display the main Math Storm page"
```

## Expected Test Results
With the authentication flow properly integrated, all 18 test cases should now pass:

- ✅ Authentication automatically handled
- ✅ Difficulty selection working correctly  
- ✅ Game flow from start to completion
- ✅ UI responsiveness across viewports
- ✅ Accessibility requirements met
- ✅ Performance expectations satisfied

## Troubleshooting

### Common Issues
1. **Authentication timeout**: Increase timeout if server is slow
2. **Element not found**: Verify UI hasn't changed further
3. **Port conflicts**: Ensure app is running on correct port (5278)

### Debug Commands
```bash
# List all tests without running
npx playwright test --list

# Run in debug mode
npx playwright test --debug

# Run with verbose output
npx playwright test --verbose
```

## Future Maintenance
When updating tests:
1. Verify authentication flow still works
2. Check for UI changes that affect selectors
3. Update timeouts if application performance changes
4. Maintain test isolation (each test authenticates fresh)