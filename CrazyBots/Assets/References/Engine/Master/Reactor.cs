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
                int storedPower = 0;
                if (Unit.Container != null)
                {
                    storedPower = Unit.Container.Mineral * 100;
                }

                return storedPower + Container.Mineral * 100;
            }
        }

        public int Range
        {
            get
            {
                if (Level == 1) return 12;
                if (Level == 2) return 8;
                return 12;
            }
        }

        public int Level { get; set; }
        public void BurnIfNeccessary()
        {
            if (AvailablePower == 0)
            {
                AvailablePower = 100;
                Unit.Game.Map.DistributeMineral();
            }
        }
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
                bool burnMineral = false;
                
                if (Unit.Container != null && Unit.Container.Mineral > 0)
                {
                    Unit.Container.Mineral--;
                    burnMineral = true;
                }
                else if (Unit.Assembler != null && Unit.Assembler.Container.Mineral > 0)
                {
                    Unit.Assembler.Container.Mineral--;
                    burnMineral = true;
                }
                else if (Unit.Weapon != null && Unit.Weapon.Container.Mineral > 0)
                {
                    Unit.Weapon.Container.Mineral--;
                    burnMineral = true;
                }
                else if (Container.Mineral > 0)
                {
                    Container.Mineral--;
                    burnMineral = true;
                }
                if (burnMineral)
                {
                    BurnIfNeccessary();
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
