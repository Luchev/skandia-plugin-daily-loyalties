using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyLoyalties
{
    public class Target
    {
        public Target(uint guid, uint id, bool dead)
        {
            Guid = guid;
            Id = id;
            Dead = dead;
        }

        public uint Guid { get; set; }
        public uint Id { get; set; }
        public bool Dead { get; set; }
    }
}
