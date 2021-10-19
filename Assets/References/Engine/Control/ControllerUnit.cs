using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Control
{
    public class ControllerUnit
    {
        public int PlayerId;
        public ulong Pos;
        public string UnitId;

        internal ControllerUnit(Unit unit)
        {
            PlayerId = unit.Owner.PlayerModel.Id;
            Pos = unit.Pos;
            UnitId = unit.UnitId;
        }

        public ControllerUnit(int playerId)
        {
            PlayerId = playerId;
        }

        public override string ToString()
        {
            return UnitId + " " + Pos.ToString() + "(" + PlayerId + ")"; ;
        }
    }
}
