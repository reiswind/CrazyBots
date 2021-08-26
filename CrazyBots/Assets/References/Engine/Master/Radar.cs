using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Radar : Ability
    {
        public override string Name { get { return "Radar"; } }

        public Radar(Unit owner, int level) : base(owner, TileObjectType.PartRadar)
        {
            Level = level;
        }
    }
}
