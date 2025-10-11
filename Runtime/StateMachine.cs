using System;
using UnityEngine;

namespace Rossoforge.StateMachine
{
    public abstract class StateMachine<T> where T : IState
    {
        public T PreviousState { get; private set; }
        public T CurrentState { get; private set; }
        public bool IsTransitionInProgress { get; private set; }

        public void StartMachine(T state)
        {
            IsTransitionInProgress = false;
            PreviousState = default;
            CurrentState = state;
            state.Enter();
        }
        public virtual async Awaitable TransitionTo(T nextState)
        {
            if (IsTransitionInProgress)
                return;

            if (nextState != null)
            {
                if (!nextState.Equals(CurrentState))
                {
                    IsTransitionInProgress = true;
                    CurrentState.Exit();
                    PreviousState = CurrentState;
                    CurrentState = nextState;
                    await Awaitable.NextFrameAsync();
                }

                CurrentState.Enter();
                IsTransitionInProgress = false;
            }
        }
        public Awaitable TransitionToPreviousState()
        {
            if (PreviousState == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("No previous state to transition to.");
#endif
                return null;
            }
            return TransitionTo(PreviousState);
        }

        public Type GetPreviousStateType()
        {
            if (PreviousState == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("No previous state defined");
#endif
                return null;
            }
            return PreviousState.GetType();
        }

        public void Update()
        {
            if (!IsTransitionInProgress)
                CurrentState?.Update();
        }
    }
}