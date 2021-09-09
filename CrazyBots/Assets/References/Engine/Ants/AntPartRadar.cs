using Engine.Control;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntPartRadar : AntPart
    {
        public Radar Radar { get; private set; }
        public AntPartRadar(Ant ant, Radar radar) : base(ant)
        {
            Radar = radar;
        }
        public override string ToString()
        {
            return "AntPartRadar";
        }
    }
}
