using System;

namespace Core {
    public sealed class BlindState {
        private const int BaseSmallBlindTarget = 300;
        private const int AnteTargetStep = 200;

        public BlindType Type { get; }
        public int Ante { get; }
        public int RoundNumber => GetRoundNumber(Type);
        public string Name => GetName(Type);
        public int Reward => GetReward(Type, Ante);
        public int TargetScore => GetTargetScore(Type, Ante);

        public BlindState(BlindType type, int ante) {
            if (ante < 1) {
                throw new ArgumentOutOfRangeException(nameof(ante));
            }

            Type = type;
            Ante = ante;
        }

        public static BlindState CreateFirst() {
            return new BlindState(BlindType.Small, 1);
        }

        public BlindState Advance() {
            return Type switch {
                BlindType.Small => new BlindState(BlindType.Big, Ante),
                BlindType.Big => new BlindState(BlindType.Boss, Ante),
                BlindType.Boss => new BlindState(BlindType.Small, Ante + 1),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static int GetRoundNumber(BlindType type) {
            return type switch {
                BlindType.Small => 1,
                BlindType.Big => 2,
                BlindType.Boss => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private static string GetName(BlindType type) {
            return type switch {
                BlindType.Small => "Small Blind",
                BlindType.Big => "Big Blind",
                BlindType.Boss => "The Club",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private static int GetReward(BlindType type, int ante) {
            return type switch {
                BlindType.Small => ante * 10,
                BlindType.Big => ante * 15,
                BlindType.Boss => ante * 20,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private static int GetTargetScore(BlindType type, int ante) {
            int smallBlindTarget = BaseSmallBlindTarget + ((ante - 1) * AnteTargetStep);

            return type switch {
                BlindType.Small => smallBlindTarget,
                BlindType.Big => (smallBlindTarget * 3) / 2,
                BlindType.Boss => smallBlindTarget * 2,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
