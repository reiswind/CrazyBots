﻿using Engine.Interface;
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

        public string UnitId { get; set; }
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

        public override string ToString()
        {
            return GameCommand.ToString();
        }
    }

    internal class GameCommand
    {
        private static int staticCommandId;

        public GameCommand()
        {
            CommandId = ++staticCommandId;
            GameCommandItems = new List<GameCommandItem>();
        }
        public GameCommand(BlueprintCommand blueprintCommand)
        {
            CommandId = ++staticCommandId;
            Layout = blueprintCommand.Layout;
            BlueprintName = blueprintCommand.Name;
            GameCommandItems = new List<GameCommandItem>();
            GameCommandType = blueprintCommand.GameCommandType;
            foreach (BlueprintCommandItem blueprintCommandItem in blueprintCommand.Units)
            {
                GameCommandItem gameCommandItem = new GameCommandItem(this, blueprintCommandItem);
                GameCommandItems.Add(gameCommandItem);
            }
        }

        public int CommandId { get; set; }

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
        //public Position2 MoveToPosition { get; set; }
        public GameCommandType GameCommandType { get; set; }
        public List<GameCommandItem> GameCommandItems { get; private set; }
        public List<RecipeIngredient> RequestedItems { get; set; }
        internal Dictionary<Position2, TileWithDistance> IncludedPositions { get; set; }

        public override string ToString()
        {
            string s = GameCommandType.ToString() + " at " + TargetPosition.ToString();
            if (CommandCanceled) s += " Canceled";
            if (CommandComplete) s += " Complete";

            foreach (GameCommandItem blueprintCommandItem in GameCommandItems)
            {
                s += blueprintCommandItem.BlueprintName;
                s += " ";
                if (blueprintCommandItem.FactoryUnit.UnitId != null)
                    s += " Factory" + blueprintCommandItem.FactoryUnit.UnitId;
                s += "\r\n";
            }
            return s;
        }
    }
}
