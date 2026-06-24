# Squad Decisions

## Active Decisions

### 2026-04-27T19:26:54.287-05:00: Story Time stays separate from difficulty
- **Sources:** Bruce, Natasha, Tony
- Track Story Time as `GameMode` rather than a fifth difficulty.
- Keep the existing Beginner, Novice, Intermediate, and Expert bands for arithmetic tuning.

### 2026-04-27T19:26:54.287-05:00: Story Time reuses existing arithmetic and stores rendered prompts
- **Sources:** Bruce, Tony
- Leave difficulty generation and scoring in the existing path.
- Carry `GameMode` through game creation and result submission, and persist the rendered `QuestionText` so web, console, and saved results can replay the exact prompt the player saw.

### 2026-04-27T19:26:54.287-05:00: Story Time ships local-first while persistence review continues
- **Sources:** Natasha, Bruce, Tony
- Present Story Time as a separate mode selector in the shipped UI flow.
- Keep Story Time submissions local for now; shared leaderboard separation and mode-aware persistence stay under final integration review instead of shipping as a new leaderboard bucket today.

### 2026-04-27T19:26:54.287-05:00: Story Time verification is arithmetic-first and wording-aware
- **Sources:** Clint, Wanda
- Preserve existing question counts plus current difficulty-based operand and operation constraints across classic and Story Time flows.
- Defer wording-specific assertions until the prompt contract is explicit because current question, results, and leaderboard surfaces do not yet render prompts consistently.
- Avoid higher-difficulty discrete-item division phrasing when answers can be fractional; shipped wording review flagged that pattern as misleading.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
