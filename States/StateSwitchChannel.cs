using Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyLoyalties.States
{
    public class StateSwitchChannel : State
    {
        public override StateType Type { get { return StateType.SwitchChannel; } }
        private short NextChannelId;
        private uint NextChannelNumber;
        public StateSwitchChannel(int _Priority = 9)
        {
            IsInitialized = false;
            IsFinished = false;
            Priority = _Priority;

        }
        public override void Call()
        {
            if (Skandia.Me.GetChannelId(1) == -1)
                return;
            if (CheckIfEntitiesAround())
                return;
            if (!IsInitialized)
                Initialize();
            else if (IsChannelChanged())
                Terminate();
            else
                ChangeChannel();
        }
        private void ChangeChannel()
        {
            if (IsFinished || !IsInitialized)
                return;
            else if (!Skandia.Me.IsSwitchingChannel)
            {
                Skandia.Me.ChangeChannel(NextChannelNumber);
            }
        }
        private bool IsChannelChanged()
        {
            return NextChannelId == Skandia.Me.GetCurrentChannelId;
        }
        private void Initialize()
        {
            uint nextChannelIndex = 0;
            var currentChannel = Skandia.Me.GetCurrentChannelId;
            var currentChannelNoMatch = true;
            uint maxChannels = 0;
            var random = new Random();
            for (uint i = 1; i < 20; i++)
            {
                var channel = Skandia.Me.GetChannelId(i);
                if (channel == currentChannel)
                {
                    nextChannelIndex = i + 1;
                    currentChannelNoMatch = false;
                }
                if (channel == 0)
                {
                    maxChannels = currentChannelNoMatch ? i - 2 : i - 1;
                    if (currentChannelNoMatch)
                    {
                        nextChannelIndex = (uint)random.Next(1, (int)maxChannels);
                    }
                    if (nextChannelIndex > maxChannels)
                    {
                        nextChannelIndex = 1;
                    }
                    break;
                }
            }
            if (Main.settings.IgnorePVPChannel && nextChannelIndex == maxChannels)
            {
                nextChannelIndex = 1;
            }
            if (nextChannelIndex == 0)
            {
                H.Log("Failed to get next channel", true);
                return;
            }
            else
            {
                NextChannelNumber = nextChannelIndex;
                NextChannelId = Skandia.Me.GetChannelId(nextChannelIndex);
                if (!Skandia.Core.GetSelfDefenseState())
                    Skandia.Core.ToggleSelfDefenseBot(true);
                IsInitialized = true;
                H.Log("[SSC]Initialized, next channel number " + NextChannelNumber.ToString());
            }
        }
        private bool CheckIfEntitiesAround()
        {
            if (ObjectManager.ObjectList.Any(x => x.IsValid && x.Template != null && x.Template.Id == Main.Manager.GetCurrentDailyAchievement().EntityID && x.Info.IsAlive))
            {
                Terminate();
                return true;
            }
            return false;
        }
        private void Terminate()
        {
            if (Skandia.Core.GetSelfDefenseState())
                Skandia.Core.ToggleSelfDefenseBot(false);
            IsFinished = true;
            if (NextChannelId == Skandia.Me.GetCurrentChannelId)
                Main.Manager.RequestTimeOut(10000);
            if (Main.Manager.GetCurrentDailyAchievement().Type == DailyAchievementType.TopKills)
            {
                foreach (var location in ObjectManager.GetEntityLocationsById(Main.Manager.GetCurrentDailyAchievement().EntityID))
                {
                    Main.Manager.AddMoveState(new StateMove(location.Location, location.MapId, 7));
                }
            }
            H.Log("[SSC]Finished");
        }
    }
}
