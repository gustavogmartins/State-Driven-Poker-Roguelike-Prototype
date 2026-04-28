using System;

namespace Core {
    public sealed class ShopState {
        public int Money { get; }
        public BlindState NextBlind { get; }

        public ShopState(int money, BlindState nextBlind) {
            if (money < 0) {
                throw new ArgumentOutOfRangeException(nameof(money));
            }

            Money = money;
            NextBlind = nextBlind ?? throw new ArgumentNullException(nameof(nextBlind));
        }
    }
}
