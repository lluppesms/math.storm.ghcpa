# Squad Team

> Math Storm squad focused on gameplay, story problems, and shipping the Story Time level cleanly.

## Coordinator

| Name | Role | Notes |
|------|------|-------|
| Squad | Coordinator | Routes work, enforces handoffs and reviewer gates. Does not generate domain artifacts. |

## Members

| Name | Role | Charter | Status |
|------|------|---------|--------|
| Tony | Lead | `.squad/agents/tony/charter.md` | ✅ Active |
| Natasha | Frontend Dev | `.squad/agents/natasha/charter.md` | ✅ Active |
| Bruce | Backend Dev | `.squad/agents/bruce/charter.md` | ✅ Active |
| Clint | Tester | `.squad/agents/clint/charter.md` | ✅ Active |
| Wanda | Content Designer | `.squad/agents/wanda/charter.md` | ✅ Active |
| Scribe | Session Logger | `.squad/agents/scribe/charter.md` | 📋 Silent |
| Ralph | Work Monitor | `.squad/agents/ralph/charter.md` | 🔄 Monitor |

## Coding Agent

<!-- copilot-auto-assign: false -->

| Name | Role | Charter | Status |
|------|------|---------|--------|
| @copilot | Coding Agent | — | 🤖 Coding Agent |

### Capabilities

**🟢 Good fit — auto-route when enabled:**
- Bug fixes with clear reproduction steps
- Test coverage additions and flaky test fixes
- Small isolated features with clear specs
- Documentation fixes and README updates

**🟡 Needs review — route to @copilot but flag for squad review:**
- Medium features with clear acceptance criteria
- Refactoring along established patterns
- Small gameplay or UI additions that stay inside existing architecture

**🔴 Not suitable — route to squad member instead:**
- Architecture decisions and system design
- Ambiguous requirements needing clarification
- Cross-system gameplay changes requiring coordination
- Security-critical or performance-critical work

## Project Context

- **Owner:** Lyle MS Luppes
- **Stack:** .NET 10, Blazor Server, ASP.NET Core, Azure Functions, xUnit, Playwright
- **Description:** Math Storm is a mental math game suite with web and console clients; the current focus is adding the Story Time level with word-based problems across four difficulty bands.
- **Created:** 2026-04-27T19:26:54.287-05:00
