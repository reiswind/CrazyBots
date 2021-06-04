using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Container : Ability
    {
        public int Level { get; set; }
        public int Metal { get; set; }

        private int extraCapacity;
        public int Capacity
        {
            get
            {
                //return 30;

                if (extraCapacity != 0) return extraCapacity;
                if (Level == 1) return 20;
                if (Level == 2) return 60;
                if (Level == 3) return 220;
                return Level * 20;
            }
            set
            {
                extraCapacity = value;
            }
        }

        public Container(Unit owner, int level) : base(owner)
        {
            Level = level;
        }


    }
}

