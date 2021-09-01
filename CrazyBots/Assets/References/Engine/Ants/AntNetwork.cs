using Engine.Control;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace Engine.Ants
{
    internal class AntNetwork
    {
    }

    internal enum AntNetworkDemandType
    {
        /// <summary>
        /// Need more space to store items
        /// </summary>
        Storage,
        Minerals
    }

    internal class AntNetworkNode
    {
        public AntNetworkNode()
        {
            Connections = new List<AntNetworkConnect>();
        }

        public List<AntNetworkConnect> Connections { get; private set; }

        public void Demand(AntPart antPart, AntNetworkDemandType antNetworkDemandType, float urgency)
        {
            foreach (AntNetworkConnect antTargetNetworkConnect in Connections)
            {
                foreach (AntNetworkConnect antSourceNetworkConnect in antTargetNetworkConnect.AntPartSource.AntNetworkNode.Connections)
                {
                    if (antSourceNetworkConnect.AntPartSource != antPart)
                        continue;

                    bool found = false;
                    if (antSourceNetworkConnect.AntNetworkDemands != null)
                    {
                        foreach (AntNetworkDemand antNetworkDemand in antSourceNetworkConnect.AntNetworkDemands)
                        {
                            if (antNetworkDemand.Demand == antNetworkDemandType)
                            {
                                antNetworkDemand.Urgency = urgency;
                                found = true;
                            }
                        }
                    }
                    if (!found)
                    {
                        if (antSourceNetworkConnect.AntNetworkDemands == null)
                        {
                            antSourceNetworkConnect.AntNetworkDemands = new List<AntNetworkDemand>();
                        }
                        // Create new 
                        AntNetworkDemand antNewNetworkDemand = new AntNetworkDemand();
                        antNewNetworkDemand.Demand = antNetworkDemandType;
                        antNewNetworkDemand.Urgency = urgency;
                        antSourceNetworkConnect.AntNetworkDemands.Add(antNewNetworkDemand);
                    }
                }
            
            }

        }
    }

    internal class AntNetworkConnect
    {
        /// <summary>
        /// The source delivers the demanded items
        /// </summary>
        public AntPart AntPartSource { get; set; }
        /// <summary>
        /// The target receives the demanded items
        /// </summary>
        public AntPart AntPartTarget { get; set; }

        public List<AntNetworkDemand> AntNetworkDemands { get; set; }

        public override string ToString()
        {
            return AntPartSource.ToString() + " to " + AntPartTarget.ToString();
        }
    }

    internal class AntNetworkDemand
    {
        public float Urgency { get; set; }
        public AntNetworkDemandType Demand { get; set; }
    }
}
