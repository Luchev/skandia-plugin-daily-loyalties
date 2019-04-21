using Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyLoyalties
{
    public class DailyAchievement
    {
        public uint EntityID { get; set; }
        public int Count { get; set; }
        public DayOfWeek Day { get; set; }
        public int LevelRequired { get; set; }
        public DailyAchievementType Type { get; set; }
        public uint Map { get; set; }
        public Vector3 Location3D { get; set; }
    }

    public enum DailyAchievementType
    {
        WipeOut,
        TopKills,
        Gathering,
        Exploration
    }
}
