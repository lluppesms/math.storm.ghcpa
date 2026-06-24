---
name: "blazor-startup-verification"
description: "Start the Math Storm web app in the background and verify the real listening URLs instead of assuming launch settings are enough"
domain: "frontend-ops"
confidence: "high"
source: "earned"
---

## Context

Use this when you need to boot the Blazor web app for review or UX checks and leave it running for follow-up work. It applies to the Math Storm web frontend because the app enforces HTTPS redirection and the useful proof is a live listener, not just a successful build.

## Patterns

- **Use the web csproj directly:** start from `src\web\MathStorm.Web\MathStorm.Web.csproj` instead of the solution so the entry path is unambiguous
- **Build before launch:** run a quick `dotnet build` first so startup failures are more likely to be config or runtime issues, not compile noise
- **Bind both Development URLs explicitly:** use `https://localhost:7159;http://localhost:5278` so the HTTP endpoint can redirect and the HTTPS endpoint can serve the actual app
- **Verify with behavior, not hope:** confirm startup from Kestrel's `Now listening on` output, then check that `http://localhost:5278` redirects and `https://localhost:7159` answers on the socket/HTTP layer
- **Keep the server detached:** launch with a background process so the app stays up after the terminal task finishes

## Examples

- Build: `dotnet build src\web\MathStorm.Web\MathStorm.Web.csproj`
- Run: `dotnet run --no-build --no-launch-profile --project src\web\MathStorm.Web\MathStorm.Web.csproj`
- Expected live URLs: `https://localhost:7159` and `http://localhost:5278`

## Anti-Patterns

- Launching the solution and guessing which web project actually owns the server
- Binding only HTTP when the app immediately redirects to HTTPS
- Reporting launch settings as success without checking a live listener
