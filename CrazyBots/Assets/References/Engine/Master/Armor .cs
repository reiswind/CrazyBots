using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Shield : Ability
    {
        public Shield(Unit owner, int level) : base(owner)
        {
            
        }

        public override string Name { get { return "Shield"; } }
    }

    public class Armor : Ability
    {
        public override string Name { get { return "Armor"; } }

        public bool ShieldActive { get; set; }

        public int ShieldPower { get; set; }

        public Armor(Unit owner, int level) : base(owner)
        {
            Level = level;
        }

        public void ShieldHit()
        {
            ShieldPower = 0;
            ShieldActive = false;
        }

        public int LoadShield(int availablePower)
        {
            int consumed = 0;
            if (availablePower > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (ShieldPower < 10)
                    {
                        ShieldPower++;
                        consumed++;
                    }
                    if (ShieldPower >= 10)
                    {
                        ShieldActive = true;
                    }
                }
            }
            return consumed;
        }

    }
}
