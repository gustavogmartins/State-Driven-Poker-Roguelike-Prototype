using System;

namespace Core {
    public sealed class ShopOfferState {
        public JokerData Joker { get; }
        public string Id => Joker.Id;
        public string Title => Joker.Name;
        public string Description => Joker.Description;
        public int Cost => Joker.Cost;
        public JokerRarity Rarity => Joker.Rarity;
        public bool IsPurchased { get; }

        public ShopOfferState(JokerData joker, bool isPurchased = false) {
            Joker = joker ?? throw new ArgumentNullException(nameof(joker));
            IsPurchased = isPurchased;
        }

        public bool CanBuy(int money) {
            return !IsPurchased && money >= Cost;
        }

        public ShopOfferState MarkPurchased() {
            return IsPurchased
                ? this
                : new ShopOfferState(Joker, isPurchased: true);
        }

        public ShopOfferState WithPurchasedState(bool isPurchased) {
            return IsPurchased == isPurchased
                ? this
                : new ShopOfferState(Joker, isPurchased);
        }
    }
}
