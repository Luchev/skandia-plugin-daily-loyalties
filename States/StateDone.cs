using Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyLoyalties.States
{
    public class StateDone : State
    {
        public override StateType Type { get { return StateType.Done; } }

        public StateDone()
        {
            IsInitialized = false;
            IsFinished = false;
        }
        public override void Call()
        {
            if (!Skandia.Core.Mover.IsIdle())
                Skandia.Core.Mover.Stop();
            if (!Skandia.Core.Fighter.IsIdle())
                Skandia.Core.Fighter.Stop();
            if (!Skandia.Me.CanSendSkill(62047))
            {
                IsFinished = true;
                Main.Manager.Toggle();
                Main.Manager.Reset();
            }
            else
            {
                Skandia.Me.SendSkill(62047);
            }
        }
    }
}
