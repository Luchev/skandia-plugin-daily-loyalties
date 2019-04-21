//using Plugins;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DailyLoyalties.States
//{
//    public class StateIdle :State
//    {
//        public override bool IsInitialized { get; set; }
//        public override StateSubType SubType { get; set; }
//        public override StateType Type { get { return StateType.Idle; } }
//        public override uint EntityID { get; set; }
//        public override bool IsDone() { return false; }
//        public StateIdle()
//        {
//            IsInitialized = false;
//            EntityID = 0;
//        }
//        public override void Call()
//        {
//            //Main.NextTimeOut = 50;

//            if (!Skandia.Core.Mover.IsIdle())
//                Skandia.Core.Mover.Stop();
//            if (!Skandia.Core.Fighter.IsIdle())
//                Skandia.Core.Fighter.Stop();
//        }
//    }
//}
