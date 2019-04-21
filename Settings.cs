using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyLoyalties
{
    public class Settings
    {
        public string CombatProfile { get; set; }
        public bool DetailedLogs { get; set; }
        public bool AutoStart { get; set; }
        public bool WipeOut { get; set; }
        public bool TopKills { get; set; }
        public bool Gathering { get; set; }
        public bool Exploration { get; set; }
        public bool IgnorePVPChannel { get; set; }
        // When updating - implemente new methods in MainUI with Invoker, update the H.LoadProfile()
        public Settings()
        {
            CombatProfile = "Backup_Template";
            DetailedLogs = false;
            AutoStart = false;
            WipeOut = true;
            TopKills = true;
            Gathering = true;
            Exploration = true;
            IgnorePVPChannel = false;
        }
    }
}
