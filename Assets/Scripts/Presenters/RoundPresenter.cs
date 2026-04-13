using Core;
using View;

namespace Presenters {
    public sealed class RoundPresenter {
        public RoundViewModel Present(RoundState roundState) {
            return new RoundViewModel {
                BlindText = $"Blind: {roundState.BlindName}",
                TargetScoreText = $"Target: {roundState.TargetScore}",
                CurrentScoreText = $"Score: {roundState.CurrentScore}",
                DiscardsLeftText = $"Discards: {roundState.DiscardsLeft}",
                HandsLeftText = $"Hands: {roundState.HandsLeft}",
                PhaseText = $"Phase: {roundState.Phase}"
            };
        }
    }
}
