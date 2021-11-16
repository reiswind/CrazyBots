
using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Assembler : Ability
    {
        public override string Name { get { return "Assembler"; } }

        public List<string> BuildQueue { get; set; }

        public bool CanProduce()
        {
            if (Unit.Power == 0)
                return false;
            if (TileContainer != null && TileContainer.Minerals > 0)
                return true;
            if (Unit.Container != null && Unit.Container.TileContainer.Minerals> 0)
                return true;
            return false;
        }

        public Assembler(Unit owner, int level) : base(owner, TileObjectType.PartAssembler)
        {
            Level = level;
            TileContainer = new TileContainer();
            TileContainer.Capacity = 4;
            TileContainer.AcceptedTileObjectTypes = TileObjectType.Mineral;
        }

        public TileObject ConsumeMineralForUnit()
        {
            TileObject tileObject = null;
            if (Unit.Container != null)
            {
                tileObject = Unit.Container.TileContainer.RemoveTileObject(TileObjectType.Mineral);
            }
            if (tileObject == null)
            {
                if (TileContainer != null)
                {
                    tileObject = TileContainer.RemoveTileObject(TileObjectType.Mineral);
                }
            }
            return tileObject;
        }

        private Move CreateAssembleMove(Position2 pos, string productCode)
        {
            Move move;

            // possible production move
            move = new Move();
            move.MoveType = MoveType.Build;
            move.Positions = new List<Position2>();
            move.Positions.Add(Unit.Pos);
            move.Positions.Add(pos);
            move.PlayerId = Unit.Owner.PlayerModel.Id;
            move.OtherUnitId = Unit.UnitId;

            move.Stats = new MoveUpdateStats();
            move.Stats.BlueprintName = productCode;

            return move;
        }

        private Move CreateUpgradeMove(Position2 pos, Unit assemblerUnit, Unit upgradedUnit, BlueprintPart blueprintPart) 
        {
            int level = blueprintPart.Level;
            
            while (level > 1)
            {
                level--;
                if (upgradedUnit.IsInstalled(blueprintPart, level))
                {
                    level++;
                    break;
                }
            }

            Move move;

            // possible production move
            move = new Move();
            move.MoveType = MoveType.Upgrade;
            move.Positions = new List<Position2>();
            move.Positions.Add(Unit.Pos);
            move.Positions.Add(pos);
            move.PlayerId = Unit.Owner.PlayerModel.Id;
            move.UnitId = assemblerUnit.UnitId;
            move.OtherUnitId = upgradedUnit.UnitId;

            move.Stats = new MoveUpdateStats();

            MoveUpdateUnitPart part = new MoveUpdateUnitPart();
            part.PartType = blueprintPart.PartType;
            part.Level = level;
            part.Name = blueprintPart.Name;
            part.Capacity = blueprintPart.Capacity;
            part.TileObjects = new List<TileObject>();
            part.CompleteLevel = blueprintPart.Level;

            move.Stats.UnitParts = new List<MoveUpdateUnitPart>();
            move.Stats.UnitParts.Add(part);

            return move;
        }

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position2> includedPosition2s, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Assemble) == 0 && (moveFilter & MoveFilter.Upgrade) == 0)
                return;

            if (!CanProduce())
                return;
            
            Dictionary<Position2, TileWithDistance> neighbors = Unit.Game.Map.EnumerateTiles(Unit.Pos, 1, false);
            
            foreach (TileWithDistance neighbor in neighbors.Values)
            {
                if (neighbor.Unit == null)
                {
                    if (!neighbor.Tile.CanBuild())
                        continue;
                }

                if (includedPosition2s != null)
                {
                    if (!includedPosition2s.Contains(neighbor.Pos))
                        continue;
                }

                if (neighbor.Unit == null)
                {
                    if ((moveFilter & MoveFilter.Assemble) > 0)
                    {
                        if (Level > 0)
                        {
                            if (Unit.CurrentGameCommand == null)
                            {
                                //Can build everything
                                foreach (Blueprint blueprint in Unit.Owner.Game.Blueprints.Items)
                                {
                                    possibleMoves.Add(CreateAssembleMove(neighbor.Pos, blueprint.Name));
                                }
                            }
                            else
                            {
                                possibleMoves.Add(CreateAssembleMove(neighbor.Pos, Unit.CurrentGameCommand.BlueprintName));
                            }
                        }
                    }
                }
                else
                {
                    if (neighbor.Unit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                    {
                        if (Level > 0 && !neighbor.Unit.IsComplete() && !neighbor.Unit.ExtractMe)
                        {
                            if ((moveFilter & MoveFilter.Upgrade) > 0)
                            {
                                foreach (BlueprintPart blueprintPart in neighbor.Unit.Blueprint.Parts)
                                {
                                    if (!neighbor.Unit.IsInstalled(blueprintPart, blueprintPart.Level))
                                    {
                                        possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, Unit, neighbor.Unit, blueprintPart));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Enemy nearby? OMG
                    }
                }
            }
        }
    }
}
