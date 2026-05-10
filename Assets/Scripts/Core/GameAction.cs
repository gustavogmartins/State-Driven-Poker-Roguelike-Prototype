using System.Collections.Generic;

namespace Core {
    public abstract class GameAction {
    }

    public sealed class StartNewRunAction : GameAction {
        public IReadOnlyList<CardData> InitialHandCards { get; }
        public int? RunSeed { get; }

        public StartNewRunAction(IReadOnlyList<CardData> initialHandCards = null, int? runSeed = null) {
            InitialHandCards = initialHandCards;
            RunSeed = runSeed;
        }
    }

    public sealed class ContinueRoundEndAction : GameAction {
        public IReadOnlyList<CardData> InitialHandCards { get; }

        public ContinueRoundEndAction(IReadOnlyList<CardData> initialHandCards = null) {
            InitialHandCards = initialHandCards;
        }
    }

    public sealed class ToggleCardSelectionAction : GameAction {
        public int Index { get; }

        public ToggleCardSelectionAction(int index) {
            Index = index;
        }
    }

    public sealed class PlaySelectedCardsAction : GameAction {
    }

    public sealed class ScorePresentationFinishedAction : GameAction {
    }

    public sealed class DiscardSelectedCardsAction : GameAction {
    }

    public sealed class DiscardPresentationFinishedAction : GameAction {
    }

    public sealed class SortHandByRankAction : GameAction {
    }

    public sealed class SortHandBySuitAction : GameAction {
    }

    public sealed class ContinueShopAction : GameAction {
        public IReadOnlyList<CardData> InitialHandCards { get; }

        public ContinueShopAction(IReadOnlyList<CardData> initialHandCards = null) {
            InitialHandCards = initialHandCards;
        }
    }

    public sealed class SelectShopOfferAction : GameAction {
        public int Index { get; }

        public SelectShopOfferAction(int index) {
            Index = index;
        }
    }

    public sealed class BuyShopOfferAction : GameAction {
        public int Index { get; }

        public BuyShopOfferAction(int index) {
            Index = index;
        }
    }

    public sealed class RerollShopAction : GameAction {
    }

    public sealed class SelectOwnedJokerAction : GameAction {
        public int Index { get; }

        public SelectOwnedJokerAction(int index) {
            Index = index;
        }
    }

    public sealed class SellOwnedJokerAction : GameAction {
        public int Index { get; }

        public SellOwnedJokerAction(int index) {
            Index = index;
        }
    }
}
