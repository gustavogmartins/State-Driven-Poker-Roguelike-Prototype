using System;

namespace Core {
    public sealed class JokerData {
        public string Id { get; }
        public string Name { get; }
        public string ShortCode { get; }
        public string Description { get; }
        public int Cost { get; }
        public JokerBonusType BonusType { get; }
        public JokerConditionType ConditionType { get; }
        public int BonusValue { get; }

        public JokerData(
            string id,
            string name,
            string shortCode,
            string description,
            int cost,
            JokerBonusType bonusType,
            JokerConditionType conditionType,
            int bonusValue) {
            if (string.IsNullOrWhiteSpace(id)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(shortCode)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(shortCode));
            }

            if (string.IsNullOrWhiteSpace(description)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
            }

            if (cost < 0) {
                throw new ArgumentOutOfRangeException(nameof(cost));
            }

            if (bonusValue < 0) {
                throw new ArgumentOutOfRangeException(nameof(bonusValue));
            }

            Id = id;
            Name = name;
            ShortCode = shortCode;
            Description = description;
            Cost = cost;
            BonusType = bonusType;
            ConditionType = conditionType;
            BonusValue = bonusValue;
        }
    }
}
