using System;

namespace HaroohiePals.NitroKart.MapObj
{
    public record StateMachineState(Action InitFunc, Action StateFunc);

    public class StateMachine<TState> where TState : Enum
    {
        private readonly StateMachineState[] _states;

        public TState CurState;
        public TState NextState;
        public bool   GotoNextState;
        public int    Counter;

        public StateMachine(StateMachineState[] states)
        {
            _states  = states;
            GotoState(default);
        }

        public void GotoState(TState state)
        {
            Counter       = 0;
            GotoNextState = true;
            CurState      = state;
            NextState     = state;
        }

        public void Execute()
        {
            if (GotoNextState)
            {
                Counter       = 0;
                GotoNextState = false;
                CurState      = NextState;
                NextState     = default;
                _states[(int)(object)CurState].InitFunc?.Invoke();
            }

            _states[(int)(object)CurState].StateFunc?.Invoke();
            Counter++;
        }
    }
}