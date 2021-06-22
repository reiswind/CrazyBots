using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Reactor : Ability
    {
        public int AvailablePower { get; set; }

        public int Level { get; set; }

        public int ConsumePower(int remove)
        {
            int removed;
            if (remove > AvailablePower)
            {
                removed = AvailablePower;
                AvailablePower = 0;                
            }    
            else
            {
                AvailablePower -= remove;
                removed = remove;
            }
            return removed;
        }

        public Reactor(Unit owner, int level) : base(owner)
        {
            AvailablePower = 1000;

            Level = level;
        }
    }
}
