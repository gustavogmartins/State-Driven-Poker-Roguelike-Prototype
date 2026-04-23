public readonly struct ScoreResult
{
    public static ScoreResult Zero => new ScoreResult(
        baseChips: 0,
        baseMult: 0,
        cardChips: 0,
        totalChips: 0,
        finalScore: 0
    );

    public int BaseChips { get; }
    public int BaseMult { get; }
    public int CardChips { get; }
    public int TotalChips { get; }
    public int FinalScore { get; }

    public ScoreResult(int baseChips, int baseMult, int cardChips, int totalChips, int finalScore)
    {
        BaseChips = baseChips;
        BaseMult = baseMult;
        CardChips = cardChips;
        TotalChips = totalChips;
        FinalScore = finalScore;
    }
}