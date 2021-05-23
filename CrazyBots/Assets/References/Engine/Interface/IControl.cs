using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public interface IControl
    {
        void ProcessMoves(Player player, List<Move> moves);
        List<Move> Turn(Player player);
    }
}
