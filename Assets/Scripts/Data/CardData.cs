public sealed class CardData {
    private static int _nextAutoInstanceId = 1;

    public int InstanceId { get; }
    public Rank Rank { get; }
    public Suit Suit { get; }

    public CardData(Rank rank, Suit suit)
        : this(GetNextAutoInstanceId(), rank, suit) {
    }

    public CardData(int instanceId, Rank rank, Suit suit) {
        if (instanceId <= 0) {
            throw new System.ArgumentOutOfRangeException(nameof(instanceId));
        }

        InstanceId = instanceId;
        Rank = rank;
        Suit = suit;

        if (instanceId >= _nextAutoInstanceId) {
            _nextAutoInstanceId = instanceId + 1;
        }
    }

    private static int GetNextAutoInstanceId() {
        return _nextAutoInstanceId++;
    }
}
