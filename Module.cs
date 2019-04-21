using Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyLoyalties
{
    public class Module
    {
        public bool IsInitialized;
        public bool IsFinished;
        public DailyAchievement Daily;
        private List<State> States;
        public State CurrentState;
        private Stopwatch ModuleTimer;
        public Module(DailyAchievement _daily)
        {
            IsInitialized = false;
            IsFinished = false;
            States = new List<State>();
            Daily = _daily;
            ModuleTimer = new Stopwatch();
        }
        public void Call()
        {
            // SUPER LOG
            //H.Log("[M]Entity ID: " + Daily.EntityID + " has " + States.Count.ToString() + " states. Current state: " + CurrentState?.Type.ToString() + ". Of which movers: " + States.Count(x => x.Type == StateType.Move).ToString(), true);
            if (!IsInitialized)
                Initialize();
            else if (CheckIfFinished())
                Terminate();
            else
            {
                SelectState();
                CurrentState.Call();
            }
        }
        private bool CheckIfFinished()
        {
            States.RemoveAll(x => x.IsFinished);
            return States.Count == 0 || ModuleTimer.ElapsedMilliseconds > 420000; // 7 minutes time out
        }
        public int HasStatesOfType(StateType type)
        {
            return States.Count(x => x.Type == type);
        }
        public void AddState(State _State)
        {
            States.Add(_State);
        }
        public void RemoveStatesByType(StateType _StateType)
        {
            States.RemoveAll(x => x.Type == _StateType);
        }
        public void StopCurrentState()
        {
            if (CurrentState != null)
                CurrentState.IsInitialized = false;
        }
        private void Initialize()
        {
            ModuleTimer.Start();
            IsInitialized = true;
            H.Log("[M]Initialized", true);
        }
        private void Terminate()
        {
            IsFinished = true;
            H.Log("[M]Finished", true);
        }
        private void SelectState()
        {
            if (CurrentState == null || CurrentState.IsFinished)
                CurrentState = States.First();
            foreach (var _state in States)
                if (CurrentState.Priority < _state.Priority)
                    CurrentState = _state;
        }
    }
}
