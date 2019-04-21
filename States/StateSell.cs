using Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyLoyalties.States
{
    class StateSell : State
    {
        public override StateType Type { get { return StateType.Sell; } }
        private StateSubType SubType;
        public override void Call()
        {
            
        }
        private void Initialize()
        {

        }
        private void Terminate()
        {
            
        }
    }
}
