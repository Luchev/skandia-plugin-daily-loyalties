using Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyLoyalties.States
{
    public class StateGather : State
    {
        public override StateType Type { get { return StateType.Gather; } }
        private uint EntityID;
        private int ItemsToGather;
        private int ItemsGathered;
        private SkandiaObject PreviousTarget;
        private List<Vector3> LocationsToGather;
        private readonly Stopwatch PickAxeTimer = new Stopwatch();
        public StateGather(uint entityID, int itemsToGather)
        {
            IsInitialized = false;
            EntityID = entityID;
            ItemsToGather = itemsToGather;
            ItemsGathered = 0;
            LocationsToGather = new List<Vector3>();
        }
        public override void Call()
        {
            if (!IsInitialized)
                Initialize();
            else if (QuotaMet())
                Terminate();
            else
            {
                FindTargets();
                CountGatheredItems();
                Gather();
            }
        }

        private void FindTargets()
        {
            var targets = ObjectManager.GatherableList.Where(x => x.IsValid && x.Template != null && x.Template.Id == EntityID);
            if (targets != null)
            {
                foreach (var item in targets)
                {
                    if (!LocationsToGather.Contains(item.Location3D))
                        LocationsToGather.Add(item.Location3D);
                }
            }
            if (LocationsToGather.Count == 0)
            {
                foreach (var location in ObjectManager.GetEntityLocationsById(Main.Manager.GetCurrentDailyAchievement().EntityID))
                {
                    Main.Manager.AddMoveState(new StateMove(location.Location, location.MapId, 7));
                }
                Main.Manager.AddChannelSwitchState(5);
            }
        }

        private void CountGatheredItems()
        {
            foreach (var loc in LocationsToGather.ToList())
            {
                if (Skandia.Me.Location3D.Distance(loc) < 2)
                    LocationsToGather.Remove(loc);
            }
            if (PreviousTarget != null && !PreviousTarget.IsValid)
            {
                PreviousTarget = null;
                ItemsGathered++;
                H.Log("[SG]" + ObjectManager.GetTemplateInfo(EntityID).Name + " " + ItemsGathered.ToString() + "/" + ItemsToGather.ToString(), true);
            }
        }

        private void Gather()
        {
            var target = ObjectManager.GatherableList.FirstOrDefault(x => x.IsValid && x.Template != null && x.Template.Id == EntityID && x.Distance < 3);
            if (target == null)
            {
                if (LocationsToGather.Count == 0)
                    return;
                var targetLocation = LocationsToGather.First();
                if (Skandia.Me.Location3D.Distance(targetLocation) > 2)
                {
                    Main.Manager.AddMoveState(new StateMove(targetLocation, (uint)Skandia.Me.Map, 8, true));
                    return;
                }
            }
            Skandia.Me.SetTarget(target.Guid);
            PreviousTarget = Skandia.Me.CurrentTarget;
            if (target.TypeId == 0x8 && target.Info.Type == EntityType.Npc)
            {
                if ((Skandia.Me.GotTarget && Skandia.Me.CurrentTarget.Template != null && Skandia.Me.CurrentTarget.Template.Id != EntityID) || !Skandia.Me.GotTarget)
                    Skandia.Me.SetTarget(target.Guid);
                foreach (var PickAxeItem in Skandia.Me.Inventory.Where(x => x.IsValid() && x.Id == 11670))
                {
                    if (!PickAxeTimer.IsRunning)
                    {
                        PickAxeTimer.Start();
                    }
                    if (PickAxeTimer.ElapsedMilliseconds < 2250)
                    {
                        return;
                    }
                    PickAxeItem.Use();
                    PickAxeTimer.Restart();
                }
            }
            else
                target.Collect();
        }

        private void Initialize()
        {
            IsInitialized = true;
            H.Log("[SG]Initialized");
        }
        private void Terminate()
        {
            Main.Manager.GetCurrentModule().RemoveStatesByType(StateType.Move);
            Main.Manager.GetCurrentModule().RemoveStatesByType(StateType.SwitchChannel);
            IsFinished = true;
            H.Log("[SG]Finished");
        }
        public bool QuotaMet()
        {
            return (ItemsGathered >= ItemsToGather);
        }
    }
}
