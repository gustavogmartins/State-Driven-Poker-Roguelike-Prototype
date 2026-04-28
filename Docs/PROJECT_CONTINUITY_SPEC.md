# Project Continuity Spec

Last updated: 2026-04-28

## Purpose

This file is the continuity spec for the project.

It exists to let:

- the project resume on another computer without re-mapping the whole repository
- a future AI session get useful context quickly
- the repo keep a single source of truth for current status, not only original intent

This file should be treated as the current working spec of the project.

## Project Summary

Project name:

- Unity State-Driven Poker Roguelike Prototype

Project intent:

- Build a poker-roguelike prototype inspired by the gameplay loop of Balatro
- Keep the project portfolio-friendly and technically explainable
- Prioritize state-driven gameplay rules, clean UI binding, and scalable architecture

Important legal/product framing:

- This is a study/portfolio project
- It should not be presented as a commercial clone
- Current UI direction is visually inspired by Balatro references, but the codebase should continue being treated as an original prototype

## Docs vs Reality

The files in `Docs/` describe a broader target architecture than what is currently implemented.

Planned in docs:

- `RunState`
- `BlindState`
- `ShopState`
- `GameStateStore`
- action/reducer/store flow
- shop
- modifiers/jokers
- ante progression through small blind / big blind / boss blind
- automated tests

Actually implemented today:

- one active playable round scene
- one run-level gameplay state: `RunState`
- one blind-level gameplay state: `RoundState`
- a presenter-driven UI refresh path
- card selection
- hand preview evaluation while selecting cards
- score calculation
- discard/play flow
- score accumulation toward a target
- explicit round win/loss derived state in `RoundState`
- centralized round setup through `RoundState.CreateInitial(...)`
- blind progression owned by `RunState`
- shop transition scaffolding through `ShopState`
- Edit Mode tests for core gameplay rules
- Balatro-inspired HUD and playfield UI

Conclusion:

- The current codebase is closer to "Milestone 2 core flow plus partial UI/presentation polish" than to the full architecture described in the older docs
- This file supersedes the older docs when there is a mismatch about implementation status

## Current Codebase Stage

The project has reached:

- a single-scene playable prototype with real ante/blind progression
- with state, scoring, rendering, selection, play, discard, and score preview working
- with a custom HUD and card presentation already integrated into `GameScene`
- with Milestone 1 and Milestone 2 core gameplay rules covered by Edit Mode tests

The project has not yet reached:

- shop UI and purchase flow
- modifiers/jokers
- boss-specific gameplay rules/debuffs
- full manual verification of the new tests through the Unity Test Runner
- production-ready content pipeline

## Actual Architecture In Use

Current real flow:

- card click
- `CardView` raises selection event
- `RoundScreen` updates `RunState`
- `RunState` updates `RoundState`
- `RoundPresenter` converts state to `RoundViewModel`
- `RoundScreen` renders texts, buttons, hand cards, and played cards

Important recent architecture changes:

- `RoundState.CreateInitial(...)` now owns round bootstrap instead of `RoundScreen` manually building domain state
- `BlindState` now owns blind type, ante, reward, and target score progression
- `RoundState` now exposes derived values such as `BlindReward`, `RemainingScore`, `HasWonRound`, and `HasLostRound`
- `RunState` now owns money, blind advancement, and run phase transitions
- `RunState.EnterShop()` and `RunState.LeaveShop()` now provide the first `Blind -> Shop -> Blind` run loop
- `RoundPresenter` now derives clearer round-end status text from domain state
- gameplay scripts now compile through a dedicated runtime assembly: `Assets/Scripts/StateDrivenPokerRoguelike.asmdef`
- gameplay tests live in `Assets/Scripts/Tests/EditMode`

This is state-driven enough to be workable, but it is not yet the full action/store/reducer architecture described in the original docs.

### Main files that currently define the project

- `Assets/Scripts/Core/RunState.cs`
- `Assets/Scripts/Core/BlindState.cs`
- `Assets/Scripts/Core/RoundState.cs`
- `Assets/Scripts/Core/ShopState.cs`
- `Assets/Scripts/Core/PokerHandEvaluator.cs`
- `Assets/Scripts/Core/ScoreCalculator.cs`
- `Assets/Scripts/Core/ScoringCardSelector.cs`
- `Assets/Scripts/Presenters/RoundPresenter.cs`
- `Assets/Scripts/MonoBehaviours/RoundScreen.cs`
- `Assets/Scripts/View/CardView.cs`
- `Assets/Scripts/View/CardViewModel.cs`
- `Assets/Scripts/View/RoundViewModel.cs`
- `Assets/Scenes/GameScene.unity`
- `Assets/Prefabs/CardViewPrefab.prefab`

## Scene and UI Status

Main scene:

- `Assets/Scenes/GameScene.unity`

Main screen script:

- `Assets/Scripts/MonoBehaviours/RoundScreen.cs`

UI state today:

- Left HUD panel exists and is bound to gameplay state
- `HandNameText` updates from evaluated selected cards
- `PlayHandButton` and `DiscardButton` are wired as persistent scene button events
- sort buttons are still wired in code at runtime
- round-end overlay primary action advances to next blind when the player wins
- bottom hand container uses `HorizontalLayoutGroup`
- middle played-cards container uses `HorizontalLayoutGroup`
- `CardViewPrefab` is the prefab used for both hand cards and played cards
- generated placeholder UI art exists under `Assets/Art/UI/Generated`
- screenshot references exist under `Assets/ReferenceScreenShots`

Important recent UI decisions:

- card placement is no longer manually mapped in script
- hand and played cards now depend on layout containers
- this is a temporary base structure for a future slot-based layout system

## Gameplay Systems Implemented

### Deck and hand

Implemented:

- standard 52-card deck generation
- shuffle
- draw to hand size
- discard selected cards and redraw
- play selected cards and redraw
- selected card cap of 5

Main files:

- `Assets/Scripts/Core/DeckBuilder.cs`
- `Assets/Scripts/Core/DeckShuffler.cs`
- `Assets/Scripts/Utility/DeckUtility.cs`
- `Assets/Scripts/Core/RoundState.cs`

### Poker hand evaluation

Implemented:

- High Card
- Pair
- Two Pair
- Three of a Kind
- Straight
- Flush
- Full House
- Four of a Kind
- Straight Flush

Recent rule fix already applied:

- `Flush` now only validates when exactly 5 cards are played and all share the same suit
- this also fixes `Straight Flush`, since it depends on the same flush validation

Main files:

- `Assets/Scripts/Core/PokerHandEvaluator.cs`
- `Assets/Scripts/Core/PokerHandResult.cs`
- `Assets/Scripts/Enums/PokerHandType.cs`

### Score system

Implemented:

- base hand score table
- scoring card selection
- final score from `Chips x Mult`
- round score accumulation
- preview score while cards are selected

Current scoring simplification:

- for `High Card`, only the highest card scores
- for all other hand types, the current implementation scores all played cards
- there is no modifier/joker layer yet

Main files:

- `Assets/Scripts/Core/HandBaseScore.cs`
- `Assets/Scripts/Core/ScoreCalculator.cs`
- `Assets/Scripts/Core/ScoreResult.cs`
- `Assets/Scripts/Core/ScoringCardSelector.cs`
- `Assets/Scripts/Utility/CardChipValueUtility.cs`

### Round flow

Implemented:

- target score for the blind
- hands remaining
- discards remaining
- current score accumulation
- blind cleared check
- round end when blind is cleared or hands reach zero
- explicit domain flags for round over / round won / round lost
- last played cards tracking
- last played hand tracking
- status text updates
- real Small Blind / Big Blind / Boss Blind sequence
- advance to shop after a blind win
- exit shop into the pending blind
- advance to next blind after a win
- advance to next ante after clearing Boss Blind
- blind reward and money carry-over between blinds

Partially implemented only as data/presentation:

- boss blind identity and progression exist, but no special boss debuff/rule exists yet

Not implemented yet:

- boss blind behavior
- shop UI
- shop offers, buying, and selling

### Run flow

Implemented:

- `RunState` owns persistent money
- `RunState` owns current phase through `RunPhase`
- `RunState` owns blind advancement and ante rollover
- `RunState` can enter a `ShopState` after a blind win
- `RunState` can leave `ShopState` into the pending next blind

Not implemented yet:

- run-owned modifier inventory
- shop offer generation
- shop transaction rules
- run win condition beyond the current blind-loss end state

## Milestone Mapping

### Milestone 1 - Single playable blind

Status:

- Implemented and stabilized in code

Completed:

- create deck
- shuffle
- draw to hand size
- select up to 5 cards
- preview evaluated hand
- play selected cards
- discard selected cards
- calculate score
- accumulate round score
- detect blind clear
- detect round end
- render current hand and played cards
- centralized round setup in domain state
- clearer round loss/win messaging and derived state
- validation around constructor/state edge cases
- Edit Mode tests for `PokerHandEvaluator`, `ScoreCalculator`, and `RoundState`

Remaining caveat:

- run the new Edit Mode suite manually in Unity Test Runner as a final human verification step

### Milestone 2 - Ante/blind progression

Status:

- Implemented in code

Completed:

- `BlindState` domain model
- `RunState` run-level domain model
- Small Blind / Big Blind / Boss Blind sequence
- advancing to the next blind
- advancing to the next ante
- blind-specific target score and reward scaling
- transition UI through the round-end overlay in `GameScene`
- money carry-over between blinds

Still missing if Milestone 2 should be considered fully polished:

- manual end-to-end verification in the Unity Test Runner / play mode
- boss-specific special rule or debuff behavior, if desired before later milestones

### Milestone 3 - Shop

Status:

- Started in code

Completed in this first slice:

- `ShopState`
- `RunState.EnterShop()`
- `RunState.LeaveShop()`
- Edit Mode coverage for shop entry/exit transitions

Still missing:

- shop screen
- offer generation
- purchase flow
- sell flow
- modifier persistence through shop purchases

### Milestone 4 - Modifiers/Jokers

Status:

- Not implemented

Missing:

- modifier data model
- modifier ownership across run
- modifier effect resolution in score/round flow
- modifier UI

### Milestone 5 - Portfolio polish

Status:

- Partially started only on visuals/UI

Already present:

- custom HUD pass
- Balatro-inspired layout study
- generated placeholder art
- card prefab for presentation

Still missing:

- architecture diagram
- cleaned final README aligned with real code
- changelog/release structure
- gameplay capture/gifs
- final repository hygiene pass

## Important Technical Notes

### Hand name preview behavior

When a card is selected:

- `CardView` emits the selected index
- `RoundScreen` calls `RoundState.ToggleCardSelection`
- `RoundScreen` re-renders
- `RoundPresenter` evaluates currently selected cards
- `HandNameText` is updated from the evaluated `PokerHandType`

This means the hand name is a presenter-derived preview, not a direct field manually mutated by the UI.

### Button wiring

Current setup:

- `PlayHandButton` uses persistent scene event binding to `RoundScreen.OnPlayHandButtonClicked`
- `DiscardButton` uses persistent scene event binding to `RoundScreen.OnDiscardButtonClicked`
- sort buttons are registered in `RoundScreen.Awake`

If button behavior changes later, keep this split in mind to avoid duplicate listeners.

### Round setup and ownership

Current setup:

- `RoundScreen` now asks `RoundState.CreateInitial(...)` for the initial round state
- debug hands still come from `DebugHandFactory`, but deck reconciliation now happens in the domain layer

This means round bootstrap is no longer duplicated in the UI script.

### Layout system

Current setup:

- `HandArea` uses `HorizontalLayoutGroup`
- `PlayedHandArea` uses `HorizontalLayoutGroup`

This is intentionally a temporary basic structure before a future slot-based system.

## Known Issues and Technical Debt

- Docs still describe broader architecture that is not implemented yet
- There are Unity Assistant Account API warnings in the editor; these are unrelated to gameplay logic
- A stale Burst/editor log appeared during the first asmdef refresh when the new test assembly was introduced; gameplay code later recompiled successfully, but the Test Runner should still be checked manually in-editor
- Current architecture is partly state-driven but not yet reducer/store based
- Card layout is still an intermediate UI solution, not the final slot system
- shop exists only as domain/run scaffolding right now; there is no shop UI or transaction logic yet

## Recommended Next Steps

Recommended order from here:

1. Run the new Edit Mode suite manually in Unity Test Runner and clear any remaining editor/test-runner issues
2. Build the first shop screen in `GameScene` and wire it to `RunState.IsInShop`
3. Add minimal shop offers plus one purchase path that spends persistent money
4. Add first modifier/joker layer after shop purchase flow exists
5. Decide whether boss blinds need a simple special rule before modifier depth increases
6. Replace temporary layout rows with a real slot-based hand/play-area system
7. Continue polishing public docs and portfolio materials

## Resume Checklist For Another Machine

When reopening this repository on another computer, read in this order:

1. `Docs/PROJECT_CONTINUITY_SPEC.md`
2. `Assets/Scripts/Core/RunState.cs`
3. `Assets/Scripts/Core/RoundState.cs`
4. `Assets/Scripts/Presenters/RoundPresenter.cs`
5. `Assets/Scripts/MonoBehaviours/RoundScreen.cs`
6. `Assets/Scripts/Core/PokerHandEvaluator.cs`
7. `Assets/Scripts/Core/ScoreCalculator.cs`
8. `Assets/Scripts/Tests/EditMode`
9. `Assets/Scenes/GameScene.unity`

After that, inspect only if needed:

- `Assets/Prefabs/CardViewPrefab.prefab`
- `Assets/Art/UI/Generated`
- `Assets/ReferenceScreenShots`

## Guidance For Future Sessions

If opens this repo later, it should assume:

- the active gameplay loop is centered on `RunState -> RoundState`
- the active UI loop is `RoundScreen -> RoundPresenter -> RoundViewModel`
- Milestone 1 and Milestone 2 core flow are complete in code, with tests added
- Milestone 3 has started at the domain layer through `ShopState` and run phase transitions
- docs in `Docs/` are aspirational unless confirmed by code
- the current stage is "playable ante flow plus early shop scaffolding", not "full run/store architecture"
- legacy files at repo root under `Assets/Scripts/` should not be used as the default source of truth

The first question future work should answer is:

- "Are we wiring the first shop UI, or are we building shop offer/purchase rules first?"

That decision should drive the next implementation slice.
