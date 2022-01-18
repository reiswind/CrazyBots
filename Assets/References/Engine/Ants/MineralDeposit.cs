using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    class MineralDeposit
    {
        public Position2 Pos { get; set; }
        public int Minerals { get; set; }
        public float Intensitiy { get; set; }
        public int DepositId { get; set; }
    }
}
