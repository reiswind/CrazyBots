
using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntPart
    {
        public AntPart(Ant ant)
        {
            Ant = ant;
            AntNetworkNode = new AntNetworkNode();
        }

        public Ant Ant { get; private set; }

        public AntNetworkNode AntNetworkNode { get; private set; }
        public virtual void OnDestroy(Player player)
        {
            List<AntNetworkConnect> current = new List<AntNetworkConnect>();
            current.AddRange(AntNetworkNode.Connections);
            foreach (AntNetworkConnect antNetworkConnect in current)
            {
                if (antNetworkConnect.AntPartSource == this ||
                    antNetworkConnect.AntPartTarget == this)
                {
                    AntNetworkNode.Connections.Remove(antNetworkConnect);
                }
            }
            AntNetworkNode.OnDestroy(player);
        }
        public virtual bool Move(ControlAnt control, Player player, List<Move> moves)
        {
            return false;
        }
    }
}
