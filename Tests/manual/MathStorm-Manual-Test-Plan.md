# MathStorm Application Test Harness

This test harness provides manual testing scenarios for the MathStorm application. Use these tests to verify the application's functionality across different scenarios.

## Prerequisites

1. Start the MathStorm application:
   ```bash
   cd src/MathStorm/MathStorm.Web
   dotnet run --urls "http://localhost:5000"
   ```

2. Open a web browser and navigate to `http://localhost:5000`

## Test Scenarios

### 1. Home Page and UI Tests

#### Test 1.1: Page Load and Basic Elements
**Steps:**
1. Navigate to `http://localhost:5000`
2. Verify the page title shows "Math Storm"
3. Verify the main heading "Math Storm" is visible
4. Verify the subtitle "Test your mathematical skills..." is visible
5. Verify "Select Difficulty Level" heading is visible

**Expected Results:**
- Page loads without errors
- All main UI elements are visible and properly styled
- No console errors in browser developer tools

#### Test 1.2: Difficulty Selection Cards
**Steps:**
1. Verify all four difficulty cards are visible:
   - 🌱 Beginner (5 questions, 2-digit numbers, Addition & Subtraction only)
   - 🚀 Novice (5 questions, 2-digit numbers, All operations)
   - ⚡ Intermediate (10 questions, 3-digit numbers, All operations)
   - 🔥 Expert (10 questions, 4-digit numbers, All operations)
2. Click on each difficulty card
3. Verify the selected card gets highlighted
4. Verify "Start Game" button appears when a difficulty is selected

**Expected Results:**
- All difficulty cards display correct information
- Cards are visually distinct and clickable
- Selection state is clearly indicated
- Start Game button appears after selection

### 2. Game Flow Tests

#### Test 2.1: Beginner Game Complete Flow
**Steps:**
1. Select Beginner difficulty
2. Click "Start Game"
3. Verify "Question 1 of 5" is displayed
4. Verify math question shows only addition or subtraction with 2-digit numbers
5. Enter an answer and click "Submit Answer"
6. Verify score is calculated and displayed
7. Click "Next Question"
8. Repeat for all 5 questions
9. Verify game completion screen shows with total score
10. Click "Play Again"

**Expected Results:**
- Game progresses through exactly 5 questions
- Only addition and subtraction operations appear
- Numbers are 2-digit as specified
- Score calculation works correctly
- Game completion screen appears
- Play Again returns to difficulty selection

#### Test 2.2: Expert Game Flow
**Steps:**
1. Select Expert difficulty
2. Click "Start Game"
3. Verify "Question 1 of 10" is displayed
4. Verify math questions include all operations (±×÷)
5. Verify numbers can be up to 4 digits
6. Complete several questions to test all operation types
7. Verify timing affects scoring (faster answers should have lower scores for correct answers)

**Expected Results:**
- Game shows 10 questions for Expert level
- All math operations appear (addition, subtraction, multiplication, division)
- Numbers can be up to 4 digits
- Timing is tracked and affects scoring

#### Test 2.3: Answer Validation and Scoring
**Steps:**
1. Start any difficulty game
2. Test correct answers (calculate and enter exact answer)
3. Test incorrect answers (enter wrong numbers)
4. Test very fast answers (submit immediately)
5. Test slower answers (wait 5+ seconds before submitting)
6. Verify percentage difference calculation for wrong answers

**Expected Results:**
- Correct answers show 0% difference
- Incorrect answers show appropriate percentage difference
- Fast answers (<1 second) get penalty scoring (higher scores)
- Score increases with both time and percentage difference
- All calculations appear accurate

### 3. Responsive Design Tests

#### Test 3.1: Mobile Device Testing
**Steps:**
1. Use browser developer tools to simulate mobile viewport (375x667)
2. Verify all elements remain accessible and readable
3. Test touch interactions (clicking difficulty cards, buttons)
4. Verify game flow works on mobile layout

**Expected Results:**
- All elements scale appropriately
- Text remains readable
- Buttons are large enough for touch interaction
- No horizontal scrolling required

#### Test 3.2: Tablet Device Testing
**Steps:**
1. Set viewport to tablet size (768x1024)
2. Test both portrait and landscape orientations
3. Verify layout adapts appropriately

**Expected Results:**
- Layout works well in both orientations
- Elements use available space effectively

### 4. Input and Navigation Tests

#### Test 4.1: Keyboard Navigation
**Steps:**
1. Start a game
2. Use Tab key to navigate between elements
3. Use Enter key to submit answers
4. Test keyboard input in answer field

**Expected Results:**
- Tab order is logical and complete
- Enter key submits answers
- Keyboard input works properly in answer field

#### Test 4.2: Input Validation
**Steps:**
1. Start a game
2. Try submitting empty answer
3. Try entering non-numeric characters
4. Try entering very large numbers
5. Try entering decimal numbers

**Expected Results:**
- Appropriate validation messages or behavior
- Invalid input is handled gracefully
- No application crashes or errors

### 5. Performance Tests

#### Test 5.1: Page Load Performance
**Steps:**
1. Open browser developer tools Network tab
2. Hard refresh the page (Ctrl+Shift+R)
3. Measure load time and resource sizes

**Expected Results:**
- Page loads within 2 seconds on broadband connection
- Total page size is reasonable (<5MB)
- No unnecessary resources loaded

#### Test 5.2: Game Performance
**Steps:**
1. Start a game and rapidly click through answers
2. Test rapid button clicking
3. Monitor browser console for errors
4. Check memory usage during extended play

**Expected Results:**
- Application remains responsive during rapid interaction
- No memory leaks during extended use
- No JavaScript errors in console

### 6. Browser Compatibility Tests

#### Test 6.1: Cross-Browser Testing
**Test the application in multiple browsers:**
- Chrome (latest)
- Firefox (latest)
- Safari (if available)
- Edge (latest)

**Steps:**
1. Complete basic game flow in each browser
2. Verify UI consistency
3. Test responsive design
4. Check for browser-specific issues

**Expected Results:**
- Consistent functionality across browsers
- Similar visual appearance
- No browser-specific errors

### 7. Accessibility Tests

#### Test 7.1: Screen Reader Compatibility
**Steps:**
1. Use browser's built-in accessibility tools
2. Verify proper heading hierarchy (H1, H2, H3, etc.)
3. Check that form elements have proper labels
4. Verify button text is descriptive

**Expected Results:**
- Logical heading structure
- All interactive elements are properly labeled
- Content is accessible via keyboard navigation

#### Test 7.2: High Contrast Mode
**Steps:**
1. Enable high contrast mode in OS or browser
2. Verify all content remains visible and usable
3. Check color contrast ratios

**Expected Results:**
- All text remains readable
- Interactive elements are clearly distinguishable
- No information is conveyed by color alone

## Test Execution Checklist

### Manual Test Execution Record
Date: ___________  Tester: ___________  Version: ___________

- [ ] Test 1.1: Page Load and Basic Elements
- [ ] Test 1.2: Difficulty Selection Cards
- [ ] Test 2.1: Beginner Game Complete Flow
- [ ] Test 2.2: Expert Game Flow
- [ ] Test 2.3: Answer Validation and Scoring
- [ ] Test 3.1: Mobile Device Testing
- [ ] Test 3.2: Tablet Device Testing
- [ ] Test 4.1: Keyboard Navigation
- [ ] Test 4.2: Input Validation
- [ ] Test 5.1: Page Load Performance
- [ ] Test 5.2: Game Performance
- [ ] Test 6.1: Cross-Browser Testing
- [ ] Test 7.1: Screen Reader Compatibility
- [ ] Test 7.2: High Contrast Mode

### Issues Found
Record any issues discovered during testing:

| Test | Issue Description | Severity | Status |
|------|-------------------|----------|--------|
|      |                   |          |        |
|      |                   |          |        |
|      |                   |          |        |

### Test Summary
- Total Tests: 13
- Passed: ___
- Failed: ___
- Notes: _________________________________________________