using Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyLoyalties.States
{
    public class StateStuck : State
    {
        public override StateType Type { get { return StateType.Stuck; } }
        private bool UseTeleport = false;

        public StateStuck(int priority = 100)
        {
            Priority = priority;
            IsInitialized = false;
            IsFinished = false;
        }
        public override void Call()
        {
            H.Log("[Stuck]Called", true);
            if (!IsInitialized)
                Initialize();
            else if (UseTeleport)
            {
                if (Skandia.Me.CanSendSkill(62047))
                {
                    Skandia.Me.SendSkill(62047);
                    H.Log("[Stuck]Trying to teleport");
                }
                else
                    Terminate();
            }
            else
            {
                Move();
                if (ObjectManager.ObjectList.Exists(x => x.Template.Id == 10045 && x.Distance < 5))
                    Terminate();
            }
            
        }
        private void Move()
        {
            if (Skandia.Core.Mover.IsIdle())
            {
                Skandia.Core.Mover.MoveTo(10045);
                H.Log("[Stuck]Trying to move to portal");
            }
            else
            {
                H.Log("[Stuck]Teleporting to portal");
                Skandia.Me.SetLocation(ObjectManager.GetEntityLocationsById(10045).First(x => x.MapId == ObjectManager.GetCurrentMapInfo().Id).Location);
            }
        }

        private void Initialize()
        {
            Skandia.Core.Mover.Stop();
            Skandia.Core.Fighter.Stop();
            Skandia.Core.ToggleSelfDefenseBot(true);
            if (Skandia.Me.CanSendSkill(62047))
                UseTeleport = true;
            else
            {
                UseTeleport = false;
            }
            IsInitialized = true;
            H.Log("[Stuck]Initialized");
        }
        private void Terminate()
        {
            H.Log("[Stuck]Finished");
            Skandia.Core.ToggleSelfDefenseBot(false);
            IsFinished = true;
        }
    }
}
