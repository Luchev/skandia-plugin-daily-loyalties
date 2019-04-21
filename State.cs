using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyLoyalties
{
    public abstract class State
    {
        public abstract StateType Type { get; }
        public StateSubType SubType;
        public int Priority;
        public bool IsInitialized;
        public bool IsFinished;
        public abstract void Call();
    }
    public enum StateType
    {
        Idle,
        Fight,
        Move,
        Done,
        Gather,
        SwitchChannel,
        Archeology,
        Sell,
        Stuck
    }
    public enum StateSubType
    {
        MoveToEntity,
        MoveToLocation,
        WipeOut,
        TopKills,
        Gathering,
        Exploration
    }
}
