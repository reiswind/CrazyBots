using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Control
{
    internal class ControlNothing : IControl
    {
        public void ProcessMoves(Player player, List<Move> moves)
        {

        }


        public List<Move> Turn(Player player)
        {
            Move move = new Move();
            move.MoveType = MoveType.Skip;

            List<Move> moves = new List<Move>();
            moves.Add(move);

            return moves;

        }
    }
}
