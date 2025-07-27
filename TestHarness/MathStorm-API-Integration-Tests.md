# MathStorm API Integration Test Harness

This document provides test scenarios for testing the MathStorm application's backend services and data integration points.

## Prerequisites

1. Start the MathStorm application in development mode:
   ```bash
   cd src/MathStorm/MathStorm.Web
   dotnet run --urls "http://localhost:5000"
   ```

## Service Integration Tests

### 1. GameService Tests

#### Test 1.1: Game Creation Service
**Description:** Test the GameService's ability to create new games with different difficulty settings.

**Manual Test Steps:**
1. Open browser developer tools (F12)
2. Navigate to Network tab
3. Start a new game with each difficulty level
4. Monitor for any AJAX calls or SignalR messages
5. Verify game state is maintained correctly during play

**Service Verification:**
- Verify 5 questions are generated for Beginner/Novice
- Verify 10 questions are generated for Intermediate/Expert
- Verify operation types match difficulty settings
- Verify number ranges match difficulty settings

#### Test 1.2: Scoring Service
**Description:** Test the scoring algorithm implementation.

**Test Cases:**
1. **Perfect Answer Test:**
   - Enter exact correct answer
   - Verify 0% difference
   - Verify time-based scoring only

2. **Wrong Answer Test:**
   - Enter incorrect answer
   - Verify percentage difference calculation
   - Example: If correct = 100, user = 110, difference should be 10%

3. **Speed Penalty Test:**
   - Submit answer very quickly (< 1 second)
   - Verify penalty scoring (time factor = 100 instead of 10)

**Expected Score Formula:**
```
Score = (PercentageDifference × Time) + (Time × TimeFactor)
Where TimeFactor = 10 (normal) or 100 (speed penalty)
```

### 2. Data Service Tests

#### Test 2.1: MockCosmosDbService (Development Mode)
**Description:** Test the mock data service functionality.

**Test Steps:**
1. Ensure application is running in Development mode
2. Complete a full game
3. Verify mock data operations work without errors

**Verification Points:**
- User creation works
- Game data storage works
- Leaderboard retrieval works
- No actual database connections are made

#### Test 2.2: Service Injection and Configuration
**Description:** Verify dependency injection is configured correctly.

**Test Steps:**
1. Review application startup logs
2. Verify correct service implementations are loaded
3. Check for any dependency injection errors

**Development vs Production:**
- Development: Should use MockCosmosDbService
- Production: Should use CosmosDbService with real database

### 3. Component Integration Tests

#### Test 3.1: Game State Management
**Description:** Test how game state is maintained during gameplay.

**Test Scenarios:**
1. **Page Refresh During Game:**
   - Start a game
   - Answer a few questions
   - Refresh the page (F5)
   - Verify game state (should reset to difficulty selection)

2. **Browser Navigation:**
   - Start a game
   - Use browser back button
   - Verify graceful handling

3. **Multiple Tab Testing:**
   - Open application in multiple browser tabs
   - Start games in each tab
   - Verify each tab maintains independent state

#### Test 3.2: Real-time Updates
**Description:** Test Blazor Server's real-time update capabilities.

**Test Steps:**
1. Start a game
2. Monitor browser network tab for SignalR connections
3. Verify UI updates happen smoothly without full page reloads
4. Test with slow network connections (throttle in dev tools)

## Load and Stress Testing

### 4. Performance Tests

#### Test 4.1: Concurrent User Simulation
**Description:** Manually simulate multiple users.

**Test Steps:**
1. Open 5-10 browser tabs to localhost:5000
2. Start games simultaneously in all tabs
3. Monitor application performance
4. Check for any errors in application logs

**Monitoring Points:**
- Memory usage of the dotnet process
- CPU usage during multiple games
- Response times for game interactions

#### Test 4.2: Extended Play Session
**Description:** Test application stability during extended use.

**Test Steps:**
1. Complete 10+ consecutive games
2. Monitor for memory leaks
3. Check application logs for warnings/errors
4. Verify performance doesn't degrade over time

### 5. Configuration Tests

#### Test 5.1: Environment Configuration
**Description:** Test different configuration scenarios.

**appsettings.json Test:**
1. Verify application reads configuration correctly
2. Test with missing configuration values
3. Verify default values are used appropriately

**Connection String Test:**
1. Test with invalid CosmosDB connection string (production mode)
2. Verify appropriate error handling
3. Ensure application doesn't crash on startup

#### Test 5.2: Logging Configuration
**Description:** Test application logging functionality.

**Test Steps:**
1. Review application console output
2. Check for appropriate log levels
3. Verify errors are logged properly
4. Test log filtering

## Security Tests

### 6. Input Validation Tests

#### Test 6.1: Answer Input Validation
**Description:** Test various input scenarios for answer submission.

**Test Cases:**
1. **Numeric Input:**
   - Valid integers: 42, -17, 0
   - Valid decimals: 3.14, -2.5, 0.0
   - Large numbers: 999999999

2. **Invalid Input:**
   - Text: "abc", "hello"
   - Special characters: "!@#$%"
   - Empty input: ""
   - Whitespace: "   "

3. **Edge Cases:**
   - Very large numbers (overflow scenarios)
   - Maximum decimal precision
   - Scientific notation: 1e10

**Expected Behavior:**
- Valid numbers should be accepted
- Invalid input should be rejected gracefully
- No application crashes or security vulnerabilities

#### Test 6.2: Cross-Site Scripting (XSS) Prevention
**Description:** Test for XSS vulnerabilities.

**Test Steps:**
1. Try entering script tags in answer field: `<script>alert('xss')</script>`
2. Test URL manipulation
3. Verify all user input is properly sanitized

**Expected Results:**
- No script execution from user input
- HTML is properly encoded
- No XSS vulnerabilities

## API Testing with REST Client

### 7. HTTP Testing

Since this is a Blazor Server application, there may not be traditional REST APIs, but we can test any exposed endpoints:

#### Test 7.1: Application Health Check
```http
### Health Check (if implemented)
GET http://localhost:5000/health
Accept: application/json
```

#### Test 7.2: Static Resource Loading
```http
### Test CSS loading
GET http://localhost:5000/css/app.css
Accept: text/css

### Test JavaScript loading  
GET http://localhost:5000/_framework/blazor.web.js
Accept: application/javascript
```

#### Test 7.3: Blazor Hub Connection
```http
### Test SignalR connection endpoint
POST http://localhost:5000/_blazor/negotiate
Content-Type: application/json
```

## Database Integration Tests (Production Mode)

### 8. CosmosDB Integration Tests

**Note:** These tests require a real CosmosDB instance and should only be run in appropriate test environments.

#### Test 8.1: Database Connection
**Test Steps:**
1. Configure application for production mode
2. Set valid CosmosDB connection string
3. Start application
4. Verify successful database connection

#### Test 8.2: CRUD Operations
**Test Steps:**
1. Complete a full game
2. Verify user data is created/updated
3. Verify game data is stored
4. Verify leaderboard is updated

## Test Execution Guide

### Daily Smoke Tests (5 minutes)
- [ ] Application starts without errors
- [ ] Can complete one game of each difficulty
- [ ] No console errors during basic gameplay

### Weekly Regression Tests (30 minutes)
- [ ] All service integration tests
- [ ] Performance tests with multiple tabs
- [ ] Input validation tests
- [ ] Cross-browser testing (Chrome, Firefox, Edge)

### Release Testing (60 minutes)
- [ ] Complete all test categories
- [ ] Database integration tests (if applicable)
- [ ] Load testing with realistic user scenarios
- [ ] Security testing
- [ ] Accessibility compliance check

## Automated Testing Integration

### Future Automation Opportunities
1. **Unit Tests:** Already implemented in src/MathStorm.Tests
2. **Integration Tests:** Could be added using ASP.NET Core TestHost
3. **UI Tests:** Playwright tests already created (need browser installation)
4. **API Tests:** Could use tools like Postman/Newman for CI/CD
5. **Performance Tests:** Could integrate with tools like k6 or NBomber

### CI/CD Integration Points
- Unit tests run on every build
- Integration tests run on PR creation
- UI tests run on staging deployments
- Performance tests run on release candidates