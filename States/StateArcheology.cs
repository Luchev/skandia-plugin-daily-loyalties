using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins;

namespace DailyLoyalties.States
{
    public class StateArcheology : State
    {
        public override StateType Type { get { return StateType.Archeology; } }
        private uint EntityID;
        private int ItemsToDig;
        private int OldDugItemsCounter;
        private int ItemsDug;
        public StateArcheology(uint _EntityID, int _ItemsToDig, int _Priority = 3)
        {
            IsInitialized = false;
            IsFinished = false;
            EntityID = _EntityID;
            ItemsToDig = _ItemsToDig;
        }
        public override void Call()
        {
            if (!IsInitialized)
                Initialize();
            else if (QuotaMet())
                Terminate();
            else
            {
                Dig();
            }
        }

        private void SetDigNPC()
        {
            if (Skandia.Core.GetArchaeologyBotVendorId() != EntityID)
                Skandia.Core.SetArchaeologyBotVendorId(EntityID);
        }

        private void Dig()
        {
            if (!Skandia.Core.GetArchaeologyBotState())
                Skandia.Core.ToggleArchaeologyBot(true);
        }

        public bool QuotaMet()
        {
            if (Skandia.Me.Inventory.Exists(x => x.IsValid() && x.InternalData.Currency == MoneyType.ArcheologyTokens))
                ItemsDug = Skandia.Me.Inventory.Where(x => x.IsValid() && x.InternalData.Currency == MoneyType.ArcheologyTokens).Sum(x => x.Amount);
            else
                ItemsDug = 0;
            if (OldDugItemsCounter != ItemsDug)
            {
                OldDugItemsCounter = ItemsDug;
                H.Log("Digs " + ItemsDug.ToString() + "/" + ItemsToDig.ToString(), true);
            }
            // Check buff for hitting 100
            //46094 - Archaeologist's Honor - 100 stacks 
            if (Skandia.Me.Buffs.Any(x => x.Key == 46094 && x.Value.Stack == 100))
                return true;
            return (ItemsDug >= ItemsToDig);
        }

        private void Initialize()
        {
            if (Skandia.Me.Inventory.Exists(x => x.IsValid() && x.InternalData.Currency == MoneyType.ArcheologyTokens))
                ItemsDug = Skandia.Me.Inventory.Where(x => x.IsValid() && x.InternalData.Currency == MoneyType.ArcheologyTokens).Sum(x => x.Amount);
            else
                ItemsDug = 0;
            OldDugItemsCounter = ItemsDug;
            ItemsToDig = ItemsToDig + ItemsDug;
            if (!ObjectManager.ObjectList.Exists(x => x.IsValid && x.Template != null && x.Template.Id == EntityID))
            {
                if (Skandia.Core.GetArchaeologyBotState())
                    Skandia.Core.ToggleArchaeologyBot(false);
                Main.Manager.AddMoveState(new StateMove(EntityID, 5, true));
            }
            SetDigNPC();
            IsInitialized = true;
            H.Log("[SA]Initialized");
        }
        private void Terminate()
        {
            Main.Manager.GetCurrentModule().RemoveStatesByType(StateType.Move);
            Main.Manager.GetCurrentModule().RemoveStatesByType(StateType.SwitchChannel);
            Skandia.Core.ToggleArchaeologyBot(false);
            IsFinished = true;
            H.Log("[SA]Finished");
        }
    }
}
