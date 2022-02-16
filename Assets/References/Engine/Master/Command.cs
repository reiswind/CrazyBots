using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{ 
    internal class GameCommandItemUnit
    {
        public GameCommandItemUnit()
        {
            Status = "Created";
        }

        public int StuckCounter { get; set; }
        internal void SetStatus(string text, bool alert = false)
        {
            if (Status != text)
                StuckCounter = 0;
            Status = text;
            Alert = alert;
        }
        internal void ResetStatus()
        {
            StuckCounter = 0;
            Status = null;
            Alert = false;
        }

        public string UnitId { get; private set; }

        public void ResetUnitId()
        {
            UnitId = null;
        }
        public void ClearUnitId(Units units)
        {
            if (UnitId != null)
            {
                string unitId = UnitId; // Can be reset be following code
                Unit unit = units.FindUnit(unitId);
                if (unit != null && unit.CurrentGameCommand != null)
                {
                    bool reset = false;
                    if (unit.CurrentGameCommand.AttachedUnit.UnitId == unitId)
                    {
                        unit.CurrentGameCommand.AttachedUnit.UnitId = null;
                        reset = true;
                    }
                    if (unit.CurrentGameCommand.TransportUnit.UnitId == unitId)
                    {
                        unit.CurrentGameCommand.TransportUnit.UnitId = null;
                        reset = true;
                    }
                    if (unit.CurrentGameCommand.FactoryUnit.UnitId == unitId)
                    {
                        unit.CurrentGameCommand.FactoryUnit.UnitId = null;
                        reset = true;
                    }
                    if (unit.CurrentGameCommand.TargetUnit.UnitId == unitId)
                    {
                        unit.CurrentGameCommand.TargetUnit.UnitId = null;
                        reset = true;
                    }
                    if (reset)
                    {
                        unit.SetGameCommand(null);
                    }
                }
            }
            UnitId = null;
        }

        public void SetUnitId(string unitId)
        {
            if (UnitId != null && UnitId != unitId)
            {
            }
            UnitId = unitId;
        }

        public string Status { get; private set; }
        public bool Alert { get; private set; }

        public override string ToString()
        {
            if (UnitId == null) return "Nothing";
            if (!Alert)
                return UnitId + " " + Status;
            return UnitId + " " + Status + " ALERT";
        }
    }


    /*
    internal class GameCommandItem
    {
        internal GameCommandItem(GameCommand gamecommand)
        {
            AttachedUnit = new GameCommandItemUnit();
            FactoryUnit = new GameCommandItemUnit();
            TargetUnit = new GameCommandItemUnit();
            TransportUnit = new GameCommandItemUnit();

            GameCommand = gamecommand;
        }
        internal GameCommandItem(GameCommand gamecommand, BlueprintCommandItem blueprintCommandItem)
        {
            AttachedUnit = new GameCommandItemUnit();
            FactoryUnit = new GameCommandItemUnit();
            TargetUnit = new GameCommandItemUnit();
            TransportUnit = new GameCommandItemUnit();

            BlueprintName = blueprintCommandItem.BlueprintName;
            Position3 = blueprintCommandItem.Position3;
            Direction = blueprintCommandItem.Direction;
            GameCommand = gamecommand;
        }
        // Runtime info
        internal GameCommand GameCommand { get; private set; }

        internal Direction Direction { get; set; }
        internal Position3 Position3 { get; set; }

        public Position3 RotatedPosition3 { get; set; }
        public Direction RotatedDirection { get; set; }
        internal string BlueprintName { get; set; }

        internal GameCommandItemUnit AttachedUnit { get; private set; }
        internal GameCommandItemUnit TransportUnit { get; private set; }
        internal GameCommandItemUnit TargetUnit { get; private set; }
        internal GameCommandItemUnit FactoryUnit { get; private set; }

        public bool AssemblerToBuild { get; set; }
        public bool DeleteWhenDestroyed { get; set; }
        public bool FollowPheromones { get; set; }
        public bool BuildPositionReached { get; set; }
        public bool DeliverContent { get; set; } // Items have been picked up, deliver the content

        public override string ToString()
        {
            return GameCommand.ToString();
        }
    }
    */
    internal class GameCommand
    {
        private static int staticCommandId;

        public GameCommand()
        {
            AttachedUnit = new GameCommandItemUnit();
            FactoryUnit = new GameCommandItemUnit();
            TargetUnit = new GameCommandItemUnit();
            TransportUnit = new GameCommandItemUnit();

            CommandId = ++staticCommandId;
            Priority = 1;
        }
        public GameCommand(BlueprintCommandItem blueprintCommandItem)
        {
            AttachedUnit = new GameCommandItemUnit();
            FactoryUnit = new GameCommandItemUnit();
            TargetUnit = new GameCommandItemUnit();
            TransportUnit = new GameCommandItemUnit();

            CommandId = ++staticCommandId;
            Layout = blueprintCommandItem.BlueprintCommand.Layout;
            BlueprintName = blueprintCommandItem.BlueprintName;

            GameCommandType = blueprintCommandItem.BlueprintCommand.GameCommandType;
            
            Priority = 1;
        }

        public int CommandId { get; set; }
        public int ClientId { get; set; }
        public int Priority { get; set; }

        public string Layout { get; set; }
        public string BlueprintName { get; set; }

        public bool CommandComplete { get; set; }
        public bool DeleteWhenFinished { get; set; }
        public bool CommandCanceled { get; set; }
        public int PlayerId { get; set; }
        public int TargetZone { get; set; }

        public int Radius { get; set; }
        public Direction Direction { get; set; }
        public Position2 TargetPosition { get; set; }

        public GameCommand NextGameCommand { get; set; }
        
        public GameCommandType GameCommandType { get; set; }
        public FollowUpUnitCommand FollowUpUnitCommand { get; set; }
        internal Dictionary<Position2, TileWithDistance> IncludedPositions { get; set; }

        internal GameCommandItemUnit AttachedUnit { get; private set; }
        internal GameCommandItemUnit TransportUnit { get; private set; }
        internal GameCommandItemUnit TargetUnit { get; private set; }
        internal GameCommandItemUnit FactoryUnit { get; private set; }

        public bool AssemblerToBuild { get; set; }
        public bool DeleteWhenDestroyed { get; set; }
        public bool FollowPheromones { get; set; }
        public bool BuildPositionReached { get; set; }
        public bool DeliverContent { get; set; } // Items have been picked up, deliver the content

        public string UnitId { get; set; }
        public List<UnitItemOrder> UnitItemOrders { get; set; }

        public override string ToString()
        {
            string s = GameCommandType.ToString() + " at " + TargetPosition.ToString();
            if (CommandCanceled) s += " Canceled";
            if (CommandComplete) s += " Complete";

            s += BlueprintName;
            s += " ";
            if (FactoryUnit.UnitId != null)
                s += " Factory" + FactoryUnit.UnitId;
            s += "\r\n";

            return s;
        }
    }
}
