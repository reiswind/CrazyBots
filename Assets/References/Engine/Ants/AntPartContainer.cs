
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntPartContainer : AntPart
    {
        public Container Container { get; private set; }
        public AntPartContainer(Ant ant, Container container) : base(ant)
        {
            Container = container;
        }
        public override string ToString()
        {
            return "AntPartContainer";
        }
        public override bool Move(ControlAnt control, Player player, List<Move> moves)
        {
            /*
            int items = Container.Unit.CountTileObjectsInContainer();
            int capacity = Container.Unit.CountCapacity();

            float urgency = 0;
            if (items > 0)
                urgency = (float)items / capacity;

            //if (items >= capacity)
            {
                AntNetworkNode.Demand(this, AntNetworkDemandType.Storage, urgency);
            }*/
            return false;
        }

    }
}
