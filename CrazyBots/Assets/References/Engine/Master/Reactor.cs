using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Reactor : Ability
    {
        public Container Container { get; set; }
        public int AvailablePower { get; set; }
        public int StoredPower
        {
            get
            {
                return Container.Metal * 100;
            }
        }

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

            if (AvailablePower == 0)
            {
                if (Container.Metal > 0)
                {
                    Container.Metal--;
                    AvailablePower = 100;
                }
            }

            return removed;
        }

        public Reactor(Unit owner, int level) : base(owner)
        {
            AvailablePower = 100;

            Level = level;

            Container = new Container(owner, 1);
            Container.Capacity = 4;
        }
    }
}
