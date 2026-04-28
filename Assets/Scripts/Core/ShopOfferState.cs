using System;

namespace Core {
    public sealed class ShopOfferState {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public int Cost { get; }
        public bool IsPurchased { get; }

        public ShopOfferState(
            string id,
            string title,
            string description,
            int cost,
            bool isPurchased = false) {
            if (string.IsNullOrWhiteSpace(id)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(title)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(title));
            }

            if (string.IsNullOrWhiteSpace(description)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
            }

            if (cost < 0) {
                throw new ArgumentOutOfRangeException(nameof(cost));
            }

            Id = id;
            Title = title;
            Description = description;
            Cost = cost;
            IsPurchased = isPurchased;
        }

        public bool CanBuy(int money) {
            return !IsPurchased && money >= Cost;
        }

        public ShopOfferState MarkPurchased() {
            return IsPurchased
                ? this
                : new ShopOfferState(Id, Title, Description, Cost, isPurchased: true);
        }
    }
}
