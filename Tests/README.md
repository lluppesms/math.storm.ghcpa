# MathStorm Testing Suite Documentation

This document provides an overview of the comprehensive testing suite created for the MathStorm application.

## Testing Structure

### 1. Unit Tests (C# - MSTest)
**Location:** `src/MathStorm.Tests/`

**Coverage:**
- **GameServiceTests.cs**: Tests for core game logic
  - Game creation with different difficulty levels
  - Question generation and validation
  - Answer submission and scoring
  - Game flow management

- **ModelTests.cs**: Tests for data models
  - MathQuestion operations and properties
  - DifficultySettings validation
  - GameSession state management
  - Computed properties and calculations

- **ServiceTests.cs**: Tests for service layer
  - MockCosmosDbService operations
  - Scoring algorithm validation
  - Database interaction patterns

**Running Unit Tests:**
```bash
cd src/MathStorm
dotnet test
```

**Test Coverage:** 26 comprehensive unit tests covering:
- All difficulty levels (Beginner, Novice, Intermediate, Expert)
- Scoring calculations and edge cases
- Time-based penalties
- Model validation and business logic

### 2. UI Tests (Playwright/TypeScript)
**Location:** `Tests/playwright/ui-tests/`

**Key Test File:** `mathstorm-ui-tests.spec.ts`

**Test Categories:**
- **Home Page and Initial Setup**: Page loading, difficulty selection
- **Game Flow**: Complete game scenarios, answer submission
- **User Interface**: Responsive design, keyboard navigation
- **Accessibility**: Screen reader compatibility, high contrast
- **Performance**: Load times, stress testing

**Running Playwright Tests:**
```bash
# Install browsers (one-time setup)
npx playwright install

# Run tests
npx playwright test

# Run with UI
npx playwright test --ui
```

**Test Scenarios:** 18 comprehensive UI tests covering:
- All game difficulty levels
- Complete game flow from start to finish
- Mobile and tablet responsiveness
- Keyboard navigation and accessibility
- Error handling and edge cases

### 3. Manual Test Harness
**Location:** `TestHarness/`

**Components:**
- **MathStorm-Manual-Test-Plan.md**: Comprehensive manual testing guide
- **MathStorm-API-Integration-Tests.md**: Service integration testing
- **MathStorm-HTTP-Tests.http**: REST client testing scenarios

**Manual Test Categories:**
- UI and UX testing
- Cross-browser compatibility
- Performance and load testing
- Security testing
- Accessibility compliance

### 4. HTTP/API Tests
**Location:** `TestHarness/MathStorm-HTTP-Tests.http`

**Test Coverage:**
- Endpoint availability
- Static resource loading
- Blazor Server SignalR connections
- Security headers
- Performance metrics
- Error handling

## Test Execution Guide

### Quick Validation (5 minutes)
```bash
# 1. Run unit tests
cd src/MathStorm && dotnet test

# 2. Start application
cd src/MathStorm/MathStorm.Web && dotnet run

# 3. Manual smoke test in browser
# Navigate to http://localhost:5000 and complete one game
```

### Full Test Suite (30 minutes)
```bash
# 1. Unit tests
cd src/MathStorm && dotnet test

# 2. Start application for UI tests
cd src/MathStorm/MathStorm.Web && dotnet run --urls "http://localhost:5000"

# 3. Run Playwright tests (in separate terminal)
npx playwright test

# 4. Execute manual test harness scenarios
# Follow TestHarness/MathStorm-Manual-Test-Plan.md

# 5. Run HTTP tests
# Use VS Code REST Client with TestHarness/MathStorm-HTTP-Tests.http
```

## Test Environment Setup

### Prerequisites
- .NET 8.0 SDK
- Node.js (for Playwright)
- Browser for manual testing

### Development Environment
```bash
# Install .NET dependencies
cd src/MathStorm && dotnet restore

# Install Node.js dependencies
npm install

# Install Playwright browsers
npx playwright install
```

### CI/CD Integration
The test suite is designed for easy CI/CD integration:

**Unit Tests:**
- Run on every build
- Fast execution (< 30 seconds)
- No external dependencies

**Playwright Tests:**
- Run on pull requests
- Require running application
- Generate HTML reports

**Manual Tests:**
- Use for release validation
- Cross-browser testing
- Accessibility compliance

## Test Data and Scenarios

### Game Logic Testing
- **Beginner**: 5 questions, 2-digit numbers, +/- only
- **Novice**: 5 questions, 2-digit numbers, all operations
- **Intermediate**: 10 questions, 3-digit numbers, all operations
- **Expert**: 10 questions, 4-digit numbers, all operations

### Scoring Algorithm
```
Score = (PercentageDifference × Time) + (Time × TimeFactor)
TimeFactor = 10 (normal), 100 (speed penalty for < 1 second)
```

### Edge Cases Tested
- Perfect answers (0% difference)
- Very fast answers (speed penalty)
- Very slow answers
- Invalid input handling
- Large number calculations
- Browser refresh during game

## Test Reporting

### Unit Test Reports
- MSTest XML output
- Console summary
- Code coverage (if configured)

### Playwright Test Reports
- HTML report with screenshots
- Video recordings of failures
- Trace files for debugging

### Manual Test Reports
- Checklist format in test plan
- Issue tracking template
- Cross-browser compatibility matrix

## Maintenance and Updates

### Adding New Tests
1. **Unit Tests**: Add to appropriate test class in `src/MathStorm.Tests/`
2. **UI Tests**: Add scenarios to `mathstorm-ui-tests.spec.ts`
3. **Manual Tests**: Update test plan documents
4. **HTTP Tests**: Add endpoints to `.http` file

### Test Data Updates
- Update difficulty settings if game rules change
- Modify scoring expectations if algorithm changes
- Add new test scenarios for new features

### Performance Baselines
- Page load: < 2 seconds
- Game completion: < 30 seconds for 10 questions
- Memory usage: Monitor for leaks during extended play

## Quality Gates

### Definition of Done
- [ ] All unit tests pass
- [ ] Playwright tests pass
- [ ] Manual smoke test completed
- [ ] No console errors
- [ ] Performance within baselines
- [ ] Accessibility compliance verified

### Release Criteria
- [ ] All automated tests pass
- [ ] Cross-browser testing completed
- [ ] Performance testing completed
- [ ] Security testing completed
- [ ] Accessibility audit completed

## Troubleshooting

### Common Issues

**Unit Tests Fail:**
- Check .NET version compatibility
- Verify test project references
- Ensure clean build

**Playwright Tests Fail:**
- Verify application is running on localhost:5000
- Check browser installation
- Review test timeouts

**Application Won't Start:**
- Check port 5000 availability
- Verify .NET SDK installation
- Check for missing dependencies

### Debug Information
- Unit test output: Console and test explorer
- Playwright traces: `npx playwright show-trace <trace-file>`
- Application logs: Console output during `dotnet run`
- Browser console: F12 developer tools

## Future Enhancements

### Potential Improvements
1. **Code Coverage**: Integrate with Coverlet for detailed coverage reports
2. **Performance Testing**: Add automated performance benchmarks
3. **Database Testing**: Integration tests with real CosmosDB
4. **Mobile Testing**: Device-specific Playwright tests
5. **Load Testing**: Automated load testing with tools like k6
6. **API Testing**: Automated API tests if REST endpoints are added

### Monitoring Integration
- Application Performance Monitoring (APM)
- Real User Monitoring (RUM)
- Error tracking and alerting
- Performance degradation detection