# Project Context

- **Owner:** Lyle MS Luppes
- **Project:** math.storm.ghcpa
- **Stack:** .NET 10, Blazor Server, ASP.NET Core, Azure Functions, xUnit, Playwright
- **Created:** 2026-04-27T19:26:54.287-05:00

## Learnings

- Day-1 context: Lyle is building Math Storm, a mental math game suite with web and console clients.
- Current focus: add the Story Time level with word-based problems across beginner, novice, intermediate, and expert difficulties.
- Story Time works cleanly when the backend stores `GameMode` and the rendered `QuestionText`, while difficulty/scoring stay untouched in the existing math generation path.

- 2026-04-27T19:26:54.287-05:00: Completed Story Time core implementation around a mode-aware contract that carries `GameMode` and rendered `QuestionText` without changing the existing arithmetic/scoring path.
