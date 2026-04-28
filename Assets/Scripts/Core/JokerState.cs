using System;

namespace Core {
    public sealed class JokerState {
        public JokerData Data { get; }
        public string Id => Data.Id;
        public string Name => Data.Name;
        public string ShortCode => Data.ShortCode;
        public string Description => Data.Description;
        public int Cost => Data.Cost;
        public JokerBonusType BonusType => Data.BonusType;
        public JokerConditionType ConditionType => Data.ConditionType;
        public int BonusValue => Data.BonusValue;

        public JokerState(JokerData data) {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}
