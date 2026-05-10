using System;

namespace Core {
    public sealed class GameStore {
        public RunState State { get; private set; }

        public event Action<RunState> StateChanged;

        public GameStore(RunState initialState) {
            State = initialState ?? throw new ArgumentNullException(nameof(initialState));
        }

        public RunState Dispatch(GameAction action) {
            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            RunState nextState = RunReducer.Reduce(State, action);
            if (ReferenceEquals(nextState, State)) {
                return State;
            }

            State = nextState;
            StateChanged?.Invoke(State);
            return State;
        }
    }
}
