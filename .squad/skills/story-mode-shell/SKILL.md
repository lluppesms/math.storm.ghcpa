---
name: "story-mode-shell"
description: "Layer a new play mode over existing difficulty tuning without pretending it is a fifth difficulty"
domain: "frontend-flow"
confidence: "medium"
source: "earned while shipping Story Time in Math Storm"
---

## Context

Use this when a game needs a new player-facing mode, but the underlying question generation and difficulty tuning can stay exactly where they are. It is especially useful when backend contracts are not ready to distinguish the new mode in persistence, analytics, or leaderboards.

## Patterns

- **Separate mode from difficulty in the entry UI:** put mode selection before difficulty so players do not confuse a presentation change with a tuning band
- **Reuse existing question data:** if the payload already includes enough structure (numbers + operation), generate alternative presentation locally instead of blocking on new APIs
- **Protect downstream systems:** if persistence cannot distinguish the new mode yet, keep score submission local and explain the limitation in the results UI
- **Keep the seam obvious:** pass mode as local UI state/parameters so backend teams can replace the temporary guard with a real contract later

## Anti-Patterns

- Presenting a new mode as a fifth difficulty card
- Saving mixed-mode scores into a shared leaderboard with no way to tell them apart
- Blocking the entire frontend experience on a backend contract when local presentation can safely ship first
