using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Reactor : Ability
    {
        public int Level { get; set; }

        public Reactor(Unit owner, int level) : base(owner)
        {
            Level = level;
        }
    }
}
