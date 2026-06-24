# Project Context

- **Owner:** Lyle MS Luppes
- **Project:** math.storm.ghcpa
- **Stack:** .NET 10, Blazor Server, ASP.NET Core, Azure Functions, xUnit, Playwright
- **Created:** 2026-04-27T19:26:54.287-05:00

## Learnings

- Day-1 context: Lyle is building Math Storm, a mental math game suite with web and console clients.
- Current focus: add the Story Time level with word-based problems across beginner, novice, intermediate, and expert difficulties.
- 2026-04-27: Completed design review for Story Time. Key architecture decisions:
  - Story Time is a **game mode** (not a 5th difficulty). New `GameMode` enum (`Classic`, `StoryTime`) keeps mode orthogonal to difficulty.
  - `MathQuestion` stays untouched. New `StoryQuestion` wraps it with `NarrativeText`.
  - New `IStoryProblemService` generates word-problem text from templates keyed by `(MathOperation, Difficulty)`.
  - UI: new `StoryQuestionComponent.razor` + mode toggle on `MathGame.razor`.
  - Console: `GameLogic.cs` gains Story Time in the difficulty/mode picker.
  - Leaderboard compound key: `"StoryTime-Expert"` etc.
- Key file paths:
  - Difficulty enum + settings: `src/Core/Models/Difficulty.cs`
  - Game logic + scoring: `src/Core/Services/GameService.cs`
  - Game service interface: `src/Core/Services/Interfaces/IGameService.cs`
  - Web game page: `src/web/MathStorm.Web/Components/Pages/MathGame.razor`
  - Question component: `src/web/MathStorm.Web/Components/Game/QuestionComponent.razor`
  - Results component: `src/web/MathStorm.Web/Components/Game/ResultsComponent.razor`
  - Console game logic: `src/console/MathStorm.Console/GameLogic.cs`
  - Tests: `src/web/MathStorm.Web.Tests/ScoringTests.cs`
  - Game model (Cosmos): `src/Core/Models/Game.cs`
  - DTOs: `src/Core/DTOs/GameDTOs.cs`
- 2026-04-27T19:26:54.287-05:00: Story Time integration review confirmed mode-over-difficulty architecture across UI, DTOs, core session state, and tests. Remaining blocker is leaderboard persistence: `LeaderboardEntry`/Cosmos queries still key on difficulty only, so Story Time scores must not enter shared boards until mode is stored and queried explicitly.

- 2026-04-27T19:26:54.287-05:00: Final integration review now focuses on reconciling the shipped local-first Story Time flow with the additive architecture plan, especially around persistence and leaderboard boundaries.
