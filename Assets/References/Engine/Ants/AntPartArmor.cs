
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntPartArmor : AntPart
    {
        public Armor Armor { get; private set; }
        public AntPartArmor(Ant ant, Armor armor) : base(ant)
        {
            Armor = armor;
        }
        public override string ToString()
        {
            return "AntPartArmor";
        }
    }
}
