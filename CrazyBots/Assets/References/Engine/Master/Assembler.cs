using Engine.Control;
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
        public int Level { get; set; }

        public Container Container { get; set; }

        public List<string> BuildQueue { get; set; }

        public void Build (string blueprintName)
        {
            if (BuildQueue == null)
                BuildQueue = new List<string>();
            BuildQueue.Add(blueprintName);
        }


        public bool CanProduce()
        {
            if (Unit.Power == 0)
                return false;
            if (Container != null && Container.Metal > 0)
                return true;
            if (Unit.Container != null && Unit.Container.Metal > 0)
                return true;
            return false;
        }

        public Assembler(Unit owner, int level) : base(owner)
        {
            Level = level;
            Container = new Container(owner, 1);
            Container.Capacity = 4;
            //Container.Metal = 4;
        }

        public void ConsumeMetalForUnit(Unit unit)
        {
            if (Unit.Container != null && Unit.Container.Metal > 0)
            {
                Unit.Container.Metal--;
            }
            else
            {
                if (Container != null && Container.Metal > 0)
                {
                    Container.Metal--;
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        private Move CreateAssembleMove(Position pos, string productCode)
        {
            Move move;

            // possible production move
            move = new Move();
            move.MoveType = MoveType.Build;
            move.Positions = new List<Position>();
            move.Positions.Add(Unit.Pos);
            move.Positions.Add(pos);
            move.PlayerId = Unit.Owner.PlayerModel.Id;
            move.UnitId = productCode;
            move.OtherUnitId = Unit.UnitId;

            return move;
        }

        private Move CreateUpgradeMove(Position pos, string productCode)
        {
            Move move;

            // possible production move
            move = new Move();
            move.MoveType = MoveType.Upgrade;
            move.Positions = new List<Position>();
            move.Positions.Add(Unit.Pos);
            move.Positions.Add(pos);
            move.PlayerId = Unit.Owner.PlayerModel.Id;
            move.UnitId = productCode;
            move.OtherUnitId = Unit.UnitId;

            return move;
        }

        public bool HandleRequestUnit(PlayerUnit playerUnit, DispatcherRequestUnit dispatcherRequestUnit, Move move)
        {
            Tile tile = playerUnit.Unit.Owner.Game.Map.GetTile(move.Positions[1]);

            if (DoesMoveMinRequest(move, dispatcherRequestUnit.UnitType, tile.Unit))
            {
                playerUnit.PossibleMoves.Add(new PlayerMove(move));

                // Take first option or random but dicide here
                return true;
            }
            return false;
        }

        public static bool DoesMoveMinRequest(Move move, UnitType unitType, Unit unit)
        {
            // Minimal requests ok?
            if (move.UnitId == "Engine" && unitType.MinEngineLevel > 0)
            {
                if (unit == null || unit.Engine == null || unit.Engine.Level < unitType.MinEngineLevel)
                {
                    return true;
                }
            }
            if (move.UnitId == "Armor" && unitType.MinArmorLevel > 0)
            {
                if (unit == null || unit.Armor == null || unit.Armor.Level < unitType.MinArmorLevel)
                {
                    return true;
                }
            }
            if (move.UnitId == "Weapon" && unitType.MinWeaponLevel > 0)
            {
                if (unit == null || unit.Weapon == null || unit.Weapon.Level < unitType.MinWeaponLevel)
                {
                    return true;
                }
            }

            if (move.UnitId == "Assembler" && unitType.MinAssemblerLevel > 0)
            {
                if (unit == null || unit.Assembler == null || unit.Assembler.Level < unitType.MinAssemblerLevel)
                {
                    return true;
                }
            }

            if (move.UnitId == "Extractor" && unitType.MinExtractorLevel > 0)
            {
                if (unit == null || unit.Extractor == null || unit.Extractor.Level < unitType.MinExtractorLevel)
                {
                    return true;
                }
            }
            if (move.UnitId == "Container" && unitType.MinContainerLevel > 0)
            {
                if (unit == null || unit.Container == null || unit.Container.Level < unitType.MinContainerLevel)
                {
                    return true;
                }
            }
            if (move.UnitId == "Reactor" && unitType.MinReactorLevel > 0)
            {
                if (unit == null || unit.Reactor == null || unit.Reactor.Level < unitType.MinReactorLevel)
                {
                    return true;
                }
            }
            if (move.UnitId == "Radar" && unitType.MinRadarLevel > 0)
            {
                if (unit == null || unit.Radar == null || unit.Radar.Level < unitType.MinRadarLevel)
                {
                    return true;
                }
            }

            if (unit != null)
            {
                // Optional requests?
                if (move.UnitId == "Engine" && unit.Engine != null && unit.Engine.Level < unitType.MaxEngineLevel)
                {
                    return true;
                }
                if (move.UnitId == "Armor" && unit.Armor != null && unit.Armor.Level < unitType.MaxArmorLevel)
                {
                    return true;
                }
                if (move.UnitId == "Weapon" && unit.Weapon != null && unit.Weapon.Level < unitType.MaxWeaponLevel)
                {
                    return true;
                }
                if (move.UnitId == "Extractor" && unit.Extractor != null && unit.Extractor.Level < unitType.MaxExtractorLevel)
                {
                    return true;
                }
                if (move.UnitId == "Container" && unit.Container != null && unit.Container.Level < unitType.MaxContainerLevel)
                {
                    return true;
                }
                if (move.UnitId == "Reactor" && unit.Reactor != null && unit.Reactor.Level < unitType.MaxReactorLevel)
                {
                    return true;
                }
                if (move.UnitId == "Radar" && unit.Radar != null && unit.Radar.Level < unitType.MaxRadarLevel)
                {
                    return true;
                }
            }
            return false;
        }


        public Dictionary<Position, TileWithDistance> CollectOutputPositions()
        {
            Dictionary<Position, TileWithDistance> positions = Unit.Game.Map.EnumerateTiles(Unit.Pos, 1, true);

            return positions;
        }

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Assemble) == 0 && (moveFilter & MoveFilter.Upgrade) == 0)
                return;

            if (!CanProduce())
                return;

            Dictionary<Position, TileWithDistance> neighbors = CollectOutputPositions();

            foreach (TileWithDistance neighbor in neighbors.Values)
            {
                if (Unit.Owner.UnitsInBuild.ContainsKey(neighbor.Tile.Pos))
                    /*
                    neighbor.Tile.UnitInBuild != null &&
                    neighbor.Tile.UnitInBuild.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)*/
                {
                    PlayerUnit playerUnit = Unit.Owner.UnitsInBuild[neighbor.Tile.Pos];
                    if (playerUnit.Unit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                    {
                        if ((moveFilter & MoveFilter.Assemble) > 0)
                        {
                            // Already one Unit in progress, do not build another
                            return;
                        }
                        if ((moveFilter & MoveFilter.Upgrade) > 0)
                        {
                            foreach (BlueprintPart blueprintPart in playerUnit.Unit.Blueprint.Parts)
                            {
                                if (!playerUnit.Unit.IsInstalled(blueprintPart))
                                {
                                    possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, blueprintPart.Name));
                                }
                            }
                        }
                    }
                }
            }
            foreach (TileWithDistance neighbor in neighbors.Values)
            {
                if (!neighbor.Tile.CanMoveTo())
                    continue;

                if (includedPositions != null)
                {
                    if (!includedPositions.Contains(neighbor.Pos))
                        continue;
                }

                if (neighbor.Unit == null)
                {
                    if ((moveFilter & MoveFilter.Assemble) > 0)
                    {
                        if (Level > 0)
                        {
                            // Can build everything
                            foreach (Blueprint blueprint in Unit.Owner.Game.Blueprints.Items)
                            {
                                possibleMoves.Add(CreateAssembleMove(neighbor.Pos, blueprint.Name));
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
                                    if (!neighbor.Unit.IsInstalled(blueprintPart))
                                    {
                                        possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, blueprintPart.Name));
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
