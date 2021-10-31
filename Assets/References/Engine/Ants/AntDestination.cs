using Engine.Algorithms;
using Engine.Ants;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Engine.Ants
{

    internal class AntDestination
    {
        public Tile Tile { get; set; }
        public Pheromone Pheromone { get; set; }
        public float phem_d { get; set; }
        public float Intensity { get; set; }
    }

}
