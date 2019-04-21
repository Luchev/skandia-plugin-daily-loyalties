using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins;

namespace DailyLoyalties.States
{
    public class StateFight : State
    {
        public override StateType Type { get { return StateType.Fight; } }
        private uint EntityID;
        private int MobsToKill;
        private int MobsKilled;
        private SkandiaObject lastTarget;
        private List<Target> PreviousTargets;

        public StateFight(uint _EntityID, int _MobsToKill, int _Priority = 3)
        {
            EntityID = _EntityID;
            MobsToKill = _MobsToKill;
            MobsKilled = 0;
            Priority = _Priority;
            PreviousTargets = new List<Target>();
            IsInitialized = false;
            IsFinished = false;
        }

        public override void Call()
        {
            if (!IsInitialized)
                Initialize();
            else if (QuotaMet())
                Terminate();
            else
            {
                CountKills();
                FightTarget();
            }
        }

        private void Terminate()
        {
            if (!Skandia.Core.Fighter.IsIdle())
                Skandia.Core.Fighter.Stop();
            IsFinished = true;
            H.Log("[SF]Finished");
        }

        private void CountKills()
    {
            if (Skandia.Me.GotTarget && Skandia.Me.CurrentTarget.IsValid && Skandia.Me.CurrentTarget.Template != null && Skandia.Me.CurrentTarget.Template.Id == EntityID
                && !(Skandia.Me.CurrentTarget.Template.TargetType == TargetType.Elite) && !PreviousTargets.Exists(x => x.Guid == Skandia.Me.CurrentTarget.Guid))
                    PreviousTargets.Add(new Target(Skandia.Me.CurrentTarget.Guid, Skandia.Me.CurrentTarget.Template.Id, Skandia.Me.CurrentTarget.Info.IsDead));
            if (Main.Manager.GetCurrentDailyAchievement().Type == DailyAchievementType.TopKills && lastTarget != null && !lastTarget.IsValid)
            {
                MobsKilled++;
                H.Log("[SF]" + "Targets " + MobsKilled.ToString() + "/" + MobsToKill.ToString(), true);
                PreviousTargets.Clear();
                lastTarget = null;
            }
            else if (Main.Manager.GetCurrentDailyAchievement().Type == DailyAchievementType.WipeOut)
            {
                var TargetsToRemove = new List<Target>();
                foreach (var _target in PreviousTargets)
                {
                    if (ObjectManager.ObjectList.Exists(x => x.IsValid && x.Guid == _target.Guid && x.Info.IsDead))
                    {
                        TargetsToRemove.Add(_target);
                    }
                }
                foreach (var item in TargetsToRemove)
                {
                    MobsKilled++;
                    PreviousTargets.Remove(item);
                    H.Log("[SF]" + ObjectManager.GetTemplateInfo(EntityID).Name + " " + MobsKilled.ToString() + "/" + MobsToKill.ToString(), true);
                }
            }
        }
        private bool HasValidTarget()
        {
            return (Skandia.Me.GotTarget && Skandia.Me.CurrentTarget.IsValid && Skandia.Me.CurrentTarget.Template != null
                && Skandia.Me.CurrentTarget.Template.Id == EntityID && Skandia.Me.CurrentTarget.Info.IsAlive);
        }
        private void FightTarget()
        {
            if (QuotaMet())
                return;
            var newTarget = ObjectManager.ObjectList.FirstOrDefault(x => x.IsValid && x.Template != null && x.Template.Id == EntityID && x.Info.IsAlive);
            if (HasValidTarget() && Main.Manager.GetCurrentDailyAchievement().Type == DailyAchievementType.WipeOut)
            {
                lastTarget = Skandia.Me.CurrentTarget;
                Fight();
                H.Log("[SF]Current target is valid", true);
            }
            else if (Main.Manager.GetCurrentDailyAchievement().Type == DailyAchievementType.TopKills && ((HasValidTarget() && Skandia.Me.CurrentTarget.Distance > 5) || newTarget != null && newTarget.Distance > 5))
            {
                if (HasValidTarget())
                {
                    Main.Manager.AddMoveState(new StateMove(Skandia.Me.CurrentTarget.Location3D, ObjectManager.GetCurrentMapInfo().Id, 7));
                }
                else if (newTarget != null)
                {
                    Main.Manager.AddMoveState(new StateMove(newTarget.Location3D, ObjectManager.GetCurrentMapInfo().Id, 7));
                }
                H.Log("[SF]Elite found, moving closer", true);
            }
            else if (newTarget != null)
            {
                Skandia.Me.SetTarget(newTarget.Guid);
                lastTarget = Skandia.Me.CurrentTarget;
                Fight();
                H.Log("[SF]Selected new target", true);
            }
            else if (newTarget == null)
            {
                if (Main.Manager.GetCurrentDailyAchievement().Type == DailyAchievementType.WipeOut)
                {
                    Main.Manager.AddMoveState(new StateMove(EntityID));
                }
                else if (Main.Manager.GetCurrentDailyAchievement().Type == DailyAchievementType.TopKills)
                {
                    Main.Manager.AddChannelSwitchState(6);
                    var locations = ObjectManager.GetEntityLocationsById(EntityID);
                    foreach (var location in locations)
                    {
                        Main.Manager.AddMoveState(new StateMove(location.Location, location.MapId, 7));
                    }
                }
                H.Log("[SF]Found no valid targets", true);
            }
        }
        private void Fight()
        {
            if (Skandia.Core.Fighter.IsIdle() && Skandia.Me.GotTarget && Skandia.Me.CurrentTarget.Info.IsAlive)
                Skandia.Core.Fighter.Start(Main.settings.CombatProfile);
        }

        private bool QuotaMet()
        {
            return MobsKilled >= MobsToKill;
        }

        private void Initialize()
        {
            IsInitialized = true;
            H.Log("[SF]Initialized");
        }
    }
}
