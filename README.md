# Unity State-Driven Poker Roguelike Prototype

> A portfolio project focused on **gameplay architecture**, **state-driven systems**, and **testable game rules**.  
> Inspired by the core loop of a poker roguelike, built from scratch in **Unity + C#** for study and professional growth.

---

## Overview

This repository is a **systems-focused card game prototype** created to demonstrate how I design and implement gameplay features with:

- clear state modeling
- state-driven game flow
- separation between game rules and presentation
- testable core logic
- scalable architecture for future features

This is **not a commercial clone** of any existing game.  
It is an original portfolio study inspired by the structure of a poker roguelike loop, using **custom code, custom architecture, and original implementation decisions**.

---

## Why I built this project

My goal with this project is to strengthen and showcase skills that matter for gameplay programming roles:

- gameplay systems architecture
- state-driven design
- maintainable C# code
- rule modeling for card games
- reducer-based update flow
- UI decoupling
- automated testing of gameplay behavior
- technical communication through a professional repository

This project exists as a **portfolio piece**, meaning it is intentionally designed to be easy to inspect, understand, and discuss in an interview.

---

## What this project demonstrates

### Architecture
- centralized game state
- predictable state transitions
- reducer-based logic flow
- presentation layer separated from gameplay rules
- scalable feature organization

### Gameplay systems
- 52-card deck generation
- draw / discard flow
- hand selection
- poker hand evaluation
- score calculation based on `Chips x Mult`
- blind progression
- ante progression
- money carry-over between blinds

### Engineering practices
- readable naming
- modular responsibility split
- Edit Mode tests for core rules
- clear repository structure
- documentation written for other developers and recruiters

---

## Core gameplay concept

The project is built around a simplified poker roguelike loop:

1. Start a run
2. Enter a blind
3. Draw cards into hand
4. Select and play up to 5 cards
5. Evaluate the poker hand
6. Calculate score
7. Compare score against the blind target
8. Progress through blinds and antes
9. Visit a simple shop between rounds
10. Buy modifiers that change future scoring or round flow

The focus is not on content volume.  
The focus is on **building a clean and expandable gameplay foundation**.

---

## Design goals

This project is considered successful when I can:

- explain the architecture in under 3 minutes
- add a new gameplay action without breaking the existing flow
- add a new modifier with minimal changes outside its feature area
- debug gameplay behavior through state transitions
- run tests that validate hand evaluation and score rules
- present the repository as a professional portfolio sample

---

## Architecture overview

The project follows a **state-driven architecture**.

The main idea is:

> the game is driven by **state transitions**, not by UI scripts directly mutating gameplay data.

### High-level flow

```text
Player Input
-> Action
-> Store
-> Reducer
-> New State
-> Presenter
-> UI Refresh
```

### Reasoning behind this approach

This architecture was chosen to make the project:

- easier to reason about
- easier to test
- easier to expand
- easier to debug
- easier to explain in interviews

Instead of spreading game rules across multiple `MonoBehaviour` scripts, the project centralizes decision-making around:

- current state
- incoming action
- deterministic state transition

---

## Main architectural components

### `RunState`
Represents the full run.

Examples:
- current ante
- current blind
- current money
- current shop transition state
- owned modifiers
- current phase of the run
- overall win / loss state

### `RoundState`
Represents the active blind.

Examples:
- current deck
- hand
- discard pile
- selected cards
- hands remaining
- discards remaining
- blind target score
- accumulated round score
- round status

`RunState` owns run-level progression such as money, blind advancement, and shop entry/exit.

### `GameStateStore`
Stores the current state and coordinates action dispatch.

Responsibilities:
- hold the current state
- receive actions
- call reducers
- publish state changes

### Reducers
Reducers are responsible for transforming state.

Examples:
- start a run
- start a blind
- draw cards
- discard selected cards
- play selected cards
- advance to next blind
- resolve win / loss state

### Presenters
Presenters convert raw game state into UI-friendly data.

Responsibilities:
- format labels
- expose derived values for the interface
- keep UI code simple and dumb

### UI
The UI layer should:
- send actions
- render view models
- avoid owning gameplay rules

---

## Planned folder structure

```text
Assets/
  Scripts/
    Core/
      State/
        RunState.cs
        RoundState.cs
        BlindState.cs
        ShopState.cs
      Actions/
        StartRunAction.cs
        StartBlindAction.cs
        ToggleCardSelectionAction.cs
        PlaySelectedCardsAction.cs
        DiscardSelectedCardsAction.cs
        AdvanceToNextBlindAction.cs
        EnterShopAction.cs
        BuyModifierAction.cs
      Reducers/
        RunReducer.cs
        RoundReducer.cs
        ScoringReducer.cs
      Store/
        GameStateStore.cs

    Domain/
      Cards/
        Card.cs
        Rank.cs
        Suit.cs
        DeckBuilder.cs
      Poker/
        PokerHandType.cs
        PokerHandEvaluator.cs
        PokerHandResult.cs
      Scoring/
        ScoreResult.cs
        ScoreCalculator.cs
      Modifiers/
        ModifierState.cs
        ModifierEffectType.cs

    Presentation/
      Presenters/
        RunPresenter.cs
        RoundPresenter.cs
        ShopPresenter.cs
      ViewModels/
        RoundViewModel.cs
        ShopViewModel.cs
      UI/
        RunScreen.cs
        RoundScreen.cs
        ShopScreen.cs

    Tests/
      EditMode/
        PokerHandEvaluatorTests.cs
        ScoreCalculatorTests.cs
        RoundReducerTests.cs
        ModifierTests.cs
```

---

## Systems roadmap

### Milestone 1 — Single Blind Prototype
- [x] create 52-card deck
- [x] draw to hand size
- [x] select up to 5 cards
- [x] play selected cards
- [x] detect poker hand
- [x] calculate score
- [x] win / lose a single blind

### Milestone 2 — Ante Flow
- [x] small blind
- [x] big blind
- [x] boss blind
- [x] blind reward
- [x] ante progression

### Milestone 3 — Shop
- [ ] basic shop screen
- [x] money system
- [x] enter / leave shop flow
- [ ] buy / sell modifiers
- [ ] persistent modifiers across the run

### Milestone 4 — Modifier System
- [ ] hand-based score bonus
- [ ] rank-based bonus
- [ ] round economy bonus
- [ ] extra hand / discard effects

### Milestone 5 — Portfolio Polish
- [ ] debug overlay
- [ ] clean screenshots
- [ ] gameplay GIF
- [ ] final README pass
- [ ] architecture diagram
- [ ] fully documented test coverage

---

## Planned gameplay systems

### Cards and deck
- standard 52-card deck
- shuffle
- draw
- discard
- selected cards for play

### Poker hand evaluation
- high card
- pair
- two pair
- three of a kind
- straight
- flush
- full house
- four of a kind
- straight flush

### Scoring
- base chips per hand type
- base mult per hand type
- final score = `Chips x Mult`
- modifier hooks for future expansion

### Blind flow
- target score
- hands remaining
- discards remaining
- round result
- progression to the next blind
- progression to the next ante
- blind rewards carried into persistent money

### Modifiers
A simplified modifier system inspired by score-changing run-based card games.

Examples:
- `+4 Mult if hand is Pair`
- `+30 Chips if played hand contains an Ace`
- `x2 Mult if hand is Flush`
- `+1 discard`
- `+1 hand`

---

## Testing strategy

The project is designed so the most important rules can be tested **without relying on scene setup or UI**.

### Priority test areas

#### Poker hand evaluation
- correctly detect `Pair`
- correctly detect `Two Pair`
- correctly detect `Straight`
- correctly detect `Flush`
- validate hand ranking precedence

#### Score calculation
- validate base hand score
- validate `Chips x Mult`
- validate modifier influence
- validate final play result

#### Round flow
- playing a hand consumes a hand use
- discarding consumes a discard use
- hitting blind target wins the round
- running out of plays without enough score loses the round

#### Modifiers
- effect only activates under valid conditions
- state updates correctly
- effect composition remains predictable

---

## Technical stack

- **Engine:** Unity
- **Language:** C#
- **Testing:** Unity Test Framework / NUnit
- **Pattern focus:** State-driven architecture
- **Target purpose:** Portfolio / study / gameplay systems practice

---

## How to read this repository

If you are a recruiter or another developer reviewing this project, the best reading order is:

1. `README.md`
2. `BlindState` / `RoundState`
3. `RoundPresenter`
4. `RoundScreen`
5. poker hand evaluator
6. score calculator
7. tests

That path should give a quick understanding of both:
- the game loop
- the architectural reasoning

---

## What I want recruiters to notice

This repository is meant to communicate that I can:

- break a game feature into clean systems
- design gameplay architecture intentionally
- model rules clearly
- separate logic from presentation
- write code that is easier to maintain and test
- learn through deliberate technical projects

This is less about cloning a finished commercial game and more about showing how I think as a **gameplay programmer**.

---

## Current status

**Status:** In active development

Current playable slice:
- one playable scene: `Assets/Scenes/GameScene.unity`
- hand evaluation working
- score calculation working
- state-driven round flow working
- Small Blind -> Big Blind -> Boss Blind -> next ante progression working
- blind rewards and money carry-over between blinds working
- `RunState` owns run-level progression and shop transitions
- `ShopState` exists and the run can transition `Blind -> Shop -> Blind`
- Edit Mode test assemblies compile successfully

Still missing from later milestones:
- shop UI and offer/purchase flow
- modifiers / jokers
- boss-specific debuff rules
- full manual verification in Unity Test Runner

---

## Media

### Screenshots
> Coming soon

### Gameplay GIF
> Coming soon

### Architecture diagram
> Coming soon

---

## Future improvements

Possible next steps after the core prototype:

- richer modifier interactions
- boss blind rule variations
- better presentation layer visuals
- run summary screen
- save / load support
- telemetry for balancing experiments
- richer debug tools for state inspection

---

## Notes on inspiration and originality

This project is **inspired by the structural loop of poker roguelike design**, but it is being implemented as an original study project for portfolio purposes.

The goal is to study:
- game flow
- scoring systems
- state architecture
- modifier interactions
- gameplay programming patterns

It does **not** aim to reproduce commercial content, original art, branding, or proprietary assets.

---

## Personal learning goals

Through this project, I want to improve my ability to:

- build gameplay systems from scratch
- reason about state and flow
- structure code for readability
- make rules easier to test
- explain technical decisions clearly
- create portfolio projects that reflect real engineering thinking

---

## Repository checklist

- [x] first playable blind
- [x] poker hand evaluator
- [x] score calculator
- [ ] round reducer flow
- [x] blind progression
- [x] shop prototype
- [ ] modifier system
- [x] tests for core systems
- [ ] architecture diagram
- [ ] gameplay capture
- [ ] polished portfolio README

---

## Contact

This repository is part of my gameplay programming portfolio.

If you are reviewing my work, thank you for your time.
