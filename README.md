# Unity State-Driven Poker Roguelike Prototype

> A portfolio project focused on gameplay architecture, state-driven systems, and testable game rules.
> Inspired by the core loop of a poker roguelike, built from scratch in Unity + C# for study and professional growth.

## Overview

This repository is a systems-focused card game prototype created to demonstrate:

- clear state modeling
- state-driven game flow
- separation between game rules and presentation
- testable core gameplay logic
- scalable architecture for future shop, joker, boss blind, and portfolio features

This is not a commercial clone. It is an original portfolio study inspired by poker roguelike structure, using custom code, custom architecture, and original implementation decisions.

## Current Status

Status as of 2026-05-04: active development, playable ante flow, shop/joker v1 implemented.

Current playable slice:

- one playable scene: `Assets/Scenes/GameScene.unity`
- standard 52-card deck, shuffle, draw, play, discard, and selection cap
- poker hand evaluation and score calculation
- preview scoring while cards are selected
- Small Blind -> Big Blind -> The Club boss blind -> next ante progression
- blind rewards and money carry-over between blinds
- `Blind -> Shop -> Blind` transition flow
- textual shop overlay with offer selection, buy, reroll, and continue actions
- persistent owned jokers across blinds
- additive joker score modifiers for Chips and Mult
- owned jokers rendered in the upper playfield area
- Edit Mode tests for core systems, run flow, shop flow, and modifier behavior

Still missing:

- structured shop offer slot UI
- sell flow
- randomized shop generation
- richer joker effects such as xMult, economy, extra hand, or extra discard
- boss debuff feedback polish
- slot-based hand/play-area layout
- final screenshots, gameplay GIF, architecture diagram, changelog, and release/tag polish
- manual Unity Test Runner verification with the project closed in other Unity instances

## Core Gameplay Concept

The project is built around a simplified poker roguelike loop:

1. Start a run.
2. Enter a blind.
3. Draw cards into hand.
4. Select and play up to 5 cards.
5. Evaluate the poker hand.
6. Calculate score from `Chips x Mult`.
7. Apply owned joker modifiers.
8. Compare score against the blind target.
9. Progress through blinds and antes.
10. Visit the shop between won blinds.
11. Buy jokers that modify future scoring.

The focus is not content volume. The focus is building a clean and expandable gameplay foundation that is easy to inspect in a portfolio review.

## Architecture Overview

The project follows a state-driven architecture:

```text
Player Input
-> RoundScreen
-> RunState
-> RoundState / ShopState
-> RoundPresenter
-> RoundViewModel
-> UI Refresh
```

The current implementation is intentionally simpler than a full action/store/reducer architecture. Domain state owns gameplay decisions, while `RoundScreen` acts as the scene bridge and `RoundPresenter` converts domain state into UI text, button states, and card view models.

Important current files:

- `Assets/Scripts/Core/RunState.cs`
- `Assets/Scripts/Core/RoundState.cs`
- `Assets/Scripts/Core/BlindState.cs`
- `Assets/Scripts/Core/ShopState.cs`
- `Assets/Scripts/Core/ShopOfferState.cs`
- `Assets/Scripts/Core/JokerCatalog.cs`
- `Assets/Scripts/Core/JokerState.cs`
- `Assets/Scripts/Core/RunModifierService.cs`
- `Assets/Scripts/Core/PokerHandEvaluator.cs`
- `Assets/Scripts/Core/ScoreCalculator.cs`
- `Assets/Scripts/Presenters/RoundPresenter.cs`
- `Assets/Scripts/MonoBehaviours/RoundScreen.cs`
- `Assets/Scripts/View/RoundViewModel.cs`
- `Assets/Scripts/Tests/EditMode`

## Actual Folder Structure

```text
Assets/
  Scripts/
    Core/
      BlindState.cs
      DeckBuilder.cs
      DeckShuffler.cs
      HandBaseScore.cs
      JokerCatalog.cs
      JokerState.cs
      PokerHandEvaluator.cs
      RoundState.cs
      RunModifierService.cs
      RunState.cs
      ScoreCalculator.cs
      ShopOfferState.cs
      ShopState.cs
    Data/
      CardData.cs
      JokerData.cs
    Debug/
      DebugCardFactory.cs
      DebugHandFactory.cs
      DebugHandScenario.cs
    Enums/
      BlindType.cs
      JokerBonusType.cs
      JokerConditionType.cs
      PokerHandType.cs
      Rank.cs
      RoundPhase.cs
      RunPhase.cs
      Suit.cs
    MonoBehaviours/
      RoundScreen.cs
    Presenters/
      RoundPresenter.cs
    Tests/
      EditMode/
    Utility/
    View/
```

Future architecture may still introduce a formal action/store/reducer layer, but that refactor is intentionally deferred until the playable loop is stronger.

## Systems Roadmap

### Milestone 1 - Single Blind Prototype

- [x] create 52-card deck
- [x] draw to hand size
- [x] select up to 5 cards
- [x] play selected cards
- [x] discard selected cards
- [x] detect poker hand
- [x] calculate score
- [x] win / lose a single blind
- [x] Edit Mode coverage for core rules

### Milestone 2 - Ante Flow

- [x] Small Blind
- [x] Big Blind
- [x] The Club boss blind identity and progression
- [x] blind reward
- [x] ante progression
- [x] money carry-over
- [x] `Blind -> Shop -> Blind` transition path
- [x] Clubs debuffed during The Club

### Milestone 3 - Shop

- [x] money system
- [x] enter / leave shop flow
- [x] shop state model
- [x] deterministic offer generation by page
- [x] offer selection
- [x] buy selected offer
- [x] reroll offers
- [x] persistent owned jokers across the run
- [x] textual shop overlay in `GameScene`
- [x] structured 3-slot shop offer UI
- [ ] sell flow
- [ ] randomized shop generation

### Milestone 4 - Joker / Modifier System

- [x] `JokerData` data model
- [x] `JokerCatalog`
- [x] `JokerState` run ownership
- [x] additive Chips bonuses
- [x] additive Mult bonuses
- [x] hand-type conditions
- [x] rank/suit/face-card conditions
- [x] preview and real score modifier application
- [x] owned joker rendering
- [ ] xMult effects
- [ ] economy effects
- [ ] extra hand / discard effects
- [ ] richer balancing and rarity model

### Milestone 5 - Portfolio Polish

- [x] custom HUD pass
- [x] generated placeholder UI art
- [x] screenshot references
- [ ] clean final screenshots
- [ ] gameplay GIF
- [ ] architecture diagram
- [ ] changelog and release tags
- [ ] final README pass after shop UI consolidation
- [ ] documented manual test pass

## Current Gameplay Systems

### Cards and Deck

Implemented:

- standard 52-card deck generation
- shuffle
- draw
- discard selected cards and redraw
- play selected cards and redraw
- selected card cap of 5

### Poker Hand Evaluation

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

Current rule note:

- `Flush` requires exactly 5 played cards of the same suit.
- `Straight Flush` depends on the same flush validation.

### Scoring

Implemented:

- base hand score table
- scoring card selection
- final score from `Chips x Mult`
- round score accumulation
- preview score while selecting cards
- joker modifier application in preview and final play scoring

Current simplification:

- for `High Card`, only the highest card scores
- for all other hand types, the current implementation scores all played cards
- joker effects are additive Chips/Mult only

### Shop and Jokers

Implemented:

- deterministic joker catalog with 9 offers across 3 offer pages
- shop state with selected offer index, reroll count, and reroll cost
- buy selected offer when affordable
- block bought/unaffordable offers
- reroll offers for money
- mark already owned offers as bought
- persist owned jokers in `RunState`
- apply owned jokers through `RunModifierService`

Next shop slice:

- replace textual `ShopOffersText` with structured offer view models and 3 offer slots
- keep deterministic offers for v1
- keep additive Chips/Mult for v1

## Testing Strategy

The project keeps core rules testable without relying on scene setup or UI.

Current Edit Mode test areas:

- `PokerHandEvaluator`
- `ScoreCalculator`
- `BlindState`
- `RoundState`
- `RunState`
- `RunModifierService`

Manual validation still needed in Unity:

- win Small Blind, enter shop, buy `Glass Joker`, see money decrease and joker render
- continue to Big Blind and confirm `Glass Joker` adds +10 Chips in preview and final score
- reroll offers and confirm money decreases and offer page changes
- attempt purchase without enough money and confirm the action is blocked
- attempt duplicate purchase and confirm no duplicate joker is added

Batchmode note:

- A Unity batchmode Edit Mode run was attempted on 2026-05-04.
- It did not run because another Unity instance had the project open.
- The test suite should be run manually in Unity Test Runner after closing other instances.

## Recommended Next Features

Priority order:

1. Consolidate Shop/Jokers v1 with structured 3-slot offer UI and offer view models.
2. Add manual and automated coverage around shop UI behavior where practical.
3. Polish Boss Blind v1 feedback for `The Club`, whose current rule debuffs Clubs.
4. Add portfolio polish: screenshots, gameplay GIF, architecture diagram, changelog, and release tags.
5. Consider full action/store/reducer refactor only after the playable loop is stronger.

## Worktree Note

As of this documentation update, the worktree already included unrelated local changes:

- `JokerData.cs` moved from `Assets/Scripts/Core` to `Assets/Scripts/Data`
- TextMesh Pro fallback asset modified

Do not revert those changes without explicit confirmation.

## Technical Stack

- Engine: Unity 6000.3.12f1
- Language: C#
- Testing: Unity Test Framework / NUnit
- Pattern focus: state-driven gameplay architecture
- Target purpose: portfolio / study / gameplay systems practice

## Notes on Inspiration and Originality

This project is inspired by the structural loop of poker roguelike design. It does not aim to reproduce commercial content, original art, branding, or proprietary assets.

The goal is to study and demonstrate game flow, scoring systems, state architecture, modifier interactions, and gameplay programming patterns.

## Contact

This repository is part of my gameplay programming portfolio.
