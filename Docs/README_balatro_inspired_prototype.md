# Unity State-Driven Poker Roguelike Prototype

> A study and portfolio project built in Unity, inspired by the core gameplay loop of a poker roguelike.
>
> **Important:** this project is for **study and portfolio purposes only**. It does **not** use original art, names, assets, UI, or copyrighted content from any existing commercial game.

---

## Overview

This project is a **state-driven card game prototype** focused on learning and demonstrating:

- clean game architecture
- state-driven flow
- reducers and actions
- separation between game rules and presentation
- poker hand evaluation
- score systems
- simple roguelike progression
- automated tests

The goal is **not** to recreate a full commercial game.

The goal is to build a **small, clean, explainable prototype** that proves architectural skill and gameplay programming ability.

---

## Project Goal

Create a playable prototype inspired by the core loop of a poker roguelike, with emphasis on:

- a centralized game state
- predictable gameplay flow
- easy-to-test game rules
- minimal but functional UI
- scalable architecture for adding new rules and modifiers

---

## Core Loop Inspiration

This prototype is inspired by a loop with:

- runs divided into **antes**
- each ante containing **3 blinds**
  - Small Blind
  - Big Blind
  - Boss Blind
- a standard **52-card deck**
- poker hand evaluation
- score based on **Chips × Mult**
- a score target to beat each blind
- a simple shop between rounds
- a few modifier cards similar in spirit to “Jokers”

---

## Scope Philosophy

This project intentionally focuses on the **core loop**, not on cloning a full game.

### Included in the project

- standard card deck
- draw / discard / play flow
- poker hand detection
- score calculation
- round progression
- ante progression
- simple shop
- simple modifiers
- tests

### Explicitly excluded from the initial version

- original art or UI reproduction
- full content replication
- large content pools
- advanced balance systems
- dozens of modifiers
- special card packs
- alternative decks
- complex boss systems
- full progression tree
- polished production visuals

---

## Why This Project Exists

This project exists to improve and demonstrate skills in:

- gameplay architecture
- gameplay systems design
- state modeling
- Unity engineering discipline
- testing gameplay rules
- explaining technical decisions clearly

It is meant to be a **portfolio-friendly technical sample**.

---

# Architecture

## Main Architectural Style

This project uses a **state-driven architecture**.

That means the game is organized around:

1. **current state**
2. **actions/events**
3. **reducers/system logic**
4. **presentation of updated state**

### Core idea

> The UI is **not** the source of truth.
>
> The **state** is the source of truth.

---

## State-Driven Flow

```text
Player input
-> Action
-> Reducer / System logic
-> New State
-> Presenter
-> UI update
```

### Mental model

- the player does something
- that becomes an action
- the game processes the action
- the state changes
- the UI reflects the new state

---

## Main Pieces

### `RunState`
Represents the entire run.

Examples of data:

- current ante
- current blind
- current money
- owned modifiers
- current phase of the run
- win / loss state

### `RoundState`
Represents the current blind/round.

Examples of data:

- deck
- hand
- discard pile
- selected cards
- hands remaining
- discards remaining
- target score
- current accumulated score
- round status

### `PlayAreaState`
Represents the current played selection.

Examples:

- selected cards
- evaluated poker hand
- score result
- triggered effects

### `ShopState`
Represents the shop phase.

Examples:

- available offers
- owned money
- selected offer
- owned modifiers

---

## Actions

### Phase 1 — Core Round

- `StartRunAction`
- `StartBlindAction`
- `ToggleCardSelectionAction`
- `PlaySelectedCardsAction`
- `DiscardSelectedCardsAction`
- `DrawToHandSizeAction`
- `ResolveBlindOutcomeAction`

### Phase 2 — Progression

- `AdvanceToNextBlindAction`
- `AdvanceToNextAnteAction`
- `LoseRunAction`
- `WinRunAction`

### Phase 3 — Shop

- `EnterShopAction`
- `BuyModifierAction`
- `SellModifierAction`
- `LeaveShopAction`

### Phase 4 — Effects

- `ApplyModifierEffectsAction`

---

## Reducers / Systems

### `RunReducer`
Responsible for:

- starting the run
- advancing blinds
- advancing antes
- entering/leaving shop
- ending the run

### `RoundReducer`
Responsible for:

- drawing cards
- discarding cards
- playing selected cards
- reducing remaining hands/discards
- accumulating round score
- determining blind victory or defeat

### `PokerHandEvaluator`
Responsible for identifying the best poker hand:

- High Card
- Pair
- Two Pair
- Three of a Kind
- Straight
- Flush
- Full House
- Four of a Kind
- Straight Flush

### `ScoreCalculator`
Responsible for:

- base chips by hand type
- base mult by hand type
- applying modifiers
- returning the final score result

---

# Domain Model

## Main Domain Types

- `Card`
- `Suit`
- `Rank`
- `BlindType`
- `PokerHandType`
- `RoundPhase`
- `RunPhase`
- `ModifierType`

## Main State Types

- `RunState`
- `RoundState`
- `BlindState`
- `DeckState`
- `HandState`
- `DiscardPileState`
- `SelectedCardsState`
- `PokerHandResult`
- `ScoreResult`
- `ModifierState`
- `ShopState`

---

# Implementation Roadmap

## Milestone 1 — Single Playable Blind

The first milestone is complete when the game allows the player to:

- start a run
- enter a blind with a target score
- draw cards up to hand size
- select up to 5 cards
- play a hand
- evaluate the poker hand
- calculate score
- accumulate score for the blind
- win or lose the blind

### Not included yet

- shop
- modifiers
- boss effects
- extra progression systems

---

## Milestone 2 — Ante Progression

Current code status:

- Small Blind implemented
- Big Blind implemented
- Boss Blind implemented
- ante progression implemented
- reward / money carry-over implemented
- transition overlay in `Assets/Scenes/GameScene.unity` implemented

Still not in this milestone:

- boss-specific debuff rules
- shop flow
- dedicated `RunState` / store-level run loop

---

## Milestone 3 — Shop

Add:

- shop state
- a few offers per shop
- buying and selling modifiers
- return to run after shopping

### Simple modifier examples

- `+4 Mult if hand is Pair`
- `+30 Chips if hand contains an Ace`
- `X2 Mult if hand is Flush`
- `+1 discard`
- `+1 hand`

---

## Milestone 4 — Simple Boss Rule

Add one simple boss-style rule such as:

- Pairs are weakened
- Only 4 cards may be played
- Flush does not score
- Round starts with `-1 discard`

The goal here is to prove that the architecture supports special rules without becoming fragile.

---

# Recommended Development Order

## Step 1 — Card Foundation

Implement:

- `Card`
- `Suit`
- `Rank`
- 52-card deck generation
- shuffle
- draw
- discard
- selection of up to 5 cards

---

## Step 2 — Round State and Flow

Implement:

- `RoundState`
- store/state container
- round actions
- round reducers
- minimal presenters
- minimal debug UI

---

## Step 3 — Poker Hand Evaluation

Implement support for:

- High Card
- Pair
- Two Pair
- Three of a Kind
- Straight
- Flush
- Full House
- Four of a Kind
- Straight Flush

Also validate correct precedence.

---

## Step 4 — Score System

Implement:

- base chips for each hand type
- base mult for each hand type
- final score = `Chips × Mult`
- blind score accumulation
- target score comparison

---

## Step 5 — Round Resolution

Implement:

- hands remaining
- discards remaining
- redraw after play/discard
- blind victory check
- blind defeat check

---

## Step 6 — Run Progression

Implement:

- `RunState`
- blind progression
- ante progression
- simple currency rewards
- run win/loss flow

---

## Step 7 — Shop

Implement:

- `ShopState`
- offers
- buy/sell flow
- return to round flow

---

## Step 8 — Modifiers

Implement a very small set of modifiers.

The goal is to prove that the game can evolve without coupling all logic together.

---

## Step 9 — Portfolio Polish

Add:

- clear logs
- debug overlay
- screenshots / GIFs
- polished README
- automated tests
- architecture explanation

---

# Testing Strategy

## 1. Poker Hand Evaluation Tests

Examples:

- correctly detect Pair
- correctly detect Two Pair
- correctly detect Straight
- correctly detect Flush
- correctly apply hand precedence

---

## 2. Score Tests

Examples:

- Pair returns expected score
- Straight returns expected score
- `Chips × Mult` is correct
- modifiers affect score correctly

---

## 3. Round Flow Tests

Examples:

- playing a hand reduces remaining hands
- discarding reduces remaining discards
- reaching target score wins the blind
- running out of attempts loses the blind

---

## 4. Modifier Tests

Examples:

- Pair modifier only triggers on Pair
- Ace modifier only triggers when an Ace is present
- `+1 discard` correctly modifies the round state

---

# Success Criteria

This project is only considered complete when I can:

- explain the architecture clearly in 3 minutes
- explain the full gameplay flow from input to UI update
- add a new action without AI assistance
- add a new modifier without breaking the architecture
- fix a bug on my own
- demonstrate automated tests running
- demonstrate a short playable run
- present a clear and professional README

---

# Suggested Folder Structure

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
        ScoreReducer.cs
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
        ModifierType.cs

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

# Recommended Final Scope

## Final recommended version of this project

- short run with 2 antes
- 3 blinds per ante
- hand size 8
- limited hands/discards
- poker-hand-based score system
- 3 to 5 simple modifiers
- 1 simple boss rule
- tests for rules and score systems
- professional README

This is already enough to demonstrate:

- gameplay programming
- state-driven architecture
- testable systems design
- code organization
- portfolio quality

---

# Guiding Rule

> First make the round work.
>
> Then make score work.
>
> Then make progression work.
>
> Only after that add extra content.

---

# Summary

This project should be treated as a **small technical prototype** inspired by a poker roguelike core loop.

The priority is:

1. **clean architecture**
2. **clear state transitions**
3. **testable gameplay rules**
4. **small but playable scope**
5. **portfolio-ready presentation**

The goal is not to replicate a full game.

The goal is to prove skill.
