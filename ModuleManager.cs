using Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyLoyalties
{
    public class ModuleManager
    {
        private bool IsRunning;
        private int NumberOfModules;
        private List<Module> Modules;
        private Stopwatch Timer;
        private int TimeOut;
        private int ModulesFinished = 0;
        private Vector3 LastLocation = new Vector3(0, 0, 0);
        private Stopwatch StuckTimer;
        private void AddModule(Module _module)
        {
            Modules.Add(_module);
            H.Log("[MM]Added module " + _module.Daily.Type.ToString(), true);
        }
        public void Call()
        {
            if (!IsRunning || Modules.Count == 0)
                return;
            if (CheckIfTimeOutRequired())
                return;
            ReviveIfDead();
            CheckIfModuleFinished();
            CheckIfFinished();
            CheckIfStuck();
            if (IsRunning)
                Modules[0].Call();
        }

        private void CheckIfStuck()
        {
            if (!StuckTimer.IsRunning)
                StuckTimer.Start();
            if (StuckTimer.ElapsedMilliseconds < 35000)
                return;
            if (Skandia.Me.Location3D.Distance(LastLocation) < 2)
                PlayerIsStuck();
            LastLocation = Skandia.Me.Location3D;
        }

        private void PlayerIsStuck()
        {
            if (Modules.Count < 0)
            {
                H.Log("[ERROR]Player is stuck but no modules are present. Report this issue to the developer");
                return;
            }
            else
            {
                if (Modules[0].HasStatesOfType(StateType.Stuck) > 0)
                {
                    Modules[0].AddState(new States.StateStuck());
                    H.Log("[MM]Injecting Unstuck state", false);
                }
            }

        }

        private bool CheckIfTimeOutRequired()
        {
            if (TimeOut == 0)
                return false;
            if (!Timer.IsRunning)
                Timer.Start();
            if (Timer.ElapsedMilliseconds >= TimeOut)
            {
                TimeOut = 0;
                Timer.Reset();
                return false;
            }
            H.Log("[MM]Waiting for TimeOut", true);
            return true;
        }
        public void RequestTimeOut(int millieconds)
        {
            H.Log("[MM]TimeOut " + (millieconds / 1000).ToString() + " requested", true);
            TimeOut = millieconds;
        }

        private void CheckIfModuleFinished()
        {
            if (Modules[0].IsFinished)
            {
                Modules.RemoveAt(0);
                ModulesFinished++;
                H.Log("[MM]Module " + ModulesFinished.ToString() + " finished");
                H.Log("[MM]Remaining modules: " + Modules.Count.ToString());
            }
        }
        private void CheckIfFinished()
        {
            if (Modules.Count == 0)
            {
                Stop();
                H.Log("[MM]Finished");
            }
        }
        private void ReviveIfDead()
        {
            if (Skandia.Me.Info.IsDead)
            {
                Skandia.Me.Ressurect(RezType.InTown);
                H.Log("[MM]Character resurrected", true);
            }
        }
        public ModuleManager()
        {
            Modules = new List<Module>();
            IsRunning = false;
            Timer = new Stopwatch();
            StuckTimer = new Stopwatch();
            H.Log("[MM]Initialized");
        }
        public void Toggle()
        {
            if (!Skandia.IsInGame)
            {
                H.Log("[MM]Skandia is not in game, cannot start/stop", true);
                return;
            }
            if (IsRunning)
            {
                Stop();
            }
            else
            {
                Start();
            }
        }
        private void Start()
        {
            if (IsRunning)
            {
                H.Log("[MM]Already running", true);
                return;
            }
            if (Modules.Count == 0 && !Main.PluginStartedOnce)
            {
                LoadModules();
            }
            IsRunning = true;
            Main.mainUI.SetStartStopButton("Stop", Color.Red);
            Main.mainUI.loadingCircle.Active = true;
            Main.mainUI.loadingCircle.Color = Color.Green;
            H.Log("[MM]Started");
        }
        private void Stop()
        {
            if (!IsRunning)
            {
                H.Log("[MM]Already stopped", true);
                return;
            }
            if (Modules.Count > 0)
                Modules[0].StopCurrentState();
            Skandia.Core.Mover.Stop();
            Skandia.Core.ToggleSelfDefenseBot(false);
            Skandia.Core.Fighter.Stop();
            Skandia.Core.ToggleArchaeologyBot(false);
            IsRunning = false;
            Main.mainUI.SetStartStopButton("Start", Color.Green);
            Main.mainUI.loadingCircle.Active = false;
            Main.mainUI.loadingCircle.Color = Color.Red;
            if (Modules.Count == 1 && Modules[0].CurrentState.Type == StateType.Done)
                H.Log("[MM]Remaining modules: 0");
            H.Log("[MM]Stopped");
        }
        public void Reset()
        {
            IsRunning = false;
            NumberOfModules = 0;
            Modules = new List<Module>();
            Main.PluginFinished = false;
            Main.PluginStartedOnce = false;
            H.Log("[MM]Reset");
        }
        public void AddChannelSwitchState(int priority = 10)
        {
            if (Modules.Count == 0)
                return;
            Modules[0].RemoveStatesByType(StateType.SwitchChannel);
            Modules[0].AddState(new States.StateSwitchChannel(priority));
            H.Log("[MM]Added ChannelSwitch State", true);
        }
        public void AddMoveState(States.StateMove mover, bool removeOldMovers = false)
        {
            if (Modules.Count == 0)
                return;
            if (removeOldMovers)
                Modules[0].RemoveStatesByType(StateType.Move);
            Modules[0].AddState(mover);
            H.Log("[MM]Added Move State", true);
        }
        public void SkipModule()
        {
            if (Modules.Count == 0)
                return;
            Modules[0].IsFinished = true;
            if (!Skandia.Core.Mover.IsIdle())
                Skandia.Core.Mover.Stop();
            if (!Skandia.Core.Fighter.IsIdle())
                Skandia.Core.Fighter.Stop();
            H.Log("[MM]Skipped module", true);
        }
        public DailyAchievement GetCurrentDailyAchievement()
        {
            if (Modules.Count == 0)
                return null;
            else
                return Modules[0].Daily;
        }
        public Module GetCurrentModule()
        {
            if (Modules.Count == 0)
                return null;
            else
                return Modules[0];
        }
        private void LoadModules()
        {
            Task.Run(() =>
            {
                // To avoid repeats
                if (Modules.Count != 0)
                {
                    H.Log("[MM]Modules are already loaded");
                    return;
                }
                //// Needs update for when different daily types are added
                ////
                //// Priority list
                //// 3 = fight
                //// 5 = Move to the entity
                //// 7 = Move to map (before moving to entity for WipeOut)
                //// 8 = Move to a location (Used to move close to elites)
                //// 9 = Channel Switch
                int tasks = 0;
                List<DailyAchievement> _dailyAchievs = H.DeserializeFromFile<List<DailyAchievement>>(H.DataFile);
                var day = ObjectManager.CurrentServerTime.DayOfWeek;
                foreach (DailyAchievement _daily in _dailyAchievs)
                {
                    if ((ObjectManager.CurrentServerTime.TimeOfDay.TotalSeconds < TimeSpan.FromHours(6).TotalSeconds) && _daily.Type != DailyAchievementType.Exploration)
                    {
                        day = DateTime.Today.AddDays(-1).DayOfWeek;
                    }
                    else
                    {
                        day = ObjectManager.CurrentServerTime.DayOfWeek;
                    }
                    // Daily Wipe Out
                    if (Main.settings.WipeOut && _daily.Day == day && _daily.LevelRequired <= Skandia.Me.Info.Level && _daily.Type == DailyAchievementType.WipeOut)
                    {
                        tasks++;
                        var _module = new Module(_daily);
                        _module.AddState(new States.StateFight(_daily.EntityID, _daily.Count));
                        AddModule(_module);
                    }
                    // Daily Top Kills
                    if (Main.settings.TopKills && _daily.Day == day && _daily.LevelRequired <= Skandia.Me.Info.Level && _daily.Type == DailyAchievementType.TopKills)
                    {
                        tasks++;
                        var _module = new Module(_daily);
                        _module.AddState(new States.StateFight(_daily.EntityID, _daily.Count));
                        AddModule(_module);
                    }
                    // Daily Gathering
                    if (Main.settings.Gathering && _daily.Day == day && _daily.LevelRequired <= Skandia.Me.Info.Level && _daily.Type == DailyAchievementType.Gathering)
                    {
                        tasks++;
                        var _module = new Module(_daily);
                        _module.AddState(new States.StateGather(_daily.EntityID, _daily.Count));
                        AddModule(_module);
                    }
                    // Daily Exploration
                    if (Main.settings.Exploration && _daily.Day == day && _daily.LevelRequired <= Skandia.Me.Info.Level && _daily.Type == DailyAchievementType.Exploration)
                    {
                        tasks++;
                        var _module = new Module(_daily);
                        _module.AddState(new States.StateArcheology(_daily.EntityID, _daily.Count));
                        AddModule(_module);
                    }
                }
                var ModDone = new Module(new DailyAchievement());
                ModDone.AddState(new States.StateDone());
                AddModule(ModDone);
                tasks++;
                if (!Main.PluginStartedOnce)
                    Main.PluginStartedOnce = true;
                NumberOfModules = Modules.Count;
                H.Log("[MM]Loaded " + tasks.ToString() + " modules");
            });
        }
    }
}
