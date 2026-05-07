using System;
using System.Collections.Generic;
using System.Linq;

namespace Core {
    public sealed class RoundState {
        private const int DefaultHandsPerBlind = 4;
        private const int DefaultDiscardsPerBlind = 3;
        private const int MaxSelectableCards = 5;

        public BlindState Blind { get; }
        public string BlindName => Blind.Name;
        public int TargetScore { get; }
        public int CurrentScore { get; }
        public int Ante => Blind.Ante;
        public int RoundNumber => Blind.RoundNumber;
        public int HandsLeft { get; }
        public int DiscardsLeft { get; }
        public RoundPhase Phase { get; }
        public IReadOnlyList<CardData> HandCards { get; }
        public IReadOnlyList<CardData> DeckCards { get; }
        public IReadOnlyList<CardData> DiscardPileCards { get; }
        public IReadOnlyList<int> SelectedCardsIndexes { get; }
        public string LastActionText { get; }
        public string LastPlayedCardsText { get; }
        public IReadOnlyList<CardData> LastPlayedCards { get; }
        public int LastPlayedCardsCount { get; }
        public int MaxHandSize { get; }
        public PokerHandType LastPlayedHandResult { get; }
        public ScoreResult LastScoreResult { get; }
        public int BlindReward => Blind.Reward;
        public int RemainingScore => Math.Max(0, TargetScore - CurrentScore);
        public int SelectedCardsCount => SelectedCardsIndexes.Count;
        public bool IsRoundOver => Phase == RoundPhase.RoundEnd;
        public bool HasClearedBlind => CurrentScore >= TargetScore;
        public bool HasWonRound => IsRoundOver && HasClearedBlind;
        public bool HasLostRound => IsRoundOver && !HasClearedBlind && HandsLeft == 0;
        public bool CanPlaySelectedCards => !IsRoundOver && SelectedCardsCount > 0 && HandsLeft > 0;
        public bool CanDiscardSelectedCards => !IsRoundOver && SelectedCardsCount > 0 && DiscardsLeft > 0;
        public bool CanSortHand => !IsRoundOver && HandCards.Count > 1;

        public RoundState(
            BlindState blind,
            int targetScore,
            int currentScore,
            int handsLeft,
            int discardsLeft,
            RoundPhase phase,
            int maxHandSize,
            IReadOnlyList<CardData> deckCards,
            IReadOnlyList<CardData> handCards,
            IReadOnlyList<CardData> discardPileCards,
            IReadOnlyList<int> selectedCardsIndexes,
            string lastActionText,
            string lastPlayedCardsText,
            IReadOnlyList<CardData> lastPlayedCards,
            int lastPlayedCardsCount,
            PokerHandType lastPlayedHandResult,
            ScoreResult lastScoreResult) {
            if (blind == null) {
                throw new ArgumentNullException(nameof(blind));
            }

            if (targetScore < 0) {
                throw new ArgumentOutOfRangeException(nameof(targetScore));
            }

            if (currentScore < 0) {
                throw new ArgumentOutOfRangeException(nameof(currentScore));
            }

            if (handsLeft < 0) {
                throw new ArgumentOutOfRangeException(nameof(handsLeft));
            }

            if (discardsLeft < 0) {
                throw new ArgumentOutOfRangeException(nameof(discardsLeft));
            }

            if (maxHandSize <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxHandSize));
            }

            Blind = blind;
            TargetScore = targetScore;
            CurrentScore = currentScore;
            HandsLeft = handsLeft;
            DiscardsLeft = discardsLeft;
            Phase = phase;
            MaxHandSize = maxHandSize;
            DeckCards = CopyCards(deckCards);
            HandCards = CopyCards(handCards);
            DiscardPileCards = CopyCards(discardPileCards);
            SelectedCardsIndexes = SanitizeSelectedIndexes(selectedCardsIndexes, HandCards.Count);
            LastActionText = string.IsNullOrWhiteSpace(lastActionText) ? "Waiting for input" : lastActionText;
            LastPlayedCardsText = string.IsNullOrWhiteSpace(lastPlayedCardsText) ? "None" : lastPlayedCardsText;
            LastPlayedCards = CopyCards(lastPlayedCards);
            LastPlayedCardsCount = Math.Max(0, lastPlayedCardsCount);
            LastPlayedHandResult = lastPlayedHandResult;
            LastScoreResult = lastScoreResult;
        }

        public static RoundState CreateInitial(
            BlindState blind = null,
            int? targetScore = null,
            int handsLeft = DefaultHandsPerBlind,
            int discardsLeft = DefaultDiscardsPerBlind,
            int maxHandSize = 8,
            IReadOnlyList<CardData> initialHandCards = null) {
            blind ??= BlindState.CreateFirst();
            int resolvedTargetScore = targetScore ?? blind.TargetScore;
            var fullDeck = DeckBuilder.CreateStandard52();
            var shuffledDeck = DeckShuffler.Shuffle(fullDeck);

            List<CardData> handCards;
            List<CardData> deckCards;

            if (initialHandCards != null && initialHandCards.Count > 0) {
                handCards = new List<CardData>(initialHandCards.Take(maxHandSize));
                deckCards = RemoveCardsFromDeck(shuffledDeck, handCards);

                int cardsNeeded = Math.Max(0, maxHandSize - handCards.Count);
                if (cardsNeeded > 0) {
                    var drawResult = DeckUtility.DrawCards(deckCards, cardsNeeded);
                    handCards.AddRange(drawResult.DrawnCards);
                    deckCards = new List<CardData>(drawResult.RemainingDeck);
                }
            } else {
                var drawResult = DeckUtility.DrawCards(shuffledDeck, maxHandSize);
                handCards = new List<CardData>(drawResult.DrawnCards);
                deckCards = new List<CardData>(drawResult.RemainingDeck);
            }

            return new RoundState(
                blind: blind,
                targetScore: resolvedTargetScore,
                currentScore: 0,
                handsLeft: handsLeft,
                discardsLeft: discardsLeft,
                phase: RoundPhase.PlayerTurn,
                maxHandSize: maxHandSize,
                deckCards: deckCards,
                handCards: handCards,
                discardPileCards: Array.Empty<CardData>(),
                selectedCardsIndexes: Array.Empty<int>(),
                lastActionText: "Waiting for input",
                lastPlayedCardsText: "None",
                lastPlayedCards: Array.Empty<CardData>(),
                lastPlayedCardsCount: 0,
                lastPlayedHandResult: PokerHandType.None,
                lastScoreResult: ScoreResult.Zero
            );
        }

        public static RoundState CreateDebug() {
            return CreateInitial();
        }

        public bool IsSelected(int index) {
            return SelectedCardsIndexes.Contains(index);
        }

        public IReadOnlyList<CardData> GetSelectedCards() {
            var selectedCards = new List<CardData>();

            foreach (int selectedIndex in SelectedCardsIndexes) {
                if (selectedIndex >= 0 && selectedIndex < HandCards.Count) {
                    selectedCards.Add(HandCards[selectedIndex]);
                }
            }

            return selectedCards;
        }

        private static IReadOnlyList<CardData> CopyCards(IReadOnlyList<CardData> cards) {
            return new List<CardData>(cards ?? Array.Empty<CardData>()).AsReadOnly();
        }

        private static IReadOnlyList<int> SanitizeSelectedIndexes(IReadOnlyList<int> selectedCardsIndexes, int handCount) {
            if (selectedCardsIndexes == null || selectedCardsIndexes.Count == 0) {
                return Array.Empty<int>();
            }

            var sanitized = selectedCardsIndexes
                .Where(index => index >= 0 && index < handCount)
                .Distinct()
                .OrderBy(index => index)
                .Take(MaxSelectableCards)
                .ToList();

            return sanitized.AsReadOnly();
        }

        private static List<CardData> RemoveCardsFromDeck(List<CardData> deck, IReadOnlyList<CardData> cardsToRemove) {
            var remainingDeck = new List<CardData>(deck);

            foreach (var cardToRemove in cardsToRemove) {
                for (int i = 0; i < remainingDeck.Count; i++) {
                    if (remainingDeck[i].Rank == cardToRemove.Rank &&
                        remainingDeck[i].Suit == cardToRemove.Suit) {
                        remainingDeck.RemoveAt(i);
                        break;
                    }
                }
            }

            return remainingDeck;
        }
    }
}
