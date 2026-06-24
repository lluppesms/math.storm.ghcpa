---
name: "story-mode-contracts"
description: "Verify narrative math modes without breaking arithmetic rules or prompt rendering"
domain: "testing"
confidence: "medium"
source: "earned"
---

## Context

When a math game adds a narrative or word-problem presentation mode, the biggest regressions are usually not in scoring. They show up when the new copy silently changes arithmetic constraints, or when one component renders the prompt text while another reconstructs the old symbolic expression.

## Patterns

- **Lock arithmetic invariants first:** Before asserting on wording, preserve existing question counts, allowed operations, and operand-shape rules for each difficulty band.
- **Audit every prompt surface:** Check live question UI, results tables, and leaderboard/history views separately. Story text often renders correctly in one place and gets duplicated or flattened back into symbols somewhere else.
- **Carry mode and prompt through contracts:** Add explicit `GameMode` metadata at session/request/response boundaries and persist the rendered `QuestionText` so consoles, results views, and saved history do not reconstruct the classic equation by accident.
- **Treat ambiguous nouns and units as correctness bugs:** If the wording hides the operation, flips actor order, drops the unit, or leaves division remainder behavior unclear, reject the prompt contract.
- **Match the noun to the answer type:** If a difficulty band can produce decimal division answers, use continuous units like time, distance, or liquid amount, or force exact division. Do not ask players to produce fractional cards, chips, or other discrete objects unless the story explicitly allows splitting them.
- **Prefer mode-specific tests over replacing baseline tests:** Keep the arithmetic-mode tests green and add Story Time coverage alongside them so regressions show whether the break is in math generation or presentation.

## Examples

- Preserve Beginner as addition/subtraction-only even after swapping `12 + 4` for a sentence prompt.
- Reject a result row that renders `➕ Maya has 12 apples and gets 4 more = ?` because it duplicates the math cue instead of presenting the stored prompt cleanly.
- Store `StoryTime` on the game/session contract and save the chosen `QuestionText` with each result so replay/history surfaces keep the same prompt the player saw.
- Block a division story that does not say whether leftovers are allowed when Novice still requires exact division.
- Rewrite `312 storm cards are shared evenly among 10 players` if the generator can legitimately expect `31.2`, or constrain that template to whole-number division only.

## Anti-Patterns

- Replacing existing difficulty tests with vague story assertions.
- Assuming `QuestionText` is the only render path without checking results or leaderboard components.
- Allowing story prompts to imply multiplication or division behavior that the selected difficulty does not support.
- Accepting wording that changes operand order or introduces extra unknowns.
- Pairing decimal-capable division with discrete nouns that make the mathematically correct answer sound impossible.
