# Project Context

- **Owner:** Lyle MS Luppes
- **Project:** math.storm.ghcpa
- **Stack:** .NET 10, Blazor Server, ASP.NET Core, Azure Functions, xUnit, Playwright
- **Created:** 2026-04-27T19:26:54.287-05:00

## Learnings

- Day-1 context: Lyle is building Math Storm, a mental math game suite with web and console clients.
- Current focus: add the Story Time level with word-based problems across beginner, novice, intermediate, and expert difficulties.
- Story Time verification belongs in `src\web\MathStorm.Web.Tests`; the safe first contract is preserving existing question counts plus Beginner/Novice operand rules while wording remains under-specified.
- Story Time copy has UI regression risk because `QuestionComponent` appends `= ?`, `ResultsComponent` prepends an operation icon to `QuestionText`, and leaderboard views rebuild prompts from operands instead of reusing `QuestionText`.

- 2026-04-27T19:26:54.287-05:00: Added StoryTimeContractTests that lock current question counts and safe difficulty rules first, while keeping wording assertions deferred until the prompt contract is explicit.
