# Project Context

- **Owner:** Lyle MS Luppes
- **Project:** math.storm.ghcpa
- **Stack:** .NET 10, Blazor Server, ASP.NET Core, Azure Functions, xUnit, Playwright
- **Created:** 2026-04-27T19:26:54.287-05:00

## Learnings

- Day-1 context: Lyle is building Math Storm, a mental math game suite with web and console clients.
- Current focus: add the Story Time level with word-based problems across beginner, novice, intermediate, and expert difficulties.
- 2026-04-27T19:26:54.287-05:00: Story Time now enters as a separate play mode above the difficulty grid, and its scores stay local until Bruce lands a mode-aware contract for persistence and leaderboard splits.

- 2026-04-27T19:26:54.287-05:00: Completed the Story Time web and console flow with a separate mode selector and local-first result handling while backend persistence review continues.
- 2026-04-27T20:02:34.288-05:00: The runnable web entry point is `src\web\MathStorm.Web\MathStorm.Web.csproj`; `Program.cs` keeps HTTPS redirection on, and `Properties\launchSettings.json` maps Development to `https://localhost:7159` plus `http://localhost:5278`.
- 2026-04-27T20:02:34.288-05:00: For local web startup, build with `dotnet build src\web\MathStorm.Web\MathStorm.Web.csproj`, then run the same project in Development with both URLs bound so the HTTP endpoint can redirect cleanly to HTTPS.
