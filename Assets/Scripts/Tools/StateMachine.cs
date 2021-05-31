using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTools
{
    public class StateMachine
    {
        private int _state = -1;
        public int State { get; set; }
        public int PreviousState { get; private set; }

        bool StartCallNeeded;


        struct StateInfo
        {
            public Func<int> updateCallback;
            public Action endCallback, startCallback;
            public Func<IEnumerator> coroutineCallback;

        }

        StateInfo[] States;
        StateInfo CurrentState { get => States[_state]; }
        Coroutine activeCoroutine;
        private int maxState;

        public StateMachine(int nStates)
        {
            maxState = nStates;
            States = new StateInfo[nStates];
        }

        public void SetCallbacks(int state, Func<int> updateCallback, Func<IEnumerator> coroutineCallback, Action startCallback, Action endCallback)
        {
            States[state].updateCallback = updateCallback;
            States[state].endCallback = endCallback;
            States[state].startCallback = startCallback;
            States[state].coroutineCallback = coroutineCallback;
        }

        public void Update()
        {
            if (State != _state)
            {
                //end  callback
                if (ValidState()) CurrentState.endCallback?.Invoke();

                PreviousState = _state;

                _state = State;

                //start callback
                if (ValidState()) CurrentState.startCallback?.Invoke();

            }
            //update callback
            if (ValidState() && CurrentState.updateCallback != null)
            {
                State = CurrentState.updateCallback();
            }
        }

        public IEnumerator CoroutineHandler()
        {
            while (true)
            {
                if (!ValidState() || CurrentState.coroutineCallback == null)
                {
                    yield return null;
                }
                else
                {
                    yield return CurrentState.coroutineCallback();
                }
            }
        }

        private bool ValidState()
        {
            return _state >= 0 && _state < Mathf.Min(maxState, States.Length);
        }
    }
}