using System;
using System.Collections.Generic;
using UnityEngine;

// public class ActionStateExample : MonoBehaviour {
//     static readonly int FIRST_STATE = 0;
//     static readonly int SECOND_STATE = 1;
//     ActionState state = new ActionState();
//     void Awake() {
//         state
//             .Define(FIRST_STATE, FirstState)
//             .Define(SECOND_STATE, SecondState);
//             .Next(FIRST_STATE);
//     }
//     void OnDestroy() {
//         state.Destroy();
//     }
//     void Update() {
//         state.Update();
//     }
//     void FirstState(ActionState state) {
//         if (state.count == 1) {
//             Debug.LogFormat("FirstState");
//         }
//         if (state.time < 1.0f) { return; }
//         state.Change(SECOND_STATE);
//     }
//     void SecondState(ActionState state) {
//         if (state.count == 1) {
//             Debug.LogFormat("SecondState");
//         }
//         if (state.time < 1.0f) { return; }
//         state.Change(FIRST_STATE);
//     }
// }

namespace GGLabo.Games.Runtime
{
    public class ActionState
    {
        #region Define

        // ReSharper disable once MemberCanBePrivate.Global
        public ActionState Define(int stateId, Action<ActionState> stateAction)
        {
            AddState(new ActionStateDefinition() { stateId = stateId, stateAction = stateAction });
            return this;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public ActionState Define<T1>(int stateId, Action<ActionState, T1> stateAction)
        {
            AddState(new ActionStateDefinition<T1>() { stateId = stateId, stateAction = stateAction });
            return this;
        }

        public ActionState Define<T1, T2>(int stateId, Action<ActionState, T1, T2> stateAction)
        {
            AddState(new ActionStateDefinition<T1, T2>() { stateId = stateId, stateAction = stateAction });
            return this;
        }

        #endregion

        #region Change

        // ReSharper disable once MemberCanBePrivate.Global
        public void Change(int stateId)
        {
            ChangeSessionState(ActionStateRunner.Rent(this, GetState(stateId)));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void Change<T1>(int stateId, T1 arg1)
        {
            ChangeSessionState(ActionStateRunner<T1>.Rent(this, GetState(stateId), arg1));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void Change<T1, T2>(int stateId, T1 arg1, T2 arg2)
        {
            ChangeSessionState(ActionStateRunner<T1, T2>.Rent(this, GetState(stateId), arg1, arg2));
        }

        #endregion

        #region Next

        // ReSharper disable once MemberCanBePrivate.Global
        public void Next(int stateId)
        {
            NextSessionState(ActionStateRunner.Rent(this, GetState(stateId)));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void Next<T1>(int stateId, T1 arg1)
        {
            NextSessionState(ActionStateRunner<T1>.Rent(this, GetState(stateId), arg1));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void Next<T1, T2>(int stateId, T1 arg1, T2 arg2)
        {
            NextSessionState(ActionStateRunner<T1, T2>.Rent(this, GetState(stateId), arg1, arg2));
        }

        #endregion

        #region Stop

        public void Stop()
        {
            StopSessionState();
        }

        #endregion

        #region Update

        public void Update()
        {
            UpdateSession();
        }

        #endregion

        #region Destroy

        public void Destroy()
        {
            DestroySession();
        }

        #endregion

        #region Entry

        private const int EntryState = -1000;

        public void Entry(int stateId)
        {
            CheckSessionEntryable();
            Next(stateId);
        }

        public void Entry<T1>(int stateId, T1 arg1)
        {
            CheckSessionEntryable();
            Next(stateId, arg1);
        }

        public void Entry<T1, T2>(int stateId, T1 arg1, T2 arg2)
        {
            CheckSessionEntryable();
            Next(stateId, arg1, arg2);
        }

        public ActionState Entry(Action<ActionState> stateAction)
        {
            CheckSessionEntryable();
            Define(EntryState, stateAction);
            Change(EntryState);
            return this;
        }

        public ActionState Entry<T1>(Action<ActionState> stateAction, T1 arg1)
        {
            CheckSessionEntryable();
            Define(EntryState, stateAction);
            Change(EntryState, arg1);
            return this;
        }

        public ActionState Entry<T1, T2>(Action<ActionState> stateAction, T1 arg1, T2 arg2)
        {
            CheckSessionEntryable();
            Define(EntryState, stateAction);
            Change(EntryState, arg1, arg2);
            return this;
        }

        #endregion

        #region OnError

        private const int ErrorState = -1001;

        /// <summary>
        /// Set error action.
        /// </summary>
        /// <param name="stateAction">Error action to set.</param>
        /// <returns>This self.</returns>
        public ActionState OnError(Action<ActionState, Exception> stateAction)
        {
            Define(ErrorState, stateAction);
            return this;
        }

        #endregion

        #region OnDestroy

        /// <summary>
        /// Set destroy action.
        /// </summary>
        /// <param name="onDestroy">Destroy action to set.</param>
        /// <returns>This self.</returns>
        public ActionState OnDestroy(Action onDestroy)
        {
            SetSessionOnDestroy(onDestroy);
            return this;
        }

        #endregion

        #region State List

        private readonly List<StateDefinition> _stateList = new List<StateDefinition>();

        /// <summary>
        /// Add state.
        /// </summary>
        /// <param name="stateDefinition">StateDefinition to add.</param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void AddState(StateDefinition stateDefinition)
        {
            var foundState = FindState(stateDefinition.stateId);
            if (foundState != null)
            {
                Debug.LogErrorFormat("State definition exists already. ({0})", stateDefinition.stateId);
                return;
            }

            _stateList.Add(stateDefinition);
        }

        /// <summary>
        /// Get state.
        /// </summary>
        /// <param name="stateId">State id to get.</param>
        /// <returns>StateDefinition, or null.</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public StateDefinition GetState(int stateId)
        {
            var foundState = FindState(stateId);
            if (foundState == null)
            {
                Debug.LogErrorFormat("No state definition exists. ({0})", stateId);
                return null;
            }

            return foundState;
        }

        /// <summary>
        /// Find state.
        /// </summary>
        /// <param name="stateId">State id to find.</param>
        /// <returns>StateDefinition, or null.</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public StateDefinition FindState(int stateId)
        {
            for (int i = 0, l = _stateList.Count - 1; i < l; i++)
            {
                var foundState = _stateList[i];
                if (foundState.stateId == stateId)
                {
                    return foundState;
                }
            }

            return null;
        }

        #endregion

        #region State Session

        private readonly StateSession _stateSession = new StateSession();

        /// <summary>
        /// Current state id.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int Current => _stateSession.stateRunner?.GetState()?.stateId ?? -1;

        /// <summary>
        /// Update time.
        /// </summary>
        public float Time => _stateSession.time;

        /// <summary>
        /// Update count.
        /// </summary>
        public int Count => _stateSession.count;

        /// <summary>
        /// Run count.
        /// </summary>
        public int RunCount => _stateSession.runCount;

        /// <summary>
        /// Stop flag.
        /// </summary>
        public bool Stopped => _stateSession.stopped;

        /// <summary>
        /// Previous state id.
        /// </summary>
        public int Previous => _stateSession.previousStateId;

        private void ChangeSessionState(StateRunner stateRunner)
        {
            RestartSessionState(stateRunner);
        }

        private void NextSessionState(StateRunner stateRunner)
        {
            ResetSessionState(stateRunner);
        }

        private void StopSessionState()
        {
            SetSessionStopped(true);
        }

        private void UpdateSession()
        {
            if (_stateSession.stateRunner == null
                || _stateSession.stopped)
            {
                return;
            }

            try
            {
                _stateSession.stateRunner.Update();
            }
            catch (Exception e)
            {
                if (Current != ErrorState && FindState(ErrorState) != null)
                {
                    Next(ErrorState, e);
                    return;
                }

                StopSessionState();
                throw;
            }
        }

        private void DestroySession()
        {
            CallSessionOnDestroy();
            ClearSessionStateRunner();
        }

        private void RestartSessionState(StateRunner stateRunner)
        {
            ResetSessionState(stateRunner);
            UpdateSession();
        }

        private void ResetSessionState(StateRunner stateRunner)
        {
            var previousStateId = _stateSession.stateRunner?.GetState()?.stateId ?? -1;
            DisposeSessionStateRunner();
            _stateSession.stateRunner = stateRunner;
            _stateSession.time = 0.0f;
            _stateSession.count = 0;
            _stateSession.runCount += 1;
            _stateSession.stopped = false;
            _stateSession.previousStateId = previousStateId;
        }

        private void ClearSessionStateRunner()
        {
            DisposeSessionStateRunner();
            _stateSession.stateRunner = null;
            _stateSession.time = 0.0f;
            _stateSession.count = 0;
            _stateSession.runCount = 0;
            _stateSession.stopped = false;
            _stateSession.previousStateId = -1;
        }

        private void DisposeSessionStateRunner()
        {
            if (_stateSession.stateRunner != null)
            {
                _stateSession.stateRunner.Return();
                _stateSession.stateRunner = null;
                _stateSession.time = 0.0f;
                _stateSession.count = 0;
                _stateSession.runCount += 1;
                _stateSession.stopped = false;
                _stateSession.previousStateId = -1;
            }
        }

        private void UpdateSessionTime()
        {
            _stateSession.time = (_stateSession.count > 0)
                ? _stateSession.time + UnityEngine.Time.deltaTime
                : 0.0f;
        }

        private void UpdateSessionCount()
        {
            _stateSession.count++;
        }

        private void SetSessionStopped(bool stopped)
        {
            _stateSession.stopped = stopped;
        }

        private void SetSessionOnDestroy(Action onDestroy)
        {
            _stateSession.onDestroy = onDestroy;
        }

        private void CallSessionOnDestroy()
        {
            if (_stateSession.onDestroy != null)
            {
                var onDestroy = _stateSession.onDestroy;
                _stateSession.onDestroy = null;
                onDestroy();
            }
        }

        private void CheckSessionEntryable()
        {
            if (Current != -1)
            {
                const string error = "Session was already entryed."
                                     + " Do not call Change(), Next() or Entry()"
                                     + " before start of state.";
                throw new Exception(error);
            }
        }

        #endregion

        #region StateDefinition Types

        public abstract class StateDefinition
        {
            public int stateId = -1;
        }

        private class ActionStateDefinition : StateDefinition
        {
            public Action<ActionState> stateAction;
        }

        private class ActionStateDefinition<T1> : StateDefinition
        {
            public Action<ActionState, T1> stateAction;
        }

        private class ActionStateDefinition<T1, T2> : StateDefinition
        {
            public Action<ActionState, T1, T2> stateAction;
        }

        #endregion

        #region StateSession Types

        private class StateSession
        {
            /// <summary>
            /// StateRunner.
            /// </summary>
            public StateRunner stateRunner;

            /// <summary>
            /// Update time.
            /// </summary>
            public float time;

            /// <summary>
            /// Update count.
            /// </summary>
            public int count;

            /// <summary>
            /// Run count.
            /// </summary>
            public int runCount;

            /// <summary>
            /// Stop flag.
            /// </summary>
            public bool stopped;

            /// <summary>
            /// Previous state id.
            /// </summary>
            public int previousStateId = -1;

            /// <summary>
            /// Destroy callback.
            /// </summary>
            public Action onDestroy;
        }

        #endregion

        #region StateRunner Types

        public abstract class StateRunner
        {
            public abstract StateDefinition GetState();
            public abstract void Update();
            public abstract void Return();
        }

        public class ActionStateRunner : StateRunner
        {
            private ActionState _actionState;
            private ActionStateDefinition _stateDefinition;

            public static ActionStateRunner Rent(ActionState actionState, StateDefinition stateDefinition)
            {
                var stateRunner = StateRunnerPool<ActionStateRunner>.Rent();
                stateRunner._actionState = actionState;
                stateRunner._stateDefinition = stateDefinition as ActionStateDefinition;
                return stateRunner;
            }

            public override StateDefinition GetState()
            {
                return _stateDefinition;
            }

            public override void Update()
            {
                _actionState.UpdateSessionTime();
                _actionState.UpdateSessionCount();
                _stateDefinition.stateAction(_actionState);
            }

            public override void Return()
            {
                _actionState = null;
                _stateDefinition = null;
                StateRunnerPool<ActionStateRunner>.Return(this);
            }
        }

        public class ActionStateRunner<T1> : StateRunner
        {
            private ActionState _actionState;
            private ActionStateDefinition<T1> _stateDefinition;
            private T1 _arg1;

            public static ActionStateRunner<T1> Rent(ActionState actionState, StateDefinition stateDefinition, T1 arg1)
            {
                var stateRunner = StateRunnerPool<ActionStateRunner<T1>>.Rent();
                stateRunner._actionState = actionState;
                stateRunner._stateDefinition = stateDefinition as ActionStateDefinition<T1>;
                stateRunner._arg1 = arg1;
                return stateRunner;
            }

            public override StateDefinition GetState()
            {
                return _stateDefinition;
            }

            public override void Update()
            {
                _actionState.UpdateSessionTime();
                _actionState.UpdateSessionCount();
                _stateDefinition.stateAction(_actionState, _arg1);
            }

            public override void Return()
            {
                _actionState = null;
                _stateDefinition = null;
                _arg1 = default(T1);
                StateRunnerPool<ActionStateRunner<T1>>.Return(this);
            }
        }

        public class ActionStateRunner<T1, T2> : StateRunner
        {
            private ActionState _actionState;
            private ActionStateDefinition<T1, T2> _stateDefinition;
            private T1 _arg1;
            private T2 _arg2;

            public static ActionStateRunner<T1, T2> Rent(ActionState actionState, StateDefinition stateDefinition,
                T1 arg1, T2 arg2)
            {
                var stateRunner = StateRunnerPool<ActionStateRunner<T1, T2>>.Rent();
                stateRunner._actionState = actionState;
                stateRunner._stateDefinition = stateDefinition as ActionStateDefinition<T1, T2>;
                stateRunner._arg1 = arg1;
                stateRunner._arg2 = arg2;
                return stateRunner;
            }

            public override StateDefinition GetState()
            {
                return _stateDefinition;
            }

            public override void Update()
            {
                _actionState.UpdateSessionTime();
                _actionState.UpdateSessionCount();
                _stateDefinition.stateAction(_actionState, _arg1, _arg2);
            }

            public override void Return()
            {
                _actionState = null;
                _stateDefinition = null;
                _arg1 = default(T1);
                _arg2 = default(T2);
                StateRunnerPool<ActionStateRunner<T1, T2>>.Return(this);
            }
        }

        #endregion

        #region StateRunnerPool Types

        private static class StateRunnerPool<T>
            where T : StateRunner, new()
        {
            private static readonly Stack<T> Pool = new ();

            public static T Rent()
            {
                if (Pool.Count > 0)
                {
                    return Pool.Pop();
                }

                return (Pool.Count > 0) ? Pool.Pop() : new T();
            }

            public static void Return(T stateRunner)
            {
                Pool.Push(stateRunner);
            }
        }

        #endregion
    }
}