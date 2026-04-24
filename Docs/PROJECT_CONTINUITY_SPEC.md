# Project Continuity Spec

Last updated: 2026-04-23

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
- one concrete gameplay state: `RoundState`
- a presenter-driven UI refresh path
- card selection
- hand preview evaluation while selecting cards
- score calculation
- discard/play flow
- score accumulation toward a target
- basic round-end condition
- Balatro-inspired HUD and playfield UI

Conclusion:

- The current codebase is closer to "Milestone 1 plus partial UI/presentation polish" than to the full architecture described in the older docs
- This file supersedes the older docs when there is a mismatch about implementation status

## Current Codebase Stage

The project has reached:

- a single-scene playable prototype for one blind
- with state, scoring, rendering, selection, play, discard, and score preview working
- with a custom HUD and card presentation already integrated into `RoundScene`

The project has not yet reached:

- real run progression
- real blind progression
- shop
- modifiers/jokers
- boss rules
- tests
- production-ready content pipeline

## Actual Architecture In Use

Current real flow:

- card click
- `CardView` raises selection event
- `RoundScreen` updates `RoundState`
- `RoundPresenter` converts state to `RoundViewModel`
- `RoundScreen` renders texts, buttons, hand cards, and played cards

This is state-driven enough to be workable, but it is not yet the full action/store/reducer architecture described in the original docs.

### Main files that currently define the project

- `Assets/Scripts/Core/RoundState.cs`
- `Assets/Scripts/Core/PokerHandEvaluator.cs`
- `Assets/Scripts/Core/ScoreCalculator.cs`
- `Assets/Scripts/Core/ScoringCardSelector.cs`
- `Assets/Scripts/Presenters/RoundPresenter.cs`
- `Assets/Scripts/MonoBehaviours/RoundScreen.cs`
- `Assets/Scripts/View/CardView.cs`
- `Assets/Scripts/View/CardViewModel.cs`
- `Assets/Scripts/View/RoundViewModel.cs`
- `Assets/Scenes/RoundScene.unity`
- `Assets/Prefabs/CardViewPrefab.prefab`

## Scene and UI Status

Main scene:

- `Assets/Scenes/RoundScene.unity`

Main screen script:

- `Assets/Scripts/MonoBehaviours/RoundScreen.cs`

UI state today:

- Left HUD panel exists and is bound to gameplay state
- `HandNameText` updates from evaluated selected cards
- `PlayHandButton` and `DiscardButton` are wired as persistent scene button events
- sort buttons are still wired in code at runtime
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
- last played cards tracking
- last played hand tracking
- status text updates

Partially implemented only as data/presentation:

- money
- ante
- round number
- blind reward text

Not implemented yet:

- transition to next blind
- transition to next ante
- boss blind behavior
- shop phase
- run win/loss loop outside the single round

## Milestone Mapping

### Milestone 1 - Single playable blind

Status:

- Mostly implemented

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

Still missing before calling it "solid":

- automated tests
- clearer round loss/win messaging/state transitions
- safer separation between domain state and UI concerns
- more validation around edge cases

### Milestone 2 - Ante/blind progression

Status:

- Not implemented as actual flow

Only partial groundwork exists:

- `Ante`
- `RoundNumber`
- blind name text
- blind reward text

Missing:

- small blind / big blind / boss blind sequence
- advancing to the next blind
- advancing to the next ante
- per-blind config/state objects
- transition UI/state

### Milestone 3 - Shop

Status:

- Not implemented

Missing:

- shop state
- shop screen
- offer generation
- purchase flow
- sell flow

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

- tests
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

### Layout system

Current setup:

- `HandArea` uses `HorizontalLayoutGroup`
- `PlayedHandArea` uses `HorizontalLayoutGroup`

This is intentionally a temporary basic structure before a future slot-based system.

### Legacy code still in repo

There is old prototype code that does not represent the active architecture:

- `Assets/Scripts/GameStateReducer.cs`
- `Assets/Scripts/Core/GameState.cs`
- `Assets/Scripts/IGameAction.cs`
- `Assets/Scripts/Card.cs`

This code appears to be from an earlier experiment and should not be treated as the main gameplay flow.

Recommendation:

- either remove or archive this legacy code once the current prototype is stable

## Known Issues and Technical Debt

- No automated tests currently exist
- Docs describe systems that are not implemented yet
- There is a warning in `Assets/Scripts/Card.cs` about `_wasDragged` being unused
- There are Unity Assistant Account API warnings in the editor; these are unrelated to gameplay logic
- Current architecture is partly state-driven but not yet reducer/store based
- Round progression is still single-scene and single-round focused
- Card layout is still an intermediate UI solution, not the final slot system

## Recommended Next Steps

Recommended order from here:

1. Stabilize Milestone 1 with tests for `PokerHandEvaluator`, `ScoreCalculator`, and `RoundState`
2. Remove or quarantine legacy prototype files that are no longer part of the active architecture
3. Introduce a real blind progression model for small blind / big blind / boss blind
4. Extract round progression out of ad hoc fields into clearer domain types
5. Add shop state and a minimal shop flow
6. Add first modifier/joker layer only after blind progression is working
7. Replace temporary layout rows with a real slot-based hand/play-area system
8. Rewrite public README files so they match what is actually implemented

## Resume Checklist For Another Machine

When reopening this repository on another computer, read in this order:

1. `Docs/PROJECT_CONTINUITY_SPEC.md`
2. `Assets/Scripts/Core/RoundState.cs`
3. `Assets/Scripts/Presenters/RoundPresenter.cs`
4. `Assets/Scripts/MonoBehaviours/RoundScreen.cs`
5. `Assets/Scripts/Core/PokerHandEvaluator.cs`
6. `Assets/Scripts/Core/ScoreCalculator.cs`
7. `Assets/Scenes/RoundScene.unity`

After that, inspect only if needed:

- `Assets/Prefabs/CardViewPrefab.prefab`
- `Assets/Art/UI/Generated`
- `Assets/ReferenceScreenShots`

## Guidance For Future Sessions

If opens this repo later, it should assume:

- the active gameplay loop is centered on `RoundState`
- the active UI loop is `RoundScreen -> RoundPresenter -> RoundViewModel`
- docs in `Docs/` are aspirational unless confirmed by code
- the current stage is "single playable blind prototype with UI pass", not "full run architecture"
- legacy files at repo root under `Assets/Scripts/` should not be used as the default source of truth

The first question future work should answer is:

- "Are we stabilizing Milestone 1, or are we starting Milestone 2?"

That decision should drive the next implementation slice.
