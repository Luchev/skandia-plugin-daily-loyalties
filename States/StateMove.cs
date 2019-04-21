using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins;

namespace DailyLoyalties.States
{
    public class StateMove : State
    {
        public override StateType Type { get { return StateType.Move; } }
        private uint EntityID;
        private uint MapID;
        private Vector3 Location3D;
        public bool IgnoreIsClose = false;
        public StateMove(uint _EntityID, int _Priority = 5, bool ignoreIsClose = false)
        {
            SubType = StateSubType.MoveToEntity;
            EntityID = _EntityID;
            Priority = _Priority;
            IsInitialized = false;
            IsFinished = false;
            IgnoreIsClose = ignoreIsClose;
        }
        public StateMove(Vector3 _Location3D, uint _MapID, int _Priority = 7, bool ignoreIsClose = false)
        {
            SubType = StateSubType.MoveToLocation;
            MapID = _MapID;
            Location3D = _Location3D;
            Priority = _Priority;
            IsInitialized = false;
            IsFinished = false;
            IgnoreIsClose = ignoreIsClose;
        }
        public override void Call()
        {
            if (!IsInitialized)
                Initialize();
            else if (IsIdle() || IsClose())
                Terminate();
            else
                Mount();
        }
        private void Mount()
        {
            if (!Skandia.Me.IsMounted && !Skandia.Me.InCombat)
            {
                Skandia.Me.ToggleMount(true);
            }
        }

        private bool IsClose()
        {
            bool isClose = false;
            bool gatheringClose = false;
            if (SubType == StateSubType.MoveToLocation)
                isClose = (Skandia.Me.Location3D.Distance(Location3D) < 2);
            if (SubType == StateSubType.MoveToEntity)
                isClose = ObjectManager.ObjectList.Exists(x => x.IsValid && x.Template != null && x.Template.Id == Main.Manager.GetCurrentDailyAchievement().EntityID);
            if (Main.Manager.GetCurrentDailyAchievement().Type == DailyAchievementType.Gathering && !IgnoreIsClose)
                gatheringClose = ObjectManager.ObjectList.Exists(x => x.IsValid && x.Template != null && x.Template.Id == Main.Manager.GetCurrentDailyAchievement().EntityID);
            if (Main.Manager.GetCurrentDailyAchievement().Type == DailyAchievementType.Exploration && SubType == StateSubType.MoveToEntity)
                isClose = ObjectManager.ObjectList.Exists(x => x.IsValid && x.Template != null && x.Template.Id == EntityID && x.Distance < 3);
            return isClose || gatheringClose;
        }
        private bool IsIdle()
        {
            return Skandia.Core.Mover.IsIdle();
        }
        private void Initialize()
        {
            if (SubType == StateSubType.MoveToEntity)
            {
                Skandia.Core.Mover.MoveToSmart(EntityID);
                H.Log("[SM]Moving to " + ObjectManager.GetTemplateInfo(EntityID).Name, true);
            }
            else if (SubType == StateSubType.MoveToLocation)
            {
                Skandia.Core.Mover.MoveAt(Location3D, (short)MapID);
                H.Log("[SM]Moving to " + ObjectManager.GetMapInfo(MapID).Name + " - " + Location3D.ToString(), true);
            }
            IsInitialized = true;
            H.Log("[SM]Initialized");
        }
        private void Terminate()
        {
            if (!Skandia.Core.Mover.IsIdle())
                Skandia.Core.Mover.Stop();
            Skandia.Me.ResyncPosition();
            IsFinished = true;
            H.Log("[SM]Finished");
        }
    }
}
