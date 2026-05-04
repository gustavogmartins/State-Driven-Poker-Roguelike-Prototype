# Project Continuity Spec

Last updated: 2026-05-04

## Purpose

This file is the current working spec for the project.

It exists to let:

- the project resume on another computer without re-mapping the repository
- a future AI session get useful context quickly
- the repo keep a single source of truth for current status, not only original intent

When this file disagrees with older docs, this file should be treated as the current source of truth.

## Project Summary

Project name:

- Unity State-Driven Poker Roguelike Prototype

Project intent:

- Build a poker-roguelike prototype inspired by the gameplay loop of Balatro
- Keep the project portfolio-friendly and technically explainable
- Prioritize state-driven gameplay rules, clean UI binding, and scalable architecture

Important framing:

- This is a study/portfolio project
- It should not be presented as a commercial clone
- The codebase should continue being treated as an original prototype

## Docs vs Reality

Older docs described a broader aspirational action/store/reducer architecture. The real codebase is currently a simpler state-driven architecture centered on `RunState`, `RoundState`, and `ShopState`.

Actually implemented today:

- one active playable scene: `Assets/Scenes/GameScene.unity`
- one run-level gameplay state: `RunState`
- one blind-level gameplay state: `RoundState`
- one shop-level gameplay state: `ShopState`
- presenter-driven UI refresh through `RoundPresenter` and `RoundViewModel`
- card selection, play, discard, draw, sort, and score preview
- Small Blind / Big Blind / The Club boss progression
- ante rollover after The Club
- persistent money and blind rewards
- `Blind -> Shop -> Blind` flow after won blinds
- deterministic shop offer pages
- structured offer selection, buy, reroll, sell, and continue actions
- persistent owned jokers
- additive Chips/Mult joker modifiers applied to preview and played score
- structured shop overlay in `GameScene`
- Edit Mode tests for core gameplay, run flow, shop flow, and modifier behavior

Not implemented yet:

- full action/store/reducer layer
- randomized shop generation
- xMult, economy, extra hand, or extra discard joker effects
- boss debuff feedback polish
- final slot-based hand/play-area layout
- final portfolio media and release polish

Conclusion:

- Milestone 1 and Milestone 2 are complete in code.
- Milestone 3 is mostly complete for v1 and now includes structured offers, buy, sell, and progressive reroll.
- Milestone 4 has a v1 implementation through additive jokers.
- The next best feature slice is random shop generation/weights or boss feedback polish, not a store/reducer refactor.

## Current Codebase Stage

The project has reached:

- a single-scene playable prototype with real ante/blind progression
- state, scoring, rendering, selection, play, discard, sort, and score preview
- a custom HUD and card presentation integrated into `GameScene`
- a structured shop overlay with clickable offers, buy/reroll/sell/continue actions
- persistent owned jokers that affect scoring
- 56 Edit Mode test methods covering the main domain behavior

The project has not yet reached:

- random shop generation and rarity/balancing rules
- boss-specific feedback polish
- non-additive modifier effects
- full manual Unity Test Runner verification after the newest tests
- production-ready content pipeline

## Actual Architecture In Use

Current real flow:

```text
CardView / Button click
-> RoundScreen
-> RunState
-> RoundState / ShopState
-> RoundPresenter
-> RoundViewModel
-> RoundScreen render
```

Important architecture notes:

- `RoundState.CreateInitial(...)` owns round bootstrap.
- `BlindState` owns blind type, ante, reward, and target score progression.
- `RunState` owns money, current phase, blind advancement, shop entry/exit, purchases, sells, rerolls, shop refresh count, and owned jokers.
- `ShopState` owns next blind, offers, selected offer, offer page index, and reroll state.
- `JokerCatalog` owns deterministic offer pages and joker data lookup.
- `RunModifierService` applies owned joker effects to `ScoreResult`.
- `RoundPresenter` derives UI copy, button states, card view models, shop text, and owned joker cards.
- gameplay scripts compile through `Assets/Scripts/StateDrivenPokerRoguelike.asmdef`.
- gameplay tests live in `Assets/Scripts/Tests/EditMode`.

This is state-driven enough to be workable, but it is not yet the full action/store/reducer architecture from the original target docs.

### Main files that currently define the project

- `Assets/Scripts/Core/RunState.cs`
- `Assets/Scripts/Core/RoundState.cs`
- `Assets/Scripts/Core/BlindState.cs`
- `Assets/Scripts/Core/ShopState.cs`
- `Assets/Scripts/Core/ShopOfferState.cs`
- `Assets/Scripts/Core/JokerCatalog.cs`
- `Assets/Scripts/Core/JokerState.cs`
- `Assets/Scripts/Data/JokerData.cs`
- `Assets/Scripts/Core/RunModifierService.cs`
- `Assets/Scripts/Core/PokerHandEvaluator.cs`
- `Assets/Scripts/Core/ScoreCalculator.cs`
- `Assets/Scripts/Presenters/RoundPresenter.cs`
- `Assets/Scripts/MonoBehaviours/RoundScreen.cs`
- `Assets/Scripts/View/RoundViewModel.cs`
- `Assets/Scripts/View/ShopOfferViewModel.cs`
- `Assets/Scripts/View/OfferView.cs`
- `Assets/Scenes/GameScene.unity`
- `Assets/Prefabs/CardViewPrefab.prefab`
- `Assets/Prefabs/Offer.prefab`

## Scene and UI Status

Main scene:

- `Assets/Scenes/GameScene.unity`

Main screen script:

- `Assets/Scripts/MonoBehaviours/RoundScreen.cs`

UI state today:

- left HUD panel exists and is bound to gameplay state
- `HandNameText` updates from evaluated selected cards
- `PlayHandButton` and `DiscardButton` are wired as persistent scene button events
- sort buttons are registered in code at runtime
- round-end overlay primary action sends won blinds to shop
- shop overlay exists in `GameScene`
- shop overlay shows 3 structured offer slots, selected/bought/affordable states, money, next blind, buy, reroll, and continue copy
- `Offer.prefab` is instantiated into the fixed `ShopOverlay/Panel/OfferSlots` container
- `OfferView` owns the clickable offer slot, `Toggle`, accent, status text, and child `BuyJokerButton`
- shop buttons are resolved and registered in `RoundScreen`
- owned jokers are rendered as card-like views under `UpperGlass`
- during shop phase, clicking an owned joker selects it and reveals its `Sell` button
- bottom hand container uses `HorizontalLayoutGroup`
- middle played-cards container uses `HorizontalLayoutGroup`
- `CardViewPrefab` is reused for hand cards, played cards, and owned joker cards
- generated placeholder UI art exists under `Assets/Art/UI/Generated`
- screenshot references exist under `Assets/ReferenceScreenShots`

Current UI limitation:

- card layout is still an intermediate row layout before a future slot-based system
- sell UI is functional but still visually basic

## Gameplay Systems Implemented

### Deck and hand

Implemented:

- standard 52-card deck generation
- shuffle
- draw to hand size
- discard selected cards and redraw
- play selected cards and redraw
- selected card cap of 5
- sort hand by rank
- sort hand by suit

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

Rule note:

- `Flush` validates only when exactly 5 cards are played and all share the same suit.
- `Straight Flush` depends on the same flush validation.

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
- owned joker modifier application in preview and final scoring

Current scoring simplification:

- for `High Card`, only the highest card scores
- for all other hand types, the current implementation scores all played cards
- jokers currently add flat Chips or flat Mult only

Main files:

- `Assets/Scripts/Core/HandBaseScore.cs`
- `Assets/Scripts/Core/ScoreCalculator.cs`
- `Assets/Scripts/Core/ScoreResult.cs`
- `Assets/Scripts/Core/ScoringCardSelector.cs`
- `Assets/Scripts/Core/RunModifierService.cs`
- `Assets/Scripts/Utility/CardChipValueUtility.cs`

### Shop and jokers

Implemented:

- `JokerData` model
- `JokerCatalog` with 9 deterministic jokers
- 3 deterministic offer pages
- `ShopOfferState`
- `ShopState`
- structured `ShopOfferViewModel`
- clickable offer selection with a `ToggleGroup`
- buy through the selected offer slot's `BuyJokerButton`
- prevent buying unaffordable or already bought offers
- mark already owned offers as purchased
- shop refreshes to a new offer page every time the run enters a shop
- reroll refreshes offers, starts at `$5`, and increases after each refresh in the current shop
- sell owned jokers during shop for half cost rounded down, minimum `$1`
- sold jokers are removed from owned modifiers immediately
- sold visible offers become buyable again
- persistent owned jokers through `RunState.OwnedJokers`
- owned joker rendering in the upper playfield area
- additive Chips/Mult modifiers
- conditions for Always, Ace, Pair, Clubs, Straight, Hearts, Flush, face cards, and Two Pair

Still missing:

- random shop generation
- rarity, weights, and balancing rules
- xMult, economy, extra hand, and extra discard effects

### Round and run flow

Implemented:

- target score for each blind
- hands remaining
- discards remaining
- current score accumulation
- blind cleared check
- round end when blind is cleared or hands reach zero
- explicit domain flags for round over / round won / round lost
- last played cards tracking
- last played hand tracking
- real Small Blind / Big Blind / The Club boss sequence
- advance to shop after a blind win
- exit shop into the pending blind
- advance to next ante after clearing The Club
- blind reward and money carry-over between blinds
- run loss state when the player loses a blind

Partially implemented:

- The Club boss identity and Clubs debuff exist; card-level debuff feedback still needs polish.

## Milestone Mapping

### Milestone 1 - Single playable blind

Status:

- Complete in code

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
- clear round win/loss messaging and derived state
- constructor/state edge-case validation
- Edit Mode tests for evaluator, scoring, blind, round, and run behavior

Remaining caveat:

- run the Edit Mode suite manually in Unity Test Runner after closing any other Unity instance that has this project open

### Milestone 2 - Ante/blind progression

Status:

- Complete in code

Completed:

- `BlindState` domain model
- `RunState` run-level domain model
- Small Blind / Big Blind / The Club boss sequence
- advancing to next blind
- advancing to next ante
- blind-specific target score and reward scaling
- transition UI through the round-end overlay
- money carry-over between blinds
- shop entry between won blinds

Still missing for polish:

- boss-specific visual feedback polish
- manual play-mode pass through multiple antes

### Milestone 3 - Shop

Status:

- Mostly complete for v1; structured UI, buy, sell, and progressive reroll exist

Completed:

- `ShopState`
- `ShopOfferState`
- deterministic offer generation through `JokerCatalog`
- shop refresh count at run level
- `RunState.EnterShop()`
- `RunState.LeaveShop()`
- each shop phase loads a fresh deterministic offer page
- structured `ShopOfferViewModel`
- `Offer.prefab`
- `OfferView`
- clickable offer slots with `ToggleGroup`
- buy through the selected slot's `BuyJokerButton`
- reroll offers through the global reroll button
- reroll cost starts at `$5` and increases after each refresh
- spend persistent money on buy/reroll
- mark purchased offers
- sell owned jokers from `UpperGlass` during shop
- sold visible offers become buyable again
- preserve bought jokers across blinds
- structured shop overlay in `GameScene`
- Edit Mode coverage for shop entry, exit, purchase, selection, reroll, sell, and purchased offer marking

Still missing:

- randomized shop generation
- rarity, weights, and balancing rules
- final visual polish

### Milestone 4 - Modifiers/Jokers

Status:

- v1 implemented

Completed:

- `JokerData`
- `JokerState`
- `JokerCatalog`
- `JokerBonusType`
- `JokerConditionType`
- `RunModifierService`
- additive Chips bonuses
- additive Mult bonuses
- basic condition matching
- preview score modifier application
- final play score modifier application
- owned joker rendering
- sold joker removal from future preview/final score
- Edit Mode coverage for modifier behavior

Still missing:

- xMult
- economy effects
- extra hand/discard effects
- triggered effect feedback in UI
- balancing/rarity rules

### Milestone 5 - Portfolio polish

Status:

- Partially started

Already present:

- custom HUD pass
- Balatro-inspired layout study
- generated placeholder art
- card prefab for presentation
- screenshot references
- README and continuity spec aligned to current state

Still missing:

- architecture diagram
- changelog/release structure
- gameplay capture/GIF
- final screenshots
- final repository hygiene pass
- documented manual test pass

## Important Technical Notes

### Hand name and score preview behavior

When cards are selected:

- `CardView` emits the selected index
- `RoundScreen` calls `RunState.ToggleCardSelection`
- `RoundScreen` re-renders
- `RoundPresenter` evaluates selected cards
- `ScoreCalculator` calculates base score
- `RunModifierService` applies owned joker modifiers
- `HandNameText`, chips, and mult update from presenter-derived values

### Shop behavior

Current shop flow:

- won round shows round-end overlay
- primary action calls `RunState.EnterShop()`
- shop overlay instantiates `Offer.prefab` slots in `ShopOverlay/Panel/OfferSlots`
- clicking an offer slot changes `ShopState.SelectedOfferIndex`
- the selected offer slot shows its child `BuyJokerButton` when it can be bought
- buy calls `RunState.BuyShopOffer(index)`
- reroll calls `RunState.RerollShop()`
- clicking a joker in `UpperGlass` during shop selects it and shows `Sell`
- sell calls `RunState.SellOwnedJoker(index)`
- continue calls `RunState.LeaveShop()`

Current shop constraints:

- offers are deterministic pages based on the run's shop refresh count and the current shop's offer page index
- every shop phase loads a new deterministic offer page within the run
- reroll cost starts at `$5` and increases by `$1` after each refresh in the current shop
- bought offers cannot be bought again
- already owned jokers are marked as bought when they appear
- sold jokers are removed from `OwnedJokers`; if the sold joker is visible in the current shop, its offer becomes buyable again

### Button wiring

Current setup:

- `PlayHandButton` uses persistent scene event binding to `RoundScreen.OnPlayHandButtonClicked`
- `DiscardButton` uses persistent scene event binding to `RoundScreen.OnDiscardButtonClicked`
- sort and shop buttons are registered in `RoundScreen.Awake`

If button behavior changes later, keep this split in mind to avoid duplicate listeners.

### Layout system

Current setup:

- `HandArea` uses `HorizontalLayoutGroup`
- `PlayedHandArea` uses `HorizontalLayoutGroup`
- `UpperGlass` displays owned joker cards

This is intentionally a temporary base before a future slot-based layout system.

## Known Issues and Technical Debt

- `dotnet build StateDrivenPokerRoguelike.EditModeTests.csproj --no-restore` passes locally.
- Unity batchmode test run on 2026-05-04 failed because another Unity instance had this project open.
- Manual Unity Test Runner verification is still needed.
- Current architecture is state-driven but not full reducer/store based.
- Card layout is still an intermediate UI solution.
- The Club debuffs Clubs in scoring, but visual card-level feedback is not implemented yet.
- Random shop generation is not implemented.
- xMult/economy/extra hand/extra discard effects are not implemented.
- Worktree contained local changes when this spec was updated, including UI/prefab work, `JokerData.cs` under `Data`, TextMesh Pro fallback asset changes, and removal of the Unity AI Assistant package. Do not revert unrelated changes without explicit confirmation.

## Recommended Next Steps

Recommended order from here:

1. Run the Edit Mode suite manually in Unity Test Runner and do a short playthrough through multiple shops.
2. Add random shop generation, weights, and basic rarity/balancing rules.
3. Keep additive Chips/Mult for now; defer xMult and economy effects.
4. Polish Boss Blind v1 feedback for `The Club`.
5. Add portfolio polish: screenshots, gameplay GIF, architecture diagram, changelog, release tags.
6. Consider full action/store/reducer refactor only after the playable loop is stronger.

## Manual Test Checklist

Run in `Assets/Scenes/GameScene.unity`:

- Win Small Blind, enter shop, buy `Glass Joker`, confirm money decreases and joker appears in `UpperGlass`.
- Continue to Big Blind, select a scoring hand, confirm `Glass Joker` adds +10 Chips in preview.
- Play that hand, confirm final score also includes the +10 Chips.
- Use reroll, confirm money decreases by `$5`, the offer page changes, and the next reroll cost becomes `$6`.
- Enter a later shop in the same run and confirm a fresh offer page is loaded.
- Click an owned joker in `UpperGlass` during shop, sell it, confirm money increases and the joker disappears.
- After selling, confirm the sold joker no longer affects preview/final score.
- If the sold joker is visible in the current shop offers, confirm it becomes buyable again.
- Try to buy without enough money, confirm buy action is blocked.
- Try to buy an already bought offer, confirm no duplicate joker is added.
- Continue through The Club to next ante, confirm next shop still preserves owned jokers.

## Resume Checklist For Another Machine

When reopening this repository on another computer, read in this order:

1. `Docs/PROJECT_CONTINUITY_SPEC.md`
2. `README.md`
3. `Assets/Scripts/Core/RunState.cs`
4. `Assets/Scripts/Core/ShopState.cs`
5. `Assets/Scripts/Core/JokerCatalog.cs`
6. `Assets/Scripts/Core/RunModifierService.cs`
7. `Assets/Scripts/Core/RoundState.cs`
8. `Assets/Scripts/Presenters/RoundPresenter.cs`
9. `Assets/Scripts/MonoBehaviours/RoundScreen.cs`
10. `Assets/Scripts/Tests/EditMode`
11. `Assets/Scenes/GameScene.unity`

After that, inspect only if needed:

- `Assets/Prefabs/CardViewPrefab.prefab`
- `Assets/Art/UI/Generated`
- `Assets/ReferenceScreenShots`

## Guidance For Future Sessions

Future work should assume:

- the active gameplay loop is centered on `RunState -> RoundState`
- the active shop loop is centered on `RunState -> ShopState`
- the active UI loop is `RoundScreen -> RoundPresenter -> RoundViewModel`
- Milestone 1 and Milestone 2 are complete in code
- Milestone 3 has v1 shop transaction, structured offer UI, sell flow, and progressive reroll
- Milestone 4 has additive joker v1 functionality but needs richer effects later
- docs in `Docs/` are current only after checking this continuity spec
- the next implementation slice should be random shop generation or boss feedback polish, not store/reducer refactor
